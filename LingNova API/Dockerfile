# Fase base: Runtime de ASP.NET Core 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Configuramos el puerto 10000 que es el estándar de Render
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Fase de compilación: SDK de .NET 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copiamos los archivos de proyecto (.csproj) manteniendo la estructura de carpetas
# Esto ayuda a que el comando 'restore' sea más rápido si no cambian las dependencias
COPY ["LingNova API/LingNova API.csproj", "LingNova API/"]
COPY ["LingNova.Core/LingNova.Core.csproj", "LingNova.Core/"]
COPY ["LingNova.Infreaestructure/LingNova.Infreaestructure.csproj", "LingNova.Infreaestructure/"]

# 2. Restauramos las dependencias
RUN dotnet restore "LingNova API/LingNova API.csproj"

# 3. Copiamos el resto de los archivos de la solución
COPY . .

# 4. Entramos a la carpeta del proyecto principal y compilamos
WORKDIR "/src/LingNova API"
RUN dotnet build "LingNova API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de publicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LingNova API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final: Imagen limpia para ejecución
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# IMPORTANTE: Asegúrate de que el nombre del DLL sea exacto (con el espacio)
ENTRYPOINT ["dotnet", "LingNova API.dll"]