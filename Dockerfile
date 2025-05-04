# Usa la imagen oficial de .NET 8.0 para runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Usa la imagen oficial de .NET 8.0 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY FRONT/FRONT.csproj FRONT/
RUN dotnet restore FRONT/FRONT.csproj
COPY FRONT/. ./FRONT
WORKDIR /src/FRONT
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FRONT.dll"]
