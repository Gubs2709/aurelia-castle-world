import sys, math
sys.path.insert(0, "D:/GAME/tools/blender")
import blendlib as B
from blendlib import Builder, rnd
import bpy


def stall(name, awn=('awning_red', 'awning_white'), goods=('fruit_orange', 'fruit_red', 'fruit_green')):
    b = Builder(name)
    W, D = 3.6, 2.2
    # posts
    for sx in (-1, 1):
        b.box((sx * (W / 2 - .1), D / 2 - .1, 1.45), (.14, .14, 2.9), 'wood_dark')
        b.box((sx * (W / 2 - .1), -D / 2 + .1, 1.15), (.14, .14, 2.3), 'wood_dark')
    # counter and lower shelf
    b.box((0, -D / 2 + .55, .85), (W - .1, .95, .12), 'wood_light')
    b.box((0, -D / 2 + .55, .45), (W - .3, .8, .08), 'wood_light')
    b.box((0, -D / 2 + .15, .45), (W - .2, .08, .8), 'wood_dark')
    b.box((0, -D / 2 + .15, .25), (W - .2, .08, .4), 'wood_dark')
    # back shelves
    b.box((0, D / 2 - .1, 1.0), (W - .2, .1, 1.8), 'wood_dark')
    for z in (.7, 1.2, 1.7):
        b.box((0, D / 2 - .35, z), (W - .3, .5, .07), 'wood_light')
    # sloped awning: front low, back high, with striped panels and valance
    n = 10
    for i in range(n):
        x0 = -W / 2 - .2 + (W + .4) * i / n
        x1 = -W / 2 - .2 + (W + .4) * (i + 1) / n
        m = awn[0] if i % 2 == 0 else awn[1]
        b.quad((x0, -D / 2 - .35, 2.15), (x1, -D / 2 - .35, 2.15), (x1, D / 2 + .1, 2.95), (x0, D / 2 + .1, 2.95), m)
        b.quad((x1, -D / 2 - .35, 2.15), (x0, -D / 2 - .35, 2.15), (x0, D / 2 + .1, 2.95), (x1, D / 2 + .1, 2.95), m)
        b.quad((x0, -D / 2 - .35, 2.15), (x1, -D / 2 - .35, 2.15), (x1 - .02, -D / 2 - .4, 1.92), (x0 + .02, -D / 2 - .4, 1.92), m)
        b.quad((x1, -D / 2 - .35, 2.15), (x0, -D / 2 - .35, 2.15), (x0 + .02, -D / 2 - .4, 1.92), (x1 - .02, -D / 2 - .4, 1.92), m)
    b.beam((-W / 2 - .2, -D / 2 - .35, 2.12), (W / 2 + .2, -D / 2 - .35, 2.12), .08, 'wood_dark')
    for sx in (-1, 1):
        b.beam((sx * (W / 2 - .1), -D / 2 + .1, 2.3), (sx * (W / 2 - .1), -D / 2 - .3, 2.12), .08, 'wood_dark')
    # goods: crates with spheres, baskets
    for k in range(4):
        x = -W / 2 + .5 + k * .85
        b.box((x, -D / 2 + .55, .98), (.7, .55, .2), 'crate_wood')
        col = goods[k % len(goods)]
        for i in range(3):
            for j in range(3):
                b.sphere((x - .22 + i * .22, -D / 2 + .38 + j * .17, 1.14 + (i + j) % 2 * .03), .095, col, 5, 6)
    for k in range(3):
        x = -W / 2 + .7 + k * 1.1
        b.cyl((x, D / 2 - .35, .73), .22, .2, 'crate_wood', 10)
        for i in range(3):
            b.sphere((x + rnd(-.1, .1), D / 2 - .35 + rnd(-.1, .1), .98), .1, goods[(k + i) % len(goods)], 5, 6)
    # hanging lantern
    b.box((W / 2 - .3, -D / 2 - .1, 1.85), (.2, .2, .3), 'lantern_glow')
    b.beam((W / 2 - .3, -D / 2 - .1, 2.12), (W / 2 - .3, -D / 2 - .1, 2.0), .03, 'iron')
    return b.build(bevel=.02)


def barrel(name, flowers=True):
    b = Builder(name)
    b.cyl((0, 0, 0), .5, .95, 'wood_light', 14, r2=.5, cap=True)
    # bulge: stacked
    b.cyl((0, 0, .0), .42, .12, 'wood_light', 14, r2=.5)
    b.cyl((0, 0, .83), .5, .12, 'wood_light', 14, r2=.42)
    for z in (.18, .45, .72):
        b.cyl((0, 0, z), .515, .06, 'iron', 14, r2=.515)
    if flowers:
        for i in range(10):
            a = i * 2.4
            r = rnd(0, .32)
            b.sphere((math.cos(a) * r, math.sin(a) * r, .98 + rnd(0, .14)), rnd(.13, .2), ['flower_pink', 'flower_yellow', 'flower_red', 'flower_purple', 'flower_white', 'leaf'][i % 6], 5, 6)
    return b.build(bevel=.01)


def crates(name):
    b = Builder(name)
    b.box((0, 0, .35), (.9, .9, .7), 'crate_wood')
    b.box((.1, 0, 1.0), (.8, .8, .6), 'crate_wood', rz=18)
    b.box((1.0, .2, .3), (.7, .7, .6), 'crate_wood', rz=-12)
    for x, y, z in ((0, 0, .35), (.1, 0, 1.0), (1.0, .2, .3)):
        b.box((x, y, z), (.97, .97, .06), 'wood_dark') if False else None
    b.cyl((-.9, .1, 0), .3, .5, 'crate_wood', 10)
    for i in range(6):
        b.sphere((-.9 + rnd(-.12, .12), .1 + rnd(-.12, .12), .55 + rnd(0, .08)), .1, ['fruit_red', 'fruit_orange', 'fruit_green'][i % 3], 5, 6)
    b.cyl((1.3, -.9, 0), .45, .9, 'hay', 10)
    b.sack = None
    return b.build(bevel=.02)


def well(name):
    b = Builder(name)
    b.cyl((0, 0, 0), 1.2, .95, 'wall_stone', 18, r2=1.15)
    b.cyl((0, 0, .95), 1.3, .14, 'stone_light', 18, r2=1.3)
    b.cyl((0, 0, .5), .96, .5, 'water', 18, r2=.96, cap=True) if False else None
    for sx in (-1, 1):
        b.box((sx * 1.05, 0, 1.9), (.2, .2, 2.0), 'wood_dark')
    b.box((0, 0, 2.95), (2.5, .16, .16), 'wood_dark')
    b.cyl((0, 0, 2.2), .12, .9, 'wood_light', 8, axis='y') if False else None
    b.beam((0, -.3, 2.2), (0, .3, 2.2), .1, 'wood_light')
    b.beam((0, 0, 2.2), (0, 0, 1.3), .03, 'rope')
    b.cyl((0, 0, 1.0), .2, .28, 'wood_light', 8)
    # little pitched roof
    b.gable_roof((0, 0, 2.95), 3.0, 2.0, .9, 'roof_shingle', over=.3, overL=.3, thick=.1, kick=.15, segs=3)
    return b.build(bevel=.015)


def lamp(name, height=3.8):
    b = Builder(name)
    b.cyl((0, 0, 0), .22, .3, 'iron', 8, r2=.12)
    b.cyl((0, 0, .3), .08, height - .3, 'iron', 8)
    b.beam((0, 0, height - .2), (.7, 0, height + .1), .06, 'iron')
    b.sphere((.7, 0, height + .1), .06, 'iron', 4, 6)
    b.beam((.7, 0, height + .1), (.7, 0, height - .1), .03, 'iron')
    b.box((.7, 0, height - .35), (.3, .3, .42), 'lantern_glow')
    b.box((.7, 0, height - .1), (.4, .4, .07), 'iron')
    b.cone((.7, 0, height - .08), .28, .22, 'iron', 4)
    b.box((.7, 0, height - .57), (.32, .32, .05), 'iron')
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box((.7 + sx * .15, sy * .15, height - .35), (.025, .025, .42), 'iron')
    return b.build(bevel=.01)


def fountain(name):
    b = Builder(name)
    b.cyl((0, 0, 0), 3.2, .7, 'wall_stone', 28, r2=3.1)
    b.cyl((0, 0, .7), 3.35, .18, 'stone_light', 28, r2=3.35)
    b.cyl((0, 0, .55), 2.85, .12, 'water', 28, r2=2.85)
    b.cyl((0, 0, .6), .55, 1.4, 'stone_light', 14, r2=.4)
    b.cyl((0, 0, 1.95), 1.35, .2, 'stone_light', 20, r2=1.3)
    b.cyl((0, 0, 2.15), 1.2, .12, 'water', 20, r2=1.2)
    b.cyl((0, 0, 1.95), .3, 1.1, 'stone_light', 12, r2=.22)
    b.cyl((0, 0, 3.0), .75, .2, 'stone_light', 16, r2=.7)
    b.cyl((0, 0, 3.15), .65, .1, 'water', 16, r2=.65)
    b.sphere((0, 0, 3.55), .35, 'stone_light', 8, 10)
    # little spouts (static water arcs)
    for i in range(8):
        a = i * math.pi / 4
        b.box((math.cos(a) * 1.0, math.sin(a) * 1.0, 2.0), (.1, .1, .3), 'water', rz=math.degrees(a))
    return b.build(bevel=.02)


def cart(name):
    b = Builder(name)
    b.box((0, 0, .85), (3.0, 1.5, .12), 'wood_light')
    for sy in (-1, 1):
        b.box((0, sy * .72, 1.15), (3.0, .1, .5), 'wood_light')
    for sx in (-1, 1):
        b.box((sx * 1.45, 0, 1.15), (.1, 1.5, .5), 'wood_light')
        for sy in (-1, 1):
            # wheel
            b.cyl((sx * .9, sy * .88, .5), .52, .12, 'wood_dark', 16, axis='y', cap=True) if False else None
    for sy in (-1, 1):
        b.cyl((.2, sy * .88 - .06, .55), .55, .12, 'wood_dark', 20, axis='y')
        b.cyl((.2, sy * .88 - .07, .55), .1, .14, 'iron', 8, axis='y')
        for k in range(8):
            a = k * math.pi / 4
            b.beam((.2 + math.cos(a) * .08, sy * .88, .55 + math.sin(a) * .08), (.2 + math.cos(a) * .5, sy * .88, .55 + math.sin(a) * .5), .06, 'wood_light')
    b.beam((-1.5, 0, 1.0), (-3.4, 0, .6), .12, 'wood_dark')
    # load: hay + sacks + pumpkins
    b.box((0, 0, 1.25), (2.7, 1.3, .6), 'hay')
    for i in range(6):
        b.sphere((-.9 + i * .35 + rnd(-.1, .1), rnd(-.3, .3), 1.65), rnd(.16, .22), ['fruit_orange', 'fruit_orange', 'fruit_red', 'fruit_yellow'][i % 4], 6, 8)
    return b.build(bevel=.02)


def bench(name):
    b = Builder(name)
    b.box((0, 0, .5), (1.8, .5, .08), 'wood_light')
    b.box((0, .22, .95), (1.8, .06, .5), 'wood_light')
    for sx in (-1, 1):
        b.box((sx * .75, 0, .25), (.08, .45, .5), 'wood_dark')
        b.box((sx * .75, .22, .72), (.08, .06, .55), 'wood_dark')
    return b.build(bevel=.01)


def banner_pole(name, mat='banner_blue'):
    b = Builder(name)
    b.cyl((0, 0, 0), .09, 5.2, 'wood_dark', 8)
    b.box((0, 0, 5.2), (.1, .1, .1), 'trim_gold')
    b.beam((0, 0, 5.0), (1.0, 0, 5.0), .06, 'wood_dark')
    for i in range(6):
        t = i / 5
        b.quad((.05, 0, 4.95 - 0), (.95, 0, 4.95), (.95 - t * .05, 0, 2.6), (.05, 0, 2.6), mat) if i == 0 else None
    b.box((.5, 0, 3.9), (.9, .03, 2.4), mat)
    b.box((.5, .03, 4.0), (.5, .04, .9), 'trim_gold')
    b.box((.5, .03, 3.4), (.2, .04, .5), 'trim_gold')
    return b.build(bevel=.0)


def bunting(name, length=14.0, n=14):
    b = Builder(name)
    mats = ['banner_red', 'banner_yellow', 'banner_blue', 'awning_green', 'awning_white']
    pts = []
    for i in range(n * 2 + 1):
        t = i / (n * 2)
        x = -length / 2 + length * t
        z = -math.sin(math.pi * t) * .9
        pts.append((x, 0, z))
    for i in range(len(pts) - 1):
        b.beam(pts[i], pts[i + 1], .025, 'rope')
    for i in range(n):
        t = (i + .5) / n
        x = -length / 2 + length * t
        z = -math.sin(math.pi * t) * .9
        m = mats[i % len(mats)]
        b.tri((x - .22, 0, z), (x + .22, 0, z), (x, 0, z - .5), m)
        b.tri((x + .22, 0, z), (x - .22, 0, z), (x, 0, z - .5), m)
    return b.build(bevel=0)


def sign_post(name):
    b = Builder(name)
    b.cyl((0, 0, 0), .1, 2.8, 'wood_dark', 8)
    for i, (z, yaw) in enumerate(((2.5, 8), (2.0, -6), (1.55, 5))):
        b.box((.55, 0, z), (1.2, .06, .34), 'wood_light', rz=yaw)
        b.tri((1.2, .03, z + .17), (1.2, .03, z - .17), (1.42, .03, z), 'wood_light')
    return b.build(bevel=.01)


def haystack(name):
    b = Builder(name)
    b.cyl((0, 0, 0), 1.3, 1.9, 'hay', 14, r2=.25, cap=True)
    b.sphere((0, 0, 1.95), .55, 'hay', 6, 8)
    return b.build(bevel=0)


def fence(name, length=4.0):
    b = Builder(name)
    n = int(length / 2)
    for i in range(n + 1):
        b.box((-length / 2 + i * length / n, 0, .55), (.14, .14, 1.1), 'wood_dark')
    for z in (.4, .8):
        b.box((0, 0, z), (length, .07, .1), 'wood_light')
    return b.build(bevel=.01)


def stone_wall(name, length=4.0):
    b = Builder(name)
    b.box((0, 0, .5), (length, .6, 1.0), 'wall_stone', taper=.95)
    b.box((0, 0, 1.05), (length + .1, .7, .14), 'stone_light')
    return b.build(bevel=.03)


def planter(name):
    b = Builder(name)
    b.box((0, 0, .3), (1.6, .5, .6), 'wood_dark')
    b.box((0, 0, .61), (1.5, .42, .06), 'leaf')
    for i in range(10):
        b.sphere((-.65 + i * .145, rnd(-.12, .12), .75 + rnd(0, .1)), rnd(.1, .16), ['flower_pink', 'flower_yellow', 'flower_red', 'flower_purple', 'flower_white', 'leaf'][i % 6], 5, 6)
    return b.build(bevel=.01)


def gate_arch(name):
    """Town gate: two squat stone towers with a pointed arch passage and portcullis."""
    b = Builder(name)
    W, D, H = 18.0, 6.0, 9.0
    for sx in (-1, 1):
        x = sx * (W / 2 - 2.8)
        b.box((x, 0, H / 2), (5.6, D, H), 'wall_stone')
        b.box((x, 0, H + .15), (6.4, D + .8, .3), 'stone_light')
        for k in range(5):
            for sy in (-1, 1):
                b.box((x - 2.4 + k * 1.2, sy * (D / 2 + .1), H + 1.0), (.8, .5, 1.5), 'stone_light')
        for sy in (-1, 1):
            b.box((x, sy * (D / 2 + .02), 6.4), (.9, .12, 1.8), 'glass')
            b.box((x, sy * (D / 2 + .05), 6.4), (1.2, .08, 2.1), 'timber')
        # conical teal roofs on small turrets
        b.cyl((x + sx * 2.4, -D / 2 + .3, H), 1.1, 2.6, 'wall_stone', 14)
        b.cone((x + sx * 2.4, -D / 2 + .3, H + 2.6), 1.5, 4.0, 'roof_teal', 14, flare=.1)
    # lintel over passage (pointed arch via two slanted blocks)
    pw = W - 2 * 5.6
    b.box((0, 0, H - 1.2), (pw + .2, D, 2.4), 'wall_stone')
    for i in range(7):
        t = i / 6
        a = math.pi * t
        px = -pw / 2 + pw * t
        pz = 4.4 + math.sin(a) * 2.6
        b.box((px, -D / 2 - .1, pz), (.9, .5, .9), 'stone_light', rz=0) if False else None
    # arch ring (voussoirs)
    for i in range(14):
        a0 = math.pi * i / 14
        a1 = math.pi * (i + 1) / 14
        cx = 0
        for sy in (-1, 1):
            r0, r1 = pw / 2, pw / 2 + .7
            p = lambda r, a: (cx - math.cos(a) * r, sy * (D / 2 + .1), 4.6 + math.sin(a) * r * 1.15)
            b.quad(p(r0, a0), p(r1, a0), p(r1, a1), p(r0, a1), 'stone_light') if sy == 1 else b.quad(p(r1, a0), p(r0, a0), p(r0, a1), p(r1, a1), 'stone_light')
    # portcullis (raised) bars at top of arch
    for i in range(7):
        x = -pw / 2 + .6 + i * (pw - 1.2) / 6
        b.box((x, 0, 6.6), (.09, .09, 1.5), 'iron')
    # flagstone road through
    b.box((0, 0, .05), (pw, D, .1), 'path_flagstone')
    # banners
    for sx in (-1, 1):
        b.box((sx * (W / 2 - 2.8), -D / 2 - .1, 4.3), (1.5, .06, 3.0), 'banner_blue')
        b.box((sx * (W / 2 - 2.8), -D / 2 - .14, 4.6), (.7, .04, .8), 'trim_gold')
    return b.build(bevel=.04)


def windmill(name):
    b = Builder(name)
    b.cyl((0, 0, 0), 3.2, 8.5, 'wall_plaster', 14, r2=2.2)
    for k in range(5):
        b.cyl((0, 0, 1 + k * 1.6), 3.15 - k * .22, .22, 'wall_stone', 14, r2=3.15 - k * .22) if False else None
    b.cone((0, 0, 8.4), 3.0, 3.6, 'roof_shingle', 14, flare=.12)
    b.box((0, -2.4, 1.2), (1.4, .3, 2.2), 'door_wood')
    for k in range(3):
        a = k * 2 * math.pi / 3 + .4
        b.box((math.cos(a) * 2.5, math.sin(a) * 2.5 - .1, 5 + k * 0), (.7, .1, .9), 'glass') if False else None
    for z in (3.5, 5.5):
        b.box((0, -2.55 + (z - 3.5) * .09, z), (.8, .1, 1.0), 'glass')
        b.box((0, -2.5 + (z - 3.5) * .09, z), (1.0, .14, 1.2), 'timber') if False else None
    # sails
    hub = (0, -2.9, 8.4)
    b.cyl((0, -2.9, 8.4), .35, .6, 'timber', 8, axis='y')
    for k in range(4):
        a = k * math.pi / 2 + math.radians(18)
        dx, dz = math.cos(a), math.sin(a)
        b.beam((0, -3.0, 8.4), (dx * 6.5, -3.0, 8.4 + dz * 6.5), .22, 'wood_dark')
        px, pz = -dz, dx
        for j in range(4):
            t0 = 1.4 + j * 1.2
            for l in range(2):
                b.box((dx * t0 + px * .6 * (1 if l == 0 else -1) * 0 + 0, -3.05, 8.4 + dz * t0), (.01, .02, .01), 'cloth_white') if False else None
        # sail cloth quad
        b.quad((dx * 1.2 + px * .1, -3.05, 8.4 + dz * 1.2 + pz * .1), (dx * 6.3 + px * .1, -3.05, 8.4 + dz * 6.3 + pz * .1), (dx * 6.3 + px * 1.5, -3.05, 8.4 + dz * 6.3 + pz * 1.5), (dx * 1.2 + px * 1.5, -3.05, 8.4 + dz * 1.2 + pz * 1.5), 'cloth_white')
        b.quad((dx * 1.2 + px * 1.5, -3.04, 8.4 + dz * 1.2 + pz * 1.5), (dx * 6.3 + px * 1.5, -3.04, 8.4 + dz * 6.3 + pz * 1.5), (dx * 6.3 + px * .1, -3.04, 8.4 + dz * 6.3 + pz * .1), (dx * 1.2 + px * .1, -3.04, 8.4 + dz * 1.2 + pz * .1), 'cloth_white')
    return b.build(bevel=.02)


def tree_stump_log(name):
    b = Builder(name)
    b.cyl((0, 0, 0), .45, .6, 'wood_dark', 10)
    b.cyl((1.4, .2, .35), .3, 2.4, 'wood_light', 8, axis='x')
    return b.build(bevel=.02)


def run():
    only = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    specs = [
        ('Prop_StallRed', lambda n: stall(n, ('awning_red', 'awning_white'), ('fruit_orange', 'fruit_red', 'fruit_green'))),
        ('Prop_StallBlue', lambda n: stall(n, ('awning_blue', 'awning_white'), ('fruit_green', 'fruit_yellow', 'fruit_red'))),
        ('Prop_StallYellow', lambda n: stall(n, ('awning_yellow', 'awning_white'), ('fruit_yellow', 'fruit_orange', 'fruit_red'))),
        ('Prop_StallGreen', lambda n: stall(n, ('awning_green', 'awning_white'), ('fruit_red', 'fruit_green', 'fruit_orange'))),
        ('Prop_Barrel', lambda n: barrel(n, True)), ('Prop_BarrelPlain', lambda n: barrel(n, False)),
        ('Prop_Crates', crates), ('Prop_Well', well), ('Prop_Lamp', lamp), ('Prop_Fountain', fountain), ('Prop_Cart', cart), ('Prop_Bench', bench),
        ('Prop_BannerBlue', lambda n: banner_pole(n, 'banner_blue')), ('Prop_BannerYellow', lambda n: banner_pole(n, 'banner_yellow')),
        ('Prop_Bunting', bunting), ('Prop_SignPost', sign_post), ('Prop_Haystack', haystack), ('Prop_Fence', fence), ('Prop_StoneWall', stone_wall),
        ('Prop_Planter', planter), ('Prop_Gate', gate_arch), ('Prop_Windmill', windmill), ('Prop_Log', tree_stump_log),
    ]
    for name, fn in specs:
        if only and name not in only:
            continue
        B.clear()
        o = fn(name)
        B.export([o], name)
        big = name in ('Prop_Gate', 'Prop_Windmill', 'Prop_Fountain')
        B.preview("D:/GAME/artifacts/living-world/blender_" + name + ".png", [o], loc=(-18, -26, 12) if big else (-5, -7, 3.5), target=(0, 0, 4.5 if big else 1.4), size=(700, 560))
        print('EXPORTED', name)


run()
