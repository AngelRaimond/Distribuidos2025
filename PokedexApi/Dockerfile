FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
USER app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["PokedexApi.csproj", "./"]
RUN dotnet restore "PokedexApi.csproj"
COPY . .
RUN ls -la
WORKDIR "/src/."
RUN dotnet build "PokedexApi.csproj" -c ${configuration} -o /app/build

FROM build AS publish
ARG configuration=Realese
RUN dotnet publish "PokedexApi.csproj" -c ${configuration} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "PokedexApi.dll" ]
