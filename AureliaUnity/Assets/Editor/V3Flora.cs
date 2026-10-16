using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Sprite-card undergrowth: bushes, grass tufts, wildflowers, ferns, plus sculpted boulders.
public static class V3Flora
{
    class Batch{public Dictionary<Vector2Int,V3.MB> chunks=new Dictionary<Vector2Int,V3.MB>();public V3.MB Get(Vector3 p,float cs=40){var k=new Vector2Int(Mathf.FloorToInt(p.x/cs),Mathf.FloorToInt(p.z/cs));if(!chunks.TryGetValue(k,out var m))chunks[k]=m=new V3.MB(1);return m;}}
    public static bool Ok(float x,float z)
    {
        float y=V3.H(x,z);if(y<2.2f||V3Layout.InLake(x,z,3))return false;if(z<62&&Mathf.Abs(x-V3.RiverX(z))<25.5f)return false;
        return true;
    }
    public static bool NearRoad(Vector2 p,float margin,List<V3Layout.Road> roads)
    {
        foreach(var r in roads)foreach(var q in r.pts)if(Mathf.Abs(q.x-p.x)<r.half+margin&&Mathf.Abs(q.y-p.y)<r.half+margin&&Vector2.Distance(q,p)<r.half+margin)return true;
        return false;
    }
    static void Cross(V3.MB m,Vector3 p,float w,float h,float yaw,int planes,Color col,float lean=0)
    {
        for(int i=0;i<planes;i++)
        {
            float a=yaw+i*Mathf.PI/planes;Vector3 r=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*w*.5f;Vector3 top=Vector3.up*h+new Vector3(Mathf.Sin(yaw*3),0,Mathf.Cos(yaw*3))*lean;
            m.Quad(0,p-r,p+r,p+r+top,p-r+top,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),col,(Vector3.up*.7f+Vector3.Cross(r,Vector3.up).normalized*.3f).normalized);
        }
    }
    static void Emit(Transform g,Batch b,string label,Material mat)
    {
        foreach(var kv in b.chunks)
        {
            var mesh=V3.SaveMesh(kv.Value.Build(label+"_"+kv.Key.x+"_"+kv.Key.y),label.Replace(' ','_')+"_"+kv.Key.x+"_"+kv.Key.y);
            var o=V3.Obj(label,mesh,new[]{mat},g);o.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
    static void BushMesh(V3.MB m,Vector3 p,float size,Color tint)
    {
        int cards=34;
        for(int i=0;i<cards;i++)
        {
            Vector3 o=Random.insideUnitSphere;o.y=Mathf.Abs(o.y)*.8f+.1f;Vector3 pos=p+Vector3.Scale(o,new Vector3(size*.5f,size*.45f,size*.5f));float s=size*Random.Range(.5f,.8f);
            Quaternion q=Random.rotation;Vector3 r=q*Vector3.right*s*.5f,u=q*Vector3.up*s*.5f;float sh=Mathf.Lerp(.7f,1.05f,o.y);var col=new Color(tint.r*sh,tint.g*sh,tint.b*sh,.3f);
            m.Quad(0,pos-r-u,pos+r-u,pos+r+u,pos-r+u,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),col,(o+Vector3.up*.6f).normalized);
        }
    }
    public static string Build()
    {
        foreach(var n in new[]{"18 • Layered undergrowth and multicolour wildflowers","23 • Soft curved forest grasses and flowering path edges","26 • Meadow grass tufts, river reeds and shrubs"})
            foreach(var old in Resources.FindObjectsOfTypeAll<GameObject>().Where(go=>go.name==n&&go.scene.IsValid()).ToArray())Object.DestroyImmediate(old);
        var g=V3.FreshGroup("27 • Meadow, bushes, flowers and ferns");Random.InitState(4242);var roads=V3Layout.Roads();
        var gMats=new[]{V3.Foliage("Grass lush","plant_grass","#ffffff",.5f),V3.Foliage("Grass tall","plant_grass_tall","#f2ffe0",.5f),V3.Foliage("Grass dry","plant_grass_dry","#ffffff",.5f)};
        var gb=new Batch[3];for(int i=0;i<3;i++)gb[i]=new Batch();int tufts=0;
        for(int i=0;i<52000;i++)
        {
            float x=Random.Range(-230f,230f),z=Random.Range(-320f,56f);if(!Ok(x,z))continue;
            float slope=Mathf.Abs(V3.H(x+2,z)-V3.H(x,z))+Mathf.Abs(V3.H(x,z+2)-V3.H(x,z));if(slope>3.4f)continue;
            if(NearRoad(new Vector2(x,z),.25f,roads))continue;
            float y=V3.H(x,z);int k=Random.value<.62f?0:Random.value<.7f?1:2;if(k==2&&V3.Fbm(x*.02f,z*.02f)<.55f)k=0;
            var m=gb[k].Get(new Vector3(x,0,z));float sz=Random.Range(.8f,1.6f);
            Cross(m,new Vector3(x,y-.03f,z),sz,sz*Random.Range(.6f,.9f),Random.value*3.14f,2,new Color(Random.Range(.85f,1.1f),Random.Range(.9f,1.1f),Random.Range(.85f,1f),.5f),Random.Range(.05f,.25f));tufts++;
        }
        for(int i=0;i<3;i++)Emit(g,gb[i],"Grass tufts "+i,gMats[i]);
        string[] fl={"flower_daisy","flower_poppy","flower_tulip","flower_lavender","flower_bell","flower_pink","flower_yellow"};var fb=new Batch[fl.Length];var fm=new Material[fl.Length];
        for(int i=0;i<fl.Length;i++){fb[i]=new Batch();fm[i]=V3.Foliage("Flower "+fl[i],fl[i],"#ffffff",.5f);}
        int fcount=0;
        for(int patch=0;patch<1500;patch++)
        {
            float cx=Random.Range(-195f,165f),cz=Random.Range(-300f,52f);if(!Ok(cx,cz))continue;int k=Random.Range(0,fl.Length);int n=Random.Range(4,11);float rad=Random.Range(1.2f,3.6f);
            for(int j=0;j<n;j++)
            {
                float x=cx+Random.Range(-rad,rad),z=cz+Random.Range(-rad,rad);if(!Ok(x,z)||NearRoad(new Vector2(x,z),.15f,roads))continue;float s=Random.Range(.55f,1.15f);
                Cross(fb[k].Get(new Vector3(x,0,z)),new Vector3(x,V3.H(x,z)-.02f,z),s,s*1.05f,Random.value*3.14f,2,new Color(1,1,1,.4f));fcount++;
            }
        }
        for(int i=0;i<fl.Length;i++)Emit(g,fb[i],"Flowers "+fl[i],fm[i]);
        var fernMat=V3.Foliage("Fern","plant_fern","#ffffff",.5f);var fe=new Batch();int ferns=0;
        foreach(Transform t in GameObject.Find("17 • Towering mixed forests and blossom groves").transform)
        {
            if(Random.value>.55f)continue;
            for(int j=0;j<Random.Range(1,4);j++)
            {
                float a=Random.value*6.28f,d=Random.Range(2.5f,7.5f);float x=t.position.x+Mathf.Cos(a)*d,z=t.position.z+Mathf.Sin(a)*d;if(!Ok(x,z)||NearRoad(new Vector2(x,z),.3f,roads))continue;
                float s=Random.Range(1.1f,1.9f);Cross(fe.Get(new Vector3(x,0,z)),new Vector3(x,V3.H(x,z)-.03f,z),s*1.4f,s,Random.value*3.14f,2,new Color(Random.Range(.8f,1f),1,Random.Range(.8f,1f),.3f));ferns++;
            }
        }
        Emit(g,fe,"Ferns",fernMat);
        string[] bk={"broad","oak","cherry","magenta","white","gold","maple_red"};var bb=new Batch[bk.Length];var bm=new Material[bk.Length];
        for(int i=0;i<bk.Length;i++){bb[i]=new Batch();bm[i]=V3.Foliage("Bush "+bk[i],"leaf_"+bk[i],"#ffffff",.45f);}
        int bush=0;
        for(int i=0;i<4200&&bush<1500;i++)
        {
            float x=Random.Range(-200f,170f),z=Random.Range(-300f,52f);if(!Ok(x,z)||NearRoad(new Vector2(x,z),1.3f,roads))continue;
            bool bank=z<62&&Mathf.Abs(x-V3.RiverX(z))<38;float want=bank?.9f:.35f;if(Random.value>want)continue;
            int k=Random.value<.4f?0:Random.value<.25f?1:Random.Range(0,bk.Length);float size=Random.Range(1.5f,3.4f);
            BushMesh(bb[k].Get(new Vector3(x,0,z)),new Vector3(x,V3.H(x,z)+.1f,z),size,new Color(Random.Range(.88f,1.05f),Random.Range(.9f,1.05f),Random.Range(.88f,1.05f)));bush++;
        }
        for(int i=0;i<bk.Length;i++)Emit(g,bb[i],"Bushes "+bk[i],bm[i]);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "tufts "+tufts+" flowers "+fcount+" ferns "+ferns+" bushes "+bush;
    }

    static Mesh RockMesh(int seed,float flat)
    {
        Random.InitState(seed);int subdiv=3;var verts=new List<Vector3>();var tris=new List<int>();
        float t=(1+Mathf.Sqrt(5))/2;
        verts.AddRange(new[]{new Vector3(-1,t,0),new Vector3(1,t,0),new Vector3(-1,-t,0),new Vector3(1,-t,0),new Vector3(0,-1,t),new Vector3(0,1,t),new Vector3(0,-1,-t),new Vector3(0,1,-t),new Vector3(t,0,-1),new Vector3(t,0,1),new Vector3(-t,0,-1),new Vector3(-t,0,1)});
        int[] f={0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};tris.AddRange(f);
        for(int s=0;s<subdiv;s++)
        {
            var nt=new List<int>();var mid=new Dictionary<long,int>();
            int Mid(int a,int b){long k=a<b?((long)a<<32)+b:((long)b<<32)+a;if(mid.TryGetValue(k,out var r))return r;verts.Add((verts[a]+verts[b])*.5f);return mid[k]=verts.Count-1;}
            for(int i=0;i<tris.Count;i+=3){int a=tris[i],b=tris[i+1],c=tris[i+2];int ab=Mid(a,b),bc=Mid(b,c),ca=Mid(c,a);nt.AddRange(new[]{a,ab,ca,b,bc,ab,c,ca,bc,ab,bc,ca});}
            tris=nt;
        }
        Vector3 off=new Vector3(Random.value*50,Random.value*50,Random.value*50);var v=new Vector3[verts.Count];var cols=new Color[verts.Count];
        for(int i=0;i<v.Length;i++)
        {
            Vector3 d=verts[i].normalized;float n=V3.Fbm(d.x*1.3f+off.x,d.y*1.3f+d.z+off.y,4)+Mathf.PerlinNoise(d.x*4+off.z,d.z*4+d.y*3)*.35f;
            float r=.7f+n*.55f;r=Mathf.Round(r*9f)/9f*.6f+r*.4f;Vector3 p=d*r;p.y*=flat;if(p.y<-.35f*flat)p.y=-.35f*flat;v[i]=p;cols[i]=new Color(Random.Range(.8f,1.1f),0,0,1);
        }
        var m=new Mesh{name="Boulder "+seed,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.vertices=v;m.triangles=tris.ToArray();m.colors=cols;m.RecalculateNormals();m.RecalculateBounds();return m;
    }
    public static string Rocks()
    {
        foreach(var n in new[]{"Weathered natural overlook rock","Stratified cliff outcrop"})
            foreach(var t in Resources.FindObjectsOfTypeAll<Transform>().Where(tr=>tr.name==n&&tr.gameObject.scene.IsValid()).ToArray())if(t)Object.DestroyImmediate(t.gameObject);
        var g=V3.FreshGroup("28 • Boulders, river stones and mossy rocks");Random.InitState(515);
        var mat=V3.Mat("Boulder stone","Aurelia/RockTri");mat.SetTexture("_Rock",V3.T("rock_stone"));mat.SetTexture("_RockN",V3.T("rock_stone_n"));mat.SetFloat("_Scale",.3f);mat.SetFloat("_Moss",.7f);mat.SetColor("_Tint",Color.white);EditorUtility.SetDirty(mat);
        var meshes=new Mesh[8];for(int i=0;i<8;i++)meshes[i]=V3.SaveMesh(RockMesh(100+i*17,Random.Range(.55f,.9f)),"Boulder_"+i);
        Random.InitState(516);var roads=V3Layout.Roads();int count=0;
        void Place(float x,float z,float size,float y=float.NaN)
        {
            float gy=float.IsNaN(y)?V3.H(x,z):y;var q=Quaternion.Euler(Random.Range(-8f,8f),Random.value*360,Random.Range(-8f,8f));
            V3.Obj("Boulder",meshes[Random.Range(0,8)],new[]{mat},g,new Vector3(x,gy-size*.12f,z),q,new Vector3(size*Random.Range(.9f,1.5f),size*Random.Range(.7f,1.1f),size*Random.Range(.9f,1.5f)));count++;
        }
        for(float z=-300;z<58;z+=Random.Range(3.5f,8f))
            foreach(int side in new[]{-1,1})
            {
                float cx=V3.RiverX(z);float x=cx+side*Random.Range(24.5f,29.5f);float s=Random.value<.2f?Random.Range(2.2f,3.8f):Random.Range(.7f,1.8f);Place(x,z,s,Mathf.Max(V3.H(x,z),1.2f));
            }
        for(int i=0;i<150;i++){float z=Random.Range(-300,55),x=V3.RiverX(z)+Random.Range(-23,23);Place(x,z,Random.Range(.5f,1.6f),1.45f);}
        for(int i=0;i<2500&&count<560;i++)
        {
            float x=Random.Range(-260,260),z=Random.Range(-340,50);if(!Ok(x,z)||V3Layout.ZoneOf(x,z)>=0||NearRoad(new Vector2(x,z),2.2f,roads))continue;if(Random.value>.07f)continue;
            Place(x,z,Random.value<.15f?Random.Range(2f,3.6f):Random.Range(.6f,1.6f));
        }
        foreach(var r in roads.Where(rr=>rr.name=="Forest trail"))
            for(int i=0;i<r.pts.Count;i+=5)
            {
                var p=r.pts[i];int side=Random.value<.5f?-1:1;var d=(r.pts[Mathf.Min(r.pts.Count-1,i+1)]-p).normalized;var nrm=new Vector2(-d.y,d.x)*side;float off=r.half+Random.Range(.8f,2.4f);
                Place(p.x+nrm.x*off,p.y+nrm.y*off,Random.Range(.5f,1.1f));
            }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "boulders "+count;
    }
}
