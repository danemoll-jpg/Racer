param([string]$Tag='release3')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$root=Join-Path $project "Docs/CR081-090/$Tag"
$checks=@(Get-ChildItem $root -Recurse -File -Filter '*checks.txt'|ForEach-Object {
 $lines=Get-Content -LiteralPath $_.FullName
 [pscustomobject]@{Test=$_.Directory.Name;File=$_.Name;Pass=@($lines -match '^PASS').Count;Fail=@($lines -match '^FAIL').Count;Failures=@($lines -match '^FAIL')}
})
$processes=@(Get-ChildItem $root -Recurse -File -Filter process.json|ForEach-Object {Get-Content -LiteralPath $_.FullName -Raw|ConvertFrom-Json})
$ramps=@(Get-ChildItem $root -Recurse -File -Filter ramps.csv|ForEach-Object {
 $file=$_;$rows=@(Import-Csv -LiteralPath $file.FullName)
 $mountain=$rows.Count -and $null -ne $rows[0].completed
 $bad=@($rows|Where-Object {if($mountain){$_.completed -ne 'True' -or [double]$_.minUp -lt .65}else{$_.outcome -ne 'completed' -or [double]$_.minUp -lt .65}})
 [pscustomobject]@{Test=$file.Directory.Name;Rows=$rows.Count;NonClean=$bad.Count;FailedRecovery=@($rows|Where-Object recovered -ne True).Count;Failures=$bad}
})
[ordered]@{GeneratedUtc=[DateTime]::UtcNow.ToString('o');Tag=$Tag;CheckFiles=$checks;Processes=$processes;RampMatrices=$ramps}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath "$root/summary.json"
$checks|Select-Object Test,File,Pass,Fail|Format-Table -AutoSize
$ramps|Select-Object Test,Rows,NonClean,FailedRecovery|Format-Table -AutoSize
Write-Output "Completed processes=$($processes.Count); timeouts=$(@($processes|Where-Object Timeout).Count)"
