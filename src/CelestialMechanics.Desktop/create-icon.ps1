# Script to create a celestial-themed icon for the application
# Run: powershell -ExecutionPolicy Bypass -File create-icon.ps1
# Then rebuild the project to embed the icon

Add-Type -AssemblyName System.Drawing

$sizes = @(16, 32, 48, 256)
$iconPath = "$PSScriptRoot\app.ico"

$images = @()

foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    
    # Dark space background circle
    $bgBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 20, 25, 45))
    $g.FillEllipse($bgBrush, 0, 0, $size - 1, $size - 1)
    
    # Draw sun (yellow-orange)
    $sunSize = [int]($size * 0.35)
    $sunX = [int]($size * 0.25)
    $sunY = [int]($size * 0.25)
    $sunBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 200, 50))
    $g.FillEllipse($sunBrush, $sunX, $sunY, $sunSize, $sunSize)
    
    # Draw orbit ring
    $orbitPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(100, 100, 150, 255), [Math]::Max(1, $size / 32))
    $orbitPen.DashStyle = [System.Drawing.Drawing2D.DashStyle]::Dash
    $orbitMargin = [int]($size * 0.1)
    $g.DrawEllipse($orbitPen, $orbitMargin, $orbitMargin, $size - $orbitMargin * 2, $size - $orbitMargin * 2)
    
    # Draw planet (blue)
    $planetSize = [int]($size * 0.2)
    $planetX = [int]($size * 0.7)
    $planetY = [int]($size * 0.15)
    $planetBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 80, 140, 220))
    $g.FillEllipse($planetBrush, $planetX, $planetY, $planetSize, $planetSize)
    
    # Draw small moon
    $moonSize = [int]($size * 0.08)
    $moonX = [int]($size * 0.55)
    $moonY = [int]($size * 0.75)
    $moonBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 180, 180, 190))
    $g.FillEllipse($moonBrush, $moonX, $moonY, $moonSize, $moonSize)
    
    $g.Dispose()
    $images += $bmp
}

# Create ICO file
$ms = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($ms)
$writer.Write([UInt16]0)
$writer.Write([UInt16]1)
$writer.Write([UInt16]$images.Count)

$headerSize = 6
$dirEntrySize = 16
$dataOffset = $headerSize + ($dirEntrySize * $images.Count)
$imageData = @()

foreach ($img in $images) {
    $pngStream = New-Object System.IO.MemoryStream
    $img.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
    $imageData += ,($pngStream.ToArray())
    $pngStream.Dispose()
}

for ($i = 0; $i -lt $images.Count; $i++) {
    $size = $sizes[$i]
    $data = $imageData[$i]
    $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
    $writer.Write([byte]$(if ($size -eq 256) { 0 } else { $size }))
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([UInt16]1)
    $writer.Write([UInt16]32)
    $writer.Write([UInt32]$data.Length)
    $writer.Write([UInt32]$dataOffset)
    $dataOffset += $data.Length
}

foreach ($data in $imageData) { $writer.Write($data) }

$writer.Flush()
[System.IO.File]::WriteAllBytes($iconPath, $ms.ToArray())
$writer.Dispose()
$ms.Dispose()
foreach ($img in $images) { $img.Dispose() }

Write-Host "Icon created: $iconPath" -ForegroundColor Green
