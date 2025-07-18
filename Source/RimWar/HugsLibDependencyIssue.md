# Issue: HugsLib Dependency Tracking for RimWar

## Problem
- RimWar mod depends on the HugsLib mod, which provides required assemblies and namespaces (e.g., `HugsLib`, `HugsLib.Settings`, `HugsLib.Utils`).
- Currently, HugsLib is downloaded from the Steam Workshop on game launch, not present in the local `Mods` folder for development/building.
- This causes build errors (e.g., CS0246) due to missing references to HugsLib assemblies.

## Proposed Solutions
1. **Local Dependency:**
   - Download/copy HugsLib 1.6 mod into the local `Mods` folder.
   - Reference the HugsLib DLL in the RimWar project for local builds.
2. **Workshop Dependency:**
   - Build logic to detect and use HugsLib from the Workshop folder if not present locally.
   - Document how to set up the reference for both cases.

## Next Steps
- [ ] Download HugsLib 1.6 and place in local `Mods` folder.
- [ ] Add reference to HugsLib.dll in RimWar project.
- [ ] Document how to update the reference if using Workshop version.
- [ ] Consider build scripts or conditional logic for local vs. Workshop dependency.

## Notes
- Ensure the referenced HugsLib version matches the one expected by RimWar (currently 1.6).
- Update this file as progress is made or decisions change.
