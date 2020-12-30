@echo off
REM In order to run this from powershell it must be called via .\install

REM Create the service LightingServiceTracker
REM start=delayed-auto means this service will startup after others services have started
sc.exe create LightingServiceTracker binpath=E:\projects\LightingServiceTracker\bin\LightingServiceTracker.exe start=delayed-auto displayname="Lighting Service Tracker"

REM Set a readable description for it
sc.exe description LightingServiceTracker "Restarts the LightingService when it's CPU usage goes over 10%"

REM Start the service
sc.exe start LightingServiceTracker