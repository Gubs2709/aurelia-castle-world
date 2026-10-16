# Aurelia · The Castle Valley

**Current game: Unity Reference Castle V8.** Run [Play Reference Castle.cmd](Play%20Reference%20Castle.cmd). The saved project is `AureliaUnity/`, scene `Assets/Scenes/ReferenceCastle.unity`. Current controls, asset locations, validation and limitations are in [HANDOFF_V8.md](HANDOFF_V8.md). The launcher starts at the new bay; press7 for the river retreat and8 for the sea tree. The browser version documented below is historical.

A single-player, third-person fantasy exploration scene built with Three.js, TypeScript, Vite, and Rapier. The world is inspired by the 19 untouched reference JPEGs in this folder.

## Run locally

Install Node.js 22.12+ (or 20.19+), then open a terminal in this folder:

```sh
npm install
npm run dev
```

Open the localhost URL printed by Vite. Click **Enter the valley**. The app needs WebGL 2; it must be served over HTTP rather than opened directly as an HTML file.

```sh
npm run typecheck
npm test
npm run build
npm run preview
```

The production output is `dist/`. No publishing, login, backend, or multiplayer service is configured. After installation, the game runs without a network connection; all models and the Rapier runtime are bundled locally.

For a static visual check of each area while the development server is running, open `/tests/visual.html`. This inspection page uses the actual world modules and is excluded from the production build. Screenshots from local validation are in `artifacts/`.

## Controls

- **WASD / arrow keys:** move relative to the camera.
- **Shift:** sprint. **Space:** jump.
- **Left mouse drag:** rotate the camera. **Scroll:** zoom.
- **Escape / Settings:** pause and change quality or return to spawn.

Find Briarwood Village, the Rivergate bridge, Whispering Grove, and Aurelia Castle. Follow the main northbound road over the bridge; the grove branches east after the crossing. The fountain occupies the centre of the castle courtyard, so walk around it. Discoveries last for the current session and survive resetting position. Reloading begins a new session.

## Graphics

Low is the default for this laptop. Medium and High increase resolution, shadow quality, vegetation detail, and particles. Tree trunks and collision geometry remain present at every setting. High is intended for the RTX 5060 Ti PC. 30 FPS on the laptop and 60 FPS at 1080p on the PC are targets, not measured guarantees. Integrated graphics drivers, browser GPU access, and thermal limits affect performance.

## Structure and limits

`src/world.ts` constructs the continuous 300 × 300 m bounded world; other modules separate rendering, physics, character, camera, UI, and shared settings. Player physics uses a fixed 60 Hz step with render interpolation. Camera obstruction uses raycasts against solid scenery. The knight is a locally stored CC0 model; see ASSETS.md.

This is an outdoor exploration prototype. Interiors, dialogue, quests, inventory, combat, persistent saves, mobile controls, and interactive dragons are not implemented. The distant dragon is ambient scenery. The four landmarks and reset are the complete exploration loop. Browser diagnostics are exposed read-only at `window.__valley`.
