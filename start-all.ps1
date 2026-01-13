# start-all.ps1
# Script for starting all NovelVision services

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   NovelVision - Starting All Services  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we are in the correct directory
if (-not (Test-Path ".\src\Services\Catalog.API")) {
    Write-Host "Error: Run script from NovelVision root directory" -ForegroundColor Red
    exit 1
}

# Function to start service
function Start-Service {
    param(
        [string]$Name,
        [string]$Path,
        [string]$Command,
        [string]$Port
    )
    
    Write-Host "Starting $Name on port $Port..." -ForegroundColor Yellow
    
    $fullPath = Join-Path $PWD $Path
    
    Start-Process cmd -ArgumentList "/k", "cd /d `"$fullPath`" && $Command" -PassThru | Out-Null
    
    Write-Host "OK - $Name started" -ForegroundColor Green
}

# 1. Start API Gateway (port 5000)
Write-Host "`n[1/3] API Gateway" -ForegroundColor Magenta
Start-Service -Name "API Gateway" `
              -Path "src\ApiGateway\NovelVision.Gateway" `
              -Command "dotnet run --urls=http://localhost:5000" `
              -Port "5000"

Start-Sleep -Seconds 2

# 2. Start Catalog.API (port 7295 for HTTPS, 5001 for HTTP)
Write-Host "`n[2/3] Catalog.API (with Identity)" -ForegroundColor Magenta
Start-Service -Name "Catalog.API" `
              -Path "src\Services\Catalog.API\NovelVision.Services.Catalog.API" `
              -Command "dotnet run" `
              -Port "7295/5001"

Start-Sleep -Seconds 3

# 3. Start Frontend (port 3000)
Write-Host "`n[3/3] Frontend React App" -ForegroundColor Magenta
Start-Service -Name "Frontend" `
              -Path "src\WebUI\novel-vision-web" `
              -Command "npm start" `
              -Port "3000"

# Service information
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "   All services started successfully!   " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Available URLs:" -ForegroundColor Cyan
Write-Host "  - Frontend:     " -NoNewline; Write-Host "http://localhost:3000" -ForegroundColor Yellow
Write-Host "  - Catalog API:  " -NoNewline; Write-Host "https://localhost:7295/swagger" -ForegroundColor Yellow
Write-Host "  - Catalog HTTP: " -NoNewline; Write-Host "http://localhost:5001/swagger" -ForegroundColor Yellow
Write-Host "  - API Gateway:  " -NoNewline; Write-Host "http://localhost:5000" -ForegroundColor Yellow
Write-Host ""
Write-Host "To stop all services use:" -ForegroundColor Cyan
Write-Host "  .\stop-all.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Enter to exit..."
Read-Host