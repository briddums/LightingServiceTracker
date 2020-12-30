# Lighting Service Tracker
The ASUS Lighting Service has a bad habit of being a CPU hog.  It will routinely take 100% CPU resources of one of my CPU cores.  When this happens, the program effectively freezes and all effects stop working.

This service checks up on the lighting service CPU's usage every minute.  If it's > 10% (average should be 5%) then it will stop & start the service.  This keeps the Lighting Service CPU utilization low.  More importantly, it keeps the effects from freezing up.
