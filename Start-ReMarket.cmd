@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\Start-ReMarketLocal.ps1" -AutoConnect
if errorlevel 1 pause
