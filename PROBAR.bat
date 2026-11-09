@echo off
echo ==== VetCare: compilar y ejecutar las pruebas unitarias ====
dotnet --version
dotnet build VetCare.sln
if errorlevel 1 goto error
dotnet test VetCare.sln
echo.
echo Si arriba dice "Superado: 45" y "Con error: 0", todo esta bien.
pause
exit /b 0
:error
echo.
echo *** La compilacion fallo. Copie el mensaje de error y envielo. ***
pause
exit /b 1
