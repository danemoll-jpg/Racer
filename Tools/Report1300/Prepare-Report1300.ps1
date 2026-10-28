$ErrorActionPreference='Stop'
Set-Location -LiteralPath (Join-Path $PSScriptRoot '../..')
$python=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
$built=(Get-Content 'Builds/Racer-1.3.0-review5-Windows/VERSION.txt' | Select-String 'Source commit: ').Line.Split(': ')[-1].Trim()
$runtime=Join-Path (Get-Location) 'Builds/Racer-1.3.0-review5-Windows'
$prior=Join-Path (Get-Location) 'Builds/Latest/versions/1030300'
foreach($file in @('WoodstockRush-Cover.png','RADIO.md')){Copy-Item -LiteralPath (Join-Path $prior $file) -Destination (Join-Path $runtime $file)}
# as 1.0: the player-facing README does not list the debug keys, and Review/ carries only TRAILER_MODE.md (the debug tool stays in the game, hidden)
@'
Woodstock Rush 1.3.0-review5
FREE ROAM WORLD: big ramps across the map (huge tables, gap jumps, kickers, two over houses and one through a barn), trees that break
when hit hard enough, smash spots (hay bales, crates, cones, boxes), flip ramps and a turnaround where roads end at the world's edge.
All on the Free Roam map and minimap, in both modes (in HIGH MODE trees break at a much lower speed).
HIGH MODE races: the bumpers stand wider apart on every jump's landing. Fence Line Smash: every fence and prop you smash counts.
review3: every Free Roam ramp entrance is flush (you roll straight up them), bigger ramp numbers on the map and minimap.
Stunt chapter: it opens at the stunt park (Red Clay Session, then the Park Competition against five riders); Mountain Loop Stunt Races have their shortcuts closed.
review4: the campaign map shows the stunt park for the park events, and the spot (with the way in) for speed traps, jumps, smashes and trick events.
review5: acorns can be collected in every Free Roam (split screen and HIGH MODE too); Land It Clean now comes before Summit Tricks; stunt landings are judged against the ground, and a quick recovery counts (SAVED IT!); Leviathan Reverse says TURN BACK at once after a wrong turn.
HIGH MODE: the switch at the top of the main menu. FREE ROAM on the main menu asks 1 or 2 players.
Race replays: every race is recorded. On the finish screen, WATCH REPLAY (X / V) and SAVE REPLAY (Y / K).
Settings > Display > Screen mode: Full screen (the default) or Windowed.
Play-Racer.cmd uses the existing signed launcher/updater.
'@ | Set-Content (Join-Path $runtime 'README.txt')
$review=Join-Path $runtime 'Review'
New-Item -ItemType Directory -Force -Path $review | Out-Null
Copy-Item -LiteralPath 'Docs/TrailerMode/TRAILER_MODE.md' -Destination $review
New-Item -ItemType Directory -Force -Path 'Temp' | Out-Null
Set-Content -LiteralPath 'Temp/Report1300-source-commit.txt' -Value $built
& $python Tools/Prepare-ComponentUpdate.py game --source Builds/Racer-1.3.0-review5-Windows --previous-catalog Builds/LauncherRelease-1030300/assets/update-catalog.json --previous-manifest Builds/LauncherRelease-1030300/assets/game-manifest.json --out Builds/LauncherRelease-1030400 --build 1030400 --version 1.3.0-review5 --notes "Woodstock Rush 1.3.0-review5: the Free Roam world (big ramps, breakable trees, smash spots, flip ramps and a turnaround where roads end); review2: HIGH MODE trees break much more easily, HIGH MODE race bumpers wider apart on jump landings, Fence Line Smash counts every fence and prop you smash; review3: every Free Roam ramp entrance flush, bigger ramp numbers on the map and minimap, the Stunt chapter opens at the stunt park with a new Park Competition, Mountain Loop Stunt Races with their shortcuts closed; review4: the campaign map shows the stunt park for the park events and the spot for one-place events. Source commit $built."
if($LASTEXITCODE -ne 0){throw 'Publisher preparation failed'}
