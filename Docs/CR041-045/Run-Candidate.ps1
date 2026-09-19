param([int]$PreviousProcess=0,[switch]$AfterJump,[string]$Only='')
$ErrorActionPreference='Stop'
$projectRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$exe=Join-Path $projectRoot 'Temp/CorrectionPlayer7/Racer.exe'
if(!(Test-Path -LiteralPath $exe)){throw 'Candidate two is not built'}
if($PreviousProcess -gt 0){$previous=Get-Process -Id $PreviousProcess -ErrorAction SilentlyContinue;if($previous){Wait-Process -Id $PreviousProcess}}
$cases=@(
 @{Name='rules';Args='-correctionRules'},
 @{Name='audio';Args='-correctionAudio'},
 @{Name='jump';Args='-correctionJump'},
 @{Name='gully-recovery';Args='-woodlandTest -woodlandRoute "Fox Gully" -woodlandLabel correction-gully-inside-recovery'},
 @{Name='normal-race';Args='-woodlandRace -racerDifficulty 1 -woodlandLaps 3'},
 @{Name='hard-race';Args='-woodlandRace -racerDifficulty 2 -woodlandLaps 3'},
 @{Name='flow';Args='-woodlandFlow'}
)
if($AfterJump){$cases=@($cases | Where-Object {$_.Name -in @('normal-race','hard-race','flow')})}
if($Only){$cases=@($cases | Where-Object {$_.Name -in $Only.Split(',')})}
Get-FileHash -LiteralPath (Join-Path $projectRoot 'Temp/CorrectionPlayer7/Racer_Data/Managed/Assembly-CSharp.dll') | ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'candidate7-assembly.json')
foreach($case in $cases){
 $name=$case.Name
 $arguments="-screen-fullscreen 0 -screen-width 1280 -screen-height 720 $($case.Args) -racerTestSave Temp/correction2-$name-save -raceTrace Docs/CR041-045/$name-trace.csv -logFile Temp/correction2-$name.log"
 "Starting $name $(Get-Date -Format o)" | Tee-Object -FilePath (Join-Path $PSScriptRoot 'candidate2-runs.txt') -Append
 # A visible standalone is intentional: the user requested ordinary-frame visible play.
 $process=Start-Process -FilePath $exe -WorkingDirectory $projectRoot -ArgumentList $arguments -PassThru
 if(!$process.WaitForExit(1500000)){throw "Timed out: $name (PID $($process.Id)); inspect without killing it"}
 "Exited $name code=$($process.ExitCode) $(Get-Date -Format o)" | Tee-Object -FilePath (Join-Path $PSScriptRoot 'candidate2-runs.txt') -Append
 if($process.ExitCode -ne 0){throw "Failed process: $name"}
}

