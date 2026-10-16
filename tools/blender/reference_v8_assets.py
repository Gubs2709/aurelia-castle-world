"""Reference-focused harbour/cottage detail kit. Original Blender geometry.
Outputs a new folder; does not overwrite the previous house or boat sources.
"""
import bpy,sys,math,os,json,random
from mathutils import Vector
sys.path.insert(0,'D:/GAME/tools/blender')
import blendlib as B
from blendlib import Builder
OUT='D:/GAME/AureliaUnity/Assets/Models/ReferenceV8/'
PREV='D:/GAME/artifacts/world-v8/assets/'
os.makedirs(OUT,exist_ok=True);os.makedirs(PREV,exist_ok=True);B.OUT=OUT
random.seed(817)
def line(b,p,w,mat):
    for a,c in zip(p,p[1:]):b.beam(a,c,w,mat)
def window(b,x,y,z,face,w=1.0,h=1.35,shut='shutter_blue',flowers=True):
    def P(u,v,t):
        if face in (-1,1):return(x+u,y+face*t,z+v)
        return(x+(-1 if face=='L' else 1)*t,y+u,z+v)
    def box(u,v,t,sw,sh,depth,ma):
        size=(sw,depth,sh) if face in (-1,1) else (depth,sw,sh)
        b.box(P(u,v,t),size,ma)
    box(0,0,.026,w+.24,h+.24,.065,'wood_dark')
    box(0,0,.071,w,h,.028,'glass')
    for u in(-w/2,0,w/2):box(u,0,.10,.065,h+.13,.06,'trim_white')
    for v in(-h/2,.12,h/2):box(0,v,.11,w+.12,.065,.06,'trim_white')
    box(0,-h/2-.12,.17,w+.38,.12,.32,'stone_light')
    for s in(-1,1):
        box(s*(w/2+.27),0,.08,.42,h+.10,.055,shut)
        for j in range(7):box(s*(w/2+.27),-h/2+.10+j*h/7,.12,.38,.022,.025,'trim_white')
        for v in(-h*.28,h*.28):box(s*(w/2+.27),v,.13,.44,.05,.025,'wood_dark')
    if flowers:
        box(0,-h/2-.29,.29,w+.12,.26,.35,'wood_light')
        for j in range(10):
            pp=P((j/9-.5)*w,-h/2-.13,.31);b.sphere(pp,.11,'leaf',5,6)
            for k in range(4):
                a=k*math.tau/4;b.sphere((pp[0]+math.cos(a)*.07,pp[1]-.06,pp[2]+.12+math.sin(a)*.06),.045,['flower_white','flower_pink','flower_yellow'][j%3],4,5)
def house(name,wall='paint_blue',roof='roof_red',floors=2,cottage=False):
    b=Builder(name);w=6.2 if not cottage else 7.4;d=5.2 if not cottage else 6.8;fh=2.7;base=.28;top=base+floors*fh
    b.box((0,0,.14),(w+.18,d+.18,.28),'wall_stone')
    b.box((0,0,(top+base)/2),(w,d,top-base),wall)
    for z in(base,base+fh,top):b.box((0,0,z),(w+.15,d+.15,.13),'trim_white')
    for x in(-w/2,w/2):
        for y in(-d/2,d/2):b.box((x,y,top/2),(.15,.15,top),'trim_white')
    for floor in range(floors):
        for x in(-w*.28,w*.28):
            if floor==0 and x>0:continue
            window(b,x,-d/2,base+floor*fh+1.5,-1,shut='shutter_green' if cottage else 'shutter_blue')
            window(b,x,d/2,base+floor*fh+1.5,1,flowers=False)
        for side in(-1,1):
            for y in(-d*.26,d*.26):window(b,side*w/2,y,base+floor*fh+1.5,'L' if side<0 else 'R',w=.85,flowers=floor==0)
    # Door is visibly in front of its surround, with panels, hinges and metal handle.
    dx=w*.26;b.box((dx,-d/2-.04,1.35),(1.25,.07,2.25),'trim_white');b.box((dx,-d/2-.10,1.34),(1.03,.06,2.12),'door_wood')
    for j in range(5):b.box((dx-.4+j*.2,-d/2-.145,1.34),(.025,.018,2.0),'wood_dark')
    for z in(.68,1.86):b.box((dx,-d/2-.17,z),(.94,.035,.07),'iron')
    b.sphere((dx+.35,-d/2-.19,1.27),.055,'gold',5,8)
    b.box((dx,-d/2-.38,.18),(1.55,.8,.18),'wall_stone')
    # Gable runs along depth, so the roof ridge and triangular front match.
    rise=w*.44;rx=w/2+.48;ry=d/2+.42
    for s in(-1,1):
        b.quad((0,-ry,top+rise),(s*rx,-ry,top),(s*rx,ry,top),(0,ry,top+rise),roof)
        b.quad((0,-ry,top+rise-.14),(s*rx,-ry,top-.14),(s*rx,ry,top-.14),(0,ry,top+rise-.14),'wood_dark')
        line(b,[(0,-ry-.025,top+rise),(s*rx,-ry-.025,top)],.15,'trim_white')
        line(b,[(0,ry+.025,top+rise),(s*rx,ry+.025,top)],.15,'trim_white')
        # Raised tile-row lips, with fine staggered ribs instead of a flat roof slab.
        for row in range(1,11):
            u=row/11;x=s*rx*u;z=top+rise*(1-u)+.025
            b.beam((x,-ry,z),(x,ry,z),.045,roof)
        for j in range(14):
            y=-ry+j*2*ry/13;b.beam((s*.04,y,top+rise+.028),(s*rx,y,top+.028),.016,roof)
    for y in(-d/2,d/2):b.tri((-w/2,y,top),(w/2,y,top),(0,y,top+rise),wall)
    window(b,0,-d/2,top+rise*.40,-1,w=.70,h=.85,flowers=False)
    b.beam((0,-ry,top+rise+.05),(0,ry,top+rise+.05),.16,roof)
    # Chimney with rim and open dark flue.
    b.box((-w*.28,d*.15,top+rise*.65),(.68,.72,rise*1.3),'wall_stone');b.box((-w*.28,d*.15,top+rise*1.31),(.9,.92,.15),'stone_light');b.box((-w*.28,d*.15,top+rise*1.31+.08),(.55,.57,.015),'iron')
    # Balcony, real balusters, support brackets and fascia.
    if floors>1:
        bw=3.5;b.box((0,-d/2-.70,base+fh),(bw,1.35,.14),'wood_light')
        for i in range(13):b.beam((-bw/2+i*bw/12,-d/2-1.30,base+fh+.05),(-bw/2+i*bw/12,-d/2-1.30,base+fh+1),.055,'trim_white')
        b.beam((-bw/2,-d/2-1.30,base+fh+1),(bw/2,-d/2-1.30,base+fh+1),.08,'wood_dark')
        for x in(-bw*.38,bw*.38):b.beam((x,-d/2,base+fh-.75),(x,-d/2-1.2,base+fh-.1),.11,'timber')
    # Striped awning with individual scalloped valance tabs.
    if not cottage:
        for i in range(10):
            x0=-w*.46+i*w*.092;x1=x0+w*.092;ma='awning_white' if i%2 else 'awning_red'
            b.quad((x0,-d/2,2.55),(x1,-d/2,2.55),(x1,-d/2-1.6,2.03),(x0,-d/2-1.6,2.03),ma)
            b.quad((x0,-d/2-1.6,2.03),(x1,-d/2-1.6,2.03),(x1,-d/2-1.6,1.84),(x0,-d/2-1.6,1.84),ma)
    else:
        # Climbing stems and small leaves, porch lantern and stone border planters.
        for s in(-1,1):
            line(b,[(s*w*.44,-d/2-.08,.2),(s*w*.43,-d/2-.09,2),(s*w*.35,-d/2-.10,4.2),(s*w*.32,-d/2-.09,5.7)],.035,'timber')
            for j in range(32):
                z=.3+j*.16;x=s*w*.42+math.sin(j*.8)*.35;b.sphere((x,-d/2-.14,z),.11,'leaf',5,6)
    return b.build(bevel=.014)
def boat(name,hull='hull_red',sail=True):
    b=Builder(name);L=11.8;W=1.75;N=32;S=12
    def point(i,j,inner=False):
        t=i/N;y=(t-.5)*L;a=-math.pi/2+j/S*math.pi;wid=W*max(.035,math.sin(math.pi*t))**.62
        rim=1.0+.35*abs(t-.5)*2
        return((wid-(.09 if inner else 0))*math.sin(a),y,rim-(1.15-(.09 if inner else 0))*math.cos(a))
    for i in range(N):
        for j in range(S):
            ma=hull if j<3 or j>8 else 'hull_teal'
            b.quad(point(i,j),point(i+1,j),point(i+1,j+1),point(i,j+1),ma)
            b.quad(point(i,j,True),point(i,j+1,True),point(i+1,j+1,True),point(i+1,j,True),'wood_light')
    for j in(0,S):line(b,[point(i,j) for i in range(N+1)],.13,'trim_white')
    for j in(1,3,S-3,S-1):line(b,[point(i,j) for i in range(N+1)],.035,'hull_yellow')
    # Deck planks follow hull width and have true seams.
    for i in range(28):
        t=(i+.5)/28;y=(t-.5)*L*.93;wid=W*max(.03,math.sin(math.pi*(y/L+.5)))**.62
        b.box((0,y,.66),(max(.15,wid*1.84),L*.93/28-.018,.065),'wood_light')
    for y in(-3.5,2.5,4):b.box((0,y,.98),(2.3 if y!=4 else 1.7,.35,.12),'wood_dark')
    if sail:
        b.cyl((0,-.7,5.2),.095,8.7,'wood_dark',12,r2=.055)
        b.beam((0,-.7,2.0),(0,4.6,2.0),.10,'wood_dark')
        def cloth(A,C,D):
            A=Vector(A);C=Vector(C);D=Vector(D);nn=18
            for i in range(nn):
                for j in range(nn-i):
                    def P(u,v):
                        p=A*(1-u-v)+C*u+D*v;p.x+=.40*math.sin(math.pi*u)*math.sin(math.pi*v);return p
                    u=i/nn;v=j/nn;b.tri(P(u,v),P(u+1/nn,v),P(u,v+1/nn),'sail')
                    if i+j<nn-1:b.tri(P(u+1/nn,v),P(u+1/nn,v+1/nn),P(u,v+1/nn),'sail')
        cloth((0,-.65,2.1),(0,-.65,9.35),(0,4.55,2.1));cloth((0,-.85,2.1),(0,-.85,8.9),(0,-5.25,2.1))
        for y in(-5.4,4.8):b.beam((0,-.7,9.5),(0,y,1.1),.025,'rope')
        for s in(-1,1):
            for y in(-2,2.8):b.beam((0,-.7,8.9),(s*1.5,y,1.15),.022,'rope')
        b.tri((0,-.7,9.5),(0,.4,9.3),(0,-.7,9.1),'banner_red')
        b.box((0,2.7,1.12),(1.9,1.8,.9),'wood_dark');b.box((0,2.7,1.61),(2.0,1.9,.1),'roof_slate')
    else:
        b.box((0,-.7,1.47),(1.9,2.1,1.55),'paint_cream');b.box((0,-.7,2.29),(2.15,2.32,.15),'roof_blue')
        for s in(-1,1):b.box((s*.965,-.7,1.8),(.03,1.3,.55),'glass')
        b.box((0,-1.765,1.79),(1.4,.03,.55),'glass');b.beam((0,2.7,.8),(0,2.7,4.3),.08,'wood_dark')
        for s in(-1,1):b.beam((0,2.7,4.1),(s*1.3,4.5,1.2),.025,'rope')
    # Mooring cleats, coiled rope and rows of hanging fenders.
    for s in(-1,1):
        for y in(-3.0,0,3.0):
            b.cyl((s*1.5,y,.70),.13,.65,'rope',10);b.beam((s*1.5,y,1.15),(s*1.5,y,.96),.022,'rope')
    for i in range(4):
        rr=.14+i*.033;line(b,[(.6+math.cos(j*math.tau/32)*rr,1.2+math.sin(j*math.tau/32)*rr,.73) for j in range(33)],.026,'rope')
    return b.build(bevel=.012)
models=[]
def save(name,fn):
    B.clear();o=fn();B.export([o],name);models.append({'name':name,'vertices':len(o.data.vertices),'materials':len(o.data.materials)})
for i,(wall,roof) in enumerate([('paint_blue','roof_red'),('paint_yellow','roof_blue'),('paint_red','roof_slate'),('paint_teal','roof_red'),('paint_cream','roof_red'),('paint_pink','roof_blue')]):
    n='V8_BayHouse_'+str(i);save(n,lambda n=n,wall=wall,roof=roof,i=i:house(n,wall,roof,2+i%2))
save('V8_RiverCottage',lambda:house('V8_RiverCottage','wall_plaster','roof_terracotta',2,True))
save('V8_SailingBoat',lambda:boat('V8_SailingBoat','hull_red',True))
save('V8_FishingBoat',lambda:boat('V8_FishingBoat','hull_blue',False))
def picnic():
    b=Builder('V8_Picnic');w=2.0;d=1.45
    for i in range(16):
        for j in range(12):
            x=-w/2+i*w/16;y=-d/2+j*d/12;z=.018+.012*math.sin(i*.8)*math.sin(j*.6)
            b.quad((x,y,z),(x+w/16,y,z),(x+w/16,y+d/12,z),(x,y+d/12,z),'cloth_white' if(i+j)%2 else 'awning_yellow')
    # Wicker picnic basket, open rim and woven sides.
    cx=.53;cy=.24;rr=.28
    for ring in range(12):
        z=.08+ring*.025;r=rr*(.86+ring*.014);line(b,[(cx+math.cos(k*math.tau/40)*r,cy+math.sin(k*math.tau/40)*r*.7,z) for k in range(41)],.016,'crate_wood')
    for i in range(24):
        a=i*math.tau/24;line(b,[(cx+math.cos(a)*rr*.86,cy+math.sin(a)*rr*.60,.07),(cx+math.cos(a)*rr,cy+math.sin(a)*rr*.7,.36)],.014,'rope')
    line(b,[(cx+math.cos(t*math.pi)*.28,cy,.36+math.sin(t*math.pi)*.34) for t in [i/24 for i in range(25)]],.026,'wood_light')
    # Open book, spine and curved pages.
    for s in(-1,1):
        b.quad((-.38,-.3,.05),(-.38+s*.30,-.3,.04),(-.38+s*.30,.04,.04),(-.38,.04,.05),'door_wood')
        for k in range(6):
            z=.054+k*.002;b.quad((-.38,-.29,z+.025),(-.38+s*.28,-.29,z),(-.38+s*.28,.03,z),(-.38,.03,z+.025),'cloth_white')
        for j in range(12):b.beam((-.38+s*.045,-.26+j*.022,.092),(-.38+s*.235,-.26+j*.022,.073),.0018,'wood_dark')
    # Tea, flask, camera and fruit plate.
    b.cyl((.12,-.32,.065),.20,.025,'cloth_white',32)
    for i in range(8):b.sphere((.12+math.sin(i*2.4)*.12,-.32+math.cos(i*2.4)*.10,.12),.055,'fruit_green' if i%2 else 'fruit_red',6,8)
    b.cyl((-.02,.27,.13),.065,.18,'cloth_white',20);b.cyl((-.02,.27,.225),.055,.003,'wood_dark',20)
    b.cyl((.79,-.30,.24),.075,.40,'paint_teal',20);b.cyl((.79,-.30,.45),.081,.035,'iron',20)
    b.box((.53,-.47,.12),(.25,.13,.18),'iron');b.cyl((.53,-.56,.13),.075,.08,'steel_dark',16,axis='y')
    # Straw sun hat.
    b.cyl((-.67,.38,.085),.21,.025,'hay',32);b.sphere((-.67,.38,.12),.13,'hay',10,16,sq=.75)
    line(b,[(-.67+math.cos(i*math.tau/40)*.13,.38+math.sin(i*math.tau/40)*.13,.12) for i in range(41)],.013,'banner_blue')
    return b.build(bevel=.003)
save('V8_Picnic',picnic)
def lantern():
    b=Builder('V8_HangingLantern');b.box((0,0,.25),(.29,.29,.42),'lantern_glow')
    for x in(-.16,.16):
        for y in(-.16,.16):b.beam((x,y,.03),(x,y,.49),.027,'iron')
    b.box((0,0,.02),(.38,.38,.04),'iron');b.cone((0,0,.52),.27,.18,'iron',4);b.cyl((0,0,.66),.03,.12,'iron',10)
    return b.build(bevel=.007)
save('V8_HangingLantern',lantern)
def barrel_props():
    b=Builder('V8_DockDetails')
    for i in range(3):
        x=i*.72;b.cyl((x,0,.46),.30,.9,'wood_light',20,r2=.28)
        for z in(.12,.38,.72):line(b,[(x+math.cos(k*math.tau/24)*.31,math.sin(k*math.tau/24)*.31,z) for k in range(25)],.035,'iron')
    for i in range(2):
        x=.2+i*1.0;y=.9;b.box((x,y,.28),(.8,.65,.5),'wood_dark')
        for k in range(7):b.box((x-.36+k*.12,y-.34,.30),(.065,.05,.56),'crate_wood')
        for k in range(6):b.sphere((x-.28+k*.11,y,.57),.08,'fish',5,8,sq=.4)
    for i in range(7):
        rr=.17+i*.025;line(b,[(2.0+math.cos(j*math.tau/32)*rr,.90+math.sin(j*math.tau/32)*rr,.04+i*.002) for j in range(33)],.025,'rope')
    return b.build(bevel=.007)
save('V8_DockDetails',barrel_props)
# One low-cost articulated resident, for fishing crowds and gentle quayside walking.
B.clear();root=bpy.data.objects.new('HarbourResident',None);bpy.context.collection.objects.link(root);npc=[]
def npcpart(n,b,pivot):
    o=b.build(bevel=.012)
    for v in o.data.vertices:v.co-=Vector(pivot)
    o.location=pivot;o.name=n;o.parent=root;npc.append(o)
b=Builder('Body');b.cyl((0,0,1.12),.20,.63,'paint_blue',18,r2=.25);b.sphere((0,0,1.65),.15,'skin',12,18,sq=1.2)
for s in(-1,1):b.sphere((s*.05,-.135,1.68),.014,'iron',5,8)
b.cyl((0,0,1.83),.23,.025,'hay',24);b.cyl((0,0,1.86),.14,.09,'hay',24);npcpart('Body',b,(0,0,0))
for s,side in[(-1,'Left'),(1,'Right')]:
    b=Builder(side+'Leg');b.cyl((s*.105,0,.48),.085,.72,'leather',14,r2=.065);b.box((s*.105,-.06,.11),(.15,.30,.16),'iron');npcpart(side+'Leg',b,(s*.105,0,.82))
    b=Builder(side+'Arm');b.beam((s*.22,0,1.39),(s*.30,-.025,1.02),.13,'paint_blue');b.sphere((s*.30,-.025,.99),.062,'skin',8,10);npcpart(side+'Arm',b,(s*.22,0,1.40))
B.export([root]+npc,'V8_Resident');models.append({'name':'V8_Resident','vertices':sum(len(o.data.vertices) for o in npc),'articulated_parts':4})
json.dump(models,open(PREV+'models.json','w'),indent=2)
print('V8_ASSET_KIT_COMPLETE',json.dumps(models))
