using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;
using K=V3House.Kind;

public static class V3Village
{
    static Material[] plasterMats,woodMats,roofMats,accentMats;static Material stone,timber,glass,light,stoneWall;static Material[] plantMats;
    static void Mats()
    {
        string[] pt={"#ffffff","#f6d6cb","#d6e4ec","#f6e7a8","#d8e8cc"};plasterMats=pt.Select((c,i)=>V3.Lit("Plaster "+i,"wall_plaster","wall_plaster_n",.05f,null,c)).ToArray();
        woodMats=new[]{V3.Lit("Planks light","wood_light","wood_light_n",.08f,null,"#ffffff"),V3.Lit("Planks dark","wood_light","wood_light_n",.08f,null,"#8a6a4c")};
        roofMats=new[]{V3.Lit("Roof terracotta","roof_terracotta","roof_terracotta_n",.12f),V3.Lit("Roof slate","roof_slate","roof_slate_n",.18f),V3.Lit("Roof shingle","roof_shingle","roof_shingle_n",.06f),V3.Lit("Roof teal","roof_teal","roof_teal_n",.2f)};
        string[] ac={"#4f7f58","#55789f","#a2503f","#c89a48","#f1ead8"};accentMats=ac.Select((c,i)=>V3.Lit("Shutter "+i,"wood_light","wood_light_n",.1f,null,c)).ToArray();
        stoneWall=V3.Lit("Wall stone","wall_stone","wall_stone_n",.05f);stone=V3.Lit("Stone trim","wall_stone","wall_stone_n",.05f,null,"#f0ece0");timber=V3.Lit("Timber dark","wood_beam","wood_beam_n",.08f);light=V3.Lit("Wood light","wood_light","wood_light_n",.08f);
        glass=V3.Mat("Window glass","Universal Render Pipeline/Lit");glass.SetColor("_BaseColor",V3.C("#2c4660"));glass.SetFloat("_Smoothness",.92f);glass.SetFloat("_Metallic",.1f);EditorUtility.SetDirty(glass);
        plantMats=new[]{V3.Foliage("Ivy","leaf_broad","#d9f0c8"),V3.Foliage("Flower pink","flower_pink"),V3.Foliage("Flower yellow","flower_yellow"),V3.Foliage("Shrub","leaf_oak","#e8ffe0"),V3.Foliage("Flower lavender","flower_lavender")};
    }
    static K[] Kinds()=>new[]{
        new K{name="Tudor",w=9,d=7,floors=2,pitch=46,overhang=.55f,frame=true,porch=false,dormers=true,wallType=0,ivy=.8f},
        new K{name="TudorSmall",w=7.2f,d=6f,floors=2,pitch=48,overhang=.5f,frame=true,porch=true,dormers=false,wallType=0,ivy=1.2f},
        new K{name="StoneCottage",w=6.6f,d=5.6f,floors=1,pitch=44,overhang=.55f,frame=false,porch=true,dormers=false,wallType=2,ivy=1.5f},
        new K{name="Chalet",w=8.6f,d=7.2f,floors=2,pitch=30,overhang=1.1f,frame=false,porch=false,balcony=true,dormers=false,wallType=1,ivy=.2f},
        new K{name="LogHut",w=6.2f,d=5.2f,floors=1,pitch=36,overhang=.8f,frame=false,porch=true,dormers=false,wallType=3,logs=true,ivy=.1f},
        new K{name="Inn",w=13.5f,d=8f,floors=2,pitch=42,overhang=.6f,frame=true,porch=true,dormers=true,wallType=0,ivy=1f},
        new K{name="Manor",w=11f,d=7.5f,floors=2,pitch=40,overhang=.5f,frame=false,porch=true,dormers=true,hip=true,wallType=2,ivy=1.2f}};
    class Built{public K kind;public Mesh body,plants;public Vector2 foot;public float height;}
    static List<Built> built=new List<Built>();
    static void BuildMeshes()
    {
        built.Clear();var kinds=Kinds();
        for(int k=0;k<kinds.Length;k++)for(int v=0;v<2;v++)
        {
            var m=new V3.MB(V3House.N_STRUCT);var pl=new V3.MB(V3House.N_PLANT);V3House.Build(m,pl,kinds[k],k*17+v*101+7,out var fp);
            var body=V3.SaveMesh(m.Build("House "+kinds[k].name+v),"House_"+kinds[k].name+v);var plants=V3.SaveMesh(pl.Build("HousePlants "+kinds[k].name+v),"HousePlants_"+kinds[k].name+v);
            built.Add(new Built{kind=kinds[k],body=body,plants=plants,foot=fp,height=body.bounds.max.y});
        }
    }
    static void Prop(V3.MB m,int sub,Vector3 p,Vector3 size,Quaternion? q=null)=>V3House.Box(m,sub,p,size,q,1.6f);
    // props sub: 0 stone,1 timber,2 light wood,3 cloth red,4 cloth blue,5 cloth yellow,6 hay,7 lantern glow
    static void Stall(V3.MB m,Vector3 c,float yaw,int cloth)
    {
        var q=Quaternion.Euler(0,yaw,0);Vector3 L(float x,float y,float z)=>c+q*new Vector3(x,y,z);
        foreach(int sx in new[]{-1,1})foreach(int sz in new[]{-1,1})Prop(m,2,L(sx*1.5f,1.35f,sz*.9f),new Vector3(.12f,2.7f,.12f),q);
        Prop(m,2,L(0,.9f,.5f),new Vector3(3.2f,.1f,1f),q);Prop(m,2,L(0,.45f,.5f),new Vector3(3f,.08f,.9f),q);
        // sloped awning in cloth colour
        Vector3 a=L(-1.75f,2.9f,-1.1f),b=L(1.75f,2.9f,-1.1f),cc=L(1.75f,2.5f,1.2f),d=L(-1.75f,2.5f,1.2f);m.Quad(3+cloth%3,d,cc,b,a,new Vector2(0,0),new Vector2(2,0),new Vector2(2,1.4f),new Vector2(0,1.4f),Color.white);
        m.Quad(3+cloth%3,a,b,cc,d,new Vector2(0,0),new Vector2(2,0),new Vector2(2,1.4f),new Vector2(0,1.4f),Color.white);
        for(int i=0;i<5;i++){var cr=Quaternion.Euler(0,Random.value*60,0);Prop(m,i%2==0?2:6,L(-1.2f+i*.6f,1.05f,.5f),new Vector3(.4f,.22f,.4f),q*cr);}
    }
    static void Well(V3.MB m,Vector3 c)
    {
        V3House.Cyl(m,0,c,1.15f,.95f,16,1.5f);V3House.Cyl(m,0,c+Vector3.up*.9f,1.25f,.12f,16,1.5f);
        foreach(int s in new[]{-1,1})Prop(m,1,c+new Vector3(s*1.1f,1.7f,0),new Vector3(.16f,2.4f,.16f));
        Prop(m,1,c+new Vector3(0,2.7f,0),new Vector3(2.5f,.14f,.14f));Prop(m,1,c+new Vector3(0,2.2f,0),new Vector3(.08f,.08f,.7f));
        Vector3 e0=c+new Vector3(-1.5f,2.65f,1.3f),e1=c+new Vector3(1.5f,2.65f,1.3f),r1=c+new Vector3(1.5f,3.4f,0),r0=c+new Vector3(-1.5f,3.4f,0),b0=c+new Vector3(-1.5f,2.65f,-1.3f),b1=c+new Vector3(1.5f,2.65f,-1.3f);
        m.Quad(8,e0,e1,r1,r0,new Vector2(0,0),new Vector2(2,0),new Vector2(2,1.4f),new Vector2(0,1.4f),Color.white);m.Quad(8,b1,b0,r0,r1,new Vector2(0,0),new Vector2(2,0),new Vector2(2,1.4f),new Vector2(0,1.4f),Color.white);
    }
    static void Lamp(V3.MB m,Vector3 c)
    {
        Prop(m,1,c+Vector3.up*1.7f,new Vector3(.14f,3.4f,.14f));Prop(m,1,c+new Vector3(.35f,3.35f,0),new Vector3(.8f,.08f,.08f));Prop(m,7,c+new Vector3(.7f,3.1f,0),new Vector3(.3f,.4f,.3f));Prop(m,1,c+new Vector3(.7f,3.35f,0),new Vector3(.4f,.06f,.4f));
    }
    static void Barrel(V3.MB m,Vector3 c){V3House.Cyl(m,2,c,.42f,.9f,10,1.2f,.36f);V3House.Cyl(m,1,c+Vector3.up*.3f,.45f,.06f,10,1);V3House.Cyl(m,1,c+Vector3.up*.65f,.45f,.06f,10,1);}
    static void Hay(V3.MB m,Vector3 c){V3House.Cyl(m,6,c,.65f,1.0f,12,1.2f);}
    static void Fence(V3.MB m,Vector3 a,Vector3 b)
    {
        Vector3 d=b-a;float len=d.magnitude;int n=Mathf.Max(1,Mathf.RoundToInt(len/2f));Quaternion q=Quaternion.LookRotation(d.normalized);
        for(int i=0;i<=n;i++){Vector3 p=Vector3.Lerp(a,b,i/(float)n);p.y=V3.H(p.x,p.z);Prop(m,2,p+Vector3.up*.6f,new Vector3(.12f,1.2f,.12f));}
        for(int j=0;j<2;j++){Vector3 mid=(a+b)/2;mid.y=(V3.H(a.x,a.z)+V3.H(b.x,b.z))/2+.4f+j*.4f;Prop(m,2,mid,new Vector3(.07f,.08f,len),q);}
    }

    public static string Build()
    {
        foreach(var n in new[]{"13 • Inhabited timber and stone hamlets","16 • Natural planted fields and cottage paths","19 • Ivy balconies, flower boxes and cottage gardens","04 • Farms, hamlets and wooded slopes","Winding forest walking trail"})
            foreach(var old in Resources.FindObjectsOfTypeAll<GameObject>().Where(go=>go.name==n&&go.scene.IsValid()).ToArray())if(old)Object.DestroyImmediate(old);
        Mats();BuildMeshes();var g=V3.FreshGroup("29 • Briarwood village");Random.InitState(2468);
        var placed=new List<(Vector2 p,float r)>();var roads=V3Layout.Roads();int houses=0;
        string[] houseRoads={"Village street","West lane","East lane","Garden lane","Viaduct approach"};
        var weights=new Dictionary<string,float[]>(){{"Village street",new[]{3f,2.5f,1.5f,.8f,.8f,.7f,.8f}},{"West lane",new[]{1.5f,2.5f,3f,.5f,2f,0f,.2f}},{"East lane",new[]{1.5f,2.5f,2.5f,2f,1.5f,0f,.2f}},{"Garden lane",new[]{.5f,1.5f,3f,1f,3f,0f,0f}},{"Viaduct approach",new[]{0f,0f,0f,0f,0f,0f,0f}}};
        Material[] Pick(Built b)
        {
            var k=b.kind;Material wall=k.wallType==0?plasterMats[Random.Range(0,plasterMats.Length)]:k.wallType==1?woodMats[0]:k.wallType==3?woodMats[1]:stoneWall;
            Material roof=k.name=="Chalet"?roofMats[1]:Random.value<.5f?roofMats[0]:Random.value<.45f?roofMats[2]:Random.value<.5f?roofMats[3]:roofMats[1];if(k.name=="LogHut")roof=roofMats[2];
            return new[]{wall,timber,roof,stone,glass,accentMats[Random.Range(0,accentMats.Length)],light};
        }
        void Spawn(Vector2 pos,Vector2 face,Built b,int idx)
        {
            float y=Mathf.Min(V3.H(pos.x,pos.y),V3.H(pos.x+b.foot.x/2,pos.y+b.foot.y/2),V3.H(pos.x-b.foot.x/2,pos.y-b.foot.y/2),V3.H(pos.x+b.foot.x/2,pos.y-b.foot.y/2),V3.H(pos.x-b.foot.x/2,pos.y+b.foot.y/2));
            var go=new GameObject("Village "+b.kind.name+" "+idx);go.transform.SetParent(g,false);go.transform.position=new Vector3(pos.x,y-.15f,pos.y);go.transform.rotation=Quaternion.LookRotation(new Vector3(face.x,0,face.y));
            var body=V3.Obj("Structure",b.body,Pick(b),go.transform);var pl=V3.Obj("Plants",b.plants,plantMats,go.transform);pl.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            var col=go.AddComponent<BoxCollider>();col.size=new Vector3(b.foot.x,b.height,b.foot.y);col.center=new Vector3(0,b.height/2,0);
        }
        foreach(var rn in houseRoads)
        {
            var road=roads.First(r=>r.name==rn);var w=weights[rn];float travelled=0;float next=Random.Range(4f,8f);
            for(int i=1;i<road.pts.Count;i++)
            {
                travelled+=Vector2.Distance(road.pts[i],road.pts[i-1]);if(travelled<next||w.Sum()<=0)continue;next=travelled+Random.Range(11f,15f);
                Vector2 pt=road.pts[i],dir=(road.pts[Mathf.Min(road.pts.Count-1,i+1)]-road.pts[Mathf.Max(0,i-1)]).normalized,nrm=new Vector2(-dir.y,dir.x);
                foreach(int side in new[]{-1,1})
                {
                    float tr=Random.value*w.Sum(),acc=0;int kind=0;for(int q=0;q<w.Length;q++){acc+=w[q];if(tr<=acc){kind=q;break;}}
                    var cand=built.Where(bb=>bb.kind.name==Kinds()[kind].name).ToList();var b=cand[Random.Range(0,cand.Count)];
                    float setback=road.half+3.2f+b.foot.y/2;Vector2 pos=pt+nrm*side*setback+dir*Random.Range(-1.5f,1.5f);
                    if(V3Layout.ViaductDist(pos.x,pos.y)<20)continue;if(pos.y<-60)continue;bool bad=false;float rad=Mathf.Max(b.foot.x,b.foot.y)*.55f+1.5f;
                    foreach(var o in placed)if(Vector2.Distance(o.p,pos)<rad+o.r){bad=true;break;}if(bad)continue;
                    if(Mathf.Abs(pos.x-V3.RiverX(pos.y))<34&&pos.y<62)continue;
                    float slope=Mathf.Abs(V3.H(pos.x+3,pos.y)-V3.H(pos.x-3,pos.y))+Mathf.Abs(V3.H(pos.x,pos.y+3)-V3.H(pos.x,pos.y-3));if(slope>3.2f)continue;
                    foreach(var r2 in roads)foreach(var q2 in r2.pts)if(r2.name!=rn&&Vector2.Distance(q2,pos)<r2.half+Mathf.Max(b.foot.x,b.foot.y)*.5f+1f){bad=true;break;}if(bad)continue;
                    placed.Add((pos,rad));Spawn(pos,-nrm*side,b,houses++);
                }
            }
        }
        // square: well, stalls, lamps, barrels, hay, fences
        var pm=new V3.MB(9);Vector3 sq=new Vector3(V3Layout.VillageCentre.x-1,0,V3Layout.VillageCentre.y+2);sq.y=V3.H(sq.x,sq.z);
        Well(pm,sq);
        for(int i=0;i<5;i++){float a=i*72*Mathf.Deg2Rad+.3f;Vector3 p=sq+new Vector3(Mathf.Cos(a)*8.5f,0,Mathf.Sin(a)*8.5f);p.y=V3.H(p.x,p.z);Stall(pm,p,-a*Mathf.Rad2Deg+90,i);}
        var road0=roads.First(r=>r.name=="Village street");for(int i=8;i<road0.pts.Count;i+=16){var p=road0.pts[i];var d=(road0.pts[Mathf.Min(road0.pts.Count-1,i+1)]-p).normalized;var nn=new Vector2(-d.y,d.x);Vector2 lp=p+nn*(road0.half+.7f)*(i%32==8?1:-1);Lamp(pm,new Vector3(lp.x,V3.H(lp.x,lp.y),lp.y));}
        foreach(var hs in placed){if(Random.value<.5f){Vector2 p=hs.p+Random.insideUnitCircle.normalized*(hs.r+.6f);if(Mathf.Abs(p.x-V3.RiverX(p.y))>30&&!V3Flora.NearRoad(p,.5f,roads)){if(Random.value<.5f)Barrel(pm,new Vector3(p.x,V3.H(p.x,p.y),p.y));else Hay(pm,new Vector3(p.x,V3.H(p.x,p.y),p.y));}}}
        var propMats=new[]{stoneWall,timber,light,V3.Lit("Cloth red","wall_plaster",null,.05f,null,"#c8453a"),V3.Lit("Cloth blue","wall_plaster",null,.05f,null,"#4a6fa8"),V3.Lit("Cloth yellow","wall_plaster",null,.05f,null,"#e0b440"),V3.Lit("Hay","ground_dirt",null,.02f,null,"#e8cf68"),null,roofMats[2]};
        var lantern=V3.Mat("Lantern glow","Universal Render Pipeline/Lit");lantern.SetColor("_BaseColor",V3.C("#ffd88a"));lantern.EnableKeyword("_EMISSION");lantern.SetColor("_EmissionColor",V3.C("#ffb050")*1.6f);lantern.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;EditorUtility.SetDirty(lantern);propMats[7]=lantern;
        V3.Obj("Village square props",V3.SaveMesh(pm.Build("Village props"),"VillageProps"),propMats,g);
        var wc=new GameObject("Well collider").AddComponent<CapsuleCollider>();wc.transform.SetParent(g,false);wc.transform.position=sq+Vector3.up*1f;wc.radius=1.3f;wc.height=2f;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "houses "+houses;
    }
}
