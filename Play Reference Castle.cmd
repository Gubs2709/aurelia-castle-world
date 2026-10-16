@echo off
setlocal
set "AURELIA_ROOT=%~dp0"
set "AURELIA_GAME=%~dp0Builds\ReferenceCastle"
if exist "%AURELIA_GAME%\Citadel.exe" goto launch

echo Downloading the Windows game from GitHub...
where gh >nul 2>&1
if errorlevel 1 goto manual_download
if not exist "%AURELIA_ROOT%.downloads" mkdir "%AURELIA_ROOT%.downloads"
gh release download v0.8.1 --repo Gubs2709/aurelia-castle-world --pattern "Aurelia-Windows-x64.zip" --dir "%AURELIA_ROOT%.downloads" --clobber
if errorlevel 1 goto manual_download
powershell.exe -NoProfile -Command "$ErrorActionPreference='Stop'; $cache=Join-Path $env:AURELIA_ROOT '.downloads\unpacked'; Expand-Archive -LiteralPath (Join-Path $env:AURELIA_ROOT '.downloads\Aurelia-Windows-x64.zip') -DestinationPath $cache -Force; Copy-Item -LiteralPath (Join-Path $cache 'Builds') -Destination $env:AURELIA_ROOT -Recurse -Force"
if errorlevel 1 goto manual_download
if not exist "%AURELIA_GAME%\Citadel.exe" goto manual_download

:launch
start "Aurelia - Citadel of Falling Water" /D "%AURELIA_GAME%" "%AURELIA_GAME%\Citadel.exe" -screen-fullscreen 1 -window-mode borderless -start-bay %*
exit /b 0

:manual_download
echo.
echo Download Aurelia-Windows-x64.zip from:
echo https://github.com/Gubs2709/aurelia-castle-world/releases/tag/v0.8.1
echo Extract the whole ZIP into this folder, then run this file again.
echo For automatic download, install GitHub CLI and sign in with gh auth login.
pause
exit /b 1
