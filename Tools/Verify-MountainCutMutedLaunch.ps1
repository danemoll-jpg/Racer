$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$settings=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3/settings.json'
$backup=Join-Path $root 'Temp/mountaincut-player-settings-backup.json'
if(Get-Process Racer -ErrorAction SilentlyContinue){throw 'Close existing game before settings-preserving launcher check'}
if(Test-Path -LiteralPath $backup){throw 'Existing backup requires inspection before retry'}
$before=[IO.File]::ReadAllBytes($settings);$hash=(Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
[IO.File]::WriteAllBytes($backup,$before)
try {
 $options=[Text.Encoding]::UTF8.GetString($before)|ConvertFrom-Json
 $options.master=0
 [IO.File]::WriteAllText($settings,($options|ConvertTo-Json -Depth 30))
 & (Join-Path $PSScriptRoot 'Verify-MountainCutPlayRacer.ps1')
}finally {
 [IO.File]::WriteAllBytes($settings,$before)
 $after=(Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
 if($after -ne $hash){throw 'Settings restoration hash mismatch; backup retained'}
 @{settingsRestored=$true;sha256=$after;verification='Temporary master mute, original bytes restored'}|ConvertTo-Json|Set-Content (Join-Path $root 'Docs/MountainCut/settings-preserved.json')
 Remove-Item -LiteralPath $backup
}
