import * as THREE from 'three';
import { mergeGeometries } from 'three/addons/utils/BufferGeometryUtils.js';
import { Physics } from './physics';
import { PRESETS, seededRandom, type Quality } from './config';

const C = {
  grass: '#809c4b', grassLight: '#97ae59', stone: '#d3cfb6', trim: '#e8dfbd', darkStone: '#9da99d',
  roof: '#416b79', roofLight: '#5e929b', timber: '#72513c', plaster: '#e6cda1', gold: '#d4aa60',
};
export class Valley {
  root = new THREE.Group();
  cameraObstacles: THREE.Object3D[] = [];
  flags: THREE.Mesh[] = [];
  clouds = new THREE.Group();
  dragon = new THREE.Group();
  leftWing = new THREE.Group(); rightWing = new THREE.Group();
  water: THREE.Mesh;
  ripples: THREE.InstancedMesh;
  particles: THREE.Points;
  trees: THREE.InstancedMesh[] = [];
  flowers: THREE.InstancedMesh[] = [];
  grass: THREE.InstancedMesh;
  random = seededRandom(815);
  private mats = new Map<string, THREE.MeshStandardMaterial>();
  private boxGeo = new THREE.BoxGeometry(1, 1, 1);
  private sphereGeo = new THREE.IcosahedronGeometry(1, 1);
  constructor(public scene: THREE.Scene, public physics: Physics) {
    scene.add(this.root, this.clouds, this.dragon);
    this.ground();
    this.paths();
    this.village();
    this.bridge();
    this.castle();
    this.grove();
    this.mountains();
    this.skyDetails();
    this.buildDragon();
    this.vegetation();
    this.grass = this.makeGrass();
    const waterGeometry = new THREE.PlaneGeometry(300, 24, 40, 12);
    waterGeometry.rotateX(-Math.PI / 2);
    this.water = new THREE.Mesh(waterGeometry, new THREE.MeshStandardMaterial({
      color: '#48aeb0', metalness: .25, roughness: .24, transparent: true, opacity: .92,
    }));
    this.water.position.set(0, -.66, 32); this.root.add(this.water);
    this.ripples = new THREE.InstancedMesh(new THREE.PlaneGeometry(1, 1),
      new THREE.MeshBasicMaterial({ color: '#c5ede0', transparent: true, opacity: .45, side: THREE.DoubleSide }), 180);
    this.root.add(this.ripples);
    const points = new Float32Array(240 * 3);
    for (let i = 0; i < 240; i++) {
      points[i * 3] = 46 + (this.random() - .5) * 47;
      points[i * 3 + 1] = 1 + this.random() * 10;
      points[i * 3 + 2] = -13 + (this.random() - .5) * 43;
    }
    const pg = new THREE.BufferGeometry(); pg.setAttribute('position', new THREE.BufferAttribute(points, 3));
    this.particles = new THREE.Points(pg, new THREE.PointsMaterial({ color: '#bdfce1', size: .19,
      transparent: true, opacity: .8, depthWrite: false, blending: THREE.AdditiveBlending }));
    this.root.add(this.particles);
    this.setQuality('low');
    this.batchStatic();
  }
  material(color: string, roughness = .9) {
    const key = `${color}:${roughness}`;
    if (!this.mats.has(key)) {
      const mat = new THREE.MeshStandardMaterial({ color, roughness });
      const grass = color === C.grass || color === C.grassLight;
      const stone = color === C.stone || color === C.trim || color === '#b6bca9';
      const path = color === '#c6b88b' || color === '#d0c6a7';
      const roof = color === C.roof || ['#a4624f', '#4c8588', '#5d7880', '#b17b52', '#73856b', '#a46554'].includes(color);
      if (grass || stone || path || roof) mat.map = this.surfaceTexture(grass ? 'grass' : stone ? 'stone' : path ? 'path' : 'roof');
      this.mats.set(key, mat);
    }
    return this.mats.get(key)!;
  }
  surfaceTexture(kind: 'grass' | 'stone' | 'path' | 'roof') {
    const canvas = document.createElement('canvas'); canvas.width = canvas.height = 256;
    const ctx = canvas.getContext('2d')!; const rand = seededRandom(456);
    ctx.fillStyle = '#eeeeee'; ctx.fillRect(0, 0, 256, 256);
    if (kind === 'grass') {
      for (let i = 0; i < 4500; i++) {
        const shade = 130 + Math.floor(rand() * 120); ctx.fillStyle = `rgb(${shade},${shade},${shade})`;
        ctx.globalAlpha = .18 + rand() * .25; ctx.fillRect(rand() * 256, rand() * 256, 1 + rand() * 3, 1 + rand() * 4);
      }
    } else {
      const h = kind === 'roof' ? 32 : 48; const w = kind === 'path' ? 44 : 70;
      for (let row = 0; row < 256 / h; row++) for (let col = -1; col < 256 / w + 1; col++) {
        const x = col * w + (row % 2) * w / 2;
        const shade = 205 + Math.floor(rand() * 45); ctx.fillStyle = `rgb(${shade},${shade},${shade})`;
        ctx.fillRect(x + 1, row * h + 1, w - 2, h - 2);
        ctx.fillStyle = '#777777'; ctx.globalAlpha = .16; ctx.fillRect(x, row * h + h - 2, w, 2); ctx.globalAlpha = 1;
      }
    }
    ctx.globalAlpha = 1;
    const tex = new THREE.CanvasTexture(canvas); tex.colorSpace = THREE.SRGBColorSpace;
    tex.wrapS = tex.wrapT = THREE.RepeatWrapping;
    const repeat = kind === 'grass' ? 65 : kind === 'path' ? 8 : kind === 'roof' ? 2 : 3;
    tex.repeat.set(repeat, repeat); tex.anisotropy = 4; return tex;
  }
  mesh(geo: THREE.BufferGeometry, color: string, x: number, y: number, z: number, parent: THREE.Object3D = this.root) {
    const m = new THREE.Mesh(geo, this.material(color)); m.position.set(x, y, z);
    m.castShadow = true; m.receiveShadow = true; parent.add(m); return m;
  }
  box(x: number, y: number, z: number, w: number, h: number, d: number, color: string, solid = false, parent: THREE.Object3D = this.root) {
    const m = this.mesh(this.boxGeo, color, x, y, z, parent); m.scale.set(w, h, d);
    if (solid) { this.physics.box(x, y, z, w, h, d); this.cameraObstacles.push(m); }
    return m;
  }
  ball(x: number, y: number, z: number, scale: THREE.Vector3, color: string, parent: THREE.Object3D = this.root) {
    const m = this.mesh(this.sphereGeo, color, x, y, z, parent); m.scale.copy(scale); return m;
  }
  ground() {
    this.box(0, -2, 97, 300, 4, 106, C.grass, true);
    this.box(0, -2, -65, 300, 4, 170, C.grassLight, true);
    this.box(0, -5, 32, 300, 2, 24, '#708b72');
    // Invisible river banks leave only the bridge corridor passable.
    for (const x of [-78, 78]) this.physics.box(x, 1.5, 32, 144, 5, 24);
    for (const x of [-150, 150]) this.physics.box(x, 15, 0, 2, 40, 302);
    for (const z of [-150, 150]) this.physics.box(0, 15, z, 302, 40, 2);
    // Irregular low bank stones visually explain the water boundary.
    for (let i = 0; i < 115; i++) {
      const x = -145 + this.random() * 290;
      if (Math.abs(x) < 8) continue;
      const z = this.random() < .5 ? 19 : 45;
      this.ball(x, -.05, z, new THREE.Vector3(1.3 + this.random(), .65, 1.2), '#9caa84');
    }
  }
  pathSegment(ax: number, az: number, bx: number, bz: number, width: number, color = '#c6b88b') {
    const length = Math.hypot(bx - ax, bz - az);
    const m = this.box((ax + bx) / 2, .035, (az + bz) / 2, width, .065, length + 1, color);
    m.material = m.material.clone();
    if (m.material.map) { m.material.map = m.material.map.clone(); m.material.map.repeat.set(width / 4, length / 4); }
    m.rotation.y = Math.atan2(bx - ax, bz - az); m.castShadow = false;
    // Small cobbles break up the path edges without adding collider clutter.
    for (let i = 0; i < length / 3; i++) {
      const t = this.random(); const side = this.random() < .5 ? -1 : 1;
      const nx = (bz - az) / length; const nz = -(bx - ax) / length;
      this.ball(ax + (bx - ax) * t + nx * width * .48 * side, .08,
        az + (bz - az) * t + nz * width * .48 * side, new THREE.Vector3(.35, .12, .28), '#b0ad8c');
    }
  }
  paths() {
    this.pathSegment(0, 148, 0, 44, 9);
    this.pathSegment(0, 20, 0, -64, 9);
    this.pathSegment(0, -6, 46, -13, 5);
    this.pathSegment(0, 78, -38, 78, 5);
    this.pathSegment(0, 78, 38, 78, 5);
    this.mesh(new THREE.CylinderGeometry(13, 13, .08, 40), '#c9bd98', 0, .05, 79);
  }
  house(x: number, z: number, color: string, roof: string, width = 9, height = 8, depth = 10) {
    const group = new THREE.Group(); group.position.set(x, 0, z); this.root.add(group);
    this.box(0, height / 2, 0, width, height, depth, color, false, group);
    this.physics.box(x, height / 2, z, width, height, depth);
    this.cameraObstacles.push(group);
    this.box(0, .4, 0, width + .5, .8, depth + .5, '#9f9f86', false, group);
    // Triangular gables and two sloping roof panels.
    const gable = new THREE.Shape(); gable.moveTo(-width / 2, 0); gable.lineTo(width / 2, 0); gable.lineTo(0, 3.5); gable.closePath();
    this.mesh(new THREE.ExtrudeGeometry(gable, { depth, bevelEnabled: false }), color, 0, height, -depth / 2, group);
    const slope = Math.atan2(3.5, width / 2 + .6); const roofW = Math.hypot(width / 2 + .6, 3.5);
    for (const side of [-1, 1]) {
      const panel = this.box(side * (width / 4 + .3), height + 1.75, 0, roofW, .38, depth + 1.5, roof, false, group);
      panel.rotation.z = -side * slope;
      for (let row = 0; row < 4; row++) {
        const t = row / 4;
        const beam = this.box(side * (width / 2 + .6) * t, height + 3.5 * (1 - t) + .16, 0,
          .1, .12, depth + 1.6, roof, false, group);
        beam.rotation.z = -side * slope;
      }
    }
    for (const side of [-1, 1]) {
      this.box(side * (width / 2 - .12), height / 2, depth / 2 + .05, .25, height, .2, C.timber, false, group);
      this.box(0, height * .55, side * (depth / 2 + .08), width, .26, .2, C.timber, false, group);
      for (const wx of [-width * .28, width * .28]) {
        this.box(wx, height * .72, side * (depth / 2 + .13), 1.3, 1.8, .15, '#526e70', false, group);
        this.box(wx, height * .72, side * (depth / 2 + .23), .1, 1.8, .12, C.trim, false, group);
        this.box(wx, height * .72, side * (depth / 2 + .23), 1.3, .1, .12, C.trim, false, group);
        this.box(wx, height * .72 - 1.1, side * (depth / 2 + .3), 1.8, .35, .55, C.timber, false, group);
        for (let f = 0; f < 4; f++) this.ball(wx - .6 + f * .4, height * .72 - .85, side * (depth / 2 + .3),
          new THREE.Vector3(.2, .25, .22), f % 2 ? '#e18e9d' : '#e8cb77', group);
      }
    }
    this.box(0, 1.6, depth / 2 + .11, 1.8, 3.2, .2, '#655344', false, group);
    // Exposed beams, door surround, shutters, and a small stone doorstep.
    this.box(0, .18, depth / 2 + .65, 2.8, .36, 1.5, '#b2ac94', false, group);
    for (const side of [-1, 1]) {
      this.box(side * 1.03, 1.7, depth / 2 + .2, .19, 3.4, .25, C.timber, false, group);
      const brace = this.box(side * width * .24, height * .27, depth / 2 + .12, .17, height * .6, .17, C.timber, false, group);
      brace.rotation.z = side * .65;
      this.box(0, height - .12, side * (depth / 2 + .11), width, .22, .25, C.timber, false, group);
      for (const wx of [-width * .28, width * .28]) for (const shutter of [-1, 1])
        this.box(wx + shutter * .85, height * .72, side * (depth / 2 + .14), .35, 1.8, .17, C.timber, false, group);
    }
    this.box(width * .28, height + 3.2, -1, 1.2, 4, 1.2, '#b3a692', false, group);
    return group;
  }
  village() {
    this.house(-16, 69, '#e8cea0', '#a4624f', 10, 9, 11);
    this.house(17, 68, '#d7c9ab', '#4c8588', 11, 10, 12);
    this.house(-29, 89, '#d3c0a1', '#5d7880', 10, 7, 10);
    this.house(28, 90, '#ead6aa', '#b17b52', 9, 8, 10);
    this.house(-36, 61, '#ddd6bd', '#73856b', 8, 11, 9);
    this.house(38, 57, '#d9c092', '#a46554', 9, 7, 9);
    // Village well and little market stalls.
    this.mesh(new THREE.CylinderGeometry(2, 2.3, 1, 16), '#a6afa0', -6, .5, 82);
    this.mesh(new THREE.CylinderGeometry(1.55, 1.55, .1, 16), '#428d97', -6, 1.02, 82);
    for (const x of [-8, -4]) this.box(x, 2.7, 82, .2, 3.8, .2, C.timber);
    this.box(-6, 4.5, 82, 5, .4, 4, C.roof);
    for (const [x, z, c] of [[-11, 92, '#ae6e5d'], [12, 86, '#c6a459'], [13, 56, '#698c81']] as const) {
      this.box(x, 1.1, z, 4.4, 2.2, 2, C.timber, true);
      for (const dx of [-2, 2]) this.box(x + dx, 2.5, z, .12, 5, .12, C.timber);
      const awning = this.box(x, 4.4, z, 5.1, .16, 3.7, c); awning.rotation.x = .13;
      for (let i = 0; i < 8; i++) this.ball(x - 1.6 + (i % 4) * .9, 2.4, z - .4 + Math.floor(i / 4) * .8,
        new THREE.Vector3(.35, .3, .35), i % 3 ? '#e0b954' : '#ac6552');
    }
    for (const x of [-7, 7]) for (const z of [100, 58]) this.lantern(x, z);
    this.signpost(7, 103, ['BRIARWOOD', 'AURELIA ↑']);
    // Pitched banners and pennant strings.
    for (let i = 0; i < 8; i++) {
      const flag = this.mesh(new THREE.PlaneGeometry(.8, 1.1, 4, 3), i % 2 ? '#d9b866' : '#517b86', -9 + i * 2.6, 6 - Math.sin(i / 7 * Math.PI), 79);
      flag.material.side = THREE.DoubleSide; this.flags.push(flag);
    }
  }
  lantern(x: number, z: number) {
    this.box(x, 2.4, z, .16, 4.8, .16, '#5f6355');
    this.box(x, 4.6, z, .7, 1, .7, C.gold);
    this.mesh(new THREE.ConeGeometry(.65, .5, 4), '#506b6b', x, 5.35, z).rotation.y = Math.PI / 4;
    this.box(x, 4.6, z, .49, .75, .49, '#f3dda4');
  }
  signpost(x: number, z: number, labels: string[]) {
    this.box(x, 1.7, z, .18, 3.4, .18, C.timber);
    labels.forEach((label, i) => {
      const canvas = document.createElement('canvas'); canvas.width = 512; canvas.height = 96;
      const ctx = canvas.getContext('2d')!; ctx.fillStyle = '#5d5944'; ctx.fillRect(0, 0, 512, 96);
      ctx.strokeStyle = '#b7a779'; ctx.lineWidth = 4; ctx.strokeRect(5, 5, 502, 86);
      ctx.fillStyle = '#f1e4bf'; ctx.font = 'bold 35px Georgia'; ctx.textAlign = 'center'; ctx.fillText(label, 256, 60);
      const tex = new THREE.CanvasTexture(canvas); tex.colorSpace = THREE.SRGBColorSpace;
      const sign = new THREE.Mesh(new THREE.BoxGeometry(3.5, .65, .14), new THREE.MeshStandardMaterial({ map: tex }));
      sign.position.set(x, 3 - i * .8, z); this.root.add(sign);
    });
  }
  bridge() {
    // Level bridge with gentle physical ramps at both ends.
    this.box(0, 1, 32, 10, .8, 26, '#b6bca9', true);
    for (const side of [-1, 1]) {
      const ramp = this.box(0, .4, 32 + side * 19, 10, .8, 12.1, '#b6bca9');
      const angle = side * Math.atan(1.2 / 12); ramp.rotation.x = angle;
      this.physics.box(0, .4, 32 + side * 19, 10, .8, 12.1, angle);
      this.cameraObstacles.push(ramp);
      this.box(side * 4.7, 2.05, 32, .6, 1.6, 26, C.stone, true);
      this.box(side * 4.7, 2.9, 32, .85, .18, 26, C.trim);
      for (let i = 0; i < 7; i++) this.box(side * 4.7, 2.3, 20 + i * 4, .95, 2.2, .95, C.trim);
    }
    for (const z of [24, 39]) {
      for (const x of [-3.5, 3.5]) this.box(x, -1, z, 2, 4, 2.8, '#a0ad9f');
      this.box(0, -.1, z, 10, .6, 2.8, '#a0ad9f');
    }
    for (const x of [-5, 5]) for (const z of [18, 46]) this.lantern(x, z);
  }
  flag(x: number, y: number, z: number, color = '#c18565', scale = 1) {
    this.box(x, y + 1.7 * scale, z, .13, 4 * scale, .13, C.gold);
    const g = new THREE.PlaneGeometry(2.7 * scale, 1.3 * scale, 8, 3);
    g.translate(1.35 * scale, 0, 0);
    const m = this.mesh(g, color, x, y + 2.5 * scale, z); m.material.side = THREE.DoubleSide;
    this.flags.push(m);
  }
  tower(x: number, z: number, radius: number, height: number, roof = true) {
    this.mesh(new THREE.CylinderGeometry(radius, radius * 1.06, height, 12), C.stone, x, height / 2, z);
    this.physics.cylinder(x, height / 2, z, radius, height);
    // The full tower mesh participates in camera obstruction.
    this.cameraObstacles.push(this.root.children[this.root.children.length - 1]);
    for (let y = 2; y < height; y += 5) this.mesh(new THREE.CylinderGeometry(radius + .13, radius + .13, .28, 12), C.trim, x, y, z);
    this.mesh(new THREE.CylinderGeometry(radius + .65, radius + .3, 1.4, 12), C.trim, x, height - .3, z);
    for (let i = 0; i < 10; i++) {
      const a = i / 10 * Math.PI * 2;
      this.box(x + Math.cos(a) * radius, height + .7, z + Math.sin(a) * radius, .9, 1.5, .9, C.trim);
    }
    for (const y of [height * .4, height * .72]) for (const a of [0, Math.PI / 2, Math.PI, -Math.PI / 2]) {
      const window = this.box(x + Math.sin(a) * (radius + .04), y, z + Math.cos(a) * (radius + .04), .7, 2.3, .13, '#587983');
      window.rotation.y = a;
    }
    if (roof) {
      this.mesh(new THREE.ConeGeometry(radius + 1, radius * 2.7, 12), C.roof, x, height + radius * 1.35, z);
      this.mesh(new THREE.ConeGeometry(radius + 1.15, .5, 12), C.roofLight, x, height + .15, z);
      this.flag(x, height + radius * 2.7, z, '#d3aa69', .8);
    } else this.flag(x, height + 1, z, '#668c99', .9);
  }
  castle() {
    this.box(0, .12, -79, 62, .24, 42, '#b9bca7');
    // Gate is a real opening: two wings and a lintel, not a blocked facade.
    for (const x of [-19, 19]) this.box(x, 6.5, -58, 25, 13, 3, C.stone, true);
    this.box(0, 11.5, -58, 13, 3, 3, C.stone, true);
    for (const x of [-31, 31]) this.box(x, 5, -79, 3, 10, 44, C.stone, true);
    for (let x = -29; x <= 29; x += 2.6) {
      if (Math.abs(x) > 7) this.box(x, 13.7, -58, 1.4, 1.4, 3.2, C.trim);
    }
    for (const x of [-31, 31]) for (let z = -62; z > -99; z -= 3) this.box(x, 10.6, z, 3.2, 1.2, 1.4, C.trim);
    this.tower(-31, -58, 4.6, 19, false); this.tower(31, -58, 4.6, 19, false);
    this.tower(-9, -59, 3.8, 23); this.tower(9, -59, 3.8, 23);
    this.box(0, 12, -102, 39, 24, 18, C.stone, true);
    this.box(0, 25, -102, 40, 2, 19, C.trim);
    this.box(0, 29, -105, 22, 8, 13, C.stone, true);
    this.mesh(new THREE.ConeGeometry(17, 12, 4), C.roof, 0, 38, -105).rotation.y = Math.PI / 4;
    this.tower(-21, -101, 5, 33); this.tower(21, -101, 5, 33);
    this.tower(0, -116, 4.8, 49);
    this.tower(-11, -119, 3, 36); this.tower(12, -119, 3, 39);
    this.flag(0, 64, -116, '#d0a258', 1.6);
    for (let x = -15; x <= 15; x += 5) {
      for (const y of [9, 17]) this.box(x, y, -92.9, 1.5, 3.8, .2, '#547b85');
      this.box(x, 23, -92.8, .25, 6, .4, C.trim);
    }
    // Courtyard garden beds, fountain, and walkable paving.
    this.pathSegment(0, -56, 0, -92, 8, '#d0c6a7');
    this.mesh(new THREE.CylinderGeometry(3.7, 4, .7, 24), C.trim, 0, .5, -79);
    this.physics.cylinder(0, .6, -79, 4, 1.2);
    this.mesh(new THREE.CylinderGeometry(3.2, 3.2, .12, 24), '#70c1bf', 0, .9, -79);
    this.mesh(new THREE.CylinderGeometry(.5, .8, 2.2, 12), C.stone, 0, 1.5, -79);
    this.mesh(new THREE.SphereGeometry(.7, 12, 8), C.gold, 0, 2.9, -79);
    for (const x of [-19, 19]) {
      this.box(x, .3, -79, 9, .6, 19, '#abb28a');
      for (let z = -87; z < -70; z += 2) for (let j = 0; j < 3; j++)
        this.ball(x - 3 + j * 3, .85, z, new THREE.Vector3(.8, .65, .8), j % 2 ? '#c58f9b' : '#a1b869');
    }
    for (const x of [-16, 16]) { this.flag(x, 4, -65, '#668998', 1.4); this.lantern(x, -88); }
    // Distant waterfalls spilling from the castle's rocky escarpment.
    for (const x of [-70, -77, 76]) {
      this.ball(x, 10, -110, new THREE.Vector3(22, 19, 23), '#9da994');
      const fall = this.box(x, 12, -87, 2.2, 25, .28, '#b5e5db'); fall.material = new THREE.MeshStandardMaterial({ color: '#b5e5db', transparent: true, opacity: .75 });
      for (let i = 0; i < 4; i++) this.box(x - 1 + i * .6, 12, -86.8, .15, 25, .1, '#e2f1de');
      this.ball(x, .1, -86, new THREE.Vector3(4, .4, 3), '#d7e9db');
    }
  }
  grove() {
    // Ancient tree, glowing crystals, mushrooms, and a moss-covered guardian.
    for (const [x, z] of [[35, -25], [58, -25], [65, -5], [38, 4]]) {
      this.mesh(new THREE.CylinderGeometry(1.1, 2, 16, 8), '#6c6c61', x, 8, z);
      this.physics.cylinder(x, 8, z, 1.8, 16);
      for (let i = 0; i < 5; i++) this.ball(x + (this.random() - .5) * 9, 17 + this.random() * 5, z + (this.random() - .5) * 8,
        new THREE.Vector3(6, 4, 5), ['#83b3ae', '#ae9ab6', '#7da595'][i % 3]);
    }
    for (let i = 0; i < 20; i++) {
      const a = i / 20 * Math.PI * 2; const x = 46 + Math.cos(a) * (8 + this.random() * 6); const z = -13 + Math.sin(a) * (8 + this.random() * 6);
      this.mesh(new THREE.CylinderGeometry(.15, .2, 1.1, 6), '#c8ccac', x, .55, z);
      this.mesh(new THREE.SphereGeometry(.7, 8, 5, 0, Math.PI * 2, 0, Math.PI / 2), i % 2 ? '#bd92a8' : '#9cbdb3', x, 1.05, z);
      const crystal = this.mesh(new THREE.OctahedronGeometry(.5, 0), '#9cdbcf', x + 1, .7, z + .5);
      crystal.scale.y = 2.3;
      crystal.material = new THREE.MeshStandardMaterial({ color: '#92d3cb', emissive: '#2e8884', emissiveIntensity: .65, roughness: .35 });
    }
    const gx = 57, gz = -17;
    this.ball(gx, 3.4, gz, new THREE.Vector3(2.3, 2.8, 1.6), '#899d83');
    this.ball(gx, 6.8, gz, new THREE.Vector3(1.8, 1.5, 1.5), '#9fae8e');
    for (const side of [-1, 1]) {
      this.ball(gx + side * 2.7, 3.2, gz, new THREE.Vector3(.9, 2.2, 1), '#869a80');
      this.ball(gx + side, .9, gz, new THREE.Vector3(.9, 1.4, 1), '#889b82');
      this.box(gx + side * .65, 7.1, gz + 1.4, .4, .23, .13, '#b8f4dd');
      this.ball(gx + side * 1.5, 7.9, gz, new THREE.Vector3(1.2, .6, 1), '#739b65');
    }
    this.physics.cylinder(gx, 4, gz, 3.7, 8);
    this.signpost(29, -5, ['WHISPERING GROVE']);
  }
  mountains() {
    const colors = ['#91b6af', '#7faaa8', '#a7c2b5'];
    for (let i = 0; i < 25; i++) {
      const a = i / 25 * Math.PI * 2;
      const radius = 195 + this.random() * 100; const height = 35 + this.random() * 75;
      const x = Math.sin(a) * radius, z = Math.cos(a) * radius;
      const m = this.mesh(new THREE.ConeGeometry(45 + this.random() * 30, height, 6), colors[i % 3], x, height / 2 - 5, z);
      m.rotation.y = this.random() * 6; m.castShadow = false;
      if (height > 85) this.mesh(new THREE.ConeGeometry(12, height * .24, 6), '#dfede1', x, height - height * .12 - 5, z);
    }
    for (let i = 0; i < 40; i++) {
      const side = i % 2 ? -1 : 1; const x = side * (110 + this.random() * 50); const z = -140 + this.random() * 285;
      this.ball(x, -1, z, new THREE.Vector3(15 + this.random() * 15, 7 + this.random() * 15, 15 + this.random() * 20), i % 2 ? '#7f9b62' : '#91a76b');
    }
  }
  skyDetails() {
    const cloudMat = new THREE.MeshStandardMaterial({ color: '#fff4dc', roughness: 1, flatShading: true });
    for (let i = 0; i < 18; i++) {
      const group = new THREE.Group(); group.position.set(-220 + this.random() * 440, 65 + this.random() * 60, -240 + this.random() * 410);
      for (let j = 0; j < 5; j++) {
        const m = new THREE.Mesh(this.sphereGeo, cloudMat); m.position.set(j * 5, this.random() * 3, this.random() * 4);
        m.scale.set(7 + this.random() * 5, 3 + this.random() * 3, 4 + this.random() * 4); group.add(m);
      }
      this.clouds.add(group);
    }
  }
  buildDragon() {
    const dragonColor = '#9d655e';
    this.ball(0, 0, 0, new THREE.Vector3(1, .8, 3), dragonColor, this.dragon);
    this.ball(0, .3, -3.1, new THREE.Vector3(.7, .65, 1.5), dragonColor, this.dragon);
    const tail = this.mesh(new THREE.ConeGeometry(.7, 5, 6), dragonColor, 0, -.1, 4, this.dragon); tail.rotation.x = Math.PI / 2;
    const shape = new THREE.Shape(); shape.moveTo(0, 0); shape.lineTo(6, -1); shape.lineTo(4, 2); shape.lineTo(1, 3.2); shape.closePath();
    for (const [side, wing] of [[-1, this.leftWing], [1, this.rightWing]] as const) {
      const m = this.mesh(new THREE.ShapeGeometry(shape), '#bd8278', 0, 0, 0, wing);
      m.rotation.x = -Math.PI / 2; m.scale.x = side; m.material.side = THREE.DoubleSide;
      this.dragon.add(wing);
    }
    this.dragon.scale.setScalar(1.8);
  }
  clearForTrees(x: number, z: number) {
    return Math.abs(x) > 12 && !(z > 45 && z < 104 && Math.abs(x) < 47)
      && !(z < -48 && Math.abs(x) < 42) && !(z > 17 && z < 48)
      && !(Math.abs(z - (-6 - x * .15)) < 6 && x > 0 && x < 55)
      && Math.hypot(x - 46, z + 13) > 14 && !(z < -80 && Math.abs(x) > 55 && Math.abs(x) < 90);
  }
  vegetation() {
    const rand = seededRandom(623); const dummy = new THREE.Object3D(); const color = new THREE.Color();
    const trunks = new THREE.InstancedMesh(new THREE.CylinderGeometry(.35, .6, 1, 6), this.material('#716e4f'), 220);
    const crowns = new THREE.InstancedMesh(new THREE.IcosahedronGeometry(1, 1), this.material('#749b69'), 220 * 3);
    const cones = new THREE.InstancedMesh(new THREE.ConeGeometry(1, 1, 7), this.material('#668c70'), 220 * 2);
    for (let i = 0; i < 220; i++) {
      let x: number, z: number;
      do { x = -140 + rand() * 280; z = -140 + rand() * 280; } while (!this.clearForTrees(x, z));
      const h = 6 + rand() * 7;
      dummy.position.set(x, h * .3, z); dummy.scale.set(1, h * .6, 1); dummy.rotation.set(0, 0, 0); dummy.updateMatrix(); trunks.setMatrixAt(i, dummy.matrix);
      // Every potential tree retains its collider across quality levels.
      this.physics.cylinder(x, h * .4, z, .55, h * .8);
      for (let j = 0; j < 3; j++) {
        dummy.position.set(x + (rand() - .5) * 3, h * .67 + rand() * 2, z + (rand() - .5) * 3);
        dummy.scale.set(h * .32, h * .27, h * .32); dummy.updateMatrix(); crowns.setMatrixAt(j * 220 + i, dummy.matrix);
        color.set(['#718f55', '#92a96b', '#749e78', '#a1b67a', '#c1ad74'][i % 5]);
        if (x > 25 && z < 15 && z > -45) color.set(['#9ca3b9', '#8eb8aa', '#b8a2b7'][i % 3]);
        crowns.setColorAt(j * 220 + i, color);
      }
      for (let j = 0; j < 2; j++) {
        dummy.position.set(x, h * .7 + j * h * .17, z); dummy.scale.set(h * .3 * (1 - j * .25), h * .6, h * .3 * (1 - j * .25)); dummy.updateMatrix();
        // Conifers share the same instances but are hidden for broadleaf trees.
        if (i % 3) dummy.scale.setScalar(0);
        dummy.updateMatrix(); cones.setMatrixAt(i * 2 + j, dummy.matrix);
      }
    }
    for (const m of [trunks, crowns, cones]) { m.castShadow = true; m.receiveShadow = true; this.root.add(m); }
    this.cameraObstacles.push(trunks);
    this.trees = [trunks, crowns, cones];
    // Flower heads in dense meadow ribbons, with stems rendered as instances.
    const heads = new THREE.InstancedMesh(new THREE.IcosahedronGeometry(.15, 0), this.material('#e7d6a1'), 6000);
    for (let i = 0; i < 6000; i++) {
      let x: number, z: number;
      do {
        x = i % 3 ? (rand() < .5 ? -1 : 1) * (6 + rand() * 9) : -105 + rand() * 210;
        z = -50 + rand() * 190;
      } while (Math.abs(x) < 6 || (z > 17 && z < 48) || (z > 50 && z < 100 && Math.abs(x) < 44) || (x > 0 && x < 58 && Math.abs(z + 6 + x * .15) < 5));
      const s = .7 + rand() * 1.1; dummy.position.set(x, .22 + rand() * .4, z); dummy.rotation.set(0, rand() * 6, 0); dummy.scale.set(s, s, s); dummy.updateMatrix();
      heads.setMatrixAt(i, dummy.matrix); heads.setColorAt(i, color.set(['#e9d9a3', '#c894aa', '#a0adc8', '#ddbb70', '#d9e5bc'][i % 5]));
    }
    const stems = new THREE.InstancedMesh(new THREE.CylinderGeometry(.018, .024, .3, 3), this.material('#657c44'), 6000);
    const matrix = new THREE.Matrix4();
    for (let i = 0; i < 6000; i++) { heads.getMatrixAt(i, matrix); matrix.elements[13] -= .17; stems.setMatrixAt(i, matrix); }
    this.root.add(heads, stems); this.flowers = [heads, stems];
    for (let i = 0; i < 80; i++) {
      const x = -125 + rand() * 250, z = -130 + rand() * 260;
      if (!this.clearForTrees(x, z)) continue;
      this.ball(x, .3, z, new THREE.Vector3(1 + rand() * 1.5, .8 + rand(), 1 + rand()), '#a3ae93');
    }
  }
  makeGrass() {
    const geo = new THREE.ConeGeometry(.12, .6, 3); geo.translate(0, .3, 0);
    const grass = new THREE.InstancedMesh(geo, this.material('#889f51'), 11000);
    const dummy = new THREE.Object3D(); const rand = seededRandom(123);
    for (let i = 0; i < 11000; i++) {
      let x: number, z: number;
      do { x = -135 + rand() * 270; z = -135 + rand() * 270; } while (!this.clearForTrees(x, z));
      dummy.position.set(x, 0, z); dummy.rotation.y = rand() * 6; dummy.scale.setScalar(.6 + rand()); dummy.updateMatrix(); grass.setMatrixAt(i, dummy.matrix);
    }
    this.root.add(grass); return grass;
  }
  setQuality(q: Quality) {
    const p = PRESETS[q];
    // Keep trunks visible wherever their colliders exist; vary crown complexity.
    this.trees[0].count = 220;
    this.trees[1].count = q === 'low' ? 220 : q === 'medium' ? 440 : 660;
    this.trees[2].visible = q !== 'low';
    this.flowers.forEach(m => { m.count = p.flowers; }); this.grass.count = p.grass;
    this.particles.geometry.setDrawRange(0, p.particles);
  }
  batchStatic() {
    this.root.updateMatrixWorld(true);
    const batches = new Map<THREE.Material, THREE.BufferGeometry[]>();
    const dynamic = new Set<THREE.Object3D>([...this.flags, this.water, this.ripples, this.particles]);
    this.root.traverse(object => {
      if (!(object instanceof THREE.Mesh) || object instanceof THREE.InstancedMesh || dynamic.has(object) || Array.isArray(object.material)) return;
      const geometry = object.geometry.clone().applyMatrix4(object.matrixWorld);
      // These primitives all have positions/normals/UVs, allowing safe batching.
      if (!geometry.index) geometry.setIndex(Array.from({ length: geometry.getAttribute('position').count }, (_, i) => i));
      const bucket = batches.get(object.material) ?? []; bucket.push(geometry); batches.set(object.material, bucket);
      object.visible = false; object.matrixAutoUpdate = false;
      // Original invisible meshes remain in place for precise camera raycasts.
    });
    for (const [material, geometries] of batches) {
      const geometry = mergeGeometries(geometries);
      if (geometry) { const batch = new THREE.Mesh(geometry, material); batch.castShadow = true; batch.receiveShadow = true; this.root.add(batch); }
      geometries.forEach(g => g.dispose());
    }
  }
  update(t: number) {
    const positions = this.water.geometry.getAttribute('position');
    for (let i = 0; i < positions.count; i++) positions.setY(i, Math.sin(positions.getX(i) * .3 + t) * .05 + Math.cos(positions.getZ(i) * .7 + t * .8) * .035);
    positions.needsUpdate = true;
    const dummy = new THREE.Object3D();
    for (let i = 0; i < 180; i++) {
      dummy.position.set(((i * 29.71 + t * .7) % 290) - 145, -.54 + Math.sin(t + i) * .025, 21 + (i * 7.13) % 22);
      dummy.rotation.set(-Math.PI / 2, 0, .1); dummy.scale.set(.5 + Math.sin(i) * .3, .025, 1); dummy.updateMatrix(); this.ripples.setMatrixAt(i, dummy.matrix);
    }
    this.ripples.instanceMatrix.needsUpdate = true;
    this.flags.forEach((flag, n) => {
      const pos = flag.geometry.getAttribute('position');
      for (let i = 0; i < pos.count; i++) pos.setZ(i, Math.sin(pos.getX(i) * 2 - t * 3 + n) * .16 * Math.max(0, pos.getX(i)));
      pos.needsUpdate = true;
    });
    this.dragon.position.set(Math.sin(t * .065) * 55, 65 + Math.sin(t * .2) * 4, -95 + Math.cos(t * .065) * 40);
    this.dragon.rotation.set(0, t * .065 + Math.PI / 2, Math.sin(t * .13) * .12);
    this.leftWing.rotation.z = Math.sin(t * 2) * .32; this.rightWing.rotation.z = -Math.sin(t * 2) * .32;
    this.clouds.position.x = Math.sin(t * .01) * 8;
    this.particles.rotation.y = Math.sin(t * .13) * .015;
    (this.particles.material as THREE.PointsMaterial).opacity = .55 + Math.sin(t * 1.5) * .2;
  }
}
