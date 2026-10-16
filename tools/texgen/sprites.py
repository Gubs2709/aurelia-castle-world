from lib import *
import sys
SZ = 1024
SS = 2


def shade(c, k):
    return tuple(max(0, min(255, int(v * k))) for v in c)


def leaf_shape(kind, L, W):
    """polygon points of a leaf pointing +x, base at origin, length L"""
    pts_top = []
    N = 24
    for i in range(N + 1):
        t = i / N
        if kind == 'oval':
            w = math.sin(math.pi * t ** .8) * W
        elif kind == 'oak':
            w = math.sin(math.pi * t ** .85) * W * (.72 + .28 * math.cos(t * 6 * math.pi))
        elif kind == 'maple':
            # palmate: approximated by wide lobes along length
            w = math.sin(math.pi * t ** .7) * W * (.55 + .45 * abs(math.cos(t * 3.2 * math.pi)))
        elif kind == 'willow':
            w = math.sin(math.pi * t ** .9) * W * .45
        elif kind == 'heart':
            w = math.sin(math.pi * t ** .6) * W * (1 - .25 * t)
        else:
            w = math.sin(math.pi * t) * W
        pts_top.append((t * L, w))
    return pts_top + [(x, -y) for x, y in reversed(pts_top)]


def draw_leaf(d, x, y, ang, L, W, col, kind='oval', vein=True, spec=True):
    ca, sa = math.cos(ang), math.sin(ang)
    base = leaf_shape(kind, L, W)
    pts = [(x + px * ca - py * sa, y + px * sa + py * ca) for px, py in base]
    d.polygon(pts, fill=col)
    # light half
    half = [(x + px * ca - py * sa, y + px * sa + py * ca) for px, py in base if py >= 0]
    d.polygon(half, fill=shade(col, 1.12))
    # edge darkening
    d.line(pts + [pts[0]], fill=shade(col, .62), width=max(1, int(L / 40)))
    if vein:
        d.line([(x, y), (x + ca * L * .96, y + sa * L * .96)], fill=shade(col, .7), width=max(1, int(L / 55)))
        for k in range(1, 6):
            t = k / 6.5
            for s in (1, -1):
                a2 = ang + s * .85
                l2 = W * .85 * math.sin(math.pi * t) * 1.0
                bx, by = x + ca * L * t, y + sa * L * t
                d.line([(bx, by), (bx + math.cos(a2) * l2, by + math.sin(a2) * l2)], fill=shade(col, .78), width=max(1, int(L / 90)))


def branch_cluster(name, seed, palette, kind, leaves=260, lw=(.07, .11), spread=1.0, twig_col=(70, 48, 32), droop=0.0, count_scale=1.0):
    r = random.Random(seed)
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    # main limb from bottom-left toward upper-right with sub twigs
    twigs = []
    def twig(x, y, ang, L, depth):
        n = 8
        px, py = x, y
        for i in range(n):
            a = ang + r.uniform(-.18, .18) + droop * .08
            nx, ny = px + math.cos(a) * L / n, py + math.sin(a) * L / n
            wdt = max(2, int((1 - i / n) * (14 - depth * 4)))
            d.line([(px, py), (nx, ny)], fill=twig_col, width=wdt)
            twigs.append((nx, ny, a, depth))
            if depth < 2 and r.random() < .55:
                twig(nx, ny, a + r.choice([-1, 1]) * r.uniform(.5, 1.0), L * r.uniform(.35, .55), depth + 1)
            px, py = nx, ny
    twig(W * .12, W * .88, -math.pi / 4 + r.uniform(-.1, .1), W * .78, 0)
    twig(W * .12, W * .88, -math.pi / 4 - .5, W * .5, 1)
    twig(W * .12, W * .88, -math.pi / 4 + .45, W * .5, 1)
    # leaves: sorted back to front by y for layering
    items = []
    n = int(leaves * count_scale)
    for i in range(n):
        tx, ty, ta, dep = r.choice(twigs)
        ox = r.gauss(0, 60 * spread); oy = r.gauss(0, 60 * spread)
        ang = ta + r.uniform(-1.6, 1.6) + (math.pi / 2 * droop)
        L = W * r.uniform(*lw) * r.uniform(.8, 1.35)
        items.append((ty + oy, tx + ox, ang, L))
    items.sort()
    for _, x, ang_y, ang, L in [(a, b, 0, c, e) for a, b, c, e in [(i[0], i[1], i[2], i[3]) for i in items]][:0]:
        pass
    for y, x, ang, L in items:
        col = r.choice(palette)
        jitter = r.uniform(.85, 1.15)
        col = shade(col, jitter)
        draw_leaf(d, x, y, ang, L, L * (.38 if kind != 'willow' else .2), col, kind)
    im = im.resize((SZ, SZ), Image.LANCZOS)
    im.save(OUT + name + '.png')


def blossom_cluster(name, seed, petal, centre, extra, count=420):
    r = random.Random(seed)
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    twigs = []
    def twig(x, y, ang, L, depth):
        n = 9; px, py = x, y
        for i in range(n):
            a = ang + r.uniform(-.2, .2)
            nx, ny = px + math.cos(a) * L / n, py + math.sin(a) * L / n
            d.line([(px, py), (nx, ny)], fill=(66, 42, 36), width=max(2, int((1 - i / n) * (13 - depth * 4))))
            twigs.append((nx, ny))
            if depth < 2 and r.random() < .6: twig(nx, ny, a + r.choice([-1, 1]) * r.uniform(.5, 1.0), L * r.uniform(.35, .55), depth + 1)
            px, py = nx, ny
    twig(W * .1, W * .9, -math.pi / 4, W * .85, 0); twig(W * .1, W * .9, -math.pi / 4 - .55, W * .5, 1); twig(W * .1, W * .9, -math.pi / 4 + .5, W * .5, 1)
    items = []
    for i in range(count):
        tx, ty = r.choice(twigs)
        items.append((ty + r.gauss(0, 55), tx + r.gauss(0, 55), r.uniform(.8, 1.4)))
    items.sort()
    for y, x, s in items:
        if r.random() < .22:
            col = shade(r.choice(extra), r.uniform(.9, 1.1)); L = 44 * s
            draw_leaf(d, x, y, r.uniform(0, 6.28), L, L * .4, col, 'oval', False)
            continue
        pc = shade(r.choice(petal), r.uniform(.92, 1.08)); R = 30 * s; rot = r.uniform(0, 6.28)
        for k in range(5):
            a = rot + k * 2 * math.pi / 5
            cx, cy = x + math.cos(a) * R * .62, y + math.sin(a) * R * .62
            d.ellipse([cx - R * .55, cy - R * .55, cx + R * .55, cy + R * .55], fill=pc)
            d.ellipse([cx - R * .3, cy - R * .5, cx + R * .15, cy], fill=shade(pc, 1.1))
        d.ellipse([x - R * .22, y - R * .22, x + R * .22, y + R * .22], fill=centre)
    im = im.resize((SZ, SZ), Image.LANCZOS)
    im.save(OUT + name + '.png')


def wisteria(name, seed):
    r = random.Random(seed)
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    d.line([(0, W * .06), (W, W * .06)], fill=(70, 48, 34), width=22)
    cols = [(150, 110, 210), (176, 136, 226), (200, 168, 240), (128, 92, 190), (214, 190, 245)]
    for i in range(26):
        x = (i + r.random() * .6) / 26 * W; L = r.uniform(.45, .92) * W
        d.line([(x, W * .06), (x + r.uniform(-14, 14), W * .06 + L)], fill=(84, 112, 60), width=5)
        n = int(L / 15)
        for k in range(n):
            t = k / n; y = W * .06 + 26 + t * (L - 26)
            wdt = (1 - t * .85) * 52
            for j in range(3):
                bx = x + r.gauss(0, wdt * .5); by = y + r.gauss(0, 7)
                s = r.uniform(11, 20) * (1 - t * .5)
                c = shade(r.choice(cols), r.uniform(.9, 1.12))
                d.ellipse([bx - s, by - s * .8, bx + s, by + s * .8], fill=c)
        for k in range(4):
            lx = x + r.uniform(-30, 30); ly = W * .06 + r.uniform(0, 120)
            draw_leaf(d, lx, ly, r.uniform(.6, 2.5), 80, 28, shade((86, 128, 56), r.uniform(.8, 1.2)), 'oval')
    im = im.resize((SZ, SZ), Image.LANCZOS); im.save(OUT + name + '.png')


def fern(name, seed):
    r = random.Random(seed)
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    for f in range(9):
        a0 = -math.pi / 2 + (f - 4) * .28 + r.uniform(-.05, .05)
        L = W * r.uniform(.62, .9)
        pts = []
        for i in range(60):
            t = i / 59
            a = a0 + (f - 4) * .1 * t + .35 * t * t * (1 if f > 4 else -1) * .6
            pts.append((W / 2 + math.cos(a) * L * t * 1.0, W * .96 + math.sin(a) * L * t))
        d.line(pts, fill=(60, 100, 40), width=7)
        for i in range(6, 58, 1):
            t = i / 59; px, py = pts[i]; a = a0 + (f - 4) * .1 * t
            for s in (1, -1):
                pa = a + s * 1.15
                pl = (math.sin(math.pi * min(1, t * 1.15)) ** .7) * W * .11 + 8
                col = shade((54, 122, 52), r.uniform(.85, 1.2))
                d.polygon([(px, py), (px + math.cos(pa - .12 * s) * pl, py + math.sin(pa - .12 * s) * pl), (px + math.cos(pa) * pl * 1.05, py + math.sin(pa) * pl * 1.05), (px + math.cos(pa + .12 * s) * pl * .8, py + math.sin(pa + .12 * s) * pl * .8)], fill=col)
    im = im.resize((SZ, SZ), Image.LANCZOS); im.save(OUT + name + '.png')


def grass_card(name, seed, tall=1.0, dry=False):
    r = random.Random(seed)
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    pal = [(44, 100, 40), (62, 128, 46), (86, 156, 54), (122, 180, 66), (160, 200, 84)]
    if dry: pal = [(120, 120, 60), (150, 150, 74), (176, 168, 90), (110, 100, 54)]
    for i in range(70):
        bx = W * (.15 + .7 * r.random()); L = W * r.uniform(.3, .95) * tall
        lean = r.uniform(-.55, .55); bend = r.uniform(-.6, .6)
        n = 10; left = []; right = []
        for k in range(n + 1):
            t = k / n; a = -math.pi / 2 + lean * .35 + bend * t * t * .8
            cx = bx + math.sin(lean + bend * t * t) * L * t * .6; cy = W * .98 - L * t * (1 - .2 * t)
            wd = (1 - t) ** .8 * W * .014 + 1.5
            left.append((cx - wd, cy)); right.append((cx + wd, cy))
        col = shade(r.choice(pal), r.uniform(.85, 1.15))
        d.polygon(left + list(reversed(right)), fill=col)
        d.line([(left[k][0] * .5 + right[k][0] * .5, left[k][1]) for k in range(n + 1)], fill=shade(col, 1.25), width=2)
    im = im.resize((SZ, SZ), Image.LANCZOS); im.save(OUT + name + '.png')


def flower_card(name, seed, kind, petal_cols):
    r = random.Random(seed)
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    stems = 5 if kind != 'lavender' else 7
    for s in range(stems):
        sx = W * (.2 + .6 * (s + r.random() * .5) / stems); top = W * r.uniform(.08, .5)
        lean = r.uniform(-.1, .1) * W
        d.line([(sx, W * .98), (sx + lean * .3, W * .7), (sx + lean, top)], fill=(58, 112, 44), width=10)
        for k in range(3):
            ly = W * r.uniform(.55, .9); side = r.choice([-1, 1])
            draw_leaf(d, sx + lean * .3, ly, (-math.pi / 2 + side * 1.0), W * .17, W * .045, shade((60, 128, 48), r.uniform(.8, 1.2)), 'willow')
        pc = r.choice(petal_cols); cx, cy = sx + lean, top
        if kind == 'daisy':
            R = W * .075
            for k in range(14):
                a = k * 2 * math.pi / 14
                d.ellipse([cx + math.cos(a) * R * .8 - R * .35, cy + math.sin(a) * R * .8 - R * .2, cx + math.cos(a) * R * .8 + R * .35, cy + math.sin(a) * R * .8 + R * .2], fill=(250, 250, 244))
                draw_leaf(d, cx, cy, a, R * 1.15, R * .26, (252, 252, 246), 'oval', False)
            d.ellipse([cx - R * .35, cy - R * .35, cx + R * .35, cy + R * .35], fill=(244, 196, 44))
        elif kind == 'poppy':
            R = W * .09
            for k in range(4):
                a = k * math.pi / 2 + .4
                draw_leaf(d, cx, cy, a, R * 1.1, R * .62, shade(pc, r.uniform(.92, 1.08)), 'heart', False)
            d.ellipse([cx - R * .22, cy - R * .22, cx + R * .22, cy + R * .22], fill=(36, 30, 24))
        elif kind == 'tulip':
            R = W * .075
            for k in range(3):
                a = -math.pi / 2 + (k - 1) * .5
                draw_leaf(d, cx, cy + R * 1.4, a, R * 2.2, R * .8, shade(pc, 1 + (k - 1) * .06), 'oval', False)
        elif kind == 'lavender':
            for k in range(16):
                t = k / 16; yy = cy + t * W * .24
                for sd in (-1, 1):
                    d.ellipse([cx + sd * 10 - 9, yy - 7, cx + sd * 10 + 9, yy + 9], fill=shade(pc, r.uniform(.9, 1.15)))
        elif kind == 'bell':
            for k in range(6):
                t = k / 6; yy = cy + t * W * .22; xx = cx + math.sin(t * 3) * 22
                d.ellipse([xx - 15, yy - 10, xx + 15, yy + 18], fill=shade(pc, r.uniform(.9, 1.15)))
        else:  # generic 5 petal
            R = W * .07
            for k in range(5):
                a = k * 2 * math.pi / 5
                draw_leaf(d, cx, cy, a, R * 1.2, R * .55, shade(pc, r.uniform(.92, 1.1)), 'heart', False)
            d.ellipse([cx - R * .28, cy - R * .28, cx + R * .28, cy + R * .28], fill=(250, 214, 64))
    im = im.resize((SZ, SZ), Image.LANCZOS); im.save(OUT + name + '.png')


def rock_tex():
    # lichen-speckled grey stone, tileable
    low = fbm(S, 4, 5, 201); fine = fbm(S, 40, 4, 202)
    cracks = np.clip(1 - np.abs(fbm(S, 9, 4, 203) - .5) / .035, 0, 1)
    col = lerp(hexc('#6f6c68'), hexc('#a8a39a'), low) * (.85 + .3 * fine[..., None])
    col = lerp(col, hexc('#2a2824'), cracks * .8)
    mo = np.clip((fbm(S, 8, 4, 204) - .55) * 3, 0, 1) * np.clip(1 - low * .8, 0, 1)
    col = lerp(col, hexc('#4b7a34'), mo * .75)
    li = np.clip((fbm(S, 20, 3, 205) - .72) * 6, 0, 1)
    col = lerp(col, hexc('#c4c690'), li * .5)
    save(col, 'rock_stone.png'); save(normal_from_height(low * .5 + fine * .3 - cracks * .3, 6.0), 'rock_stone_n.png')


def reed_card(name, seed, flowers=True):
    r = random.Random(seed)
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    greens = [(52, 108, 44), (70, 130, 52), (96, 152, 60), (120, 170, 72), (88, 124, 58)]
    for i in range(46):
        bx = W * (.1 + .8 * r.random()); L = W * r.uniform(.45, .96)
        lean = r.uniform(-.25, .25); bend = r.uniform(-.35, .35)
        n = 12; left = []; right = []
        for k in range(n + 1):
            t = k / n
            cx = bx + math.sin(lean + bend * t * t) * L * t * .55
            cy = W * .985 - L * t
            wd = (1 - t) ** .7 * W * .012 + 1.5
            left.append((cx - wd, cy)); right.append((cx + wd, cy))
        col = shade(r.choice(greens), r.uniform(.85, 1.15))
        d.polygon(left + list(reversed(right)), fill=col)
    # cattails (brown) and lavender-purple spikes like the harbour reference
    for i in range(9):
        sx = W * (.15 + .7 * r.random()); top = W * r.uniform(.1, .45)
        lean = r.uniform(-.08, .08) * W
        d.line([(sx, W * .98), (sx + lean * .4, W * .6), (sx + lean, top)], fill=(86, 118, 56), width=7)
        kind = i % 3
        if kind == 0:
            d.ellipse([sx + lean - 11, top - 4, sx + lean + 11, top + 70], fill=(96, 62, 36))
        elif kind == 1 and flowers:
            for k in range(14):
                t = k / 14
                col = shade((150 + int(60 * t), 110 + int(40 * t), 210), r.uniform(.9, 1.1))
                for sd in (-1, 1):
                    d.ellipse([sx + lean + sd * 7 - 8, top + t * 150 - 8, sx + lean + sd * 7 + 8, top + t * 150 + 10], fill=col)
        else:
            for k in range(10):
                a = k * .6
                d.ellipse([sx + lean + math.cos(a) * 9 - 7, top + k * 9 - 7, sx + lean + math.cos(a) * 9 + 7, top + k * 9 + 7], fill=(236, 226, 236))
    im = im.resize((SZ, SZ), Image.LANCZOS); im.save(OUT + name + '.png')


def gull_card(name):
    W = SZ * SS
    im = Image.new('RGBA', (W, W), (0, 0, 0, 0)); d = ImageDraw.Draw(im)
    cx, cy = W / 2, W / 2
    wing = [(cx, cy), (cx - W * .46, cy - W * .16), (cx - W * .48, cy - W * .10), (cx - W * .30, cy + W * .04), (cx - W * .1, cy + W * .06)]
    d.polygon(wing, fill=(250, 250, 252))
    d.polygon([(cx - W * .46, cy - W * .16), (cx - W * .48, cy - W * .10), (cx - W * .42, cy - W * .115)], fill=(40, 44, 54))
    d.polygon([(2 * cx - x, y) for x, y in wing], fill=(250, 250, 252))
    d.polygon([(2 * cx - W * .46 + 0, cy - W * .16), (cx + W * .48, cy - W * .10), (cx + W * .42, cy - W * .115)], fill=(40, 44, 54))
    d.ellipse([cx - W * .07, cy - W * .05, cx + W * .07, cy + W * .1], fill=(252, 252, 252))
    d.ellipse([cx - W * .04, cy - W * .1, cx + W * .04, cy - W * .02], fill=(252, 252, 252))
    d.polygon([(cx - W * .01, cy - W * .075), (cx + W * .01, cy - W * .075), (cx, cy - W * .115)], fill=(240, 170, 60))
    d.polygon([(cx - W * .04, cy + W * .09), (cx + W * .04, cy + W * .09), (cx, cy + W * .22)], fill=(244, 244, 248))
    im = im.resize((SZ, SZ), Image.LANCZOS); im.save(OUT + name + '.png')


if __name__ == '__main__':
    which = sys.argv[1:] or ['leaves', 'flowers', 'misc', 'rock']
    if 'leaves' in which:
        branch_cluster('leaf_oak', 301, [(46, 110, 40), (66, 134, 46), (92, 160, 52), (126, 184, 62), (36, 88, 36)], 'oak', 300)
        branch_cluster('leaf_broad', 302, [(52, 120, 44), (78, 150, 50), (110, 176, 58), (150, 198, 72), (40, 98, 40)], 'oval', 340, lw=(.06, .09))
        branch_cluster('leaf_maple_red', 303, [(176, 28, 24), (206, 52, 28), (226, 86, 30), (150, 22, 28), (240, 120, 36)], 'maple', 280, lw=(.075, .115))
        branch_cluster('leaf_maple_orange', 304, [(230, 110, 30), (242, 148, 38), (248, 184, 52), (214, 84, 28), (236, 128, 34)], 'maple', 280, lw=(.075, .115))
        branch_cluster('leaf_gold', 305, [(214, 170, 44), (232, 196, 60), (190, 140, 36), (244, 214, 84), (170, 120, 34)], 'oval', 320, lw=(.06, .09))
        branch_cluster('leaf_crimson', 306, [(120, 16, 30), (150, 22, 34), (96, 12, 28), (176, 32, 40)], 'maple', 280, lw=(.075, .115))
        branch_cluster('leaf_willow', 307, [(90, 150, 56), (116, 174, 64), (70, 126, 48), (140, 188, 76)], 'willow', 400, lw=(.1, .16), droop=1.0)
        blossom_cluster('leaf_cherry', 311, [(250, 190, 214), (248, 160, 196), (255, 214, 228), (236, 124, 176)], (246, 214, 90), [(96, 150, 70), (118, 170, 80)])
        blossom_cluster('leaf_magenta', 312, [(220, 40, 150), (196, 28, 128), (240, 84, 176), (170, 20, 120)], (250, 220, 120), [(126, 70, 150), (150, 90, 170)])
        blossom_cluster('leaf_white', 313, [(255, 252, 248), (252, 236, 240), (255, 244, 224)], (246, 200, 80), [(110, 160, 80)])
        wisteria('leaf_wisteria', 321)
    if 'flowers' in which:
        flower_card('flower_daisy', 401, 'daisy', [(250, 250, 244)])
        flower_card('flower_poppy', 402, 'poppy', [(214, 40, 34), (232, 70, 40)])
        flower_card('flower_tulip', 403, 'tulip', [(236, 90, 130), (250, 190, 60), (240, 240, 240)])
        flower_card('flower_lavender', 404, 'lavender', [(142, 100, 206), (170, 130, 224)])
        flower_card('flower_bell', 405, 'bell', [(90, 130, 220), (120, 160, 236)])
        flower_card('flower_pink', 406, 'five', [(240, 130, 180), (250, 170, 200)])
        flower_card('flower_yellow', 407, 'five', [(252, 216, 60), (248, 190, 40)])
    if 'misc' in which:
        fern('plant_fern', 501); grass_card('plant_grass', 502); grass_card('plant_grass_tall', 503, 1.0); grass_card('plant_grass_dry', 504, .8, True)
    if 'rock' in which: rock_tex()
    if 'reeds' in which:
        reed_card('plant_reed', 601); reed_card('plant_reed2', 602, False); gull_card('deco_gull')
    contact([n for n in ['leaf_oak.png', 'leaf_broad.png', 'leaf_maple_red.png', 'leaf_maple_orange.png', 'leaf_gold.png', 'leaf_crimson.png', 'leaf_willow.png', 'leaf_cherry.png', 'leaf_magenta.png', 'leaf_white.png', 'leaf_wisteria.png', 'plant_fern.png', 'plant_grass.png', 'plant_grass_dry.png', 'flower_daisy.png', 'flower_poppy.png', 'flower_tulip.png', 'flower_lavender.png', 'flower_bell.png', 'flower_pink.png', 'flower_yellow.png', 'rock_stone.png']], 'sheet_sprites.png', 6, 220)
