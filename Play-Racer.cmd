@echo off
setlocal
set "RACER_LATEST=%~dp0Builds\Latest"
if not exist "%RACER_LATEST%\Racer.exe" (
  echo The latest Racer build is missing from Builds\Latest.
  pause
  exit /b 1
)
start "" /D "%RACER_LATEST%" "%RACER_LATEST%\Racer.exe"
