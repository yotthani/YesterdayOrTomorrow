# Patch tqdm std.py to handle flush errors
$tqdmPath = "C:\Program Files\AILocal\ComfyUI\venv\Lib\site-packages\tqdm\std.py"

# Read the file
$content = Get-Content $tqdmPath -Raw

# Find and replace the problematic flush calls in status_printer
# Original:
#   if fp in (sys.stderr, sys.stdout):
#       getattr(sys.stderr, 'flush', lambda: None)()
#       getattr(sys.stdout, 'flush', lambda: None)()
#
# Patched to wrap in try/except:

$oldCode = @"
        if fp in (sys.stderr, sys.stdout):
            getattr(sys.stderr, 'flush', lambda: None)()
            getattr(sys.stdout, 'flush', lambda: None)()
"@

$newCode = @"
        if fp in (sys.stderr, sys.stdout):
            try:
                getattr(sys.stderr, 'flush', lambda: None)()
            except (OSError, ValueError, IOError):
                pass
            try:
                getattr(sys.stdout, 'flush', lambda: None)()
            except (OSError, ValueError, IOError):
                pass
"@

if ($content.Contains($oldCode)) {
    $content = $content.Replace($oldCode, $newCode)
    $content | Out-File -FilePath $tqdmPath -Encoding utf8NoBOM -Force
    Write-Host "Patched tqdm std.py successfully!"
} elseif ($content.Contains("try:`n                getattr(sys.stderr")) {
    Write-Host "tqdm std.py already patched"
} else {
    Write-Host "WARNING: Could not find the code to patch in tqdm std.py"
    Write-Host "Looking for alternative pattern..."

    # Try alternative - the code might have different indentation
    $lines = Get-Content $tqdmPath
    $newLines = @()
    $i = 0
    $patched = $false

    while ($i -lt $lines.Count) {
        $line = $lines[$i]

        # Look for the pattern
        if ($line -match "if fp in \(sys\.stderr, sys\.stdout\):" -and !$patched) {
            $newLines += $line
            $i++

            # Check next two lines for flush calls
            if ($i -lt $lines.Count -and $lines[$i] -match "getattr\(sys\.stderr.*flush") {
                $indent = $lines[$i] -replace "^(\s*).*", '$1'
                $newLines += "${indent}try:"
                $newLines += "    " + $lines[$i]
                $newLines += "${indent}except (OSError, ValueError, IOError):"
                $newLines += "${indent}    pass"
                $i++

                if ($i -lt $lines.Count -and $lines[$i] -match "getattr\(sys\.stdout.*flush") {
                    $newLines += "${indent}try:"
                    $newLines += "    " + $lines[$i]
                    $newLines += "${indent}except (OSError, ValueError, IOError):"
                    $newLines += "${indent}    pass"
                    $i++
                    $patched = $true
                }
            }
        } else {
            $newLines += $line
            $i++
        }
    }

    if ($patched) {
        $newLines | Out-File -FilePath $tqdmPath -Encoding utf8NoBOM -Force
        Write-Host "Patched tqdm std.py using alternative method!"
    } else {
        Write-Host "Could not patch - manual intervention needed"
    }
}

# Also delete tqdm __pycache__
$tqdmCache = "C:\Program Files\AILocal\ComfyUI\venv\Lib\site-packages\tqdm\__pycache__"
if (Test-Path $tqdmCache) {
    Remove-Item -Path $tqdmCache -Recurse -Force
    Write-Host "Deleted tqdm __pycache__"
}

Write-Host "Done!"
