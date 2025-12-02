# ============================================
# Dockerfile para ChatbotTCS.AdminAPI
# ============================================
# API .NET Core 9.0 con MongoDB y Ollama
# ============================================

# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
COPY ChatbotTCS.AdminAPI/ChatbotTCS.AdminAPI.csproj ChatbotTCS.AdminAPI/
RUN dotnet restore "ChatbotTCS.AdminAPI/ChatbotTCS.AdminAPI.csproj"

# Copiar el resto de archivos y compilar
COPY ChatbotTCS.AdminAPI/ ChatbotTCS.AdminAPI/
WORKDIR "/src/ChatbotTCS.AdminAPI"
RUN dotnet build "ChatbotTCS.AdminAPI.csproj" -c Release -o /app/build

# Etapa 2: Publish
FROM build AS publish
RUN dotnet publish "ChatbotTCS.AdminAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Instalar herramientas útiles para debugging
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Exponer puertos
EXPOSE 8080
EXPOSE 8081

# Variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck para Docker
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "ChatbotTCS.AdminAPI.dll"]
