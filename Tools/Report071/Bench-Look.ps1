# 0.71 Part C evidence: before (0.70 pipeline values, -lookOff) and after (Clear Day) players at 3840x2160.
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..')); Set-Location $root
$t="$env:LOCALAPPDATA\Temp\report071"; $o="$t\$env:BENCH_OUT"; New-Item -ItemType Directory -Force $o | Out-Null
function Bench($exe,$dir,$extra){ $a=@('-lookBench',$dir,'-racerTestSave',"$dir\save",'-screen-fullscreen','1','-screen-width','3840','-screen-height','2160','-logFile',"$dir.log")+$extra; $p=Start-Process $exe -ArgumentList $a -PassThru; if(!$p.WaitForExit(600000)){Stop-Process -Id $p.Id -Force; "TIMEOUT $dir"} }
foreach($side in @($env:BENCH_SIDES -split ",")){
  & Tools/Report071/Run-Unity.ps1 -Method ($(if($side -eq 'before'){'Report071Look.Restore'}else{'Report071Look.Setup'})) -Out "look-$side" -Minutes 15
  $env:REPORT071_OUT="$t\build-$side"; if(Test-Path $env:REPORT071_OUT){Remove-Item -Recurse -Force $env:REPORT071_OUT}
  & Tools/Report071/Run-Unity.ps1 -Method Report071Build.Test -Out "build-$side" -Minutes 40 | Select-Object -First 1
  Get-Content "$env:REPORT071_OUT\build-result.txt"
  $extra=@(); if($side -eq 'before'){$extra=@('-lookOff')}
  Bench "$env:REPORT071_OUT\Racer.exe" "$o\$side-warmup" ($extra+@('-lookFpsOnly'))
  Bench "$env:REPORT071_OUT\Racer.exe" "$o\$side" $extra
}
Get-Content Docs/Report071/look-setup.txt
Get-ChildItem "$o\before","$o\after" -Filter "bench*.txt" | ForEach-Object { Get-Content $_.FullName }
"BENCH DONE"
