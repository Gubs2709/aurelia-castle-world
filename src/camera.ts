import * as THREE from 'three';

export class FollowCamera {
  yaw = 0;
  pitch = .24;
  distance = 7.5;
  dragging = false;
  enabled = false;
  raycaster = new THREE.Raycaster();
  target = new THREE.Vector3();
  desired = new THREE.Vector3();
  constructor(public camera: THREE.PerspectiveCamera, canvas: HTMLCanvasElement, public obstacles: THREE.Object3D[]) {
    canvas.addEventListener('pointerdown', e => {
      if (!this.enabled || e.button !== 0) return;
      this.dragging = true; canvas.setPointerCapture(e.pointerId); canvas.classList.add('dragging');
    });
    canvas.addEventListener('pointerup', e => {
      this.dragging = false; if (canvas.hasPointerCapture(e.pointerId)) canvas.releasePointerCapture(e.pointerId); canvas.classList.remove('dragging');
    });
    canvas.addEventListener('pointercancel', () => { this.dragging = false; canvas.classList.remove('dragging'); });
    canvas.addEventListener('pointermove', e => {
      if (!this.dragging || !this.enabled) return;
      this.yaw -= e.movementX * .004;
      this.pitch = THREE.MathUtils.clamp(this.pitch + e.movementY * .003, -.1, 1.15);
    });
    canvas.addEventListener('wheel', e => {
      e.preventDefault(); if (this.enabled) this.distance = THREE.MathUtils.clamp(this.distance + e.deltaY * .008, 3, 17);
    }, { passive: false });
    addEventListener('blur', () => { this.dragging = false; canvas.classList.remove('dragging'); });
  }
  update(playerPosition: THREE.Vector3, dt: number, immediate = false) {
    this.target.copy(playerPosition).add(new THREE.Vector3(0, 1.55, 0));
    const offset = new THREE.Vector3(Math.sin(this.yaw) * Math.cos(this.pitch), Math.sin(this.pitch), Math.cos(this.yaw) * Math.cos(this.pitch));
    this.raycaster.set(this.target, offset); this.raycaster.far = this.distance + .45;
    const hit = this.raycaster.intersectObjects(this.obstacles, true)[0];
    const distance = hit ? Math.max(.65, hit.distance - .5) : this.distance;
    this.desired.copy(this.target).addScaledVector(offset, distance);
    this.desired.y = Math.max(.6, this.desired.y);
    if (immediate || hit) this.camera.position.copy(this.desired);
    else this.camera.position.lerp(this.desired, 1 - Math.exp(-12 * dt));
    this.camera.lookAt(this.target);
  }
  reset() { this.yaw = 0; this.pitch = .24; this.distance = 7.5; }
}
