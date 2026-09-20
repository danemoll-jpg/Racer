param([string]$Runtime='Builds/CR087-baseline-instrumented/Racer.exe')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$exe=[IO.Path]::GetFullPath((Join-Path $project $Runtime))
if(!(Test-Path -LiteralPath $exe)){throw "Diagnostic runtime missing: $exe"}
$runs=@()
foreach($course in @('StreetLoopGreybox','StreetLoopReverse')) {
 foreach($opposite in @('no','yes')) {
  $name="$course-roam-opposite-$opposite"
  $evidence=Join-Path $project "Docs/CR082-089/baseline-instrumented/$name"
  New-Item -ItemType Directory -Force -Path $evidence | Out-Null
  $testSave=Join-Path $project ('Temp/cr087-'+[Guid]::NewGuid().ToString('N'))
  $flags="-batchmode -screen-width 960 -screen-height 540 -racerTestSave `"$testSave`" -evidence `"$evidence`" -logFile `"$project/Logs/cr087-$name.log`" -arcadeRamp full -course $course -rampRoam yes -rampOpposite $opposite"
  $p=Start-Process -FilePath $exe -ArgumentList $flags -WorkingDirectory $project -WindowStyle Hidden -PassThru
  $runs+=[pscustomobject]@{Name=$name;PID=$p.Id;Executable=$exe;Flags=$flags;Evidence=$evidence;StartedUtc=[DateTime]::UtcNow.ToString('o')}
 }
}
$runs|ConvertTo-Json -Depth 4|Set-Content (Join-Path $project 'Docs/CR082-089/baseline-instrumented/processes.json')
$runs|Select-Object Name,PID
