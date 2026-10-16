import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { Physics, RAPIER } from './physics';
import { movementVector, SPAWN } from './config';

export class Player {
  root = new THREE.Group();
  visual = new THREE.Group();
  body: RAPIER.RigidBody;
  collider: RAPIER.Collider;
  controller: RAPIER.KinematicCharacterController;
  mixer?: THREE.AnimationMixer;
  actions: Record<string, THREE.AnimationAction> = {};
  state = 'idle';
  velocityY = 0;
  grounded = false;
  keys = new Set<string>();
  jumpQueued = false;
  moving = false;
  enabled = false;
  previous = new THREE.Vector3();
  current = new THREE.Vector3();
  constructor(public physics: Physics, scene: THREE.Scene) {
    scene.add(this.root); this.root.add(this.visual);
    this.body = physics.world.createRigidBody(RAPIER.RigidBodyDesc.kinematicPositionBased().setTranslation(SPAWN.x, SPAWN.y, SPAWN.z));
    this.collider = physics.world.createCollider(RAPIER.ColliderDesc.capsule(.55, .36), this.body);
    this.controller = physics.world.createCharacterController(.025);
    this.controller.enableAutostep(.4, .25, true);
    this.controller.enableSnapToGround(.3);
    this.controller.setMaxSlopeClimbAngle(Math.PI / 4);
    this.controller.setMinSlopeSlideAngle(Math.PI / 3);
    this.current.copy(SPAWN); this.previous.copy(this.current);
    addEventListener('keydown', e => {
      if (!this.enabled || (e.target instanceof HTMLElement && /INPUT|SELECT|BUTTON/.test(e.target.tagName))) return;
      if (['Space', 'ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.code)) e.preventDefault();
      this.keys.add(e.code);
      if (e.code === 'Space' && !e.repeat) this.jumpQueued = true;
    });
    addEventListener('keyup', e => this.keys.delete(e.code));
    addEventListener('blur', () => this.clearInput());
    document.addEventListener('visibilitychange', () => { if (document.hidden) this.clearInput(); });
    this.reset();
  }
  async load() {
    const gltf = await new GLTFLoader().loadAsync(`${import.meta.env.BASE_URL}models/knight.gltf`);
    const model = gltf.scene;
    const bounds = new THREE.Box3().setFromObject(model);
    const size = bounds.getSize(new THREE.Vector3()); const scale = 1.9 / size.y;
    model.scale.setScalar(scale); model.position.y = -bounds.min.y * scale;
    model.traverse(object => { if (object instanceof THREE.Mesh) { object.castShadow = true; object.receiveShadow = true; } });
    this.visual.add(model);
    // Armour layers give the CC0 base character a distinctive travelling-knight silhouette.
    const steel = new THREE.MeshStandardMaterial({ color: '#9daeb3', metalness: .65, roughness: .42 });
    const dark = new THREE.MeshStandardMaterial({ color: '#334d55', metalness: .3, roughness: .6 });
    const brass = new THREE.MeshStandardMaterial({ color: '#cab782', metalness: .6, roughness: .4 });
    const armour = (geometry: THREE.BufferGeometry, material: THREE.Material, x: number, y: number, z: number) => {
      const mesh = new THREE.Mesh(geometry, material); mesh.position.set(x, y, z); mesh.castShadow = true; this.visual.add(mesh); return mesh;
    };
    const helm = armour(new THREE.SphereGeometry(.29, 12, 8), steel, 0, 1.65, 0); helm.scale.set(1, 1.15, 1);
    armour(new THREE.BoxGeometry(.37, .045, .05), dark, 0, 1.68, .25);
    armour(new THREE.BoxGeometry(.055, .24, .06), brass, 0, 1.58, .26);
    for (const side of [-1, 1]) {
      const shoulder = armour(new THREE.SphereGeometry(.2, 8, 6), steel, side * .32, 1.25, 0); shoulder.scale.y = .7;
    }
    const shield = armour(new THREE.CylinderGeometry(.31, .31, .07, 8), dark, -.4, .88, -.12); shield.rotation.x = Math.PI / 2;
    armour(new THREE.BoxGeometry(.045, .42, .025), brass, -.4, .88, -.17);
    armour(new THREE.BoxGeometry(.32, .045, .025), brass, -.4, .92, -.17);
    const sword = armour(new THREE.BoxGeometry(.07, .75, .045), steel, .35, .63, -.08); sword.rotation.z = -.12;
    armour(new THREE.BoxGeometry(.25, .055, .08), brass, .3, 1, -.08);
    const cape = armour(new THREE.PlaneGeometry(.55, .88, 4, 5), new THREE.MeshStandardMaterial({ color: '#3b7178', side: THREE.DoubleSide, roughness: 1 }), 0, .96, -.22);
    cape.rotation.x = -.13;
    this.mixer = new THREE.AnimationMixer(model);
    for (const clip of gltf.animations) this.actions[clip.name] = this.mixer.clipAction(clip);
    this.actions.Idle?.play();
  }
  clearInput() { this.keys.clear(); this.jumpQueued = false; }
  reset() {
    this.body.setTranslation(SPAWN, true); this.body.setNextKinematicTranslation(SPAWN);
    this.current.copy(SPAWN); this.previous.copy(SPAWN); this.velocityY = 0;
    this.grounded = false; this.clearInput(); this.root.position.set(SPAWN.x, SPAWN.y - .91, SPAWN.z);
    this.root.rotation.y = Math.PI;
  }
  fixedUpdate(dt: number, yaw: number) {
    this.previous.copy(this.current);
    const forward = this.enabled ? Number(this.keys.has('KeyW') || this.keys.has('ArrowUp')) - Number(this.keys.has('KeyS') || this.keys.has('ArrowDown')) : 0;
    const right = this.enabled ? Number(this.keys.has('KeyD') || this.keys.has('ArrowRight')) - Number(this.keys.has('KeyA') || this.keys.has('ArrowLeft')) : 0;
    const sprint = this.keys.has('ShiftLeft') || this.keys.has('ShiftRight');
    const direction = movementVector(forward, right, yaw); const speed = sprint ? 9 : 5;
    this.moving = !!(forward || right);
    if (this.grounded && this.velocityY < 0) this.velocityY = 0;
    if (this.jumpQueued && this.grounded) { this.velocityY = 8.2; this.grounded = false; }
    this.jumpQueued = false; this.velocityY -= 22 * dt;
    this.controller.computeColliderMovement(this.collider, { x: direction.x * speed * dt, y: this.velocityY * dt, z: direction.z * speed * dt }, RAPIER.QueryFilterFlags.EXCLUDE_SENSORS);
    const move = this.controller.computedMovement();
    const pos = this.body.translation();
    this.body.setNextKinematicTranslation({ x: pos.x + move.x, y: pos.y + move.y, z: pos.z + move.z });
    this.grounded = this.controller.computedGrounded();
    this.physics.world.timestep = dt; this.physics.world.step();
    this.current.copy(this.body.translation());
    if (this.current.y < -15 || Math.abs(this.current.x) > 155 || Math.abs(this.current.z) > 155) this.reset();
    if (this.moving) {
      const target = Math.atan2(direction.x, direction.z);
      const delta = Math.atan2(Math.sin(target - this.root.rotation.y), Math.cos(target - this.root.rotation.y));
      this.root.rotation.y += delta * (1 - Math.exp(-14 * dt));
    }
    const next = !this.grounded ? 'jump' : this.moving ? (sprint ? 'run' : 'walk') : 'idle';
    if (next !== this.state) {
      const oldClip = this.state === 'idle' || this.state === 'jump' ? this.actions.Idle : this.actions.Run;
      const newClip = next === 'idle' || next === 'jump' ? this.actions.Idle : this.actions.Run;
      if (newClip && newClip !== oldClip) { newClip.reset().fadeIn(.18).play(); oldClip?.fadeOut(.18); }
      this.state = next;
    }
    if (this.actions.Run) this.actions.Run.timeScale = sprint ? 1.15 : .7;
    this.mixer?.update(dt);
    // The source has idle/run clips. Airborne pose is a distinct procedural state.
    this.visual.rotation.x = THREE.MathUtils.damp(this.visual.rotation.x, next === 'jump' ? -.16 : 0, 12, dt);
    this.visual.position.y = next === 'jump' ? .06 : 0;
  }
  interpolate(alpha: number) {
    this.root.position.lerpVectors(this.previous, this.current, alpha);
    this.root.position.y -= .91;
  }
}
