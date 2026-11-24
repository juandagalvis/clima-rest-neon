@echo off
setlocal
cd /d "%~dp0"
echo Iniciando Clima REST...
powershell -NoProfile -ExecutionPolicy Bypass -File ".\install-local.ps1"
if errorlevel 1 (
  pause
  exit /b 1
)
dotnet run --project .\RestFulWebService.csproj --urls http://localhost:5160
pause
