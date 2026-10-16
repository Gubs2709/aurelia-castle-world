import bpy, bmesh, math, sys, os, random
from mathutils import Vector, Matrix, Euler

OUTFBX = "D:/GAME/AureliaUnity/Assets/Models/Village/Knight2.fbx"
BLEND = "D:/GAME/tools/blender/Knight2.blend"
PREV = "D:/GAME/artifacts/living-world/"
random.seed(4)

# ------------------------------------------------------------------ scene / materials
for o in list(bpy.data.objects):
    bpy.data.objects.remove(o, do_unlink=True)
for m in list(bpy.data.meshes):
    bpy.data.meshes.remove(m)
for m in list(bpy.data.materials):
    bpy.data.materials.remove(m)
for a in list(bpy.data.actions):
    bpy.data.actions.remove(a)
for a in list(bpy.data.armatures):
    bpy.data.armatures.remove(a)

MAT = {}


def mat(name, color, metal=0.0, rough=0.5, emit=None, es=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    b = m.node_tree.nodes.get('Principled BSDF')
    h = color.lstrip('#')
    c = tuple(int(h[i:i + 2], 16) / 255 for i in (0, 2, 4)) + (1,)
    b.inputs['Base Color'].default_value = c
    b.inputs['Metallic'].default_value = metal
    b.inputs['Roughness'].default_value = rough
    if emit:
        eh = emit.lstrip('#')
        ec = tuple(int(eh[i:i + 2], 16) / 255 for i in (0, 2, 4)) + (1,)
        b.inputs['Emission Color'].default_value = ec
        b.inputs['Emission Strength'].default_value = es
    m.diffuse_color = c
    MAT[name] = m
    return m


mat('armor_silver', '#a9b6c6', .55, .42)
mat('armor_blue', '#3f719c', .55, .4)
mat('armor_dark', '#34415a', 0, .8)
mat('trim_gold', '#e0b445', .95, .25)
mat('cape_teal', '#1c9c9a', 0, .75)
mat('cape_inner', '#e9f1f0', 0, .8)
mat('robe_white', '#eef2f8', 0, .8)
mat('skin', '#f0c19a', 0, .6)
mat('hair', '#7a4a28', 0, .38)
mat('eye_white', '#f8f8f8', 0, .2)
mat('eye_green', '#79d84a', 0, .15, '#5ac032', .35)
mat('eye_dark', '#16301a', 0, .2)
mat('lips', '#c47878', 0, .5)
mat('leather', '#6a4630', 0, .55)
mat('steel_dark', '#6a707c', .8, .35)
mat('gem_teal', '#37d6d0', 0, .1, '#37d6d0', 1.2)
mat('blade_glow', '#bfe6ff', .3, .15, '#8ad0ff', 2.2)

bpy.context.scene.unit_settings.system = 'METRIC'
P = []   # all created part objects


def activate(o):
    bpy.ops.object.select_all(action='DESELECT')
    o.select_set(True)
    bpy.context.view_layer.objects.active = o


def finish(o, matname, bone, subsurf=0, shade=True, loc=(0, 0, 0), scale=(1, 1, 1), rot=(0, 0, 0), bevel=0.0):
    o.location = loc
    o.scale = scale
    o.rotation_euler = [math.radians(a) for a in rot]
    activate(o)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if bevel > 0:
        md = o.modifiers.new('b', 'BEVEL')
        md.width = bevel
        md.segments = 2
        md.limit_method = 'ANGLE'
        md.angle_limit = math.radians(35)
        bpy.ops.object.modifier_apply(modifier='b')
    if subsurf:
        md = o.modifiers.new('s', 'SUBSURF')
        md.levels = subsurf
        md.render_levels = subsurf
        bpy.ops.object.modifier_apply(modifier='s')
    o.data.materials.clear()
    o.data.materials.append(MAT[matname])
    if shade:
        for p in o.data.polygons:
            p.use_smooth = True
    vg = o.vertex_groups.new(name=bone)
    vg.add(list(range(len(o.data.vertices))), 1.0, 'REPLACE')
    P.append(o)
    return o


def sph(matname, bone, loc, scale, rot=(0, 0, 0), seg=20, rings=12, sub=0):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=1)
    return finish(bpy.context.active_object, matname, bone, sub, True, loc, scale, rot)


def cyl(matname, bone, loc, r, h, rot=(0, 0, 0), seg=20, scale=(1, 1, 1), bevel=0.0, sub=0, r2=None):
    if r2 is None:
        bpy.ops.mesh.primitive_cylinder_add(vertices=seg, radius=r, depth=h)
    else:
        bpy.ops.mesh.primitive_cone_add(vertices=seg, radius1=r, radius2=r2, depth=h)
    return finish(bpy.context.active_object, matname, bone, sub, True, loc, scale, rot, bevel)


def box(matname, bone, loc, size, rot=(0, 0, 0), bevel=0.01, sub=0):
    bpy.ops.mesh.primitive_cube_add(size=1)
    return finish(bpy.context.active_object, matname, bone, sub, False, loc, size, rot, bevel)


def tor(matname, bone, loc, R, r, rot=(0, 0, 0), scale=(1, 1, 1), seg=24, rseg=8):
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, major_segments=seg, minor_segments=rseg)
    return finish(bpy.context.active_object, matname, bone_name_or(bone), 0, True, loc, scale, rot)


def bone_name_or(b):
    return b


def mirror_x(fn, *a, **k):
    """call fn for +x then -x; fn receives side sign through kw 'sx'"""
    for sx in (1, -1):
        fn(*a, sx=sx, **k)


# ------------------------------------------------------------------ armature
arm_data = bpy.data.armatures.new('KnightRig')
arm = bpy.data.objects.new('KnightRig', arm_data)
bpy.context.scene.collection.objects.link(arm)
activate(arm)
bpy.ops.object.mode_set(mode='EDIT')
EB = arm_data.edit_bones
BONES = {}


def bone(name, head, tail, parent=None, roll=0.0, connect=False):
    b = EB.new(name)
    b.head = head
    b.tail = tail
    b.roll = roll
    if parent:
        b.parent = EB[parent]
        b.use_connect = connect
    BONES[name] = (head, tail)
    return b


bone('Root', (0, 0, 0), (0, 0, .2))
bone('Hips', (0, 0, .98), (0, 0, 1.08), 'Root')
bone('Spine', (0, 0, 1.08), (0, 0, 1.3), 'Hips', connect=True)
bone('Chest', (0, 0, 1.3), (0, 0, 1.52), 'Spine', connect=True)
bone('Neck', (0, 0, 1.52), (0, 0, 1.63), 'Chest', connect=True)
bone('Head', (0, 0, 1.63), (0, 0, 1.88), 'Neck', connect=True)
for s, sx in (('L', 1), ('R', -1)):
    bone('Clavicle.' + s, (sx * .04, 0, 1.48), (sx * .17, 0, 1.49), 'Chest')
    bone('UpperArm.' + s, (sx * .19, 0, 1.47), (sx * .30, 0, 1.2), 'Clavicle.' + s)
    bone('Forearm.' + s, (sx * .30, 0, 1.2), (sx * .37, -.01, .96), 'UpperArm.' + s, connect=True)
    bone('Hand.' + s, (sx * .37, -.01, .96), (sx * .40, -.015, .85), 'Forearm.' + s, connect=True)
    bone('Thigh.' + s, (sx * .10, 0, .97), (sx * .11, -.005, .54), 'Hips')
    bone('Shin.' + s, (sx * .11, -.005, .54), (sx * .11, .01, .10), 'Thigh.' + s, connect=True)
    bone('Foot.' + s, (sx * .11, .01, .10), (sx * .11, -.16, .045), 'Shin.' + s, connect=True)
    bone('Toe.' + s, (sx * .11, -.16, .045), (sx * .11, -.25, .03), 'Foot.' + s, connect=True)
for i, (a, b2) in enumerate((((0, .10, 1.78), (0, .19, 1.70)), ((0, .19, 1.70), (0, .25, 1.54)), ((0, .25, 1.54), (0, .27, 1.38)))):
    bone('Pony.%d' % (i + 1), a, b2, 'Head' if i == 0 else 'Pony.%d' % i, connect=(i > 0))
for col, x in (('C', 0), ('L', .2), ('R', -.2)):
    prev = 'Chest'
    for i, (y, z) in enumerate(((.14, 1.50), (.21, 1.12), (.25, .74), (.27, .38))):
        if i == 3:
            break
        ny, nz = ((.14, 1.50), (.21, 1.12), (.25, .74), (.27, .38))[i + 1]
        n = 'Cape%s.%d' % (col, i + 1)
        bone(n, (x, y, z), (x, ny, nz), prev, connect=(i > 0))
        prev = n
for i, (nm, y0) in enumerate((('TabF', -.125), ('TabB', .125))):
    bone(nm + '.1', (0, y0, 1.02), (0, y0 * 1.15, .78), 'Hips')
    bone(nm + '.2', (0, y0 * 1.15, .78), (0, y0 * 1.3, .52), nm + '.1', connect=True)
bone('SwordSheath', (.19, .02, 1.02), (.31, .1, .42), 'Hips')
bpy.ops.object.mode_set(mode='OBJECT')

# ------------------------------------------------------------------ undersuit (Skin modifier body)
verts = [
    (0, 0, 1.0), (0, 0, 1.16), (0, 0, 1.33), (0, 0, 1.47), (0, 0, 1.57),
    (.12, 0, 1.47), (.20, 0, 1.46), (.30, 0, 1.21), (.37, -.01, .97), (.40, -.015, .90),
    (-.12, 0, 1.47), (-.20, 0, 1.46), (-.30, 0, 1.21), (-.37, -.01, .97), (-.40, -.015, .90),
    (.10, 0, .96), (.11, -.005, .54), (.11, .01, .10), (.11, -.12, .05),
    (-.10, 0, .96), (-.11, -.005, .54), (-.11, .01, .10), (-.11, -.12, .05)]
edges = [(0, 1), (1, 2), (2, 3), (3, 4), (3, 5), (5, 6), (6, 7), (7, 8), (8, 9), (3, 10), (10, 11), (11, 12), (12, 13), (13, 14),
         (0, 15), (15, 16), (16, 17), (17, 18), (0, 19), (19, 20), (20, 21), (21, 22)]
radii = [(.17, .11), (.145, .1), (.17, .11), (.16, .09), (.048, .05),
         (.05, .05), (.055, .055), (.05, .05), (.04, .04), (.036, .028),
         (.05, .05), (.055, .055), (.05, .05), (.04, .04), (.036, .028),
         (.092, .09), (.068, .068), (.052, .052), (.05, .042),
         (.092, .09), (.068, .068), (.052, .052), (.05, .042)]
me = bpy.data.meshes.new('Body')
me.from_pydata(verts, edges, [])
body = bpy.data.objects.new('Body', me)
bpy.context.scene.collection.objects.link(body)
activate(body)
md = body.modifiers.new('skin', 'SKIN')
md.use_smooth_shade = True
for i, r in enumerate(radii):
    body.data.skin_vertices[0].data[i].radius = r
body.data.skin_vertices[0].data[0].use_root = True
md2 = body.modifiers.new('sub', 'SUBSURF')
md2.levels = 2
md2.render_levels = 2
bpy.ops.object.modifier_apply(modifier='skin')
bpy.ops.object.modifier_apply(modifier='sub')
body.data.materials.append(MAT['armor_dark'])
for p in body.data.polygons:
    p.use_smooth = True
# parent with automatic weights
bpy.ops.object.select_all(action='DESELECT')
body.select_set(True)
arm.select_set(True)
bpy.context.view_layer.objects.active = arm
try:
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
except Exception as e:
    print('auto weights failed', e)
    bpy.ops.object.parent_set(type='ARMATURE_ENVELOPE')
print('BODY groups', len(body.vertex_groups))

# ------------------------------------------------------------------ helper: local side-aware builders
def S(sx):
    return 'L' if sx > 0 else 'R'


def cloth_strip(name, bone1, bone2, y, x0, x1, z0, z1, matname, rows=6, cols=4, bulge=.0, flare=.06):
    bm = bmesh.new()
    grid = []
    for r in range(rows + 1):
        t = r / rows
        z = z0 + (z1 - z0) * t
        row = []
        for c in range(cols + 1):
            u = c / cols
            x = x0 + (x1 - x0) * u
            x *= (1 + flare * t)
            yy = y + math.copysign(1, y) * (flare * t * .8 + bulge * math.sin(math.pi * u) * .5)
            row.append(bm.verts.new((x, yy, z)))
        grid.append(row)
    for r in range(rows):
        for c in range(cols):
            f = bm.faces.new((grid[r][c], grid[r][c + 1], grid[r + 1][c + 1], grid[r + 1][c]))
            f.smooth = True
    me2 = bpy.data.meshes.new(name)
    bm.to_mesh(me2)
    bm.free()
    o = bpy.data.objects.new(name, me2)
    bpy.context.scene.collection.objects.link(o)
    activate(o)
    md = o.modifiers.new('sol', 'SOLIDIFY')
    md.thickness = .012
    md.offset = 0
    bpy.ops.object.modifier_apply(modifier='sol')
    o.data.materials.append(MAT[matname])
    for p in o.data.polygons:
        p.use_smooth = True
    # weights: top rows to bone1, lower rows blend to bone2
    g1 = o.vertex_groups.new(name=bone1)
    g2 = o.vertex_groups.new(name=bone2)
    for v in o.data.vertices:
        t = (z0 - v.co.z) / (z0 - z1) if z0 != z1 else 0
        t = max(0, min(1, t))
        g1.add([v.index], 1 - t, 'REPLACE')
        g2.add([v.index], t, 'REPLACE')
    P.append(o)
    return o



# ---------------------------------------------------------------- v3 parts (loft based)
def mk_obj(bm, name):
    me2 = bpy.data.meshes.new(name)
    bm.to_mesh(me2)
    bm.free()
    o = bpy.data.objects.new(name, me2)
    bpy.context.scene.collection.objects.link(o)
    return o


def seal(o, matname, bone, sub=1, shade=True):
    activate(o)
    if sub:
        md = o.modifiers.new('s', 'SUBSURF')
        md.levels = sub
        md.render_levels = sub
        bpy.ops.object.modifier_apply(modifier='s')
    o.data.materials.clear()
    o.data.materials.append(MAT[matname])
    if shade:
        for p in o.data.polygons:
            p.use_smooth = True
    vg = o.vertex_groups.new(name=bone)
    vg.add(list(range(len(o.data.vertices))), 1.0, 'REPLACE')
    P.append(o)
    return o


def tube(matname, bone, pts, rx, ry=None, sides=14, sub=1, cap_start=True, cap_end=True, point_end=False, name='tube', fwd=(0, -1, 0)):
    pts = [Vector(p) for p in pts]
    n = len(pts)
    rx = rx if isinstance(rx, (list, tuple)) else [rx] * n
    ry = (ry if ry is not None else rx)
    ry = ry if isinstance(ry, (list, tuple)) else [ry] * n
    bm = bmesh.new()
    rings = []
    ref = Vector(fwd)
    for i in range(n):
        a = pts[max(0, i - 1)]
        b = pts[min(n - 1, i + 1)]
        t = (b - a)
        t.normalize()
        r0 = ref if abs(t.dot(ref)) < .95 else Vector((1, 0, 0))
        side = t.cross(r0)
        side.normalize()
        fw = side.cross(t)
        fw.normalize()
        ring = []
        for k in range(sides):
            ph = 2 * math.pi * k / sides
            ring.append(bm.verts.new(pts[i] + side * (rx[i] * math.cos(ph)) + fw * (ry[i] * math.sin(ph))))
        rings.append(ring)
    for i in range(n - 1):
        for k in range(sides):
            k2 = (k + 1) % sides
            bm.faces.new((rings[i][k], rings[i][k2], rings[i + 1][k2], rings[i + 1][k]))
    if point_end:
        tip = bm.verts.new(pts[-1] + (pts[-1] - pts[-2]).normalized() * max(rx[-1], ry[-1]) * 1.1)
        for k in range(sides):
            bm.faces.new((rings[-1][k], rings[-1][(k + 1) % sides], tip))
    elif cap_end:
        bm.faces.new(rings[-1][::-1])
    if cap_start:
        bm.faces.new(rings[0])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    o = mk_obj(bm, name)
    return seal(o, matname, bone, sub)


def shell(matname, bone, center, radii, rot=(0, 0, 0), cut=-.1, thick=.012, seg=22, rings=12, sub=1, name='shell', keep='upper'):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=seg, v_segments=rings, radius=1.0)
    for v in bm.verts:
        v.co = Vector((v.co.x * radii[0], v.co.y * radii[1], v.co.z * radii[2]))
    zc = cut * radii[2]
    kill = [f for f in bm.faces if (f.calc_center_median().z < zc if keep == 'upper' else f.calc_center_median().z > zc)]
    bmesh.ops.delete(bm, geom=kill, context='FACES')
    M = Euler([math.radians(a) for a in rot], 'XYZ').to_matrix().to_4x4()
    bmesh.ops.transform(bm, matrix=Matrix.Translation(Vector(center)) @ M, verts=bm.verts)
    o = mk_obj(bm, name)
    activate(o)
    md = o.modifiers.new('sol', 'SOLIDIFY')
    md.thickness = thick
    md.offset = 1
    bpy.ops.object.modifier_apply(modifier='sol')
    return seal(o, matname, bone, sub)


def ell(matname, bone, c, r, rot=(0, 0, 0), seg=18, rings=12, sub=0):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=rings, radius=1)
    return finish(bpy.context.active_object, matname, bone, sub, True, c, r, rot)


def ring_band(matname, bone, c, R, h, rx=1.0, ry=1.0, rot=(0, 0, 0), seg=24, thick=.008):
    """flattened torus band approximating a gold trim ring"""
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=thick, major_segments=seg, minor_segments=6)
    return finish(bpy.context.active_object, matname, bone, 0, True, c, (rx, ry, h), rot)


def lock(bone, pts, w, matname='hair', taper=True, sides=7):
    n = len(pts)
    rx = [w * (1 - .8 * (i / (n - 1)) ** 1.3) for i in range(n)]
    ry = [w * .75 * (1 - .8 * (i / (n - 1)) ** 1.3) for i in range(n)]
    return tube(matname, bone, pts, rx, ry, sides, 1, True, False, True, 'lock')


HZ = 1.745
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
        X *= 1 - .30 * low ** 1.5
        Z -= .008 * low ** 2.2
        Y *= 1 - .12 * low ** 1.2
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
tube('skin', 'Neck', [(0, 0, 1.53), (0, .002, 1.64)], [.044, .04], [.046, .043], 16, 1, False, False, False, 'neck')
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


shell('hair', 'Head', (0, .014, 1.785), (.101, .111, .108), (-16, 0, 0), -.12, .012, 28, 14, 1, 'haircap')
shell('hair', 'Head', (0, .048, 1.74), (.097, .1, .125), (0, 0, 0), .0, .012, 24, 12, 1, 'hairback', 'lower')
# parted fringe: strands start at the crown, sweep across the forehead and curve to the sides
FR = [(-.8, .03, .02), (-.55, .026, .022), (-.3, .024, .026), (-.08, .022, .03), (.12, .026, .03), (.34, .03, .026), (.6, .034, .02), (.85, .04, .014)]
for i, (t, L, drift) in enumerate(FR):
    x1 = t * .095
    z_hair = 1.815 - abs(t) * .01
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

# ---------------- torso
tube('armor_silver', 'Chest', [(0, 0, 1.075), (0, .0, 1.135), (0, -.004, 1.22), (0, -.01, 1.32), (0, -.01, 1.41), (0, -.002, 1.48), (0, 0, 1.52)],
     [.15, .135, .15, .185, .2, .158, .1], [.1, .09, .098, .115, .125, .1, .078], 28, 2, True, True, False, 'cuirass')
shell('armor_blue', 'Chest', (0, -.06, 1.35), (.12, .075, .13), (-8, 0, 0), -.4, .014, 26, 12, 1, 'inlay')
ring_band('trim_gold', 'Chest', (0, -.064, 1.35), .118, 1.15, 1.0, .6, (86, 0, 0), 32, .006)
ell('gem_teal', 'Chest', (0, -.139, 1.43), (.02, .012, .028), (0, 0, 0), 12, 8)
for sx in (1, -1):
    box('trim_gold', 'Chest', (sx * .05, -.141, 1.36), (.08, .008, .014), (0, 0, -sx * 24), bevel=.003)
    box('trim_gold', 'Chest', (sx * .066, -.141, 1.322), (.065, .008, .012), (0, 0, -sx * 38), bevel=.003)
box('trim_gold', 'Chest', (0, -.141, 1.31), (.014, .008, .15), bevel=.003)
tube('armor_silver', 'Chest', [(0, 0, 1.52), (0, 0, 1.585)], [.1, .088], [.088, .08], 22, 1, False, False, False, 'gorget')
ring_band('trim_gold', 'Chest', (0, 0, 1.585), .088, 1.0, 1.0, 1.0, (0, 0, 0), 24, .007)
ring_band('trim_gold', 'Chest', (0, 0, 1.52), .1, 1.0, 1.0, .85, (0, 0, 0), 24, .007)
# waist lames with gold edges
for i, z in enumerate((1.245, 1.19, 1.135)):
    tube('armor_silver', 'Spine', [(0, 0, z - .026), (0, 0, z + .026)], [.155 - i * .003 + i * .004, .16 - i * .003 + i * .004], [.1, .104], 26, 1, False, False, False, 'lame')
    ring_band('trim_gold', 'Spine', (0, 0, z - .028), .155 - i * .003 + i * .004, 1.0, 1.0, .65, (0, 0, 0), 26, .005)
# belt
tube('leather', 'Hips', [(0, 0, 1.05), (0, 0, 1.105)], [.165, .167], [.105, .107], 26, 1, False, False, False, 'belt')
box('trim_gold', 'Hips', (0, -.12, 1.078), (.075, .018, .065), bevel=.007)
ell('gem_teal', 'Hips', (0, -.131, 1.078), (.018, .008, .022), (0, 0, 0), 10, 8)
box('leather', 'Hips', (.055, -.118, .96), (.05, .014, .22), (0, 0, -3), bevel=.003)
box('leather', 'Hips', (.0, -.117, 1.005), (.17, .012, .026), (0, 0, 14), bevel=.002)
# hip armour (tassets)
for sx in (1, -1):
    for k in range(3):
        tube('armor_silver', 'Thigh.' + S(sx), [(sx * (.15 + k * .02), -.02, 1.0 - k * .05), (sx * (.17 + k * .02), -.005, .88 - k * .05), (sx * (.18 + k * .02), .015, .78 - k * .05)],
             [.03, .034, .036], [.07, .075, .07], 12, 1, True, True, False, 'tasset')
        ring_band('trim_gold', 'Thigh.' + S(sx), (sx * (.18 + k * .02), .015, .78 - k * .05), .05, 1.0, .7, 1.4, (90, 0, 0), 16, .004)

cloth_strip('TabardFront', 'TabF.1', 'TabF.2', -.135, -.1, .1, 1.02, .5, 'cape_teal', 7, 4, flare=.18)
cloth_strip('TabardFrontIn', 'TabF.1', 'TabF.2', -.128, -.04, .04, 1.0, .52, 'robe_white', 6, 2, flare=.1)
cloth_strip('TabardBack', 'TabB.1', 'TabB.2', .135, -.12, .12, 1.02, .5, 'cape_teal', 7, 4, flare=.18)
box('trim_gold', 'TabF.1', (0, -.145, .64), (.05, .008, .09), (0, 0, 0), bevel=.003)
for sx in (1, -1):
    cloth_strip('Skirt' + S(sx), 'Hips', 'Thigh.' + S(sx), 0, .12 * sx, .26 * sx, 1.0, .6, 'robe_white', 5, 2, flare=.0)


# ---------------- arms
def arm_path(sx):
    s = S(sx)
    sh = Vector(BONES['UpperArm.' + s][0])
    el = Vector(BONES['Forearm.' + s][0])
    wr = Vector(BONES['Hand.' + s][0])
    return sh, el, wr


def build_arm(sx):
    s = S(sx)
    sh, el, wr = arm_path(sx)
    d1 = (el - sh)
    d2 = (wr - el)
    # pauldron: layered shells that overlap like scales
    for k, (dz, rr, matn) in enumerate(((.055, (.118, .112, .085), 'armor_silver'), (.02, (.124, .118, .08), 'armor_blue'), (-.015, (.126, .12, .075), 'armor_silver'))):
        shell(matn, 'UpperArm.' + s, (sx * (.21 + k * .006), 0, 1.495 + dz), rr, (0, sx * (16 + k * 4), 0), -.05, .011, 22, 12, 1, 'pauldron')
        ring_band('trim_gold', 'UpperArm.' + s, (sx * (.212 + k * .008), 0, 1.495 + dz - .004), .105, 1.0, 1.05, 1.0, (0, sx * (16 + k * 4), 0), 22, .004)
    ell('gem_teal', 'UpperArm.' + s, (sx * .235, -.075, 1.555), (.012, .007, .012), (0, 0, 0), 8, 6)
    # upper arm: dark sleeve + silver rerebrace
    tube('armor_dark', 'UpperArm.' + s, [sh + d1 * -.05, sh + d1 * .55, sh + d1 * 1.02], [.06, .056, .05], [.062, .058, .052], 14, 1, True, True, False, 'sleeve')
    tube('armor_silver', 'UpperArm.' + s, [sh + d1 * .3, sh + d1 * .62, sh + d1 * .9], [.066, .064, .058], [.068, .066, .06], 14, 1, True, True, False, 'rerebrace')
    ring_band('trim_gold', 'UpperArm.' + s, sh + d1 * .3, .066, 1.0, 1.0, 1.0, (0, sx * 20, 0), 16, .005)
    # elbow couter + wing
    shell('armor_silver', 'Forearm.' + s, (el.x, el.y + .02, el.z + .005), (.07, .068, .07), (-100, 0, 0), -.15, .012, 16, 10, 1, 'couter')
    box('armor_silver', 'Forearm.' + s, (el.x + sx * .04, el.y + .05, el.z), (.018, .06, .11), (-25, 0, sx * 14), bevel=.006)
    ring_band('trim_gold', 'Forearm.' + s, (el.x, el.y + .02, el.z), .058, 1.0, 1.0, 1.0, (0, sx * 9, 0), 16, .005)
    # vambrace
    tube('armor_dark', 'Forearm.' + s, [el + d2 * -.02, el + d2 * .5, wr], [.05, .044, .038], [.052, .046, .04], 14, 1, True, True, False, 'forearm')
    tube('armor_silver', 'Forearm.' + s, [el + d2 * .15, el + d2 * .5, el + d2 * .95], [.054, .05, .044], [.056, .052, .046], 14, 1, True, True, False, 'vambrace')
    ring_band('trim_gold', 'Forearm.' + s, el + d2 * .97, .046, 1.0, 1.0, 1.0, (0, sx * 9, 0), 16, .005)
    box('leather', 'Forearm.' + s, (el.x + d2.x * .5, el.y - .052, el.z + d2.z * .5), (.02, .012, .17), (0, sx * 9, 0), bevel=.002)
    # gauntlet
    h0 = wr
    box('armor_silver', 'Hand.' + s, (h0.x + sx * .01, h0.y - .005, h0.z - .04), (.074, .076, .072), (0, sx * 7, 0), bevel=.014)
    ring_band('trim_gold', 'Hand.' + s, (h0.x, h0.y, h0.z - .004), .042, 1.0, 1.0, 1.0, (0, sx * 9, 0), 14, .005)
    for f in range(4):
        fx = h0.x + sx * (.01 + (f - 1.5) * .0095)
        base = Vector((fx, h0.y - .03, h0.z - .07))
        tube('armor_dark', 'Hand.' + s, [base, base + Vector((0, -.012, -.028)), base + Vector((0, -.004, -.055)), base + Vector((0, .014, -.071))], [.0085, .0082, .0075, .0065], [.0095, .009, .0082, .007], 8, 1, True, True, False, 'finger')
    tb = Vector((h0.x - sx * .012, h0.y - .05, h0.z - .05))
    tube('armor_dark', 'Hand.' + s, [tb, tb + Vector((-sx * .008, -.022, -.015)), tb + Vector((-sx * .004, -.036, -.04))], [.0105, .0095, .008], [.0105, .0095, .008], 8, 1, True, True, False, 'thumb')


build_arm(1)
build_arm(-1)


# ---------------- legs
def build_leg(sx):
    s = S(sx)
    hip = Vector(BONES['Thigh.' + s][0])
    kn = Vector(BONES['Shin.' + s][0])
    an = Vector(BONES['Foot.' + s][0])
    d1 = kn - hip
    d2 = an - kn
    tube('armor_dark', 'Thigh.' + s, [hip + d1 * -.02, hip + d1 * .5, kn + d1 * .02], [.088, .078, .062], [.09, .08, .064], 16, 1, True, True, False, 'thighdark')
    tube('armor_silver', 'Thigh.' + s, [hip + d1 * .02 + Vector((0, -.012, 0)), hip + d1 * .5 + Vector((0, -.014, 0)), hip + d1 * .93 + Vector((0, -.01, 0))], [.098, .088, .072], [.098, .09, .074], 16, 1, True, True, False, 'cuisse')
    ring_band('trim_gold', 'Thigh.' + s, hip + d1 * .88 + Vector((0, -.01, 0)), .072, 1.0, 1.0, 1.0, (0, 0, 0), 18, .006)
    ring_band('trim_gold', 'Thigh.' + s, hip + d1 * .15 + Vector((0, -.012, 0)), .094, 1.0, 1.0, 1.0, (0, 0, 0), 18, .005)
    shell('armor_silver', 'Shin.' + s, (kn.x, kn.y - .045, kn.z + .008), (.068, .06, .066), (-90, 0, 0), -.2, .012, 16, 10, 1, 'poleyn')
    box('trim_gold', 'Shin.' + s, (kn.x, kn.y - .095, kn.z + .005), (.016, .016, .08), bevel=.004)
    tube('armor_dark', 'Shin.' + s, [kn + d2 * -.02, kn + d2 * .5, an + Vector((0, 0, .015))], [.062, .054, .045], [.064, .056, .047], 16, 1, True, True, False, 'shindark')
    tube('armor_silver', 'Shin.' + s, [kn + d2 * .04 + Vector((0, -.012, 0)), kn + d2 * .5 + Vector((0, -.014, 0)), kn + d2 * .95 + Vector((0, -.008, 0))], [.068, .06, .052], [.066, .058, .052], 16, 1, True, True, False, 'greave')
    box('trim_gold', 'Shin.' + s, (kn.x, kn.y + d2.y * .5 - .072, kn.z + d2.z * .5), (.014, .012, .2), bevel=.003)
    ring_band('trim_gold', 'Shin.' + s, kn + d2 * .92 + Vector((0, -.008, 0)), .052, 1.0, 1.0, 1.0, (0, 0, 0), 16, .005)
    # boot
    tube('leather', 'Foot.' + s, [(sx * .11, .018, .17), (sx * .11, .014, .1), (sx * .11, -.03, .062), (sx * .11, -.11, .05), (sx * .11, -.17, .04)],
         [.058, .06, .055, .05, .038], [.06, .062, .052, .044, .028], 14, 1, True, True, False, 'boot')
    tube('armor_silver', 'Foot.' + s, [(sx * .11, -.1, .056), (sx * .11, -.16, .045), (sx * .11, -.19, .038)], [.047, .042, .03], [.04, .034, .024], 12, 1, True, True, False, 'toecap')
    box('steel_dark', 'Foot.' + s, (sx * .11, -.06, .012), (.1, .26, .026), bevel=.008)


build_leg(1)
build_leg(-1)


# ---------------- cape: three columns blended by weights, plus collar
def cape():
    bm = bmesh.new()
    C = 14
    R = 18
    grid = []
    for r in range(R + 1):
        t = r / R
        z = 1.53 - 1.18 * t
        row = []
        for c in range(C + 1):
            u = c / C * 2 - 1
            half = .24 + .14 * t
            x = u * half * 1.0
            wrap = (1 - u * u) * .05
            y = .125 + .16 * t + abs(u) * (-.0) - (0.02 * (1 - t) if abs(u) > .6 else 0) + wrap * (1 - t) * .5
            wav = math.sin(u * 6 + t * 4) * .018 * t
            row.append(bm.verts.new((x, y + wav, z)))
        grid.append(row)
    for r in range(R):
        for c in range(C):
            f = bm.faces.new((grid[r][c], grid[r + 1][c], grid[r + 1][c + 1], grid[r][c + 1]))
            f.smooth = True
    me2 = bpy.data.meshes.new('Cape')
    bm.to_mesh(me2)
    bm.free()
    o = bpy.data.objects.new('Cape', me2)
    bpy.context.scene.collection.objects.link(o)
    activate(o)
    md = o.modifiers.new('sol', 'SOLIDIFY')
    md.thickness = .014
    md.offset = 0
    bpy.ops.object.modifier_apply(modifier='sol')
    o.data.materials.append(MAT['cape_teal'])
    o.data.materials.append(MAT['cape_inner'])
    # inner face material: faces whose normal points toward -Y (facing body)
    for p in o.data.polygons:
        p.use_smooth = True
        p.material_index = 1 if p.normal.y < 0 else 0
    names = ['Cape%s.%d' % (c, i) for c in 'CLR' for i in (1, 2, 3)]
    vg = {n: o.vertex_groups.new(name=n) for n in names + ['Chest']}
    for v in o.data.vertices:
        t = max(0, min(1, (1.53 - v.co.z) / 1.18))
        u = max(-1, min(1, v.co.x / (.24 + .14 * t)))
        wl = max(0, u)
        wr = max(0, -u)
        wc = 1 - wl - wr
        # rows: segment index from t (3 segments)
        seg = t * 3
        i0 = min(2, int(seg))
        fr = seg - i0
        chest_w = max(0, 1 - t * 8)
        rest = 1 - chest_w
        for col, w in (('C', wc), ('L', wl), ('R', wr)):
            if rest * w * (1 - fr) > 0:
                vg['Cape%s.%d' % (col, i0 + 1)].add([v.index], rest * w * (1 - fr), 'ADD')
            if i0 < 2 and rest * w * fr > 0:
                vg['Cape%s.%d' % (col, i0 + 2)].add([v.index], rest * w * fr, 'ADD')
            elif i0 == 2 and rest * w * fr > 0:
                vg['Cape%s.3' % col].add([v.index], rest * w * fr, 'ADD')
        if chest_w > 0:
            vg['Chest'].add([v.index], chest_w, 'ADD')
    P.append(o)
    # fur-free collar
    cyl('cape_teal', 'Chest', (0, .02, 1.55), .13, .06, seg=24, r2=.115)
    for sx in (1, -1):
        box('trim_gold', 'Chest', (sx * .14, -.02, 1.545), (.035, .02, .035), bevel=.008)
        sph('gem_teal', 'Chest', (sx * .14, -.034, 1.545), (.012, .006, .012), seg=8, rings=6)


cape()

# ---------------- sword in a sheath at the left hip + shoulder baldric
def sword():
    ang = (0, 0, 0)
    cyl('leather', 'SwordSheath', (.275, .075, .66), .028, .6, (-14, 14, 0), 10, (1, .5, 1), 0.0)
    cyl('trim_gold', 'SwordSheath', (.246, .062, .96), .032, .05, (-14, 14, 0), 10, (1, .6, 1))
    cyl('trim_gold', 'SwordSheath', (.31, .095, .38), .024, .05, (-14, 14, 0), 10, (1, .6, 1))
    # hilt above the belt: grip, pommel, guard
    cyl('leather', 'SwordSheath', (.222, .05, 1.1), .018, .16, (-14, 14, 0), 10)
    sph('trim_gold', 'SwordSheath', (.212, .047, 1.19), (.026, .026, .026), seg=10, rings=8)
    box('trim_gold', 'SwordSheath', (.232, .054, 1.03), (.19, .028, .028), (-14, 14, 0), bevel=.008)
    box('blade_glow', 'SwordSheath', (.236, .056, .98), (.035, .014, .03), (-14, 14, 0), bevel=.003)
    # shoulder baldric strap
    box('leather', 'Chest', (.0, -.118, 1.34), (.034, .012, .5), (0, 0, -32), bevel=.003)
    # satchel and pouch
    box('leather', 'Hips', (-.2, -.02, .97), (.1, .08, .12), (0, 0, 8), bevel=.014)
    box('trim_gold', 'Hips', (-.2, -.063, .995), (.02, .01, .03), bevel=.003)


sword()

# ------------------------------------------------------------------ join everything into one skinned mesh
bpy.ops.object.select_all(action='DESELECT')
for o in P:
    o.select_set(True)
body.select_set(True)
bpy.context.view_layer.objects.active = body
for o in P:
    o.parent = None
bpy.ops.object.join()
knight = body
knight.name = 'Knight'
# make sure armature modifier present
if not any(m.type == 'ARMATURE' for m in knight.modifiers):
    am = knight.modifiers.new('Armature', 'ARMATURE')
    am.object = arm
knight.parent = arm
# smooth/normal cleanup
activate(knight)
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.normals_make_consistent(inside=False)
bpy.ops.object.mode_set(mode='OBJECT')
print('KNIGHT verts', len(knight.data.vertices), 'groups', len(knight.vertex_groups), 'mats', len(knight.data.materials))

# ------------------------------------------------------------------ animation
pb = arm.pose.bones
for b in pb:
    b.rotation_mode = 'XYZ'
activate(arm)
bpy.ops.object.mode_set(mode='POSE')

D = math.radians


def key(name, frame, rot=None, loc=None):
    b = pb[name]
    if rot is not None:
        b.rotation_euler = (D(rot[0]), D(rot[1]), D(rot[2]))
        b.keyframe_insert('rotation_euler', frame=frame)
    if loc is not None:
        b.location = loc
        b.keyframe_insert('location', frame=frame)


def reset():
    for b in pb:
        b.rotation_euler = (0, 0, 0)
        b.location = (0, 0, 0)


def new_action(name):
    reset()
    act = bpy.data.actions.new(name)
    arm.animation_data_create()
    arm.animation_data.action = act
    return act


def cape_sway(frame, amp, t, speed):
    for col, ph in (('C', 0), ('L', .9), ('R', 1.8)):
        for i in (1, 2, 3):
            lag = i * .55
            ang = 4 + speed * (10 + 12 * i) + math.sin(t - lag + ph) * (2 + amp * 7) * i * .5
            key('Cape%s.%d' % (col, i), frame, (ang, math.sin(t * .5 - lag + ph) * (1.5 + speed * 3), math.sin(t - lag * 1.3 + ph) * (2 + amp * 6) * (1 if col == 'C' else (1.4 if col == 'L' else -1.4))))
    for i in (1, 2, 3):
        key('Pony.%d' % i, frame, (6 + speed * 16 * i * .6 + math.sin(t - i * .5) * (3 + amp * 5), 0, math.sin(t * .7 - i * .5) * (3 + amp * 6)))
    for nm in ('TabF', 'TabB'):
        pass


def tabard(frame, thl, thr):
    f = (thl + thr) * .5
    key('TabF.1', frame, (min(0, (thl + thr) * -.3) * 0 + f * .35, 0, 0))
    key('TabF.2', frame, (f * .5, 0, 0))
    key('TabB.1', frame, (-f * .18, 0, 0))
    key('TabB.2', frame, (-f * .3, 0, 0))


def locomotion(name, frames, stride, knee, armamp, bob, lean, elbow, tilt, speed, hipsway):
    act = new_action(name)
    for f in range(frames + 1):
        t = 2 * math.pi * f / frames
        s, c = math.sin(t), math.cos(t)
        thL = -stride * s
        thR = stride * s
        shL = knee * max(0, c) + 4
        shR = knee * max(0, -c) + 4
        if name == 'Run':
            shL = knee * (.35 + .65 * max(0, c))
            shR = knee * (.35 + .65 * max(0, -c))
        key('Root', f, loc=(0, 0, 0))
        key('Hips', f, (lean * .5 + 2 * c * c, -hipsway * s, hipsway * 2 * s), (hipsway * .006 * s, 0, -bob * abs(s) if name == 'Walk' else bob * (math.cos(2 * t) * .5 - .5)))
        key('Spine', f, (lean * .5, hipsway * s * 1.2, -hipsway * 1.8 * s))
        key('Chest', f, (lean * .3 + 1.5 * c * c, hipsway * s * 1.0, -hipsway * 2.2 * s))
        key('Neck', f, (-lean * .5, 0, 0))
        key('Head', f, (-lean * .4 + tilt, 0, hipsway * 1.3 * s))
        key('Thigh.L', f, (thL, 0, 2))
        key('Thigh.R', f, (thR, 0, -2))
        key('Shin.L', f, (shL, 0, 0))
        key('Shin.R', f, (shR, 0, 0))
        key('Foot.L', f, (-(thL + shL) * .7 + (max(0, -c) * 18 if name == 'Walk' else 0), 0, 0))
        key('Foot.R', f, (-(thR + shR) * .7 + (max(0, c) * 18 if name == 'Walk' else 0), 0, 0))
        key('Toe.L', f, (max(0, -c * s) * 12 * 0 + max(0, c) * 10, 0, 0))
        key('Toe.R', f, (max(0, -c) * 10, 0, 0))
        aL = armamp * s
        aR = -armamp * s
        key('Clavicle.L', f, (0, 0, -2 * s))
        key('Clavicle.R', f, (0, 0, 2 * s))
        key('UpperArm.L', f, (aL, 0, 6 + speed * 6))
        key('UpperArm.R', f, (aR, 0, -6 - speed * 6))
        key('Forearm.L', f, (-(elbow + 14 * max(0, -s)), 0, 0))
        key('Forearm.R', f, (-(elbow + 14 * max(0, s)), 0, 0))
        key('Hand.L', f, (-6, 0, 0))
        key('Hand.R', f, (-6, 0, 0))
        key('SwordSheath', f, (-(thL * .2) + lean * .3, 0, 0))
        tabard(f, thL, thR)
        cape_sway(f, speed, t, speed)
    return act


locomotion('Walk', 28, 26, 36, 22, .028, 2, 14, 0, .25, 2.5)
locomotion('Run', 18, 50, 105, 52, .06, 14, 78, 4, 1.0, 6)


def idle():
    act = new_action('Idle')
    N = 72
    for f in range(N + 1):
        t = 2 * math.pi * f / N
        b = math.sin(t)
        key('Root', f, loc=(0, 0, 0))
        key('Hips', f, (0, 0, 0), (0, 0, .004 * b))
        key('Spine', f, (1.2 * b, 0, 0))
        key('Chest', f, (1.8 * b, .8 * math.sin(t * .5), 0))
        key('Neck', f, (-1 * b, 0, 0))
        key('Head', f, (-2 + 1.2 * b, 6 * math.sin(t * .5), 1.5 * math.sin(t)))
        for s, sx in (('L', 1), ('R', -1)):
            key('Clavicle.' + s, f, (0, 0, sx * 1.5 * b))
            key('UpperArm.' + s, f, (2 * b * -1 + 4, 0, sx * -(7 + b * 1.5)))
            key('Forearm.' + s, f, (-14 - 3 * b, 0, 0))
            key('Hand.' + s, f, (-5, 0, 0))
            key('Thigh.' + s, f, (0, 0, sx * 2.5))
            key('Shin.' + s, f, (2, 0, 0))
            key('Foot.' + s, f, (-2, 0, 0))
            key('Toe.' + s, f, (0, 0, 0))
        key('SwordSheath', f, (0, 0, 0))
        tabard(f, 0, 0)
        cape_sway(f, .15, t, 0)
    return act


idle()


def jump():
    act = new_action('Jump')
    # phases: 0 stand, 6 crouch, 12 launch, 22 apex, 32 falling, 40 contact, 46 absorb, 54 recover
    K = {
        0: dict(hz=0, sp=0, ch=0, th=0, sh=2, ft=0, ua=0, fa=-14, ua_z=7, cape=4, head=-2),
        6: dict(hz=-.19, sp=18, ch=8, th=-62, sh=104, ft=-42, ua=28, fa=-42, ua_z=12, cape=-10, head=-14),
        12: dict(hz=.03, sp=-8, ch=-6, th=14, sh=6, ft=40, ua=-130, fa=-34, ua_z=24, cape=40, head=6),
        22: dict(hz=.16, sp=-3, ch=-3, th=-40, sh=70, ft=22, ua=-148, fa=-22, ua_z=48, cape=62, head=8),
        32: dict(hz=.08, sp=4, ch=2, th=-58, sh=88, ft=12, ua=-70, fa=-32, ua_z=38, cape=28, head=2),
        40: dict(hz=-.01, sp=10, ch=4, th=-34, sh=36, ft=10, ua=-24, fa=-24, ua_z=20, cape=10, head=-4),
        46: dict(hz=-.2, sp=20, ch=10, th=-68, sh=112, ft=-46, ua=24, fa=-48, ua_z=14, cape=-12, head=-16),
        56: dict(hz=0, sp=0, ch=0, th=0, sh=2, ft=0, ua=0, fa=-14, ua_z=7, cape=4, head=-2),
    }
    for f, k in K.items():
        key('Root', f, loc=(0, 0, 0))
        key('Hips', f, (k['sp'] * .25, 0, 0), (0, 0, k['hz']))
        key('Spine', f, (k['sp'] * .5, 0, 0))
        key('Chest', f, (k['ch'], 0, 0))
        key('Neck', f, (-k['ch'] * .4, 0, 0))
        key('Head', f, (k['head'], 0, 0))
        for s, sx in (('L', 1), ('R', -1)):
            off = 6 if f in (22, 32) and s == 'R' else 0
            key('Thigh.' + s, f, (k['th'] + off, 0, sx * 3))
            key('Shin.' + s, f, (k['sh'] - off * 2, 0, 0))
            key('Foot.' + s, f, (k['ft'], 0, 0))
            key('Toe.' + s, f, (12 if f in (12, 22) else 0, 0, 0))
            key('Clavicle.' + s, f, (0, 0, 0))
            key('UpperArm.' + s, f, (k['ua'], 0, sx * -k['ua_z']))
            key('Forearm.' + s, f, (k['fa'], 0, 0))
            key('Hand.' + s, f, (-6, 0, 0))
        key('SwordSheath', f, (k['th'] * -.2, 0, 0))
        for col in 'CLR':
            for i in (1, 2, 3):
                key('Cape%s.%d' % (col, i), f, (k['cape'] * (.5 + i * .2), 0, 0))
        for i in (1, 2, 3):
            key('Pony.%d' % i, f, (k['cape'] * .5 * i * .5, 0, 0))
        tabard(f, k['th'], k['th'])
    return act


jump()


def sit_pose(f, k):
    key('Root', f, loc=(0, 0, 0))
    key('Hips', f, (k['hp'], 0, 0), (0, k['hy'], k['hz']))
    key('Spine', f, (k['sp'], 0, 0))
    key('Chest', f, (k['ch'], 0, 0))
    key('Neck', f, (k['ne'], 0, 0))
    key('Head', f, (k['he'], k['hr'], 0))
    for s, sx in (('L', 1), ('R', -1)):
        key('Thigh.' + s, f, (k['th'], 0, sx * k['sp_out']))
        key('Shin.' + s, f, (k['sh'], 0, 0))
        key('Foot.' + s, f, (k['ft'], 0, 0))
        key('Toe.' + s, f, (0, 0, 0))
        key('Clavicle.' + s, f, (0, 0, 0))
        key('UpperArm.' + s, f, (k['ua'], 0, sx * -k['uaz']))
        key('Forearm.' + s, f, (k['fa'], 0, 0))
        key('Hand.' + s, f, (-6, 0, 0))
    key('SwordSheath', f, (k['th'] * -.1, 0, 0))
    for col in 'CLR':
        for i in (1, 2, 3):
            key('Cape%s.%d' % (col, i), f, (k['cape'], 0, 0))
    for i in (1, 2, 3):
        key('Pony.%d' % i, f, (k['cape'] * .3, 0, 0))
    key('TabF.1', f, (k['tf'], 0, 0))
    key('TabF.2', f, (k['tf'] * .8, 0, 0))
    key('TabB.1', f, (-k['tb'], 0, 0))
    key('TabB.2', f, (-k['tb'], 0, 0))


SITK = dict(hp=-4, hy=.16, hz=-.5, sp=4, ch=2, ne=0, he=-2, hr=0, th=-92, sh=92, ft=-2, sp_out=3, ua=-12, uaz=12, fa=-52, cape=-8, tf=-45, tb=-8)
STAND = dict(hp=0, hy=0, hz=0, sp=0, ch=0, ne=0, he=-2, hr=0, th=0, sh=2, ft=0, sp_out=2, ua=0, uaz=7, fa=-14, cape=4, tf=0, tb=0)


def sit_down():
    act = new_action('SitDown')
    mid = dict(hp=-8, hy=.1, hz=-.28, sp=10, ch=6, ne=0, he=-6, hr=0, th=-60, sh=70, ft=-20, sp_out=3, ua=-8, uaz=10, fa=-40, cape=-6, tf=-25, tb=-4)
    sit_pose(0, STAND)
    sit_pose(10, mid)
    sit_pose(22, dict(SITK, hz=-.46, sp=8))
    sit_pose(32, SITK)
    return act


def sit_idle():
    act = new_action('SitIdle')
    N = 90
    for f in range(N + 1):
        t = 2 * math.pi * f / N
        b = math.sin(t)
        k = dict(SITK)
        k['sp'] += 1.2 * b
        k['ch'] += 1.4 * b
        k['he'] = -2 + 1.2 * b
        k['hr'] = 7 * math.sin(t * .5)
        k['ua'] = -12 - 1.5 * b
        k['cape'] = -8 + math.sin(t) * 1.5
        sit_pose(f, k)
    return act


sit_down()
sit_idle()
bpy.ops.object.mode_set(mode='OBJECT')
reset()
arm.animation_data.action = bpy.data.actions['Idle']
for a in bpy.data.actions:
    a.use_fake_user = True
bpy.context.scene.frame_start = 0
bpy.context.scene.frame_end = 90

# ------------------------------------------------------------------ export FBX
activate(knight)
knight.select_set(True)
arm.select_set(True)
bpy.context.view_layer.objects.active = arm
bpy.ops.export_scene.fbx(filepath=OUTFBX, use_selection=True, apply_unit_scale=True, global_scale=1.0, axis_forward='-Z', axis_up='Y',
                         bake_space_transform=False, object_types={'ARMATURE', 'MESH'}, add_leaf_bones=False, bake_anim=True,
                         bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False, bake_anim_force_startend_keying=True,
                         bake_anim_step=1.0, bake_anim_simplify_factor=0.0, mesh_smooth_type='FACE', path_mode='COPY', embed_textures=False,
                         armature_nodetype='NULL', use_armature_deform_only=False)
bpy.ops.wm.save_as_mainfile(filepath=BLEND)
print('EXPORTED KNIGHT2', OUTFBX)
