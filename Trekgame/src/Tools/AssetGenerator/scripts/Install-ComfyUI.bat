@echo off
title Star Trek Asset Generator - ComfyUI Installation
color 0B

echo.
echo  ============================================================
echo   Star Trek Asset Generator - ComfyUI Local Setup
echo  ============================================================
echo.
echo  This will install ComfyUI for local AI image generation.
echo  Requirements:
echo    - NVIDIA GPU with 8GB+ VRAM (RTX 3070 or better recommended)
echo    - Python 3.10 or newer
echo    - Git
echo    - ~15GB free disk space
echo.
echo  Your GPU:
nvidia-smi --query-gpu=name,memory.total --format=csv,noheader 2>nul || echo   (Could not detect - make sure NVIDIA drivers are installed)
echo.

set /p CONFIRM="Continue with installation? (Y/N): "
if /i not "%CONFIRM%"=="Y" (
    echo Installation cancelled.
    pause
    exit /b
)

echo.
echo  Choose installation type:
echo    1. Full Install (SDXL + SDXL Turbo) - ~10GB download
echo    2. Minimal Install (SDXL Turbo only) - ~3GB download
echo.
set /p INSTALL_TYPE="Enter choice (1 or 2): "

echo.
set /p INSTALL_PATH="Installation path [C:\ComfyUI]: "
if "%INSTALL_PATH%"=="" set INSTALL_PATH=C:\ComfyUI

echo.
echo  Starting installation...
echo.

if "%INSTALL_TYPE%"=="2" (
    powershell -ExecutionPolicy Bypass -File "%~dp0Setup-ComfyUI.ps1" -InstallPath "%INSTALL_PATH%" -MinimalInstall
) else (
    powershell -ExecutionPolicy Bypass -File "%~dp0Setup-ComfyUI.ps1" -InstallPath "%INSTALL_PATH%"
)

echo.
echo  ============================================================
echo   Installation complete! Press any key to exit.
echo  ============================================================
pause >nul
