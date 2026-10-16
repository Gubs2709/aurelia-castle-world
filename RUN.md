# Play Aurelia on Windows

Download **Aurelia-Windows-x64.zip** from the [Windows release](https://github.com/Gubs2709/aurelia-castle-world/releases/tag/v0.8.1), extract the entire ZIP, and double-click **Play Reference Castle.cmd**. Keep the Builds directory and all of its contents together. Unity Editor, Blender and Node.js are not needed to play.

The game opens in borderless full screen at your desktop resolution and starts at the harbour. Press Escape to pause/release the cursor, or Alt+F4 to quit. Mouse controls the camera and the wheel changes the distance.

| Input | Action |
|---|---|
| WASD / Shift / Space | Walk / run / jump |
| Mouse / wheel | Orbit / zoom |
| 1 / 2 / 3 / 4 | Forest / blossoms / cottage / waterfalls |
| 5 / 6 / 7 / 8 | Harbour / autumn / river retreat / sea tree |
| P / R | Castle vista / original spawn |
| F / Q / E | Fly camera / down / up |
| H / Escape | Toggle help / pause and cursor |

## From a source checkout

Run the same CMD file. If the Windows build is missing, the launcher uses an installed, authenticated GitHub CLI (`gh`) to download and extract release v0.8.1. Access to this private repository is required. Without GitHub CLI, download/extract the release ZIP manually into the checkout.

## Edit or rebuild

Open `AureliaUnity` with Unity **6000.6.5f1**. Open `Assets/Scenes/ReferenceCastle.unity`, then press Play. Packages and their pinned versions are in `AureliaUnity/Packages`.

Use **Aurelia > Build Windows release (fullscreen)** in the Editor, or run the Editor executable with:

```text
-batchmode -quit -projectPath <checkout>/AureliaUnity -executeMethod ReleaseBuild.Build -logFile <checkout>/build.log
```

The result goes to `Builds/ReferenceCastle/Citadel.exe`. `ReleaseBuild` sets borderless full screen and native desktop resolution. Runtime startup also applies full screen unless `-screen-fullscreen 0` is explicitly supplied for diagnostics. Example windowed launch:

```bat
Builds\ReferenceCastle\Citadel.exe -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 -start-bay
```

The original procedural art scripts under `tools/blender` contain historical `D:/GAME` output paths. Existing saved assets work from a normal clone; adjust those paths before regenerating art elsewhere. See `HANDOFF_V8.md` before running any old scene builder.

## Scope and hardware

This is a local 3D exploration world. Interiors, quests, dialogue, progression, audio and saving are not implemented. The reference illustrations' exact style and incidental detail are not fully matched. The game has been tested on an RTX5060Ti; integrated-graphics performance remains untested.
