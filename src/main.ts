import './style.css';
import { Rendering } from './rendering';
import { Physics, RAPIER } from './physics';
import { Valley } from './world';
import { Player } from './player';
import { FollowCamera } from './camera';
import { UI } from './ui';

const ui = new UI();
async function boot() {
  ui.progress(12, 'Opening the gates…');
  const canvas = document.querySelector<HTMLCanvasElement>('#world')!;
  const graphics = new Rendering(canvas);
  canvas.addEventListener('webglcontextlost', e => { e.preventDefault(); ui.error(new Error('The graphics context was lost. Close other GPU-heavy tabs, then reload the valley.')); });
  await RAPIER.init();
  ui.progress(35, 'Planting the grove and laying the bridge…');
  // Yield once so the loading status can paint before construction.
  await new Promise<void>(resolve => requestAnimationFrame(() => resolve()));
  const physics = new Physics();
  const valley = new Valley(graphics.scene, physics);
  const player = new Player(physics, graphics.scene);
  ui.progress(78, 'Your knight is getting ready…');
  await player.load().catch(() => { throw new Error('The knight asset could not load. Run the project through npm run dev or npm run preview, and check public/models contains knight.gltf and KnightCharacter.bin.'); });
  const camera = new FollowCamera(graphics.camera, canvas, valley.cameraObstacles);
  camera.update(player.root.position, 0, true);
  ui.onStart = () => { player.enabled = true; camera.enabled = true; player.clearInput(); canvas.tabIndex = 0; canvas.focus(); };
  ui.onPause = paused => {
    player.enabled = ui.started && !paused; camera.enabled = ui.started && !paused;
    player.clearInput(); camera.dragging = false;
    if (!paused && ui.started) canvas.focus();
  };
  ui.onReset = () => { player.reset(); camera.reset(); camera.update(player.root.position, 0, true); };
  ui.onQuality = q => { graphics.setQuality(q); valley.setQuality(q); };
  ui.ready();
  let last = performance.now(), accumulator = 0, time = 0, uiTime = 0;
  const step = 1 / 60;
  const frame = (now: number) => {
    requestAnimationFrame(frame);
    const dt = Math.min((now - last) / 1000, .1); last = now;
    if (!ui.paused) {
      time += dt; accumulator += dt;
      while (accumulator >= step) { player.fixedUpdate(step, camera.yaw); accumulator -= step; }
      player.interpolate(accumulator / step);
      valley.update(time); camera.update(player.root.position, dt);
      // A moving shadow volume follows the traveller instead of the whole map.
      graphics.sun.position.set(player.current.x - 65, 110, player.current.z + 65);
      graphics.sun.target.position.set(player.current.x, 0, player.current.z);
    } else accumulator = 0;
    uiTime += dt;
    if (uiTime > .1) {
      ui.update(player.current.x, player.current.z, camera.yaw); uiTime = 0;
      canvas.dataset.position = JSON.stringify(player.body.translation());
      canvas.dataset.state = player.state;
      canvas.dataset.drawCalls = String(graphics.renderer.info.render.calls);
    }
    graphics.draw();
  };
  requestAnimationFrame(frame);
  // Read-only diagnostics for local performance and acceptance checks.
  Object.defineProperty(window, '__valley', { value: {
    get position() { return { ...player.body.translation() }; },
    get state() { return player.state; },
    get grounded() { return player.grounded; },
    get discoveries() { return [...ui.discovered]; },
    get graphics() { return { quality: graphics.quality, calls: graphics.renderer.info.render.calls, triangles: graphics.renderer.info.render.triangles }; },
  } });
}
boot().catch(error => { console.error(error); ui.error(error); });
