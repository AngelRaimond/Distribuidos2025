
const apiBase = "/api/plants"; 

async function fetchPlants() {
  try {
    const res = await fetch(apiBase);
    if (!res.ok) { console.error('Error fetching', res.status); return; }
    const data = await res.json();
    const tbody = document.querySelector("#plantsTable tbody");
    tbody.innerHTML = "";
    data.forEach(p => {
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${p.name}</td>
        <td>${p.scientificName ?? ""}</td>
        <td>${p.family ?? ""}</td>
        <td>
          <button data-id="${p.id}" class="delete">Eliminar</button>
        </td>`;
      tbody.appendChild(tr);
    });
  } catch (err) {
    console.error(err);
  }
}

document.getElementById("createForm").addEventListener("submit", async e => {
  e.preventDefault();
  const body = {
    name: document.getElementById("name").value,
    scientificName: document.getElementById("scientificName").value,
    family: document.getElementById("family").value,
  };
  const res = await fetch(apiBase, {
    method: "POST",
    headers: {"Content-Type":"application/json"},
    body: JSON.stringify(body)
  });
  if (res.ok) {
    document.getElementById("createForm").reset();
    await fetchPlants();
  } else {
    alert("Error creando planta");
  }
});

document.addEventListener("click", async (e) => {
  if (e.target.matches("button.delete")) {
    const id = e.target.dataset.id;
    const res = await fetch(`${apiBase}/${id}`, { method: "DELETE" });
    if (res.ok) await fetchPlants();
    else alert("No se pudo eliminar");
  }
});


fetchPlants();
