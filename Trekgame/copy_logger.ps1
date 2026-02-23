# Copy patched logger
$source = "C:\Work\Sources\github\YesterdayOrTomorrow\Trekgame\logger_patched.py"
$dest = "C:\Program Files\AILocal\ComfyUI\app\logger.py"

# Clear pycache first
$comfyPath = "C:\Program Files\AILocal\ComfyUI"
Get-ChildItem -Path $comfyPath -Recurse -Directory -Filter "__pycache__" -ErrorAction SilentlyContinue | ForEach-Object {
    Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
}

# Copy the file
Copy-Item $source $dest -Force
Write-Host "Copied patched logger to $dest"

# Show contents to verify
Write-Host "`nFlush method in new file:"
Get-Content $dest | Select-Object -Index (37,38,39,40,41,42,43,44,45)
