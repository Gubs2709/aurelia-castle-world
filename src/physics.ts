import RAPIER from '@dimforge/rapier3d-compat';
import { Quaternion, Vector3 } from 'three';

export { RAPIER };
export class Physics {
  world: RAPIER.World;
  constructor() { this.world = new RAPIER.World({ x: 0, y: -22, z: 0 }); }
  box(x: number, y: number, z: number, w: number, h: number, d: number, rotationX = 0) {
    const desc = RAPIER.ColliderDesc.cuboid(w / 2, h / 2, d / 2).setTranslation(x, y, z);
    if (rotationX) { const q = new Quaternion().setFromAxisAngle(new Vector3(1, 0, 0), rotationX); desc.setRotation(q); }
    return this.world.createCollider(desc);
  }
  cylinder(x: number, y: number, z: number, radius: number, h: number) {
    return this.world.createCollider(RAPIER.ColliderDesc.cylinder(h / 2, radius).setTranslation(x, y, z));
  }
}
