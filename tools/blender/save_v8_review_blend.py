"""Keep an editable Blender review kit of the exported original V8 meshes."""
import bpy,glob,os
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for i,path in enumerate(sorted(glob.glob('D:/GAME/AureliaUnity/Assets/Models/ReferenceV8/*.fbx'))):
    before=set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=path)
    objects=set(bpy.data.objects)-before
    col=bpy.data.collections.new(os.path.basename(path)[:-4])
    bpy.context.scene.collection.children.link(col)
    for obj in objects:
        for parent in list(obj.users_collection):parent.objects.unlink(obj)
        col.objects.link(obj)
        if obj.parent not in objects:
            obj.location.x+=(i%4)*20
            obj.location.y+=(i//4)*22
    print('SAVED_REVIEW_COLLECTION',col.name)
bpy.ops.wm.save_as_mainfile(filepath='D:/GAME/artifacts/world-v8/assets/ReferenceV8_EditableKit.blend')
