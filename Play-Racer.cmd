@echo off
setlocal
set "RACER_LATEST=%~dp0Builds\Latest"
if not exist "%RACER_LATEST%\WoodstockRushLauncher.exe" (
  echo The Woodstock Rush launcher installation is missing from Builds\Latest.
  pause
  exit /b 1
)
echo Starting Woodstock Rush with the existing signed-release launcher...
start "" /D "%RACER_LATEST%" "%RACER_LATEST%\WoodstockRushLauncher.exe"

