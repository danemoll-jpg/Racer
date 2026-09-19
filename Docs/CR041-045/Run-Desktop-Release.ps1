param([ValidateSet('Routes','Systems')][string]$Lane='Systems')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$build=Join-Path $root 'Builds/Racer-0.6.1-review2-Windows'
$source=(Get-Content "$root/Temp/correction-source-commit.txt" -Raw).Trim()
if(!(Get-Content "$build/VERSION.txt" -Raw).Contains("Source commit: $source")){throw 'Source mismatch'}
if($Lane -eq 'Routes'){
 $cases=@(@{Name='release-routes';Args='-woodlandTest -woodlandLabel correction-release2-all'})
}else{
 foreach($dir in @('rules','audio-final','jamerson')){if(!(Test-Path "$PSScriptRoot/$dir-before-desktop")){Copy-Item "$PSScriptRoot/$dir" "$PSScriptRoot/$dir-before-desktop" -Recurse}}
 $cases=@(
  @{Name='release-rules';Args='-correctionRules'},
  @{Name='release-audio';Args='-correctionAudio'},
  @{Name='release-jump';Args='-correctionJump'},
  @{Name='normal-race';Args='-woodlandRace -racerDifficulty 1 -woodlandLaps 3'},
  @{Name='hard-race';Args='-woodlandRace -racerDifficulty 2 -woodlandLaps 3'},
  @{Name='release-flow';Args='-woodlandFlow'},
  @{Name='easy-race';Args='-woodlandRace -racerDifficulty 0 -woodlandLaps 1'},
  @{Name='hard-mistake';Args='-woodlandRace -racerDifficulty 2 -woodlandLaps 1 -deliberateMistake'}
 )
}
@{Source=$source;AssemblySHA256=(Get-FileHash "$build/Racer_Data/Managed/Assembly-CSharp.dll").Hash;Launch='Elevated desktop launch, windowed real-time player; no batchmode or manual physics stepping';Lane=$Lane} | ConvertTo-Json | Set-Content "$PSScriptRoot/desktop-$Lane-identity.json"
foreach($case in $cases){
 $name=$case.Name
 $arguments="-screen-fullscreen 0 -screen-width 1280 -screen-height 720 $($case.Args) -racerTestSave Temp/desktop-$name-save -raceTrace Docs/CR041-045/$name-trace.csv -logFile Temp/desktop-$name.log"
 "Start $name $(Get-Date -Format o)" | Tee-Object "$PSScriptRoot/desktop-$Lane-runs.txt" -Append
 # The user explicitly requested a visible standalone; execute this script on the user desktop.
 $process=Start-Process "$build/Racer.exe" -WorkingDirectory $root -ArgumentList $arguments -PassThru
 if(!$process.WaitForExit(2400000)){throw "Timeout $name; PID $($process.Id) retained for inspection"}
 "Exit $name code=$($process.ExitCode) $(Get-Date -Format o)" | Tee-Object "$PSScriptRoot/desktop-$Lane-runs.txt" -Append
 if($process.ExitCode -ne 0){throw "Failed process $name"}
}

