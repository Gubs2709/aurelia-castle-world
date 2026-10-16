# Aurelia — PC continuation handoff

**Latest saved Unity revision: V8. Read [HANDOFF_V8.md](HANDOFF_V8.md) first.** It contains the current ReferenceCastle scene, new harbour/path/river/headland assets, controls and verification. The historical notes below predate it.

## Read this first

This file preserves project context from the laptop conversation on 15 October 2026. It is a portable engineering handoff, not a transfer of the original chat transcript or account memories. Read README.md, VALIDATION.md, and ASSETS.md, then inspect the implementation before changing it. Preserve the 19 user-provided JPEGs in the root. Do not restart or replace the project without a reason.

## User intent and decisions

- User wants a browser-playable fantasy world inspired by all 19 pictures, with movement in every direction and a freely rotating camera.
- Selected Three.js, TypeScript, and Vite; Rapier for physics. Selected castle valley, rich stylized 3D, third-person knight, exploration with four landmarks, both computers supported through quality settings, free licensed assets plus custom scenery. No purchases or external publishing requested.
- Reference style: detailed fantasy illustrations/pixel art, enormous castles, waterfalls, colourful villages, lantern markets, magical forests, friendly golems, dragons, and peaceful travel. Current visuals are simplified procedural stylized 3D, substantially less detailed than the reference artwork. This fidelity gap is acknowledged, not solved.
- Target PC: RTX 5060 Ti with 16 GB VRAM and 16 GB system RAM rated 6000 MHz. Laptop CPU: Intel i3-7020U at 2.30 GHz; RAM/GPU could not be verified because system queries were restricted.

## Implemented

- Continuous bounded 300 × 300 m outdoor valley: Briarwood Village, Rivergate bridge, Whispering Grove, and Aurelia Castle with accessible courtyard, fountain, and gardens.
- Animated local CC0 knight model by Quaternius; custom helmet, cape, shield, and sword. Idle/run source clips; reduced run speed for walking and a procedural airborne pose for jump.
- WASD/arrow keys move relative to camera, Shift sprints, Space jumps, left mouse drag rotates, scroll zooms, Escape pauses. Capsule character controller, fixed 60 Hz physics and render interpolation, gravity, autostep, bridge ramps, river barriers, building collision, world bounds, and reset.
- Camera raycasts prevent building obstruction, including after static geometry batching.
- Session-only discovery counter and minimap, start/loading/error screens, settings, Low/Medium/High graphics presets defaulting to Low, return to spawn preserving discoveries.
- Procedural environment includes sky, distant mountains, clouds, trees, grass/flowers, signs, village market props, waving flags, animated water, simple distant waterfalls, particles, guardian, and circling ambient dragon.
- No quests, inventory, dialogue, combat, interiors, multiplayer, persistent saves, mobile controls, or interactive dragon. No audio included.

## Structure

- src/main.ts: bootstrap, fixed-step loop, integration, local diagnostics.
- src/world.ts: geometry, static batching, vegetation instancing, animated environment, physical map boundaries.
- src/player.ts, src/camera.ts, src/physics.ts: controller, character/animation, follow camera, physics helpers.
- src/rendering.ts, src/config.ts, src/ui.ts, src/style.css: graphics, shared presets/landmarks, interface.
- public/models/knight.gltf and KnightCharacter.bin: required local knight assets. See ASSETS.md for verified CC0 source.
- tests/game.test.ts: actual Rapier and scene acceptance tests with minimal DOM stubs.
- tests/visual.html: development-only inspection page with four static viewpoints using actual game modules. Not shipped in dist.
- artifacts/: saved browser screenshots.

## Validation and limitations

- Seven tests passed: diagonal/camera-relative movement; equal travel under simulated 30/60/144 render FPS; jumping/landing/reset; camera obstruction; bridge travel both directions; river/building collision and open gate; landmark approaches with invariant collision geometry across quality levels.
- Production build and TypeScript checking passed.
- Browser production preview checked: assets load locally, start, actual keyboard movement, village discovery (1/4), all quality choices, settings, reset. No application console errors captured.
- Four areas visually inspected, with screenshots in artifacts/. Desktop layout checked at 1280×720 and 1920×1080, plus narrow sidebar layout.
- FPS targets (30 on laptop Low, 60 at 1080p on PC High) have NOT been benchmarked. A browser automation key batch timed out during the laptop's 1080p check. Do not report layout checks or fixed-step tests as GPU performance measurements.
- Known nonfatal notices: Vite warns about the ~2.9 MB main bundle (~1 MB gzip; Rapier embeds WASM). Rapier compat 0.19.3 logs an upstream initialization deprecation warning despite using its public no-argument init() API.
- Laptop localhost preview/dev servers do not transfer to the PC. Start new local processes there. No Git remote, deployment, or external hosting was configured.

## PC setup

Install Node.js 22.12+ (Node 24 is also suitable). Open this project folder in Codex on the PC. In the terminal:

```powershell
npm ci
npm test
npm run build
npm run dev
```

Use the URL printed by Vite. For built output use npm run preview after a build. Quality remains Low initially; select High in Settings on the GPU PC. npm ci recreates node_modules, which is intentionally excluded from the transfer ZIP; dist is also excluded and regenerated by the build.

## Suggested continuation prompt

"Read HANDOFF.md, README.md, VALIDATION.md, and ASSETS.md. Continue the existing Aurelia Three.js game from this folder. This is my RTX 5060 Ti 16 GB VRAM PC with 16 GB RAM. Install dependencies and run the existing game. Preserve our current design and reference images; do not rebuild from scratch. Summarize its current state, then wait for my next change request."

Do not infer authorization for new features or visual redesign from this handoff alone. The immediate task is continuity and running the existing project on the PC.
