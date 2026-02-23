# Clear ALL Python cache files in ComfyUI and its venv
$comfyPath = "C:\Program Files\AILocal\ComfyUI"

Write-Host "Clearing all __pycache__ directories..."

# Find and delete all __pycache__ folders
$cacheCount = 0
Get-ChildItem -Path $comfyPath -Recurse -Directory -Filter "__pycache__" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "  Deleting: $($_.FullName)"
    Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    $cacheCount++
}

# Also delete any .pyc files directly
$pycCount = 0
Get-ChildItem -Path $comfyPath -Recurse -Filter "*.pyc" -ErrorAction SilentlyContinue | ForEach-Object {
    Remove-Item -Path $_.FullName -Force -ErrorAction SilentlyContinue
    $pycCount++
}

Write-Host ""
Write-Host "Deleted $cacheCount __pycache__ directories and $pycCount .pyc files"
Write-Host ""

# Verify patches
Write-Host "Verifying patches..."

$loggerPath = "$comfyPath\app\logger.py"
$loggerContent = Get-Content $loggerPath -Raw
if ($loggerContent -match "except \(OSError, ValueError, IOError\):") {
    Write-Host "  [OK] logger.py is patched"
} else {
    Write-Host "  [FAIL] logger.py patch NOT found!"
}

$tqdmPath = "$comfyPath\venv\Lib\site-packages\tqdm\std.py"
$tqdmContent = Get-Content $tqdmPath -Raw
if ($tqdmContent -match "try:\s+getattr\(sys\.stderr") {
    Write-Host "  [OK] tqdm std.py is patched"
} else {
    Write-Host "  [FAIL] tqdm std.py patch NOT found!"
}

Write-Host ""
Write-Host "Done! Please restart ComfyUI."
