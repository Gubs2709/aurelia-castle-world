import bpy, bmesh, math, random, os
from mathutils import Vector, Matrix, Euler

OUT = "D:/GAME/AureliaUnity/Assets/Models/Village/"
# metres per UV tile per material
TILE = {
    'wall_plaster': 2.6, 'wall_stone': 2.2, 'timber': 1.6, 'wood_light': 1.6, 'wood_dark': 1.6,
    'roof_terracotta': 2.4, 'roof_red': 2.4, 'roof_blue': 2.4, 'roof_slate': 2.4, 'roof_shingle': 2.2, 'roof_teal': 2.4,
    'path_flagstone': 3.0, 'cobble': 2.0,
}
_rand = random.Random(1)

PAL = {
    'wall_plaster': '#efe3c8', 'wall_stone': '#a9a396', 'timber': '#4a3020', 'wood_light': '#a67c52', 'wood_dark': '#5a3d28',
    'roof_terracotta': '#b8562f', 'roof_slate': '#566a7c', 'roof_shingle': '#7a5c3c', 'roof_teal': '#2fa8a2',
    'glass': '#4a7aa0', 'shutter_green': '#4e8a5a', 'shutter_blue': '#4f7db0', 'shutter_red': '#b0483a', 'shutter_yellow': '#d9ac48',
    'awning_red': '#cf4a3a', 'awning_blue': '#3f67a6', 'awning_yellow': '#e4c25a', 'awning_green': '#3f8a5a', 'awning_white': '#f2eee0',
    'flower_pink': '#f08ab4', 'flower_yellow': '#fcd23c', 'flower_red': '#e0382c', 'flower_purple': '#9a6ad8', 'flower_white': '#fff8ee', 'leaf': '#4f9a3c',
    'iron': '#2a2a30', 'lantern_glow': '#ffd27a', 'door_wood': '#6a4428', 'banner_blue': '#2f4fa0', 'banner_yellow': '#e8b43a', 'banner_red': '#b8382c',
    'fruit_orange': '#f0821e', 'fruit_red': '#d8301e', 'fruit_green': '#7ab43a', 'fruit_yellow': '#f0cf3a', 'crate_wood': '#b68a56', 'rope': '#c8b080',
    'cloth_white': '#f2eee0', 'stone_light': '#d8d2c2', 'gold': '#e0b84a', 'water': '#4aa8c0', 'hay': '#e0c060', 'chimney_smoke': '#e0e0e0',
    'armor_silver': '#c8d2dc', 'armor_dark': '#2a2c34', 'cape_teal': '#1f9a98', 'skin': '#f0c8a0', 'hair': '#8a5a34', 'eye_green': '#7ad04a', 'leather': '#5a3a24', 'trim_gold': '#d8b04a',
    'trim_white': '#f6f2e8', 'paint_yellow': '#f2c94c', 'paint_blue': '#4f86c6', 'paint_red': '#d2493c', 'paint_pink': '#e98ab0', 'paint_green': '#5fae6a', 'paint_teal': '#3fb5b0', 'paint_orange': '#ee8f3c', 'paint_purple': '#8e6cc2', 'paint_cream': '#f1e4c4',
    'hull_red': '#c8382c', 'hull_blue': '#2d5fa8', 'hull_wood': '#9a6a40', 'hull_teal': '#2aa3a0', 'hull_yellow': '#e8b83a', 'sail': '#f6f2e6', 'net': '#c9b88a', 'fish': '#a8c4d6', 'lighthouse_red': '#d03c34', 'roof_red': '#c8453a', 'roof_blue': '#3e6fb4',
    'robe_white': '#e8ecf4', 'steel_dark': '#5a606c', 'black_armor': '#16161c', 'cloth_blue': '#3050a0',
}


def hexc(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i + 2], 16) / 255.0 for i in (0, 2, 4)) + (1.0,)


def rnd(a, b):
    return _rand.uniform(a, b)


class Builder:
    def __init__(self, name):
        self.name = name
        self.bm = bmesh.new()
        self.mats = []

    def mi(self, m):
        if m not in self.mats:
            self.mats.append(m)
        return self.mats.index(m)

    def face(self, pts, mat, smooth=False):
        vs = [self.bm.verts.new(Vector(p)) for p in pts]
        try:
            f = self.bm.faces.new(vs)
        except ValueError:
            return None
        f.material_index = self.mi(mat)
        f.smooth = smooth
        return f

    def quad(self, a, b, c, d, mat):
        return self.face([a, b, c, d], mat)

    def tri(self, a, b, c, mat):
        return self.face([a, b, c], mat)

    def box(self, c, s, mat, rz=0, rx=0, ry=0, taper=1.0, mats_by_face=None):
        hx, hy, hz = s[0] / 2, s[1] / 2, s[2] / 2
        R = Euler((math.radians(rx), math.radians(ry), math.radians(rz)), 'XYZ').to_matrix()
        C = Vector(c)

        def P(x, y, z):
            return C + R @ Vector((x, y, z))
        t = taper
        v = {
            (-1, -1, -1): P(-hx, -hy, -hz), (1, -1, -1): P(hx, -hy, -hz), (1, 1, -1): P(hx, hy, -hz), (-1, 1, -1): P(-hx, hy, -hz),
            (-1, -1, 1): P(-hx * t, -hy * t, hz), (1, -1, 1): P(hx * t, -hy * t, hz), (1, 1, 1): P(hx * t, hy * t, hz), (-1, 1, 1): P(-hx * t, hy * t, hz)}
        mm = mats_by_face or {}

        def F(keys, m):
            self.face([v[k] for k in keys], m)
        F([(-1, -1, -1), (-1, 1, -1), (1, 1, -1), (1, -1, -1)], mm.get('bottom', mat))
        F([(-1, -1, 1), (1, -1, 1), (1, 1, 1), (-1, 1, 1)], mm.get('top', mat))
        F([(-1, -1, -1), (1, -1, -1), (1, -1, 1), (-1, -1, 1)], mm.get('front', mat))
        F([(1, 1, -1), (-1, 1, -1), (-1, 1, 1), (1, 1, 1)], mm.get('back', mat))
        F([(-1, 1, -1), (-1, -1, -1), (-1, -1, 1), (-1, 1, 1)], mm.get('left', mat))
        F([(1, -1, -1), (1, 1, -1), (1, 1, 1), (1, -1, 1)], mm.get('right', mat))

    def beam(self, a, b, w, mat, w2=None):
        a = Vector(a)
        b = Vector(b)
        d = b - a
        if d.length < 1e-5:
            return
        z = d.normalized()
        up = Vector((0, 0, 1)) if abs(z.z) < .95 else Vector((1, 0, 0))
        x = up.cross(z).normalized()
        y = z.cross(x)
        w2 = w if w2 is None else w2

        def corners(p, ww):
            return [p + x * ww / 2 + y * ww / 2, p - x * ww / 2 + y * ww / 2, p - x * ww / 2 - y * ww / 2, p + x * ww / 2 - y * ww / 2]
        A = corners(a, w)
        B = corners(b, w2)
        for i in range(4):
            j = (i + 1) % 4
            self.face([A[i], A[j], B[j], B[i]], mat)
        self.face(A[::-1], mat)
        self.face(B, mat)

    def cyl(self, c, r, h, mat, sides=16, r2=None, axis='z', cap=True, smooth=True):
        r2 = r if r2 is None else r2
        c = Vector(c)

        def P(ang, rr, hh):
            x, y = math.cos(ang) * rr, math.sin(ang) * rr
            if axis == 'z':
                return c + Vector((x, y, hh))
            if axis == 'x':
                return c + Vector((hh, x, y))
            return c + Vector((x, hh, y))
        top_c = P(0, 0, h)
        for i in range(sides):
            a0 = i * 2 * math.pi / sides
            a1 = (i + 1) * 2 * math.pi / sides
            self.face([P(a0, r, 0), P(a1, r, 0), P(a1, r2, h), P(a0, r2, h)], mat, smooth)
            if cap:
                self.face([P(a1, r2, h), P(a0, r2, h), top_c], mat)
                self.face([P(a0, r, 0), P(a1, r, 0), c], mat)

    def cone(self, c, r, h, mat, sides=16, flare=0.0):
        c = Vector(c)
        for i in range(sides):
            a0 = i * 2 * math.pi / sides
            a1 = (i + 1) * 2 * math.pi / sides
            p0 = c + Vector((math.cos(a0) * r, math.sin(a0) * r, 0))
            p1 = c + Vector((math.cos(a1) * r, math.sin(a1) * r, 0))
            k = .42 + flare
            m0 = c + Vector((math.cos(a0) * r * k, math.sin(a0) * r * k, h * .5))
            m1 = c + Vector((math.cos(a1) * r * k, math.sin(a1) * r * k, h * .5))
            tip = c + Vector((0, 0, h))
            self.face([p0, p1, m1, m0], mat, True)
            self.face([m0, m1, tip], mat, True)

    def sphere(self, c, r, mat, rings=8, sides=12, sq=1.0):
        c = Vector(c)
        pts = []
        for i in range(rings + 1):
            th = math.pi * i / rings
            row = []
            for j in range(sides):
                ph = 2 * math.pi * j / sides
                row.append(c + Vector((math.sin(th) * math.cos(ph) * r, math.sin(th) * math.sin(ph) * r, math.cos(th) * r * sq)))
            pts.append(row)
        for i in range(rings):
            for j in range(sides):
                k = (j + 1) % sides
                self.face([pts[i][j], pts[i][k], pts[i + 1][k], pts[i + 1][j]], mat, True)

    def gable_roof(self, c, L, D, rise, mat, over=0.5, overL=0.35, thick=.14, kick=.35, under='wood_dark', segs=7):
        """Ridge along X. Eaves at +/-Y with a gentle upward kick like the reference roofs."""
        cx, cy, cz = c
        half = D / 2 + over
        Lx = L / 2 + overL
        base_rise = rise * half / (D / 2)
        rows = []
        for i in range(segs + 1):
            t = i / segs
            y = half * (1 - t)
            z = base_rise * t
            # concave sweep: eaves lifted slightly, mid-slope slightly lowered
            z += kick * 0.35 * (1 - t) ** 3 - kick * .12 * math.sin(math.pi * t)
            rows.append((y, z))
        for side in (-1, 1):
            for i in range(segs):
                (y0, z0), (y1, z1) = rows[i], rows[i + 1]
                A = (cx - Lx, cy + side * y0, cz + z0)
                B = (cx + Lx, cy + side * y0, cz + z0)
                C = (cx + Lx, cy + side * y1, cz + z1)
                Dd = (cx - Lx, cy + side * y1, cz + z1)
                if side == 1:
                    self.quad(A, B, C, Dd, mat)
                else:
                    self.quad(B, A, Dd, C, mat)
                dz = -thick
                A2, B2, C2, D2 = [(p[0], p[1], p[2] + dz) for p in (A, B, C, Dd)]
                if side == 1:
                    self.quad(D2, C2, B2, A2, under)
                else:
                    self.quad(C2, D2, A2, B2, under)
            y0, z0 = rows[0]
            e0 = (cx - Lx, cy + side * y0, cz + z0)
            e1 = (cx + Lx, cy + side * y0, cz + z0)
            e0d = (e0[0], e0[1], e0[2] - thick)
            e1d = (e1[0], e1[1], e1[2] - thick)
            if side == 1:
                self.quad(e1, e0, e0d, e1d, under)
            else:
                self.quad(e0, e1, e1d, e0d, under)
        ridge_z = cz + rows[-1][1]
        self.box((cx, cy, ridge_z + .03), (L + 2 * overL + .1, .36, .16), under)
        # triangular gable end boards (wooden bargeboards)
        for sx in (-1, 1):
            x = cx + sx * Lx
            for side in (-1, 1):
                for i in range(segs):
                    (y0, z0), (y1, z1) = rows[i], rows[i + 1]
                    self.beam((x, cy + side * y0, cz + z0 - .05), (x, cy + side * y1, cz + z1 - .05), .14, under) if False else None
        return ridge_z

    def gable_infill(self, x, cy, base_z, D, ridge_z, mat):
        self.tri((x, cy - D / 2, base_z), (x, cy + D / 2, base_z), (x, cy, ridge_z), mat)

    def build(self, bevel=0.02, smooth_angle=None):
        bm = self.bm
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        me = bpy.data.meshes.new(self.name)
        uv = bm.loops.layers.uv.new('UVMap')
        for f in bm.faces:
            mname = self.mats[f.material_index]
            tile = TILE.get(mname, 2.0)
            n = f.normal
            ax = max(range(3), key=lambda i: abs(n[i]))
            for l in f.loops:
                p = l.vert.co
                if ax == 0:
                    u, v = p.y, p.z
                elif ax == 1:
                    u, v = p.x, p.z
                else:
                    u, v = p.x, p.y
                l[uv].uv = (u / tile, v / tile)
        bm.to_mesh(me)
        bm.free()
        obj = bpy.data.objects.new(self.name, me)
        bpy.context.scene.collection.objects.link(obj)
        for m in self.mats:
            mat = bpy.data.materials.get(m)
            if mat is None:
                mat = bpy.data.materials.new(m)
                mat.diffuse_color = hexc(PAL.get(m, '#c0c0c0'))
            obj.data.materials.append(mat)
        if bevel > 0:
            bpy.context.view_layer.objects.active = obj
            md = obj.modifiers.new('bev', 'BEVEL')
            md.width = bevel
            md.segments = 1
            md.limit_method = 'ANGLE'
            md.angle_limit = math.radians(35)
            try:
                bpy.ops.object.modifier_apply(modifier='bev')
            except Exception as e:
                print('bevel skipped', e)
        return obj


def clear():
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for m in list(bpy.data.meshes):
        bpy.data.meshes.remove(m)


def export(objs, name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    path = OUT + name + '.fbx'
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_unit_scale=True, global_scale=1.0,
                             axis_forward='-Z', axis_up='Y', bake_space_transform=True, mesh_smooth_type='FACE',
                             add_leaf_bones=False, path_mode='STRIP')
    return path


def preview(path, objs, loc=(8, -12, 6), target=(0, 0, 2.5), size=(900, 700), lens=40):
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'MATERIAL'
    scene.display.shading.show_cavity = False
    scene.render.resolution_x, scene.render.resolution_y = size
    scene.render.image_settings.file_format = 'PNG'
    scene.world = scene.world or bpy.data.worlds.new('w')
    scene.world.color = (.55, .7, .9)
    for o in [o for o in bpy.data.objects if o.type == 'CAMERA']:
        bpy.data.objects.remove(o, do_unlink=True)
    cam_d = bpy.data.cameras.new('cam')
    cam_d.lens = lens
    cam = bpy.data.objects.new('cam', cam_d)
    bpy.context.scene.collection.objects.link(cam)
    cam.location = loc
    d = Vector(target) - Vector(loc)
    cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    scene.camera = cam
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
