import sys, math
sys.path.insert(0, "D:/GAME/tools/blender")
import blendlib as B
from blendlib import Builder, rnd
import bpy
from mathutils import Vector

# Silver-and-teal knight with brown ponytail hair (reference knight.jpg). Faces -Y, Z up, ~1.85 m tall.
objs = []


def empty(name, loc, parent=None):
    e = bpy.data.objects.new(name, None)
    e.empty_display_type = 'PLAIN_AXES'
    e.empty_display_size = .1
    bpy.context.scene.collection.objects.link(e)
    e.location = loc if parent is None else (Vector(loc) - parent.matrix_world.translation)
    if parent is not None:
        e.parent = parent
    bpy.context.view_layer.update()
    return e


def part(name, pivot_world, parent, fn, bevel=.012):
    b = Builder(name)
    fn(b)
    o = b.build(bevel=bevel)
    o.location = pivot_world if parent is None else (Vector(pivot_world) - parent.matrix_world.translation)
    if parent is not None:
        o.parent = parent
    bpy.context.view_layer.update()
    objs.append(o)
    return o


S = 'armor_silver'
D = 'armor_dark'
T = 'cape_teal'
G = 'trim_gold'


def limb_segment(b, length, r_top, r_bot, mat, sides=10):
    b.cyl((0, 0, -length), r_bot, length, mat, sides, r2=r_top, cap=True)


def build():
    B.clear()
    global objs
    objs = []
    root = empty('KnightRoot', (0, 0, 0))
    hips = empty('Hips', (0, 0, .98), root)
    spine = empty('Spine', (0, 0, 1.12), hips)
    chest = empty('Chest', (0, 0, 1.3), spine)
    neck = empty('Neck', (0, 0, 1.55), chest)
    head = empty('Head', (0, 0, 1.62), neck)
    shL = empty('ShoulderL', (-.27, 0, 1.46), chest)
    elL = empty('ElbowL', (-.37, 0, 1.2), shL)
    shR = empty('ShoulderR', (.27, 0, 1.46), chest)
    elR = empty('ElbowR', (.37, 0, 1.2), shR)
    hipL = empty('HipL', (-.12, 0, .96), hips)
    knL = empty('KneeL', (-.12, 0, .52), hipL)
    hipR = empty('HipR', (.12, 0, .96), hips)
    knR = empty('KneeR', (.12, 0, .52), hipR)
    capeP = empty('CapeTop', (0, .12, 1.48), chest)

    # ---- pelvis / skirt
    def f(b):
        b.box((0, 0, .02), (.36, .22, .2), D)
        b.box((0, -.0, -.1), (.4, .26, .1), S)
        # tabard front/back panels (teal) hanging to knees, white inner robe
        b.quad((-.17, -.125, .08), (.17, -.125, .08), (.15, -.15, -.5), (-.15, -.15, -.5), T)
        b.quad((.17, .125, .08), (-.17, .125, .08), (-.15, .15, -.5), (.15, .15, -.5), T)
        b.box((0, -.1, -.28), (.26, .03, .5), 'robe_white')
        b.box((0, -.13, -.05), (.13, .04, .1), G)
        for sx in (-1, 1):
            b.box((sx * .22, 0, -.18), (.08, .24, .3), S, rz=sx * 6)
    part('Pelvis', (0, 0, .98), hips, f)

    # ---- belt
    def f(b):
        b.cyl((0, 0, 0), .22, .09, D, 16, r2=.22)
        b.box((0, -.215, .045), (.1, .03, .1), G)
        b.box((0, -.245, .045), (.05, .02, .06), 'iron')
        b.box((.2, -.05, -.18), (.04, .06, .6), 'leather', ry=-4)       # diagonal strap
    part('Belt', (0, 0, 1.1), spine, f)

    # ---- torso: cuirass, abdominal plates
    def f(b):
        # waist taper
        b.box((0, 0, -.1), (.36, .22, .2), D, taper=1.08)
        # breastplate: upper chest wider, with teal underlay showing in a V
        b.box((0, -.02, .12), (.46, .26, .3), S, taper=.96)
        b.box((0, -.145, .12), (.3, .03, .25), T)       # teal scale panel
        for r in range(4):
            for c in range(4):
                b.sphere((-.12 + c * .08 + (r % 2) * .04, -.165, .03 + r * .06), .028, 'cape_teal', 4, 5, sq=.5)
        b.box((0, -.17, .22), (.06, .03, .17), G)        # winged crest
        b.box((0, -.17, .27), (.16, .02, .03), G)
        b.box((0, 0, -.02), (.4, .24, .08), S, taper=1.1)
        # back plate
        b.box((0, .12, .1), (.42, .06, .32), S)
        # collar
        b.cyl((0, 0, .27), .13, .1, D, 12, r2=.11)
    part('Torso', (0, 0, 1.2), chest, f)

    # ---- head: skin, eyes, hair, ponytail
    def f(b):
        b.sphere((0, 0, .1), .108, 'skin', 10, 14, sq=1.12)
        # jaw
        b.box((0, -.02, .02), (.15, .15, .1), 'skin', taper=.8)
        # eyes (green) and brows, smile
        for sx in (-1, 1):
            b.box((sx * .04, -.098, .115), (.034, .01, .026), 'eye_green')
            b.box((sx * .04, -.1, .13), (.05, .01, .01), 'hair')
        b.box((0, -.1, .035), (.04, .008, .008), 'iron')
        b.box((0, -.108, .08), (.015, .02, .03), 'skin')
        # hair cap and fringe
        b.sphere((0, .02, .15), .12, 'hair', 8, 12, sq=.85)
        for i in range(7):
            a = -.9 + i * .3
            b.box((math.sin(a) * .09, -.09 + abs(a) * .03, .17 - abs(a) * .03), (.04, .035, .09), 'hair', rz=math.degrees(a) * .6, rx=-18)
        b.box((.0, -.1, .185), (.1, .04, .04), 'hair', rx=-20)
        # side locks
        for sx in (-1, 1):
            b.box((sx * .105, -.02, .06), (.03, .06, .15), 'hair')
        # ponytail
        b.box((0, .13, .09), (.06, .06, .08), 'hair', rx=-20)
        b.box((0, .17, -.0), (.075, .075, .24), 'hair', rx=-15)
        b.box((0, .2, -.17), (.055, .055, .18), 'hair', rx=-10)
        b.cyl((0, .135, .09), .045, .035, G, 8, axis='y')
    part('Head', (0, 0, 1.62), neck, f, bevel=.006)

    # ---- neck
    def f(b):
        b.cyl((0, 0, 0), .05, .09, 'skin', 8, r2=.05)
    part('NeckMesh', (0, 0, 1.53), neck, f, bevel=0)

    # ---- arms: pauldron layers (silver), upper arm (dark sleeve + armor), forearm (vambrace) and glove
    for sx, sh, el, nm in ((-1, shL, elL, 'L'), (1, shR, elR, 'R')):
        def f(b, sx=sx):
            # layered pauldron
            for k in range(3):
                b.box((sx * .02, 0, .05 - k * .045), (.26 - k * .015, .26, .06), S, rx=0, ry=-sx * (16 - k * 3), taper=1.0)
                b.box((sx * (.1 + k * .01), 0, .035 - k * .045), (.04, .27, .07), S, ry=-sx * 30)
            b.sphere((sx * .02, 0, .07), .1, S, 8, 10, sq=.6)
            b.box((sx * .02, -.12, .05), (.22, .03, .03), G) if False else None
            b.box((sx * .0, 0, .115), (.1, .1, .02), G)
            # teal edge of pauldron
            b.box((sx * .14, 0, -.06), (.05, .22, .03), T)
            # upper arm sleeve
            b.cyl((sx * .1, 0, -.26), .055, .27, D, 10, r2=.065)
            b.cyl((sx * .1, 0, -.12), .075, .11, S, 10, r2=.075)
        part('UpperArm' + nm, sh.matrix_world.translation, sh, f)

        def f(b, sx=sx):
            # elbow cop
            b.sphere((0, 0, 0), .065, S, 6, 8)
            # forearm vambrace
            b.cyl((0, 0, -.3), .05, .3, D, 10, r2=.06)
            b.cyl((0, 0, -.28), .062, .2, S, 10, r2=.07)
            b.box((0, -.05, -.18), (.05, .02, .1), G)
            b.box((0, 0, -.12), (.14, .1, .025), 'leather')
            # glove
            b.sphere((0, 0, -.35), .055, 'armor_dark', 7, 8, sq=1.0)
            b.box((0, -.01, -.4), (.07, .05, .08), 'armor_dark')
        part('LowerArm' + nm, el.matrix_world.translation, el, f)

    # ---- legs
    for sx, hp, kn, nm in ((-1, hipL, knL, 'L'), (1, hipR, knR, 'R')):
        def f(b, sx=sx):
            b.cyl((0, 0, -.44), .075, .44, D, 10, r2=.095)
            b.cyl((0, 0, -.4), .088, .26, S, 10, r2=.098)
            b.box((0, -.09, -.2), (.1, .02, .22), T)
        part('UpperLeg' + nm, hp.matrix_world.translation, hp, f)

        def f(b, sx=sx):
            b.sphere((0, 0, 0), .075, S, 6, 8)
            b.cyl((0, 0, -.44), .06, .44, D, 10, r2=.075)
            b.cyl((0, 0, -.4), .072, .32, S, 10, r2=.082)     # greave
            b.box((0, -.07, -.18), (.045, .02, .2), G)
            # boot
            b.box((0, -.04, -.5), (.1, .24, .1), 'leather')
            b.cyl((0, 0, -.5), .08, .1, 'leather', 10, r2=.07)
        part('LowerLeg' + nm, kn.matrix_world.translation, kn, f)

    # ---- cape (teal) with high collar and trailing panels
    def f(b):
        b.box((0, 0, 0), (.58, .05, .05), T)
        # shoulders cloak part
        for sx in (-1, 1):
            b.quad((sx * .3, .0, .02), (sx * .22, .13, .0), (sx * .26, .2, -.9), (sx * .42, .06, -.8), T)
            b.quad((sx * .42, .06, -.8), (sx * .26, .2, -.9), (sx * .22, .13, .0), (sx * .3, .0, .02), T)
        b.quad((-.22, .13, 0), (.22, .13, 0), (.26, .22, -1.05), (-.26, .22, -1.05), T)
        b.quad((.22, .13, 0), (-.22, .13, 0), (-.26, .22, -1.05), (.26, .22, -1.05), T)
        b.quad((-.26, .22, -1.05), (.26, .22, -1.05), (.3, .24, -1.2), (-.3, .24, -1.2), 'cape_teal')
        b.quad((.26, .22, -1.05), (-.26, .22, -1.05), (-.3, .24, -1.2), (.3, .24, -1.2), 'cape_teal')
        b.box((0, .13, -.02), (.5, .03, .05), G)
    part('Cape', capeP.matrix_world.translation, chest, f, bevel=0)

    # ---- sword sheathed on the hip
    def f(b):
        b.box((0, 0, -.35), (.045, .02, .75), 'steel_dark', rx=0)
        b.box((0, 0, .05), (.2, .035, .035), G)
        b.cyl((0, 0, .06), .018, .17, 'leather', 8)
        b.sphere((0, 0, .26), .03, G, 5, 6)
    part('Sword', (.26, -.02, 1.04), hips, f)
    return root


def run():
    root = build()
    B.export([root] + objs, 'Knight')
    # also the dark lord statue variant could be added later
    # previews: front and 3/4
    B.preview("D:/GAME/artifacts/living-world/blender_Knight_front.png", objs, loc=(0, -3.2, 1.2), target=(0, 0, 1.0), size=(700, 900), lens=45)
    B.preview("D:/GAME/artifacts/living-world/blender_Knight_34.png", objs, loc=(-2.6, -2.4, 1.6), target=(0, 0, 1.0), size=(700, 900), lens=45)
    print('EXPORTED Knight', len(objs))


run()
