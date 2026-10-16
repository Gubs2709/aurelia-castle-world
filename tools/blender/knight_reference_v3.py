"""Second character study: image knight/download (2).jpg is the primary likeness.
No game files modified. Geometry authored locally in Blender, no paid service.
"""
import bpy, math, os, random, json
from mathutils import Vector
SOURCE='D:/GAME/tools/blender/zed_knight_review.py'
# Reuse only generic mesh/material helpers, not the previous character.
helpers=open(SOURCE,encoding='utf-8').read().split('# Adult proportions')[0]
helpers=helpers.replace("OUT='D:/GAME/artifacts/zed-knight-review'","OUT='D:/GAME/artifacts/knight-reference-v3'")
exec(compile(helpers,SOURCE,'exec'))
random.seed(81)
mat('Pale face','ead2c5',0,.72);mat('Blush','c99688',0,.76)
mat('Silvery hair','b6c0c7',.05,.56);mat('Pearl hair','e0e2da',.04,.57);mat('Hair occlusion','6e808b',.03,.62)
mat('Lash','35444c',0,.65);mat('Iris blue','45a5bc',.05,.4);mat('Iris inner','195066',.03,.4)
mat('Pupil','162d3c');mat('Eye light','f0f5ee',0,.5);mat('Catchlight','f0ffff',0,.35,.3)
mat('Ivory cloth','c9c7b9',0,.84);mat('Cloth shade','929994',0,.9);mat('Lining','626d72',0,.85)
mat('Porcelain plate','c8cfca',.4,.48);mat('Etched silver','889a9b',.6,.43);mat('Old gold','b7a17a',.66,.44)
mat('Teal plate shadow','5d777f',.45,.53);mat('Pants','293b43',0,.9)
mat('Sash','454f50',0,.9);mat('Belting','4c4640',0,.65);mat('Boot','303d43',.08,.54)
mat('Sword ice','9cdce5',.48,.3,.45);mat('Sword silver','bfd6d7',.66,.34)
mat('Sword light','c5f7fc',.22,.27,2);mat('Engraving','538c9e',.42,.45)
def smooth(points,steps=6):
    p=[Vector(x) for x in points];out=[]
    for i in range(len(p)-1):
        a=p[max(0,i-1)];b=p[i];c=p[i+1];d=p[min(len(p)-1,i+2)]
        for k in range(steps):
            t=k/steps;out.append(.5*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t))
    return out+[p[-1]]
def line(n,p,r,ma,b='Chest'):
    return curve(n,smooth(p),r,ma,b)
def soft(o,level=1):
    mod=o.modifiers.new('Sculpted smooth surface','SUBSURF');mod.levels=level;return o
def hair(n,p,width,ma):
    pts=smooth(p,5);v=[];f=[];sides=10
    for i,c in enumerate(pts):
        t=i/(len(pts)-1);tangent=pts[min(i+1,len(pts)-1)]-pts[max(0,i-1)];tangent.normalize()
        across=tangent.cross(Vector((0,1,0)))
        if across.length<.01:across=tangent.cross(Vector((1,0,0)))
        across.normalize();depth=tangent.cross(across).normalized()
        taper=(.23+.92*math.sin(math.pi*min(t*1.1,1))**.6)*max(.012,1-t)**.45
        for k in range(sides):
            a=math.tau*k/sides;v.append(c+across*(math.cos(a)*width*taper)+depth*(math.sin(a)*width*.32*taper))
    for i in range(len(pts)-1):
        for k in range(sides):a=i*sides+k;j=i*sides+(k+1)%sides;f.append((a,j,j+sides,a+sides))
    o=mesh(n,v,f,ma,'Head');soft(o);return o
def cloth(n,rows,ma,b='Chest',fold=.008,cols=16):
    # rows are (cx,cy,z,width,depth) semi-elliptic draped surface.
    v=[];f=[]
    for i,(cx,cy,z,w,d) in enumerate(rows):
        u=i/max(1,len(rows)-1)
        for j in range(cols):
            t=j/(cols-1);a=(t-.5)*math.pi
            v.append((cx+math.sin(a)*w,cy-math.cos(a)*d+math.sin(t*math.pi*6+u*2)*fold*u,z+math.cos(t*math.pi*5)*fold*u))
    for i in range(len(rows)-1):
        for j in range(cols-1):k=i*cols+j;f.append((k+cols,k+1+cols,k+1,k))
    o=mesh(n,v,f,ma,b);o.data.materials.append(M['Lining']);md=o.modifiers.new('Lined fabric thickness','SOLIDIFY');md.thickness=.005;md.material_offset=1;soft(o)
    return o,v
def plate(n,rows,ma='Porcelain plate',b='Chest'):
    o=loft(n,rows,ma,b,32);soft(o);return o
# Anatomical foundation: narrower shoulder span and longer, leaner silhouette.
plate('Tailored fitted bodice',[(0,0,1.025,.13,.086),(0,0,1.11,.13,.09),(0,.006,1.30,.164,.10),(0,.008,1.46,.198,.10),(0,.014,1.50,.17,.077)],'Lining')
plate('Neck',[(0,0,1.51,.045,.043),(0,0,1.657,.045,.047)],'Pale face','Head')
# Sculpted face; front depth contains cheek planes, nose bridge and orbital hollows.
profile=[(1.632,.009,.032,-.014),(1.648,.034,.045,-.010),(1.67,.058,.058,-.004),(1.699,.079,.068,.002),(1.73,.086,.072,.007),(1.76,.087,.074,.009),(1.79,.089,.076,.014),(1.82,.085,.076,.021),(1.85,.071,.066,.023),(1.874,.039,.042,.023),(1.883,.003,.004,.022)]
N=64;v=[];f=[]
def g(x,c,s):return math.exp(-((x-c)/s)**2)
for z,rx,ry,cy in profile:
    for k in range(N):
        a=math.tau*k/N;x=math.sin(a)*rx;fr=max(0,math.cos(a));y=cy-ry*math.cos(a)
        # actual shaped nose and cheeks, not a flat face image
        y-=fr**9*(.023*g(x,0,.015)*g(z,1.723,.042)+.012*g(x,0,.021)*g(z,1.707,.012))
        y-=fr**4*.006*(g(x,-.050,.025)+g(x,.050,.025))*g(z,1.716,.025)
        y+=fr**5*.007*(g(x,-.039,.025)+g(x,.039,.025))*g(z,1.759,.015)
        v.append((x,y,z))
for i in range(len(profile)-1):
    for k in range(N):a=i*N+k;j=i*N+(k+1)%N;f.append((a,j,j+N,a+N))
soft(mesh('Face • tapered jaw and sculpted nose',v,f,'Pale face','Head'),2)
for s in (-1,1):
    uv('Sculpted ear',(s*.088,.004,1.737),(.014,.010,.028),'Pale face','Head')
    uv('Ear concha',(s*.097,-.002,1.737),(.004,.006,.016),'Blush','Head')
    # Larger expressive sharp anime eye opening. Projected geometry wraps the face.
    # Each is an almond patch, not a round toy eye.
    p=[(s*.014,-.072,1.754),(s*.030,-.075,1.764),(s*.049,-.070,1.766),(s*.071,-.055,1.766),(s*.056,-.065,1.750),(s*.035,-.074,1.747)]
    centre=sum((Vector(x) for x in p),Vector())/len(p);vv=[centre]+[Vector(x) for x in p];ff=[(0,k+1,(k+1)%len(p)+1) for k in range(len(p))]
    mesh('Almond sclera',vv,ff,'Eye light','Head')
    # Iris disc is foreshortened on the face, surrounded by a dark limbal ring.
    for ma,rx,rz,y in [('Iris inner',.009,.010,-.076),('Iris blue',.0075,.009,-.077),('Pupil',.003,.006,-.078)]:
        uv('Turquoise iris • '+ma,(s*.040,y,1.757),(rx,.0017,rz),ma,'Head',32,16)
    uv('Eye catchlight',(s*.037,-.080,1.761),(.003,.001,.0035),'Catchlight','Head',16,8)
    line('Strong upper lash',[p[0],p[1],p[2],p[3]],.0022,'Lash','Head')
    line('Subtle lower lid',[p[3],p[4],p[5],p[0]],.0009,'Blush','Head')
    line('Outer eyelash',[(s*.065,-.058,1.762),(s*.079,-.047,1.768)],.0015,'Lash','Head')
    line('Confident silver brow',[(s*.018,-.071,1.774),(s*.042,-.073,1.782),(s*.067,-.056,1.786)],.0025,'Hair occlusion','Head')
    line('Nostril crease',[(s*.008,-.090,1.706),(s*.013,-.084,1.707)],.0008,'Blush','Head')
line('Upper lip',[(-.019,-.070,1.684),(-.006,-.075,1.687),(0,-.076,1.685),(.019,-.070,1.685)],.0011,'Lips','Head')
line('Lower lip soft light',[(-.010,-.073,1.679),(0,-.076,1.677),(.011,-.073,1.680)],.001,'Pale face','Head')
# Full hair cap covers the cranium down to the nape, overlaid with asymmetric locks.
uv('Full layered hair base',(0,.030,1.811),(.093,.088,.083),'Hair occlusion','Head',48,32)
for i in range(16):
    a=math.pi*(i/15);x=math.cos(a);y=math.sin(a)
    p=[(.018,.013,1.887),(x*.055,.035+y*.044,1.867),(x*.094,.039+y*.069,1.806),(x*.108-.025,.035+y*.092,1.729+random.uniform(-.018,.02))]
    hair('Soft nape layering',p,.024,'Silvery hair' if i%3 else 'Pearl hair')
# One broad swept part, distinct S-curves and crossing tips, not parallel comb teeth.
locks=[
([(.038,.014,1.897),(.012,-.042,1.909),(-.047,-.077,1.853),(-.103,-.072,1.818),(-.144,-.035,1.815)],.029),
([(.047,.012,1.890),(.015,-.058,1.900),(-.034,-.083,1.842),(-.078,-.085,1.807),(-.117,-.066,1.788)],.029),
([(.046,-.005,1.888),(.035,-.067,1.874),(.006,-.084,1.817),(-.032,-.086,1.786),(-.063,-.077,1.767)],.023),
([(.045,.010,1.889),(.075,-.043,1.861),(.084,-.071,1.813),(.109,-.065,1.777),(.129,-.036,1.784)],.027),
([(.025,.033,1.897),(-.030,.023,1.916),(-.084,-.022,1.874),(-.130,-.030,1.861)],.023),
([(.010,.043,1.888),(-.052,.049,1.884),(-.102,.018,1.848),(-.143,.026,1.835)],.025),
([(-.031,.032,1.864),(-.081,-.022,1.830),(-.105,-.036,1.785),(-.128,-.002,1.768)],.024),
([(-.055,.010,1.833),(-.090,-.011,1.792),(-.091,-.015,1.737),(-.135,.015,1.718)],.024),
([(-.042,.051,1.839),(-.081,.049,1.789),(-.108,.054,1.740),(-.154,.076,1.735)],.022),
([(.067,.026,1.855),(.097,.009,1.819),(.106,-.013,1.770),(.099,-.023,1.722)],.020),
([(.045,.055,1.858),(.091,.072,1.814),(.110,.072,1.763),(.094,.079,1.714)],.024)]
for i,(p,w) in enumerate(locks):
    hair('Swept silver lock %02d'%i,p,w,'Pearl hair' if i%3==0 else 'Silvery hair')
    # Fine shadow groove follows the inner crest, deliberately restrained.
    if i<6:line('Fine strand crease',[(x,y-.001,z+.001) for x,y,z in p[1:-1]],.00065,'Hair occlusion','Head')
    if i<9:
        for strand in range(4):
            off=(strand-1.5)*w*.32
            pp=[(x+off,y-.004,z-.002) for x,y,z in p]
            # finer smaller pointed locks create a layered silhouette, not smooth clay ribbons
            hair('Fine swept strand',pp,w*.19,'Pearl hair' if strand%2 else 'Silvery hair')
for i in range(7):
    x=-.053+i*.017
    hair('Parting wisps',[(.031,.006,1.891),(x,-.054,1.862),(x-.016,-.078,1.819),(x-.027,-.081,1.796+random.random()*.017)],.009,'Silvery hair')
# Torso: layered shaped pale breastplate, teal-grey recesses and raised engraving.
plate('Sculpted white breastplate',[(0,-.005,1.105,.125,.094),(0,-.008,1.20,.146,.109),(0,-.005,1.35,.177,.113),(0,.005,1.445,.180,.096)],'Porcelain plate')
def chest_y(x,z):
    return -.009-.112*math.sqrt(max(.06,1-(x/.185)**2))-.004
for s in (-1,1):
    # flowing engraved filigree projects onto the curved plate
    for k in range(4):
        cx=s*(.052+k*.023);cz=1.22+k*.038;p=[]
        for j in range(40):
            a=j/39*math.pi*2.5;r=.027*(1-j/50)
            x=cx+s*math.cos(a)*r;z=cz+math.sin(a)*r*1.8;p.append((x,chest_y(x,z),z))
        curve('Breastplate chased scroll',p,.0015,'Etched silver')
    line('Breastplate pointed centre edge',[(s*.13,chest_y(s*.13,1.38),1.38),(s*.09,chest_y(s*.09,1.25),1.25),(0,-.112,1.11)],.003,'Old gold')
uv('Teal crystal brooch',(0,-.132,1.375),(.016,.009,.037),'Cyan crystal')
for s in (-1,1):
    line('Brooch silver mount',[(0,-.137,1.418),(s*.023,-.133,1.378),(0,-.137,1.332)],.003,'Etched silver')
# Cross-draped shoulder scarf and tall cloth collar, not a cylindrical neck tube.
for k in range(3):
    z=1.52+k*.018
    p=[(-.16,-.017,z-.03),(-.10,-.090,z-.012),(.015,-.102,z-.035),(.12,-.060,z-.06),(.18,.025,z-.075)]
    o=line('Soft overlapping scarf fold',p,.013-k*.001,'Ivory cloth')
cloth('Raised asymmetric scarf collar',[(0,.005,1.603,.062,.051),(-.015,.002,1.572,.094,.072),(-.02,.012,1.53,.17,.086)],'Ivory cloth',fold=.004)
# Draping jacket lapels: long curved smooth fabric, stitched inner edges.
for s in (-1,1):
    p=[(s*.06,-.073,1.54),(s*.16,-.064,1.49),(s*.14,-.116,1.37),(s*.05,-.132,1.24)]
    o=panel('Long tailored lapel',p,'Ivory cloth',thickness=.006);soft(o)
    line('Lapel stitched seam',p,.0014,'Cloth shade')
# Arms: one hand resting naturally on the pommel in the reference pose.
def arm(s,raised):
    side='L' if s>0 else 'R';ua='UpperArm.'+side;fa='Forearm.'+side;hand='Hand.'+side
    shoulder=(s*.19,0,1.445);elbow=(s*.285,-.008,1.235 if raised else 1.17);wrist=(.035,-.253,1.267) if raised else (s*.314,-.055,.94)
    tube('Woven sleeve '+side,smooth([shoulder,(s*.24,0,1.35),elbow],4),[.073]*9,'Cloth shade',ua,20)
    tube('Inner forearm '+side,[elbow,tuple(Vector(elbow).lerp(Vector(wrist),.5)),wrist],[.058,.050,.036],'Lining',fa,20)
    # rounded shoulder shell built as a proper curved surface with scalloped hem
    v=[];f=[];rows=13;cols=24
    for i in range(rows):
        u=i/(rows-1);ph=.18+u*1.57
        for j in range(cols):
            a=-math.pi/2+j/(cols-1)*math.pi*1.12
            x=s*(.18+math.sin(ph)*.115*math.cos(a));y=math.sin(ph)*.107*math.sin(a);z=1.45+math.cos(ph)*.10
            z-=.011*math.sin(j/(cols-1)*math.pi*3)**2*u*u
            v.append((x,y,z))
    for i in range(rows-1):
        for j in range(cols-1):k=i*cols+j;f.append((k,k+1,k+1+cols,k+cols))
    o=mesh('Curved embossed pauldron '+side,v,f,'Porcelain plate',ua);md=o.modifiers.new('Forged shell thickness','SOLIDIFY');md.thickness=.008;soft(o)
    curve('Scalloped pauldron rim '+side,[v[(rows-1)*cols+j] for j in range(cols)],.0035,'Old gold',ua)
    for swirl in range(3):
        p=[]
        for j in range(45):
            t=j/44;a=t*math.pi*2.3;r=.020*(1-t*.7);x=s*(.218+swirl*.018+math.cos(a)*r);z=1.484-swirl*.023+math.sin(a)*r
            y=-math.sqrt(max(.001,.107**2-((x-s*.18)*.45)**2-((z-1.45)*.5)**2))-.005;p.append((x,y,z))
        curve('Pauldron embossed scroll '+side,p,.0018,'Etched silver',ua)
    # elbow couter and tapered forearm sleeve armour, smoothly lofted along the actual arm
    uv('Elbow couter '+side,tuple(Vector(elbow)+Vector((0,-.024,0))),(.065,.058,.065),'Teal plate shadow',fa)
    pts=smooth([tuple(Vector(elbow).lerp(Vector(wrist),.15)),tuple(Vector(elbow).lerp(Vector(wrist),.55)),tuple(Vector(elbow).lerp(Vector(wrist),.92))],4)
    rr=[.060-(j/(len(pts)-1))*.018 for j in range(len(pts))]
    soft(tube('Contoured vambrace '+side,pts,rr,'Porcelain plate',fa,20))
    line('Vambrace ornate ridge '+side,[tuple(Vector(x)+Vector((0,-.055,0))) for x in pts],.003,'Old gold',fa)
    # palm and curled individual fingers resting over the sword cap
    if raised:
        palm=(.007,-.288,1.270);uv('Gloved resting palm',palm,(.053,.032,.021),'Boot',hand)
        for i in range(4):
            x=-.032+i*.020;p=[(x,-.288,1.275),(x,-.324,1.267),(x,-.338,1.244),(x,-.329,1.233)]
            soft(tube('Articulated resting finger',smooth(p,3),[.009]*10,'Boot',hand,10))
            uv('Silver glove knuckle',(x,-.308,1.279),(.008,.014,.005),'Etched silver',hand,12,8)
        tube('Resting thumb',smooth([(.050,-.278,1.26),(.055,-.31,1.245),(.037,-.325,1.239)],4),[.011]*9,'Boot',hand,10)
    else:
        uv('Relaxed gauntlet',tuple(Vector(wrist)+Vector((0,0,-.045))),(.037,.031,.055),'Boot',hand)
        for i in range(4):
            x=s*.29-i*.013;p=[(x,-.071,.914),(x,-.079,.87),(x,-.062,.86)]
            tube('Curled relaxed finger',smooth(p,3),[.008]*7,'Boot',hand,8)
    return shoulder,elbow,wrist
arm_l=arm(1,True);arm_r=arm(-1,False)
# Replace the first-pass mitten-like hands with connected palms, thumb webs and
# three phalanges per finger. Keep all new components bound to individual bones.
for o in list(parts):
    if o.get('bone','').startswith('Hand.'):
        parts.remove(o);bpy.data.objects.remove(o,do_unlink=True)
finger_bones=[]
def finger(name,points,radii,parent):
    for j in range(3):
        bn=name+'.'+str(j+1);a=Vector(points[j]);b=Vector(points[j+1]);finger_bones.append((bn,tuple(a),tuple(b),parent if j==0 else name+'.'+str(j)))
        # rounded continuous skin/glove shape extends slightly across each joint
        aa=a-(b-a)*.08;bb=b+(b-a)*.08
        o=tube('Articulated glove '+bn,smooth([tuple(aa),tuple((a+b)/2+Vector((0,-.001,.001))),tuple(bb)],3),[radii[j]]*7,'Boot',bn,12);soft(o)
        uv('Finger knuckle '+bn,tuple(a),(radii[j]*1.04,radii[j]*1.02,radii[j]*1.02),'Boot',bn,16,10)
        if j<2:
            line('Finger flexion seam '+bn,[tuple(a+Vector((-.006,-radii[j]*.7,.002))),tuple(a+Vector((0,-radii[j],.003))),tuple(a+Vector((.006,-radii[j]*.7,.002)))],.0007,'Etched silver',bn)
# Left palm over the pommel; a flattened anatomical palm with broad metacarpals.
o=loft('Sculpted left glove palm',[(.017,-.270,1.285,.033,.019),(.009,-.289,1.275,.045,.021),(.003,-.305,1.262,.042,.018)],'Boot','Hand.L',28);soft(o)
for i in range(4):
    x=-.029+i*.020;length=[.062,.073,.069,.056][i]
    finger('Finger'+str(i)+'.L',[(x,-.302,1.267),(x,-.326,1.257),(x,-.334,1.233),(x,-.320,1.219+( .073-length)*.25)],[.0095,.0083,.007],'Hand.L')
    uv('Metacarpal glove plate',(x,-.292,1.291),(.008,.014,.004),'Etched silver','Hand.L',16,8)
finger('Thumb.L',[(.045,-.280,1.267),(.055,-.303,1.250),(.044,-.324,1.235),(.028,-.325,1.239)],[.013,.0105,.0085],'Hand.L')
uv('Left thumb web',(.036,-.282,1.265),(.020,.022,.014),'Boot','Hand.L')
# Right hanging hand: palm narrows at the wrist; fingers are naturally staggered.
o=loft('Sculpted right glove palm',[(-.314,-.055,.950,.028,.021),(-.314,-.056,.920,.036,.024),(-.313,-.058,.888,.034,.020)],'Boot','Hand.R',28);soft(o)
for i in range(4):
    x=-.342+i*.019;z=.894+[0,.003,.001,-.005][i];length=[.063,.073,.068,.054][i]
    finger('Finger'+str(i)+'.R',[(x,-.059,z),(x,-.061,z-length*.42),(x,-.070,z-length*.78),(x,-.083,z-length*.91)],[.0092,.008,.0068],'Hand.R')
    uv('Right glove plate',(x,-.079,.919),(.008,.003,.016),'Etched silver','Hand.R',16,8)
finger('Thumb.R',[(-.282,-.056,.927),(-.268,-.065,.907),(-.273,-.080,.888),(-.285,-.085,.884)],[.012,.010,.008],'Hand.R')
uv('Right thumb web',(-.284,-.056,.916),(.016,.020,.026),'Boot','Hand.R')
# A folded broad sash and multiple diagonally wrapped belts.
for k in range(4):
    o=loft('Folded waist sash',[(0,0,1.035+k*.018,.137,.099),(0,0,1.052+k*.018,.141,.101)],'Sash','Hips',40)
for off in (0,.053):
    p=[]
    for j in range(65):
        a=math.tau*j/64;p.append((math.sin(a)*.149,-math.cos(a)*.112,1.048+off+math.sin(a)*.028))
    tube('Wrapped leather belt',p,[(.016,.007)]*len(p),'Belting','Hips',8)
for x,z in [(-.057,1.091),(.025,1.041)]:
    line('Forged belt buckle',[(x-.021,-.121,z-.017),(x+.021,-.121,z-.017),(x+.021,-.121,z+.017),(x-.021,-.121,z+.017),(x-.021,-.121,z-.017)],.004,'Old gold','Hips')
for s in (-1,1):
    uv('Leather utility pouch',(s*.162,.005,1.015),(.044,.049,.064),'Belting','Hips')
    line('Pouch flap stitch',[(s*.194,-.04,1.042),(s*.162,-.050,1.020),(s*.130,-.04,1.042)],.002,'Old gold','Hips')
# Long parted coat with irregular, curved, wind-swept tails and visible lining.
for s in (-1,1):
    side='L' if s>0 else 'R';rows=22;cols=20;v=[];f=[]
    for i in range(rows):
        u=i/(rows-1)
        for j in range(cols):
            t=j/(cols-1);a=-.85+t*3.0;rad=.143+u*.16
            x=s*(math.cos(a)*rad+.065*u*u)+.04*u*u
            y=math.sin(a)*rad*.70+.14*u*u+.013*math.sin(t*math.pi*8+u*2)*u
            z=1.065-u*(.56+.16*math.sin(math.pi*t))+.045*math.sin(t*math.pi*3)*u*u
            v.append((x,y,z))
    for i in range(rows-1):
        for j in range(cols-1):k=i*cols+j;f.append((k,k+1,k+1+cols,k+cols))
    o=mesh('Sweeping embroidered coat '+side,v,f,'Ivory cloth','Coat.'+side);o.data.materials.append(M['Lining']);md=o.modifiers.new('Coat lining','SOLIDIFY');md.thickness=.006;md.material_offset=1;soft(o)
    curve('Coat embroidered hem',[v[(rows-1)*cols+j] for j in range(cols)],.003,'Old gold','Coat.'+side)
    for j in (1,6,13,18):curve('Coat tailored seam',[v[i*cols+j] for i in range(rows)],.0014,'Cloth shade','Coat.'+side)
    # Floral embroidery is sewn onto the sampled cloth grid.
    for k in range(4):
        ri=12+k*2;cj=3+k*3;p=[]
        for j in range(25):
            t=j/24;a=t*math.pi*2.5;ii=min(rows-1,max(0,ri+round(math.sin(a)*(1-t)*2)));jj=min(cols-1,max(0,cj+round(math.cos(a)*(1-t)*2)));p.append(Vector(v[ii*cols+jj])+Vector((s*.002,-.002,0)))
        curve('Coat botanical embroidery',p,.0013,'Etched silver','Coat.'+side)
# A full shoulder mantle drapes continuously over the upper back.
v=[];f=[];rows=20;cols=24
for i in range(rows):
    u=i/(rows-1)
    for j in range(cols):
        t=j/(cols-1);x=(t-.5)*(.32+.32*u)+.075*u*u;y=.070+.18*u+.018*math.sin(t*math.pi*8+u*2)*u;z=1.51-.60*u+.045*math.cos(t*math.pi*4)*u*u
        v.append((x,y,z))
for i in range(rows-1):
    for j in range(cols-1):k=i*cols+j;f.append((k,k+cols,k+cols+1,k+1))
o=mesh('Flowing shoulder mantle',v,f,'Ivory cloth','Cape');md=o.modifiers.new('Mantle thickness','SOLIDIFY');md.thickness=.006;soft(o)
curve('Mantle decorative edging',[v[(rows-1)*cols+j] for j in range(cols)],.003,'Old gold','Cape')
# Loose trousers, gathered knees and slim tall boots like the illustration.
for s in (-1,1):
    side='L' if s>0 else 'R';th='Thigh.'+side;sh='Shin.'+side;foot='Foot.'+side
    rows=[(s*.085,0,1.02,.092,.086),(s*.11,.006,.89,.108,.088),(s*.125,.012,.73,.111,.087),(s*.12,-.003,.59,.088,.067),(s*.11,-.007,.49,.066,.055)]
    o=loft('Gathered tailored trousers '+side,rows,'Pants',th,32)
    # Small irregular radial folds, baked into silhouette instead of random decoration.
    for q in o.data.vertices:
        a=math.atan2(q.co.y,q.co.x-s*.11);amp=.004*math.sin(a*7+q.co.z*17);q.co.x+=amp*math.cos(a);q.co.y+=amp*math.sin(a)
    soft(o,2)
    for k in range(5):line('Gathered knee fabric fold',[(s*.06,-.057,.57+k*.018),(s*.11,-.071,.55+k*.018),(s*.17,-.048,.57+k*.018)],.003,'Sash',sh)
    # The ankle and foot form one continuous sculpted boot; no disconnected capsules.
    boot=loft('Anatomical continuous boot '+side,[(s*.11,-.049,.027,.061,.119),(s*.11,-.050,.047,.063,.122),(s*.11,-.040,.078,.061,.109),(s*.11,-.014,.117,.050,.076),(s*.11,.009,.16,.044,.053),(s*.11,.019,.23,.047,.050),(s*.11,.019,.32,.056,.054),(s*.11,.015,.41,.064,.057),(s*.11,.008,.493,.066,.058),(s*.11,.008,.512,.067,.058)],'Boot',sh,32);soft(boot)
    line('Boot long seam '+side,[(s*.11,-.051,.49),(s*.11,-.048,.31),(s*.11,-.058,.13)],.002,'Etched silver',sh)
    # A distinct thin sole and low heel give the foot weight and a readable profile.
    sole=loft('Leather boot sole '+side,[(s*.11,-.050,.016,.063,.122),(s*.11,-.050,.029,.065,.124)],'Black leather',foot,32)
    line('Boot topline '+side,[(s*.047,-.017,.50),(s*.11,-.052,.51),(s*.174,-.017,.50)],.004,'Cloth shade',sh)
    for j in range(3):line('Boot ankle fold '+side,[(s*.064,-.035,.13+j*.018),(s*.11,-.062,.126+j*.018),(s*.155,-.035,.13+j*.018)],.0025,'Sash',sh)
    line('Boot instep trim',[(s*.053,-.08,.086),(s*.11,-.12,.088),(s*.168,-.08,.086)],.003,'Etched silver',foot)
# Ornate icy longsword held vertically by the resting hand.
sx=0;sy=-.315
outline=[(0,.052),(-.036,.12),(-.048,.70),(-.069,.84),(-.05,.93),(-.072,1.012),(-.045,1.058),(.045,1.058),(.072,1.012),(.05,.93),(.069,.84),(.048,.70),(.036,.12)]
v=[(x,sy-.012,z) for x,z in outline]+[(x,sy+.012,z) for x,z in outline]+[(0,sy-.030,.63),(0,sy+.030,.63)];f=[];n=len(outline)
for j in range(n):f.extend([(2*n,j,(j+1)%n),(2*n+1,n+(j+1)%n,n+j),(j,n+j,n+(j+1)%n,(j+1)%n)])
mesh('Sculpted ice blade',v,f,'Sword ice','Sword',False)
panel('Blade luminous core',[(-.012,sy-.031,.16),(-.015,sy-.031,.94),(0,sy-.034,1.036),(.015,sy-.031,.94),(.012,sy-.031,.16),(0,sy-.031,.07)],'Sword light','Sword',.002)
for s in (-1,1):
    line('Crystalline blade ornament',[(s*.04,sy-.020,.90),(s*.048,sy-.024,.96),(0,sy-.036,1.025),(s*.015,sy-.036,.92),(s*.028,sy-.025,.87)],.0025,'Engraving','Sword')
    # layered curved wings form the distinctive wide ornate guard
    p=[(0,sy,1.055),(s*.045,sy,1.093),(s*.104,sy,1.103),(s*.145,sy,1.084),(s*.17,sy,1.093)]
    o=tube('Winged ice guard',smooth(p,5),[(.018,.012)]*21,'Sword silver','Sword',12)
    line('Guard cyan inset',[(x,y-.016,z+.006) for x,y,z in p],.004,'Cyan crystal','Sword')
    line('Dragon guard flourish',[(0,sy-.020,1.05),(s*.042,sy-.025,1.017),(s*.025,sy-.024,.972),(0,sy-.030,.99)],.005,'Sword silver','Sword')
plate('Sword grip',[(0,sy,1.10,.018,.017),(0,sy,1.245,.021,.019)],'Teal plate shadow','Sword')
for i in range(8):line('Grip chased bands',[(-.017,sy-.012,1.112+i*.016),(0,sy-.022,1.12+i*.016),(.017,sy-.012,1.112+i*.016)],.0018,'Etched silver','Sword')
uv('Pommel crystal',(0,sy,1.26),(.024,.021,.025),'Cyan crystal','Sword')
# Rig for a usable continuation file; hero pose authored directly in the rest geometry.
ad=bpy.data.armatures.new('Reference knight armature');rig=bpy.data.objects.new('Reference Knight Rig',ad);bpy.context.collection.objects.link(rig);bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
def bone(n,h,t,parent=None):
    b=ad.edit_bones.new(n);b.head=h;b.tail=t
    if parent:b.parent=ad.edit_bones[parent]
bone('Root',(0,0,0),(0,0,.2));bone('Hips',(0,0,1.02),(0,0,1.15),'Root');bone('Chest',(0,0,1.15),(0,0,1.50),'Hips');bone('Head',(0,0,1.53),(0,0,1.83),'Chest');bone('Cape',(0,.07,1.49),(0,.23,.93),'Chest');bone('Sword',(0,sy,1.26),(0,sy,.2),'Root')
for s,ap in [(1,arm_l),(-1,arm_r)]:
    side='L' if s>0 else 'R';shoulder,elbow,wrist=ap
    bone('UpperArm.'+side,shoulder,elbow,'Chest');bone('Forearm.'+side,elbow,wrist,'UpperArm.'+side);bone('Hand.'+side,wrist,tuple(Vector(wrist)+Vector((0,-.05,-.02))),'Forearm.'+side)
    bone('Thigh.'+side,(s*.09,0,1.02),(s*.11,0,.54),'Hips');bone('Shin.'+side,(s*.11,0,.54),(s*.11,0,.12),'Thigh.'+side);bone('Foot.'+side,(s*.11,0,.12),(s*.11,-.16,.05),'Shin.'+side);bone('Coat.'+side,(s*.15,0,1.07),(s*.28,.19,.45),'Hips')
for n,h,t,p in finger_bones:bone(n,h,t,p)
ad.edit_bones['Sword'].parent=ad.edit_bones['Hand.L']
bpy.ops.object.mode_set(mode='OBJECT')
for o in parts:
    vg=o.vertex_groups.new(name=o['bone']);vg.add(list(range(len(o.data.vertices))),1,'REPLACE');md=o.modifiers.new('Character armature','ARMATURE');md.object=rig;o.parent=rig
    # Correct winding on enclosed lofts whose centreline runs downwards.
    if any(k in o.name for k in ('Gathered tailored trousers','Woven sleeve','Inner forearm','Contoured vambrace')):
        import bmesh
        bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
# Smooth neighbouring leg weights across knee and ankle instead of rigid limb chunks.
for o in parts:
    n=o.name
    if not ('Gathered tailored trousers' in n or 'Anatomical continuous boot' in n):continue
    side='L' if n.endswith('L') else 'R';isboot='boot' in n
    names=['Shin.'+side,'Foot.'+side] if isboot else ['Thigh.'+side,'Shin.'+side]
    for vg in list(o.vertex_groups):o.vertex_groups.remove(vg)
    groups=[o.vertex_groups.new(name=x) for x in names]
    for v in o.data.vertices:
        z=v.co.z+o.location.z;t=max(0,min(1,((.19-z)/.11 if isboot else (.63-z)/.15)));t=t*t*(3-2*t)
        groups[0].add([v.index],1-t,'REPLACE');groups[1].add([v.index],t,'REPLACE')
# Authored local actions, with cape and split-coat secondary motion.
rig.animation_data_create()
def reset_pose():
    for b in rig.pose.bones:b.rotation_mode='XYZ';b.rotation_euler=(0,0,0);b.location=(0,0,0)
def key(n,f,xyz):
    b=rig.pose.bones[n];b.rotation_euler=tuple(math.radians(x) for x in xyz);b.keyframe_insert('rotation_euler',frame=f)
actions=[]
for name,frames,stride in [('Idle',60,0),('Walk',32,24),('Run',24,42),('Jump',36,0),('Attack',30,0)]:
    reset_pose();a=bpy.data.actions.new(name);a.use_fake_user=True;rig.animation_data.action=a;actions.append(a)
    for f in range(1,frames+2):
        t=(f-1)/frames;phase=math.tau*t
        key('Chest',f,(2*math.sin(phase) if name=='Idle' else 7 if name=='Run' else 1,0,math.sin(phase)*stride*.08))
        key('Head',f,(-1,math.sin(phase)*1.5,0))
        for s in (-1,1):
            side='L' if s>0 else 'R';q=phase+(math.pi if s<0 else 0)
            key('Thigh.'+side,f,(math.sin(q)*stride,0,s*1));key('Shin.'+side,f,(-max(0,math.cos(q))*stride*1.15,0,0));key('Foot.'+side,f,(math.sin(q)*stride*.22,0,0))
            key('Coat.'+side,f,(-5-stride*.25+math.sin(q)*stride*.16,0,s*3));key('UpperArm.'+side,f,(-math.sin(q)*stride*.35 if s<0 else 0,0,0))
        key('Cape',f,(-stride*.35+math.sin(phase)*2,0,math.sin(phase)*2))
        if name=='Jump':
            lift=math.sin(math.pi*min(1,t));key('Thigh.L',f,(-30*lift,0,0));key('Thigh.R',f,(15*lift,0,0));key('Shin.L',f,(-50*lift,0,0));key('Cape',f,(-25*lift,0,0))
        if name=='Attack':
            strike=math.sin(math.pi*min(1,t));key('Chest',f,(4*strike,0,-24*strike));key('Sword',f,(-65*strike,10*strike,32*strike));key('UpperArm.L',f,(-20*strike,0,12*strike))
reset_pose();rig.animation_data.action=actions[0];bpy.context.scene.frame_set(1)
# Fabric microstructure, restrained enough not to distract from silhouette.
for name in ('Ivory cloth','Cloth shade','Lining','Pants','Sash'):
    m=M[name];nt=m.node_tree;p=nt.nodes.get('Principled BSDF');noise=nt.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=190;noise.inputs['Detail'].default_value=2
    bump=nt.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.13;bump.inputs['Distance'].default_value=.0012;nt.links.new(noise.outputs['Fac'],bump.inputs['Height']);nt.links.new(bump.outputs['Normal'],p.inputs['Normal'])
for p in ('D:/GAME/knight/download (1).jpg','D:/GAME/knight/download (2).jpg'):
    im=bpy.data.images.load(p);im.pack()
# A more illustrative studio setup, without altering the model into a 2D billboard.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=64;scene.cycles.use_denoising=True
scene.render.image_settings.file_format='PNG';scene.render.resolution_percentage=100;scene.render.film_transparent=False
scene.view_settings.view_transform='AgX';scene.world.color=(.16,.16,.16)
mat('Studio floor','35444b',.04,.7);bpy.ops.mesh.primitive_plane_add(size=200);bpy.context.object.data.materials.append(M['Studio floor'])
def lamp(n,loc,power,color,size):
    d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=size;o=bpy.data.objects.new(n,d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,1.1))-o.location).to_track_quat('-Z','Y').to_euler()
lamp('Soft warm key',(-3,-4,5),420,(1,.88,.72),4);lamp('Large blue fill',(3,-3,3),190,(.72,.85,1),4);lamp('Sunlit hair rim',(0,2.5,4),600,(1,.87,.64),3)
cd=bpy.data.cameras.new('Character review camera');cam=bpy.data.objects.new('Character review camera',cd);scene.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
def aim(loc,target,scale):cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cd.ortho_scale=scale
aim((2.0,-6,2.45),(0,0,.98),2.23);scene.render.resolution_x=1120;scene.render.resolution_y=1500
bpy.ops.wm.save_as_mainfile(filepath=OUT+'/ReferenceKnight_v3.blend')
# Standalone exported model: bake fabric subdivision into a joined skinned mesh,
# keeping all vertex groups; original separate sculpt parts remain in the saved blend.
bpy.ops.object.select_all(action='DESELECT');export_parts=[]
for source in parts:
    o=source.copy();o.data=source.data.copy();bpy.context.collection.objects.link(o);export_parts.append(o)
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    for md in list(o.modifiers):
        if md.type!='ARMATURE':bpy.ops.object.modifier_apply(modifier=md.name)
    o.select_set(False)
for o in export_parts:o.select_set(True)
bpy.context.view_layer.objects.active=export_parts[0];bpy.ops.object.join();joined=bpy.context.object;joined.name='Reference Knight skinned mesh'
rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=OUT+'/ReferenceKnight_v3.fbx',use_selection=True,object_types={'MESH','ARMATURE'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,mesh_smooth_type='FACE')
bpy.data.objects.remove(joined,do_unlink=True)
stats={'primary_reference':'knight/download (2).jpg','secondary_reference':'knight/download (1).jpg','status':'Second visual interpretation, not installed in Unity, no claim of exact likeness','method':'Locally authored Blender mesh, no paid or image-to-3D provider','mesh_objects':len(parts),'vertices_base_mesh':sum(len(o.data.vertices) for o in parts),'bones':len(ad.bones),'limitations':['Face likeness and hair need human review','Rigid first-pass skinning, no new animation authored','Embroidery and cloth are approximations; hidden body details inferred']}
stats.update(status='V3 anatomical hand/leg revision; animated FBX for Unity',actions=[a.name for a in actions],limitations=['Face remains a stylized approximation','Attack hand contact and clothing deformation need visual review','Finger bones are rigged but not individually keyframed'])
with open(OUT+'/PROVENANCE.json','w') as f:json.dump(stats,f,indent=2)
for n,loc,target,scale,res in [('hero',(2,-6,2.45),(0,0,.98),2.23,(1120,1500)),('face',(.27,-3,1.84),(0,0,1.746),.46,(1200,1200)),('back',(-2,6,2.35),(0,0,.98),2.23,(1120,1500))]:
    aim(loc,target,scale);scene.render.resolution_x,scene.render.resolution_y=res;scene.render.filepath=OUT+'/'+n+'.png';bpy.ops.render.render(write_still=True)
print('REFERENCE_KNIGHT_V2_COMPLETE',json.dumps(stats))
