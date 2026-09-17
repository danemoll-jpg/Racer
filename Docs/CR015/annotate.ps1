param([string]$Label)
$dir=Join-Path $PSScriptRoot '.'
$data=Get-Content (Join-Path $dir "$Label-labels.json") -Raw | ConvertFrom-Json
$png=[Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $dir "$Label-overview.png")))
$parts=[Collections.Generic.List[string]]::new()
$parts.Add('<svg xmlns="http://www.w3.org/2000/svg" width="1440" height="1080" viewBox="0 0 1440 1080"><rect width="1440" height="1080" fill="#14252b"/>')
$parts.Add("<image href='data:image/png;base64,$png' y='80' width='1440' height='1000'/>")
$title=if($Label -eq 'before'){'BEFORE — saved woodland baseline'}else{'AFTER — CR-015 + modest roadside details'}
$parts.Add("<text x='32' y='47' fill='white' font-family='sans-serif' font-size='30'>$title</text><text x='1050' y='47' fill='#bce5da' font-family='sans-serif' font-size='18'>North ↑ · interpretive placement</text>")
$labels=@("Dan's house",'House #2','Friend — fixed reference','House #3 — valley')
if($Label -eq 'after'){
 $sites=(Get-Content (Join-Path $dir 'after-sites.json') -Raw | ConvertFrom-Json).sites
 foreach($i in @(1,3)){
  $m=$data.marks[$i -eq 3 ? 3 : 1];$r=$sites[$i].road
  $rx=720+($r.x-442)*1000/450;$ry=580-($r.z+49)*1000/450
  $parts.Add("<line x1='$($m.x)' y1='$($m.y+80)' x2='$rx' y2='$ry' stroke='#f7df88' stroke-width='3' stroke-dasharray='7 6'/><circle cx='$rx' cy='$ry' r='7' fill='#f7df88'/>")
 }
}
for($i=0;$i -lt 4;$i++){
 $m=$data.marks[$i];$x=[math]::Round($m.x);$y=[math]::Round($m.y+80)
 $tx=if($i -eq 2){1010}else{260};$ty=$y-20
 $text=$labels[$i];$w=if($i -eq 2){345}else{240}
 $parts.Add("<line x1='$x' y1='$y' x2='$tx' y2='$ty' stroke='#fff3a5' stroke-width='3'/><circle cx='$x' cy='$y' r='12' fill='none' stroke='#fff3a5' stroke-width='4'/><rect x='$($tx-12)' y='$($ty-27)' width='$w' height='40' rx='7' fill='#14252b' fill-opacity='.94'/><text x='$tx' y='$ty' fill='white' font-family='sans-serif' font-size='22'>$text</text>")
}
$x=[math]::Round($data.drop.x);$y=[math]::Round($data.drop.y+80)
$parts.Add("<line x1='$x' y1='$y' x2='1060' y2='795' stroke='#ffb17f' stroke-width='4'/><circle cx='$x' cy='$y' r='20' fill='none' stroke='#ffb17f' stroke-width='4'/><rect x='1040' y='760' width='350' height='70' rx='7' fill='#14252b'/><text x='1060' y='788' fill='#ffcfb0' font-family='sans-serif' font-size='22'>Big drop — upper descent</text><text x='1060' y='815' fill='white' font-family='sans-serif' font-size='16'>Approach from main road: north → south</text></svg>")
[IO.File]::WriteAllText((Join-Path $dir "$Label-annotated.svg"),($parts -join "`n"))
