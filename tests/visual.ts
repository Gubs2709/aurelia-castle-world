import { Rendering } from '../src/rendering';
import { RAPIER, Physics } from '../src/physics';
import { Valley } from '../src/world';
import { Player } from '../src/player';

// Development-only visual inspection page; Vite does not include it in dist.
await RAPIER.init();
const renderer = new Rendering(document.querySelector('canvas')!);
const physics = new Physics();
const world = new Valley(renderer.scene, physics);
const player = new Player(physics, renderer.scene);
await player.load();
renderer.setQuality('medium'); world.setQuality('medium');
const views: Record<string, { camera: [number, number, number]; target: [number, number, number]; knight: [number, number, number] }> = {
  village: { camera: [0, 5, 110], target: [0, 10, -50], knight: [0, .03, 102] },
  bridge: { camera: [16, 7, 54], target: [0, 3, 22], knight: [0, 1.45, 36] },
  forest: { camera: [29, 5, 6], target: [53, 6, -18], knight: [42, .03, -7] },
  castle: { camera: [-19, 6, -67], target: [3, 15, -105], knight: [-13, .2, -74] },
};
const name = new URLSearchParams(location.search).get('scene') ?? 'village'; const view = views[name] ?? views.village;
renderer.camera.position.set(...view.camera); renderer.camera.lookAt(...view.target);
player.root.position.set(...view.knight); player.root.rotation.y = Math.PI;
renderer.scene.updateMatrixWorld(true);
world.update(20); renderer.draw();
document.querySelector('#status')!.textContent = `${name} · ${renderer.renderer.info.render.calls} draw calls · rendered with actual game modules`;
