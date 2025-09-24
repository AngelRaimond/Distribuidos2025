FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src

COPY ["GardenApi.csproj", "./"]
RUN dotnet restore "GardenApi.csproj"

COPY . .
RUN dotnet build "GardenApi.csproj" -c ${configuration} -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "GardenApi.csproj" -c ${configuration} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GardenApi.dll"]