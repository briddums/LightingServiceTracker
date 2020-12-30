@echo off
REM In order to run this from powershell it must be called via .\uninstall

REM Stop the service if it's currently running
sc stop LightingServiceTracker

REM Delete the service
sc delete LightingServiceTracker