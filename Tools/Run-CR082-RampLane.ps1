param([string]$Runtime='Builds/Racer-0.14.0-review2-Windows/Racer.exe',[string]$Tag='final',[string]$Course='StreetLoopReverse',[string]$Mode='race',[string]$Opposite='no')
$ErrorActionPreference='Stop'
$lane="$Course-$Mode-$Opposite"
$flags="-course $Course -rampOpposite $Opposite"
if($Mode -eq 'roam'){$flags+=' -rampRoam yes'}
& "$PSScriptRoot/Run-CR082-Check.ps1" -Runtime $Runtime -Tag $Tag -Name "ramp-$lane" -Flags "-arcadeRamp full $flags"
& "$PSScriptRoot/Run-CR082-Check.ps1" -Runtime $Runtime -Tag $Tag -Name "high-$lane" -Flags "-arcadeRamp high $flags"
if($Course -eq 'StreetLoopReverse'){
 foreach($edge in @('-10.3','7.3')){& "$PSScriptRoot/Run-CR082-Check.ps1" -Runtime $Runtime -Tag $Tag -Name "outer-$lane-$edge" -Flags "-arcadeRamp case -rampSpeed 24 -rampLine $edge $flags"}
}

