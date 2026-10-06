# 0.84 Part D: five cold launches of the fresh build to the title (settings temporarily muted, original bytes restored);
# each Player.log's "Title voice:" line (how the phrase played) is collected.
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$exe=Join-Path $root 'Builds/Racer-0.84.0-review1-Windows/Racer.exe'
$saveRoot=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer'
$settings=Join-Path $saveRoot 'Phase7/street-loop-gates-v1-laps3/settings.json'
$log=Join-Path $saveRoot 'Player.log'
if(Get-Process Racer -ErrorAction SilentlyContinue){throw 'Close the game first'}
$before=[IO.File]::ReadAllBytes($settings);$hash=(Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
$results=@()
try{
 $o=[Text.Encoding]::UTF8.GetString($before)|ConvertFrom-Json;$o.master=0;[IO.File]::WriteAllText($settings,($o|ConvertTo-Json -Depth 30))
 for($i=1;$i -le 5;$i++){
  $p=Start-Process -FilePath $exe -PassThru
  $deadline=(Get-Date).AddSeconds(90);$line=$null
  do{Start-Sleep -Seconds 2;if(Test-Path $log){$line=Select-String -LiteralPath $log -Pattern 'Title voice:' -SimpleMatch | Select-Object -Last 1}}while(!$line -and (Get-Date) -lt $deadline -and !$p.HasExited)
  $results+=[ordered]@{launch=$i;line=if($line){$line.Line}else{'no Title voice line within 90 s'}}
  if(!$p.HasExited){$null=$p.CloseMainWindow();if(!$p.WaitForExit(8000)){$p.Kill()}}
  Start-Sleep -Seconds 2
 }
}finally{
 [IO.File]::WriteAllBytes($settings,$before)
 if((Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash -ne $hash){throw 'Settings not restored'}
}
$results|ConvertTo-Json|Set-Content (Join-Path $root 'Docs/Report084/title-cold-launches.json')
$results|ForEach-Object{"$($_.launch): $($_.line)"}
