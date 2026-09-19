param([string]$Version='0.9.0-review1',[int[]]$Difficulties=@(0,2))
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=Join-Path $project "Builds/Racer-$Version-Windows/Racer.exe"
foreach($difficulty in $Difficulties){
 $runs=@()
 foreach($course in @('LakeWoods','StreetLoopGreybox')){foreach($baseline in @($false,$true)){
  $tag="$course-d$difficulty-$baseline"
  $extra=if($baseline){'-errorsDisabled'}else{''}
  $arguments="-batchmode -nographics -livingTest race -course $course -racerDifficulty $difficulty -timeScale 12 -racerTestSave Temp/matrix-$tag-save -evidence Docs/CR057-060/final-$tag -logFile Logs/matrix-$tag.log $extra"
  $process=Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory $project -WindowStyle Hidden -PassThru
  $runs+=@{Tag=$tag;Process=$process}
 }
 }
 foreach($run in $runs){$run.Process.WaitForExit();if($run.Process.ExitCode -ne 0){throw "Diagnostic failed: $($run.Tag)"};Select-String -LiteralPath "$project/Logs/matrix-$($run.Tag).log" -Pattern '^AI_JUDGMENT ' | ForEach-Object Line | Set-Content "$project/Docs/CR057-060/final-$($run.Tag)/events.txt"}
}
'Complete' | Set-Content "$project/Docs/CR057-060/matrix-done.txt"
