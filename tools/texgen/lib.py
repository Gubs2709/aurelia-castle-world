import numpy as np, math, random
from PIL import Image, ImageDraw, ImageFilter
from scipy import ndimage as ndi

S = 1024
OUT = r"D:/GAME/AureliaUnity/Assets/Art/Gen/"

def rng(seed): return np.random.default_rng(seed)

def noise(size, cells, seed, aspect=(1, 1)):
    """tileable smooth noise in [0,1]; cells=(cy,cx) lattice counts"""
    r = rng(seed)
    cy, cx = (cells if isinstance(cells, tuple) else (cells, cells))
    g = r.random((cy, cx))
    z = ndi.zoom(g, (size / cy, size / cx), order=3, mode='grid-wrap')
    z = z[:size, :size]
    return np.clip(z, 0, 1)

def fbm(size, base, octaves, seed, pers=.5, stretch=(1, 1)):
    tot = np.zeros((size, size)); amp = 1; norm = 0
    for o in range(octaves):
        cy = max(2, int(base * stretch[0] * 2 ** o)); cx = max(2, int(base * stretch[1] * 2 ** o))
        tot += amp * noise(size, (cy, cx), seed + o * 17)
        norm += amp; amp *= pers
    t = tot / norm
    t = (t - t.min()) / (t.max() - t.min() + 1e-9)
    return t

def norm01(a):
    return (a - a.min()) / (a.max() - a.min() + 1e-9)

def lerp(a, b, t):
    t = np.asarray(t)
    if t.ndim == 2: t = t[..., None]
    return a + (b - a) * t

def hexc(h):
    h = h.lstrip('#'); return np.array([int(h[i:i + 2], 16) for i in (0, 2, 4)]) / 255.0

def normal_from_height(h, strength=3.0):
    dx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * strength
    dy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * strength
    n = np.stack([-dx, dy, np.ones_like(h) * 1.0], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return n * .5 + .5

def save(arr, name, alpha=None):
    a = np.clip(arr, 0, 1)
    a = (a * 255).astype(np.uint8)
    if alpha is not None:
        a = np.dstack([a, (np.clip(alpha, 0, 1) * 255).astype(np.uint8)])
        Image.fromarray(a, 'RGBA').save(OUT + name)
    else:
        Image.fromarray(a, 'RGB').save(OUT + name)

def voronoi(size, n, seed, relax=0):
    """tileable voronoi: returns (dist_to_nearest, dist2-dist1 edge, cell_id, cell_random)"""
    r = rng(seed)
    pts = r.random((n, 2)) * size
    ys, xs = np.mgrid[0:size, 0:size].astype(np.float32)
    d1 = np.full((size, size), 1e9, np.float32); d2 = d1.copy(); idx = np.zeros((size, size), np.int32)
    for i, (py, px) in enumerate(pts):
        dy = np.abs(ys - py); dy = np.minimum(dy, size - dy)
        dx = np.abs(xs - px); dx = np.minimum(dx, size - dx)
        d = np.sqrt(dx * dx + dy * dy)
        m = d < d1
        d2 = np.where(m, d1, np.minimum(d2, d)); idx = np.where(m, i, idx); d1 = np.where(m, d, d1)
    return d1, d2 - d1, idx, r.random(n)

def tile_draw(canvas_size, draw_fn, margin=0):
    """call draw_fn(draw, ox, oy) 9 times so shapes wrap"""
    pass

def contact(names, out='sheet.png', cols=4, cell=256):
    ims = [Image.open(OUT + n).convert('RGBA').resize((cell, cell)) for n in names]
    rows = (len(ims) + cols - 1) // cols
    sheet = Image.new('RGBA', (cols * cell, rows * cell), (60, 60, 60, 255))
    for i, im in enumerate(ims):
        bg = Image.new('RGBA', im.size, (90, 90, 90, 255)); bg.alpha_composite(im)
        sheet.paste(bg, ((i % cols) * cell, (i // cols) * cell))
    sheet.convert('RGB').save(r"D:/GAME/tools/texgen/" + out)
