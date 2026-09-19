$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$path=Join-Path $root 'PROJECT_TODO.md'
$text=Get-Content -LiteralPath $path -Raw
$status="IMPLEMENTED IN 0.6.1-review2 — integrated validation recorded; awaiting Dan's approval. See Docs/CR041-045/VALIDATION.md and AI-RACES.md for actual results and limitations."
$outcomes=@{
 'BUG-004'='80/80 ordinary-frame route/road attempts, 48 verified branch exits and16 local recoveries: zero misses, charges or buzzes. Swept guards cover reversal, abandonment, entrance-touch exploits and shared AI rules. Exact old user incident was not reproduced; source recognition/context defects were corrected.'
 'BUG-005'='111/111 rule/dynamic checks and8/8 actual jump flights passed intended crossing credit. Direction uses movement; only CP14 has the24m upper envelope. A fast ATV genuinely missed later CP15 (+5), retained in evidence.'
 'BUG-006'='Five materials physically hit at default/lowered/muted levels;15 DSP mix captures, no clipping, muted peak/RMS exactly zero. Gains/attenuation/voice handling improved. No OS-endpoint or subjective listening claim; Dan must review actual audible prominence.'
 'BUG-007'='Only the identified floating Fox Gully residence was adapted into the supported glass-entry/ramp/upper-window stunt. All12 final gully branch attempts crossed both panes. All46 unrelated building transforms unchanged.'
 'CR-036'='Normal/Hard driver pace strengthened with unchanged player motors/capabilities and Easy constants. Both20-traffic three-lap races finished4/4 with zero misses/recoveries. All profiles improved best laps, but Hard tourer/motorcycle total races slowed in congestion. Exact laps, sectors, gaps, braking and Easy/mistake results are in Docs/CR041-045/AI-RACES.md; no human difficulty acceptance claimed.'
 'CR-041'='44m supported interior rise,20m clear openings, sliding entry glass and upper exit glass; no launch boost/teleport. Initial narrow-aperture and embedded-crossbeam failures retained. Final all-profile normal/varied/interior-recovery attempts passed with both panes broken and zero charges/buzzes.'
 'CR-042'='Each genuine miss exactly+5s; authorized bypass zero; no distance surcharge. street-v6-flat5 record category preserves historical files/settings. Broad cuts may save more than their flat missed-gate cost; ordered progress, bounded movement and finish checks remain.'
 'CR-043'='169 breakable perimeter sections:48 chain-link at Dan,121 white-X at Houses2/3. Front/side/rear runs,12m front access and6m woodland openings; front setback11m from road centre. Other house positions unchanged. Repeated physical material hits and restoration checks passed.'
 'CR-044'='16 dedicated highway plus4 local cars. Full Normal/Hard traces averaged11.49/11.68 highway cars and21.91/22.30 passes/minute; all four lanes represented.294 recycles, minimum player distance220m, zero traffic recoveries. Safe recycling permits temporary density dips. Measured core same-lane minimum gaps26.45/24.52m; detailed limits in AI-RACES.md.'
 'CR-045'='Jamerson raised-pavement roadworks with supported takeoff, graded shoulder, breakable markers and open-right bypass.8/8 flights landed and credited CP14; fast motorcycle51.224m/s,17.430m apex,3.263s airtime. Fast ATV later CP15 miss retained as genuine+5.'
}
foreach($id in $outcomes.Keys){
 $pattern='(?ms)^### '+[regex]::Escape($id)+'[^\r\n]*\r?\n.*?(?=^### |^---|\z)'
 $result=$outcomes[$id]
 $text=[regex]::Replace($text,$pattern,[Text.RegularExpressions.MatchEvaluator]{param($match)
  $section=[regex]::Replace($match.Value,'(?m)^\*\*Status:\*\*.*$',"**Status:** $status")
  if($section -match '(?m)^\*\*Result:\*\*'){$section=[regex]::Replace($section,'(?m)^\*\*Result:\*\*.*$',"**Result:** $result")}
  else{$section=$section.TrimEnd()+"`r`n**Result:** $result`r`n`r`n"}
  return $section
 })
}
$text=$text.Replace('Current woodland review is 0.6.0-review1','Current integrated correction is 0.6.1-review2')
$text=$text.Replace('REVISION REQUIRED — latest ten-item playtest feedback supersedes previous technical passes; see BUG-004–007 and CR-041–045.',"0.6.1 CORRECTION DELIVERED — awaiting Dan's playtest; see BUG-004–007 and CR-041–045. Earlier technical passes remain historical evidence.")
$performance=@(Get-Content "$PSScriptRoot/performance-comparison.json" -Raw|ConvertFrom-Json)
if($performance.Count -ne 2){throw 'Both performance periods must be complete before final handoff'}
$before=$performance|Where-Object Period -eq 'before';$after=$performance|Where-Object Period -eq 'after'
$handoff=@"
# SESSION HANDOFF

**Current delivery:** 0.6.1-review2 integrated correction, awaiting Dan's approval. BUG-004–007, CR-041–045 and further CR-036 are implemented. No separate lake circuit, multiplayer, legacy environment rebuild or unrelated expansion.

Safety checkpoint: 88064bd196d68e40bfb624f2d5879613d962dbf5, verified before changes. Git permission failures were safely retried through supported elevation. No changes discarded, hooks/signing bypassed or history rewritten. Executable source:566b4180fa633d182deb7559ae31c025f36ab52f. The completion commit is reported in the delivery response; it contains final evidence/documentation for this source.

Open Play-Racer.cmd or Builds/Latest/Racer.exe. Full Windows package:Builds/Racer-0.6.1-review2-Windows.zip. All231 staged/extracted/Latest files hash-verified; launcher visibly opened the correct Latest on Dan's desktop. VERSION metadata matches executable source. Saved vehicle/colors/volumes remained unchanged; old builds and records preserved. Nothing uploaded/distributed.

Race credit uses bounded swept movement, persistent earned shortcut context, sustained partial-rejoin handling and movement direction rather than body heading. CP14 alone has the24m upper jump envelope. Each genuine miss is exactly+5 seconds; authorized bypass zero. street-v6-flat5 records preserve historical categories. Broad cuts can save more than their flat charge; there is no hidden replacement punishment. Original0.6.0 build/source identity was verified, but its limited existing harness did not reproduce Dan's exact false-penalty/silent-impact incident.

Final user-desktop validation:80/80 ordinary-frame route/road attempts across all four routes/profiles,48 branch exits,16 local recoveries, zero misses/charges/buzzes. All12 gully attempts broke both panes.111/111 rule/dynamic checks plus77/77 route invariants. All8 Jamerson flights landed/credited CP14; fast ATV's later CP15 miss was genuine+5 and remains disclosed.137/137 menu/recovery/settings/restoration/results flow checks passed. Reversal/abandonment combinations use swept fixtures in addition to ordinary-frame recovery; no exhaustive human-driving/controller claim.

The exact gully residence is now a supported44m/6m-rise drive-through house. Narrow-doorway and overlapping-crossbeam failures were retained before the final20m opening/continuous ramp fix.169 perimeter fence sections surround the three properties; all46 other building transforms and player profile/motor tuning remain unchanged. Jamerson uses supported pavement roadworks with a bypass. Before/after views are linked in Docs/CR041-045/VALIDATION.md.

Audio:15 physical authored-prop DSP mix captures with engine throttle at default/lowered/muted settings; default/lowered nonzero, mute exactly zero, no clipping. Attenuation, gains and bounded voice handling improved; oriented restoration fixes wide diagonal glass. No OS-endpoint capture or subjective listening performed. Dan's speaker-level impact assessment remains required.

Traffic:16 dedicated Hwy92 +4 local cars, about22 passes/minute, mean11.5–11.7 cars observed on highway, temporary2–17 occupancy due safe recycling.294 measured recycles at least220m from player; no traffic recoveries. Normal/Hard full races both finished4/4 with zero misses/recoveries. Best laps improved for every profile; Hard tourer/motorcycle total times regressed in congestion. Easy constants unchanged. Actual lap/sector/gap/mistake results and limits:Docs/CR041-045/AI-RACES.md. Automated pilots do not establish human difficulty.

Comparable foreground performance:one visible standalone,1280x720, VSync off,120fps cap, one Normal lap, same mixed roster. Old4-traffic median/p95=$($before.MedianMs)/$($before.P95Ms)ms, peak working set$($before.PeakWorkingSetMiB)MiB. New20-traffic median/p95=$($after.MedianMs)/$($after.P95Ms)ms, peak$($after.PeakWorkingSetMiB)MiB. This is a capped local comparison, not a worst-case FPS guarantee. Build:zero errors; existing future collision-prebake warning and intentionally absent optional Runtime Pipeline warning retained.

Dan's ten-item checklist is in Docs/CR041-045/VALIDATION.md:shortcut legality; airborne/backwards credit; flat penalties; Normal/Hard/Easy feel; gully stunt/recovery; property perimeters/access; audible impacts/mute; busy four-lane traffic; environmental Jamerson jump/bypass; preserved garage/menu/recovery/results/records. CR-040 remains backlog. Stop expansion and await combined approval.

---
"@
$text=[regex]::Replace($text,'(?ms)^# SESSION HANDOFF\r?\n.*?(?=^# How Dan and ChatGPT Will Use This File)',$handoff+"`r`n")
[IO.File]::WriteAllText($path,$text)
'PROJECT_TODO.md and SESSION HANDOFF updated from completed evidence.'
