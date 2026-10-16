using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using Random=UnityEngine.Random;

public static class V8Kit
{
    public const string Art="Assets/Art/ReferenceV8/";
    public static void Import()
    {
        foreach(string name in new[]{"GreenBough","MeadowFlowers","CliffStone","RoofTiles"})
        {string p=Art+name+".png";AssetDatabase.ImportAsset(p);var imp=(TextureImporter)AssetImporter.GetAtPath(p);bool alpha=name=="GreenBough"||name=="MeadowFlowers";imp.alphaIsTransparency=alpha;imp.mipmapEnabled=true;imp.mipMapsPreserveCoverage=alpha;imp.alphaTestReferenceValue=.35f;imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.wrapMode=alpha?TextureWrapMode.Clamp:TextureWrapMode.Repeat;imp.SaveAndReimport();}
    }
    static Dictionary<string,Material> library=new Dictionary<string,Material>();
    public static Material Surface(string key,string hex,string texture=null,float scale=.22f)
    {
        var m=V3.Mat("V8 "+key,texture!=null?"Aurelia/Surface Detail":"Universal Render Pipeline/Lit");m.SetColor("_BaseColor",V3.C(hex));m.SetFloat("_Smoothness",.05f);
        if(texture!=null){var t=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+texture+".png")??V3.T(texture);m.SetTexture("_BaseMap",t);m.SetFloat("_Scale",scale);m.SetFloat("_Relief",.12f);m.SetFloat("_TextureStrength",.7f);m.SetFloat("_Cull",0);}return m;
    }
    public static Material Material(string name)
    {
        if(library.TryGetValue(name,out var m))return m;var src=V4Mats.Get(name);string path=V3.Dir+"/V8 Model "+name+".mat";m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(src){name="V8 "+name};AssetDatabase.CreateAsset(m,path);}else m.CopyPropertiesFromMaterial(src);
        m.SetFloat("_Smoothness",name=="glass"?.28f:.08f);if(name=="glass"){m.DisableKeyword("_EMISSION");m.SetColor("_BaseColor",V3.C("#527f94"));}
        if(name=="roof_red"||name=="roof_terracotta"){m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"RoofTiles.png"));m.SetTextureScale("_BaseMap",Vector2.one*.8f);m.SetColor("_BaseColor",Color.white);m.DisableKeyword("_NORMALMAP");}
        if(name=="lantern_glow"){m.DisableKeyword("_EMISSION");m.SetColor("_BaseColor",V3.C("#d7cc9a"));}
        library[name]=m;EditorUtility.SetDirty(m);return m;
    }
    public static GameObject Place(string model,Vector3 pos,Vector3 facing,Transform parent,float scale=1,bool collider=true)
    {
        string path="Assets/Models/ReferenceV8/"+model+".fbx";var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!prefab)throw new Exception("Missing V8 asset "+path);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name=model;go.transform.position=pos;go.transform.rotation=Quaternion.LookRotation(new Vector3(facing.x,0,facing.z).normalized);go.transform.localScale=Vector3.one*scale;
        foreach(var r in go.GetComponentsInChildren<MeshRenderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>Material(m?m.name:"wood_light")).ToArray();
        if(collider){var rs=go.GetComponentsInChildren<MeshRenderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);var box=go.AddComponent<BoxCollider>();box.center=go.transform.InverseTransformPoint(b.center);box.size=new Vector3(6.2f,5.7f,5.2f);}
        return go;
    }
    public static void Box(V3.MB b,Vector3 c,Vector3 s,Quaternion q,int sub=0,Color? color=null)
    {
        Vector3 P(float x,float y,float z)=>c+q*Vector3.Scale(new Vector3(x,y,z),s)*.5f;Color col=color??Color.white;
        void F(Vector3 a,Vector3 bb,Vector3 cc,Vector3 d)=>b.Quad(sub,a,bb,cc,d,Vector2.zero,Vector2.right,Vector2.one,Vector2.up,col);
        F(P(-1,1,-1),P(-1,1,1),P(1,1,1),P(1,1,-1));F(P(-1,-1,1),P(-1,-1,-1),P(1,-1,-1),P(1,-1,1));F(P(-1,-1,-1),P(-1,-1,1),P(-1,1,1),P(-1,1,-1));F(P(1,-1,1),P(1,-1,-1),P(1,1,-1),P(1,1,1));F(P(1,-1,-1),P(-1,-1,-1),P(-1,1,-1),P(1,1,-1));F(P(-1,-1,1),P(1,-1,1),P(1,1,1),P(-1,1,1));
    }
    public static void Beam(V3.MB b,Vector3 a,Vector3 c,float width,int sub=0,Color? color=null)=>Box(b,(a+c)*.5f,new Vector3(width,width,(a-c).magnitude),Quaternion.LookRotation((c-a).normalized),sub,color);
    public static GameObject Obj(string name,V3.MB b,Material[] mats,Transform g,bool collision=false)
    {var mesh=V3.SaveMesh(b.Build(name),"V8_"+name.Replace(' ','_'));var o=V3.Obj(name,mesh,mats,g);if(collision)o.AddComponent<MeshCollider>().sharedMesh=mesh;return o;}
    static Mesh rock;
    public static GameObject Rock(Vector3 p,Vector3 scale,Transform g,int seed=0,bool collider=false)
    {
        if(!rock){var mb=new V3.MB();int sides=9,rings=5;for(int i=0;i<rings;i++){float u=i/(float)(rings-1);float rr=Mathf.Sin(u*Mathf.PI)*.82f+.08f;for(int j=0;j<sides;j++){float a=j*Mathf.PI*2/sides;float bump=1+.19f*Mathf.Sin(j*4.1f+i*2.3f);mb.Vert(new Vector3(Mathf.Sin(a)*rr*bump,u-.14f,Mathf.Cos(a)*rr*bump),new Vector3(Mathf.Sin(a),.3f,Mathf.Cos(a)).normalized,Vector2.zero,Color.white);}}for(int i=0;i<rings-1;i++)for(int j=0;j<sides;j++){int a=i*sides+j,k=i*sides+(j+1)%sides;mb.Tri(0,a,k,a+sides);mb.Tri(0,k,k+sides,a+sides);}rock=V3.SaveMesh(mb.Build("Sculpted river stone"),"V8_RiverStone");rock.RecalculateNormals();}
        var o=V3.Obj("Weathered stone and moss",rock,new[]{Surface("River stones","#b1bac0","CliffStone",.55f)},g,p,Quaternion.Euler(0,seed*137.5f,0),scale);if(collider)o.AddComponent<MeshCollider>().sharedMesh=rock;return o;
    }
    public static Material Flowers=>LeafMat("Meadow border","MeadowFlowers","#ffffff");
    public static Material LeafMat(string name,string tex,string hex)
    {var m=V3.Mat("V8 "+name,"Aurelia/Natural Foliage");m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+tex+".png"));m.SetColor("_BaseColor",V3.C(hex));m.SetFloat("_Cutoff",.38f);m.SetFloat("_Wind",.8f);m.SetFloat("_Lift",.3f);return m;}
    public static void FlowerCard(V3.MB b,Vector3 p,float size,float yaw)
    {
        for(int k=0;k<2;k++){float a=yaw+k*Mathf.PI/2;var r=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*size*.70f;var up=Vector3.up*size;var col=new Color(1,1,1,.35f);b.Quad(0,p-r,p+r,p+r+up,p-r+up,Vector2.zero,Vector2.right,Vector2.one,Vector2.up,col,Vector3.up*.65f+Vector3.forward*.35f);}
    }
    public static GameObject Path(string name,List<Vector3> points,float width,Transform g,bool details=true)
    {
        var mb=new V3.MB();var flowers=new V3.MB();
        for(int i=0;i<points.Count;i++)
        {
            var p=points[i];var tangent=points[Mathf.Min(i+1,points.Count-1)]-points[Mathf.Max(0,i-1)];var right=Vector3.Cross(Vector3.up,tangent).normalized;float w=width*(1+.07f*Mathf.Sin(i*1.74f));
            mb.Vert(p-right*w*.5f+Vector3.up*.035f,Vector3.up,new Vector2(0,i*.7f),Color.white);mb.Vert(p+right*w*.5f+Vector3.up*.035f,Vector3.up,new Vector2(1,i*.7f),Color.white);
            if(i<points.Count-1){int a=i*2;mb.Tri(0,a,a+2,a+1);mb.Tri(0,a+1,a+2,a+3);}
            if(details&&i%2==0)foreach(float side in new[]{-1f,1f})
            {
                var q=p+right*side*(w*.5f+.5f);q.y=V3.H(q.x,q.z)+.02f;Rock(q,new Vector3(.5f+Random.value*.35f,.35f+Random.value*.3f,.5f+Random.value*.5f),g,i);
                for(int j=0;j<3;j++){var f=p+right*side*(w*.5f+.9f+j*.5f)+tangent.normalized*Random.Range(-1,1);f.y=V3.H(f.x,f.z);FlowerCard(flowers,f,Random.Range(.35f,.65f),i*.8f);}
            }
        }
        var o=Obj(name,mb,new[]{Surface("Warm worn earth path","#d2ba86","ground_dirt",.32f)},g,true);if(details)Obj(name+" flowering borders",flowers,new[]{Flowers},g);return o;
    }
}
