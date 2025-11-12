$folderPath = ""

# Vérifie si le chemin existe
if (-not (Test-Path -Path $folderPath -PathType Container)) {
    Write-Error "Le chemin spécifié n'existe pas ou n'est pas un dossier."
    exit 1
}

# Supprime le fichier .zip existant dans le dossier
Get-ChildItem -Path $folderPath -Filter "*.zip" | Remove-Item -Force -ErrorAction SilentlyContinue

# Récupère tous les fichiers du dossier (sans les sous-dossiers)
$filesToZip = Get-ChildItem -Path $folderPath -File

# Vérifie s'il y a des fichiers à zipper
if ($filesToZip.Count -eq 0) {
    Write-Host "Aucun fichier à zipper dans le dossier spécifié."
    exit 0
}

# Chemin du fichier zip de sortie
$zipFilePath = Join-Path -Path $folderPath -ChildPath "archive.zip"

# Crée le fichier zip
try {
    $filePaths = $filesToZip | ForEach-Object { $_.FullName }
    Compress-Archive -Path $filePaths -DestinationPath $zipFilePath -Force
    Write-Host "Le fichier $zipFilePath a été créé avec succès."
}
catch {
    Write-Error "Une erreur est survenue lors de la création du fichier zip : $_"
    exit 1
}

# Exécute la commande butler push
$butlerCommand = "butler push `"$zipFilePath`" PhilG/Mut8:"
Write-Host "Exécution de la commande : $butlerCommand"
Invoke-Expression $butlerCommand