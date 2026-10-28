@echo off
REM Ejecuta el ejemplo Java de un solo clic (compila y corre).
REM Requisitos: Java (JDK) en el PATH. secret.dll y jna.jar en esta carpeta.
cd /d "%~dp0"

echo Compilando EjemploSecrets.java ...
javac -cp jna.jar EjemploSecrets.java
if %errorlevel% neq 0 ( echo [ERROR] Fallo la compilacion. & pause & exit /b 1 )

echo.
java -cp ".;jna.jar" EjemploSecrets
echo.
pause
