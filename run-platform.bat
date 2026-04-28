@echo off
TITLE Excellence Hotel & Lab Platform
SETLOCAL

echo ====================================================
echo   EXCELLENCE HOTEL ^& LAB PLATFORM - STARTUP
echo ====================================================

:: 1. Build and Run Backend
echo [1/2] Inciando Backend (HotelAPI) em nova janela no perfil HTTPS...
start "HotelAPI - Backend" cmd /k "cd Hotel/HotelAPI && dotnet run --launch-profile https"

:: 2. Build and Run Frontend
echo [2/2] Iniciando Frontend (HotelUI) em nova janela...
start "HotelUI - Frontend" cmd /k "cd Hotel/HotelUI && npm install && npm run dev"

echo ====================================================
echo   SISTEMA INICIALIZADO
echo   - API: https://localhost:7233
echo   - App: http://localhost:5173
echo ====================================================
echo.
echo Pressione qualquer tecla para encerrar este script (as janelas do sistema permanecerão abertas).
pause > nul
