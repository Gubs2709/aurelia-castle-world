import bpy, math, sys, os
from mathutils import Vector

PREV = "D:/GAME/artifacts/living-world/knight/"
os.makedirs(PREV, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath="D:/GAME/tools/blender/Knight2.blend")
scene = bpy.context.scene
arm = bpy.data.objects['KnightRig']
for eng in ('BLENDER_EEVEE', 'BLENDER_EEVEE_NEXT', 'BLENDER_WORKBENCH'):
    try:
        scene.render.engine = eng
        break
    except Exception:
        continue
print('ENGINE', scene.render.engine)
scene.render.resolution_x, scene.render.resolution_y = 560, 760
scene.render.image_settings.file_format = 'PNG'
scene.render.film_transparent = False
try:
    scene.view_settings.view_transform = 'Standard'
    scene.view_settings.look = 'None'
except Exception as e:
    print('vt', e)
# world
w = bpy.data.worlds.new('w')
scene.world = w
w.use_nodes = True
bg = w.node_tree.nodes['Background']
bg.inputs['Color'].default_value = (.55, .72, .95, 1)
bg.inputs['Strength'].default_value = .55
# lights
def light(name, kind, loc, energy, rot):
    d = bpy.data.lights.new(name, kind)
    d.energy = energy
    o = bpy.data.objects.new(name, d)
    scene.collection.objects.link(o)
    o.location = loc
    o.rotation_euler = rot
    return o
light('sun', 'SUN', (0, 0, 5), 2.8, (math.radians(50), 0, math.radians(-35)))
light('fill', 'SUN', (0, 0, 5), .7, (math.radians(70), 0, math.radians(150)))
# floor
bpy.ops.mesh.primitive_plane_add(size=30, location=(0, 0, 0))
fl = bpy.context.active_object
m = bpy.data.materials.new('floor')
m.use_nodes = True
m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (.30, .5, .22, 1)
fl.data.materials.append(m)
cam_d = bpy.data.cameras.new('cam')
cam_d.lens = 50
cam = bpy.data.objects.new('cam', cam_d)
scene.collection.objects.link(cam)
scene.camera = cam


def aim(loc, target):
    cam.location = loc
    cam.rotation_euler = (Vector(target) - Vector(loc)).to_track_quat('-Z', 'Y').to_euler()


def render(path):
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)


bpy.ops.mesh.primitive_cube_add(size=1)
bench = bpy.context.active_object
bench.scale = (1.2, .5, .04)
bench.location = (0, .17, .37)
bm_ = bpy.data.materials.new('bench'); bm_.use_nodes = True
bm_.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (.45, .3, .18, 1)
bench.data.materials.append(bm_)
for sx in (-1, 1):
    bpy.ops.mesh.primitive_cube_add(size=1)
    l = bpy.context.active_object
    l.scale = (.06, .4, .35); l.location = (sx * .5, .17, .175); l.data.materials.append(bm_)
    l.hide_render = True; l.name = 'benchleg'
bench.hide_render = True


def set_bench(on):
    for o in bpy.data.objects:
        if o.name.startswith('Cube') or o.name == 'benchleg' or o is bench:
            o.hide_render = not on


def pose_at(action, frame):
    set_bench(action.startswith('Sit'))
    arm.animation_data.action = bpy.data.actions[action]
    scene.frame_set(frame)
    bpy.context.view_layer.update()


# turnaround at idle
pose_at('Idle', 0)
views = {'front': (0, -3.3, 1.05), 'q34': (-2.4, -2.5, 1.2), 'side': (-3.3, 0, 1.05), 'back': (0, 3.3, 1.1), 'closeup': (-.28, -.9, 1.76)}
for k, loc in views.items():
    aim(loc, (0, 0, 1.75 if k == 'closeup' else 0.95))
    cam_d.lens = 70 if k == 'closeup' else 50
    render(PREV + 'turn_%s.png' % k)
cam_d.lens = 50
# animation strips: frames chosen per action
strips = {'Walk': [0, 4, 7, 11, 14, 18, 21, 25], 'Run': [0, 2, 4, 6, 9, 11, 13, 15], 'Jump': [0, 6, 12, 18, 22, 32, 40, 46], 'SitDown': [0, 6, 12, 18, 22, 26, 30, 32], 'SitIdle': [0, 20, 45, 70], 'Idle': [0, 18, 36, 54]}
for act, frs in strips.items():
    for i, f in enumerate(frs):
        pose_at(act, f)
        aim((-2.6, -3.4, 1.1), (0, 0, .9))
        render(PREV + 'anim_%s_%02d.png' % (act, i))
print('RENDERED')
