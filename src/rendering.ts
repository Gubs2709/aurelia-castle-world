import * as THREE from 'three';
import { PRESETS, type Quality } from './config';

export class Rendering {
  scene = new THREE.Scene();
  camera = new THREE.PerspectiveCamera(57, innerWidth / innerHeight, .15, 460);
  renderer: THREE.WebGLRenderer;
  sun = new THREE.DirectionalLight('#fff0ce', 3.1);
  quality: Quality = 'low';
  constructor(canvas: HTMLCanvasElement) {
    this.renderer = new THREE.WebGLRenderer({ canvas, antialias: true, powerPreference: 'high-performance' });
    this.renderer.outputColorSpace = THREE.SRGBColorSpace;
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.renderer.toneMappingExposure = 1.05;
    this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    this.scene.background = new THREE.Color('#a6d8dc');
    this.scene.fog = new THREE.Fog('#b2d5cd', 150, 470);
    this.scene.add(new THREE.HemisphereLight('#c6eaff', '#777f3a', 2.3));
    this.sun.position.set(-70, 110, 65);
    this.sun.shadow.camera.left = -95; this.sun.shadow.camera.right = 95;
    this.sun.shadow.camera.top = 95; this.sun.shadow.camera.bottom = -95;
    this.sun.shadow.camera.far = 360; this.sun.shadow.bias = -.0006;
    this.sun.shadow.normalBias = .12;
    this.scene.add(this.sun, this.sun.target);
    // A gradient sky dome keeps the horizon light and the zenith a deep blue.
    const sky = new THREE.Mesh(new THREE.SphereGeometry(700, 24, 12), new THREE.ShaderMaterial({
      side: THREE.BackSide, depthWrite: false, depthTest: false,
      uniforms: { top: { value: new THREE.Color('#4d9ec5') }, bottom: { value: new THREE.Color('#e5ead0') } },
      vertexShader: 'varying vec3 vWorld; void main(){vWorld=(modelMatrix*vec4(position,1.)).xyz;gl_Position=projectionMatrix*modelViewMatrix*vec4(position,1.);gl_Position.z=gl_Position.w;}',
      fragmentShader: 'uniform vec3 top;uniform vec3 bottom;varying vec3 vWorld;void main(){float h=max(normalize(vWorld).y,0.);gl_FragColor=vec4(mix(bottom,top,pow(h,.55)),1.); #include <tonemapping_fragment> \n #include <colorspace_fragment> }',
    }));
    sky.material.fragmentShader = sky.material.fragmentShader.replace('; #include', ';\n #include');
    sky.renderOrder = -1000; sky.frustumCulled = false; this.scene.add(sky);
    window.addEventListener('resize', () => this.resize());
    this.setQuality('low');
  }
  setQuality(quality: Quality) {
    this.quality = quality;
    const p = PRESETS[quality];
    this.renderer.setPixelRatio(Math.min(devicePixelRatio, p.pixelRatio) * p.scale);
    this.renderer.shadowMap.enabled = p.shadow > 0;
    this.sun.castShadow = p.shadow > 0;
    if (p.shadow) {
      this.sun.shadow.mapSize.set(p.shadow, p.shadow);
      this.sun.shadow.map?.dispose(); this.sun.shadow.map = null;
    }
    this.camera.far = p.far; this.camera.updateProjectionMatrix();
    this.resize();
  }
  resize() {
    this.renderer.setSize(innerWidth, innerHeight);
    this.camera.aspect = innerWidth / innerHeight; this.camera.updateProjectionMatrix();
  }
  draw() { this.renderer.render(this.scene, this.camera); }
}
