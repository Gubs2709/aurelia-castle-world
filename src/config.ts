export type Quality = 'low' | 'medium' | 'high';
export const PRESETS = {
  low: { scale: .8, pixelRatio: 1, shadow: 0, trees: 100, flowers: 1800, grass: 3000, far: 460, particles: 60 },
  medium: { scale: 1, pixelRatio: 1.25, shadow: 1024, trees: 160, flowers: 3500, grass: 6500, far: 600, particles: 140 },
  high: { scale: 1, pixelRatio: 1.5, shadow: 2048, trees: 220, flowers: 6000, grass: 11000, far: 800, particles: 240 },
} as const;
export const SPAWN = { x: 0, y: 1.1, z: 109 };
export const LANDMARKS = [
  { id: 'village', name: 'Briarwood Village', subtitle: 'Where every journey begins', x: 0, z: 79, radius: 13 },
  { id: 'bridge', name: 'The Rivergate', subtitle: 'A crossing between two worlds', x: 0, z: 32, radius: 11 },
  { id: 'forest', name: 'Whispering Grove', subtitle: 'Follow the light between the leaves', x: 46, z: -13, radius: 15 },
  { id: 'castle', name: 'Aurelia Castle', subtitle: 'The heart of the valley', x: 0, z: -76, radius: 16 },
] as const;
export function movementVector(forward: number, right: number, yaw: number) {
  const length = Math.hypot(forward, right) || 1;
  return { x: (-Math.sin(yaw) * forward + Math.cos(yaw) * right) / length,
    z: (-Math.cos(yaw) * forward - Math.sin(yaw) * right) / length };
}
export function seededRandom(seed: number) {
  return () => { seed = (seed * 1664525 + 1013904223) >>> 0; return seed / 4294967296; };
}
