# OCMonitor

## Description
OCMonitor will activate your GPU overclock only when you need it. It allows you to setup different OC profiles in MSI Afterburner according to how your hardware is behaving. Ex: 
- If your GPU has a 60% LOAD, then select the overvolt overclocking profile to gain performance in your game.
- If your GPU has less than a 60% LOAD, then select the undervolt overclocking profile to keep your card chill and make your system use less power. You don't need an overclocked card for browsing the web or watching a movie anyways, do you?

## Requirements
- MSI Afterburner
- Overclocking profiles created

## Installation
- Create a scheduled task for launching OCMonitor.App.exe as a service
- Edit appsettings.json
  - Monitor Settings: 
    - CoreMonitor: GPU core settings
    - MemoryMonitor: GPU memory settings
    - IntervalSecs: The monitor refresh interval in seconds
    - AfterBurnerSettings: Installation directory and profiles to be used
