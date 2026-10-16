# Verified on the local PC — 15 October 2026

## Final Windows build

- Unity 6000.6.5f1, URP 17.6.0, StandaloneWindows64.
- Result: **Succeeded**, zero build errors, four nonfatal warnings.
- Reported output: 279,470,591 bytes, approximately 267 MiB.
- Evidence: parent `artifacts/unity/build-result.json`.
- The corrected executable loads, renders, runs the native test harness, and exits cleanly. The initial scene-load crash is fixed; the earlier crash log is retained separately.

## Native input and rendering

The built player ran at 1920 × 1080 on an NVIDIA GeForce RTX 5060 Ti. Input was injected through Unity Input System in the running player, not a replacement controller implementation.

- Walking for two seconds: **10.62 m**, pass.
- Jump and landing: **1.44 m** rise and returned to ground, pass.
- Paused movement: blocked, pass.
- Four native screenshots: parent `artifacts/unity/native-0.png` through `native-3.png`.
- Native input evidence: parent `artifacts/unity/native-controls.txt`.
- Runtime log: parent `artifacts/unity/standalone.log`; no crash or managed exception in the final run.

## Short rendered-frame timing sample

The visible standalone player sampled each fixed vista for seven seconds after warmup, with v-sync disabled. The harness waits for rendered frames and saves a screenshot at each viewpoint.

| View | Mean FPS | 95th percentile frame time |
|---|---:|---:|
| Village | 310.6 | 4.51 ms |
| Castle/viaduct vista | 521.2 | 2.04 ms |
| Grove | 354.6 | 4.33 ms |
| Gardens | 516.9 | 3.09 ms |

These are short, stationary samples of this scene on this PC. They do not establish minimum FPS while roaming, laptop performance, other hardware, or future larger-world performance. The normal game uses v-sync. A preliminary minimized-window timing was invalid and is explicitly retained as `invalid-minimized-timing.txt`; it is not performance evidence.

## Geometry and persistence

Actual Unity CharacterController checks pass for village → castle, castle → village, grove approach, and jump/landing. A bridge entry lip and a route crossing a house were found and fixed. Evidence: parent `artifacts/unity/navigation.txt`.

The final scene was saved and reopened from disk. Traveller costume and Animator controller survived. All MonoBehaviour references were persistent script assets; no missing scripts or materials were found. The environmental component lives in its own matching `ValleyMotion.cs` file.

## Notices and limitations

Build warnings concern collision pre-baking defaults for future Unity versions, disabled runtime Pipeline control, and two stripped internal debug shaders. Runtime URP logs additionally report stripped **unused** depth-of-field and Panini effect passes. Those effects are not part of the world profile. The project does not silence these upstream notices or enable unused effects just to retain their shaders.

Unity's background Game view sometimes stalled, so editor counters and stale screen captures were not used as final performance evidence. Native rendering and native input checks are the final runtime evidence.

Art quality remains an early stylized pass: repeated houses, procedural architectural textures, simple waterfall/creature geometry, sparse areas and limited animation. No quests, combat, interiors, dialogue, multiplayer, saves or audio were implemented under the world-first scope.
