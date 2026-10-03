# syntax=docker/dockerfile:1

# Etapa 1: restauracion de paquetes (cache separado del codigo fuente)
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copiar solo los archivos de proyecto para aprovechar la cache de restore
COPY InventarioCelulares.sln ./
COPY backend/Inventario.Celulares.Api/Inventario.Celulares.Api.csproj backend/Inventario.Celulares.Api/
COPY backend/Inventario.Celulares.Core/Inventario.Celulares.Core.csproj backend/Inventario.Celulares.Core/
COPY backend/Inventario.Celulares.Infrastructure/Inventario.Celulares.Infrastructure.csproj backend/Inventario.Celulares.Infrastructure/
RUN dotnet restore backend/Inventario.Celulares.Api/Inventario.Celulares.Api.csproj

# Copiar el codigo y publicar en Release
COPY backend/ backend/
RUN dotnet publish backend/Inventario.Celulares.Api/Inventario.Celulares.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Etapa 2: imagen final minima sin herramientas de compilacion
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Usuario dedicado: la API no se ejecuta como root
RUN addgroup -S apiuser && adduser -S apiuser -G apiuser
USER apiuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URL=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -qO- http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "Inventario.Celulares.Api.dll"]