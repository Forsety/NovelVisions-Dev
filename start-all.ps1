# NovelVision - Start All Services Script
# Includes: Catalog.API, Visualization.API, PromptGen.API, Gateway, Frontend

param(
    [switch]$FrontendOnly,
    [switch]$BackendOnly,
    [switch]$All,
    [switch]$NoGateway,
    [switch]$Status
)

$ErrorActionPreference = "Continue"

# ==================== HELPERS ====================

function Write-Success { param([string]$Text) Write-Host "[OK] $Text" -ForegroundColor Green }
function Write-Info { param([string]$Text) Write-Host "[INFO] $Text" -ForegroundColor Yellow }
function Write-Err { param([string]$Text) Write-Host "[ERROR] $Text" -ForegroundColor Red }
function Write-Header { param([string]$Text) Write-Host "`n$Text" -ForegroundColor Magenta }

# ==================== PATHS ====================

$RootPath = $PSScriptRoot
$CatalogApiPath = Join-Path $RootPath "src\Services\Catalog.API\NovelVision.Services.Catalog.API"
$VisualizationApiPath = Join-Path $RootPath "src\Services\Visualization.API\NovelVision.Services.Visualization.API"
$PromptGenApiPath = Join-Path $RootPath "src\Services\PromptGen.API"
$GatewayApiPath = Join-Path $RootPath "src\ApiGateway\NovelVision.Gateway"
$FrontendPath = Join-Path $RootPath "src\WebUI\novel-vision-web"

# ==================== PORTS ====================

$Ports = @{
    "Catalog.API" = @(5231, 7295)
    "Visualization.API" = @(5130, 7130)
    "PromptGen.API" = @(8000)
    "Gateway" = @(5000)
    "Frontend" = @(3000)
}

# ==================== STATUS CHECK ====================

function Check-ServiceStatus {
    Write-Header "Service Status Check"
    
    $services = @(
        @{ Name = "Catalog.API"; Url = "http://localhost:5231/health" },
        @{ Name = "Catalog.API (HTTPS)"; Url = "https://localhost:7295/health" },
        @{ Name = "Visualization.API"; Url = "https://localhost:7130/health" },
        @{ Name = "PromptGen.API"; Url = "http://localhost:8000/health" },
        @{ Name = "Gateway"; Url = "http://localhost:5000/health" },
        @{ Name = "Frontend"; Url = "http://localhost:3000" }
    )
    
    foreach ($svc in $services) {
        try {
            $response = Invoke-WebRequest -Uri $svc.Url -TimeoutSec 2 -UseBasicParsing -ErrorAction Stop
            Write-Success "$($svc.Name) - Running"
        } catch {
            Write-Err "$($svc.Name) - Not responding"
        }
    }
}

if ($Status) {
    Check-ServiceStatus
    return
}

# ==================== LOGO ====================

Write-Host ""
Write-Host "  _   _                 ___      ___     _             " -ForegroundColor Cyan
Write-Host " | \ | | _____   _____| \ \    / (_)___(_) ___  _ __  " -ForegroundColor Cyan
Write-Host " |  \| |/ _ \ \ / / _ \ |\ \  / /| / __| |/ _ \| '_ \ " -ForegroundColor Cyan
Write-Host " | |\  | (_) \ V /  __/ | \ \/ / | \__ \ | (_) | | | |" -ForegroundColor Cyan
Write-Host " |_| \_|\___/ \_/ \___|_|  \__/  |_|___/_|\___/|_| |_|" -ForegroundColor Cyan
Write-Host ""
Write-Host "  AI-Powered Book Visualization Platform" -ForegroundColor DarkGray
Write-Host ""

# ==================== START FUNCTIONS ====================

function Start-CatalogApi {
    Write-Info "Starting Catalog.API (ports 5231/7295)..."
    if (Test-Path $CatalogApiPath) {
        Start-Process powershell -ArgumentList "-NoExit", "-Command", `
            "`$host.UI.RawUI.WindowTitle = 'Catalog.API'; cd '$CatalogApiPath'; dotnet run"
        Write-Success "Catalog.API starting..."
        return $true
    } else {
        Write-Err "Catalog.API not found: $CatalogApiPath"
        return $false
    }
}

function Start-VisualizationApi {
    Write-Info "Starting Visualization.API (ports 5130/7130)..."
    if (Test-Path $VisualizationApiPath) {
        Start-Process powershell -ArgumentList "-NoExit", "-Command", `
            "`$host.UI.RawUI.WindowTitle = 'Visualization.API'; cd '$VisualizationApiPath'; dotnet run"
        Write-Success "Visualization.API starting..."
        return $true
    } else {
        Write-Err "Visualization.API not found: $VisualizationApiPath"
        return $false
    }
}

function Start-PromptGenApi {
    Write-Info "Starting PromptGen.API (port 8000)..."
    if (Test-Path $PromptGenApiPath) {
        # Check if Python venv exists
        $venvPath = Join-Path $PromptGenApiPath "venv"
        $activateScript = Join-Path $venvPath "Scripts\Activate.ps1"
        
        if (Test-Path $activateScript) {
            Start-Process powershell -ArgumentList "-NoExit", "-Command", `
                "`$host.UI.RawUI.WindowTitle = 'PromptGen.API'; cd '$PromptGenApiPath'; & '$activateScript'; uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload"
        } else {
            # Try without venv
            Start-Process powershell -ArgumentList "-NoExit", "-Command", `
                "`$host.UI.RawUI.WindowTitle = 'PromptGen.API'; cd '$PromptGenApiPath'; python -m uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload"
        }
        Write-Success "PromptGen.API starting..."
        return $true
    } else {
        Write-Err "PromptGen.API not found: $PromptGenApiPath"
        return $false
    }
}

function Start-Gateway {
    Write-Info "Starting Gateway (port 5000)..."
    if (Test-Path $GatewayApiPath) {
        Start-Process powershell -ArgumentList "-NoExit", "-Command", `
            "`$host.UI.RawUI.WindowTitle = 'API Gateway'; cd '$GatewayApiPath'; dotnet run"
        Write-Success "Gateway starting..."
        return $true
    } else {
        Write-Err "Gateway not found: $GatewayApiPath"
        return $false
    }
}

function Start-Frontend {
    Write-Info "Starting Frontend (port 3000)..."
    if (Test-Path $FrontendPath) {
        Start-Process powershell -ArgumentList "-NoExit", "-Command", `
            "`$host.UI.RawUI.WindowTitle = 'NovelVision Frontend'; cd '$FrontendPath'; npm start"
        Write-Success "Frontend starting..."
        return $true
    } else {
        Write-Err "Frontend not found: $FrontendPath"
        return $false
    }
}

# ==================== MODES ====================

# Frontend Only
if ($FrontendOnly) {
    Write-Header "Starting Frontend Only"
    if (Test-Path $FrontendPath) {
        Set-Location $FrontendPath
        npm start
    } else {
        Write-Err "Frontend not found: $FrontendPath"
    }
    return
}

# Backend Only (with or without Gateway)
if ($BackendOnly) {
    Write-Header "Starting Backend Services"
    
    Start-CatalogApi
    Start-Sleep -Seconds 2
    
    Start-VisualizationApi
    Start-Sleep -Seconds 2
    
    Start-PromptGenApi
    Start-Sleep -Seconds 2
    
    if (-not $NoGateway) {
        Start-Gateway
    } else {
        Write-Info "Gateway skipped (NoGateway flag)"
    }
    
    Write-Host ""
    Write-Success "Backend services started!"
    Write-Host ""
    Write-Host "Endpoints:" -ForegroundColor Cyan
    Write-Host "  Catalog.API:       http://localhost:5231  |  https://localhost:7295" -ForegroundColor White
    Write-Host "  Visualization.API: http://localhost:5130  |  https://localhost:7130" -ForegroundColor White
    Write-Host "  PromptGen.API:     http://localhost:8000" -ForegroundColor White
    if (-not $NoGateway) {
        Write-Host "  Gateway:           http://localhost:5000" -ForegroundColor White
    }
    return
}

# All Services
if ($All) {
    Write-Header "Starting All Services"
    
    # Backend
    Start-CatalogApi
    Start-Sleep -Seconds 2
    
    Start-VisualizationApi
    Start-Sleep -Seconds 2
    
    Start-PromptGenApi
    Start-Sleep -Seconds 2
    
    if (-not $NoGateway) {
        Start-Gateway
        Start-Sleep -Seconds 2
    }
    
    # Frontend
    Start-Frontend
    
    Write-Host ""
    Write-Success "All services started in separate windows!"
    Write-Host ""
    Write-Host "Endpoints:" -ForegroundColor Cyan
    Write-Host "  Catalog.API:       http://localhost:5231  |  https://localhost:7295" -ForegroundColor White
    Write-Host "  Visualization.API: http://localhost:5130  |  https://localhost:7130" -ForegroundColor White
    Write-Host "  PromptGen.API:     http://localhost:8000" -ForegroundColor White
    if (-not $NoGateway) {
        Write-Host "  Gateway:           http://localhost:5000" -ForegroundColor White
    }
    Write-Host "  Frontend:          http://localhost:3000" -ForegroundColor White
    Write-Host ""
    Write-Host "Swagger UI:" -ForegroundColor Cyan
    Write-Host "  Catalog:       https://localhost:7295/swagger" -ForegroundColor White
    Write-Host "  Visualization: https://localhost:7130/swagger" -ForegroundColor White
    Write-Host "  PromptGen:     http://localhost:8000/docs" -ForegroundColor White
    return
}

# ==================== INTERACTIVE MENU ====================

Write-Host "What would you like to start?" -ForegroundColor Cyan
Write-Host ""
Write-Host "  [1] Frontend only" -ForegroundColor White
Write-Host "  [2] Backend only (Catalog + Visualization + PromptGen + Gateway)" -ForegroundColor White
Write-Host "  [3] Backend only (NO Gateway - direct API access)" -ForegroundColor White
Write-Host "  [4] All services" -ForegroundColor White
Write-Host "  [5] All services (NO Gateway)" -ForegroundColor White
Write-Host "  [6] Check service status" -ForegroundColor White
Write-Host ""
Write-Host "  Individual services:" -ForegroundColor DarkGray
Write-Host "  [C] Catalog.API only" -ForegroundColor DarkGray
Write-Host "  [V] Visualization.API only" -ForegroundColor DarkGray
Write-Host "  [P] PromptGen.API only" -ForegroundColor DarkGray
Write-Host "  [G] Gateway only" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  [Q] Quit" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Your choice"

switch ($choice.ToUpper()) {
    "1" { 
        & $PSCommandPath -FrontendOnly
    }
    "2" { 
        & $PSCommandPath -BackendOnly
    }
    "3" { 
        & $PSCommandPath -BackendOnly -NoGateway
    }
    "4" { 
        & $PSCommandPath -All
    }
    "5" { 
        & $PSCommandPath -All -NoGateway
    }
    "6" {
        Check-ServiceStatus
    }
    "C" {
        Start-CatalogApi
    }
    "V" {
        Start-VisualizationApi
    }
    "P" {
        Start-PromptGenApi
    }
    "G" {
        Start-Gateway
    }
    "Q" {
        Write-Host "Bye!" -ForegroundColor Cyan
    }
    default { 
        Write-Host "Invalid choice. Bye!" -ForegroundColor Yellow
    }
}