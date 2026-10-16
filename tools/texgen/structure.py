from lib import *
from ground import canvas_from, wrap_offsets, SS
import sys


def bark(name, seed, base_a, base_b, fissure_col, moss=0.0, warm=0.0):
    # vertical ridges: stretch noise vertically
    ridge = fbm(S, 6, 5, seed, .55, stretch=(.18, 1.0))
    ridge2 = fbm(S, 12, 4, seed + 5, .5, stretch=(.1, 1.0))
    h = ridge * .6 + ridge2 * .4
    # fissures where h is low band
    fis = np.clip(1 - np.abs(h - .38) / .08, 0, 1) ** 1.5
    plates = fbm(S, 8, 3, seed + 9, .5, stretch=(.35, 1))
    col = lerp(hexc(base_a), hexc(base_b), plates)
    col = col * (.62 + .55 * h[..., None])
    col = lerp(col, hexc(fissure_col), fis * .85)
    # warm sun streaks
    if warm > 0:
        col = lerp(col, hexc('#b8895a'), np.clip((ridge2 - .6) * 3, 0, 1) * warm)
    if moss > 0:
        m = np.clip((fbm(S, 5, 4, seed + 31, .5, stretch=(.5, 1)) - .55) * 3.0, 0, 1) * moss
        col = lerp(col, hexc('#587a34') * (.7 + .5 * ridge2[..., None]), m)
    save(col, name + '.png')
    save(normal_from_height(h * 1.0 + fis * -.3, 7.0), name + '_n.png')


def birch():
    h = fbm(S, 5, 3, 71)
    col = lerp(hexc('#d8d4c8'), hexc('#efece2'), fbm(S, 8, 3, 72))
    col = col * (.88 + .16 * fbm(S, 30, 3, 73, stretch=(.2, 1))[..., None])
    im = canvas_from(col); d = ImageDraw.Draw(im); r = random.Random(7); W = S * SS
    for i in range(160):
        x = r.random() * W; y = r.random() * W; w = r.uniform(40, 220); t = r.uniform(5, 16)
        for ox, oy in wrap_offsets(x, y, 230):
            d.ellipse([x + ox - w / 2, y + oy - t / 2, x + ox + w / 2, y + oy + t / 2], fill=(34, 30, 28))
    im = im.resize((S, S), Image.LANCZOS)
    save(np.asarray(im) / 255.0, 'bark_birch.png')
    save(normal_from_height(ndi.gaussian_filter(np.asarray(im.convert('L')) / 255.0, 1.5, mode='wrap'), 3.0), 'bark_birch_n.png')


def plaster():
    low = fbm(S, 4, 4, 81); fine = fbm(S, 70, 3, 82)
    col = lerp(hexc('#e9dfc6'), hexc('#f6efdb'), low)
    # damp stains near the bottom is applied via vertex color in the mesh; here only mottling
    col = col * (.93 + .1 * fine[..., None])
    col = lerp(col, hexc('#cdbf9d'), np.clip((fbm(S, 9, 4, 83) - .62) * 3, 0, 1) * .5)
    save(col, 'wall_plaster.png')
    save(normal_from_height(ndi.gaussian_filter(fine, .8, mode='wrap') * .6 + low * .3, 2.5), 'wall_plaster_n.png')


def roof_tiles(name, seed, cols, rows, palette, curved=True, wood=False):
    # rows of tiles with staggered offset
    h = np.zeros((S, S)); col = np.zeros((S, S, 3))
    ys, xs = np.mgrid[0:S, 0:S].astype(np.float32)
    th = S / rows; tw = S / cols
    ri = np.floor(ys / th); yi = (ys / th) % 1
    off = (ri % 2) * .5
    xx = xs / tw + off; ci = np.floor(xx); xi = xx % 1
    r = rng(seed)
    rid = ri.astype(int) % rows; cid = ci.astype(int) % cols
    rnd = r.random((rows, cols * 2 + 2))
    cv = rnd[rid, cid]
    pal = np.array([hexc(p) for p in palette])
    pick = (cv * len(pal)).astype(int) % len(pal)
    c = pal[pick] * (.86 + .24 * r.random((rows, cols * 2 + 2))[rid, cid][..., None])
    if curved:
        prof = np.sin(xi * math.pi) ** .6      # tile barrel curvature
        shade = .62 + .5 * prof
        # overlap shadow near the top edge of each row
        under = np.clip(yi / .22, 0, 1)
        shade = shade * (.55 + .45 * under) * (1 - .25 * (yi > .96))
    else:
        prof = np.ones_like(xi)
        under = np.clip(yi / .12, 0, 1)
        shade = (.6 + .4 * under) * (1 - .0)
        edge = np.clip(np.minimum(xi, 1 - xi) / .03, 0, 1)
        shade = shade * (.6 + .4 * edge)
    mott = fbm(S, 25, 4, seed + 4)
    col = c * shade[..., None] * (.84 + .3 * mott[..., None])
    if wood:
        grain = fbm(S, 60, 3, seed + 6, stretch=(.1, 1))
        col = col * (.8 + .4 * grain[..., None])
    # moss streaks in the lower rows
    m = np.clip((fbm(S, 6, 4, seed + 8) - .6) * 3, 0, 1) * .35
    col = lerp(col, hexc('#58783a'), m)
    save(col, name + '.png')
    hh = prof * under * .6 + (1 - yi) * 0.0
    save(normal_from_height(ndi.gaussian_filter(hh, .8, mode='wrap'), 4.0), name + '_n.png')


def planks(name, seed, a, b, vertical=True):
    gr = fbm(S, 40, 4, seed, stretch=((.06, 1) if vertical else (1, .06)))
    n = 9
    ys, xs = np.mgrid[0:S, 0:S]
    pos = xs if vertical else ys
    pid = (pos * n // S).astype(int)
    r = rng(seed + 2).random(n)
    tone = r[pid]
    col = lerp(hexc(a), hexc(b), tone * .7 + gr * .3) * (.75 + .5 * gr[..., None])
    edge = np.abs(((pos * n / S) % 1) - .5) * 2
    col = col * (1 - .55 * (edge > .96))[..., None]
    save(col, name + '.png')
    save(normal_from_height(gr * .7 - (edge > .96) * .5, 4.0), name + '_n.png')


def stone_wall():
    # coursed rubble
    rows = 9
    ys, xs = np.mgrid[0:S, 0:S].astype(np.float32)
    ri = np.floor(ys / (S / rows)).astype(int); yi = (ys / (S / rows)) % 1
    r = rng(91)
    widths = r.uniform(.8, 1.7, (rows, 12))
    col = np.zeros((S, S, 3)); hh = np.zeros((S, S))
    for row in range(rows):
        edges = np.cumsum(widths[row]); edges = edges / edges[-1] * S
        starts = np.concatenate([[0], edges[:-1]])
        for k in range(12):
            m = (ri == row) & (xs >= starts[k]) & (xs < edges[k])
            tone = r.random()
            c = lerp(hexc('#8d8a80'), hexc('#c9c0a8'), tone)
            lx = (xs - starts[k]) / (edges[k] - starts[k]);
            bevel = np.clip(np.minimum.reduce([lx, 1 - lx, yi, 1 - yi]) / .1, 0, 1)
            shade = (.55 + .45 * bevel)
            col[m] = (c * shade[m][..., None])
            hh[m] = bevel[m]
    col *= (.85 + .3 * fbm(S, 30, 4, 92)[..., None])
    save(col, 'wall_stone.png'); save(normal_from_height(hh, 4.5), 'wall_stone_n.png')


if __name__ == '__main__':
    which = sys.argv[1:] or ['bark', 'plaster', 'roof', 'planks', 'stone']
    if 'bark' in which:
        bark('bark_oak', 101, '#6a503a', '#9a7a58', '#2a1c12', moss=.5, warm=.5)
        bark('bark_dark', 111, '#54402f', '#7e6650', '#20150e', moss=.35, warm=.3)
        bark('bark_cherry', 121, '#6e4a3c', '#9a6e58', '#2a1612', moss=.2, warm=.4)
        birch()
    if 'plaster' in which: plaster()
    if 'roof' in which:
        roof_tiles('roof_terracotta', 131, 10, 12, ['#b5522f', '#c8663a', '#a8452a', '#d27a45', '#9c4a30'], True)
        roof_tiles('roof_slate', 141, 8, 14, ['#4f5d6b', '#5f6e7c', '#46525f', '#6c7b88', '#566a72'], False)
        roof_tiles('roof_shingle', 151, 12, 16, ['#6a5238', '#7c6244', '#5a4430', '#8a7048'], False, wood=True)
        roof_tiles('roof_teal', 161, 12, 16, ['#2f8f92', '#3aa3a4', '#287f86', '#46b0aa', '#348a98'], True)
    if 'planks' in which:
        planks('wood_beam', 171, '#3a2618', '#5a3e28', True)
        planks('wood_light', 181, '#8a6a46', '#b08c5e', True)
    if 'stone' in which: stone_wall()
    contact(['bark_oak.png', 'bark_dark.png', 'bark_cherry.png', 'bark_birch.png', 'wall_plaster.png', 'roof_terracotta.png', 'roof_slate.png', 'roof_shingle.png', 'roof_teal.png', 'wood_beam.png', 'wood_light.png', 'wall_stone.png'], 'sheet_struct.png', 4, 300)
