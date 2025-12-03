param(
  [string]$BaseUrl = "http://localhost:8088",
  [string]$TokenFile = "../tmp/access_token",
  [string]$HydraTokenUrl = "http://localhost:4444/oauth2/token"
)

# Normalize token path relative to repository root when running from different cwd
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = Resolve-Path (Join-Path $ScriptDir '..\..') -ErrorAction SilentlyContinue
if ($RepoRoot) {
    $defaultToken = Join-Path $RepoRoot 'tmp\access_token'
    if (-not (Test-Path $TokenFile)) { $TokenFile = $defaultToken }
}
function Get-Token {
  param([string]$TokenFile, [string]$HydraTokenUrl)
  if (Test-Path $TokenFile) {
    $t = (Get-Content $TokenFile -ErrorAction SilentlyContinue) -join ""
    if ($t) { return $t }
  }
  Write-Host "No token file found, requesting one from Hydra..." -ForegroundColor Yellow
  $body = "grant_type=client_credentials&client_id=musicshop&client_secret=secret&scope=read%20write"
  try {
    $resp = Invoke-RestMethod -Method Post -Uri $HydraTokenUrl -ContentType "application/x-www-form-urlencoded" -Body $body -ErrorAction Stop
    $token = $resp.access_token
    if (-not $token) { throw "No access_token in response" }
  # ensure directory exists before writing token
  $tokenDir = Split-Path -Parent $TokenFile
  if (-not (Test-Path $tokenDir)) { New-Item -ItemType Directory -Path $tokenDir -Force | Out-Null }
  $token | Out-File $TokenFile -Encoding ascii
    return $token
  } catch {
    throw "Unable to obtain token: $($_.Exception.Message)"
  }
}

# Helper: invoke request, capture status, headers and parsed JSON if present
function Invoke-Api {
  param(
    [string]$Method,
    [string]$Url,
    $Body = $null,
    [hashtable]$Headers = @{},
    [string]$ContentType = $null
  )

  $result = [PSCustomObject]@{
    StatusCode = $null
    Headers = @{}
    Content = $null
    Json = $null
    Raw = $null
    Request = [PSCustomObject]@{ Method=$Method; Url=$Url; Body=$Body; Headers=$Headers; ContentType=$ContentType }
  }

  try {
    $invokeParams = @{ Uri = $Url; Method = $Method; ErrorAction = 'Stop'; UseBasicParsing = $true }
    if ($Body -ne $null) { $invokeParams['Body'] = $Body }
    if ($ContentType) { $invokeParams['ContentType'] = $ContentType }
    if ($Headers -and $Headers.Count -gt 0) { $invokeParams['Headers'] = $Headers }

    $resp = Invoke-WebRequest @invokeParams
  # Some PowerShell versions don't expose StatusCode on success
  try { $result.StatusCode = $resp.StatusCode.Value__ } catch { $result.StatusCode = $null }
  if (-not $result.StatusCode) { try { $result.StatusCode = [int]$resp.StatusCode } catch { $result.StatusCode = 200 } }
    $result.Headers = $resp.Headers
    $result.Raw = $resp.Content
    if ($resp.Content -and $resp.Headers['Content-Type'] -match 'application/json') {
      try { $result.Json = $resp.Content | ConvertFrom-Json -ErrorAction Stop } catch { $result.Json = $null }
    }
    return $result
  } catch [System.Net.WebException] {
    $resp = $_.Exception.Response
    if ($null -ne $resp) {
      $status = $resp.StatusCode.value__
      $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
      $body = $reader.ReadToEnd(); $reader.Close()
      $result.StatusCode = $status
      $result.Raw = $body
      try { $result.Json = $body | ConvertFrom-Json -ErrorAction Stop } catch { $result.Json = $null }
      return $result
    }
    # Network failure
    $result.StatusCode = -1
    $result.Raw = $_.Exception.Message
    return $result
  } catch {
    # Unknown failure
    $result.StatusCode = -2
    $result.Raw = $_.Exception.Message
    return $result
  }
}

function Assert-Status {
  param([int]$Got, [int[]]$Expected, [string]$Why)
  if ($Expected -notcontains $Got) {
    Write-Host ("Status {0}, expected: {1} -> {2}" -f $Got, ($Expected -join '|'), $Why) -ForegroundColor Red
    throw "FAILED"
  }
  Write-Host "$Why -> OK ($Got)" -ForegroundColor Blue
}

# Helpers to parse Location and ID
function Get-IdFromLocation {
  param([string]$Location)
  if (-not $Location) { return $null }
  # assume last path segment is the id
  $uri = [Uri]$Location
  return ($uri.Segments | Select-Object -Last 1).TrimEnd('/')
}

# Pretty printers :D (not made by me...)
function Show-Request {
  param($req)
  Write-Host ("REQUEST: {0} {1}" -f $req.Method, $req.Url) -ForegroundColor Gray
  if ($req.ContentType) { Write-Host ("  Content-Type: {0}" -f $req.ContentType) -ForegroundColor DarkGray }
  if ($req.Headers -and $req.Headers.Count -gt 0) { Write-Host ("  Headers: {0}" -f ($req.Headers.Keys -join ', ')) -ForegroundColor Gray }
  if ($req.Body) {
    Write-Host "  Body:" -ForegroundColor DarkGray
    Write-Host $req.Body -ForegroundColor Yellow
  }
}
function Show-Response {
  param($res, $expectedCodes = @())
  $code = $res.StatusCode
  $isExpected = ($expectedCodes.Count -eq 0) -or ($code -in $expectedCodes)
  $color = if ($isExpected) { 'Blue' } else { 'Red' }
  Write-Host ("RESPONSE: Status {0}" -f $code) -ForegroundColor $color
  if ($res.Json) {
    Write-Host "  JSON:" -ForegroundColor DarkGray
    ($res.Json | ConvertTo-Json -Depth 8) | Write-Host
  } elseif ($res.Raw) {
    Write-Host "  Body:" -ForegroundColor DarkGray
    Write-Host $res.Raw
  }
}
$ErrorActionPreference = "Stop"

$token = Get-Token -TokenFile $TokenFile -HydraTokenUrl $HydraTokenUrl

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Token length: $($token.Length)" -ForegroundColor DarkGray
Write-Host "OAuth Scopes: read + write" -ForegroundColor Cyan
Write-Host "  - 'read' scope: Allows GET operations" -ForegroundColor Green
Write-Host "  - 'write' scope: Allows POST, PUT, PATCH, DELETE operations" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "--- 1) POST invalid -> expect 400 Bad Request" -ForegroundColor Magenta
$invalidBody = @{ nombre = ""; modelo = "Player"; anio = 1800; precio = -10; marca = "Fender"; categoria = "" } | ConvertTo-Json
$r = Invoke-Api -Method Post -Url "$BaseUrl/instruments" -Body $invalidBody -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
Show-Request $r.Request
Show-Response $r -expectedCodes @(400)
Assert-Status -Got $r.StatusCode -Expected @(400) -Why "POST invalid should return 400"

Write-Host "--- 2) POST create -> expect 201 Created (or 424/502 if SOAP fails)" -ForegroundColor Magenta
$create = @{ nombre = "Stratocaster $(Get-Random -Maximum 99999)"; modelo = "Player"; anio = 2020; precio = 1500; marca = "Fender"; categoria = "guitarra" } | ConvertTo-Json
$r = Invoke-Api -Method Post -Url "$BaseUrl/instruments" -Body $create -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
Show-Request $r.Request
Show-Response $r -expectedCodes @(200,201,424,502)
$looksCreated = $false
if (-not $r.Json -and $r.Raw) { try { $r.Json = $r.Raw | ConvertFrom-Json -ErrorAction Stop } catch {} }
if ($r.Json -and $r.Json.id) { $looksCreated = $true }
if (($r.StatusCode -in @(201,200)) -or ($null -eq $r.StatusCode -and $looksCreated)) {
  if ($r.StatusCode) { Assert-Status -Got ($r.StatusCode -as [int]) -Expected @(200,201) -Why "POST create" }
  else { Write-Host "POST create -> Created (status not reported by shell)" -ForegroundColor Green }
  $location = $r.Headers['Location']
  if (-not $location) { Write-Host "Warning: missing Location header in 201" -ForegroundColor Yellow }
  $newId = Get-IdFromLocation $location
  if (-not $newId -and $r.Json -and $r.Json.id) { $newId = [string]$r.Json.id }
  if ($newId) { Write-Host "New resource ID: $newId" -ForegroundColor Green }
} elseif ($r.StatusCode -in @(424,502,409)) {
  Assert-Status -Got $r.StatusCode -Expected @(424,502,409) -Why "POST create mapped SOAP/conflict"
} elseif ($r.StatusCode -eq 400) {
  Write-Host "POST create returned 400 (validation)." -ForegroundColor Yellow
  if ($r.Json) { ($r.Json | ConvertTo-Json -Depth 5) | Write-Host }
  else { Write-Host $r.Raw }
} else {
  Write-Host "POST create -> Unexpected status: $($r.StatusCode)" -ForegroundColor Red
  Show-Response $r
}

Write-Host "--- 3) POST duplicate -> expect 409 Conflict" -ForegroundColor Magenta
$rdup = Invoke-Api -Method Post -Url "$BaseUrl/instruments" -Body $create -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
Show-Request $rdup.Request
Show-Response $rdup -expectedCodes @(409)
if ($rdup.StatusCode -eq 409) { Write-Host "Duplicate -> 409 OK" -ForegroundColor Green }
elseif ($rdup.StatusCode -eq 201 -or $rdup.StatusCode -eq 200) { Write-Host "Duplicate returned success ($($rdup.StatusCode)) (not ideal)" -ForegroundColor Yellow }
else { Write-Host "Duplicate returned: $($rdup.StatusCode)" -ForegroundColor Red }

Write-Host "--- 3b) POST duplicate by nombre only -> expect 409 Conflict" -ForegroundColor Magenta
if ($r.Json -and $r.Json.nombre) {
  $dupNombreBody = @{ nombre = $r.Json.nombre; modelo = "Another"; anio = 2021; precio = 999; marca = "Other"; categoria = "guitarra" } | ConvertTo-Json
  $rdup2 = Invoke-Api -Method Post -Url "$BaseUrl/instruments" -Body $dupNombreBody -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
  Show-Request $rdup2.Request
  Show-Response $rdup2 -expectedCodes @(409)
  if ($rdup2.StatusCode -eq 409) { Write-Host "Duplicate by nombre -> 409 OK" -ForegroundColor Green }
  else { Write-Host "Duplicate by nombre returned: $($rdup2.StatusCode) (expected 409)" -ForegroundColor Red }
} else {
  Write-Host "Skipping POST duplicate-by-nombre (no nombre from created item)" -ForegroundColor Yellow
}

Write-Host "--- 4) GET list -> expect 200 OK with pagination" -ForegroundColor Magenta
$listUrl = "$BaseUrl/instruments?page=1&pageSize=5&sort=nombre"
$rl = Invoke-Api -Method Get -Url $listUrl -Headers @{ Authorization = "Bearer $token" }
Show-Request $rl.Request
Show-Response $rl -expectedCodes @(200)
if ($rl.StatusCode -eq 200) {
  Write-Host "GET list -> OK (200)" -ForegroundColor Green
  if ($rl.Json -and $rl.Json._embedded) { Write-Host ("List contains {0}/{1} items" -f $rl.Json.count, $rl.Json.total) }
  elseif ($rl.Json -and ($rl.Json -is [System.Array])) { Write-Host ("List contains {0} items" -f $rl.Json.Length) }
  else { Write-Host "List OK (JSON shape not recognized)" -ForegroundColor Yellow }
  # Cache headers visibility
  if ($rl.Headers -and $rl.Headers['X-Cache-Status']) {
    Write-Host ("  Cache Status: {0}" -f $rl.Headers['X-Cache-Status']) -ForegroundColor Cyan
  }
} elseif ($rl.StatusCode -eq 400) {
  Write-Host "GET list returned 400 (validation)." -ForegroundColor Yellow
  if ($rl.Json) { $rl.Json | ConvertTo-Json -Depth 5 | Write-Host } else { Write-Host $rl.Raw }
} else {
  Write-Host "GET list -> Unexpected status: $($rl.StatusCode)" -ForegroundColor Red
  Show-Response $rl
}

Write-Host "--- 5) GET by existing id -> expect 200 OK (test cache)" -ForegroundColor Magenta
if (-not $newId) {
    # fallback: pick first id from listing if available
    if ($rl.Json -and $rl.Json._embedded -and $rl.Json._embedded.Count -gt 0) { $newId = $rl.Json._embedded[0].id }
    elseif ($rl.Json -and $rl.Json.Count -gt 0) { $newId = $rl.Json[0].id }
    else { Write-Host "No id available to test GET by id" -ForegroundColor Yellow }
}
if ($newId) {
    $ri = Invoke-Api -Method Get -Url "$BaseUrl/instruments/$newId" -Headers @{ Authorization = "Bearer $token" }
    Show-Request $ri.Request
    Show-Response $ri -expectedCodes @(200)
    Assert-Status -Got $ri.StatusCode -Expected @(200) -Why "GET by id"
    if ($ri.Headers -and $ri.Headers['X-Cache-Status']) {
      Write-Host ("  Cache Status: {0}" -f $ri.Headers['X-Cache-Status']) -ForegroundColor Cyan
    }
    # Second request to verify cache hit
    Write-Host "  Re-fetching same resource to test cache..." -ForegroundColor DarkGray
    $ri2 = Invoke-Api -Method Get -Url "$BaseUrl/instruments/$newId" -Headers @{ Authorization = "Bearer $token" }
    if ($ri2.Headers -and $ri2.Headers['X-Cache-Status']) {
      Write-Host ("  Cache Status on 2nd request: {0}" -f $ri2.Headers['X-Cache-Status']) -ForegroundColor Cyan
    }
} else { Write-Host "Skipping GET by id (no id available)" -ForegroundColor Yellow }

Write-Host "--- 6) GET by missing id -> expect 404 Not Found" -ForegroundColor Magenta
$missingId = '99999'
$rmiss = Invoke-Api -Method Get -Url "$BaseUrl/instruments/$missingId" -Headers @{ Authorization = "Bearer $token" }
Show-Request $rmiss.Request
Show-Response $rmiss -expectedCodes @(404)
Assert-Status -Got $rmiss.StatusCode -Expected @(404) -Why "GET by missing id"

Write-Host "--- 7) Create second instrument for duplicate tests" -ForegroundColor Magenta
$create2 = @{ nombre = "Les Paul $(Get-Random -Maximum 99999)"; modelo = "Standard"; anio = 2019; precio = 2000; marca = "Gibson"; categoria = "guitarra" } | ConvertTo-Json
$r2 = Invoke-Api -Method Post -Url "$BaseUrl/instruments" -Body $create2 -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
Show-Request $r2.Request
Show-Response $r2 -expectedCodes @(200,201,424,502)
$secondId = $null
if ($r2.StatusCode -in @(200,201) -and $r2.Json -and $r2.Json.id) {
  $secondId = [string]$r2.Json.id
  Write-Host "Second resource ID: $secondId" -ForegroundColor Green
  # Store the nombre for duplicate tests
  $secondNombre = $r2.Json.nombre
} else {
  Write-Host "Could not create second instrument, skipping duplicate tests" -ForegroundColor Yellow
}

Write-Host "--- 8) PUT replace -> expect 200 or 204" -ForegroundColor Magenta
if ($newId) {
    $replace = @{ nombre="Replaced"; modelo="Pro"; anio=2021; precio=999; marca="Yamaha"; categoria="guitarra" } | ConvertTo-Json
    $rput = Invoke-Api -Method Put -Url "$BaseUrl/instruments/$newId" -Body $replace -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
    Show-Request $rput.Request
    Show-Response $rput -expectedCodes @(200,204)
    Assert-Status -Got $rput.StatusCode -Expected @(200,204) -Why "PUT replace"
} else { Write-Host "Skipping PUT (no id available)" -ForegroundColor Yellow }

Write-Host "--- 9) PUT with duplicate nombre -> expect 409 Conflict" -ForegroundColor Magenta
if ($newId -and $secondId -and $secondNombre) {
  # Try to update first instrument with the nombre from second instrument
  $replaceDup = @{ nombre=$secondNombre; modelo="Pro"; anio=2021; precio=999; marca="Yamaha"; categoria="guitarra" } | ConvertTo-Json
  $rputdup = Invoke-Api -Method Put -Url "$BaseUrl/instruments/$newId" -Body $replaceDup -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
  Show-Request $rputdup.Request
  Show-Response $rputdup -expectedCodes @(409)
  if ($rputdup.StatusCode -eq 409) { 
    Write-Host "PUT duplicate nombre -> 409 OK" -ForegroundColor Green 
  } else { 
    Write-Host "PUT duplicate nombre returned: $($rputdup.StatusCode) (expected 409)" -ForegroundColor Red 
  }
} else { Write-Host "Skipping PUT duplicate test (no second id available)" -ForegroundColor Yellow }

Write-Host "--- 10) PATCH partial update -> expect 200 or 204" -ForegroundColor Magenta
if ($newId) {
    $patch = @{ precio = 1234 } | ConvertTo-Json
    $rpatch = Invoke-Api -Method Patch -Url "$BaseUrl/instruments/$newId" -Body $patch -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
    Show-Request $rpatch.Request
    Show-Response $rpatch -expectedCodes @(200,204)
    Assert-Status -Got $rpatch.StatusCode -Expected @(200,204) -Why "PATCH partial"
} else { Write-Host "Skipping PATCH (no id available)" -ForegroundColor Yellow }

Write-Host "--- 11) PATCH with duplicate nombre -> expect 409 Conflict" -ForegroundColor Magenta
if ($newId -and $secondId -and $secondNombre) {
  # Try to update first instrument's nombre to match second instrument
  $patchDup = @{ nombre=$secondNombre } | ConvertTo-Json
  $rpatchdup = Invoke-Api -Method Patch -Url "$BaseUrl/instruments/$newId" -Body $patchDup -Headers @{ Authorization = "Bearer $token" } -ContentType 'application/json'
  Show-Request $rpatchdup.Request
  Show-Response $rpatchdup -expectedCodes @(409)
  if ($rpatchdup.StatusCode -eq 409) { 
    Write-Host "PATCH duplicate nombre -> 409 OK" -ForegroundColor Green 
  } else { 
    Write-Host "PATCH duplicate nombre returned: $($rpatchdup.StatusCode) (expected 409)" -ForegroundColor Red 
  }
} else { Write-Host "Skipping PATCH duplicate test (no second id available)" -ForegroundColor Yellow }

Write-Host "--- 12) DELETE first instrument -> expect 204 No Content" -ForegroundColor Magenta
if ($newId) {
    $rdel = Invoke-Api -Method Delete -Url "$BaseUrl/instruments/$newId" -Headers @{ Authorization = "Bearer $token" }
    Show-Request $rdel.Request
    Show-Response $rdel -expectedCodes @(204,200)
    Assert-Status -Got $rdel.StatusCode -Expected @(204,200) -Why "DELETE should be 204 or 200 depending on impl"
} else { Write-Host "Skipping DELETE (no id available)" -ForegroundColor Yellow }

Write-Host "--- 13) DELETE second instrument -> expect 204 No Content" -ForegroundColor Magenta
if ($secondId) {
  $rdel3 = Invoke-Api -Method Delete -Url "$BaseUrl/instruments/$secondId" -Headers @{ Authorization = "Bearer $token" }
  Show-Request $rdel3.Request
  Show-Response $rdel3 -expectedCodes @(204,200)
  Assert-Status -Got $rdel3.StatusCode -Expected @(204,200) -Why "DELETE second instrument"
} else { Write-Host "Skipping DELETE second (no id available)" -ForegroundColor Yellow }

Write-Host "--- 14) DELETE missing -> expect 404 (or 204/200 for idempotent)" -ForegroundColor Magenta
$rdel2 = Invoke-Api -Method Delete -Url "$BaseUrl/instruments/$missingId" -Headers @{ Authorization = "Bearer $token" }
# Accept 400
Show-Request $rdel2.Request
Show-Response $rdel2 -expectedCodes @(404,204,200,400)
Assert-Status -Got $rdel2.StatusCode -Expected @(404,204,200,400) -Why "DELETE missing (some APIs return 204 for idempotent deletes)"

Write-Host "--- 15) GET without token -> expect 401 Unauthorized" -ForegroundColor Magenta
$rnot = Invoke-Api -Method Get -Url "$BaseUrl/instruments" -Headers @{}
Show-Request $rnot.Request
Show-Response $rnot -expectedCodes @(401,403)
Assert-Status -Got $rnot.StatusCode -Expected @(401,403) -Why "GET without token should be 401 or 403"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Tests finished :D pls 100/100 in the assignment." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
