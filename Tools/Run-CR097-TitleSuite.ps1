param([string]$Runtime='Builds/Racer-0.17.0-review1-Windows/Racer.exe',[string]$Tag='release-title')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
foreach($case in @(
 @{Name='on-keyboard-720';Height=720;On=$true;Master=0;Flags='-titleCheck loops -titleInput keyboard'},
 @{Name='off-held-gamepad-800';Height=800;On=$false;Master=0;Flags='-titleCheck loops -titleInput gamepad -held yes'},
 @{Name='early-mouse-delayed-800';Height=800;On=$true;Master=0;Flags='-titleCheck skip -titleInput mouse -titleDelay 8'},
 @{Name='targeted-audio-720';Height=720;On=$false;Master=.65;Flags='-titleCheck skip -titleInput keyboard -racerAudioCheck yes'}
)){
 $save=Join-Path $project ('Temp/cr097-title-'+[Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory -Path $save | Out-Null
 @{version=1;master=$case.Master;music=.37;radioOn=$case.On;radioChannel=$(if($case.On){'folder:Groove'}else{'off:'});musicSource='bundled';frameLimit=60}|ConvertTo-Json|Set-Content "$save/settings.json"
 & "$PSScriptRoot/Run-CR097-Check.ps1" -Runtime $Runtime -Tag $Tag -Name $case.Name -Flags $case.Flags -Height $case.Height -ReuseSave $save -Limit 180
}
