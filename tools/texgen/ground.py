from lib import *
import sys
SS = 2  # supersample


def canvas_from(arr):
    return Image.fromarray((np.clip(arr, 0, 1) * 255).astype(np.uint8), 'RGB').resize((S * SS, S * SS), Image.BICUBIC)


def wrap_offsets(x, y, ext):
    W = S * SS
    xs = [0]; ys = [0]
    if x - ext < 0: xs.append(W)
    if x + ext > W: xs.append(-W)
    if y - ext < 0: ys.append(W)
    if y + ext > W: ys.append(-W)
    return [(ox, oy) for ox in xs for oy in ys]


def grass():
    r = random.Random(5)
    low = fbm(S, 4, 4, 11); mid = fbm(S, 14, 3, 12)
    dark, midc, light, yel = hexc('#2c5a24'), hexc('#4a8a2c'), hexc('#8cc04a'), hexc('#b7cf58')
    base = lerp(lerp(dark, midc, low), light, np.clip((mid - .45) * 1.8, 0, 1) * .6)
    base = lerp(base, yel, np.clip((fbm(S, 6, 3, 13) - .62) * 2.5, 0, 1) * .5)
    im = canvas_from(base); d = ImageDraw.Draw(im)
    W = S * SS
    pal = [(30, 70, 28), (44, 92, 34), (62, 120, 40), (90, 150, 52), (126, 178, 64), (170, 205, 84), (36, 80, 30)]
    for i in range(52000):
        x = r.random() * W; y = r.random() * W
        L = r.uniform(14, 44); ang = -math.pi / 2 + r.uniform(-.55, .55)
        bend = r.uniform(-.5, .5)
        pts = []
        for k in range(4):
            t = k / 3; ang2 = ang + bend * t * t
            pts.append((x + math.cos(ang2) * L * t, y + math.sin(ang2) * L * t))
        col = r.choice(pal); w = r.choice([2, 3, 3, 4])
        for ox, oy in wrap_offsets(x, y, 50):
            d.line([(px + ox, py + oy) for px, py in pts], fill=col, width=w)
    for i in range(500):
        x = r.random() * W; y = r.random() * W; s = r.uniform(7, 13)
        col = r.choice([(56, 118, 44), (72, 138, 50), (40, 96, 38)])
        for ox, oy in wrap_offsets(x, y, 40):
            for k in range(3):
                a = k * 2.094 + r.random() * .5
                cx = x + math.cos(a) * s * .8 + ox; cy = y + math.sin(a) * s * .8 + oy
                d.ellipse([cx - s * .7, cy - s * .7, cx + s * .7, cy + s * .7], fill=col)
    fl = [(255, 255, 240), (255, 238, 120), (250, 190, 215), (200, 190, 250), (255, 255, 255)]
    for i in range(520):
        x = r.random() * W; y = r.random() * W; s = r.uniform(3, 6); col = r.choice(fl)
        for ox, oy in wrap_offsets(x, y, 20):
            d.ellipse([x + ox - s, y + oy - s, x + ox + s, y + oy + s], fill=col)
            d.ellipse([x + ox - s * .35, y + oy - s * .35, x + ox + s * .35, y + oy + s * .35], fill=(240, 190, 50))
    im = im.resize((S, S), Image.LANCZOS)
    a = np.asarray(im) / 255.0
    a = a * (.9 + .2 * fbm(S, 3, 2, 99)[..., None])
    save(a, 'ground_grass.png')
    h = np.asarray(im.convert('L')) / 255.0
    save(normal_from_height(ndi.gaussian_filter(h, 1.0, mode='wrap'), 2.0), 'ground_grass_n.png')


def maple_poly(cx, cy, s, rot):
    pts = []
    lobes = [(0, 1.0), (72, .82), (144, .62), (216, .62), (288, .82)]
    for k in range(60):
        ang = k * 6
        rad = .34
        for la, ls in lobes:
            da = ((ang - la + 180) % 360) - 180
            rad = max(rad, ls * math.exp(-(da / 27) ** 2) * (1 + .12 * math.sin(da * .55)))
        a = math.radians(ang) + rot
        pts.append((cx + math.cos(a) * rad * s, cy + math.sin(a) * rad * s))
    return pts


def leaf_litter():
    r = random.Random(8)
    low = fbm(S, 5, 4, 21)
    base = lerp(hexc('#3a2a1a'), hexc('#6d4a22'), low)
    base = lerp(base, hexc('#3d5a26'), np.clip((fbm(S, 7, 3, 22) - .6) * 3, 0, 1) * .5)
    im = canvas_from(base); d = ImageDraw.Draw(im); W = S * SS
    cols = [(200, 48, 28), (224, 84, 28), (238, 128, 36), (244, 176, 48), (170, 40, 30), (214, 110, 30), (120, 140, 40), (236, 200, 70), (150, 70, 26)]
    for i in range(2300):
        x = r.random() * W; y = r.random() * W; s = r.uniform(30, 74); rot = r.random() * 6.28
        c = r.choice(cols); sh = tuple(int(v * .55) for v in c)
        for ox, oy in wrap_offsets(x, y, 80):
            d.polygon(maple_poly(x + ox + 4, y + oy + 5, s, rot), fill=(30, 18, 10))
            d.polygon(maple_poly(x + ox, y + oy, s, rot), fill=c)
            d.line([(x + ox, y + oy), (x + ox + math.cos(rot) * s, y + oy + math.sin(rot) * s)], fill=sh, width=2)
            for la in (72, -72, 144, -144):
                a = rot + math.radians(la)
                d.line([(x + ox, y + oy), (x + ox + math.cos(a) * s * .6, y + oy + math.sin(a) * s * .6)], fill=sh, width=1)
    for i in range(160):
        x = r.random() * W; y = r.random() * W; L = r.uniform(40, 130); a = r.random() * 6.28
        for ox, oy in wrap_offsets(x, y, 140):
            d.line([(x + ox, y + oy), (x + ox + math.cos(a) * L, y + oy + math.sin(a) * L)], fill=(62, 40, 24), width=3)
    im = im.resize((S, S), Image.LANCZOS)
    save(np.asarray(im) / 255.0, 'ground_leaves.png')
    h = np.asarray(im.convert('L')) / 255.0
    save(normal_from_height(ndi.gaussian_filter(h, 1.2, mode='wrap'), 2.5), 'ground_leaves_n.png')


def dirt():
    r = random.Random(3)
    low = fbm(S, 5, 4, 31); fine = fbm(S, 60, 2, 32)
    base = lerp(hexc('#8a6d48'), hexc('#c8a874'), low)
    base = base * (.9 + .2 * fine[..., None])
    im = canvas_from(base); d = ImageDraw.Draw(im); W = S * SS
    for i in range(1400):
        x = r.random() * W; y = r.random() * W; s = r.uniform(4, 15)
        c = r.choice([(150, 138, 120), (120, 106, 90), (176, 160, 138), (98, 86, 72)])
        for ox, oy in wrap_offsets(x, y, 20):
            d.ellipse([x + ox - s + 2, y + oy - s + 3, x + ox + s + 2, y + oy + s + 3], fill=(60, 46, 32))
            d.ellipse([x + ox - s, y + oy - s * .8, x + ox + s, y + oy + s * .8], fill=c)
    im = im.resize((S, S), Image.LANCZOS)
    save(np.asarray(im) / 255.0, 'ground_dirt.png')
    save(normal_from_height(ndi.gaussian_filter(np.asarray(im.convert('L')) / 255.0, 1.0, mode='wrap'), 2.5), 'ground_dirt_n.png')


def flagstone():
    d1, edge, idx, rr = voronoi(S, 70, 41)
    h = np.clip(edge / 9.0, 0, 1) ** .6
    cell = rr[idx]
    c1, c2 = hexc('#7d7a73'), hexc('#b0a999')
    base = lerp(c1, c2, cell) * (.82 + .3 * fbm(S, 30, 3, 42)[..., None])
    base = base * (.8 + .25 * h[..., None])
    gap = np.clip(1 - edge / 5.0, 0, 1)
    moss = np.clip((fbm(S, 12, 4, 43) - .42) * 3, 0, 1) * np.clip(gap * 1.2 + .3, 0, 1)
    base = lerp(base, hexc('#3d6a2c'), (moss * .75))
    base = lerp(base, hexc('#2a2a24'), (gap * (1 - moss * .6)))
    base *= (.94 + .12 * fbm(S, 90, 2, 44)[..., None])
    save(base, 'path_flagstone.png')
    save(normal_from_height(h * .8 + fbm(S, 50, 3, 45) * .15, 4.0), 'path_flagstone_n.png')


def cobble():
    d1, edge, idx, rr = voronoi(S, 220, 51)
    h = np.clip(edge / 6.0, 0, 1) ** .5
    cell = rr[idx]
    base = lerp(hexc('#6e6a62'), hexc('#a79f8c'), cell) * (.85 + .3 * fbm(S, 40, 3, 52)[..., None])
    base *= (.7 + .4 * h[..., None])
    gap = np.clip(1 - edge / 3.0, 0, 1)
    base = lerp(base, hexc('#34302a'), gap)
    save(base, 'path_cobble.png'); save(normal_from_height(h, 3.5), 'path_cobble_n.png')


if __name__ == '__main__':
    which = sys.argv[1:] or ['grass', 'leaf', 'dirt', 'flag', 'cobble']
    if 'grass' in which: grass()
    if 'leaf' in which: leaf_litter()
    if 'dirt' in which: dirt()
    if 'flag' in which: flagstone()
    if 'cobble' in which: cobble()
    contact(['ground_grass.png', 'ground_leaves.png', 'ground_dirt.png', 'path_flagstone.png', 'path_cobble.png'], 'sheet_ground.png', 3, 400)
