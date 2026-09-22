$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$expected=Join-Path $root 'Builds/Latest/Racer.exe'
$version=Get-Content (Join-Path $root 'Builds/Latest/VERSION.txt') -Raw
if(!$version.Contains('Racer 0.26.0-review1')){throw 'Latest version mismatch'}
$before=@(Get-Process Racer -ErrorAction SilentlyContinue | ForEach-Object Id)
& (Join-Path $root 'Play-Racer.cmd')
$deadline=(Get-Date).AddSeconds(45)
$launched=$null
do {
    Start-Sleep -Seconds 2
    $found=@(Get-Process Racer -ErrorAction SilentlyContinue | Where-Object {$_.Id -notin $before -and $_.Path -eq $expected})
    if($found.Count -gt 1){throw 'Ambiguous new game processes; none closed'}
    if($found.Count -eq 1){$launched=$found[0];$launched.Refresh()}
} while((!$launched -or !$launched.MainWindowHandle -or !$launched.Responding) -and (Get-Date) -lt $deadline)
if(!$launched -or !$launched.MainWindowHandle -or !$launched.Responding){throw 'New release did not present a responsive game window'}
$record=[ordered]@{entryPoint=(Join-Path $root 'Play-Racer.cmd');path=$launched.Path;version='0.26.0-review1';pid=$launched.Id;responsive=$launched.Responding;window=$launched.MainWindowTitle;verifiedAt=(Get-Date -Format o)}
$record | ConvertTo-Json | Set-Content (Join-Path $root 'Docs/SurgicalTracks/play-racer-launch.json')
$null=$launched.CloseMainWindow()
if(!$launched.WaitForExit(5000)){
    # The window can finish closing between the wait and the fallback lookup.
    $remaining=Get-Process -Id $launched.Id -ErrorAction SilentlyContinue
    if($remaining -and $remaining.Path -eq $expected){
        try {$remaining.Kill()} catch [InvalidOperationException] {if(!$remaining.HasExited){throw}}
    }
}
$record | ConvertTo-Json
