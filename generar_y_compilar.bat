@echo off
REM Paso 1: genera secret.c desde secrets.txt (contrasenas ofuscadas con XOR)
REM Paso 2: compila secret.dll con MSVC
REM Luego copia el DLL a los dos ejemplos (C# y Java).

cd /d "%~dp0"

echo ============================================
echo  Paso 1: generar secret.c desde secrets.txt
echo ============================================
dotnet run --project Generador -- secrets.txt secret.c
if %errorlevel% neq 0 (
    echo [ERROR] Fallo el generador.
    pause
    exit /b 1
)

echo.
echo ============================================
echo  Paso 2: compilar secret.dll con MSVC
echo ============================================
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
cl /LD secret.c /Fe:secret.dll
if %errorlevel% neq 0 (
    echo [ERROR] Fallo la compilacion del DLL.
    pause
    exit /b 1
)

echo.
echo  Copiando secret.dll a los ejemplos...
copy /Y secret.dll EjemploCSharp\ >nul
copy /Y secret.dll EjemploJava\ >nul

echo.
echo ============================================
echo  LISTO. secret.dll generado y compilado.
echo ============================================
echo.
pause
