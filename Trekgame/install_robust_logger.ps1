# Install robust logger
$source = "C:\Work\Sources\github\YesterdayOrTomorrow\Trekgame\logger_robust.py"
$dest = "C:\Program Files\AILocal\ComfyUI\app\logger.py"

# Copy the file
Copy-Item $source $dest -Force
Write-Host "Installed robust logger to $dest"

# Clear app pycache
$appCache = "C:\Program Files\AILocal\ComfyUI\app\__pycache__"
if (Test-Path $appCache) {
    Remove-Item -Path $appCache -Recurse -Force
    Write-Host "Cleared app __pycache__"
}

Write-Host "Done!"
