@echo off
setlocal
set "RACER_LATEST=%~dp0Builds\Latest"
if not exist "%RACER_LATEST%\Racer.exe" (
  echo The latest Racer build is missing from Builds\Latest.
  pause
  exit /b 1
)
if exist "%RACER_LATEST%\VERSION.txt" type "%RACER_LATEST%\VERSION.txt"
echo Starting the latest Racer combined review build...
start "" /D "%RACER_LATEST%" "%RACER_LATEST%\Racer.exe"

