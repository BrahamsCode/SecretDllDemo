@echo off
REM Script para compilar la versión SEGURA del DLL y los consumidores
REM Uso: compilar_secure.bat

echo ============================================
echo Compilando secret_secure.dll con MSVC
echo ============================================

REM 1. Configurar entorno MSVC x64
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"

REM 2. Compilar el DLL seguro
cl /LD secret_secure.c /Fe:secret_secure.dll
if %errorlevel% neq 0 (
    echo ERROR: Fallo al compilar secret_secure.dll
    exit /b 1
)

echo.
echo ============================================
echo DLL compilado exitosamente
echo ============================================
echo.

REM 3. Copiar DLL a los directorios de los consumidores
copy /Y secret_secure.dll ConsumidorCSharp\
copy /Y secret_secure.dll ConsumidorJava\

echo.
echo ============================================
echo Compilando consumidor C#
echo ============================================
cd ConsumidorCSharp
dotnet build -c Release
if %errorlevel% neq 0 (
    echo ERROR: Fallo al compilar consumidor C#
    cd ..
    exit /b 1
)
cd ..

echo.
echo ============================================
echo Compilando consumidor Java
echo ============================================
cd ConsumidorJava
javac -cp jna.jar ConsumidorJavaSecure.java
if %errorlevel% neq 0 (
    echo ERROR: Fallo al compilar consumidor Java
    cd ..
    exit /b 1
)
cd ..

echo.
echo ============================================
echo COMPILACION COMPLETADA EXITOSAMENTE
echo ============================================
echo.
echo Para ejecutar:
echo   C#   : cd ConsumidorCSharp ^&^& dotnet run -c Release
echo   Java : cd ConsumidorJava ^&^& java -cp ".;jna.jar" -Djna.library.path=. ConsumidorJavaSecure
echo.
