"""Original 3D reconstruction study from the user's Zed sprite references.
Writes only artifacts/zed-knight-review, never replaces the game character.
"""
import bpy, math, os, json, random
from mathutils import Vector
random.seed(15)
OUT='D:/GAME/artifacts/zed-knight-review'
os.makedirs(OUT,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
M={}; parts=[]
def mat(n,h,metal=0,rough=.5,emission=0):
    c=tuple((int(h[i:i+2],16)/255)**2.2 for i in (0,2,4))+(1,)
    m=bpy.data.materials.new(n);m.diffuse_color=c;m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=c
    p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
    if emission:p.inputs['Emission Color'].default_value=c;p.inputs['Emission Strength'].default_value=emission
    M[n]=m;return m
mat('Ivory enamel','c7c9c5',.38,.37);mat('Silver edge','a6b4bc',.72,.29)
mat('Warm white cloth','d4d3c9');mat('Coat shadow','858e93');mat('Charcoal fabric','252c35')
mat('Black leather','24262d',.08,.4);mat('Dark steel','48535d',.65,.32)
mat('Hair silver','b7bdc5',.12,.48);mat('Hair light','e0e0da',.12,.45)
mat('Hair shadow','737d8b',.15,.47);mat('Skin','d7b9ac',0,.58)
mat('Lips','966c65');mat('Eyes','354a59',.05,.25);mat('Eye whites','e8e4dc')
mat('Cyan crystal','73dbea',.25,.25,2.5);mat('Blade core','c8f4fb',.45,.24,1.8)
mat('Blade steel','93bbc9',.72,.25);mat('Belt leather','4b4545');mat('Buckles','8e989d',.75,.32)
def mesh(n,v,f,ma,bone='Chest',smooth=True):
    me=bpy.data.meshes.new(n);me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);bpy.context.collection.objects.link(o);o.data.materials.append(M[ma]);o['bone']=bone
    for p in me.polygons:p.use_smooth=smooth
    parts.append(o);return o
def uv(n,c,s,ma,b='Chest',seg=24,rings=16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,location=c);o=bpy.context.object;o.name=n;o.scale=s
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(M[ma]);o['bone']=b;parts.append(o)
    for p in o.data.polygons:p.use_smooth=True
    return o
def loft(n,rows,ma,b='Chest',sides=20):
    # rows: x,y,z, width radius, depth radius
    v=[];f=[]
    for x,y,z,rx,ry in rows:
        for j in range(sides):
            a=j*2*math.pi/sides;v.append((x+rx*math.sin(a),y-ry*math.cos(a),z))
    for i in range(len(rows)-1):
        for j in range(sides):
            a=i*sides+j;k=i*sides+(j+1)%sides;f.append((a,k,k+sides,a+sides))
    f.extend([tuple(reversed(range(sides))),tuple((len(rows)-1)*sides+j for j in range(sides))])
    return mesh(n,v,f,ma,b)
def tube(n,points,radii,ma,b='Chest',sides=10):
    v=[];f=[]
    for i,p in enumerate(points):
        p=Vector(p);d=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])
        d.normalize();axis=d.cross(Vector((0,1,0)))
        if axis.length<.01:axis=d.cross(Vector((1,0,0)))
        axis.normalize();axis2=d.cross(axis).normalized()
        r=radii[i];rx,ry=(r,r) if isinstance(r,(int,float)) else r
        for k in range(sides):
            a=k*math.tau/sides;v.append(p+axis*math.cos(a)*rx+axis2*math.sin(a)*ry)
    for i in range(len(points)-1):
        for k in range(sides):a=i*sides+k;j=i*sides+(k+1)%sides;f.append((a,j,j+sides,a+sides))
    f.extend([tuple(reversed(range(sides))),tuple((len(points)-1)*sides+k for k in range(sides))])
    return mesh(n,v,f,ma,b)
def curve(n,points,r,ma,b='Chest'):
    return tube(n,points,[r]*len(points),ma,b,8)
def panel(n,pts,ma,b='Chest',thickness=.008):
    o=mesh(n,pts,[tuple(range(len(pts)))],ma,b,False)
    md=o.modifiers.new('Real cloth thickness','SOLIDIFY');md.thickness=thickness
    be=o.modifiers.new('Soft stitched edges','BEVEL');be.width=.004;be.segments=2
    return o
# Adult proportions, front = -Y. Total height ~1.94m.
loft('Fitted dark undercoat',[(0,0,1.01,.145,.095),(0,0,1.11,.15,.105),(0,0,1.25,.18,.115),(0,.012,1.45,.235,.112),(0,.015,1.51,.195,.09)],'Charcoal fabric')
loft('High ribbed collar',[(0,.006,1.48,.085,.078),(0,.008,1.61,.073,.074)],'Coat shadow')
for z in (1.51,1.535,1.56):
    curve('Collar seam',[(-.068,-.055,z),(0,-.074,z),(.068,-.055,z)],.002,'Silver edge')
loft('Neck',[(0,0,1.51,.055,.056),(0,0,1.65,.055,.058)],'Skin','Head')
# Anatomical face with a tapered jaw, nose/eye sockets are sculpted into geometry.
v=[];f=[];N=48
profile=[(1.619,.014,.041,-.012),(1.642,.043,.059,-.008),(1.675,.072,.071,0),(1.711,.083,.077,.003),(1.75,.088,.077,.008),(1.79,.087,.078,.015),(1.83,.075,.069,.022),(1.858,.045,.044,.024),(1.87,.005,.005,.024)]
for z,rx,ry,cy in profile:
    for j in range(N):
        a=math.tau*j/N;x=rx*math.sin(a);y=cy-ry*math.cos(a)
        front=max(0,math.cos(a))**12
        y-=front*.011*math.exp(-((z-1.716)/.025)**2)
        v.append((x,y,z))
for i in range(len(profile)-1):
    for j in range(N):a=i*N+j;k=i*N+(j+1)%N;f.append((a,k,k+N,a+N))
head=mesh('Sculpted face and jaw',v,f,'Skin','Head');sub=head.modifiers.new('Face smooth','SUBSURF');sub.levels=1
for side in (-1,1):
    uv('Ear',(side*.087,.002,1.737),(.016,.012,.029),'Skin','Head')
    # thin almond-shaped geometry, restrained anime proportions
    pts=[(side*.018,-.071,1.754),(side*.037,-.080,1.760),(side*.060,-.067,1.754),(side*.040,-.079,1.744)]
    eye=panel('Almond eye',pts,'Eye whites','Head',.001)
    uv('Slate iris',(side*.039,-.0805,1.751),(.007,.002,.007),'Eyes','Head',16,8)
    curve('Upper eyelid',[pts[0],pts[1],pts[2]],.0016,'Hair shadow','Head')
    curve('Silver eyebrow',[(side*.018,-.074,1.771),(side*.040,-.080,1.777),(side*.063,-.065,1.772)],.003,'Hair shadow','Head')
panel('Nose bridge',[(-.009,-.073,1.749),(.009,-.073,1.749),(.010,-.092,1.709),(0,-.101,1.706),(-.010,-.092,1.709)],'Skin','Head')
curve('Quiet mouth',[(-.018,-.072,1.685),(0,-.078,1.683),(.018,-.072,1.685)],.0014,'Lips','Head')
# Swept silver hair, curved solid locks with tapered cross sections.
uv('Hair undercut',(0,.029,1.804),(.091,.082,.077),'Hair shadow','Head')
for i in range(24):
    a=i*math.tau/24
    x=math.sin(a);y=math.cos(a)
    start=(x*.035,.025+y*.025,1.876)
    mid=(x*.083+.012,.026+y*.069,1.851)
    end=(x*.101+.020,.025+y*.084,1.797-random.uniform(0,.04))
    tube('Layered silver hair lock',[start,mid,end],[(.012,.015),(.015,.012),(.001,.001)],'Hair light' if i%3==0 else 'Hair silver','Head',8)
for i in range(10):
    x=-.075+i*.016
    tip=1.764+random.uniform(0,.043)+abs(x)*.14
    tube('Swept fringe',[(-.048+i*.007,.008,1.867),(x-.024,-.030,1.887-abs(x)*.19),(x+.004,-.078,1.830),(x+.029,-.085,tip)],[(.013,.010),(.017,.012),(.012,.008),(.0005,.0005)],'Hair light' if i%3==0 else 'Hair silver','Head',10)
for s in (-1,1):
    tube('Temple wisps',[(s*.083,-.014,1.824),(s*.096,-.046,1.775),(s*.085,-.040,1.715)],[(.017,.009),(.014,.008),(.001,.001)],'Hair silver','Head')
# Layered fitted breastplate and sculpted white lapels.
loft('Ivory cuirass',[(0,-.006,1.13,.139,.101),(0,-.006,1.25,.171,.124),(0,.002,1.40,.205,.128),(0,.010,1.47,.192,.105)],'Ivory enamel')
for s in (-1,1):
    panel('Swept coat lapel',[(s*.045,-.096,1.53),(s*.185,-.075,1.49),(s*.172,-.115,1.37),(s*.063,-.145,1.30),(s*.085,-.132,1.43)],'Warm white cloth')
    curve('Lapel piping',[(s*.045,-.101,1.53),(s*.085,-.138,1.43),(s*.063,-.151,1.30)],.003,'Silver edge')
    panel('Chest armour inset',[(s*.035,-.135,1.38),(s*.14,-.139,1.34),(s*.13,-.132,1.27),(s*.04,-.138,1.25)],'Coat shadow')
    curve('Chest silver edge',[(s*.035,-.140,1.38),(s*.14,-.144,1.34),(s*.13,-.137,1.27)],.003,'Silver edge')
uv('Cyan breast clasp',(0,-.153,1.355),(.018,.009,.028),'Cyan crystal')
# Offset shoulder cape panels and arms in relaxed lowered pose.
for s in (-1,1):
    side='L' if s>0 else 'R';ua='UpperArm.'+side;fa='Forearm.'+side;hand='Hand.'+side
    tube('Upper sleeve '+side,[(s*.205,0,1.45),(s*.27,.005,1.31),(s*.30,0,1.17)],[.081,.067,.056],'Coat shadow',ua,16)
    tube('Forearm undersleeve '+side,[(s*.30,0,1.17),(s*.325,-.015,1.06),(s*.34,-.035,.94)],[.055,.052,.040],'Charcoal fabric',fa,16)
    # Ridged pauldrons: shield form, not spheres.
    for layer in range(3):
        z=1.485-layer*.042;w=.105-layer*.009
        panel('Overlapping shoulder plate '+side,[(s*.155,-.075,z+.022),(s*(.23+w),-.065,z-.025),(s*(.245+w),.025,z-.04),(s*.19,.086,z+.018),(s*.165,.01,z+.065)],'Ivory enamel',ua,.018)
        curve('Pauldron edge '+side,[(s*.155,-.080,z+.022),(s*(.23+w),-.070,z-.025),(s*(.245+w),.025,z-.04)],.004,'Silver edge',ua)
    tube('Pointed vambrace '+side,[(s*.306,-.016,1.18),(s*.326,-.028,1.105),(s*.34,-.043,.985)],[.063,.058,.045],'Ivory enamel',fa,12)
    curve('Vambrace crest '+side,[(s*.306,-.080,1.18),(s*.326,-.088,1.105),(s*.34,-.087,.985)],.006,'Silver edge',fa)
    uv('Leather glove '+side,(s*.345,-.038,.91),(.044,.036,.068),'Black leather',hand)
    for j in range(4):
        x=s*(.318+j*.015)
        tube('Glove fingers '+side,[(x,-.058,.905),(x,-.063,.864),(x,-.048,.85)],[.009,.008,.006],'Black leather',hand,8)
    uv('Glove thumb '+side,(s*.303,-.052,.914),(.014,.018,.029),'Black leather',hand,12,8)
# Cross body harness and buckled waist belts.
tube('Shoulder baldric',[(-.16,-.094,1.48),(-.09,-.151,1.34),(.015,-.143,1.18),(.11,-.110,1.06)],[(.018,.009)]*4,'Belt leather','Chest',8)
loft('Leather waist belt',[(0,0,1.065,.151,.113),(0,0,1.116,.153,.116)],'Belt leather','Hips')
panel('Belt buckle',[(-.026,-.122,1.066),(.026,-.122,1.066),(.026,-.122,1.116),(-.026,-.122,1.116)],'Buckles','Hips')
panel('Buckle inset',[(-.017,-.126,1.076),(.017,-.126,1.076),(.017,-.126,1.106),(-.017,-.126,1.106)],'Black leather','Hips')
# Long split coat: curved surfaces with shaped hems and folds, all genuinely 3D.
for s in (-1,1):
    side='L' if s>0 else 'R'
    rows=13;cols=10;v=[];f=[]
    for i in range(rows):
        u=i/(rows-1)
        for j in range(cols):
            t=j/(cols-1);a=-1.0+t*2.65
            r=.151+u*.155
            x=s*(math.cos(a)*r+.035*u*u)
            y=math.sin(a)*(r*.74)+.08*u*u+.008*math.sin(t*math.tau*3+u*2)*u
            z=1.07-u*(.51+.12*math.sin(t*math.pi))+.025*math.sin(t*math.pi*3)*u*u
            v.append((x,y,z))
    for i in range(rows-1):
        for j in range(cols-1):k=i*cols+j;f.append((k,k+1,k+1+cols,k+cols))
    o=mesh('Flowing split coat '+side,v,f,'Warm white cloth','Coat.'+side)
    o.data.materials.append(M['Coat shadow']);sol=o.modifiers.new('Lined tailored cloth','SOLIDIFY');sol.thickness=.008;sol.material_offset=1
    sub=o.modifiers.new('Cloth folds','SUBSURF');sub.levels=1
    curve('Coat hem binding '+side,[v[(rows-1)*cols+j] for j in range(cols)],.004,'Silver edge','Coat.'+side)
    for j in (2,6):curve('Coat stitched seam '+side,[v[i*cols+j] for i in range(rows)],.0018,'Coat shadow','Coat.'+side)
# Short shoulder mantle trailing behind, matching the sprite's scarf silhouette.
for s in (-1,1):
    v=[];f=[];rows=12;cols=9
    for i in range(rows):
        u=i/(rows-1)
        for j in range(cols):
            t=j/(cols-1);v.append((s*(.015+t*(.19+.12*u)),.07+.20*u+.025*math.sin(t*math.tau*2+u*4)*u,1.50-.69*u+.08*t*u))
    for i in range(rows-1):
        for j in range(cols-1):k=i*cols+j;f.append((k,k+cols,k+cols+1,k+1))
    o=mesh('Layered rear mantle',v,f,'Coat shadow','Cape');md=o.modifiers.new('Mantle thickness','SOLIDIFY');md.thickness=.006
    md=o.modifiers.new('Mantle drape','SUBSURF');md.levels=1
    curve('Mantle silver edge',[v[(rows-1)*cols+j] for j in range(cols)],.003,'Silver edge','Cape')
# Legs, articulated tall boots and toe armour.
for s in (-1,1):
    side='L' if s>0 else 'R';th='Thigh.'+side;sh='Shin.'+side;foot='Foot.'+side
    tube('Tailored trousers '+side,[(s*.09,0,1.035),(s*.105,0,.82),(s*.11,-.01,.57)],[.092,.085,.062],'Charcoal fabric',th,18)
    for j in range(3):curve('Trouser crease',[(s*.075,-.077,.91-j*.08),(s*.13,-.066,.88-j*.08)],.003,'Coat shadow',th)
    loft('Tall leather boot '+side,[(s*.11,0,.12,.067,.061),(s*.11,.007,.26,.057,.058),(s*.11,.008,.43,.069,.062),(s*.11,-.008,.57,.071,.064)],'Black leather',sh)
    panel('Angular knee guard '+side,[(s*.11,-.082,.63),(s*.18,-.068,.565),(s*.11,-.102,.497),(s*.045,-.068,.565)],'Dark steel',sh,.015)
    panel('Shin blade '+side,[(s*.075,-.063,.47),(s*.15,-.063,.47),(s*.148,-.073,.21),(s*.11,-.092,.16),(s*.073,-.073,.21)],'Dark steel',sh,.012)
    curve('Shin ridge '+side,[(s*.11,-.079,.47),(s*.11,-.096,.30),(s*.11,-.101,.17)],.005,'Silver edge',sh)
    loft('Sculpted boot foot '+side,[(s*.11,-.063,.035,.073,.129),(s*.11,-.063,.077,.074,.13),(s*.11,-.025,.145,.059,.081)],'Black leather',foot)
    panel('Steel boot toe '+side,[(s*.05,-.185,.07),(s*.17,-.185,.07),(s*.17,-.11,.103),(s*.05,-.11,.103)],'Dark steel',foot,.012)
# Oversized faceted crystalline greatsword, held by right hand, point near ground.
# Its local centreline runs down Z, actual thickness and bevel facets visible in profile.
sx=-.35;sy=-.074
sword_start=len(parts)
blade=[(sx,sy,.18),(sx-.061,sy,.36),(sx-.069,sy,.98),(sx-.047,sy,1.09),(sx+.047,sy,1.09),(sx+.069,sy,.98),(sx+.061,sy,.36)]
v=[(x,y-.014,z) for x,y,z in blade]+[(x,y+.014,z) for x,y,z in blade]+[(sx,sy-.033,.58),(sx,sy+.033,.58)]
f=[]
for j in range(7):f.extend([(14,j,(j+1)%7),(15,7+(j+1)%7,7+j),(j,7+j,7+(j+1)%7,(j+1)%7)])
mesh('Faceted frost greatsword',v,f,'Blade steel','Hand.R',False)
panel('Luminous blade fuller',[(sx,sy-.034,.22),(sx-.016,sy-.034,.39),(sx-.016,sy-.034,1.07),(sx+.016,sy-.034,1.07),(sx+.016,sy-.034,.39)],'Blade core','Hand.R',.003)
for s in (-1,1):
    tube('Winged crossguard',[(sx,sy,1.08),(sx+s*.087,sy,1.105),(sx+s*.13,sy,1.145)],[(.022,.023),(.017,.023),(.003,.007)],'Dark steel','Hand.R')
    curve('Cyan crossguard inlay',[(sx,sy-.026,1.085),(sx+s*.087,sy-.022,1.11),(sx+s*.123,sy-.009,1.143)],.004,'Cyan crystal','Hand.R')
tube('Sword leather grip',[(sx,sy,1.105),(sx,sy,1.282)],[.022,.020],'Black leather','Hand.R',12)
for i in range(8):
    z=1.12+i*.019;curve('Grip binding',[(sx-.018,sy-.014,z),(sx+.018,sy-.014,z+.01)],.002,'Silver edge','Hand.R')
uv('Pommel crystal',(sx,sy,1.296),(.025,.021,.033),'Cyan crystal','Hand.R')
for o in parts[sword_start:]:
    # Align the actual hilt with the hand and keep the point clear of the ground.
    for v in o.data.vertices:
        world_z=v.co.z+o.location.z
        v.co.z=.80+(world_z-1.08)*.82-o.location.z
# Armature: rigid plate skinning with independent cloth bones and articulated limbs.
ad=bpy.data.armatures.new('Zed reconstruction skeleton');rig=bpy.data.objects.new('Zed Knight Rig',ad);bpy.context.collection.objects.link(rig)
bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
def bone(n,h,t,parent=None):
    b=ad.edit_bones.new(n);b.head=h;b.tail=t
    if parent:b.parent=ad.edit_bones[parent]
bone('Root',(0,0,0),(0,0,.2));bone('Hips',(0,0,1.02),(0,0,1.15),'Root');bone('Chest',(0,0,1.15),(0,0,1.50),'Hips');bone('Head',(0,0,1.53),(0,0,1.82),'Chest');bone('Cape',(0,.07,1.49),(0,.2,.9),'Chest')
for s in (-1,1):
    side='L' if s>0 else 'R'
    bone('UpperArm.'+side,(s*.21,0,1.45),(s*.30,0,1.17),'Chest');bone('Forearm.'+side,(s*.30,0,1.17),(s*.34,-.035,.96),'UpperArm.'+side);bone('Hand.'+side,(s*.34,-.035,.96),(s*.345,-.04,.87),'Forearm.'+side)
    bone('Thigh.'+side,(s*.09,0,1.02),(s*.11,0,.57),'Hips');bone('Shin.'+side,(s*.11,0,.57),(s*.11,0,.14),'Thigh.'+side);bone('Foot.'+side,(s*.11,0,.14),(s*.11,-.17,.07),'Shin.'+side);bone('Coat.'+side,(s*.15,0,1.07),(s*.26,.15,.56),'Hips')
bpy.ops.object.mode_set(mode='OBJECT')
for o in parts:
    vg=o.vertex_groups.new(name=o['bone']);vg.add(list(range(len(o.data.vertices))),1,'REPLACE');mod=o.modifiers.new('Knight armature','ARMATURE');mod.object=rig;o.parent=rig
# Preview-neutral idle pose, understated asymmetry.
rig.animation_data_create()
def reset():
    for b in rig.pose.bones:b.rotation_mode='XYZ';b.rotation_euler=(0,0,0);b.location=(0,0,0)
def key(n,f,xyz):
    b=rig.pose.bones[n];b.rotation_euler=tuple(math.radians(x) for x in xyz);b.keyframe_insert('rotation_euler',frame=f)
action=bpy.data.actions.new('Idle');action.use_fake_user=True;rig.animation_data.action=action
for f in (1,31,61):
    key('Chest',f,(0,0,1 if f==31 else -1));key('Head',f,(1,0,-2));key('Cape',f,(2 if f==31 else 0,0,0))
    key('Forearm.R',f,(-5,0,0));key('UpperArm.R',f,(-8,0,0));key('UpperArm.L',f,(3,0,-3))
idle=action
reset();action=bpy.data.actions.new('Run');action.use_fake_user=True;rig.animation_data.action=action
for f in range(1,26):
    a=math.tau*(f-1)/24
    key('Chest',f,(10,0,math.sin(a)*3))
    for s in (-1,1):
        side='L' if s>0 else 'R';p=a+(math.pi if s<0 else 0)
        key('Thigh.'+side,f,(math.sin(p)*38,0,0));key('Shin.'+side,f,(-max(0,math.cos(p))*62,0,0));key('UpperArm.'+side,f,(-math.sin(p)*24,0,0));key('Forearm.'+side,f,(-30,0,0));key('Coat.'+side,f,(-15+math.sin(p)*10,0,s*5))
    key('Cape',f,(-18+math.sin(a)*5,0,0))
reset();rig.animation_data.action=idle;scene=bpy.context.scene;scene.frame_set(1);scene.frame_end=61
# Preserve reference images packed in the native project without using flat images as geometry.
for p in ('D:/GAME/Zed-run-trimmed.png','D:/GAME/Zed-asm_greatsword_dragon_upper.png'):
    img=bpy.data.images.load(p);img.pack()
# Studio staging, camera and lighting. Blender renders only; no generated mockup.
scene.render.engine='CYCLES';scene.cycles.samples=48;scene.cycles.use_denoising=True
scene.render.resolution_x=1080;scene.render.resolution_y=1350;scene.render.resolution_percentage=100
scene.world.color=(.12,.12,.12)
scene.view_settings.view_transform='AgX'
mat('Studio slate','263440',.15,.55)
bpy.ops.mesh.primitive_plane_add(size=200);floor=bpy.context.object;floor.name='Studio ground';floor.data.materials.append(M['Studio slate'])
def light(n,loc,power,color,size,target=(0,0,1)):
    d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=size;o=bpy.data.objects.new(n,d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
light('Softbox key',(-3,-4,5),500,(.84,.91,1),4)
light('Warm fill',(3,-2,2.8),230,(1,.84,.70),3)
light('Cool rim',(1,3,4),650,(.45,.75,1),3)
cd=bpy.data.cameras.new('Review camera');cam=bpy.data.objects.new('Review camera',cd);scene.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO';cd.ortho_scale=2.35
def aim(p,target=(0,0,.97)):
    cam.location=p;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
aim((3,-5,2.7))
scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
bpy.ops.wm.save_as_mainfile(filepath=OUT+'/ZedKnight_Review.blend')
stats={'source_references':['Zed-run-trimmed.png','Zed-asm_greatsword_dragon_upper.png'],'method':'Original scripted Blender mesh reconstruction, no image-to-mesh provider','status':'First visual review; not installed in Unity; not retopology or deformation validated','mesh_objects':len(parts),'vertices':sum(len(o.data.vertices) for o in parts),'bones':len(ad.bones),'actions':['Idle','Run'],'outputs':['ZedKnight_Review.blend','three-quarter.png','back.png','front.png'],'limitations':['Hidden details inferred','Rigid plate weights and first-pass animation','Sword grip and finger articulation need refinement before gameplay','No authored attack animation yet']}
with open(OUT+'/PROVENANCE.json','w') as f:json.dump(stats,f,indent=2)
for name,p in [('three-quarter',(3,-5,2.7)),('back',(-3,5,2.5)),('front',(0,-6,2.3))]:
    aim(p);scene.render.filepath=OUT+'/'+name+'.png';bpy.ops.render.render(write_still=True)
print('ZED_REVIEW_COMPLETE',json.dumps(stats))
