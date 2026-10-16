# Play-Racer.cmd launch with the master volume muted for the check; Dan's settings.json restored byte for byte, and his campaign saves
# (both slots and their backups) checked byte for byte before and after.
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$dir=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3'
$settings=Join-Path $dir 'settings.json'
$backup=Join-Path $root 'Temp/report1000-player-settings-backup.json'
if(Get-Process Racer -ErrorAction SilentlyContinue){throw 'Close existing game before settings-preserving launcher check'}
if(Test-Path -LiteralPath $backup){throw 'Existing backup requires inspection before retry'}
$saves=@(Get-ChildItem -LiteralPath $dir -File -Filter 'campaign*' | Sort-Object Name)
$savesBefore=@{};foreach($f in $saves){$savesBefore[$f.Name]=(Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash}
$before=[IO.File]::ReadAllBytes($settings);$hash=(Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
New-Item -ItemType Directory -Force (Split-Path $backup) | Out-Null
[IO.File]::WriteAllBytes($backup,$before)
try {
 $options=[Text.Encoding]::UTF8.GetString($before)|ConvertFrom-Json
 $options.master=0
 [IO.File]::WriteAllText($settings,($options|ConvertTo-Json -Depth 30))
 & (Join-Path $PSScriptRoot 'Verify-Report1000PlayRacer.ps1')
}finally {
 [IO.File]::WriteAllBytes($settings,$before)
 $after=(Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
 if($after -ne $hash){throw 'Settings restoration hash mismatch; backup retained'}
 $changed=@($saves | Where-Object {(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash -ne $savesBefore[$_.Name]} | ForEach-Object Name)
 @{settingsRestored=$true;sha256=$after;verification='Temporary master mute, original bytes restored';campaignSaves=$savesBefore;campaignSavesChanged=$changed}|ConvertTo-Json -Depth 5|Set-Content (Join-Path $root 'Docs/Report1000/settings-preserved.json')
 Remove-Item -LiteralPath $backup
 if($changed.Count){throw ('Campaign save changed: '+($changed -join ', '))}
}
