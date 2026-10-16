# Reference world V8 — 16 October 2026

Current Unity scene: `D:/GAME/AureliaUnity/Assets/Scenes/ReferenceCastle.unity`.
This supersedes HANDOFF_V7.md for the scenery, coastline, controls and current build. Preserve the V7 document for knight rigging history. The browser app and older Aurelia.unity scene are separate historical versions.

## GitHub and fullscreen release update

Current release is **v0.8.1**, in the private repository `https://github.com/Gubs2709/aurelia-castle-world`. The source checkout is now a Git repository. Source and saved generated assets are tracked; Unity caches, machine settings, raw reference collections, backups and built binaries are ignored. Editable Blender kits are additionally preserved in `SourceAssets/`, with engine screenshots and validation under `docs/`.

Run `Play Reference Castle.cmd`. It now starts the current game in borderless full screen at desktop resolution; `Play Aurelia.cmd` forwards to it. If the executable is missing, the canonical CMD downloads the Windows release using an authenticated GitHub CLI. The release ZIP is self-contained and requires no Unity installation. See `RUN.md`.

`ReleaseBuild.Build` saves fullscreen/native-resolution PlayerSettings and builds the current scene. `CastleReferenceView.Start` also enforces native-resolution borderless fullscreen, while honouring an explicit `-screen-fullscreen 0` for diagnostics. Opt-in `DisplayModeCheck` verified `fullscreen=true`, `FullScreenWindow`,1920x1080,desktop1920x1080 through the CMD launcher. Report: `docs/validation/fullscreen-display.json`. Fullscreen release build:0errors,4nonfatal warnings; see `docs/validation/fullscreen-build.json`. The V8 scenery verification below predates this packaging-only update.

## Final verified result

Final Windows build: **Succeeded,0errors,1warning,544440641bytes**. Warning is the optional CLI RuntimePipelineConfig being absent; that disables live CLI control in the built player, not gameplay. Every check in `artifacts/world-v8/native/validation.txt` passes, including effective bloom0,50knight bones,full-cycle knee motion,jump/attack,orbit,grounding,walking/sprinting and cloud/boat/NPC motion. No exceptions/error entries found in the final native log.

Last standalone sample at1600x1000 on RTX5060Ti, with Unity Editor closed: harbour218.7FPS(p95 5.38ms),river120.3FPS(p95 8.62ms),forest119.1FPS(p95 9.16ms),castle109.5FPS(p95 9.48ms),sea-tree892.1FPS(p95 1.30ms). Each is a short3second uncapped sample, not a guarantee across the whole world or on other hardware. Earlier readings below were affected by concurrently running rendering processes; use only the final report. Integrated graphics remain untested.

## User direction and actual scope

The latest references request a winding alpine path instead of the huge harbour stairs, a green riverside cottage/stone bridge/picnic setting, an old tree on a sea cliff, colourful cliffside harbour houses, small timber piers, boats, white lighthouse, turquoise sea, moving large/small clouds, snow peaks, and no overhead bloom glare. This is a real 3D exploration scene with an animated knight. Leaf/flower/cloud alpha cards are surface assets on 3D scenery, not a replacement for the 3D world.

The result is an original stylized interpretation. It does **not** exactly reproduce the illustrations, their composition, gradients or all incidental objects. Do not claim AAA/anime production fidelity or that every tiny detail is complete. Remaining art differences are listed below.

## Saved assets and changes

- Four original image_gen assets: `AureliaUnity/Assets/Art/ReferenceV8/GreenBough.png`, `MeadowFlowers.png`, `CliffStone.png`, `RoofTiles.png`. Original prompts are in PROVENANCE.json beside them. Leaves and flowers have genuine alpha. Prior V7/generated assets remain saved.
- Blender source: `tools/blender/reference_v8_assets.py`; FBX outputs in `AureliaUnity/Assets/Models/ReferenceV8/`. Editable mesh review kit: `artifacts/world-v8/assets/ReferenceV8_EditableKit.blend` (imported exports arranged in named collections; generator remains the procedural source). Inventory: `artifacts/world-v8/assets/models.json`; generator log: `artifacts/v8-blender-assets.log`.
- Six coloured bay house variants with complete roof backing, raised tiles, chimneys, framed/mullioned windows, shutter slats, doors/handles, balconies and striped awnings. One ivy river cottage. Sailing/fishing vessels have curved hulls, planked decks, masts, curved sails, rigging and small fittings.
- Picnic blanket, woven basket/handle, open book/pages, hat, tea/flask, fruit plate and camera; hanging lantern; dock barrels/crates/rope. Harbour residents are articulated simple 3D figures, not detailed skinned human rigs.
- Group35 replaces the V7 giant stair/quay layout: 24 hill/shore houses, continuous 19.6-degree-max path, curved timber promenade, five short landing stages, 13 ocean boats, white lighthouse, fishing rods/lines, fish stalls, 22 residents, gulls and clouds.
- Group38: true open-arch bridge across the river, white ivy cottage, winding path and stone/flower borders, broad green trees, picnic clearing, hanging lantern, retaining wall and small boat. Nearby old grass cards were clipped out of the blanket footprint.
- Group39: rooted green sea tree on an irregular rocky, grassy headland, small stones/flowers and connecting path.
- Group40: connected 3D mountain heightfield with twelve summit seeds, ridges/gullies, snow/blue rock colours and conifer foothills. Replaces old cone-like ranges.
- Group41: stone foundation skirts for 224 existing city houses. Three obsolete overlapping eastern houses are disabled. Existing roof materials use new red tiles. Detailed original city models are retained.
- CloudDrift now faces the active camera horizontally, preventing clouds becoming thin strips when viewed from the side. Original large and small cloud layers drift; new bay layers added. Cloud colours are cooler white/blue.
- Analytic blue sky without a sun disc; Bloom has an **active intensity-zero override**, and LiftGammaGain is disabled in the scene profile. Do not disable the Bloom component itself: runtime inspection found URP falls back to the pipeline profile's0.25bloom, producing the user's bright white orbs. The effective runtime stack is explicitly tested for zero. Directional light reduced to1.3; restrained colour adjustments. Ocean brighter turquoise with bounded glints; river reflection distortion/specular/foam reduced.
- Current knight/Generic avatar/50 bones/Idle,Walk,Run,Jump,Attack remain from V7. Do not rerun an older knight installer that removes the imported avatar.

## Ownership and safe continuation

`Assets/Editor/V8Run.cs` queues the build and captures, with progress at `artifacts/world-v8/progress.txt`. `V8Landscape.cs` owns shore, route, terraced plots and coastal height. `ReferenceCastleBuilder.Height` calls V8Landscape.Height last. After changing heights, use `V3Run.Terrain()` through live Unity CLI.

`V8Kit.cs`: material/import/mesh/FBX helpers. `V8Grove.cs`: broad green branches/roots/cards and headland. `V8Harbor.cs`: bay layout/props/NPCs. `V8Scenery.cs`: river retreat, lighting and snow range. Runtime `HarbourResidentMotion.cs`, `CloudDrift.cs`, `CastleReferenceView.cs`, and the opt-in WorldV7Benchmark support V8.

Use the live Unity CLI for scene mutations; never hand-edit scene YAML. Unity6000.6.5f1, current Editor port7800 at time of work; verify before use. CLI normal-user visibility may require approved escalation. Eval timeout may occur while work continues: inspect progress/logs before retrying a mutation. `V8Run.Start()` rebuilds the V8 regions; it is not a full original-scene bootstrap. Do not run V7Run.Start, V4City.Build, or older full-world builders over V8. Do not call V4Mats.Rebuild after the V8 material repairs. Save via V3Run.Save.

Pre-V8 backup: `backup/2026-10-16-v8/`. No git repository exists here. Keep .meta files and entire build directory together.

## Play and controls

Launch `D:/GAME/Play Reference Castle.cmd`, which starts `D:/GAME/Builds/ReferenceCastle/Citadel.exe` in borderless full screen at desktop resolution with `-start-bay`.

WASD walk, Shift run, Space jump, mouse orbit, wheel zoom, Escape cursor/pause, F fly (Q/E vertical), R original spawn, P castle vista, H help. Shortcuts:1 forest,2 blossoms,3 cottage,4 waterfalls,5 bay,6 autumn,7 river retreat,8 sea tree. The main camera remains player-relative and freely rotatable.

## Verification

Editor checks at `artifacts/world-v8/editor-validation.txt`: all pass, including no old harbour stairs, max path slope19.6degrees,27/27 sampled path collisions, bridge/headland/promenade collision,24 houses,five piers,residents,no bloom and no missing/error materials. These are finite checks, not an exhaustive exploration of every surface.

Actual Unity renders: `artifacts/world-v8/bay-reference.png`, `hillside-path.png`, `riverside-reference.png`, `guardian-reference.png`, `mountains-reference.png`. They are engine renders, not reference/concept images.

Build and native results are recorded in `artifacts/world-v8/build-summary.json` and `artifacts/world-v8/native/validation.txt`. Native check uses `-world-v8-check`, exercises knight clips, orbit, real keyboard input, grounding, boats/clouds/NPC motion, captures views and measures FPS. It exits automatically; normal launch does not run it.

Diagnostic history: `native/validation-curved-promenade.txt` preserves the first check where straight sprinting hit the curved boardwalk's posts. The speed test now runs along a straight landing stage, keeping the collision intact. `native/validation-with-old-instance.txt` preserves a passing but resource-contended run: old V7 game PID40200 was still running and was closed gracefully. `native/validation-editor-rendering.txt` preserves another passing run while the live Editor was rendering heavily. Its22–40FPS readings are resource-contended; do not use them as standalone performance. The Editor was saved and closed for the last run (GPU utilisation dropped from84percent to0before the game launch). Reopen the project in Unity Hub when editing; the previously used Editor port7800 is now closed. Camera screenshots now wait for orbit smoothing before capture. The initial sea-tree screenshot caught the transition, while runtime inspection confirmed correct camera positions and skinned mesh bounds.

## Remaining differences / future work

The reference painting's exact landscape arrangement, colour treatment and fine silhouettes are not matched. The bay still has extensive exposed rock; foliage lighting and leaf clustering can improve; paths need more irregular wear/embedded pebbles; headland integration is visibly procedural; mountains are generated relief rather than individually sculpted hero peaks. Older town houses/castle windows remain from earlier passes despite material/foundation repair. Water is animated shaded geometry, not a fluid simulation. The white lighthouse is reused geometry. NPCs are simple decorative figures with walk/idle motion, no conversations or fishing gameplay. Boats bob rather than navigate. Houses have exterior collision and no interiors. No quests/combat progression/audio/save system was added. GPU-less/integrated-graphics performance has not been tested; RTX results cannot establish it.
