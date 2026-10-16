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
# ---------------- head
tube('skin', 'Head', [(0, -.03, 1.64), (0, -.032, 1.67), (0, -.02, 1.70), (0, -.008, 1.735), (0, -.003, 1.77), (0, 0, 1.805), (0, .006, 1.835), (0, .01, 1.856)],
     [.03, .062, .083, .092, .092, .084, .06, .028], [.04, .06, .082, .097, .104, .104, .085, .05], 24, 2, True, True, False, 'head')
tube('skin', 'Neck', [(0, 0, 1.53), (0, .002, 1.64)], [.05, .048], [.052, .05], 16, 1, False, False, False, 'neck')
for sx in (1, -1):
    ell('skin', 'Head', (sx * .093, .0, HZ - .005), (.012, .026, .04), (0, 0, sx * -10))
    # eyes: white, iris, pupil, highlight, upper lash, brow
    ell('eye_white', 'Head', (sx * .038, -.099, HZ + .004), (.027, .012, .021), (0, 0, sx * -6), 16, 10)
    ell('eye_green', 'Head', (sx * .036, -.108, HZ + .003), (.0165, .006, .0185), (0, 0, 0), 16, 10)
    ell('eye_dark', 'Head', (sx * .036, -.113, HZ + .003), (.0075, .003, .009), (0, 0, 0), 10, 8)
    ell('eye_white', 'Head', (sx * .031, -.1155, HZ + .01), (.0035, .002, .0035), (0, 0, 0), 8, 6)
    box('hair', 'Head', (sx * .038, -.105, HZ + .022), (.056, .008, .01), (0, 0, sx * -7), bevel=.003)
    box('hair', 'Head', (sx * .041, -.1, HZ + .043), (.05, .009, .008), (0, 0, sx * 12), bevel=.003)
ell('skin', 'Head', (0, -.108, HZ - .026), (.011, .016, .018))
box('lips', 'Head', (0, -.1, HZ - .062), (.032, .007, .006), bevel=.002)
for sx in (1, -1):
    box('lips', 'Head', (sx * .02, -.098, HZ - .058), (.012, .007, .006), (0, 0, sx * 38), bevel=.002)
# hair cap
shell('hair', 'Head', (0, .016, 1.79), (.102, .112, .105), (22, 0, 0), -.25, .012, 26, 14, 1, 'haircap')
shell('hair', 'Head', (0, .045, 1.745), (.098, .1, .12), (0, 0, 0), .0, .012, 22, 12, 1, 'hairback', 'lower')
# bangs and swept locks
for i in range(13):
    t = (i - 6) / 6.0
    x0 = t * .05
    x1 = t * .098
    z1 = 1.80 - abs(t) * .01
    L = .045 + .035 * (1 - abs(t) ** 1.5) + random.uniform(-.008, .012)
    sweep = -t * .02 + random.uniform(-.012, .012)
    lock('Head', [(x0, -.06, 1.875), (x0 * .6 + x1 * .4, -.1, 1.845), (x1, -.112, z1 - .005), (x1 + sweep, -.112 + abs(t) * .006, z1 - L)], .016 + (1 - abs(t)) * .004)
for sx in (1, -1):
    for k in range(4):
        zt = 1.8 - k * .008
        lock('Head', [(sx * (.088 + k * .001), -.03 + k * .008, zt), (sx * (.1 + k * .002), -.02 + k * .012, zt - .06), (sx * (.098 + k * .002), -.03 + k * .01, zt - .125 - k * .012), (sx * (.088 + k * .005), -.045 + k * .008, zt - .19 - k * .018)], .014)
    lock('Head', [(sx * .09, -.07, 1.8), (sx * .1, -.085, 1.745), (sx * .098, -.09, 1.7), (sx * .09, -.095, 1.655)], .012)
for k in range(7):
    xo = (k - 3) * .026
    lock('Head', [(xo, .09, 1.835 - abs(k - 3) * .01), (xo * 1.05, .125, 1.79), (xo * 1.1, .135, 1.73), (xo * 1.15, .13, 1.68)], .02)
# ponytail: three bone-bound segments, strands overlapped for volume
for si, bn in enumerate(('Pony.1', 'Pony.2', 'Pony.3')):
    a, b2 = BONES[bn]
    for st in range(5):
        off = (st - 2) * .014
        mid = (Vector(a) + Vector(b2)) * .5
        w = .028 - si * .004
        tube('hair', bn, [Vector(a) + Vector((off, 0, 0)), mid + Vector((off * 1.3, .008, 0)), Vector(b2) + Vector((off * 1.2 if si < 2 else off * .5, .006, 0))],
             [w, w * 1.1, w * (.9 if si < 2 else .35)], [w * .9, w, w * (.8 if si < 2 else .3)], 8, 1, True, si == 2 and False, si == 2, 'pony')
cyl('trim_gold', 'Head', (0, .128, 1.775), .033, .024, (80, 0, 0), 14)

# ---------------- torso
tube('armor_silver', 'Chest', [(0, 0, 1.075), (0, .0, 1.135), (0, -.004, 1.22), (0, -.01, 1.32), (0, -.01, 1.41), (0, -.002, 1.48), (0, 0, 1.52)],
     [.165, .152, .165, .192, .205, .16, .1], [.108, .098, .104, .118, .128, .102, .078], 28, 2, True, True, False, 'cuirass')
shell('armor_blue', 'Chest', (0, -.052, 1.375), (.135, .085, .17), (-8, 0, 0), -.35, .014, 26, 12, 1, 'inlay')
ring_band('trim_gold', 'Chest', (0, -.058, 1.375), .125, 1.45, 1.0, .6, (86, 0, 0), 32, .007)
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
    tube('armor_silver', 'Spine', [(0, 0, z - .026), (0, 0, z + .026)], [.172 - i * .004, .178 - i * .004], [.109, .113], 26, 1, False, False, False, 'lame')
    ring_band('trim_gold', 'Spine', (0, 0, z - .028), .172 - i * .004, 1.0, 1.0, .64, (0, 0, 0), 26, .005)
# belt
tube('leather', 'Hips', [(0, 0, 1.05), (0, 0, 1.105)], [.178, .18], [.113, .115], 26, 1, False, False, False, 'belt')
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
    for k, (dz, rr, matn) in enumerate(((.075, (.15, .14, .095), 'armor_silver'), (.04, (.155, .145, .09), 'armor_blue'), (.005, (.158, .148, .085), 'armor_silver'), (-.03, (.15, .14, .08), 'armor_blue'))):
        shell(matn, 'UpperArm.' + s, (sx * (.215 + k * .006), 0, 1.5 + dz), rr, (0, sx * (14 + k * 3), 0), -.1, .011, 22, 12, 1, 'pauldron')
        ring_band('trim_gold', 'UpperArm.' + s, (sx * (.218 + k * .008), 0, 1.5 + dz - .006), .13, 1.0, 1.08, 1.0, (0, sx * (14 + k * 3), 0), 22, .004)
    ell('gem_teal', 'UpperArm.' + s, (sx * .262, -.062, 1.59), (.013, .007, .013), (0, 0, 0), 8, 6)
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
    tube('armor_silver', 'Thigh.' + s, [hip + d1 * .1 + Vector((0, -.012, 0)), hip + d1 * .5 + Vector((0, -.014, 0)), hip + d1 * .9 + Vector((0, -.01, 0))], [.095, .084, .07], [.095, .086, .072], 16, 1, True, True, False, 'cuisse')
    ring_band('trim_gold', 'Thigh.' + s, hip + d1 * .88 + Vector((0, -.01, 0)), .072, 1.0, 1.0, 1.0, (0, 0, 0), 18, .006)
    ring_band('trim_gold', 'Thigh.' + s, hip + d1 * .15 + Vector((0, -.012, 0)), .094, 1.0, 1.0, 1.0, (0, 0, 0), 18, .005)
    shell('armor_silver', 'Shin.' + s, (kn.x, kn.y - .045, kn.z + .008), (.068, .06, .066), (-90, 0, 0), -.2, .012, 16, 10, 1, 'poleyn')
    box('trim_gold', 'Shin.' + s, (kn.x, kn.y - .095, kn.z + .005), (.016, .016, .08), bevel=.004)
    tube('armor_dark', 'Shin.' + s, [kn + d2 * -.02, kn + d2 * .5, an + Vector((0, 0, .015))], [.062, .054, .045], [.064, .056, .047], 16, 1, True, True, False, 'shindark')
    tube('armor_silver', 'Shin.' + s, [kn + d2 * .1 + Vector((0, -.012, 0)), kn + d2 * .5 + Vector((0, -.014, 0)), kn + d2 * .92 + Vector((0, -.008, 0))], [.066, .058, .05], [.064, .056, .05], 16, 1, True, True, False, 'greave')
    box('trim_gold', 'Shin.' + s, (kn.x, kn.y + d2.y * .5 - .072, kn.z + d2.z * .5), (.014, .012, .2), bevel=.003)
    ring_band('trim_gold', 'Shin.' + s, kn + d2 * .92 + Vector((0, -.008, 0)), .052, 1.0, 1.0, 1.0, (0, 0, 0), 16, .005)
    # boot
    tube('leather', 'Foot.' + s, [(sx * .11, .018, .17), (sx * .11, .014, .1), (sx * .11, -.03, .062), (sx * .11, -.11, .05), (sx * .11, -.17, .04)],
         [.058, .06, .055, .05, .038], [.06, .062, .052, .044, .028], 14, 1, True, True, False, 'boot')
    tube('armor_silver', 'Foot.' + s, [(sx * .11, -.1, .056), (sx * .11, -.16, .045), (sx * .11, -.19, .038)], [.047, .042, .03], [.04, .034, .024], 12, 1, True, True, False, 'toecap')
    box('steel_dark', 'Foot.' + s, (sx * .11, -.06, .012), (.1, .26, .026), bevel=.008)


build_leg(1)
build_leg(-1)
