param(
    [Parameter(Mandatory=$true)]
    [string]$InputPath,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "",
    
    [Parameter(Mandatory=$false)]
    [int]$TileWidth = 16,
    
    [Parameter(Mandatory=$false)]
    [int]$TileHeight = 16,
    
    [Parameter(Mandatory=$false)]
    [int]$IndexFontSize = 12
)

# Add System.Drawing assembly
Add-Type -AssemblyName System.Drawing

function Add-TilesheetIndices {
    param(
        [string]$inputPath,
        [string]$outputPath,
        [int]$tileWidth,
        [int]$tileHeight,
        [int]$indexFontSize
    )
    
    try {
        # Load the original image
        Write-Host "Loading original image: $inputPath"
        $originalImage = [System.Drawing.Image]::FromFile($inputPath)
        
        # Calculate dimensions
        $originalWidth = $originalImage.Width
        $originalHeight = $originalImage.Height
        
        # Calculate number of tiles
        $tilesPerRow = [Math]::Floor($originalWidth / $tileWidth)
        $tilesPerColumn = [Math]::Floor($originalHeight / $tileHeight)
        
        Write-Host "Original image: ${originalWidth}x${originalHeight}"
        Write-Host "Tiles per row: $tilesPerRow, Tiles per column: $tilesPerColumn"
        
        # Calculate new dimensions
        $indexRowHeight = $tileHeight
        $indexColumnWidth = $tileWidth
        $newWidth = $originalWidth + (2 * $indexColumnWidth)
        $newHeight = $originalHeight + (2 * $indexRowHeight)
        
        Write-Host "New image dimensions: ${newWidth}x${newHeight}"
        
        # Create new bitmap
        $newImage = New-Object System.Drawing.Bitmap($newWidth, $newHeight)
        $graphics = [System.Drawing.Graphics]::FromImage($newImage)
        
        # Set background to black
        $graphics.Clear([System.Drawing.Color]::Black)
        
        # Copy original image to center
        $graphics.DrawImage($originalImage, $indexColumnWidth, $indexRowHeight)
        
        # Create font for indices
        $font = New-Object System.Drawing.Font("Consolas", $indexFontSize, [System.Drawing.FontStyle]::Bold)
        $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $stringFormat = New-Object System.Drawing.StringFormat
        $stringFormat.Alignment = [System.Drawing.StringAlignment]::Center
        $stringFormat.LineAlignment = [System.Drawing.StringAlignment]::Center
        
        # Add column indices at top
        for ($col = 0; $col -lt $tilesPerRow; $col++) {
            $x = $indexColumnWidth + ($col * $tileWidth) + ($tileWidth / 2)
            $y = $indexRowHeight / 2
            $graphics.DrawString($col.ToString(), $font, $brush, $x, $y, $stringFormat)
        }
        
        # Add column indices at bottom
        for ($col = 0; $col -lt $tilesPerRow; $col++) {
            $x = $indexColumnWidth + ($col * $tileWidth) + ($tileWidth / 2)
            $y = $indexRowHeight + $originalHeight + ($indexRowHeight / 2)
            $graphics.DrawString($col.ToString(), $font, $brush, $x, $y, $stringFormat)
        }
        
        # Add row indices at left
        for ($row = 0; $row -lt $tilesPerColumn; $row++) {
            $x = $indexColumnWidth / 2
            $y = $indexRowHeight + ($row * $tileHeight) + ($tileHeight / 2)
            $graphics.DrawString($row.ToString(), $font, $brush, $x, $y, $stringFormat)
        }
        
        # Add row indices at right
        for ($row = 0; $row -lt $tilesPerColumn; $row++) {
            $x = $indexColumnWidth + $originalWidth + ($indexColumnWidth / 2)
            $y = $indexRowHeight + ($row * $tileHeight) + ($tileHeight / 2)
            $graphics.DrawString($row.ToString(), $font, $brush, $x, $y, $stringFormat)
        }
        
        # Save the new image
        Write-Host "Saving new image: $outputPath"
        $newImage.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
        
        # Cleanup
        $graphics.Dispose()
        $newImage.Dispose()
        $originalImage.Dispose()
        $font.Dispose()
        $brush.Dispose()
        
        Write-Host "Successfully created tilesheet with indices!"
        
    } catch {
        Write-Error "Error processing image: $($_.Exception.Message)"
        throw
    }
}

# Validate input file exists
if (-not (Test-Path $InputPath)) {
    Write-Error "Input file does not exist: $InputPath"
    exit 1
}

# Set default output path if not provided
if ([string]::IsNullOrEmpty($OutputPath)) {
    $directory = Split-Path $InputPath -Parent
    $filename = [System.IO.Path]::GetFileNameWithoutExtension($InputPath)
    $extension = [System.IO.Path]::GetExtension($InputPath)
    $OutputPath = Join-Path $directory "${filename}_with_indices${extension}"
}

Write-Host "Input: $InputPath"
Write-Host "Output: $OutputPath"
Write-Host "Tile size: ${TileWidth}x${TileHeight}"
Write-Host "Index font size: $IndexFontSize"

# Call the function
Add-TilesheetIndices -inputPath $InputPath -outputPath $OutputPath -tileWidth $TileWidth -tileHeight $TileHeight -indexFontSize $IndexFontSize
