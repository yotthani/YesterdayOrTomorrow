
Add-Type -AssemblyName System.Drawing

$targetDir = "Trekgame/src/Presentation/Web/wwwroot/images/theme-federation"
if (-not (Test-Path $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force }

function Get-Color {
    param([string]$hex)
    return [System.Drawing.ColorTranslator]::FromHtml($hex)
}

$c_orange = Get-Color "#FF9900"
$c_gold   = Get-Color "#FFCC00"
$c_tan    = Get-Color "#CC9966"
$c_purple = Get-Color "#CC99CC"
$c_blue   = Get-Color "#9999FF"
$c_black  = Get-Color "#000000"

function Save-Image {
    param($bitmap, $filename)
    $path = Join-Path $targetDir $filename
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Host "Saved $path"
}

# 1. Elbow Top-Left (The classic TNG connector)
# Shape: 
#   ________
#  /
# |   ____
# |  |
# |  |
$width = 200
$height = 100
$thickness = 40
$radius = 60

$bmp = New-Object System.Drawing.Bitmap $width, $height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

# Gradient Brush
$rect = New-Object System.Drawing.Rectangle 0, 0, $width, $height
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $c_orange, $c_gold, 0.0

# Define the path for the elbow
$path = New-Object System.Drawing.Drawing2D.GraphicsPath

# Outer curve (top-left)
$path.AddArc(0, 0, $radius*2, $radius*2, 180, 90) # Top-left corner arc
$path.AddLine($radius, 0, $width, 0)              # Top edge
$path.AddLine($width, $thickness, $radius, $thickness) # Top-inner edge (horizontal)
# Inner curve (top-left, sharper)
$innerRadius = $radius - $thickness
if ($innerRadius -gt 0) {
    $path.AddArc($thickness, $thickness, $innerRadius*2, $innerRadius*2, 270, -90)
} else {
    $path.AddLine($thickness, $thickness, $thickness, $height)
}
$path.AddLine($thickness, $height, 0, $height)    # Left edge (vertical)
$path.CloseFigure()

$g.FillPath($brush, $path)

# Add a subtle highlight line
$pen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(100, 255, 255, 255)), 2
$g.DrawPath($pen, $path)

Save-Image $bmp "lcars_elbow_tl.png"
$g.Dispose()
$bmp.Dispose()


# 2. Horizontal Bar Cap (Right)
$width = 100
$height = 40
$bmp = New-Object System.Drawing.Bitmap $width, $height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$rect = New-Object System.Drawing.Rectangle 0, 0, $width, $height
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $c_tan, $c_orange, 0.0

$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddLine(0, 0, $width - ($height/2), 0)
$path.AddArc($width - $height, 0, $height, $height, 270, 180)
$path.AddLine($width - ($height/2), $height, 0, $height)
$path.CloseFigure()

$g.FillPath($brush, $path)
Save-Image $bmp "lcars_cap_right.png"
$g.Dispose()
$bmp.Dispose()


# 3. Vertical Bar Cap (Bottom)
$width = 40
$height = 100
$bmp = New-Object System.Drawing.Bitmap $width, $height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$rect = New-Object System.Drawing.Rectangle 0, 0, $width, $height
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $c_purple, $c_blue, 90.0

$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddLine(0, 0, 0, $height - ($width/2))
$path.AddArc(0, $height - $width, $width, $width, 180, -180)
$path.AddLine($width, $height - ($width/2), $width, 0)
$path.CloseFigure()

$g.FillPath($brush, $path)
Save-Image $bmp "lcars_cap_bottom.png"
$g.Dispose()
$bmp.Dispose()


# 4. Pill Button (Standard)
$width = 120
$height = 40
$bmp = New-Object System.Drawing.Bitmap $width, $height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$rect = New-Object System.Drawing.Rectangle 0, 0, $width, $height
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $c_orange, $c_gold, 90.0

$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$path.AddArc(0, 0, $height, $height, 90, 180)
$path.AddLine($height/2, 0, $width - ($height/2), 0)
$path.AddArc($width - $height, 0, $height, $height, 270, 180)
$path.CloseFigure()

$g.FillPath($brush, $path)
Save-Image $bmp "lcars_pill.png"
$g.Dispose()
$bmp.Dispose()

