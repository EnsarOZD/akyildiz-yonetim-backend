# Akyildiz Yonetim Backend Build Script
# This script helps build and test the application locally before deployment

param(
    [string]$Configuration = "Release",
    [switch]$Clean,
    [switch]$Test,
    [switch]$Docker,
    [switch]$Help
)

if ($Help) {
    Write-Host "Akyildiz Yonetim Backend Build Script"
    Write-Host ""
    Write-Host "Usage: .\build.ps1 [options]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -Configuration <config>  Build configuration (Debug|Release, default: Release)"
    Write-Host "  -Clean                   Clean build outputs before building"
    Write-Host "  -Test                    Run tests after building"
    Write-Host "  -Docker                  Build Docker image"
    Write-Host "  -Help                    Show this help message"
    Write-Host ""
    exit 0
}

Write-Host "🚀 Starting Akyildiz Yonetim Backend Build Process..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Check if .NET SDK is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8.0 SDK." -ForegroundColor Red
    exit 1
}

# Clean if requested
if ($Clean) {
    Write-Host "🧹 Cleaning build outputs..." -ForegroundColor Yellow
    dotnet clean --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Clean failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Clean completed" -ForegroundColor Green
}

# Restore packages
Write-Host "📦 Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restore failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Packages restored" -ForegroundColor Green

# Build the solution
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build completed successfully" -ForegroundColor Green

# Run tests if requested
if ($Test) {
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tests failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ All tests passed" -ForegroundColor Green
}

# Build Docker image if requested
if ($Docker) {
    Write-Host "🐳 Building Docker image..." -ForegroundColor Yellow
    
    # Check if Docker is available
    try {
        $dockerVersion = docker --version
        Write-Host "✅ Docker Version: $dockerVersion" -ForegroundColor Green
    } catch {
        Write-Host "❌ Docker not found. Please install Docker Desktop." -ForegroundColor Red
        exit 1
    }
    
    # Build Docker image
    docker build -t akyildiz-yonetim-backend:$Configuration .
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Docker build failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Docker image built successfully" -ForegroundColor Green
}

Write-Host "🎉 Build process completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  - Run the API: dotnet run --project AkyildizYonetim.API"
Write-Host "  - Access Swagger: https://localhost:7001/swagger"
Write-Host "  - Run with Docker: docker run -p 8080:8080 akyildiz-yonetim-backend:Release" 