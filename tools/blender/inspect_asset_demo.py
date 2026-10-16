import bpy,os,math,json
from mathutils import Vector
OUT='D:/GAME/artifacts/asset-demo-review';os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
files=sorted(__import__('glob').glob('D:/GAME/asset_demo/*.glb'));report=[]
for i,p in enumerate(files):
    before=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=p);objs=list(set(bpy.data.objects)-before);root=bpy.data.objects.new(os.path.basename(p),None);bpy.context.collection.objects.link(root)
    for o in objs:
        if o.parent not in objs:o.parent=root
    # Source demos were authored Z-up but exported without glTF's Y-up conversion.
    root.rotation_euler.x=math.pi/2;bpy.context.view_layer.update()
    coords=[o.matrix_world@Vector(c) for o in objs if o.type=='MESH' for c in o.bound_box]
    lo=Vector(tuple(min(c[k] for c in coords) for k in range(3)));hi=Vector(tuple(max(c[k] for c in coords) for k in range(3)));size=hi-lo
    scale=2.6/max(size);root.scale=(scale,)*3;root.location=((i%5)*4-lo.x*scale-size.x*scale/2,(i//5)*4-lo.y*scale-size.y*scale/2,-lo.z*scale)
    report.append({'file':os.path.basename(p),'bounds':list(size),'animations':0,'rigged':False})
    # Labels are Blender text geometry, not painted onto the source image.
    d=bpy.data.curves.new('Label','FONT');d.body=os.path.basename(p).split('_',1)[1].replace('.glb','');d.size=.18;d.align_x='CENTER';o=bpy.data.objects.new('Label',d);bpy.context.collection.objects.link(o);o.location=((i%5)*4,(i//5)*4-1.8,.02)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.world.color=(.2,.2,.2);scene.render.resolution_x=1800;scene.render.resolution_y=900;scene.render.resolution_percentage=100
bpy.ops.mesh.primitive_plane_add(size=200);floor=bpy.context.object;floor.location.z=-.03
for loc,power,size in [((2,-5,12),1800,12),((14,8,10),2200,10)]:
    d=bpy.data.lights.new('Softbox','AREA');d.energy=power;d.size=size;o=bpy.data.objects.new('Softbox',d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((8,2,0))-o.location).to_track_quat('-Z','Y').to_euler()
d=bpy.data.cameras.new('Camera');o=bpy.data.objects.new('Camera',d);scene.collection.objects.link(o);o.location=(8,-14,16);o.rotation_euler=(Vector((8,2,.5))-o.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=21;scene.camera=o;scene.render.filepath=OUT+'/models.png';bpy.ops.render.render(write_still=True)
json.dump(report,open(OUT+'/review.json','w'),indent=2)
