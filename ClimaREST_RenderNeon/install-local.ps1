$ErrorActionPreference = "Stop"
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "  Instalador local - Clima REST" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan

try {
    $sdkVersion = (& dotnet --version 2>$null).Trim()
} catch {
    throw "No se encontró el SDK de .NET. Instala .NET 10 SDK desde https://dotnet.microsoft.com/download/dotnet/10.0 y vuelve a ejecutar este instalador."
}

$majorVersion = [int]($sdkVersion.Split('.')[0])
if ($majorVersion -lt 10) {
    throw "Se requiere .NET SDK 10 o superior. Versión detectada: $sdkVersion. Actualiza el SDK desde https://dotnet.microsoft.com/download/dotnet/10.0."
}

New-Item -ItemType Directory -Force -Path ".\App_Data" | Out-Null
Write-Host "SDK detectado: $sdkVersion" -ForegroundColor Green
Write-Host "Restaurando dependencias NuGet..." -ForegroundColor Yellow
dotnet restore .\RestFulWebService.csproj
if ($LASTEXITCODE -ne 0) { throw "Falló la restauración de dependencias." }
Write-Host "Instalación completada. Ejecuta .\run-local.bat para iniciar la aplicación." -ForegroundColor Green
