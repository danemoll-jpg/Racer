@echo off
rem Builds Base64Check.exe into Temp\reportdeck with the same compiler and SDK as Tools\Build-Launcher.ps1.
set "ROOT=%~dp0..\.."
call "C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" >nul 2>nul
set "INCLUDE=%ROOT%\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\ucrt;%ROOT%\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\shared;%ROOT%\Builds\LauncherSDK\microsoft.windows.sdk.cpp\c\Include\10.0.28000.0\um;%INCLUDE%"
set "LIB=%ROOT%\Builds\LauncherSDK\microsoft.windows.sdk.cpp.x64\c\um\x64;%ROOT%\Builds\LauncherSDK\microsoft.windows.sdk.cpp.x64\c\ucrt\x64;%LIB%"
if not exist "%ROOT%\Temp\reportdeck" mkdir "%ROOT%\Temp\reportdeck"
cl /nologo /utf-8 /std:c++17 /EHsc /O2 /MT /W4 /Fe:"%ROOT%\Temp\reportdeck\Base64Check.exe" /Fo:"%ROOT%\Temp\reportdeck\\" "%ROOT%\Tools\ReportDeck\Base64Check.cpp" "%ROOT%\Launcher\core.cpp" "%ROOT%\Launcher\store.cpp" "%ROOT%\Builds\Launcher\miniz.obj" /link /SUBSYSTEM:CONSOLE
