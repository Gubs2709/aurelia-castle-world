import sys, math
sys.path.insert(0, "D:/GAME/tools/blender")
import blendlib as B
from blendlib import Builder, rnd
import houses as H
import bpy
from mathutils import Vector

PL = .5


def harbor_house(name, w, d, floors, wall, roof, shut, awning=None, balcony=False, chim=True, jetty=0.0, tower=False):
    b = Builder(name)
    FHh = 2.7
    b.box((0, 0, PL / 2), (w + .3, d + .3, PL), 'wall_stone')
    zs = [PL + i * FHh for i in range(floors + 1)]
    for f in range(floors):
        jy = jetty * f
        cy = -jy / 2
        dd = d + jy
        b.box((0, cy, (zs[f] + zs[f + 1]) / 2), (w, dd, FHh), wall)
        b.box((0, cy, zs[f] + .02), (w + .16, dd + .16, .22), 'trim_white')       # white band between floors
    for sx in (-1, 1):
        for sy in (-1, 1):
            b.box((sx * w / 2, sy * d / 2 - (jetty * (floors - 1) / 2 if sy < 0 else 0) * 0, (zs[0] + zs[-1]) / 2), (.28, .28, zs[-1] - zs[0] + .1), 'trim_white')
    front_y = -d / 2 - jetty * (floors - 1)
    # windows on front, sides, back
    nwin = max(2, int(w / 2.6))
    for f in range(floors):
        yf = -d / 2 - jetty * f
        for i in range(nwin):
            x = -w / 2 + w * (i + .5) / nwin
            if f == 0 and abs(x) < 1.0 and not awning:
                continue
            if f == 0 and awning and i == nwin // 2:
                continue
            H.window(b, x, yf, zs[f] + 1.5, -1, w=.85, h=1.2, shutter=shut, flowers=(f == 0 or (balcony and f == 1)))
        for i in range(max(1, int(w / 3.2))):
            H.window(b, -w / 2 + w * (i + .5) / max(1, int(w / 3.2)), d / 2, zs[f] + 1.5, 1, w=.85, shutter=shut, flowers=False)
        for sx, face in ((-1, 'L'), (1, 'R')):
            for i in range(max(1, int(d / 3.4))):
                H.window(b, sx * w / 2, -d / 2 + d * (i + .5) / max(1, int(d / 3.4)) - jetty * f * .5, zs[f] + 1.5, face, w=.8, shutter=shut, flowers=False)
    # door (or shop front)
    if awning:
        b.box((0, -d / 2 + .06, PL + 1.15), (w * .5, .1, 1.7), 'glass')
        b.box((0, -d / 2 + .04, PL + 1.15), (w * .5 + .2, .08, 1.9), 'trim_white')
        b.box((0, -d / 2 - .22, PL + .9), (w * .5, .45, .1), 'wood_light')
        H.awning(b, 0, -d / 2, PL + 2.4, w * .62, 1.5, awning[0], awning[1])
        for i, c in enumerate(['fruit_orange', 'fruit_red', 'fish', 'fruit_yellow', 'fish']):
            b.sphere((-w * .2 + i * w * .1, -d / 2 - .22, PL + 1.0), .1, c, 5, 6)
    else:
        H.door(b, 0, PL, -d / 2, -1, w=1.1)
    # balcony
    if balcony and floors >= 2:
        zb = zs[1]
        bw = min(w * .7, 4.0)
        b.box((0, -d / 2 - .7, zb), (bw, 1.4, .14), 'wood_light')
        for i in range(int(bw / .3) + 1):
            b.box((-bw / 2 + i * bw / int(bw / .3), -d / 2 - 1.35, zb + .55), (.05, .05, 1.0), 'trim_white')
        b.box((0, -d / 2 - 1.35, zb + 1.08), (bw, .08, .08), 'trim_white')
        for sx in (-1, 1):
            b.box((sx * bw / 2, -d / 2 - .7, zb + 1.08), (.08, 1.4, .08), 'trim_white')
        for i in range(6):
            b.sphere((-bw / 2 + .4 + i * (bw - .8) / 5, -d / 2 - 1.2, zb + .3), rnd(.12, .2), ['flower_pink', 'flower_yellow', 'flower_red', 'flower_white'][i % 4], 5, 6)
    Z = zs[-1]
    top_d = d + jetty * (floors - 1)
    cy = -jetty * (floors - 1) / 2
    rise = (top_d / 2) * 1.0
    b.box((0, cy, Z + .06), (w + .5, top_d + .5, .2), 'trim_white')
    b.gable_roof((0, cy, Z + .12), w, top_d, rise, roof, over=.7, overL=.45, thick=.15, kick=.3)
    for sx in (-1, 1):
        x = sx * w / 2
        b.tri((x, cy - top_d / 2, Z + .12), (x, cy + top_d / 2, Z + .12), (x, cy, Z + .12 + rise), wall)
        b.beam((x + sx * .03, cy - top_d / 2 - .1, Z + .1), (x + sx * .03, cy, Z + .12 + rise + .08), .14, 'trim_white')
        b.beam((x + sx * .03, cy + top_d / 2 + .1, Z + .1), (x + sx * .03, cy, Z + .12 + rise + .08), .14, 'trim_white')
        H.window(b, x, cy, Z + .12 + rise * .38, 'L' if sx < 0 else 'R', w=.6, h=.8, shutter=shut, flowers=False)
    if chim:
        H.chimney(b, w / 2 - .9, cy + .3, Z + .1, Z + .12 + rise + .85)
    # round attic window in the front under the eave line via dormer
    if w > 5.5:
        t = .45
        yd = cy - (top_d / 2 + .7) * (1 - t)
        zd = Z + .12 + rise * ((top_d / 2 + .7) / (top_d / 2)) * t
        b.box((0, yd + .1, zd + .5), (1.4, 1.0, 1.2), wall)
        H.window(b, 0, yd - .4, zd + .55, -1, w=.6, h=.8, shutter=shut, flowers=False)
        r = .7
        b.quad((-.9, yd - .5, zd + 1.05), (0, yd - .55, zd + 1.05 + r), (0, yd + .6, zd + 1.05 + r), (-.9, yd + .6, zd + 1.05), roof)
        b.quad((0, yd - .55, zd + 1.05 + r), (.9, yd - .5, zd + 1.05), (.9, yd + .6, zd + 1.05), (0, yd + .6, zd + 1.05 + r), roof)
        b.tri((-.9, yd - .5, zd + 1.05), (.9, yd - .5, zd + 1.05), (0, yd - .55, zd + 1.05 + r), 'trim_white')
    H.lantern(b, w / 2 - .5, -d / 2, PL + 2.4, -1)
    for sx in (-1, 1):
        b.cyl((sx * 1.3, -d / 2 - 1.2, PL - .45), .22, .4, 'wood_dark', 10)
        for k in range(4):
            b.sphere((sx * 1.3 + rnd(-.12, .12), -d / 2 - 1.2 + rnd(-.1, .1), PL + rnd(-.02, .18)), .16, ['flower_pink', 'flower_yellow', 'flower_red', 'flower_purple'][k], 5, 6)
    return b.build(bevel=.025)


def dock(name, L=8.0, W=3.2):
    b = Builder(name)
    n = int(L / .32)
    for i in range(n):
        y = -L / 2 + (i + .5) * L / n
        b.box((0, y, 0.92), (W, L / n - .04, .1), 'wood_light' if i % 3 else 'wood_dark')
    for sx in (-1, 1):
        b.box((sx * (W / 2 - .15), 0, .8), (.16, L, .2), 'wood_dark')
        for k in range(int(L / 2) + 1):
            y = -L / 2 + k * L / int(L / 2)
            b.cyl((sx * (W / 2 - .05), y, -1.2), .14, 2.8, 'wood_dark', 8)
            b.sphere((sx * (W / 2 - .05), y, 1.6), .15, 'wood_dark', 6, 8) if False else None
            b.cyl((sx * (W / 2 - .05), y, 1.0), .12, .65, 'wood_dark', 8)
    for k in range(int(L / 2)):
        y = -L / 2 + (k + .5) * L / int(L / 2)
        for sx in (-1, 1):
            b.beam((sx * (W / 2 - .05), y - 1, 1.5), (sx * (W / 2 - .05), y + 1, 1.45), .04, 'rope')
    b.box((0, -L / 2 + .5, 1.0), (.5, .5, .2), 'iron') if False else None
    b.box((0, 0, 0.3), (W - .1, L - .1, .4), 'wood_dark')
    return b.build(bevel=.012)


def hull(b, L, W, Hh, outer, inner, trim, rise=.5, stern=.65):
    st = 9
    prof = []
    for i in range(st + 1):
        t = i / st
        y = -L / 2 + L * t
        wd = W / 2 * (math.sin(math.pi * min(1, t * 1.02 + .02)) ** .55 if t < 1 else 0)
        wd = W / 2 * (1 - (abs(t - .45) / .55) ** 2.2) ** .6 if abs(t - .45) < .55 else 0
        lift = rise * (max(0, t - .55) / .45) ** 2 + rise * .5 * (max(0, .2 - t) / .2) ** 2
        prof.append((y, max(.03, wd), lift))
    for side in (-1, 1):
        for i in range(st):
            y0, w0, l0 = prof[i]
            y1, w1, l1 = prof[i + 1]
            bt0 = (side * w0 * .55, y0, .0 + l0 * .3)
            bt1 = (side * w1 * .55, y1, .0 + l1 * .3)
            tp0 = (side * w0, y0, Hh + l0)
            tp1 = (side * w1, y1, Hh + l1)
            if side == 1:
                b.quad(bt0, bt1, tp1, tp0, outer)
                b.quad(tp0, tp1, (tp1[0] - .05, tp1[1], tp1[2] - .02), (tp0[0] - .05, tp0[1], tp0[2] - .02), trim)
                b.quad((tp0[0] - .08, tp0[1], tp0[2] - .02), (tp1[0] - .08, tp1[1], tp1[2] - .02), (bt1[0] - .06, bt1[1], bt1[2] + .08), (bt0[0] - .06, bt0[1], bt0[2] + .08), inner)
            else:
                b.quad(bt1, bt0, tp0, tp1, outer)
                b.quad(tp1, tp0, (tp0[0] + .05, tp0[1], tp0[2] - .02), (tp1[0] + .05, tp1[1], tp1[2] - .02), trim)
                b.quad((tp1[0] + .08, tp1[1], tp1[2] - .02), (tp0[0] + .08, tp0[1], tp0[2] - .02), (bt0[0] + .06, bt0[1], bt0[2] + .08), (bt1[0] + .06, bt1[1], bt1[2] + .08), inner)
    # keel/floor and transom/bow caps
    for i in range(st):
        y0, w0, l0 = prof[i]
        y1, w1, l1 = prof[i + 1]
        b.quad((-w0 * .5, y0, l0 * .3 + .08), (w0 * .5, y0, l0 * .3 + .08), (w1 * .5, y1, l1 * .3 + .08), (-w1 * .5, y1, l1 * .3 + .08), inner)
        b.quad((-w0 * .55, y0, l0 * .3), (-w1 * .55, y1, l1 * .3), (w1 * .55, y1, l1 * .3), (w0 * .55, y0, l0 * .3), outer)
    y0, w0, l0 = prof[0]
    b.quad((-w0 * .55, y0, l0 * .3), (w0 * .55, y0, l0 * .3), (w0, y0, Hh + l0), (-w0, y0, Hh + l0), outer)
    y1, w1, l1 = prof[-1]
    b.quad((w1 * .55, y1, l1 * .3), (-w1 * .55, y1, l1 * .3), (-w1, y1, Hh + l1), (w1, y1, Hh + l1), outer)
    for k in range(3):
        y = -L * .25 + k * L * .25
        b.box((0, y, Hh * .45), (W * .8, .1, .08), 'hull_wood')
    return prof


def row_boat(name, hullc='hull_wood'):
    b = Builder(name)
    hull(b, 4.2, 1.5, .55, hullc, 'wood_light', 'wood_dark', .45)
    b.box((0, 0, .5), (1.2, .5, .06), 'wood_light')
    for sx in (-1, 1):
        b.beam((sx * .6, -.2, .75), (sx * 1.9, -.5, .3), .06, 'wood_light')
        b.box((sx * 1.95, -.52, .28), (.08, .5, .28), 'wood_light')
    return b.build(bevel=.012)


def fishing_boat(name, hullc, cabin, roof):
    b = Builder(name)
    hull(b, 8.0, 2.8, 1.1, hullc, 'wood_light', 'trim_white', .9)
    b.box((0, 0, 1.0), (2.4, 5.2, .1), 'wood_light')
    # cabin near stern
    b.box((0, -.8, 1.9), (1.9, 1.8, 1.7), cabin)
    for sx in (-1, 1):
        b.box((sx * .96, -.8, 2.0), (.04, .7, .6), 'glass')
    b.box((0, -.8, 1.95), (.04, 1.9, .5), 'glass') if False else None
    b.box((0, -.8, 2.8), (2.2, 2.1, .14), roof)
    b.box((0, -1.2, 2.95), (.9, .9, .3), 'trim_white')
    b.cyl((0, -1.2, 3.1), .13, .5, 'iron', 8)
    # mast with boom + net on a rail
    b.cyl((0, 1.4, 1.0), .09, 4.0, 'wood_dark', 8)
    b.beam((0, 1.4, 4.2), (0, 3.0, 3.1), .05, 'wood_dark')
    b.beam((0, 1.4, 4.2), (0, -2.8, 2.9), .02, 'rope')
    b.box((0, .0, 1.2), (.1, 3.0, .08), 'wood_dark') if False else None
    # crates of fish and a net pile
    for k in range(3):
        b.box((.55, 2.0 + k * .5, 1.25), (.55, .45, .3), 'crate_wood')
        for j in range(3):
            b.sphere((.55 + (j - 1) * .15, 2.0 + k * .5, 1.45), .09, 'fish', 5, 6)
    b.sphere((-.6, 2.3, 1.2), .4, 'net', 6, 8, sq=.5)
    # bow flag
    b.cyl((0, 3.7, 1.4), .04, 1.2, 'iron', 6)
    b.quad((0, 3.7, 2.55), (0, 3.7, 2.2), (0, 3.15, 2.38), (0, 3.15, 2.55), 'banner_red')
    b.quad((0, 3.15, 2.55), (0, 3.15, 2.38), (0, 3.7, 2.2), (0, 3.7, 2.55), 'banner_red')
    return b.build(bevel=.014)


def sail_boat(name, hullc='hull_blue'):
    b = Builder(name)
    hull(b, 9.0, 3.0, 1.2, hullc, 'wood_light', 'trim_white', 1.0)
    b.box((0, 0, 1.15), (2.6, 6.0, .1), 'wood_light')
    b.box((0, -1.2, 1.75), (1.9, 2.0, 1.2), 'wood_light')
    b.box((0, -1.2, 2.45), (2.2, 2.3, .12), 'roof_red')
    b.cyl((0, .8, 1.1), .12, 8.8, 'wood_dark', 8)
    b.cyl((0, 3.4, 1.1), .08, 4.2, 'wood_dark', 8) if False else None
    for sgn in (1, -1):
        b.quad((0, .9, 9.4), (0, -2.8, 2.6), (0, .9, 2.6), (0, .9, 9.4), 'sail') if False else None
    # main sail (double sided)
    A = (0, .85, 9.6)
    Bk = (0, .85, 2.3)
    C = (0, -3.4, 2.3)
    b.tri(A, Bk, C, 'sail')
    b.tri(A, C, Bk, 'sail')
    # jib sail
    A2 = (0, 1.0, 9.0)
    B2 = (0, 1.0, 2.3)
    C2 = (0, 5.2, 2.0)
    b.tri(A2, C2, B2, 'sail')
    b.tri(A2, B2, C2, 'sail')
    b.beam((0, .85, 2.35), (0, -3.5, 2.3), .08, 'wood_dark')
    for k in range(3):
        b.box((0, -.6 - k * .7, 4 + k * .8), (.04, .5, .04), 'banner_red')
    b.quad((0, .8, 10.0), (0, .8, 9.5), (0, -.5, 9.7), (0, -.5, 10.0), 'banner_red')
    b.quad((0, -.5, 10.0), (0, -.5, 9.7), (0, .8, 9.5), (0, .8, 10.0), 'banner_red')
    return b.build(bevel=.012)


def lighthouse(name):
    b = Builder(name)
    b.box((0, 0, .6), (9.0, 9.0, 1.2), 'wall_stone')
    b.box((0, -3.2, 2.2), (4.2, 3.0, 2.6), 'trim_white')
    b.gable_roof((0, -3.2, 3.5), 4.6, 3.4, 1.8, 'roof_red', over=.4, overL=.3, thick=.12, kick=.2, segs=3)
    b.cyl((0, 0, 1.2), 2.6, 5.5, 'trim_white', 20, r2=2.2)
    b.cyl((0, 0, 6.7), 2.25, 3.2, 'lighthouse_red', 20, r2=2.0)
    b.cyl((0, 0, 9.9), 2.0, 3.4, 'trim_white', 20, r2=1.8)
    b.cyl((0, 0, 13.3), 1.75, 3.0, 'lighthouse_red', 20, r2=1.6)
    b.cyl((0, 0, 16.3), 2.5, .35, 'iron', 20)
    for k in range(16):
        a = k * math.pi / 8
        b.box((math.cos(a) * 2.35, math.sin(a) * 2.35, 17.0), (.08, .08, 1.0), 'iron')
    b.cyl((0, 0, 16.65), 1.6, 2.2, 'lantern_glow', 16, r2=1.5)
    b.cyl((0, 0, 18.9), 1.9, .3, 'iron', 16)
    b.cone((0, 0, 19.2), 1.9, 2.0, 'lighthouse_red', 16)
    b.sphere((0, 0, 21.4), .25, 'gold', 6, 8)
    for z in (4.0, 8.0, 11.5, 14.8):
        b.box((0, -2.18 + (z - 4) * .01, z), (.55, .1, .9), 'glass')
    b.box((0, -3.6, 1.0), (1.2, .1, 2.0), 'door_wood') if False else None
    return b.build(bevel=.03)


def fish_stall(name, awn=('awning_blue', 'awning_white')):
    b = Builder(name)
    W, D = 4.2, 2.4
    for sx in (-1, 1):
        b.box((sx * (W / 2 - .1), D / 2 - .1, 1.5), (.14, .14, 3.0), 'wood_dark')
        b.box((sx * (W / 2 - .1), -D / 2 + .1, 1.2), (.14, .14, 2.4), 'wood_dark')
    b.box((0, -D / 2 + .6, .85), (W - .1, 1.0, .12), 'wood_light')
    b.box((0, -D / 2 + .1, .45), (W - .2, .08, .85), 'wood_dark')
    # sloped awning
    n = 10
    for i in range(n):
        x0 = -W / 2 - .25 + (W + .5) * i / n
        x1 = -W / 2 - .25 + (W + .5) * (i + 1) / n
        m = awn[0] if i % 2 == 0 else awn[1]
        b.quad((x0, -D / 2 - .4, 2.2), (x1, -D / 2 - .4, 2.2), (x1, D / 2 + .1, 3.05), (x0, D / 2 + .1, 3.05), m)
        b.quad((x1, -D / 2 - .4, 2.2), (x0, -D / 2 - .4, 2.2), (x0, D / 2 + .1, 3.05), (x1, D / 2 + .1, 3.05), m)
        b.quad((x0, -D / 2 - .4, 2.2), (x1, -D / 2 - .4, 2.2), (x1 - .02, -D / 2 - .45, 1.96), (x0 + .02, -D / 2 - .45, 1.96), m)
        b.quad((x1, -D / 2 - .4, 2.2), (x0, -D / 2 - .4, 2.2), (x0 + .02, -D / 2 - .45, 1.96), (x1 - .02, -D / 2 - .45, 1.96), m)
    # tubs of fish and crates of crabs/prawns
    for k in range(4):
        x = -W / 2 + .55 + k * 1.0
        b.cyl((x, -D / 2 + .6, .91), .38, .3, 'crate_wood', 12, r2=.42)
        for j in range(5):
            a = j * 1.25
            b.sphere((x + math.cos(a) * .15, -D / 2 + .6 + math.sin(a) * .12, 1.24), .1, 'fish' if j % 2 else 'fruit_orange', 5, 6, sq=.6)
    # hanging fish and net
    b.beam((-W / 2 + .2, D / 2 - .3, 2.5), (W / 2 - .2, D / 2 - .3, 2.5), .05, 'rope')
    for k in range(7):
        x = -W / 2 + .5 + k * .52
        b.beam((x, D / 2 - .3, 2.5), (x, D / 2 - .3, 2.1), .015, 'rope')
        b.sphere((x, D / 2 - .3, 1.95), .1, 'fish', 5, 6, sq=2.0)
    b.quad((-1.2, D / 2 - .2, 2.4), (1.2, D / 2 - .2, 2.4), (1.0, D / 2 - .2, 1.2), (-1.0, D / 2 - .2, 1.2), 'net') if False else None
    b.box((W / 2 - .3, -D / 2 - .1, 1.8), (.2, .2, .3), 'lantern_glow')
    return b.build(bevel=.02)


def net_rack(name):
    b = Builder(name)
    for sx in (-1, 1):
        b.cyl((sx * 1.6, 0, 0), .1, 2.8, 'wood_dark', 8)
    b.beam((-1.7, 0, 2.7), (1.7, 0, 2.7), .08, 'wood_dark')
    b.beam((-1.5, 0, 1.2), (1.5, 0, 1.2), .06, 'wood_dark')
    b.box((0, 0, 1.9), (2.8, .05, 1.6), 'net')
    for k in range(6):
        b.sphere((-1.2 + k * .5, 0, 1.4 + (k % 2) * .35), .1, 'trim_white', 5, 6)
    return b.build(bevel=0)


def crate_fish(name):
    b = Builder(name)
    b.box((0, 0, .3), (1.0, .7, .6), 'crate_wood')
    b.box((1.2, .1, .25), (.8, .6, .5), 'crate_wood', rz=14)
    for j in range(7):
        b.sphere((-.3 + (j % 4) * .2, -.1 + (j // 4) * .2, .62), .1, 'fish', 5, 6, sq=.55)
    b.cyl((-1.1, .3, 0), .35, .7, 'wood_dark', 10)
    b.cyl((-1.1, .3, .7), .3, .08, 'net', 10) if False else None
    b.sphere((.2, -1.0, .2), .35, 'net', 6, 8, sq=.5)
    return b.build(bevel=.015)


def wall_section(name, L=12.0):
    b = Builder(name)
    T, Hh = 4.0, 15.0
    b.box((0, 0, Hh / 2), (L, T, Hh), 'wall_stone', taper=.97)
    b.box((0, 0, 1.0), (L + .2, T + 1.0, 2.0), 'wall_stone')            # plinth
    # string courses and corbelled parapet
    for z in (5.0, 10.0):
        b.box((0, -T / 2 - .12, z), (L + .1, .3, .35), 'stone_light')
        b.box((0, T / 2 + .12, z), (L + .1, .3, .35), 'stone_light')
    b.box((0, 0, Hh + .2), (L + .1, T + .9, .5), 'stone_light')
    for sy in (-1, 1):
        n = int(L / 1.5)
        for k in range(n):
            x = -L / 2 + .75 + k * L / n
            if k % 2 == 0:
                b.box((x, sy * (T / 2 + .35), Hh + 1.2), (.95, .7, 1.7), 'stone_light')
            b.box((x, sy * (T / 2 + .1), Hh - .4), (.5, .5, .9), 'stone_light', taper=.7) if False else None
    # buttresses
    for sx in (-L / 2 + .6, L / 2 - .6):
        b.box((sx, -T / 2 - .55, 5.0), (1.2, 1.2, 10.0), 'stone_light', taper=.85)
        b.box((sx, -T / 2 - .55, 10.3), (1.4, 1.4, .4), 'stone_light')
    # arrow slits and teal banners
    for k in range(2):
        x = -L / 4 + k * L / 2
        b.box((x, -T / 2 - .05, 8.5), (.35, .1, 2.2), 'iron')
        b.box((x, -T / 2 - .05, 4.0), (.35, .1, 2.2), 'iron')
    b.box((0, -T / 2 - .08, 11.5), (2.2, .08, 3.6), 'banner_blue')
    b.box((0, -T / 2 - .12, 12.0), (1.0, .06, 1.1), 'trim_gold')
    # ivy cards as flat green patches
    for k in range(5):
        b.sphere((-L / 2 + 1.5 + k * 2.2 + rnd(-.3, .3), -T / 2 - .25, 1.8 + rnd(0, 3.5)), rnd(.5, 1.1), 'leaf', 5, 6, sq=.8)
    return b.build(bevel=.04)


def wall_tower(name, R=5.0, Hh=24.0):
    b = Builder(name)
    b.cyl((0, 0, 0), R + .8, 2.2, 'wall_stone', 20, r2=R + .4)
    b.cyl((0, 0, 2.0), R, Hh - 2, 'wall_stone', 20, r2=R * .96)
    for z in (6.5, 12.5, 18.0):
        b.cyl((0, 0, z), R * 1.03, .5, 'stone_light', 20)
    # machicolation crown
    b.cyl((0, 0, Hh - 2.4), R * 1.02, 1.8, 'stone_light', 20, r2=R * 1.22)
    b.cyl((0, 0, Hh - .6), R * 1.22, 1.2, 'wall_stone', 20)
    for k in range(14):
        a = k * 2 * math.pi / 14
        b.box((math.cos(a) * R * 1.2, math.sin(a) * R * 1.2, Hh + 1.6), (1.4, .9, 1.9), 'stone_light', rz=math.degrees(a) + 90)
    # teal cone
    b.cyl((0, 0, Hh + .6), R * .86, 1.2, 'wall_stone', 20, r2=R * .8)
    b.cone((0, 0, Hh + 1.8), R * 1.0, 11.0, 'roof_teal', 20, flare=.1)
    b.sphere((0, 0, Hh + 13.0), .5, 'gold', 6, 8)
    b.beam((0, 0, Hh + 13.2), (0, 0, Hh + 16.0), .1, 'iron')
    b.quad((0, 0, Hh + 16.0), (3.0, 0, Hh + 15.5), (3.0, 0, Hh + 14.2), (0, 0, Hh + 14.5), 'banner_blue')
    b.quad((0, 0, Hh + 14.5), (3.0, 0, Hh + 14.2), (3.0, 0, Hh + 15.5), (0, 0, Hh + 16.0), 'banner_blue')
    for z in (9.5, 15.5, 21.0):
        for k in range(6):
            a = k * math.pi / 3 + .3
            b.box((math.cos(a) * (R - .02), math.sin(a) * (R - .02), z), (.5, .5, 1.8), 'glass', rz=math.degrees(a))
            b.box((math.cos(a) * (R + .04), math.sin(a) * (R + .04), z + .3), (.8, .15, 2.4), 'stone_light', rz=math.degrees(a) + 90) if False else None
    return b.build(bevel=.03)


def run():
    only = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    specs = [
        ('Harbor_A', lambda n: harbor_house(n, 5.6, 5.4, 3, 'paint_yellow', 'roof_red', 'shutter_blue', None, True)),
        ('Harbor_B', lambda n: harbor_house(n, 6.2, 5.6, 3, 'paint_blue', 'roof_red', 'shutter_red', None, False)),
        ('Harbor_C', lambda n: harbor_house(n, 6.0, 5.4, 2, 'paint_red', 'roof_blue', 'shutter_yellow', ('awning_red', 'awning_white'), False)),
        ('Harbor_D', lambda n: harbor_house(n, 5.2, 5.2, 3, 'paint_pink', 'roof_blue', 'shutter_green', None, True)),
        ('Harbor_E', lambda n: harbor_house(n, 7.0, 6.0, 2, 'paint_teal', 'roof_red', 'shutter_red', ('awning_blue', 'awning_yellow'), False)),
        ('Harbor_F', lambda n: harbor_house(n, 5.8, 5.4, 3, 'paint_orange', 'roof_blue', 'shutter_blue', None, True)),
        ('Harbor_G', lambda n: harbor_house(n, 6.4, 5.6, 2, 'paint_green', 'roof_red', 'shutter_yellow', ('awning_green', 'awning_white'), False)),
        ('Harbor_H', lambda n: harbor_house(n, 5.0, 5.0, 4, 'paint_purple', 'roof_blue', 'shutter_yellow', None, True, True, .4)),
        ('Harbor_I', lambda n: harbor_house(n, 6.6, 5.8, 3, 'paint_cream', 'roof_red', 'shutter_green', None, True)),
        ('Dock_8', lambda n: dock(n, 8.0)),
        ('Boat_Row', lambda n: row_boat(n)), ('Boat_RowRed', lambda n: row_boat(n, 'hull_red')),
        ('Boat_FishRed', lambda n: fishing_boat(n, 'hull_red', 'trim_white', 'roof_blue')),
        ('Boat_FishBlue', lambda n: fishing_boat(n, 'hull_blue', 'paint_yellow', 'roof_red')),
        ('Boat_FishTeal', lambda n: fishing_boat(n, 'hull_teal', 'paint_cream', 'roof_red')),
        ('Boat_Sail', lambda n: sail_boat(n, 'hull_blue')), ('Boat_SailRed', lambda n: sail_boat(n, 'hull_red')),
        ('Prop_Lighthouse', lighthouse),
        ('Prop_FishStallBlue', lambda n: fish_stall(n, ('awning_blue', 'awning_white'))), ('Prop_FishStallRed', lambda n: fish_stall(n, ('awning_red', 'awning_white'))),
        ('Prop_NetRack', net_rack), ('Prop_CrateFish', crate_fish),
        ('Wall_Section', wall_section), ('Wall_Tower', wall_tower),
    ]
    for name, fn in specs:
        if only and name not in only:
            continue
        B.clear()
        o = fn(name)
        B.export([o], name)
        big = name.startswith(('Prop_Lighthouse', 'Wall_', 'Boat_Sail'))
        boat = name.startswith('Boat')
        loc = (-30, -42, 22) if name.startswith('Wall_Tower') else (-26, -32, 14) if big else (-12, -15, 7) if boat else (-12, -16, 7)
        tgt = (0, 0, 12 if name.startswith(('Wall_Tower', 'Prop_Lighthouse')) else 5 if big else 2.5)
        B.preview("D:/GAME/artifacts/living-world/blender_" + name + ".png", [o], loc=loc, target=tgt, size=(640, 520))
        print('EXPORTED', name)


if __name__ == '__main__':
    run()
