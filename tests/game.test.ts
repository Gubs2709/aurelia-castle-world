import test from 'node:test';
import assert from 'node:assert/strict';
import * as THREE from 'three';
import { RAPIER, Physics } from '../src/physics';
import { Player } from '../src/player';
import { Valley } from '../src/world';
import { FollowCamera } from '../src/camera';
import { movementVector, LANDMARKS, SPAWN } from '../src/config';

// Only DOM services used while constructing signs and registering input.
Object.assign(globalThis, {
  addEventListener() {},
  document: {
    addEventListener() {},
    createElement() { return { width: 0, height: 0, getContext() { return new Proxy({}, { get: () => () => {}, set: () => true }); } }; },
  },
});
await RAPIER.init();

function fixture(fullWorld = false) {
  const physics = new Physics(); const scene = new THREE.Scene();
  const valley = fullWorld ? new Valley(scene, physics) : undefined;
  if (!fullWorld) physics.box(0, -.5, 0, 300, 1, 300);
  const player = new Player(physics, scene); player.enabled = true;
  function tick(count = 60) { for (let i = 0; i < count; i++) player.fixedUpdate(1 / 60, 0); }
  function teleport(x: number, z: number, y = 1) {
    player.body.setTranslation({ x, y, z }, true); player.body.setNextKinematicTranslation({ x, y, z });
    player.current.set(x, y, z); player.previous.copy(player.current); player.velocityY = 0; player.grounded = false;
    tick(20);
  }
  tick();
  return { physics, valley, player, tick, teleport };
}

test('camera-relative movement normalises diagonals and rotates correctly', () => {
  assert.equal(Math.hypot(...Object.values(movementVector(1, 1, 0))), 1);
  assert.deepEqual(movementVector(0, 0, 0), { x: 0, z: -0 });
  assert.ok(Math.abs(movementVector(1, 0, Math.PI / 2).x + 1) < 1e-8);
});

test('fixed-step movement covers the same distance at 30, 60, and 144 render FPS', () => {
  const distances = [30, 60, 144].map(fps => {
    const f = fixture(); f.player.keys.add('KeyW');
    let accumulator = 0;
    for (let frame = 0; frame < fps * 2; frame++) {
      accumulator += 1 / fps;
      while (accumulator + 1e-10 >= 1 / 60) { f.player.fixedUpdate(1 / 60, 0); accumulator -= 1 / 60; }
    }
    const distance = SPAWN.z - f.player.current.z; f.physics.world.free(); return distance;
  });
  distances.forEach(d => assert.ok(Math.abs(d - 10) < .08, `Distance ${d}`));
});

test('jump leaves ground, lands, and reset clears movement and velocity', () => {
  const f = fixture(); const base = f.player.current.y;
  assert.ok(f.player.grounded); f.player.jumpQueued = true; f.tick(15);
  assert.ok(f.player.current.y > base + .7); assert.equal(f.player.grounded, false);
  f.tick(80); assert.ok(f.player.grounded); assert.ok(Math.abs(f.player.current.y - base) < .05);
  f.player.keys.add('KeyW'); f.player.reset();
  assert.equal(f.player.keys.size, 0); assert.equal(f.player.velocityY, 0); assert.equal(f.player.current.z, SPAWN.z);
  f.physics.world.free();
});

test('camera obstruction still works against batched building originals', () => {
  const f = fixture(true);
  const camera = new THREE.PerspectiveCamera();
  const canvas = { addEventListener() {} } as unknown as HTMLCanvasElement;
  const follow = new FollowCamera(camera, canvas, f.valley!.cameraObstacles);
  const position = new THREE.Vector3(17, 0, 59);
  follow.update(position, 1 / 60, true);
  assert.ok(camera.position.z < 62, `Camera entered house at ${camera.position.z}`);
  assert.ok(camera.position.distanceTo(position) < 5);
  f.physics.world.free();
});

test('full valley bridge ramps are traversable in both directions', () => {
  const f = fixture(true); f.teleport(0, 60);
  f.player.keys.add('KeyW'); let maxY = 0;
  for (let i = 0; i < 660; i++) { f.tick(1); maxY = Math.max(maxY, f.player.current.y); }
  assert.ok(f.player.current.z < 7, JSON.stringify(f.player.current));
  assert.ok(maxY > 2.1, `Bridge deck height ${maxY}`);
  f.player.keys.clear(); f.player.keys.add('KeyS'); f.tick(660);
  assert.ok(f.player.current.z > 58, JSON.stringify(f.player.current));
  assert.ok(f.player.grounded); f.physics.world.free();
});

test('water banks and buildings block movement; castle gate remains open', () => {
  const f = fixture(true);
  f.teleport(15, 53); f.player.keys.add('KeyW'); f.tick(180);
  assert.ok(f.player.current.z > 44, `River bank: ${f.player.current.z}`);
  f.player.clearInput(); f.teleport(17, 84); f.player.keys.add('KeyW'); f.tick(180);
  assert.ok(f.player.current.z > 74, `House: ${f.player.current.z}`);
  f.player.clearInput(); f.teleport(0, -45); f.player.keys.add('KeyW'); f.tick(240);
  assert.ok(f.player.current.z < -63, `Gate: ${f.player.current.z}`);
  f.physics.world.free();
});

test('all landmark approaches are reachable and quality never changes colliders', () => {
  const f = fixture(true); const before = f.physics.world.colliders.len();
  f.valley!.setQuality('high'); f.valley!.setQuality('low');
  assert.equal(f.physics.world.colliders.len(), before);
  for (const l of LANDMARKS) {
    f.player.clearInput(); f.teleport(l.x, l.z === -76 ? -70 : l.z, l.id === 'bridge' ? 2.5 : 1);
    assert.ok(f.player.grounded, `${l.name} must have walkable ground`);
    assert.ok(Math.hypot(f.player.current.x - l.x, f.player.current.z - l.z) < l.radius);
  }
  // Check the eastbound branch is continuous from the road to the grove.
  f.player.clearInput(); f.teleport(8, -8); f.player.keys.add('KeyD'); f.tick(430);
  assert.ok(f.player.current.x > 42, `Grove approach: ${JSON.stringify(f.player.current)}`);
  f.physics.world.free();
});
