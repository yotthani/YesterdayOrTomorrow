$tqdmCache = "C:\Program Files\AILocal\ComfyUI\venv\Lib\site-packages\tqdm\__pycache__"
if (Test-Path $tqdmCache) {
    Remove-Item -Path $tqdmCache -Recurse -Force
    Write-Host "Deleted tqdm __pycache__"
} else {
    Write-Host "No tqdm __pycache__ found"
}
