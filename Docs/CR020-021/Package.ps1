$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$name = 'Racer-0.2.0-review1-Windows'
$buildRoot = Join-Path $projectRoot "Builds/$name"
$stageRoot = Join-Path $projectRoot "Builds/Package/$name"
$archive = Join-Path $projectRoot "Builds/$name.zip"
if (Test-Path $stageRoot) { throw 'Use a fresh staging directory; existing staging is not overwritten.' }
if (Test-Path $archive) { throw 'Archive already exists; do not overwrite a reviewed delivery.' }
foreach ($required in @('Racer.exe','UnityPlayer.dll','Racer_Data','MonoBleedingEdge','VERSION.txt')) {
    if (!(Test-Path (Join-Path $buildRoot $required))) { throw "Missing runtime item: $required" }
}
$excluded = @()
Get-ChildItem -LiteralPath $buildRoot -Recurse -File | ForEach-Object {
    $relative = [IO.Path]::GetRelativePath($buildRoot, $_.FullName)
    if ($relative -match 'DoNotShip|Do Not Ship|BackUpThisFolder|(^|[\\/])\.git([\\/]|$)|\.(pdb|mdb|log)$') {
        $excluded += $relative
    } else {
        $target = Join-Path $stageRoot $relative
        New-Item -ItemType Directory -Force ([IO.Path]::GetDirectoryName($target)) | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $target
    }
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README-player.txt') -Destination (Join-Path $stageRoot 'README.txt')
$licenses = Join-Path $stageRoot 'LICENSES'
New-Item -ItemType Directory -Force $licenses | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'AUDIO-ASSET-NOTICES.txt') -Destination $licenses
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Unity-Windows-Mono-Notices.pdf') -Destination $licenses
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PackageNotices') -Destination $licenses -Recurse
$manifest = Get-ChildItem -LiteralPath $stageRoot -Recurse -File | ForEach-Object {
    [pscustomobject]@{ Path=[IO.Path]::GetRelativePath($stageRoot,$_.FullName); Bytes=$_.Length; SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
}
$manifest | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $PSScriptRoot 'package-files.json')
$excluded | Set-Content (Join-Path $PSScriptRoot 'package-excluded.txt')
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($stageRoot, $archive, [IO.Compression.CompressionLevel]::Optimal, $true)
$zip = Get-Item -LiteralPath $archive
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash
@{Archive=$archive;Bytes=$zip.Length;SHA256=$hash;Files=$manifest.Count} | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'package.json')
Get-Content (Join-Path $PSScriptRoot 'package.json')
