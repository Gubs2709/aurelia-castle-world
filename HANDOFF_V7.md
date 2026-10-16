# World V7 continuation — 16 October 2026

This supersedes HANDOFF_V5.md and HANDOFF_LIVING_WORLD.md for current state. No git repository exists. Pre-change scene/source backup: `backup/2026-10-16-world-pass/`.

## Final verified result

Latest Windows build succeeded: **0errors,1warning**,518,070,269bytes. Final native `-world-v7-check` report at `artifacts/world-v7/native/validation.txt` **passes every check**. Fifty bones; full-cycle knee motion25.59degrees; jump/attack states; grounded quay; keyboard walk5.01m/s and sprint10.01m/s; orbit; boat/cloud motion. At1600x1000 on RTX5060Ti: ocean613.3FPS(p95 1.82ms), forest116.5FPS(p95 9.38ms), castle109.5FPS(p95 9.48ms). No runtime exceptions/error entries found. Unused stripped DOF/Panini warnings remain in Unity's runtime log; those effects are not used.

The initial knee test was also too narrow: two samples can both fall in the straight-knee phase. It now measures a whole cycle. Independent Editor sampling found27.6degrees in the imported Walk clip. Synthetic keyboard testing now sets IgnoreFocus only inside the opt-in benchmark, eliminating background focus failures; normal game input behaviour is unchanged. Final captures overwrite earlier test images. `validation-before-avatar-fix.txt` preserves the first run as historical diagnostic evidence.

## Current user direction

World first, proper anime-inspired 3D with animated models. Latest request: improve knight hands/legs, inspect asset_demo, taller denser trees with varied branches/trunks/roots and painted bark/leaf colour like canopy/*.jpg, finish previous town/gate work and a vast ocean/dock on the castle side. Do not describe flat sprite sheets as 3D models or promise an exact match to the illustrations.

## Saved project and launch

- Unity project `D:/GAME/AureliaUnity`, Unity6000.6.5f1, active `Assets/Scenes/ReferenceCastle.unity`.
- Windows build `D:/GAME/Builds/ReferenceCastle/Citadel.exe`; launcher `D:/GAME/Play Reference Castle.cmd`.
- Live Editor was PID25044, port7800; verify with `tools/unity.exe status --format json`. It is in normal user context, so restricted calls can falsely report no instances. CLI may require require_escalated.
- Editor log `AureliaUnity/Logs/Editor.log`.

## Assets created first

- `Assets/Art/AnimeWorld`: PaintedBark.png, AutumnMaple.png, BlossomBough.png, exact built-in image_gen prompts in PROVENANCE.json. Branches have genuine transparent alpha. Existing original art in Art/Forest and Art/Realism preserved.
- `tools/blender/knight_reference_v3.py`: builds `artifacts/knight-reference-v3/ReferenceKnight_v3.blend` and .fbx plus hero/face/back renders. Fifty bones including three phalanges per finger/thumb; shaped palms and thumb webs; continuous boot/ankle geometry and blended knee/ankle weights. Source blend retains editable parts. Export joins evaluated parts into one skinned mesh.
- Imported knight `Assets/Models/KnightV3/ReferenceKnight.fbx`, materials and Animator controller in same folder. Clips: Idle, Walk, Run, Jump, Attack. `KnightV3Motion.cs` handles grounded/attack parameters; CastleReferenceView feeds walk/run speed. Sword bone is parented to Hand.L, preserving hand contact better than the initial revision.
- Generic avatar creation/binding is required. First native test failed knee deformation because animator.avatar was null despite valid curves. `V7Knight.Install()` now explicitly sets ModelImporter avatarSetup=CreateFromThisModel and assigns imported avatar; fallback AvatarBuilder exists. Last Editor probe confirmed `ReferenceKnightAvatar`, isValid=true. Do not regress this.
- `AnimeCharacter.shader` supplies bright restrained wrap/banded shading so cloth/armour stay readable in shadow. Initial URP material version looked much darker than the Blender review.

## Forest

`V7Forest.cs` owns 24 branching/root templates (6 shapes x4 variants), each with two full 3D mesh LODs. Current scene uses 21 templates among 382 trees; measured median renderer height 48.12431m. Typical desired heights36–64m, ancient64–78m. Curved trunks, uneven buttress roots and four branch orders; multi-angle dense leaf cards; four bark tints and six foliage palettes. Trees clear walking roads and east port; floor has small crimson/amber/gold leaf mesh litter. Existing ferns/flowers/shrubs remain.

This replaces V3Trees as the current forest owner. DO NOT run ReferenceLivingWorld.Apply or old builders; they can remove current work. V7Forest.Build can rerun, gathering existing sites and preserving blossom/ancient identities. It prevents duplicate designed canopy sites but random filling can add a few sites if earlier filtering left space; not a byte-identical regeneration guarantee.

## Ocean harbour and prior unfinished work

`V7Coast.cs` extends the east side into a real lower coast. `ReferenceCastleBuilder.Height` now calls V7Coast.Height last; changing it requires `V3Run.Terrain()`.

- Original elevated inland lake retained (water61.4). New ocean sea level1.6, quay3.0, near x333,z194, extending to a14km horizon.
- Group `35 • Open eastern ocean and seaward harbour`: 3 long walkable piers,17 ocean vessels with BoatBob, lighthouse, fishing stalls, crates/lamps/net racks,6 harbour houses,24 ocean gulls,5 new drifting horizon cumulus cards. The lake still has15boats and existing gulls.
- Two visible stair flights connect old east district at (254,~59.18,146) through (298,34.1,195) to (327,3.05,145). Smooth invisible ramp colliders beneath visual steps. Correct winding matters: collider normal must face up; editor ray samples passed after fixing.
- East mountain range directly blocking the sea view is inactive; other mountains retained. Camera far plane16000.
- `V7Run.Start()` queued terrain, forest, ground, City, lake Harbor, Walls, Portal, Coast, Save, validation and captures. This applied previously source-only palace walks, hill road, West Market/Back Town plazas and gate portal/masonry. City report227houses before6port cottages. Existing detailed Blender models reused.

## asset_demo assessment

See `ASSET_DEMO_REVIEW.md`, `artifacts/asset_demo_inventory.json`, and `artifacts/asset-demo-review/models.png`. All10GLBs are static, untextured 3D models, no skins or animation clips. Many are basic blockouts. Potential distant props/base meshes; not suitable unchanged for the requested detailed anime world. Original demos untouched; none substituted into game. Preview corrects apparent Z-up source/glTF axis mismatch.

## Controls

WASD walk, Shift sprint, Space jump; mouse look/orbit, wheel zoom; Esc frees cursor, left click relocks; F free flight; Enter back to player; R spawn; P castle vista; H help. 1forest,2blossoms,3town,4falls,**5ocean port,6autumn canopy**. E or locked-cursor left click triggers the authored visual attack (no combat/damage system requested).

## Verification and artifacts

`artifacts/world-v7/editor-validation.txt`: all checks passed, including counts/median, LODs/templates, no missing shaders/materials/scripts, portal/ocean/piers/boats and quay/pier/landing/stair-flight ray collision.

`artifacts/world-v7/forest.png`, ocean-harbour.png, seaward-quay.png, castle-and-city.png are actual Unity Camera.Render captures, not generated concept images. Native captures/report in `artifacts/world-v7/native/`.

First native run: all checks passed except knee motion; walk5.01m/s, sprint10.05m/s; orbit,50bones,jump/attack states, grounded quay,boat/cloud motion passed. Measured at1600x1000 on RTX5060Ti: forest116.4FPS,castle109.2FPS,ocean604.9FPS. These were BEFORE the avatar/character-shader fix; use the final report after rerun, not this baseline. No integrated-GPU measurement exists.

New native test component `WorldV7Benchmark.cs` runs only with **-world-v7-check**, saves screenshots/report and exits. Launch a NORMAL visible game window in normal user context (require_escalated if sandbox launch disappears). Hidden/minimized FPS is invalid. Use -screen-fullscreen0 -screen-width1600 -screen-height1000 -logFile D:/GAME/artifacts/world-v7/native/player.log. Existing -castle-check uses a separate older benchmark.

Build: `tools/unity.exe command build --target StandaloneWindows64 --outputPath D:/GAME/Builds/ReferenceCastle/Citadel.exe --confirm true --caller plugin --skill unity-cli --detach --format json`. Build_status JSON is huge: parse/select summary fields, never dump full report. Do not eval during build. Eval main-thread timeout5s can still finish; inspect before retrying mutations. Always pass caller/skill flags. Save viaV3Run.Save.

## Candid limits

This remains stylized authored art, not exact illustrated likeness/texture reproduction. Facial detail, hair, cloth and attack contact can be improved. No production character retopology or exhaustive animation/contact pass is claimed. No quests/combat systems or online services were added. Preserve earlier review blend files and sources for comparison.
