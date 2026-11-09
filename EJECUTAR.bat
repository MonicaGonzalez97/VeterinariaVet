@echo off
echo ==== VetCare: iniciando la API en http://localhost:5080 ====
echo Para detenerla presione Ctrl+C
start "" http://localhost:5080
dotnet run --project VetCare.Api --launch-profile http
pause
