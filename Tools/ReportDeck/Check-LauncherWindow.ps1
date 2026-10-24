# Steam Deck launcher round: run the real launcher window (--manage, off screen, never activated), choose "Check for updates"
# by posted keys, and capture the window to a PNG. Closes the launcher afterwards. Does not start the game.
param([Parameter(Mandatory)][string]$Root,[Parameter(Mandatory)][string]$Out,[int]$WaitSeconds=20)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;using System.Runtime.InteropServices;
public static class W{
 [StructLayout(LayoutKind.Sequential)] public struct RECT{public int L,T,R,B;}
 [StructLayout(LayoutKind.Sequential)] public struct POINT{public int X,Y;}
 [StructLayout(LayoutKind.Sequential)] public struct PLACEMENT{public int length,flags,showCmd;public POINT min,max;public RECT normal;}
 [DllImport("user32.dll")] public static extern bool GetWindowPlacement(IntPtr h,ref PLACEMENT p);
 [DllImport("user32.dll")] public static extern bool SetWindowPlacement(IntPtr h,ref PLACEMENT p);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr h,uint m,IntPtr w,IntPtr l);
 [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h,IntPtr dc,uint flags);
 [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h,out RECT r);
}
'@
$exe=Join-Path $Root 'WoodstockRushLauncher.exe'
$p=Start-Process -FilePath $exe -ArgumentList '--manage' -WorkingDirectory $Root -WindowStyle Minimized -PassThru
$deadline=(Get-Date).AddSeconds(20)
do{Start-Sleep -Milliseconds 300;$p.Refresh()}while($p.MainWindowHandle -eq 0 -and (Get-Date) -lt $deadline)
$h=$p.MainWindowHandle;if($h -eq 0){throw 'Launcher window did not open'}
$pl=New-Object W+PLACEMENT;$pl.length=[Runtime.InteropServices.Marshal]::SizeOf($pl);[void][W]::GetWindowPlacement($h,[ref]$pl)
$w=$pl.normal.R-$pl.normal.L;$t=$pl.normal.B-$pl.normal.T
$pl.normal.L=-6000;$pl.normal.T=-6000;$pl.normal.R=-6000+$w;$pl.normal.B=-6000+$t;$pl.showCmd=4 # SW_SHOWNOACTIVATE, off screen
[void][W]::SetWindowPlacement($h,[ref]$pl)
Start-Sleep -Milliseconds 500
foreach($k in 0x28,0x0D){[void][W]::PostMessage($h,0x0100,[IntPtr]$k,[IntPtr]0);Start-Sleep -Milliseconds 300} # Down, Enter: "Check for updates"
Start-Sleep -Seconds $WaitSeconds
$r=New-Object W+RECT;[void][W]::GetWindowRect($h,[ref]$r)
$bmp=New-Object Drawing.Bitmap ($r.R-$r.L),($r.B-$r.T);$g=[Drawing.Graphics]::FromImage($bmp);$dc=$g.GetHdc()
[void][W]::PrintWindow($h,$dc,2);$g.ReleaseHdc($dc);$g.Dispose()
New-Item -ItemType Directory -Force (Split-Path $Out) | Out-Null;$bmp.Save($Out,[Drawing.Imaging.ImageFormat]::Png);$bmp.Dispose()
[void][W]::PostMessage($h,0x0010,[IntPtr]0,[IntPtr]0) # WM_CLOSE
if(!$p.WaitForExit(10000)){$p.Kill()}
"captured $Out; launcher exit $($p.ExitCode)"
