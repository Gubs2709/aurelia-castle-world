import sys, math
sys.path.insert(0, "D:/GAME/tools/blender")
import importlib
import blendlib as B
importlib.reload(B)
from blendlib import Builder, rnd
import bpy

FH = 2.8        # floor height
PL = 0.55       # stone plinth


def window(b, x, y, z, face, w=0.95, h=1.25, shutter='shutter_green', flowers=True, depth=.09):
    """face: -1 front (-Y) / +1 back (+Y) / 'L' (-X) / 'R' (+X).  (x,y,z) is the wall point of the window centre."""
    # orientation helpers: u = along wall, n = outward normal
    if face in (-1, 1):
        n = (0, face, 0)
        def P(u, v, t): return (x + u, y + n[1] * t, z + v)
        rot = 0
        size = lambda sw, sh, st: (sw, st, sh)
    else:
        sgn = -1 if face == 'L' else 1
        n = (sgn, 0, 0)
        def P(u, v, t): return (x + sgn * t, y + u, z + v)
        size = lambda sw, sh, st: (st, sw, sh)
    b.box(P(0, 0, depth / 2), size(w + .22, h + .22, depth), 'timber')                     # frame
    b.box(P(0, 0, depth), size(w, h, .05), 'glass')
    b.box(P(0, 0, depth + .03), size(.06, h, .04), 'timber')
    b.box(P(0, 0, depth + .03), size(w, .06, .04), 'timber')
    b.box(P(0, -h / 2 - .07, .13), size(w + .4, .1, .3), 'wall_stone')                      # sill
    if shutter:
        for s in (-1, 1):
            b.box(P(s * (w / 2 + .27), 0, depth + .02), size(.5, h + .1, .06), shutter)
            b.box(P(s * (w / 2 + .27), .12, depth + .06), size(.4, .05, .03), 'timber')
    if flowers:
        b.box(P(0, -h / 2 - .22, .27), size(w + .15, .26, .3), 'wood_dark')
        for i in range(7):
            u = (i - 3) * (w * .15)
            col = ['flower_pink', 'flower_yellow', 'flower_red', 'flower_purple', 'flower_white'][(i + int(abs(x * 3 + z))) % 5]
            b.sphere(P(u, -h / 2 - .02, .28), rnd(.1, .15), col, 5, 6)
            b.sphere(P(u + .05, -h / 2 - .12, .3), .11, 'leaf', 5, 6)


def door(b, x, z, y_wall, face=-1, w=1.1, h=2.2, arch=True):
    s = face
    b.box((x, y_wall + s * .06, z + h / 2), (w + .5, .12, h + .35), 'wall_stone')
    b.box((x, y_wall + s * .13, z + h / 2 - .05), (w, .08, h - .1), 'door_wood')
    if arch:
        b.cyl((x, y_wall + s * .12, z + h - .1), w / 2, .1, 'door_wood', 14, axis='y')
    for dx in (-.28, 0, .28):
        b.box((x + dx, y_wall + s * .19, z + h / 2 - .05), (.05, .04, h - .15), 'timber')
    b.box((x + .34, y_wall + s * .2, z + h / 2 - .1), (.1, .06, .1), 'iron')
    # stone steps
    for i in range(3):
        b.box((x, y_wall + s * (.3 + i * .32), z - .12 - .0 * i + (-i * .15) + .15), (w + .9 - i * .1, .5 + .0, .18), 'wall_stone')
    # little pitched hood
    b.box((x, y_wall + s * .5, z + h + .2), (w + 1.2, 1.1, .1), 'wood_dark', rx=s * 18 * -1 if False else 0)
    b.beam((x - w / 2 - .5, y_wall + s * .1, z + h), (x - w / 2 - .5, y_wall + s * .95, z + h + .35), .09, 'timber')
    b.beam((x + w / 2 + .5, y_wall + s * .1, z + h), (x + w / 2 + .5, y_wall + s * .95, z + h + .35), .09, 'timber')


def lantern(b, x, y, z, face=-1):
    s = face
    b.beam((x, y, z + .25), (x, y + s * .55, z + .25), .05, 'iron')
    b.beam((x, y + s * .55, z + .25), (x, y + s * .55, z + .05), .03, 'iron')
    b.box((x, y + s * .55, z - .1), (.22, .22, .3), 'lantern_glow')
    b.box((x, y + s * .55, z + .08), (.3, .3, .05), 'iron')
    b.box((x, y + s * .55, z - .28), (.26, .26, .04), 'iron')


def sign(b, x, y, z, face=-1, mat='banner_blue'):
    s = face
    b.beam((x, y, z + .5), (x, y + s * .9, z + .5), .05, 'iron')
    b.box((x, y + s * .85, z), (.06, .06, .9), 'iron')
    b.box((x, y + s * .85, z - .12), (.07, .75, .9), mat)
    b.box((x, y + s * .85, z - .12), (.09, .45, .5), 'trim_gold')


def awning(b, x, y_wall, z, width, depth, mat1, mat2, face=-1):
    s = face
    n = 8
    for i in range(n):
        u0 = x - width / 2 + width * i / n
        u1 = x - width / 2 + width * (i + 1) / n
        m = mat1 if i % 2 == 0 else mat2
        b.quad((u0, y_wall + s * depth, z - .5), (u1, y_wall + s * depth, z - .5), (u1, y_wall, z), (u0, y_wall, z), m) if s == -1 else b.quad((u1, y_wall + s * depth, z - .5), (u0, y_wall + s * depth, z - .5), (u0, y_wall, z), (u1, y_wall, z), m)
        b.quad((u0, y_wall + s * depth, z - .5), (u1, y_wall + s * depth, z - .5), (u1, y_wall, z), (u0, y_wall, z), m) if s == 1 else b.quad((u1, y_wall + s * depth, z - .5), (u0, y_wall + s * depth, z - .5), (u0, y_wall, z), (u1, y_wall, z), m)
        # scalloped valance
        b.quad((u0, y_wall + s * depth, z - .5), (u1, y_wall + s * depth, z - .5), (u1, y_wall + s * depth, z - .75), (u0, y_wall + s * depth, z - .75), m) if s == -1 else b.quad((u1, y_wall + s * depth, z - .5), (u0, y_wall + s * depth, z - .5), (u0, y_wall + s * depth, z - .75), (u1, y_wall + s * depth, z - .75), m)
    for dx in (-width / 2 + .1, width / 2 - .1):
        b.beam((x + dx, y_wall + s * (depth - .05), z - .75), (x + dx, y_wall + s * (depth - .05), z - 2.4), .1, 'timber')


def chimney(b, x, y, z0, z1):
    b.box((x, y, (z0 + z1) / 2), (.95, .95, z1 - z0), 'wall_stone')
    b.box((x, y, z1 + .1), (1.15, 1.15, .22), 'wall_stone')
    for dx in (-.2, .2):
        b.box((x + dx, y, z1 + .45), (.3, .3, .5), 'wall_stone')


def dormer(b, x, y_roof, z_roof, w=1.5, face=-1, roof='roof_slate', shutter='shutter_blue'):
    s = face
    h = 1.35
    b.box((x, y_roof, z_roof + h / 2 - .15), (w, 1.4, h), 'wall_plaster')
    window(b, x, y_roof + s * .7, z_roof + h / 2 - .05, s, w=.7, h=.85, shutter=shutter, flowers=False)
    # tiny gable roof
    n = 6
    ang = 40
    rise = (w / 2 + .25) * math.tan(math.radians(ang))
    c = (x, y_roof + s * .1, z_roof + h - .15)
    b.gable_roof((c[0], c[1], c[2]), 1.6, w + .2, rise * .8, roof, over=.28, overL=.35, kick=.15, segs=3)
    # rotate? the dormer ridge must run along Y (into the roof) - handled by building as a rotated copy below


def timber_frame_front(b, x0, x1, y, z0, z1, face=-1, density=1.35, braces=True, mat='timber'):
    """Half-timber: vertical studs, rails, diagonal braces on a wall plane at y."""
    s = face
    w = x1 - x0
    n = max(2, round(w / density))
    for i in range(n + 1):
        x = x0 + w * i / n
        b.box((x, y + s * .04, (z0 + z1) / 2), (.14, .1, z1 - z0), mat)
    for z in (z0, z1):
        b.box(((x0 + x1) / 2, y + s * .04, z), (w + .14, .1, .16), mat)
    b.box(((x0 + x1) / 2, y + s * .04, (z0 + z1) / 2 + (z1 - z0) * .18), (w, .09, .12), mat) if (z1 - z0) > 2 else None
    if braces:
        for i in range(n):
            if (i + int(x0)) % 2 == 0:
                xa = x0 + w * i / n
                xb = x0 + w * (i + 1) / n
                if i % 4 == 0:
                    b.beam((xa, y + s * .06, z0), (xb, y + s * .06, z1), .11, mat)
                else:
                    b.beam((xa, y + s * .06, z1), (xb, y + s * .06, z0), .11, mat)


def tudor(name, w=9.0, d=6.5, floors=3, jetty=.5, roofmat='roof_terracotta', shutter='shutter_green', shop=False, awn=('awning_red', 'awning_white'),
          dorm=True, chim=True, plaster='wall_plaster', rise=None, banner=None, lantern_on=True):
    b = Builder(name)
    base_y = 0.0
    # stone plinth
    b.box((0, 0, PL / 2), (w + .3, d + .3, PL), 'wall_stone')
    # floors; ground floor occupies (-d/2 .. d/2); upper floors jetty toward front (-Y)
    zs = [PL + i * FH for i in range(floors + 1)]
    fronts = []
    for f in range(floors):
        jy = jetty * f
        dd = d + jy
        cy = -jy / 2
        z0, z1 = zs[f], zs[f + 1]
        wx = w + (0 if f == 0 else .2)
        b.box((0, cy, (z0 + z1) / 2), (wx, dd, z1 - z0), plaster)
        fronts.append((cy - dd / 2, wx, z0, z1))
        # floor beam band (projecting joist ends)
        b.box((0, cy - dd / 2 + .02, z0 + .02), (wx + .35, .36, .22), 'timber')
        if f > 0:
            # joist ends
            for i in range(int(wx / .55)):
                xx = -wx / 2 + .3 + i * .55
                b.box((xx, -d / 2 - jetty * f * .5 - .1, z0 - .05), (.16, jetty * f + .3, .2), 'timber')
            # corner brackets
            for sx in (-1, 1):
                b.beam((sx * (wx / 2 - .1), -d / 2 + .0, z0 - .8), (sx * (wx / 2 - .1), -d / 2 - jetty * f + .05, z0), .17, 'timber')
    # front/back/side frames + windows
    for f in range(floors):
        yfront, wx, z0, z1 = fronts[f]
        timber_frame_front(b, -wx / 2, wx / 2, yfront, z0 + .02, z1 - .02, -1)
        ybk = d / 2
        timber_frame_front(b, -wx / 2, wx / 2, ybk, z0 + .02, z1 - .02, 1, braces=False)
        # side frames drawn as vertical studs only (L/R)
        for sx in (-1, 1):
            nz = max(2, round((d + jetty * f) / 1.5))
            for i in range(nz + 1):
                y = -d / 2 - jetty * f + (d + jetty * f) * i / nz
                b.box((sx * (wx / 2 + .03), y, (z0 + z1) / 2), (.1, .14, z1 - z0), 'timber')
            b.box((sx * (wx / 2 + .03), -jetty * f / 2, z0 + .05), (.1, d + jetty * f, .16), 'timber')
            b.box((sx * (wx / 2 + .03), -jetty * f / 2, z1 - .05), (.1, d + jetty * f, .16), 'timber')
        nwin = max(2, int(wx / 3.0))
        for i in range(nwin):
            wxp = -wx / 2 + wx * (i + .5) / nwin
            skip = (f == 0 and i == nwin // 2 and not shop)
            if skip:
                continue
            if f == 0 and shop:
                continue
            window(b, wxp, yfront, z0 + 1.55, -1, shutter=shutter, flowers=True)
        for i in range(max(1, int(wx / 3.5))):
            window(b, -wx / 2 + wx * (i + .5) / max(1, int(wx / 3.5)), d / 2, z0 + 1.55, 1, shutter=shutter, flowers=(f == 0))
        for sx, face in ((-1, 'L'), (1, 'R')):
            for i in range(max(1, int(d / 3.5))):
                yy = -d / 2 - jetty * f + (d + jetty * f) * (i + .5) / max(1, int(d / 3.5))
                window(b, sx * (wx / 2 + .03), yy, z0 + 1.55, face, w=.85, shutter=shutter, flowers=(f == 0 and i % 2 == 0))
    # ground floor door / shop front
    if shop:
        yfront = fronts[0][0]
        # shop arcade: big counter window + door
        b.box((-w * .22, yfront + .1, PL + 1.2), (w * .4, .2, 1.6), 'glass')
        b.box((-w * .22, yfront + .08, PL + 1.2), (w * .4 + .2, .1, 1.8), 'timber')
        b.box((-w * .22, yfront - .25, PL + .9), (w * .42, .5, .12), 'wood_light')
        door(b, w * .26, PL, yfront, -1)
        awning(b, -w * .22, yfront, PL + 2.45, w * .46, 1.4, awn[0], awn[1])
        for i, c in enumerate(['fruit_orange', 'fruit_red', 'fruit_green', 'fruit_yellow']):
            for j in range(5):
                b.sphere((-w * .22 - w * .16 + j * w * .08, yfront - .25 + (i % 2) * .06, PL + 1.05 + .08 * (i % 2)), .09, c, 5, 6)
    else:
        door(b, 0, PL, fronts[0][0], -1)
    if lantern_on:
        lantern(b, w / 2 - .6, fronts[0][0], PL + 2.6, -1)
        lantern(b, -w / 2 + .6, fronts[min(1, floors - 1)][0], zs[min(1, floors - 1)] + 1.6 if floors > 1 else PL + 2.6, -1)
    # roof
    Z = zs[-1]
    top_depth = d + jetty * (floors - 1)
    rise = rise or (top_depth / 2) * 1.35
    cy = -jetty * (floors - 1) / 2
    top_w = w + (0 if floors == 1 else .2)
    b.box((0, cy, Z + .06), (top_w + .5, top_depth + .5, .22), 'timber')   # wall plate
    rz = b.gable_roof((0, cy, Z + .15), top_w, top_depth, rise, roofmat, over=.75, overL=.55, thick=.16, kick=.5)
    # gable infill + framing (triangles both ends)
    for sx in (-1, 1):
        x = sx * top_w / 2
        b.tri((x, cy - top_depth / 2, Z + .15), (x, cy + top_depth / 2, Z + .15), (x, cy, Z + .15 + rise), plaster)
        for i in range(-1, 2):
            b.beam((x + sx * .04, cy + i * top_depth / 4, Z + .15), (x + sx * .04, cy + i * top_depth / 8, Z + .15 + rise * (1 - abs(i) * .5)), .13, 'timber')
        b.beam((x + sx * .04, cy - top_depth / 2, Z + .15), (x + sx * .04, cy, Z + .15 + rise), .16, 'timber')
        b.beam((x + sx * .04, cy + top_depth / 2, Z + .15), (x + sx * .04, cy, Z + .15 + rise), .16, 'timber')
        b.box((x + sx * .04, cy, Z + .15 + rise * .3), (.1, top_depth * .72, .14), 'timber')
        gz = Z + .15 + rise * .55
        window(b, x, cy, gz, 'L' if sx < 0 else 'R', w=.7, h=.9, shutter=shutter, flowers=False)
    # chimney
    if chim:
        chimney(b, w / 2 - 1.1, cy + 0.2, Z + .1, Z + .15 + rise + 1.0)
    # dormers on the front slope
    if dorm and top_w > 7:
        nd = 2 if top_w > 8.5 else 1
        for i in range(nd):
            xd = (-top_w * .22 + i * top_w * .44) if nd == 2 else 0
            # front slope point at 55% towards eave
            t = .42
            yd = cy - (top_depth / 2 + .75) * (1 - t)
            zd = Z + .15 + rise * ((top_depth / 2 + .75) / (top_depth / 2)) * t
            dw = 1.5
            b.box((xd, yd + .15, zd + .55), (dw, 1.1, 1.3), plaster)
            window(b, xd, yd - .4, zd + .62, -1, w=.7, h=.9, shutter='shutter_blue', flowers=False)
            # dormer roof (ridge along Y)
            r = .75
            for sx in (-1, 1):
                b.quad((xd + sx * (dw / 2 + .2), yd - .5, zd + 1.15), (xd, yd - .55, zd + 1.15 + r), (xd, yd + .7, zd + 1.15 + r), (xd + sx * (dw / 2 + .2), yd + .7, zd + 1.15), roofmat) if sx == -1 else b.quad((xd, yd - .55, zd + 1.15 + r), (xd + sx * (dw / 2 + .2), yd - .5, zd + 1.15), (xd + sx * (dw / 2 + .2), yd + .7, zd + 1.15), (xd, yd + .7, zd + 1.15 + r), roofmat)
            b.tri((xd - dw / 2 - .2, yd - .5, zd + 1.15), (xd + dw / 2 + .2, yd - .5, zd + 1.15), (xd, yd - .55, zd + 1.15 + r), 'wood_dark')
    # banner and sign
    if banner:
        yf = fronts[min(1, floors - 1)][0]
        zb = zs[min(1, floors - 1)] + 1.0
        b.box((-w / 2 + .5, yf - .25, zb + 1.4), (.07, .55, .07), 'iron') if False else None
        b.beam((-w / 2 + .3, yf, zb + 1.8), (-w / 2 + .3, yf - .8, zb + 1.8), .05, 'iron')
        b.box((-w / 2 + .3, yf - .8, zb + .5), (.06, .7, 2.2), banner)
        b.box((-w / 2 + .3, yf - .8, zb + .5), (.08, .35, .8), 'trim_gold')
    if shop:
        sign(b, -w / 2 + .2, fronts[0][0], PL + 3.2, -1, 'banner_yellow')
    # flower pots by the door
    for sx in (-1, 1):
        b.cyl((sx * 1.2 + (w * .26 if shop else 0), fronts[0][0] - 1.15, PL - .45), .22, .4, 'wood_dark', 10)
        for k in range(4):
            b.sphere((sx * 1.2 + (w * .26 if shop else 0) + rnd(-.15, .15), fronts[0][0] - 1.15 + rnd(-.1, .1), PL - .02 + rnd(0, .2)), .17, ['flower_pink', 'flower_yellow', 'flower_red', 'flower_purple'][k], 5, 6)
    return b.build(bevel=.025)


def cottage(name, w=7.5, d=6.2, roofmat='roof_slate', stone=True, porch=True, shutter='shutter_blue'):
    """Steep-roofed stone/timber cottage with a covered wooden porch and dormers (reference download (2))."""
    b = Builder(name)
    wall = 'wall_stone' if stone else 'wall_plaster'
    b.box((0, 0, PL / 2), (w + .3, d + .3, PL), 'wall_stone')
    H = 3.0
    b.box((0, 0, PL + H / 2), (w, d, H), wall)
    # stone quoins at corners + timber upper gable
    for sx in (-1, 1):
        for sy in (-1, 1):
            for k in range(6):
                b.box((sx * w / 2, sy * d / 2, PL + .3 + k * .5), (.5 + (k % 2) * .12, .5 + ((k + 1) % 2) * .12, .46), 'stone_light')
    rise = d / 2 * 1.5
    Z = PL + H
    b.box((0, 0, Z + .05), (w + .5, d + .5, .2), 'timber')
    b.gable_roof((0, 0, Z + .1), w, d, rise, roofmat, over=.9, overL=.7, thick=.18, kick=.6)
    for sx in (-1, 1):
        x = sx * w / 2
        b.tri((x, -d / 2, Z + .1), (x, d / 2, Z + .1), (x, 0, Z + .1 + rise), 'wall_plaster')
        b.beam((x + sx * .05, -d / 2, Z + .1), (x + sx * .05, 0, Z + .1 + rise), .16, 'timber')
        b.beam((x + sx * .05, d / 2, Z + .1), (x + sx * .05, 0, Z + .1 + rise), .16, 'timber')
        b.box((x + sx * .05, 0, Z + .1 + rise * .35), (.1, d * .7, .14), 'timber')
        b.beam((x + sx * .05, -d * .2, Z + .1 + rise * .35), (x + sx * .05, 0, Z + .1 + rise * .72), .12, 'timber')
        b.beam((x + sx * .05, d * .2, Z + .1 + rise * .35), (x + sx * .05, 0, Z + .1 + rise * .72), .12, 'timber')
        window(b, x, 0, Z + .1 + rise * .22, 'L' if sx < 0 else 'R', w=.7, h=.85, shutter=shutter, flowers=False)
    # windows ground floor
    for i in range(2):
        window(b, -w * .27 + i * w * .54 if False else (-w * .3 if i == 0 else w * .3), -d / 2, PL + 1.55, -1, shutter=shutter, flowers=True, w=1.0)
    for sx, face in ((-1, 'L'), (1, 'R')):
        for i in range(2):
            window(b, sx * w / 2, -d / 4 + i * d / 2, PL + 1.55, face, w=.9, shutter=shutter, flowers=i == 0)
    for i in range(2):
        window(b, -w * .25 + i * w * .5, d / 2, PL + 1.55, 1, shutter=shutter, flowers=False)
    door(b, 0, PL, -d / 2, -1, w=1.0)
    # wooden porch with railing
    pz = PL
    pw = w * .62
    pd = 2.4
    b.box((0, -d / 2 - pd / 2, pz + .08), (pw, pd, .16), 'wood_light')
    for sx in (-1, 1):
        for sy in (0, 1):
            x = sx * (pw / 2 - .12)
            y = -d / 2 - (pd - .12) * sy
            b.box((x, y, pz + 1.6), (.2, .2, 3.1), 'wood_dark')
    for sx in (-1, 1):
        x = sx * (pw / 2 - .12)
        b.box((x, -d / 2 - pd / 2, pz + 2.95), (.18, pd, .22), 'wood_dark')
    b.box((0, -d / 2 - pd + .12, pz + 2.95), (pw, .2, .22), 'wood_dark')
    # porch roof lean-to
    z0 = pz + 3.05
    b.quad((-pw / 2 - .4, -d / 2 - pd - .55, z0 - .3), (pw / 2 + .4, -d / 2 - pd - .55, z0 - .3), (pw / 2 + .4, -d / 2 + .1, z0 + 1.1), (-pw / 2 - .4, -d / 2 + .1, z0 + 1.1), roofmat)
    b.quad((-pw / 2 - .4, -d / 2 + .1, z0 + 1.0), (pw / 2 + .4, -d / 2 + .1, z0 + 1.0), (pw / 2 + .4, -d / 2 - pd - .55, z0 - .4), (-pw / 2 - .4, -d / 2 - pd - .55, z0 - .4), 'wood_dark')
    for i in range(int(pw / .35) + 1):
        x = -pw / 2 + .12 + i * .35
        if abs(x) < pw / 2:
            b.box((x, -d / 2 - pd + .12, pz + .65), (.07, .07, 1.0), 'wood_light')
            b.box((x, -d / 2 - pd / 2 - .0 + 1.0, pz + .0 - 1) , (0.001, .001, .001), 'wood_light') if False else None
    b.box((0, -d / 2 - pd + .12, pz + 1.15), (pw, .1, .1), 'wood_light')
    for sx in (-1, 1):
        for i in range(5):
            y = -d / 2 - .25 - i * .45
            b.box((sx * (pw / 2 - .12), y, pz + .65), (.07, .07, 1.0), 'wood_light')
        b.box((sx * (pw / 2 - .12), -d / 2 - pd / 2, pz + 1.15), (.1, pd, .1), 'wood_light')
    # chimney (stone) on side
    chimney(b, w / 2 - .9, 0, Z, Z + rise + .9)
    # dormers on front slope
    for xd in (-w * .26, w * .26):
        t = .4
        yd = -(d / 2 + .9) * (1 - t)
        zd = Z + .1 + rise * ((d / 2 + .9) / (d / 2)) * t
        dw = 1.5
        b.box((xd, yd + .2, zd + .55), (dw, 1.2, 1.3), 'wall_plaster')
        window(b, xd, yd - .4, zd + .6, -1, w=.7, h=.9, shutter='shutter_red', flowers=False)
        r = .8
        b.quad((xd - dw / 2 - .25, yd - .55, zd + 1.15), (xd, yd - .6, zd + 1.15 + r), (xd, yd + .8, zd + 1.15 + r), (xd - dw / 2 - .25, yd + .8, zd + 1.15), roofmat)
        b.quad((xd, yd - .6, zd + 1.15 + r), (xd + dw / 2 + .25, yd - .55, zd + 1.15), (xd + dw / 2 + .25, yd + .8, zd + 1.15), (xd, yd + .8, zd + 1.15 + r), roofmat)
        b.tri((xd - dw / 2 - .25, yd - .55, zd + 1.15), (xd + dw / 2 + .25, yd - .55, zd + 1.15), (xd, yd - .6, zd + 1.15 + r), 'wood_dark')
    lantern(b, w * .22, -d / 2, PL + 2.4, -1)
    for sx in (-1, 1):
        for k in range(5):
            b.sphere((sx * (w * .38) + rnd(-.3, .3), -d / 2 - 1.0 + rnd(-.3, .3), PL + rnd(.05, .45)), rnd(.25, .45), 'leaf', 5, 6)
            b.sphere((sx * (w * .38) + rnd(-.3, .3), -d / 2 - 1.0 + rnd(-.3, .3), PL + rnd(.2, .6)), rnd(.1, .18), ['flower_pink', 'flower_purple', 'flower_yellow'][k % 3], 5, 6)
    return b.build(bevel=.025)


def farmhouse(name, w=9.0, d=6.0, roofmat='roof_shingle'):
    """Red board-and-batten farmhouse with long porch and shingle roof (reference download (3))."""
    b = Builder(name)
    b.box((0, 0, PL / 2 - .05), (w + .3, d + .3, PL), 'wall_stone')
    H = 3.2
    b.box((0, 0, PL + H / 2), (w, d, H), 'wood_dark')
    # vertical board lines via thin battens (red tint handled by Unity material: wood_dark -> red-brown)
    for i in range(int(w / .5)):
        x = -w / 2 + .25 + i * .5
        b.box((x, -d / 2 - .02, PL + H / 2), (.06, .04, H), 'timber')
        b.box((x, d / 2 + .02, PL + H / 2), (.06, .04, H), 'timber')
    for i in range(int(d / .5)):
        y = -d / 2 + .25 + i * .5
        for sx in (-1, 1):
            b.box((sx * (w / 2 + .02), y, PL + H / 2), (.04, .06, H), 'timber')
    rise = d / 2 * 1.05
    Z = PL + H
    b.box((0, 0, Z + .05), (w + .4, d + .4, .2), 'timber')
    b.gable_roof((0, 0, Z + .1), w, d, rise, roofmat, over=.7, overL=.5, thick=.15, kick=.3)
    for sx in (-1, 1):
        x = sx * w / 2
        b.tri((x, -d / 2, Z + .1), (x, d / 2, Z + .1), (x, 0, Z + .1 + rise), 'wood_dark')
        for i in range(int(d / .5)):
            y = -d / 2 + .25 + i * .5
            h = rise * (1 - abs(y) / (d / 2))
            if h > .2:
                b.box((x + sx * .02, y, Z + .1 + h / 2), (.04, .06, h), 'timber')
        window(b, x, 0, Z + .1 + rise * .35, 'L' if sx < 0 else 'R', w=.7, h=.9, shutter='shutter_yellow', flowers=False)
    for i in range(3):
        window(b, -w / 2 + w * (i + .5) / 3 if i != 1 else -w * .1, -d / 2, PL + 1.55, -1, shutter='shutter_yellow', flowers=(i != 1), w=.95)
    door(b, w * .26, PL, -d / 2, -1)
    for sx, face in ((-1, 'L'), (1, 'R')):
        for i in range(2):
            window(b, sx * w / 2, -d / 4 + i * d / 2, PL + 1.55, face, w=.9, shutter='shutter_yellow', flowers=False)
    for i in range(3):
        window(b, -w / 2 + w * (i + .5) / 3, d / 2, PL + 1.55, 1, shutter='shutter_yellow', flowers=False)
    # porch across the front
    pd = 2.2
    b.box((0, -d / 2 - pd / 2, PL + .08), (w, pd, .16), 'wood_light')
    for i in range(5):
        x = -w / 2 + .12 + i * (w - .24) / 4
        b.box((x, -d / 2 - pd + .12, PL + 1.6), (.2, .2, 3.1), 'wood_dark')
        b.box((x, -d / 2 - pd + .12, PL + .65), (.07, .07, 1.0), 'wood_light') if False else None
    for i in range(int(w / .4)):
        x = -w / 2 + .15 + i * .4
        b.box((x, -d / 2 - pd + .12, PL + .6), (.06, .06, .95), 'wood_light')
    b.box((0, -d / 2 - pd + .12, PL + 1.1), (w, .1, .1), 'wood_light')
    b.quad((-w / 2 - .5, -d / 2 - pd - .5, Z - .3), (w / 2 + .5, -d / 2 - pd - .5, Z - .3), (w / 2 + .5, -d / 2 + .1, Z + .8), (-w / 2 - .5, -d / 2 + .1, Z + .8), roofmat)
    b.quad((-w / 2 - .5, -d / 2 + .1, Z + .7), (w / 2 + .5, -d / 2 + .1, Z + .7), (w / 2 + .5, -d / 2 - pd - .5, Z - .4), (-w / 2 - .5, -d / 2 - pd - .5, Z - .4), 'wood_dark')
    for i in range(3):
        b.box((w * .26, -d / 2 - pd + .3 - i * 0, PL - .15 - i * .15 + .1), (1.6, .5 + 0, .16), 'wall_stone') if False else None
    # stairs at the porch
    for i in range(4):
        b.box((w * .26, -d / 2 - pd - .25 - i * .3, PL - i * .15 + .05), (1.8, .3, .16), 'wood_light')
    # dormer
    chimney(b, -w / 2 + 1.3, 0, Z, Z + rise + .9)
    b.box((w * .2, -(d / 2) * .45, Z + rise * .55 + .25), (1.6, 1.2, 1.2), 'wood_dark')
    window(b, w * .2, -(d / 2) * .45 - .62, Z + rise * .55 + .3, -1, w=.7, h=.85, shutter='shutter_yellow', flowers=False)
    r = .7
    xd, yd, zd = w * .2, -(d / 2) * .45, Z + rise * .55 + .8
    b.quad((xd - 1.0, yd - .6, zd), (xd, yd - .65, zd + r), (xd, yd + .8, zd + r), (xd - 1.0, yd + .8, zd), roofmat)
    b.quad((xd, yd - .65, zd + r), (xd + 1.0, yd - .6, zd), (xd + 1.0, yd + .8, zd), (xd, yd + .8, zd + r), roofmat)
    lantern(b, -w * .3, -d / 2 - pd + .1, PL + 2.4, -1)
    return b.build(bevel=.02)


def tavern(name):
    """Large inn: L-ish with a tower, big hanging sign, awnings."""
    b = Builder(name)
    obj = tudor(name, w=13.0, d=7.5, floors=3, jetty=.6, roofmat='roof_slate', shutter='shutter_red', shop=True, awn=('awning_blue', 'awning_white'), banner='banner_blue')
    return obj


def run():
    B.clear()
    out = []
    specs = [
        ('House_TudorTall', lambda n: tudor(n, 8.5, 6.2, 3, .5, 'roof_terracotta', 'shutter_green', False, banner='banner_blue')),
        ('House_TudorBlue', lambda n: tudor(n, 9.5, 6.6, 2, .45, 'roof_slate', 'shutter_blue', False, plaster='wall_plaster', banner='banner_yellow')),
        ('House_ShopRed', lambda n: tudor(n, 9.0, 6.2, 2, .45, 'roof_terracotta', 'shutter_red', True, awn=('awning_red', 'awning_white'))),
        ('House_ShopGreen', lambda n: tudor(n, 8.0, 6.0, 3, .5, 'roof_teal', 'shutter_green', True, awn=('awning_green', 'awning_white'))),
        ('House_ShopBlue', lambda n: tudor(n, 7.8, 6.0, 2, .4, 'roof_slate', 'shutter_blue', True, awn=('awning_blue', 'awning_yellow'))),
        ('House_Cottage', lambda n: cottage(n)),
        ('House_CottageSmall', lambda n: cottage(n, 6.2, 5.2, 'roof_shingle', False, True, 'shutter_green')),
        ('House_Farm', lambda n: farmhouse(n)),
        ('House_Tavern', lambda n: tavern(n)),
        ('House_RowA', lambda n: tudor(n, 5.4, 6.0, 3, .5, 'roof_terracotta', 'shutter_green', False, dorm=False, chim=True, banner=None)),
        ('House_RowB', lambda n: tudor(n, 4.9, 6.0, 2, .45, 'roof_teal', 'shutter_red', True, awn=('awning_yellow', 'awning_white'), dorm=False)),
        ('House_RowC', lambda n: tudor(n, 5.6, 6.0, 3, .55, 'roof_slate', 'shutter_blue', False, dorm=False, banner='banner_yellow')),
        ('House_RowD', lambda n: tudor(n, 5.0, 6.0, 4, .5, 'roof_terracotta', 'shutter_yellow', True, awn=('awning_green', 'awning_white'), dorm=False)),
    ]
    only = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    for name, fn in specs:
        if only and name not in only:
            continue
        B.clear()
        o = fn(name)
        o.location = (0, 0, 0)
        path = B.export([o], name)
        B.preview("D:/GAME/artifacts/living-world/blender_" + name + ".png", [o], loc=(-13, -17, 7), target=(0, -.5, 3.2))
        print('EXPORTED', path)


if __name__ == '__main__':
    run()
