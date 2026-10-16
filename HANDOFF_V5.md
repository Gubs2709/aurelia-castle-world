# Handoff: Citadel of Falling Water (Unity), saved 15 Oct 2026, ~23:55 IST

Read this first. It supersedes `HANDOFF_LIVING_WORLD.md` (older history, still useful for pitfalls).

## Project
- Unity project `D:\GAME\AureliaUnity` (Unity 6000.6.5f1, URP). Active scene `Assets/Scenes/ReferenceCastle.unity`. Root object: `THE CITADEL OF FALLING WATER • reference composition`.
- Windows build: `D:\GAME\Builds\ReferenceCastle\Citadel.exe` (built 23:45, 0 errors, 4 warnings, 476 MB). Launcher: `D:\GAME\Play Reference Castle.cmd`.
- Editor CLI: `D:\GAME\tools\unity.exe`. Eval pattern: `./tools/unity.exe command eval --code 'return V3Run.X();' --timeout 280 --caller plugin --skill unity-cli --format json`. After editing any C# file run `UnityEditor.AssetDatabase.Refresh();` and wait ~60-90 s for the domain reload; calls fail with "does not contain a definition" until it finishes. Eval "main thread timed out after 5000ms" usually means it is still running; check the result in the scene/log before retrying.
- Blender 5.2: `C:\Program Files\Blender Foundation\Blender 5.2\blender.exe -b -P script.py [-- Name ...]`. Scripts in `D:\GAME\tools\blender`; FBX output goes to `Assets/Models/Village`; previews to `artifacts/living-world/blender_*.png`.
- Texture/sprite generators (numpy/PIL): `D:\GAME\tools\texgen` (`ground.py`, `structure.py`, `sprites.py`), output `Assets/Art/Gen`.
- Backups: `D:\GAME\backup\ReferenceCastle_20h10.unity` (scene before the V3+ work). No git.

## Controls
WASD move, Shift run, Space jump, mouse look (cursor locks; Esc frees it, left click relocks), wheel zoom, F fly, Enter back to player, R spawn, 1-4 area jumps, P vista, H hide help.

## What is in the saved scene and build
- **Terrain**: `ReferenceCastleBuilder.Height` now has a wide plateau (|x| to ~245, z 92..345, y=65), city levelling zones (`V3Layout.CityLevel`), and a lake bowl (`V3Layout.LakeCarve`). After changing Height run `V3Run.Terrain()` to regenerate the mesh.
- **Ground/foliage (V3)**: `Terrain2` splat shader and textures, branching card trees (`V3Trees`), grass/flowers/ferns/bushes (`V3Flora`), boulders, `NaturalFoliage` shader.
- **City (V4)**: `V3Layout.Roads()` is the single source of truth for streets and districts; `V4City.Build()` places Blender houses along streets, market square, landmarks (gate, clock tower, chapel, windmill), outer homesteads. Materials by FBX slot name come from `V4Mats`.
- **Districts (V5)**: West Market (D), Harbour (E), Back Town (F, behind the palace) streets/houses; `V5Harbor` builds lake mesh (`Aurelia/Lake` shader), 8 docks, boats with `BoatBob`, lighthouse, fish stalls, reeds, `GullFlock` gulls; `V5Walls` builds a crown wall on the cliff rim, a ring wall (x=+/-124, z 100..240) with towers, and gatehouses W/E/N at z=166 and x=0.
- **Blender models**: houses (`houses.py`: 13 types, `harbor.py`: 9 Harbor_*), props (`props.py`, `landmarks.py`, `harbor.py`: docks, boats, lighthouse, fish stalls, Wall_Section, Wall_Tower), `portal.py` (Gate_Portal), knights (`knight.py` old blocky in-game version).
- **Look**: `V4Look` colour grade (saturated, bloom, ACES), warm sun.
- **Player in game**: the OLD blocky Blender knight (`Knight.fbx` + `Scripts/KnightAnimator.cs`), installed by `V4Knight.Install()`.

## UPDATE 16 Oct 00:15 - sprite knight installed
The player is now a 2D billboard knight from `D:\GAME\Zed-walk-trimmed.png` (5x3 grid of 256 px cells, 13 side-view walk frames facing right; copy at `Assets/Art/Gen/deco_knight_sheet.png`). `Scripts/SpriteKnight.cs` + `Shaders/SpriteBillboard.shader` + `SlashFx.shader`, installed by `V3Run.SpriteKnightInstall()`. Idle = frame 5 with breathing bob; walk plays frames by speed; attack (left mouse while cursor locked, or E) is a procedural lunge/tilt plus a slash arc, because the sheet has NO real idle or attack frames. The 3D Knight/Knight2 FBX are no longer used in the scene. Build rebuilt 00:12 (0 errors). Native check: all pass except the "visible animated player" (expects an Animator) and, in that run, the orbit check (not investigated; focus/timing suspected).

## Not done / known problems
1. **Upper castle gate is still a dark opening** (castle opening at x=0, z~105, y from 34, 16 wide, 19 high). `Gate_Portal.fbx` is built and previewed but NOT placed. Plan: `V4City.Place("Gate_Portal", new Vector3(0,34,105.4f), Vector3.back, V3.Root, 1, false)`.
2. **Cliff face under the palace is earth/moss**, not masonry. Plan: swap material on renderers named `Terraced mossy escarpment *` to a `Aurelia/Surface Detail` stone material (wall_stone texture, tint ~1.5,1.42,1.3, `_Terrain` 0).
3. West Market and Back Town plazas have no fountain/stalls; the old `Plaza` code was written but never applied. Harbour has one ring of houses only (a "Hill Road" outer ring was planned). No walkways from the new W/E/N gates to the palace (planned "Palace Walk West/East/North" roads in `V3Layout`). Trees overhang district edges; plan was `ZoneOf(...,-16)` in `V3Trees`.
4. After any layout change, rerun passes in this order: `Terrain, Ground, Trees, Flora, Rocks, Escarp, City, Harbor, Walls, Bridge` (all idempotent; `V3Run.Save()` then build).
5. **New anime knight is NOT installed.** `tools/blender/knight3.py` (+ `knight_face.py` face texture, `knight2_render.py` previews) exports `Models/Village/Knight2.fbx` (armature, ~47k verts, actions Idle, Walk, Run, Jump, SitDown, SitIdle) and `KnightFace.png`. Previews: `artifacts/living-world/knight/sheet_*.png`. The user said "not approved yet" and will give changes; they then said to leave the knight and move on. Installing it needs an Animator/controller or a script that plays the baked clips by speed state, and the `face` material from the FBX/texture. Known weaknesses: chunky hair ribbons, flat face texture, plain cape, sword always sheathed, stiff arms.
6. Map is larger on the plateau only; valley unchanged. Performance last measured earlier at about 130-150 FPS on the RTX 5060 Ti at 1600x1000 (before the new districts); not re-measured. Integrated GPUs untested.
7. The build was NOT run through the native check after the last rebuild (`-castle-check`, window must be focused or key-input checks fail).
8. User references: castle `WhatsApp Image 2026-10-15 at 12.22.44 PM.jpeg`, `ref\download (5).jpg`, `ref\fondos de panatalla hd.jpg`; folders `ref`, `houseandsceneref`, `canopy`, `town`, `knight`, plus the harbour/fish-town/market pictures the user attached in chat (colourful hillside harbour with lighthouse, picnic by the river, cliff tree, waterfall castle, dock at sunset, dragon tower).

## Pitfalls learned
- `Mathf.SmoothStep(from,to,t)` is not HLSL smoothstep (use a helper in C#).
- Float literals in C# need `f` (many compile errors came from `.5` doubles).
- Never hand-edit scene YAML. Do not run `KingdomBuilder`/`ReferenceCastleBuilder` rebuilds (they replace the scene).
- Bash tool: very long heredocs mixing Python and C# sometimes fail to parse; write files with the Write tool.
- Runtime MonoBehaviours must live in files with matching names (native build corruption otherwise).
- Unity renames saved sub-meshes to their file names; don't look meshes up by `.name`.

## Suggested next steps (in order)
1. Place Gate_Portal, stone-face the cliff, add plazas/palace walks/hill ring (items 1-3), rerun passes, build, native check.
2. Get the user's change list for the anime knight, update `knight3.py`, re-render, get approval, then install it.
3. Performance pass (combine/instance more, LODs, fewer shadow casters) and re-measure.
