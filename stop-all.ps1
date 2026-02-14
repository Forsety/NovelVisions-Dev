# stop-all.ps1
# Скрипт для остановки всех сервисов NovelVision

Write-Host "========================================" -ForegroundColor Red
Write-Host "   NovelVision - Остановка сервисов    " -ForegroundColor Red
Write-Host "========================================" -ForegroundColor Red
Write-Host ""

# Функция для остановки процессов по порту
function Stop-ProcessByPort {
    param([int]$Port)
    
    Write-Host "Остановка процесса на порту $Port..." -ForegroundColor Yellow
    
    try {
        $processId = (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue).OwningProcess | Select-Object -Unique
        
        if ($processId) {
            foreach ($pid in $processId) {
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "  Остановка процесса: $($process.ProcessName) (PID: $pid)" -ForegroundColor Cyan
                    Stop-Process -Id $pid -Force
                    Write-Host "  ✓ Процесс остановлен" -ForegroundColor Green
                }
            }
        } else {
            Write-Host "  Нет активных процессов на порту $Port" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  Ошибка при остановке: $_" -ForegroundColor Red
    }
}

# Остановка всех сервисов
Write-Host "Остановка Frontend (порт 3000)..." -ForegroundColor Magenta
Stop-ProcessByPort -Port 3000

Write-Host "`nОстановка Catalog.API (порт 5001)..." -ForegroundColor Magenta
Stop-ProcessByPort -Port 5001

Write-Host "`nОстановка API Gateway (порт 5000)..." -ForegroundColor Magenta
Stop-ProcessByPort -Port 5000

# Дополнительно останавливаем процессы dotnet и node
Write-Host "`nОстановка оставшихся процессов..." -ForegroundColor Yellow

# Останавливаем все процессы dotnet
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    foreach ($process in $dotnetProcesses) {
        Write-Host "  Остановка dotnet процесса (PID: $($process.Id))" -ForegroundColor Cyan
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
}

# Останавливаем процессы node
$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    foreach ($process in $nodeProcesses) {
        Write-Host "  Остановка node процесса (PID: $($process.Id))" -ForegroundColor Cyan
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "   Все сервисы остановлены             " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Нажмите любую клавишу для выхода..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")