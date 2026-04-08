@echo off
REM Build script for Celestial Mechanics Desktop
REM Creates a self-contained single-file EXE with icon

echo === Celestial Mechanics Build Script ===
echo.

REM Get script directory and change to it
cd /d "%~dp0"
echo Working directory: %CD%

REM Step 1: Create icon if it doesn't exist
if not exist "src\CelestialMechanics.Desktop\app.ico" (
    echo Creating application icon...
    pushd src\CelestialMechanics.Desktop
    powershell.exe -ExecutionPolicy Bypass -File create-icon.ps1
    popd
    if errorlevel 1 (
        echo Warning: Icon creation failed. Building without custom icon.
    )
) else (
    echo Icon already exists.
)

REM Step 2: Build and publish
echo.
echo Building Release configuration...
dotnet publish "%~dp0src\CelestialMechanics.Desktop\CelestialMechanics.Desktop.csproj" -c Release -o "%~dp0publish"

if errorlevel 1 (
    echo.
    echo Build failed! Check errors above.
    pause
    exit /b 1
)

echo.
echo === Build Complete ===
echo.
echo Output location: %~dp0publish\CelestialMechanics.Desktop.exe
echo.
pause
