param([string]$Runtime='Builds/CR097-build-final/Racer.exe',[string]$Tag='final')
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$prior=@(Get-ChildItem "$project/Temp" -Directory -Filter 'cr091-*' | ForEach-Object {
 $folder=Join-Path $_.FullName 'CleanLapGhosts';if(Test-Path $folder){Get-ChildItem $folder -File -Filter '*.json'}
} | Where-Object Name -Match '^(street-v14-discovery|street-reverse-v5-discovery|lake-v7-discovery|forest-reverse-v5-discovery)_' | Group-Object Name | ForEach-Object {$_.Group | Sort-Object LastWriteTime -Descending | Select-Object -First 1})
if($prior.Count -ne 12){throw "Expected twelve baseline test ghost categories, got $($prior.Count)"}
$prior|ForEach-Object{[pscustomobject]@{Source=$_.FullName;Name=$_.Name;SHA256=(Get-FileHash $_.FullName).Hash}}|ConvertTo-Json|Set-Content "$project/Docs/CR097-100/preserved-ghost-inputs.json"
foreach($course in @('StreetLoopGreybox','LakeWoods','StreetLoopReverse','ForestLoopReverse')){
 $save=Join-Path $project ('Temp/cr097-preserved-ghost-'+[Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory -Path "$save/CleanLapGhosts" -Force | Out-Null
 foreach($file in $prior){Copy-Item -LiteralPath $file.FullName -Destination "$save/CleanLapGhosts/$($file.Name)"}
 '{"version":1,"master":0,"music":0.37,"radioOn":false,"radioChannel":"off:","musicSource":"bundled","frameLimit":60}'|Set-Content "$save/settings.json"
 $vehicle=if($course -in @('LakeWoods','ForestLoopReverse')){'moto'}else{'original'}
 & "$PSScriptRoot/Run-CR097-Check.ps1" -Runtime $Runtime -Tag $Tag -Name "preserved-ghost-$course" -Flags "-threeFeatureTest drive -course $course -vehicle $vehicle -laps 1 -testSpeed 6 -ghostValidation yes -lifeSeed 82089" -ReuseSave $save -Limit 600
 $rows=foreach($file in $prior){$hash=(Get-FileHash $file.FullName).Hash;$kept=@(Get-ChildItem "$save/CleanLapGhosts" -File | Where-Object Name -Like "$($file.Name)*" | Where-Object {(Get-FileHash $_.FullName).Hash -eq $hash});$(if($kept.Count){'PASS '}else{'FAIL '})+'Original compatible ghost retained as active or backup: '+$file.Name}
 $rows|Set-Content "$project/Docs/CR097-100/$Tag/preserved-ghost-$course/preservation-checks.txt"
}
