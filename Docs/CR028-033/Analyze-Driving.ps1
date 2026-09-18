param([string]$Label='baseline')
$rows=Import-Csv (Join-Path $PSScriptRoot "$Label-driving.csv")
$samples=$rows | Where-Object { $_.contact -eq '' }
$samples | Group-Object profile,scenario | ForEach-Object {
 $g=$_.Group
 [pscustomobject]@{Scenario=$_.Name;Peak=($g | ForEach-Object {[double]$_.speed} | Measure-Object -Maximum).Maximum;LargestFrameLoss=($g | ForEach-Object {[double]$_.deltaSpeed} | Measure-Object -Minimum).Minimum;WipeoutSamples=($g | Where-Object wipeout -eq 'True').Count;BrakeSamples=($g | Where-Object {[double]$_.brake -gt 0}).Count}
} | Format-Table -AutoSize
$samples | Where-Object {[double]$_.deltaSpeed -lt -3} | Sort-Object {[double]$_.deltaSpeed} | Select-Object -First 16 profile,scenario,time,speed,throttle,brake,grounded,wipeout,x,z,deltaSpeed | Format-Table -AutoSize
