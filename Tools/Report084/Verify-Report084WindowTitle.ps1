# 0.84 Part A: the running game's window title (settings temporarily muted, original bytes restored).
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$exe=Join-Path $root 'Builds/Latest/versions/84000/Racer.exe'
$settings=Join-Path $env:USERPROFILE 'AppData/LocalLow/DefaultCompany/Racer/Phase7/street-loop-gates-v1-laps3/settings.json'
if(Get-Process Racer -ErrorAction SilentlyContinue){throw 'Close the game first'}
$before=[IO.File]::ReadAllBytes($settings);$hash=(Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash
try{
 $o=[Text.Encoding]::UTF8.GetString($before)|ConvertFrom-Json;$o.master=0;[IO.File]::WriteAllText($settings,($o|ConvertTo-Json -Depth 30))
 $p=Start-Process -FilePath $exe -PassThru;$titles=@()
 for($i=0;$i -lt 12;$i++){Start-Sleep -Seconds 2;$p.Refresh();$titles+=$p.MainWindowTitle}
 "titles seen: "+(($titles|Select-Object -Unique) -join ' | ')
 if(!$p.HasExited){$null=$p.CloseMainWindow();if(!$p.WaitForExit(8000)){$p.Kill()}}
}finally{
 [IO.File]::WriteAllBytes($settings,$before)
 if((Get-FileHash -LiteralPath $settings -Algorithm SHA256).Hash -ne $hash){throw 'Settings not restored'}
}
