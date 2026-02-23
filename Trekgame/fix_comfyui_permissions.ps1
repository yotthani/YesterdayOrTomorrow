# Fix all ComfyUI permission issues
$comfyPath = "C:\Program Files\AILocal\ComfyUI"

Write-Host "Fixing permissions for ComfyUI..."

# Grant full control to Users for the entire ComfyUI folder
icacls $comfyPath /grant "Users:(OI)(CI)F" /T /Q

Write-Host "Done! All permissions fixed."
