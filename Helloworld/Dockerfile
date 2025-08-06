FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV MINOMBRE_ES=Angel

USER app 
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Helloworld.csproj", "./"]
RUN dotnet restore "Helloworld.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Helloworld.csproj" -c ${configuration}  -o /app/build

RUN ls

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Helloworld.csproj" -c ${configuration} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Helloworld.dll"]

