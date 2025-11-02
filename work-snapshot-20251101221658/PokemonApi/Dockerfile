FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["PokemonApi.csproj", "./"]
RUN dotnet restore "PokemonApi.csproj"

COPY . .
RUN dotnet build "PokemonApi.csproj" -c ${configuration} -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "PokemonApi.csproj" -c ${configuration} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PokemonApi.dll"]
