# Clear all Python cache files in ComfyUI
$comfyPath = "C:\Program Files\AILocal\ComfyUI"

# Find and delete all __pycache__ folders
Get-ChildItem -Path $comfyPath -Recurse -Directory -Filter "__pycache__" | ForEach-Object {
    Write-Host "Deleting: $($_.FullName)"
    Remove-Item -Path $_.FullName -Recurse -Force
}

# Also delete any .pyc files directly
Get-ChildItem -Path $comfyPath -Recurse -Filter "*.pyc" | ForEach-Object {
    Write-Host "Deleting: $($_.FullName)"
    Remove-Item -Path $_.FullName -Force
}

Write-Host "Cache cleared!"

# Verify the logger.py patch
$loggerPath = "$comfyPath\app\logger.py"
$content = Get-Content $loggerPath -Raw
if ($content -match "except OSError:") {
    Write-Host "Logger patch verified - OSError exception handling is present"
} else {
    Write-Host "WARNING: Logger patch may not be correct!"
}
