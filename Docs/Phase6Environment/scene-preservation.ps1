$before=(git show '442d7af:Assets/Scenes/StreetLoopGreybox.unity') -join "`n"
$after=(Get-Content Assets/Scenes/StreetLoopGreybox.unity -Raw).Replace("`r`n","`n").TrimEnd()
function Records($s){$map=@{};foreach($m in [regex]::Matches($s,'(?ms)^--- !u!\d+ &(-?\d+).*?(?=^--- !u!|\z)')){$map[$m.Groups[1].Value]=$m.Value.TrimEnd()};return $map}
$b=Records $before;$a=Records $after;$rows=@();$changed=0;$removed=0
foreach($id in $b.Keys){if(!$a.ContainsKey($id)){$removed++;continue};if($b[$id] -ceq $a[$id]){continue};$changed++;$body=$b[$id];$name=[regex]::Match($body,'(?m)^  m_Name: (.*)').Groups[1].Value;if(!$name){$go=[regex]::Match($body,'m_GameObject: \{fileID: (\d+)\}').Groups[1].Value;if($go -and $b.ContainsKey($go)){$name=[regex]::Match($b[$go],'(?m)^  m_Name: (.*)').Groups[1].Value}};$rows+="$name | $id | $($body.Split("`n")[1])"}
$report=@("Baseline 442d7af; retained scene records changed: $changed; removed: $removed",'Changed retained records grouped by object name:')
$report+=($rows|Group-Object {($_ -split ' \| ')[0]}|Sort-Object Name|ForEach-Object {"$($_.Name): $($_.Count)"})
$report+='Detailed retained changes:';$report+=$rows|Sort-Object
$report | Set-Content Docs/Phase6Environment/scene-preservation.txt
$report | Select-Object -First 22




