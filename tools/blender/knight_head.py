# ---------------- head (sculpted anime head + textured face)
FACE_TEX = "D:/GAME/AureliaUnity/Assets/Models/Village/KnightFace.png"
face_img = bpy.data.images.load(FACE_TEX)
fm = bpy.data.materials.new('face')
fm.use_nodes = True
nt = fm.node_tree
bs = nt.nodes['Principled BSDF']
tn = nt.nodes.new('ShaderNodeTexImage')
tn.image = face_img
nt.links.new(tn.outputs['Color'], bs.inputs['Base Color'])
bs.inputs['Roughness'].default_value = .55
fm.diffuse_color = (.94, .76, .6, 1)
MAT['face'] = fm


def gauss(x, c, s):
    return math.exp(-((x - c) / s) ** 2)


def sculpt_head():
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=64, v_segments=40, radius=1.0)
    CX, CY, CZ = 0.0, -.004, 1.755
    for v in bm.verts:
        ux, uy, uz = v.co.x, v.co.y, v.co.z
        X = .088 * ux
        Y = .098 * uy
        Z = .112 * uz
        low = max(0.0, -uz)             # 0..1 towards the chin
        front = max(0.0, -uy)           # 0..1 towards the face
        # V-shaped jaw: narrower and slightly longer chin
        X *= 1 - .50 * low ** 1.35
        Z -= .016 * low ** 2.2
        Y *= 1 - .22 * low ** 1.2
        # cheeks fuller at eye/mouth height
        X *= 1 + .06 * gauss(uz, -.35, .3)
        # flatten the face plane a little, keep forehead round
        if uy < 0:
            Y = -(abs(Y) ** 1.0) * (1 - .1 * front * gauss(uz, -.1, .55))
        # nose bridge and tip
        Y -= .0 + .0125 * gauss(ux, 0, .13) * gauss(uz, -.30, .13) * front
        Y -= .004 * gauss(ux, 0, .08) * gauss(uz, -.08, .2) * front
        # eye sockets
        for sx in (1, -1):
            Y += .0035 * gauss(ux, sx * .42, .17) * gauss(uz, .0, .16) * front
        # mouth recess and chin cleft
        Y += .003 * gauss(ux, 0, .22) * gauss(uz, -.62, .07) * front
        # brow ridge
        Y -= .003 * gauss(uz, .28, .1) * front * (1 - abs(ux) * .5)
        v.co = Vector((CX + X, CY + Y, CZ + Z))
    # planar front projection UVs
    uv = bm.loops.layers.uv.new('UVMap')
    for f in bm.faces:
        for l in f.loops:
            p = l.vert.co
            l[uv].uv = ((p.x - (-.105)) / .21, (p.z - 1.62) / .26)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    o = mk_obj(bm, 'head')
    return seal(o, 'face', 'Head', 1)


sculpt_head()
tube('skin', 'Neck', [(0, 0, 1.53), (0, .002, 1.64)], [.05, .046], [.052, .05], 16, 1, False, False, False, 'neck')
for sx in (1, -1):
    ell('skin', 'Head', (sx * .089, .006, 1.745), (.011, .024, .036), (0, 0, sx * -12))
    ell('skin', 'Head', (sx * .092, .005, 1.74), (.006, .015, .024), (0, 0, sx * -12))

# hair: swept ribbon locks with two tones, a cap and flowing back hair
mat('hair_hi', '#a8683a', 0, .34)


def ribbon(bone, pts, w, th, matname='hair', point=True):
    n = len(pts)
    rx = [w * (1 - .85 * (i / (n - 1)) ** 1.5) for i in range(n)]
    ry = [th * (1 - .6 * (i / (n - 1)) ** 1.2) for i in range(n)]
    return tube(matname, bone, pts, rx, ry, 8, 1, True, False, point, 'ribbon')


shell('hair', 'Head', (0, .016, 1.795), (.101, .111, .104), (20, 0, 0), -.28, .012, 28, 14, 1, 'haircap')
shell('hair', 'Head', (0, .048, 1.74), (.097, .1, .125), (0, 0, 0), .0, .012, 24, 12, 1, 'hairback', 'lower')
# parted fringe: strands start at the crown, sweep across the forehead and curve to the sides
FR = [(-.8, .062, .015), (-.55, .056, .02), (-.3, .05, .026), (-.08, .046, .03), (.12, .05, .03), (.34, .056, .024), (.6, .06, .018), (.85, .066, .012)]
for i, (t, L, drift) in enumerate(FR):
    x1 = t * .095
    z_hair = 1.808 - abs(t) * .01
    side = 1 if t >= 0 else -1
    ribbon('Head', [(t * .03, -.045, 1.885), (t * .06, -.098, 1.855), (x1, -.111, z_hair + .004), (x1 + side * drift * .5, -.113, z_hair - L * .55), (x1 + side * drift, -.111, z_hair - L)],
           .03, .0085, 'hair' if i % 2 == 0 else 'hair_hi')
# long strands framing the face (past the cheek, not over the eyes)
for sx in (1, -1):
    ribbon('Head', [(sx * .085, -.06, 1.81), (sx * .102, -.09, 1.76), (sx * .108, -.098, 1.70), (sx * .1, -.1, 1.63), (sx * .09, -.095, 1.575)], .022, .008, 'hair_hi')
    ribbon('Head', [(sx * .09, -.03, 1.8), (sx * .106, -.04, 1.74), (sx * .108, -.04, 1.66), (sx * .098, -.045, 1.59), (sx * .09, -.04, 1.55)], .026, .009)
    ribbon('Head', [(sx * .075, .04, 1.78), (sx * .1, .06, 1.72), (sx * .098, .08, 1.65), (sx * .085, .09, 1.6)], .028, .01)
# back hair mass flowing into the ponytail
for k in range(5):
    xo = (k - 2) * .034
    ribbon('Head', [(xo, .09, 1.85 - abs(k - 2) * .012), (xo * 1.05, .13, 1.8), (xo * 1.1, .142, 1.73), (xo * 1.15, .135, 1.67)], .034, .012, 'hair' if k % 2 == 0 else 'hair_hi')
# ponytail: three bone-bound segments with several ribbons
for si, bn in enumerate(('Pony.1', 'Pony.2', 'Pony.3')):
    a, b2 = BONES[bn]
    for st in range(5):
        off = (st - 2) * .016
        mid = (Vector(a) + Vector(b2)) * .5
        w = .026 - si * .0035
        tube('hair' if st % 2 == 0 else 'hair_hi', bn, [Vector(a) + Vector((off, 0, 0)), mid + Vector((off * 1.4, .01, 0)), Vector(b2) + Vector((off * (1.3 if si < 2 else .4), .008, 0))],
             [w, w * 1.15, w * (.95 if si < 2 else .3)], [w * .7, w * .8, w * (.7 if si < 2 else .25)], 8, 1, True, False, si == 2, 'pony')
cyl('trim_gold', 'Head', (0, .128, 1.78), .034, .024, (80, 0, 0), 14)
