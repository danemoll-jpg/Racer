param([string]$Tag='verified')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot "../Docs/CR091-096/$Tag"))
$checks=@(Get-ChildItem -LiteralPath $root -Recurse -Filter checks.txt | ForEach-Object {
 $file=$_; $lines=@(Get-Content -LiteralPath $file.FullName)
 [pscustomobject]@{Test=$file.Directory.Name;Pass=@($lines | Where-Object {$_ -like 'PASS *'}).Count;Fail=@($lines | Where-Object {$_ -like 'FAIL *'}).Count;Failures=@($lines | Where-Object {$_ -like 'FAIL *'})}
})
$ramps=@(Get-ChildItem -LiteralPath $root -Recurse -Filter ramps.csv | ForEach-Object {
 $file=$_;$rows=@(Import-Csv -LiteralPath $file.FullName)
 [pscustomobject]@{Test=$file.Directory.Name;Cases=$rows.Count;Incomplete=@($rows|Where-Object {if($_.PSObject.Properties.Name -contains 'completed'){$_.completed -ne 'True'}else{$_.outcome -ne 'completed'}}).Count;LowUpright=@($rows|Where-Object {[double]$_.minUp -lt .65}).Count;NoAward=@($rows|Where-Object {[int]$_.awards -eq 0}).Count;RecoveryFailures=@($rows|Where-Object recovered -ne True).Count}
})
$processes=@(Get-ChildItem -LiteralPath $root -Recurse -Filter process.json | ForEach-Object {Get-Content -LiteralPath $_.FullName -Raw|ConvertFrom-Json})
$report=[ordered]@{Tag=$Tag;GeneratedUtc=[DateTime]::UtcNow.ToString('o');Checks=$checks;Ramps=$ramps;Processes=$processes.Count;Timeouts=@($processes|Where-Object Timeout).Count;NonzeroExits=@($processes|Where-Object Exit -ne 0|Select-Object Name,Exit)}
$report|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $root 'summary.json')
$checks|Format-Table Test,Pass,Fail
$ramps|Format-Table
