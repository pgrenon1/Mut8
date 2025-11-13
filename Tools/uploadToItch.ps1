$folderPath = ".\bin\Release\net8.0-windows\win-x64\publish"

dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishSingleFile=true --self-contained

# Check if the path exists
if (-not (Test-Path -Path $folderPath -PathType Container)) {
    Write-Error "The specified path does not exist or is not a folder."
    exit 1
}

# Execute the butler command to upload to itch
$butlerCommand = "butler push `"$folderPath`" PhilG/Mut8:win"
Write-Host "Executing command : $butlerCommand"
Invoke-Expression $butlerCommand