import sys, math
sys.path.insert(0, "D:/GAME/tools/blender")
import blendlib as B
from blendlib import Builder, rnd
import bpy


def ogive(hw, spring, apex, n=14):
    """pointed arch outline from left springing point to right (list of (x,z))"""
    pts = [(-hw, 0.0), (-hw, spring)]
    R = (hw * hw + (apex - spring) ** 2) / (2 * hw) if False else hw * 1.9
    cxL = -hw + R
    cxR = hw - R
    # left arc centered at (cxL, spring) going to apex at x=0
    ang0 = math.pi
    for i in range(1, n):
        t = i / n
        a = math.pi - t * math.acos(max(-1, min(1, (cxL - 0) / R)))
        x = cxL + math.cos(a) * R
        z = spring + math.sin(a) * R * ((apex - spring) / (math.sin(math.acos(max(-1, min(1, (cxL - 0) / R)))) * R))
        pts.append((x, z))
    pts.append((0.0, apex))
    for i in range(n - 1, 0, -1):
        px, pz = pts[2 + i - 1]
        pts.append((-px, pz))
    pts.append((hw, spring))
    pts.append((hw, 0.0))
    return pts


def build():
    b = Builder('Gate_Portal')
    W, H, D = 17.0, 20.0, 4.0
    hw, spring, apex = 7.0, 10.0, 18.0
    arch = ogive(hw, spring, apex)
    # stone infill between the rectangle and the arch (front and back faces, plus a thick lintel body)
    left = [p for p in arch if p[0] <= 0]
    right = [p for p in arch if p[0] >= 0]
    for sy, y in ((-1, -D / 2), (1, D / 2)):
        # left infill column: from outer edge x=-W/2 up to arch
        prev = None
        for i in range(len(arch) - 1):
            (x0, z0), (x1, z1) = arch[i], arch[i + 1]
            # quad between arch segment and a vertical strip up to H
            if x0 <= 0 and x1 <= 0:
                q = [(x0, y, z0), (x1, y, z1), (x1, y, H), (x0, y, H)]
            elif x0 >= 0 and x1 >= 0:
                q = [(x1, y, z1), (x0, y, z0), (x0, y, H), (x1, y, H)]
            else:
                continue
            if sy == -1:
                b.quad(*q, 'wall_stone')
            else:
                b.quad(*q[::-1], 'wall_stone')
        # side piers
        for sx in (-1, 1):
            if sy == -1:
                b.quad((sx * hw, y, 0), (sx * W / 2, y, 0), (sx * W / 2, y, H), (sx * hw, y, H), 'wall_stone') if sx == 1 else b.quad((-W / 2, y, 0), (-hw, y, 0), (-hw, y, H), (-W / 2, y, H), 'wall_stone')
            else:
                b.quad((sx * W / 2, y, 0), (sx * hw, y, 0), (sx * hw, y, H), (sx * W / 2, y, H), 'wall_stone') if sx == 1 else b.quad((-hw, y, 0), (-W / 2, y, 0), (-W / 2, y, H), (-hw, y, H), 'wall_stone')
    # tunnel walls (inside of piers) and soffit
    for i in range(len(arch) - 1):
        (x0, z0), (x1, z1) = arch[i], arch[i + 1]
        b.quad((x0, -D / 2, z0), (x0, D / 2, z0), (x1, D / 2, z1), (x1, -D / 2, z1), 'wall_stone') if x1 >= x0 else b.quad((x1, -D / 2, z1), (x1, D / 2, z1), (x0, D / 2, z0), (x0, -D / 2, z0), 'wall_stone')
    # outer sides, top
    b.quad((-W / 2, -D / 2, 0), (-W / 2, D / 2, 0), (-W / 2, D / 2, H), (-W / 2, -D / 2, H), 'wall_stone')
    b.quad((W / 2, D / 2, 0), (W / 2, -D / 2, 0), (W / 2, -D / 2, H), (W / 2, D / 2, H), 'wall_stone')
    b.quad((-W / 2, -D / 2, H), (-W / 2, D / 2, H), (W / 2, D / 2, H), (W / 2, -D / 2, H), 'wall_stone')
    # voussoir ring (stone_light) on the front face
    for i in range(len(arch) - 1):
        (x0, z0), (x1, z1) = arch[i], arch[i + 1]
        if z0 < spring - .01 and z1 < spring - .01:
            continue
        nx = -(z1 - z0)
        nz = (x1 - x0)
        L = math.hypot(nx, nz) or 1
        nx, nz = nx / L, nz / L
        t = .9
        for sy, y in ((-1, -D / 2 - .25),):
            p0 = (x0, y, z0)
            p1 = (x1, y, z1)
            p2 = (x1 + nx * t * (1 if x1 >= 0 else 1), y, z1 + nz * t)
            p3 = (x0 + nx * t, y, z0 + nz * t)
            b.quad(p0, p1, p2, p3, 'stone_light')
            b.quad(p3, p2, p1, p0, 'stone_light')
    # corbelled string courses and crenellation on top
    b.box((0, -D / 2 - .15, H + .25), (W + .6, .5, .5), 'stone_light')
    for k in range(9):
        b.box((-W / 2 + .8 + k * (W - 1.6) / 8, -D / 2 + .3, H + 1.2), (1.2, 1.0, 1.4), 'stone_light')
    for sx in (-1, 1):
        for k in range(3):
            b.box((sx * (hw + 1.2 + k * 0), -D / 2 - .5, 3.5 + k * 5.5), (.5, .5, 3.0), 'stone_light') if False else None
        b.box((sx * (hw + .6), -D / 2 - .05, 6), (.9, .1, 3.0), 'banner_blue')
        b.box((sx * (hw + .6), -D / 2 - .08, 6.6), (.5, .06, .6), 'trim_gold')
    # open double doors swung against the tunnel walls
    for sx in (-1, 1):
        b.box((sx * (hw - .5), -D / 2 + 1.3, 5.2), (.35, 2.4, 10.4), 'door_wood')
        for z in (2.0, 5.2, 8.4):
            b.box((sx * (hw - .5), -D / 2 + 1.3, z), (.45, 2.5, .35), 'iron')
        b.box((sx * (hw - .3), -D / 2 + 1.3, 5.2), (.1, .14, 10.2), 'iron')
        for k in range(6):
            b.sphere((sx * (hw - .28), -D / 2 + .4 + k * .45, 9.0 - k * 1.1), .08, 'iron', 4, 6)
    # raised portcullis in the upper arch
    for k in range(-5, 6):
        x = k * (2 * hw - 1.5) / 10
        zt = apex - 3.5
        zb = spring + 1.2 + abs(k) * .35
        b.box((x, -.8, (zt + zb) / 2 + 3.0), (.12, .12, zt - zb + 1.0), 'iron') if False else None
    for k in range(-4, 5):
        b.box((k * 1.55, -.2, 15.3), (.14, .14, 3.4), 'iron')
    for z in (14.2, 15.5, 16.8):
        b.box((0, -.2, z), (11.0 - (z - 14.2) * 1.6, .14, .14), 'iron')
    # torches
    for sx in (-1, 1):
        b.box((sx * (hw - .15), 0, 6.0), (.15, .15, .9), 'wood_dark')
        b.box((sx * (hw - .15), 0, 6.6), (.4, .4, .5), 'lantern_glow')
    return b.build(bevel=.03)


B.clear()
o = build()
B.export([o], 'Gate_Portal')
B.preview("D:/GAME/artifacts/living-world/blender_Gate_Portal.png", [o], loc=(-14, -34, 12), target=(0, 0, 9), size=(640, 560))
print('EXPORTED Gate_Portal')
