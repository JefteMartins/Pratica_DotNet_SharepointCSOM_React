@echo off
TITLE SharePoint Lab Runner
echo ==========================================
echo   Iniciando Laboratorio SharePoint CSOM
echo ==========================================

echo.
echo [1/2] Iniciando Backend API (ASP.NET Core)...
start "Backend API" cmd /k "cd SharePointCsomApi && dotnet run"

echo.
echo [2/2] Iniciando Frontend UI (Vite + React)...
start "Frontend UI" cmd /k "cd sharepoint-lab-ui && npm run dev"

echo.
echo Abrindo navegador...
:: Espera uns segundos pro server subir antes de abrir o browser
timeout /t 5 /nobreak > nul
start http://localhost:5118/swagger
start http://localhost:5173

echo.
echo ==========================================
echo   Tudo pronto! As janelas estao abrindo.
echo   Mantenha-as abertas enquanto trabalhar.
echo ==========================================
pause
