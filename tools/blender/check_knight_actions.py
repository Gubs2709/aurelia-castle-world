import bpy,json
bpy.ops.wm.open_mainfile(filepath='D:/GAME/artifacts/knight-reference-v3/ReferenceKnight_v3.blend')
r=bpy.data.objects['Reference Knight Rig'];out=[]
for a in bpy.data.actions:
    r.animation_data.action=a
    values=[]
    for f in (1,7,15,23):
        bpy.context.scene.frame_set(f);bpy.context.view_layer.update();values.append(list(r.pose.bones['Shin.L'].rotation_euler))
    out.append({'action':a.name,'slots':[s.identifier for s in a.slots],'assigned':str(r.animation_data.action_slot),'knee':values})
json.dump(out,open('D:/GAME/artifacts/knight-reference-v3/action-diagnosis.json','w'),indent=2)
print(json.dumps(out))
