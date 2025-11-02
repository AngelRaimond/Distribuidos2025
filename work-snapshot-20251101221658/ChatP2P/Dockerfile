FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV MINOMBRE_ES=Angel

USER app 
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["ChatP2P.csproj", "./"]
RUN dotnet restore "ChatP2P.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ChatP2P.csproj" -c ${configuration}  -o /app/build

RUN ls

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ChatP2P.csproj" -c ${configuration} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatP2P.dll"]
