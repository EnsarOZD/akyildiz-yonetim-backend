@echo off
REM Akyildiz Yonetim Backend Build Script
REM This script helps build and test the application locally before deployment

setlocal enabledelayedexpansion

REM Parse command line arguments
set CONFIGURATION=Release
set CLEAN=false
set TEST=false
set DOCKER=false

:parse_args
if "%1"=="" goto :start
if "%1"=="--help" goto :show_help
if "%1"=="-h" goto :show_help
if "%1"=="--clean" set CLEAN=true
if "%1"=="--test" set TEST=true
if "%1"=="--docker" set DOCKER=true
if "%1"=="--debug" set CONFIGURATION=Debug
if "%1"=="--release" set CONFIGURATION=Release
shift
goto :parse_args

:show_help
echo Akyildiz Yonetim Backend Build Script
echo.
echo Usage: build.bat [options]
echo.
echo Options:
echo   --debug              Build in Debug configuration
echo   --release            Build in Release configuration (default)
echo   --clean              Clean build outputs before building
echo   --test               Run tests after building
echo   --docker             Build Docker image
echo   --help, -h           Show this help message
echo.
exit /b 0

:start
echo 🚀 Starting Akyildiz Yonetim Backend Build Process...
echo Configuration: %CONFIGURATION%

REM Check if .NET SDK is available
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET SDK not found. Please install .NET 8.0 SDK.
    exit /b 1
)
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ .NET SDK Version: !DOTNET_VERSION!

REM Clean if requested
if "%CLEAN%"=="true" (
    echo 🧹 Cleaning build outputs...
    dotnet clean --configuration %CONFIGURATION%
    if errorlevel 1 (
        echo ❌ Clean failed!
        exit /b 1
    )
    echo ✅ Clean completed
)

REM Restore packages
echo 📦 Restoring packages...
dotnet restore
if errorlevel 1 (
    echo ❌ Package restore failed!
    exit /b 1
)
echo ✅ Packages restored

REM Build the solution
echo 🔨 Building solution...
dotnet build --configuration %CONFIGURATION% --no-restore
if errorlevel 1 (
    echo ❌ Build failed!
    exit /b 1
)
echo ✅ Build completed successfully

REM Run tests if requested
if "%TEST%"=="true" (
    echo 🧪 Running tests...
    dotnet test --configuration %CONFIGURATION% --no-build --verbosity normal
    if errorlevel 1 (
        echo ❌ Tests failed!
        exit /b 1
    )
    echo ✅ All tests passed
)

REM Build Docker image if requested
if "%DOCKER%"=="true" (
    echo 🐳 Building Docker image...
    
    REM Check if Docker is available
    docker --version >nul 2>&1
    if errorlevel 1 (
        echo ❌ Docker not found. Please install Docker Desktop.
        exit /b 1
    )
    for /f "tokens=*" %%i in ('docker --version') do set DOCKER_VERSION=%%i
    echo ✅ Docker Version: !DOCKER_VERSION!
    
    REM Build Docker image
    docker build -t akyildiz-yonetim-backend:%CONFIGURATION% .
    if errorlevel 1 (
        echo ❌ Docker build failed!
        exit /b 1
    )
    echo ✅ Docker image built successfully
)

echo 🎉 Build process completed successfully!
echo.
echo Next steps:
echo   - Run the API: dotnet run --project AkyildizYonetim.API
echo   - Access Swagger: https://localhost:7001/swagger
echo   - Run with Docker: docker run -p 8080:8080 akyildiz-yonetim-backend:%CONFIGURATION%

endlocal 