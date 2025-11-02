FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["BiologistApi.csproj","./"]
RUN dotnet restore "BiologistApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BiologistApi.csproj" -c ${configuration} -o /app/build



FROM build AS publish
ARG configuration=Release
RUN dotnet publish "BiologistApi.csproj" -c ${configuration} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8088
ENV ASPNETCORE_URLS=http://+:8088
ENTRYPOINT [ "dotnet", "BiologistApi.dll" ]
