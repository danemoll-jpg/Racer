# Test voice for the police radio: one offline Windows SAPI clip per ID in Tools/Audio/police-radio-lines.csv, written to
# SourceArt/Audio/PoliceRadio/_placeholder/<id>.wav. The game plays a placeholder only when Dan's own file for that ID is missing.
# Usage (from the Racer folder):  powershell -File Tools/Audio/Make-PoliceRadioPlaceholders.ps1 [-Only R01,R02]
param([string[]]$Only=@())
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Speech
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$out=Join-Path $root 'SourceArt/Audio/PoliceRadio/_placeholder'; New-Item -ItemType Directory -Force $out | Out-Null
$synth=New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.Rate=1
$fmt=New-Object System.Speech.AudioFormat.SpeechAudioFormatInfo(22050,[System.Speech.AudioFormat.AudioBitsPerSample]::Sixteen,[System.Speech.AudioFormat.AudioChannel]::Mono)
$made=0
foreach($row in (Import-Csv (Join-Path $PSScriptRoot 'police-radio-lines.csv'))){
  if($Only.Count -gt 0 -and $Only -notcontains $row.id){continue}
  $text=($row.text -replace '\.\.\.','' -replace '…','').Trim()
  $path=Join-Path $out ($row.id+'.wav')
  $synth.SetOutputToWaveFile($path,$fmt); $synth.Speak($text); $synth.SetOutputToNull(); $made++
}
$synth.Dispose()
"made $made placeholder clips in $out (voice: $((New-Object System.Speech.Synthesis.SpeechSynthesizer).Voice.Name))"
