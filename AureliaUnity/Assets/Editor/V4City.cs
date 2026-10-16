using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Builds the explorable city from the Blender-authored models: streets, rows, squares, landmarks, outer homesteads.
public static class V4City
{
    public class Info{public float w,d,front;public Vector3 frontLocal=Vector3.back;public float h=8;}
    static Dictionary<string,Info> table=new Dictionary<string,Info>{
        {"House_TudorTall",new Info{w=8.9f,d=6.6f,front=1.3f}},{"House_TudorBlue",new Info{w=9.9f,d=7.0f,front=.9f}},
        {"House_ShopRed",new Info{w=9.4f,d=6.6f,front=1.9f}},{"House_ShopGreen",new Info{w=8.4f,d=6.4f,front=2.0f}},{"House_ShopBlue",new Info{w=8.2f,d=6.4f,front=1.9f}},
        {"House_Cottage",new Info{w=8.4f,d=6.6f,front=3.4f}},{"House_CottageSmall",new Info{w=7.2f,d=5.8f,front=3.4f}},{"House_Farm",new Info{w=9.8f,d=6.4f,front=3.0f}},
        {"House_Tavern",new Info{w=13.6f,d=7.9f,front=2.3f}},
        {"Harbor_A",new Info{w=5.9f,d=5.6f,front=1.9f}},{"Harbor_B",new Info{w=6.5f,d=5.8f,front=1.5f}},{"Harbor_C",new Info{w=6.3f,d=5.6f,front=1.9f}},{"Harbor_D",new Info{w=5.5f,d=5.4f,front=1.9f}},{"Harbor_E",new Info{w=7.3f,d=6.2f,front=1.9f}},
        {"Harbor_F",new Info{w=6.1f,d=5.6f,front=1.9f}},{"Harbor_G",new Info{w=6.7f,d=5.8f,front=1.9f}},{"Harbor_H",new Info{w=5.5f,d=6.3f,front=1.9f}},{"Harbor_I",new Info{w=6.9f,d=6.0f,front=1.9f}},
        {"House_RowA",new Info{w=5.7f,d=6.4f,front=1.3f}},{"House_RowB",new Info{w=5.3f,d=6.4f,front=1.9f}},{"House_RowC",new Info{w=5.9f,d=6.4f,front=1.4f}},{"House_RowD",new Info{w=5.3f,d=6.4f,front=2.3f}},
    };
    public class Rect{public Vector2 c;public float hw,hd,yaw;}
    static List<Rect> placed;static Transform group;static List<Vector2> frontDoors;
    static readonly Dictionary<string,GameObject> cache=new Dictionary<string,GameObject>();
    static GameObject Model(string n){if(!cache.TryGetValue(n,out var g)||!g){g=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Village/"+n+".fbx");cache[n]=g;}return g;}

    // ---- orientation: FrontOffset is the yaw (deg) that must be added so a model's local front faces +Z
    public static float FrontYaw=0;
    public static GameObject Place(string model,Vector3 pos,Vector3 facing,Transform parent,float scale=1,bool collider=true,float frontOffsetDeg=0)
    {
        var src=Model(model);if(!src)return null;
        var go=(GameObject)PrefabUtility.InstantiatePrefab(src,parent);
        go.name=model;go.transform.position=pos;facing.y=0;if(facing.sqrMagnitude<1e-4)facing=Vector3.forward;
        go.transform.rotation=Quaternion.LookRotation(facing.normalized)*Quaternion.Euler(0,FrontYaw+frontOffsetDeg,0);go.transform.localScale=Vector3.one*scale;
        foreach(var r in go.GetComponentsInChildren<MeshRenderer>(true))
        {
            var ms=r.sharedMaterials;var nm=new Material[ms.Length];for(int i=0;i<ms.Length;i++)nm[i]=V4Mats.Get(ms[i]?ms[i].name:"");r.sharedMaterials=nm;
            if(model.StartsWith("Prop_Bunting")||model.StartsWith("Prop_Banner"))r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.BatchingStatic|StaticEditorFlags.ContributeGI);
        }
        if(collider)
        {
            var rs=go.GetComponentsInChildren<MeshRenderer>();if(rs.Length>0){var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);var col=go.AddComponent<BoxCollider>();col.center=go.transform.InverseTransformPoint(b.center);var sz=b.size;
                // box in local space (account for rotation by transforming extents)
                var e=go.transform.InverseTransformVector(new Vector3(b.size.x,0,0));var f=go.transform.InverseTransformVector(new Vector3(0,0,b.size.z));col.size=new Vector3(Mathf.Abs(e.x)+Mathf.Abs(f.x),b.size.y/Mathf.Max(.01f,scale),Mathf.Abs(e.z)+Mathf.Abs(f.z));}
        }
        return go;
    }
    static Vector3 Ground(float x,float z)=>new Vector3(x,V3.H(x,z),z);

    // ---- geometry helpers
    static void Corners(Rect r,Vector2[] o){var cs=Mathf.Cos(r.yaw*Mathf.Deg2Rad);var sn=Mathf.Sin(r.yaw*Mathf.Deg2Rad);Vector2 ax=new Vector2(cs,sn),ay=new Vector2(-sn,cs);o[0]=r.c+ax*r.hw+ay*r.hd;o[1]=r.c-ax*r.hw+ay*r.hd;o[2]=r.c-ax*r.hw-ay*r.hd;o[3]=r.c+ax*r.hw-ay*r.hd;}
    static bool Overlap(Rect a,Rect b,float margin)
    {
        if(Vector2.Distance(a.c,b.c)>a.hw+a.hd+b.hw+b.hd+margin)return false;
        foreach(var r in new[]{a,b})
        {
            var cs=Mathf.Cos(r.yaw*Mathf.Deg2Rad);var sn=Mathf.Sin(r.yaw*Mathf.Deg2Rad);Vector2[] axes={new Vector2(cs,sn),new Vector2(-sn,cs)};
            foreach(var ax in axes)
            {
                float a0=1e9f,a1=-1e9f,b0=1e9f,b1=-1e9f;var ca=new Vector2[4];var cb=new Vector2[4];Corners(a,ca);Corners(b,cb);
                foreach(var p in ca){float d=Vector2.Dot(p,ax);a0=Mathf.Min(a0,d);a1=Mathf.Max(a1,d);}
                foreach(var p in cb){float d=Vector2.Dot(p,ax);b0=Mathf.Min(b0,d);b1=Mathf.Max(b1,d);}
                if(a1+margin<b0||b1+margin<a0)return false;
            }
        }
        return true;
    }
    static bool OnOtherStreet(Rect r,V3Layout.Road self,List<V3Layout.Road> roads)
    {
        var cs=new Vector2[4];Corners(r,cs);
        foreach(var rd in roads){if(rd==self)continue;foreach(var q in rd.pts){float lim=rd.half+.8f;foreach(var p in cs)if((p-q).sqrMagnitude<lim*lim)return true;if((r.c-q).sqrMagnitude<(lim+r.hd)*(lim+r.hd)*.9f)return true;}}
        return false;
    }
    static float[] CumLen(List<Vector2> pts){var c=new float[pts.Count];for(int i=1;i<pts.Count;i++)c[i]=c[i-1]+Vector2.Distance(pts[i],pts[i-1]);return c;}
    static void At(List<Vector2> pts,float[] cum,float s,out Vector2 p,out Vector2 t)
    {
        s=Mathf.Clamp(s,0,cum[cum.Length-1]-.01f);int i=1;while(i<cum.Length-1&&cum[i]<s)i++;float u=(s-cum[i-1])/Mathf.Max(.001f,cum[i]-cum[i-1]);p=Vector2.Lerp(pts[i-1],pts[i],u);t=(pts[i]-pts[i-1]).normalized;
    }
    static bool InZoneFor(V3Layout.Road r,Vector2 p,float margin)
    {
        int z=V3Layout.ZoneOf(p.x,p.y,margin);if(r.district=="A")return z==0;if(r.district=="B")return z==1;if(r.district=="D")return z==2;if(r.district=="E")return z==3;if(r.district=="F")return z==4;return true;
    }

    public static string Build()
    {
        V3Layout.Reset();V4Mats.Rebuild();
        foreach(var n in new[]{"29 • Briarwood village","31 • City of Briarwood"})foreach(var old in Resources.FindObjectsOfTypeAll<GameObject>().Where(go=>go.name==n&&go.scene.IsValid()).ToArray())if(old)Object.DestroyImmediate(old);
        group=V3.FreshGroup("31 • City of Briarwood");placed=new List<Rect>();frontDoors=new List<Vector2>();Random.InitState(31415);
        var roads=V3Layout.Roads();int houses=0;var sb=new System.Text.StringBuilder();
        var props=new GameObject("Props and street furniture").transform;props.SetParent(group,false);var bldg=new GameObject("Buildings").transform;bldg.SetParent(group,false);
        // ---------- reserved landmarks first so houses avoid them
        Vector2 sq=new Vector2(-104,-45);
        void Reserve(Vector2 c,float hw,float hd,float yaw){placed.Add(new Rect{c=c,hw=hw,hd=hd,yaw=yaw});}
        Reserve(sq,22,17,0);Reserve(new Vector2(-180,166),17,17,0);Reserve(new Vector2(0,272),15,15,0);                                   // market square
        // clock tower (south-east corner of the square) and chapel (west side), facing the square
        Vector2 ct=new Vector2(-79,-28),cp=new Vector2(-142,-44),gt=new Vector2(-104,-116);
        Reserve(ct,4,4,0);Reserve(cp,10,6,0);Reserve(gt,10,4,0);
        Reserve(new Vector2(-196,-96),5,5,0);                  // windmill A
        // ---------- houses along streets
        foreach(var rd in roads)
        {
            if(rd.noHouses||rd.models==null||rd.models.Length==0)continue;
            var cum=CumLen(rd.pts);float total=cum[cum.Length-1];
            foreach(int side in rd.onlySide!=0?new[]{rd.onlySide}:new[]{-1,1})
            {
                float cursor=rd.rows?Random.Range(2f,6f):Random.Range(4f,12f);string last="";int fails=0;
                while(cursor<total-4&&fails<400)
                {
                    string m=rd.models[Random.Range(0,rd.models.Length)];if(m==last&&rd.models.Length>1&&Random.value<.7f)m=rd.models[Random.Range(0,rd.models.Length)];
                    var inf=table[m];float w=inf.w;
                    At(rd.pts,cum,cursor+w/2,out var p,out var t);Vector2 n=new Vector2(-t.y,t.x)*side;
                    float off=rd.half+rd.setback+inf.front+inf.d/2;Vector2 body=p+n*off;Vector2 rc=p+n*(rd.half+rd.setback+(inf.front+inf.d)/2);
                    float yawDeg=Mathf.Atan2(t.y,t.x)*Mathf.Rad2Deg;
                    var rect=new Rect{c=rc,hw=w/2,hd=(inf.front+inf.d)/2,yaw=yawDeg};
                    bool ok=InZoneFor(rd,body,8);
                    if(ok)foreach(var o in placed)if(Overlap(rect,o,.25f)){ok=false;break;}
                    if(ok&&OnOtherStreet(rect,rd,roads))ok=false;
                    if(ok&&(Mathf.Abs(body.x-V3.RiverX(body.y))<32&&body.y<62))ok=false;if(ok&&V3Layout.InLake(body.x,body.y,6)||ok&&V3Layout.InLake(rc.x,rc.y,3))ok=false;if(ok){var slopeP=new Vector2[]{body+new Vector2(3,3),body+new Vector2(-3,3),body+new Vector2(3,-3),body+new Vector2(-3,-3)};float mn=1e9f,mx=-1e9f;foreach(var q in slopeP){float yy=V3.H(q.x,q.y);mn=Mathf.Min(mn,yy);mx=Mathf.Max(mx,yy);}if(mx-mn>3.2f)ok=false;}
                    if(!ok){cursor+=2.2f;fails++;continue;}
                    placed.Add(rect);last=m;
                    Vector3 pos=Ground(body.x,body.y);pos.y-=.1f;Vector3 facing=new Vector3(-n.x,0,-n.y);
                    var go=Place(m,pos,facing,bldg);if(go!=null){houses++;frontDoors.Add(p);}
                    cursor+=w+(rd.rows?Random.Range(0f,.25f):rd.gap+Random.Range(0f,3.5f));
                }
            }
        }
        sb.Append("houses "+houses+"; ");
        // ---------- landmarks
        Place("Prop_Gate",Ground(gt.x,gt.y),Vector3.back,props);
        Place("Prop_ClockTower",Ground(ct.x,ct.y),new Vector3(sq.x-ct.x,0,sq.y-ct.y),props);
        Place("Prop_Chapel",Ground(cp.x,cp.y),new Vector3(1,0,0),props,1,true,0).name="Prop_Chapel";
        Place("Prop_Windmill",Ground(-196,-96),Vector3.right,props);
        // ---------- the market square
        Place("Prop_Fountain",Ground(sq.x,sq.y),Vector3.back,props);
        string[] stalls={"Prop_StallRed","Prop_StallBlue","Prop_StallYellow","Prop_StallGreen"};
        for(int i=0;i<10;i++)
        {
            float a=i*Mathf.PI*2/10+.15f;float R=11.2f+(i%2)*1.6f;Vector2 sp=sq+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*R;
            if(Mathf.Abs(sp.x-(-104))<4.8f)continue;   // keep the great avenue open
            if(Mathf.Abs(sp.y-sq.y)<4.8f&&true)continue; // and the cross avenue
            Place(stalls[i%4],Ground(sp.x,sp.y),new Vector3(sq.x-sp.x,0,sq.y-sp.y),props);
        }
        for(int i=0;i<4;i++){float a=i*Mathf.PI/2+Mathf.PI/4;Vector2 bp=sq+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*5.6f;Place("Prop_Bench",Ground(bp.x,bp.y),new Vector3(sq.x-bp.x,0,sq.y-bp.y),props);}
        for(int i=0;i<4;i++){float a=i*Mathf.PI/2+Mathf.PI/4;Vector2 bp=sq+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*16.5f;Place(i%2==0?"Prop_BannerBlue":"Prop_BannerYellow",Ground(bp.x,bp.y),new Vector3(sq.x-bp.x,0,sq.y-bp.y),props);}
        for(int i=0;i<12;i++){float a=i*Mathf.PI/6+.2f;Vector2 bp=sq+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(14.5f+Random.value*1.2f);if(Mathf.Abs(bp.x+104)<5||Mathf.Abs(bp.y-sq.y)<5)continue;string pm=i%3==0?"Prop_Barrel":i%3==1?"Prop_Planter":"Prop_Crates";Place(pm,Ground(bp.x,bp.y),new Vector3(Random.value-.5f,0,Random.value-.5f),props);}
        Place("Prop_Cart",Ground(sq.x+14,sq.y+8),new Vector3(1,0,.4f),props);Place("Prop_Cart",Ground(sq.x-16,sq.y-9),new Vector3(-1,0,.2f),props);
        // ---------- West market plaza and back-town plaza
        void Plaza(Vector2 c,int stalls,bool fountain)
        {
            if(fountain)Place("Prop_Fountain",Ground(c.x,c.y),Vector3.back,props);
            string[] st={"Prop_StallRed","Prop_StallBlue","Prop_StallYellow","Prop_StallGreen"};
            for(int i=0;i<stalls;i++){float a=i*Mathf.PI*2/stalls+.2f;float R=10.5f+(i%2)*1.8f;Vector2 sp=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*R;Place(st[i%4],Ground(sp.x,sp.y),new Vector3(c.x-sp.x,0,c.y-sp.y),props);}
            for(int i=0;i<4;i++){float a=i*Mathf.PI/2+Mathf.PI/4;Vector2 bp=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*5.8f;Place("Prop_Bench",Ground(bp.x,bp.y),new Vector3(c.x-bp.x,0,c.y-bp.y),props);}
            for(int i=0;i<6;i++){float a=i*Mathf.PI/3+.5f;Vector2 bp=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(14f+Random.value);string pm=i%3==0?"Prop_Barrel":i%3==1?"Prop_Planter":"Prop_Crates";Place(pm,Ground(bp.x,bp.y),new Vector3(Random.value-.5f,0,Random.value-.5f),props);}
            Place("Prop_Cart",Ground(c.x+9,c.y-12),new Vector3(1,0,.3f),props);Place(Random.value<.5f?"Prop_BannerBlue":"Prop_BannerYellow",Ground(c.x+15,c.y+3),Vector3.back,props);
        }
        Plaza(new Vector2(-180,166),9,true);Plaza(new Vector2(0,272),7,true);
        // ---------- street furniture
        foreach(var rd in roads)
        {
            if(rd.noHouses&&rd.name!="Castle Road"&&!rd.name.EndsWith("Road"))continue;if(rd.district=="C"||rd.district==null)continue;
            var cum=CumLen(rd.pts);float total=cum[cum.Length-1];float step=rd.name.StartsWith("Great")||rd.name.StartsWith("Market C")?15f:22f;int k=0;
            for(float s=8;s<total-6;s+=step,k++)
            {
                At(rd.pts,cum,s,out var p,out var t);Vector2 n=new Vector2(-t.y,t.x)*(k%2==0?1:-1);Vector2 lp=p+n*(rd.half+.45f);if(V3Layout.ZoneOf(lp.x,lp.y,4)<0&&rd.district!="C")continue;if(Vector2.Distance(lp,sq)<22)continue;
                Place("Prop_Lamp",Ground(lp.x,lp.y),new Vector3(-n.x,0,-n.y),props,1,false);
                // little accents beside the lamp
                if(Random.value<.5f){Vector2 ap=lp+n*.9f+t*1.4f;Place(Random.value<.5f?"Prop_Barrel":"Prop_Planter",Ground(ap.x,ap.y),new Vector3(t.x,0,t.y),props,1,false);}
                if(Random.value<.12f){Vector2 ap=lp+n*.6f-t*2.6f;Place("Prop_Bench",Ground(ap.x,ap.y),new Vector3(-n.x,0,-n.y),props,1,false);}
            }
            // bunting across the main avenues
            if(rd.name.StartsWith("Great")||rd.name.StartsWith("Market C")||rd.name.StartsWith("Artisan"))
                for(float s=16;s<total-10;s+=26)
                {
                    At(rd.pts,cum,s,out var p,out var t);if(Vector2.Distance(p,sq)<24)continue;float span=2*(rd.half+rd.setback+.3f);
                    var go=Place("Prop_Bunting",Ground(p.x,p.y)+Vector3.up*7.4f,new Vector3(-t.y,0,t.x),props,1,false);if(go){go.transform.localScale=new Vector3(span/14f,1,1);}
                }
        }
        // ---------- outer homesteads: lone houses like the reference paintings
        string[] farm={"House_Farm","House_Cottage","House_CottageSmall","House_Farm","House_Cottage"};
        Vector2[] homes={new Vector2(-250,-135),new Vector2(-236,40),new Vector2(-262,-20),new Vector2(214,-168),new Vector2(236,52),new Vector2(250,-60),new Vector2(70,-300),new Vector2(-70,-326),new Vector2(210,-110),new Vector2(-222,-190)};
        int hi=0;
        foreach(var hp0 in homes)
        {
            Vector2 best=hp0;float bs=1e9f;for(int i=0;i<40;i++){var c=hp0+Random.insideUnitCircle*30;float sl=Mathf.Abs(V3.H(c.x+5,c.y)-V3.H(c.x-5,c.y))+Mathf.Abs(V3.H(c.x,c.y+5)-V3.H(c.x,c.y-5));if(sl<bs&&V3.H(c.x,c.y)>2.5f){bs=sl;best=c;}}
            if(bs>9)continue;
            string m=farm[hi++%farm.Length];Vector3 face=new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f));var gp=Ground(best.x,best.y);float miny=gp.y;for(int i=0;i<4;i++){var q=best+new Vector2(i<2?6:-6,i%2==0?6:-6);miny=Mathf.Min(miny,V3.H(q.x,q.y));}gp.y=miny-.1f;
            var home=Place(m,gp,face,bldg);houses++;
            // yard: fence arcs, haystacks, cart, barrel, trodden path to the front
            Vector3 f=face.normalized,sd=Vector3.Cross(Vector3.up,f);
            for(int i=-3;i<=3;i++){var fp=gp+f*-9.5f+sd*i*4;fp.y=V3.H(fp.x,fp.z);Place("Prop_Fence",fp,sd,props,1,false);}
            for(int i=-2;i<=2;i++){var fp=gp+f*10f+sd*i*4;fp.y=V3.H(fp.x,fp.z);if(i!=0)Place("Prop_Fence",fp,sd,props,1,false);}
            for(int i=-1;i<=1;i+=2){var fp=gp+sd*(i*8.5f)+f*2;fp.y=V3.H(fp.x,fp.z);Place("Prop_Fence",fp,f,props,1,false);}
            var hay=gp+sd*-12.5f+f*-4;hay.y=V3.H(hay.x,hay.z);Place("Prop_Haystack",hay,f,props);var hay2=gp+sd*-14.5f+f*-1;hay2.y=V3.H(hay2.x,hay2.z);Place("Prop_Haystack",hay2,sd,props,.8f);
            var cp2=gp+sd*12f+f*6;cp2.y=V3.H(cp2.x,cp2.z);Place("Prop_Cart",cp2,sd+f,props);
            var bp2=gp+sd*4f+f*5.5f;bp2.y=V3.H(bp2.x,bp2.z);Place("Prop_Barrel",bp2,f,props,1,false);
        }
        Place("Prop_Windmill",Ground(172,12),new Vector3(-1,0,-.2f),props);
        Place("Prop_Well",Ground(150,-62),Vector3.back,props);
        foreach(var z in new[]{new Vector2(150,-72),new Vector2(158,-56)})Place("Prop_Haystack",Ground(z.x,z.y),Vector3.back,props);
        Place("Prop_SignPost",Ground(-98,-108),Vector3.back,props,1,false);Place("Prop_SignPost",Ground(-86,26),Vector3.left,props,1,false);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return sb+"total houses "+houses+" placed rects "+placed.Count;
    }
    public static string TerrainRebuild()
    {
        var t=GameObject.Find("Sculpted river valley and planted escarpment");var mf=t.GetComponent<MeshFilter>();var mesh=mf.sharedMesh;var v=mesh.vertices;
        for(int i=0;i<v.Length;i++){var w=t.transform.TransformPoint(v[i]);float y=ReferenceCastleBuilder.Height(w.x,w.z);v[i]=t.transform.InverseTransformPoint(new Vector3(w.x,y,w.z));}
        mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        var mc=t.GetComponent<MeshCollider>();if(mc){mc.sharedMesh=null;mc.sharedMesh=mesh;}
        return "terrain verts "+v.Length;
    }
    public static string Probe()
    {
        var src=Model("House_Cottage");if(!src)return "no model";var go=(GameObject)PrefabUtility.InstantiatePrefab(src);var rs=go.GetComponentsInChildren<MeshRenderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
        var res="cottage bounds centre "+b.center+" size "+b.size+" rot "+go.transform.rotation.eulerAngles+" scale "+go.transform.localScale+" mats "+string.Join(",",rs[0].sharedMaterials.Select(m=>m?m.name:"null"));
        var k=Model("Knight");if(k){var kg=(GameObject)PrefabUtility.InstantiatePrefab(k);var krs=kg.GetComponentsInChildren<Renderer>();var kb=krs[0].bounds;foreach(var r in krs)kb.Encapsulate(r.bounds);res+=" | knight bounds "+kb.center+" size "+kb.size;Object.DestroyImmediate(kg);}
        Object.DestroyImmediate(go);return res;
    }
}
