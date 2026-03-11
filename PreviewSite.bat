# Docsify Local Preview Server
# File: G:\Copilot_OutPut\FishingGame\PreviewSite.bat
@echo off
cd /d "%~dp0"
echo Starting local documentation server...
echo.
echo Trying Python 3...
python -m http.server 8000
if %ERRORLEVEL% EQU 0 goto :end

echo.
echo Python not found. Trying Python 2...
python -m SimpleHTTPServer 8000
if %ERRORLEVEL% EQU 0 goto :end

echo.
echo Could not start a local server. Please install Python.
pause
exit /b 1

:end
echo.
echo Server stopped.
pause
