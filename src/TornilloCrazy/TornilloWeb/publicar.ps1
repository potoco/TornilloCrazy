# para ejecutar script 
# y que no te detenga la seguridad -> pwsh -ExecutionPolicy Bypass -File .\publicar.ps1
# Forzar que el script se detenga ante cualquier error inesperado
$ErrorActionPreference = "Stop"

$projectFile = ".\TornilloWeb.csproj"
$publishDir  = ".\bin\Release\net10.0\publish\"
# Definimos dónde va a quedar el archivo ZIP (un nivel arriba de /publish)
$fechaActual = Get-Date -Format "yyyy-MM-dd_HH-mm"
$zipOutputFile = ".\bin\Release\net10.0\TornilloWeb_$fechaActual.zip"

Write-Host "🔍 Verificando carpeta de destino..." -ForegroundColor Cyan

if (Test-Path $publishDir) {
    try {
        Write-Host "🗑️ Intentando vaciar el contenido de: $publishDir" -ForegroundColor Yellow
        Remove-Item -Path "$publishDir\*" -Recurse -Force -ErrorAction Stop
        Write-Host "✨ Carpeta de destino limpia y lista." -ForegroundColor Green
    }
    catch {
        Write-Host "`n[ERROR] 🛑 No se pudo vaciar la carpeta de publicación." -ForegroundColor Red
        Write-Host "[REASÓN] Es muy probable que algún archivo esté tomado por otro proceso." -ForegroundColor Yellow
        Write-Host "[DETALLE] $($_.Exception.Message)" -ForegroundColor LightMagenta
        Write-Host "`nPor favor, liberá el archivo manualmente y volvé a ejecutar el script.`n" -ForegroundColor Cyan
        Exit
    }
} else {
    Write-Host "📂 La carpeta de destino no existe todavía, se creará al publicar." -ForegroundColor Gray
}

# --- Ejecución de comandos nativos de .NET ---

Write-Host "`n🧹 Ejecutando dotnet clean..." -ForegroundColor Cyan
dotnet clean $projectFile --configuration Release

Write-Host "🚀 Publicando proyecto..." -ForegroundColor Green
dotnet publish $projectFile /p:PublishProfile=FolderProfile /p:Configuration=Release

Write-Host "✅ ¡Publicación completada con éxito!" -ForegroundColor Green

# --- Automatización del empaquetado ZIP ---

try {
    # Si ya existía un ZIP de una vuelta anterior, lo borramos para que no tire error
    if (Test-Path $zipOutputFile) {
        Write-Host "`n📦 Eliminando ZIP anterior..." -ForegroundColor Gray
        Remove-Item -Path $zipOutputFile -Force
    }

    Write-Host "`n🗜️ Comprimiendo archivos de publicación en: $zipOutputFile..." -ForegroundColor Cyan
    
    # Comprime el contenido de la carpeta publish (el \* asegura que no incluya la carpeta 'publish' en sí dentro del zip)
    Compress-Archive -Path "$publishDir\*" -DestinationPath $zipOutputFile -Force
    
    Write-Host "🎉 ¡Archivo ZIP creado exitosamente listo para subir!" -ForegroundColor Green
}
catch {
    Write-Host "`n[ADVERTENCIA] ⚠️ La publicación fue exitosa, pero falló la creación del ZIP." -ForegroundColor Yellow
    Write-Host "[DETALLE] $($_.Exception.Message)" -ForegroundColor LightMagenta
}