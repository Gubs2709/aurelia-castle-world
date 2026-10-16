# Local validation — 15 October 2026

## Passed

- `npm run build`: TypeScript check and Vite production build succeed.
- `npm test`: seven passing tests against actual Rapier physics and scene geometry: normalised camera-relative movement; equivalent simulated travel at 30/60/144 render FPS; jump/landing/reset; camera obstruction against batched building originals; bridge traversal in both directions; building and water barriers with a passable castle gate; walkable landmarks and unchanged collision geometry across quality settings.
- Browser: production preview at `http://127.0.0.1:4173/` loads with local knight assets and no application console errors. Start, keyboard movement, village discovery (1/4), all quality selections, settings, and return-to-spawn were checked.
- Visual inspection: village, bridge, grove, and courtyard use real Three.js geometry and were rendered in-browser. The development-only `tests/visual.html` provides reproducible viewpoints. Screenshots are stored in `artifacts/`.
- Desktop layout checked at 1280×720 and 1920×1080; narrow sidebar layout also inspected. Temporary browser size overrides were reset after checking.

## Limits

- 30 FPS on the i3 laptop and 60 FPS on the RTX 5060 Ti PC have **not** been benchmarked. A 1080p layout check does not establish a frame-rate result. A batch of browser automation inputs timed out during the 1080p check; smaller normal-size interactions completed. Use Low on the laptop and measure High on the PC.
- Vite emits an advisory about the roughly 2.9 MB main bundle (about 1 MB gzip), which includes Rapier's inlined WebAssembly. This does not fail the build.
- Rapier 0.19.3 logs an upstream initialization deprecation warning from its compatibility wrapper. The public `init()` API takes no arguments; physics tests and runtime initialization succeed.
- Visuals are a stylized, simplified 3D interpretation of the references, not equivalent to the detail in the original illustrations.
