# Install rich module with full permissions
$pipPath = "C:\Program Files\AILocal\ComfyUI\venv\Scripts\pip.exe"
$pythonPath = "C:\Program Files\AILocal\ComfyUI\venv\Scripts\python.exe"

# First grant permissions to site-packages
$sitePackages = "C:\Program Files\AILocal\ComfyUI\venv\Lib\site-packages"
icacls $sitePackages /grant "Users:(OI)(CI)F" /T

# Now install rich
& $pythonPath -m pip install rich --no-cache-dir

Write-Host "Done!"
