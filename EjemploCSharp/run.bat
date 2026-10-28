@echo off
REM Ejecuta el ejemplo C# de un solo clic. Requisitos: .NET SDK en el PATH.
cd /d "%~dp0"
echo Compilando y ejecutando ejemplo C# ...
echo.
dotnet run -c Release
echo.
pause
