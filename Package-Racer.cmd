@echo off
pushd "%~dp0"
pwsh -NoProfile -File "%~dp0Tools\Package-Racer.ps1" -Version 0.15.0-review3
if errorlevel 1 echo Packaging failed. Previous files have been preserved; see the error above.
pause
popd



