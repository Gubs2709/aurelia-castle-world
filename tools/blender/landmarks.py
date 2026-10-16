import sys, math
sys.path.insert(0, "D:/GAME/tools/blender")
import blendlib as B
from blendlib import Builder, rnd
import bpy


def lancet(b, x, y, z, face, h=2.2, w=.7):
    # pointed gothic window on a wall facing +/-Y ('Y') or +/-X
    s = face
    b.box((x, y + s * .04, z), (w + .3, .08, h + .3), 'stone_light')
    b.box((x, y + s * .07, z), (w, .06, h), 'glass')
    b.box((x, y + s * .1, z), (.06, .04, h), 'stone_light')
    b.tri((x - w / 2, y + s * .07, z + h / 2), (x + w / 2, y + s * .07, z + h / 2), (x, y + s * .07, z + h / 2 + w * .7), 'glass') if s == -1 else b.tri((x + w / 2, y + s * .07, z + h / 2), (x - w / 2, y + s * .07, z + h / 2), (x, y + s * .07, z + h / 2 + w * .7), 'glass')


def clock_tower(name):
    b = Builder(name)
    W = 5.2
    H1 = 15.0
    b.box((0, 0, H1 / 2), (W, W, H1), 'wall_stone', taper=.96)
    for z in (4, 8.5, 12.5):
        b.box((0, 0, z), (W + .35, W + .35, .35), 'stone_light')
    # corner quoins
    for sx in (-1, 1):
        for sy in (-1, 1):
            for k in range(14):
                b.box((sx * W / 2, sy * W / 2, .5 + k), (.6 + (k % 2) * .1, .6 + ((k + 1) % 2) * .1, .9), 'stone_light')
    for z in (6.4, 10.6):
        for sy in (-1, 1):
            lancet(b, 0, sy * W / 2, z, sy, 2.2, .8)
        for sx in (-1, 1):
            b.box((sx * W / 2, 0, z), (.08, .8, 2.2), 'glass')
    b.box((0, 0, H1 + .5), (W + 1.1, W + 1.1, 1.0), 'stone_light')
    # clock stage
    H2 = 4.2
    b.box((0, 0, H1 + 1 + H2 / 2), (W * .86, W * .86, H2), 'wall_plaster')
    for sy in (-1, 1):
        r = 1.5
        b.cyl((0, sy * (W * .43 + .05), H1 + 1 + H2 / 2), r, .1, 'cloth_white', 20, axis='y')
        b.cyl((0, sy * (W * .43 + .1), H1 + 1 + H2 / 2), r * 1.05, .08, 'trim_gold', 20, axis='y', cap=False) if False else None
        b.box((0, sy * (W * .43 + .12), H1 + 1 + H2 / 2 + .5), (.1, .06, 1.0), 'iron')
        b.box((.4, sy * (W * .43 + .12), H1 + 1 + H2 / 2), (.8, .06, .1), 'iron')
        for k in range(12):
            a = k * math.pi / 6
            b.box((math.cos(a) * 1.25, sy * (W * .43 + .12), H1 + 1 + H2 / 2 + math.sin(a) * 1.25), (.1, .05, .1), 'iron')
    for sx in (-1, 1):
        b.box((sx * (W * .43 + .05), 0, H1 + 1 + H2 / 2), (.1, 1.2, 2.2), 'glass')
    # belfry + spire
    b.box((0, 0, H1 + 1 + H2 + .3), (W * .86 + .8, W * .86 + .8, .6), 'timber')
    b.cone((0, 0, H1 + 1 + H2 + .5), W * .72, 10.5, 'roof_teal', 8, flare=.12)
    b.sphere((0, 0, H1 + 1 + H2 + 11.2), .35, 'gold', 6, 8)
    b.beam((0, 0, H1 + 1 + H2 + 11.2), (0, 0, H1 + 1 + H2 + 13), .08, 'gold')
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.cone((sx * W * .48, sy * W * .48, H1 + 1.6), .55, 3.2, 'roof_teal', 6)
    # door
    b.box((0, -W / 2 - .02, 1.4), (1.8, .12, 2.8), 'stone_light')
    b.box((0, -W / 2 - .06, 1.3), (1.3, .1, 2.5), 'door_wood')
    b.banner = None
    for sy in (-1, 1):
        b.box((1.7, sy * (W / 2 + .1), 8.2), (1.1, .06, 3.2), 'banner_blue')
    return b.build(bevel=.04)


def chapel(name):
    b = Builder(name)
    L, W, Hh = 15.0, 7.6, 6.6
    b.box((0, 0, Hh / 2), (L, W, Hh), 'wall_plaster')
    b.box((0, 0, .45), (L + .4, W + .4, .9), 'wall_stone')
    for k in range(7):
        x = -L / 2 + 1.2 + k * (L - 2.4) / 6
        for sy in (-1, 1):
            b.box((x, sy * (W / 2 + .12), Hh / 2), (.34, .3, Hh), 'wall_stone')
    for x in (-4.5, -1.5, 1.5, 4.5):
        for sy in (-1, 1):
            lancet(b, x, sy * W / 2, 3.4, sy, 2.6, .9)
    rise = 4.6
    b.gable_roof((0, 0, Hh), L, W, rise, 'roof_slate', over=.7, overL=.6, thick=.16, kick=.4)
    for sx in (-1, 1):
        x = sx * L / 2
        b.tri((x, -W / 2, Hh), (x, W / 2, Hh), (x, 0, Hh + rise), 'wall_plaster')
    # round window at the facade
    b.cyl((-L / 2 - .05, 0, 4.8), 1.1, .1, 'glass', 18, axis='x') if False else None
    b.box((-L / 2 - .05, 0, 4.4), (.1, 1.6, 1.6), 'glass')
    b.box((-L / 2 - .08, 0, 4.4), (.08, 2.0, .15), 'stone_light')
    b.box((-L / 2 - .08, 0, 4.4), (.08, .15, 2.0), 'stone_light')
    # portal
    b.box((-L / 2 - .1, 0, 1.7), (.3, 2.4, 3.4), 'stone_light')
    b.box((-L / 2 - .14, 0, 1.5), (.2, 1.8, 3.0), 'door_wood')
    for k in range(4):
        b.box((-L / 2 - .6 - k * .3, 0, .4 - k * .12), (.6, 3 + k * .2, .22), 'wall_stone')
    # bell tower at the back corner
    tx = L / 2 - 1.8
    ty = W / 2 + 1.2
    b.box((tx, ty, 6.0), (3.4, 3.4, 12.0), 'wall_stone')
    for z in (4.5, 9):
        b.box((tx, ty, z), (3.8, 3.8, .3), 'stone_light')
    for sy in (-1, 1):
        lancet(b, tx, ty + sy * 1.7, 6.2, sy, 2.0, .7)
    b.box((tx, ty, 12.4), (4.0, 4.0, .8), 'stone_light')
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box((tx + sx * 1.5, ty + sy * 1.5, 13.4), (.5, .5, 1.6), 'stone_light')
    b.box((tx, ty, 13.0), (2.4, 2.4, .5), 'timber')
    b.cone((tx, ty, 13.4), 2.9, 7.0, 'roof_teal', 8, flare=.1)
    b.sphere((tx, ty, 20.5), .3, 'gold', 6, 8)
    return b.build(bevel=.04)


def run():
    only = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    for name, fn in (('Prop_ClockTower', clock_tower), ('Prop_Chapel', chapel)):
        if only and name not in only:
            continue
        B.clear()
        o = fn(name)
        B.export([o], name)
        B.preview("D:/GAME/artifacts/living-world/blender_" + name + ".png", [o], loc=(-22, -30, 14), target=(0, 0, 8), size=(700, 700))
        print('EXPORTED', name)


run()
