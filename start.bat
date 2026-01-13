@echo off
REM start.bat - Запуск всех сервисов NovelVision

echo ========================================
echo    NovelVision - Запуск всех сервисов
echo ========================================
echo.

REM Проверяем наличие PowerShell скрипта
if not exist "start-all.ps1" (
    echo Ошибка: Файл start-all.ps1 не найден!
    echo Запустите скрипт из корневой директории NovelVision
    pause
    exit /b 1
)

REM Запускаем PowerShell скрипт
echo Запуск PowerShell скрипта...
powershell.exe -ExecutionPolicy Bypass -File ".\start-all.ps1"

pause