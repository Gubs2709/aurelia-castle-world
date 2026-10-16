# Unity continuation — 15 October 2026

> **Current state: [HANDOFF_V7.md](HANDOFF_V7.md), 16 October.** The old notes below are history. V7 includes the 3D knight, dense reference forests, completed castle/town passes and eastern ocean harbour.

> Historical handoff: [HANDOFF_LIVING_WORLD.md](HANDOFF_LIVING_WORLD.md). Its unfinished refinement and stale executable notes have been superseded by V5 and V7.

## Latest request: the supplied waterfall castle image

The user then rejected the static clouds and basic houses/trees/rocks/water/walls/windows. `ReferenceRealismPass.cs` now adds independent large/small cloud drift, generated detailed foliage and surface textures, branching oaks, complete cottage architecture, irregular rocks, proper gothic surrounds and leaded glass, planar water reflections/depth/refraction, spray, and planted fields. See `REFERENCE_CASTLE.md`. Do not present the earlier ~323 FPS result as the performance of this heavier revision; use the latest native report. The sky retains its blue colours; cloud cards are independently animated. New assets and exact generation prompts are under `Assets/Art/Realism`.

Latest native result: build zero errors/four warnings, 197 MB; all 21 cloud components moved, sky-image motion confirmed, walking/flying/reset passed, visible reflected castle in river. 1600x1000 fixed-view sample: 442.7 FPS average, p95 2.37 ms on RTX 5060 Ti. Source/materials changed enough that this is not a controlled performance comparison with the old art. No native runtime exceptions/shader errors found. Actual game screenshot: `artifacts/reference-castle/native-landscape.png`; detailed review shots: `artifacts/reference-castle/realism`.

The user explicitly prioritized `WhatsApp Image 2026-10-15 at 12.22.44 PM.jpeg`: recreate its castle and background first. A separate `Assets/Scenes/ReferenceCastle.unity` scene now exists, with a tall layered ivory/turquoise castle, eight falls, winding river, diagonal bridge, valley farms, mountain ranges, painted cumulus sky, flowers and mounted traveller. Launch `D:\GAME\Play Reference Castle.cmd`; build is `Builds/ReferenceCastle/Citadel.exe`. See `REFERENCE_CASTLE.md` for coverage, controls and candid visual limits. Current build settings target this scene. The old world below is preserved.

Actual renders and validation reports are under `artifacts/reference-castle`. Build succeeded with zero errors/four warnings. Scene reopen/material/script checks and bridge/causeway collision sampling passed. Do not claim exact visual fidelity: this remains a simpler procedural interpretation of the reference.

## Current user direction

The browser scene was judged much too basic. The user explicitly requested Unity, a proper 3D world based on all 19 original images, then chose **focus entirely on the world first**. They subsequently requested additional adventure-manhwa and open-world inspiration, mentioning "wuthering winds" (interpreted as Wuthering Waves). Preserve that world-first scope. No combat or quest systems were requested after that choice.

## Project

- Unity project: `D:\GAME\AureliaUnity`.
- Editor: installed Unity 6000.6.5f1.
- Scene: `Assets/Scenes/Aurelia.unity`.
- Windows executable: `D:\GAME\Builds\Aurelia\Aurelia.exe` when build succeeds.
- Launcher: `D:\GAME\Play Aurelia.cmd`.
- Original browser project and all 19 JPEGs are preserved in `D:\GAME`.
- CLI binary: `D:\GAME\tools\unity.exe`, downloaded from Unity's official CDN and checked against its published SHA-256. No PATH edit was needed.
- Packages added: Unity Pipeline 0.8.0-exp.1 and Unity glTFast 6.20.0. URP/Input System came with the installed URP template.
- Unity Hub registry and licensing access require the normal user context outside the restricted shell. The existing Personal license was active; no license purchase or cloud deployment occurred.

## What is in the saved world

An 800 m terrain with a continuous route from the southern village to an elevated castle, arched viaduct, three waterfalls, formal gardens, textured trees, flower ribbons, lantern street, market stalls, river, grove and gentle guardian, distant ridgelines, procedural illustrated sky, and ambient dragon. The traveller has imported idle/run animations, custom helmet, shoulders, cloak, shield and sheathed blade. These weapons are visual only.

Controls are documented in the Unity project README. F enables flying inspection; T changes atmosphere; 1–4 visit areas; H hides UI. Gameplay is local and offline. No online services, spending, hosting or publishing were configured.

## Sources and visual limits

The original images remain the primary reference. Kuro's developer interview supplied additional art direction on grading, complementary colour, atmospheric depth, and stylized lighting:
https://www.unrealengine.com/developer-interviews/exploring-the-post-apocalyptic-charm-of-asg-open-worlds-in-wuthering-waves

The world is still an early stylized environment, with repeated buildings, procedural stone/slate textures, simple waterfalls and simple creatures. It does not match Wuthering Waves production fidelity or the full detail of the supplied illustrations. Be candid about that. The finished scene is an editable starting point for further art iteration, not a shipped commercial open-world RPG.

`AureliaUnity/ASSET_PROVENANCE.md` covers Quaternius CC0 textured nature, Kenney CC0 nature/town props and the existing CC0 knight. `Assets/Art/StylizedNature/provenance.json` records downloaded source hashes. No proprietary Wuthering Waves or manhwa assets were imported. No audio is included.

## Validation

`artifacts/unity/navigation.txt` records actual CharacterController checks: village to castle, castle to village, forest approach, jump and landing. Initial checks found a bridge entry lip and a path through a house; a southern ramp and northern forest branch fixed those failures. All four checks passed after the fix.

Runtime input was verified through a real Input System W event and an EditorApplication.Step frame: z advanced from -175 to -174.89 and the Animator Speed parameter became 1. The current editor's background Game view did not continuously advance, despite reporting Play mode; its frozen screenshot must not be presented as an active performance run. A fresh off-screen camera capture was used to inspect the character and scene. An early stale first-frame Game-view capture contained overbright bloom; a reduced bloom clamp/intensity and fresh-render checks followed.

Final build result: `artifacts/unity/build-result.json` (the older `.txt` file belongs to the initial build). Performance result, when run: `artifacts/unity/standalone-benchmark.txt`. Do not claim a frame rate unless the rendered-frame benchmark completes. Native input checks are recorded in `artifacts/unity/native-controls.txt`.

The initial native launch crashed loading `level0` with "Position out of bounds". An environmental MonoBehaviour originally shared the controller file and had a transient, non-asset MonoScript. It was moved into its own `Assets/Scripts/ValleyMotion.cs`, the complete scene regenerated, and the saved scene reopened from disk. After that round trip, the scene had 15 persistent environmental-motion components, zero transient script references, zero missing scripts and zero missing materials. The initial build's one logged error was the Pipeline eval response timeout, not a C# compiler error. The final corrected native launch is verified below.

The corrected build subsequently launched, loaded the world and exited cleanly. A second round-trip check caught costume and Animator edits that had not marked the scene dirty; explicit `MarkSceneDirty`/`SetDirty` fixed those. The costume and Animator both survived the next scene reload. A minimized-window timing sample was invalid because the GPU did not render; it was renamed `invalid-minimized-timing.txt` and must never be cited as FPS evidence. The final benchmark requires `WaitForEndOfFrame` and saves native screenshots in a visible player window.

**Final evidence:** the visible Windows player at 1920×1080 passed real Input System walking (10.62 m in 2 s), jumping/landing (1.44 m rise) and pause checks. Seven-second rendered-frame samples measured mean 310.6 FPS village, 521.2 castle vista, 354.6 grove and 516.9 gardens. These are static-view samples, not roaming guarantees. The final build reports Succeeded, zero errors and four warnings; its native log has no crash or managed exception. See `AureliaUnity/VALIDATION.md` for exact evidence and limitations. Four real native screenshots are `artifacts/unity/native-0.png` through `native-3.png`.

The standalone benchmark flag is `-aurelia-benchmark`; it samples four fixed vistas at the requested resolution with v-sync disabled, records mean FPS and 95th-percentile frame time, then exits. It is a short static-view sample, not a full roaming stress test or a minimum-FPS guarantee. It hides the gameplay controller while measuring.

## Continue safely

Use Unity Pipeline commands through `tools/unity.exe`; inspect `unity status` and the command catalog. Pass `--caller plugin --skill unity-cli`. Drive live Unity scene and asset changes through C# Editor APIs, not YAML.

The base `KingdomBuilder.Build()` replaces the active scene. The saved scene includes later art passes and route fixes. If intentionally regenerating, use **Aurelia > Rebuild complete world (replaces scene)**, which calls `KingdomBuilder.BuildComplete()`. It applies the bridge ramp, forest route, art and costume passes and runs the route checks. Do not run just the base method and assume the final art survived. Save manual scene work separately before regenerating.

Earlier world-generation/build eval calls exceeded the Pipeline command's default 5000 ms response timeout but continued on Unity's main thread. Inspect logs and the resulting artifacts before retrying; don't launch overlapping builds. Scheduling delayCall immediately before a domain reload can lose the callback. Direct method invocation was more reliable for the long build.

Keep scene, generated meshes/materials, source imports and their `.meta` files together. `Library`, `Temp`, `Logs`, IDE files and local builds are ignored. No Git repository or remote was set up.
