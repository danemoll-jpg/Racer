param([string]$Tag='release2')
$ErrorActionPreference='Stop'
$root=Join-Path ([IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))) "Docs/CR082-089/$Tag"
$lines=[Collections.Generic.List[string]]::new()
$lines.Add("# Compiled-player results: $Tag")
$lines.Add('')
$lines.Add('Automated evidence only. Concurrent hidden players are not performance benchmarks, human driving, physical-controller testing or listening. A successful process exit does not override failed assertions or ramp outcomes.')
$lines.Add('')
$lines.Add('| Suite | Passed / total | Completed |')
$lines.Add('| --- | ---: | --- |')
foreach($file in Get-ChildItem -LiteralPath $root -Recurse -Filter checks.txt | Sort-Object FullName){
 $checks=@(Get-Content -LiteralPath $file.FullName);$passed=@($checks|Where-Object {$_ -like 'PASS *'}).Count
 $done=Test-Path -LiteralPath (Join-Path $file.Directory.FullName 'done.txt')
 $lines.Add("| $($file.Directory.Name) | $passed / $($checks.Count) | $done |")
}
$lines.Add('');$lines.Add('## Ramp matrix');$lines.Add('')
$lines.Add('The probe uses canonical m/s. 12/24/36/43 m/s = 26.8/53.7/80.5/96.2 mph. Completed does not guarantee a clean traversal: upright below 0.65 is separately counted. Raw traces retain inputs, contact normals/separation/impulse, suspension, velocity components and stability.');$lines.Add('')
$lines.Add('| Matrix | Runs | Non-completed/unstable final | Upright < 0.65 | Recovery failures | Completed |')
$lines.Add('| --- | ---: | ---: | ---: | ---: | --- |')
$all=@();$failed=@()
foreach($file in Get-ChildItem -LiteralPath $root -Recurse -Filter ramps.csv | Sort-Object FullName){
 $rows=@(Import-Csv -LiteralPath $file.FullName)
 foreach($r in $rows){$r|Add-Member -NotePropertyName Matrix -NotePropertyValue $file.Directory.Name;$all+=$r;if($r.outcome -ne 'completed' -or [double]$r.minUp -lt .65 -or $r.recovered -ne 'True'){$failed+=$r}}
 $bad=@($rows|Where-Object {$_.outcome -ne 'completed'}).Count;$low=@($rows|Where-Object {[double]$_.minUp -lt .65}).Count;$recover=@($rows|Where-Object {$_.recovered -ne 'True'}).Count
 $done=Test-Path -LiteralPath (Join-Path $file.Directory.FullName 'done.txt')
 $lines.Add("| $($file.Directory.Name) | $($rows.Count) | $bad | $low | $recover | $done |")
}
$all|Export-Csv -LiteralPath "$root/ramp-summary.csv" -NoTypeInformation
$failed|Export-Csv -LiteralPath "$root/ramp-retained-failures.csv" -NoTypeInformation
$lines.Add('');$lines.Add("Total recorded ramp traversals: $($all.Count). Explicit non-clean cases retained: $($failed.Count).")
$lines.Add('');$lines.Add('## Failed assertions retained');$lines.Add('')
foreach($file in Get-ChildItem -LiteralPath $root -Recurse -Filter checks.txt | Sort-Object FullName){foreach($line in Get-Content -LiteralPath $file.FullName | Where-Object {$_ -like 'FAIL *'}){$lines.Add("- $($file.Directory.Name): $line")}}
$lines|Set-Content -LiteralPath "$root/RESULTS.md"
$lines
