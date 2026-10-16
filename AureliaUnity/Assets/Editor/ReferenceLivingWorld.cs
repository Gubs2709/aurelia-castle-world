using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using Aurelia;
using Random=UnityEngine.Random;

public static class ReferenceLivingWorld
{
    const string D="Assets/LivingWorld";
    static Transform root,group;
    static Material bark,wood,stone,soil,flowerMaterial;
    static Material[] foliage;
    static Mesh[,] tree;
    static Mesh[] blooms;
    static int id;
    static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
    static Material Mat(string name,string shader,string color)
    {string p=D+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find(shader)){name=name};AssetDatabase.CreateAsset(m,p);}m.shader=Shader.Find(shader);m.SetColor("_BaseColor",C(color));m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
    static Transform Group(string name){var previous=GameObject.Find(name);if(previous)UnityEngine.Object.DestroyImmediate(previous);group=new GameObject(name).transform;group.parent=root;return group;}
    static Mesh Save(string name,List<Vector3> v,List<int> t,List<Vector2> uv=null,List<Color> colors=null)
    {var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);if(uv!=null)m.SetUVs(0,uv);if(colors!=null)m.SetColors(colors);m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,D+"/Meshes/"+(id++)+".asset");return m;}
    static GameObject Model(string name,Mesh mesh,Material m,Vector3 p,Quaternion q,Vector3 s,Transform parent=null)
    {var g=new GameObject(name);g.transform.SetParent(parent?parent:group,false);g.transform.localPosition=p;g.transform.localRotation=q;g.transform.localScale=s;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=m;return g;}
    static GameObject Box(string name,Vector3 p,Vector3 s,Material m,Transform parent=null)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent?parent:group,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static void Beam(string name,Vector3 a,Vector3 b,float width,Material m,Transform parent=null){var g=Box(name,(a+b)/2,new Vector3(width,(b-a).magnitude,width),m,parent);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d){int k=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
    public static float PathX(float z)=>-104+Mathf.Sin(z*.022f)*5;
    [MenuItem("Aurelia/Build living forests, gardens and traveller")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit play mode first");root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;
        Directory.CreateDirectory(D+"/Meshes");id=0;Random.InitState(22067);
        Materials();Templates();Forest();Gardens();Cottages();Bridge();Falls();Life();Player();Refine();Cabins();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("LIVING_WORLD_READY");
    }
    static void Materials()
    {
        AssetDatabase.Refresh();foreach(string n in new[]{"Cherry","Maple","Wisteria"}){var p="Assets/Art/Forest/"+n+".png";var imp=(TextureImporter)AssetImporter.GetAtPath(p);imp.alphaIsTransparency=true;imp.mipmapEnabled=true;imp.maxTextureSize=2048;imp.wrapMode=TextureWrapMode.Clamp;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.SaveAndReimport();}
        foliage=new Material[5];string[] paths={"Assets/Art/Realism/LeafBranch.png","Assets/Art/Forest/Cherry.png","Assets/Art/Forest/Maple.png","Assets/Art/Forest/Wisteria.png","Assets/Art/Realism/LeafBranch.png"};string[] colors={"#c7e0bd","#fff0fa","#cfbdad","#e6c6fa","#e4d298"};
        for(int i=0;i<5;i++){foliage[i]=Mat(new[]{"Summer oak","Pink cherry blossom","Crimson maple","Lavender wisteria","Golden woodland"}[i],"Aurelia/Natural Foliage",colors[i]);foliage[i].SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(paths[i]));foliage[i].SetFloat("_Cutoff",.32f);foliage[i].SetFloat("_AlphaClip",1);foliage[i].EnableKeyword("_ALPHATEST_ON");foliage[i].SetFloat("_Cull",0);}
        bark=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceRealism/Oak bark.mat");wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceRealism/Seasoned structural oak.mat");stone=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Cool ivory limestone.mat");soil=Mat("Garden earth","Universal Render Pipeline/Lit","#4e4935");flowerMaterial=Mat("Living coloured petals","Aurelia/Meadow","#ffffff");
    }
    static void Templates()
    {
        tree=new Mesh[5,2];var files=Directory.GetFiles("Assets/ReferenceRealism/Meshes","*.asset");
        for(int kind=0;kind<5;kind++)for(int part=0;part<2;part++)
        {
            string key=(part==0?"Oak branching wood ":"Oak natural canopy ")+(kind%6)+"_";var original=AssetDatabase.LoadAssetAtPath<Mesh>(files.First(p=>System.IO.Path.GetFileName(p).StartsWith(key)));var v=original.vertices;
            for(int i=0;i<v.Length;i++)
            {float y=v[i].y,bend=Mathf.Pow(Mathf.Max(0,y)/10,1.6f);float spread=kind==1?1.2f:kind==2?.92f:kind==3?1.12f:1.05f;v[i].x=v[i].x*spread+Mathf.Sin(y*.19f+kind)*bend*.8f;v[i].z*=spread;if(kind==1)v[i].y*=.86f;if(kind==3&&part==1)v[i].y-=Mathf.Abs(v[i].x)*.24f;}
            tree[kind,part]=Save("Mature tree type "+kind+" part "+part,v.ToList(),original.triangles.ToList(),original.uv.ToList());
        }
        blooms=Enumerable.Range(0,9).Select(FlowerMesh).ToArray();
    }
    static GameObject Tree(Vector3 p,float height,int kind,string name="Mature woodland tree")
    {
        var g=new GameObject(name+" • "+new[]{"oak","cherry","maple","wisteria","golden oak"}[kind]).transform;g.SetParent(group,false);g.localPosition=p;g.localRotation=Quaternion.Euler(0,Random.Range(0,360),0);float s=height/10;g.localScale=new Vector3(s*Random.Range(.9f,1.15f),s,s*Random.Range(.9f,1.15f));
        Model("Sculpted mature trunk",tree[kind,0],bark,Vector3.zero,Quaternion.identity,Vector3.one,g);Model("Layered natural canopy",tree[kind,1],foliage[kind],Vector3.zero,Quaternion.identity,Vector3.one,g);
        var collider=g.gameObject.AddComponent<CapsuleCollider>();collider.radius=.32f;collider.height=5.5f;collider.center=Vector3.up*2.75f;
        return g.gameObject;
    }
    static void Forest()
    {
        Group("17 • Towering mixed forests and blossom groves");
        var old=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Oak • irregular trunk and leaf clusters").ToArray();
        foreach(var tr in old)
        {var p=tr.position;int kind=p.x>50&&p.z<-20?1:p.x<-140?2:Random.value<.14f?4:0;float height=Random.Range(17,29);if(p.z<-230)height=Random.Range(12,22);if(Mathf.Abs(p.x)<55)height=Random.Range(11,17);UnityEngine.Object.DestroyImmediate(tr.gameObject);Tree(p,height,kind);}
        for(int side=-1;side<=1;side+=2)for(int i=0;i<23;i++)
        {
            float z=-221+i*8.1f+Random.Range(-2,2),x=PathX(z)+side*Random.Range(10,19);Tree(new Vector3(x,ReferenceCastleBuilder.Height(x,z),z),Random.Range(27,43),i%9==0?3:0,"Forest cathedral canopy");
        }
        for(int i=0;i<45;i++){float x=Random.Range(70,133),z=Random.Range(-180,10);if(Mathf.Abs(x-(18+Mathf.Sin((z+80)*.023f)*42))<35)continue;Tree(new Vector3(x,ReferenceCastleBuilder.Height(x,z),z),Random.Range(22,36),i%4==0?3:1,"Blossom grove");}
        for(int i=0;i<25;i++){float z=Random.Range(-175,55),x=Random.Range(-180,-139);Tree(new Vector3(x,ReferenceCastleBuilder.Height(x,z),z),Random.Range(25,40),i%5==0?4:2,"Crimson maple grove");}
        // A broad old shade tree, curved root buttresses and an actual hanging swing.
        var ancient=Tree(new Vector3(-45,ReferenceCastleBuilder.Height(-45,-227),-227),49,0,"Ancient riverside guardian").transform;
        for(int i=0;i<9;i++){float a=i*Mathf.PI*2/9;Beam("Spreading buttress root",new Vector3(0,.5f,0),new Vector3(Mathf.Sin(a)*1.2f,-.02f,Mathf.Cos(a)*1.2f),.16f,bark,ancient);}
        var swing=new GameObject("Hanging garden swing").transform;swing.parent=group;swing.position=ancient.position+new Vector3(7,2.3f,-2);Box("Worn wooden swing seat",Vector3.zero,new Vector3(2,.14f,.65f),wood,swing);foreach(float x in new[]{-.85f,.85f})Beam("Swing rope",new Vector3(x,0,0),new Vector3(x,17,0),.045f,wood,swing);
        swing.gameObject.AddComponent<WoodlandLife>().kind=WoodlandLife.Kind.Swing;Path();
    }
    static void Path()
    {
        var mat=Mat("Forest earth trail","Aurelia/Surface Detail","#b5a47d");mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Realism/Rock.png"));mat.SetFloat("_TextureStrength",.13f);mat.SetFloat("_Scale",.5f);
        var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<=90;i++){float z=-230+i*2.3f;float x=PathX(z);foreach(float s in new[]{-1f,1f})v.Add(new Vector3(x+s*2,ReferenceCastleBuilder.Height(x+s*2,z)+.04f,z));if(i<90){int k=i*2;t.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}Model("Winding forest walking trail",Save("Forest trail",v,t),mat,Vector3.zero,Quaternion.identity,Vector3.one);
    }
    static Mesh FlowerMesh(int kind)
    {
        var v=new List<Vector3>();var t=new List<int>();var c=new List<Color>();Color[] palette={C("#fff9e6"),C("#f4cb49"),C("#e682b3"),C("#9b7cda"),C("#648dcb"),C("#da5b60"),C("#e5a450"),C("#c3a4df"),C("#f3dfbd")};
        float height=kind==3||kind==7?1.15f:.5f;Quad(v,t,new Vector3(-.018f,0,0),new Vector3(.018f,0,0),new Vector3(.035f,height,0),new Vector3(.005f,height,0));while(c.Count<v.Count)c.Add(C("#4e823e"));
        for(int j=0;j<3;j++){float y=.12f+j*.16f,s=j%2==0?1:-1;Quad(v,t,new Vector3(0,y,0),new Vector3(s*.2f,y+.08f,.05f),new Vector3(s*.35f,y+.15f,0),new Vector3(s*.12f,y+.12f,-.05f));while(c.Count<v.Count)c.Add(C("#548640"));}
        int tiers=kind==3||kind==7?5:1;for(int tier=0;tier<tiers;tier++)for(int petal=0;petal<(tiers==1?8:5);petal++)
        {float angle=petal*Mathf.PI*2/(tiers==1?8:5)+tier*.7f;Vector3 center=new Vector3(.02f,height-tier*.13f,0);float r=tiers==1?.21f:.13f;Vector3 dir=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle)),side=Vector3.Cross(Vector3.up,dir);int n=v.Count;v.AddRange(new[]{center,center+dir*r*.55f-side*r*.36f+Vector3.up*.035f,center+dir*r+Vector3.up*.075f,center+dir*r*.55f+side*r*.36f+Vector3.up*.035f});t.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});while(c.Count<v.Count)c.Add(palette[kind]);}
        return Save("Living flower variety "+kind,v,t,null,c);
    }
    static void Bloom(Vector3 p,int kind,float size,Transform parent=null)=>Model("Flower • "+kind,blooms[kind],flowerMaterial,p,Quaternion.Euler(0,Random.Range(0,360),0),Vector3.one*size,parent);
    static void Bush(Vector3 p,float size,int kind,Transform parent=null)
    {Model("Flowering and leafy shrub",tree[kind,1],foliage[kind],p-Vector3.up*size*6,Quaternion.Euler(0,Random.Range(0,360),0),new Vector3(size,size*.85f,size),parent);}
    static void Gardens()
    {
        Group("18 • Layered undergrowth and multicolour wildflowers");Random.InitState(617);
        for(int i=0;i<600;i++)
        {float z=Random.Range(-230,65),x=Random.Range(-175,150);if(ReferenceCastleBuilder.Height(x,z)<2)continue;if(z<-30&&Mathf.Abs(x-PathX(z))<3.5f)continue;int kind=i%7==0?3:i%5==0?1:0;Bush(new Vector3(x,ReferenceCastleBuilder.Height(x,z)+.08f,z),Random.Range(.18f,.46f),kind);}
        for(int patch=0;patch<110;patch++)
        {
            float z=Random.Range(-285,65),x=Random.Range(-170,145);if(ReferenceCastleBuilder.Height(x,z)<2||Mathf.Abs(x-PathX(z))<2.4f)continue;int kind=patch%9;
            var combines=new List<CombineInstance>();for(int f=0;f<42;f++){float px=x+Random.Range(-3,3),pz=z+Random.Range(-3,3);float y=ReferenceCastleBuilder.Height(px,pz);if(y<2)continue;combines.Add(new CombineInstance{mesh=blooms[kind],transform=Matrix4x4.TRS(new Vector3(px,y+.03f,pz),Quaternion.Euler(0,Random.Range(0,360),0),Vector3.one*Random.Range(.55f,1.9f))});}
            var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combines.ToArray(),true,true);AssetDatabase.CreateAsset(mesh,D+"/Meshes/"+(id++)+".asset");Model("Wildflower drift • varied size and colour",mesh,flowerMaterial,Vector3.zero,Quaternion.identity,Vector3.one);
        }
    }
    static void Cottages()
    {
        Group("19 • Ivy balconies, flower boxes and cottage gardens");Random.InitState(991);var houses=GameObject.Find("13 • Inhabited timber and stone hamlets").transform;
        foreach(var house in houses.Cast<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")))
        {
            var body=house.Find("Solid lime plaster walls");float w=body.localScale.x,d=body.localScale.z,h=body.localScale.y;
            var garden=new GameObject("Ivy and cottage garden").transform;garden.parent=group;garden.SetPositionAndRotation(house.position,house.rotation);
            for(int i=0;i<20;i++){float side=i%2==0?-1:1;float y=Random.Range(.4f,h+1);Bush(new Vector3(side*w*.48f,y,-d/2-.1f),.1f,i%4==0?3:0,garden);}
            foreach(float x in new[]{-w*.3f,w*.3f})
            {var box=Box("Planted timber window box",new Vector3(x,1.7f,-d/2-.4f),new Vector3(1.45f,.28f,.45f),wood,garden);for(int j=0;j<7;j++)Bloom(new Vector3(x+(j-3)*.17f,1.88f,-d/2-.42f),j%2==0?2:0,.65f,garden);}
            if(h>6)
            {
                Box("Projecting cottage balcony",new Vector3(0,4.35f,-d/2-.85f),new Vector3(3.3f,.22f,1.65f),wood,garden);
                for(int i=0;i<12;i++)Box("Balcony spindle",new Vector3(-1.5f+i*.27f,4.9f,-d/2-1.55f),new Vector3(.07f,1.1f,.07f),wood,garden);
                Box("Balcony handrail",new Vector3(0,5.5f,-d/2-1.55f),new Vector3(3.4f,.1f,.12f),wood,garden);
                for(int i=0;i<13;i++)Bloom(new Vector3(-1.4f+i*.23f,4.6f,-d/2-1.65f),i%3==0?3:2,.9f,garden);
            }
            for(int i=0;i<9;i++){float x=Random.Range(-w,w),z=-d/2-Random.Range(2,5);Vector3 world=house.TransformPoint(new Vector3(x,0,z));float y=ReferenceCastleBuilder.Height(world.x,world.z)-house.position.y;Bush(new Vector3(x,y+.1f,z),Random.Range(.17f,.3f),i%4==0?1:0,garden);for(int j=0;j<6;j++)Bloom(new Vector3(x+Random.Range(-.7f,.7f),y+.1f,z+Random.Range(-.5f,.5f)),(i+j)%9,Random.Range(.65f,1.3f),garden);}
            Combine(garden);
        }
    }
    static void Bridge()
    {
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Open stone bridge arch"||t.name=="Bridge stone pier").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        Group("20 • True vaulted masonry bridge");Vector3 a=new Vector3(50,12,-245),b=new Vector3(0,12,64),dir=(b-a).normalized,right=Vector3.Cross(Vector3.up,dir);float span=(b-a).magnitude/13;
        for(int i=0;i<13;i++)
        {
            var v=new List<Vector3>();var t=new List<int>();Vector3 centre=Vector3.Lerp(a,b,(i+.5f)/13f);centre.y=2;
            for(int k=0;k<24;k++)
            {float u=k*Mathf.PI/24,w=(k+1)*Mathf.PI/24;Vector3 ia=centre+dir*(Mathf.Cos(u)*(span*.45f))+Vector3.up*(Mathf.Sin(u)*8.3f),ib=centre+dir*(Mathf.Cos(w)*(span*.45f))+Vector3.up*(Mathf.Sin(w)*8.3f),oa=centre+dir*(Mathf.Cos(u)*(span*.49f))+Vector3.up*(Mathf.Sin(u)*9.8f),ob=centre+dir*(Mathf.Cos(w)*(span*.49f))+Vector3.up*(Mathf.Sin(w)*9.8f);
                foreach(float side in new[]{-1f,1f}){Quad(v,t,ia+right*side*6.3f,ib+right*side*6.3f,ob+right*side*6.3f,oa+right*side*6.3f);var topA=oa;var topB=ob;topA.y=12;topB.y=12;Quad(v,t,oa+right*side*6.3f,ob+right*side*6.3f,topB+right*side*6.3f,topA+right*side*6.3f);}Quad(v,t,ia-right*6.3f,ib-right*6.3f,ib+right*6.3f,ia+right*6.3f);
            }
            var archMat=new Material(stone);archMat.SetFloat("_Cull",0);AssetDatabase.CreateAsset(archMat,D+"/Arch "+i+".mat");Model("Elliptical stone vault and spandrels",Save("Bridge true vault "+i,v,t),archMat,Vector3.zero,Quaternion.identity,Vector3.one);
            Vector3 pier=Vector3.Lerp(a,b,i/13f);pier.y=6;var g=Box("Tapered viaduct pier",pier,new Vector3(13,12,span*.1f),stone);g.transform.rotation=Quaternion.LookRotation(dir);
        }
    }
    static void Falls()
    {
        Group("21 • Powerful castle cascades and luminous whitewater");var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/White flowing cascade.mat");m.SetColor("_BaseColor",new Color(.7f,.93f,1));EditorUtility.SetDirty(m);
        foreach(var f in root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Broad falling water curtain"))
        {var mesh=f.sharedMesh;var p=mesh.vertices;var bounds=mesh.bounds;for(int i=0;i<p.Length;i++){float u=(p[i].y-bounds.min.y)/bounds.size.y;p[i].x=bounds.center.x+(p[i].x-bounds.center.x)*1.35f;p[i].z-=Mathf.Pow(1-u,3)*4;}mesh.vertices=p;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);}
        foreach(var ps in root.GetComponentsInChildren<ParticleSystem>()){ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.startSize=new ParticleSystem.MinMaxCurve(4,9);main.startColor=new Color(.85f,.95f,1,.75f);var em=ps.emission;em.rateOverTime=14;}
        var water=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Turquoise river.mat");water.SetColor("_BaseColor",C("#126d75"));water.SetColor("_ShallowColor",C("#63cbb2"));EditorUtility.SetDirty(water);
        var foam=Mat("Bright impact whitewater","Aurelia/Living Whitewater","#ffffff");foreach(float x in new[]{-106f,-83,-60,-39,37,60,83,106})
        {var v=new List<Vector3>();var t=new List<int>();Quad(v,t,new Vector3(-12,0,-8),new Vector3(-12,0,8),new Vector3(12,0,8),new Vector3(12,0,-8));Model("Surging waterfall impact foam",Save("Impact pool foam",v,t,new List<Vector2>{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)}),foam,new Vector3(x,1.82f,70+Mathf.Abs(x)*.06f),Quaternion.identity,Vector3.one);}
    }
    static void Player()
    {
        var current=EditorSceneManager.GetActiveScene();var controller=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();var walker=controller.walker;
        var old=walker.Find("Restored animated traveller");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Aurelia.unity",OpenSceneMode.Additive);var explorer=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ValleyExplorer>(true)).First();var avatar=UnityEngine.Object.Instantiate(explorer.avatar.gameObject);avatar.name="Restored animated traveller";UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(avatar,current);avatar.transform.SetParent(walker,false);avatar.transform.localPosition=Vector3.zero;avatar.transform.localRotation=Quaternion.identity;
        EditorSceneManager.CloseScene(scene,true);EditorSceneManager.SetActiveScene(current);
        var bounds=new Bounds(walker.position,Vector3.zero);foreach(var r in avatar.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);avatar.transform.localScale*=1.95f/Mathf.Max(.01f,bounds.size.y);foreach(var tr in walker.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=2;
        controller.avatar=avatar.transform;controller.startWithPlayer=true;controller.forestPoint=new Vector3(PathX(-169),ReferenceCastleBuilder.Height(PathX(-169),-169)+.15f,-169);controller.spawn=controller.forestPoint;controller.blossomPoint=new Vector3(111,ReferenceCastleBuilder.Height(111,-98)+.15f,-98);controller.cottagePoint=new Vector3(-145,ReferenceCastleBuilder.Height(-145,-100)+.15f,-100);controller.waterfallPoint=new Vector3(-29,ReferenceCastleBuilder.Height(-29,35)+.15f,35);controller.WalkAt(controller.spawn);EditorUtility.SetDirty(controller);
    }
    static void Combine(Transform source)
    {
        var batch=new GameObject("Garden material batches").transform;batch.SetParent(source,false);
        foreach(var set in source.GetComponentsInChildren<MeshFilter>().GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial).ToArray())
        {var m=new Mesh{indexFormat=IndexFormat.UInt32};m.CombineMeshes(set.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=batch.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray(),true,true);AssetDatabase.CreateAsset(m,D+"/Meshes/"+(id++)+".asset");Model("Garden flowers, ivy and joinery",m,set.Key,Vector3.zero,Quaternion.identity,Vector3.one,batch);foreach(var f in set)f.GetComponent<MeshRenderer>().enabled=false;}
    }
    static void Life()
    {
        Group("22 • Falling petals and woodland butterflies");var petalMat=Mat("Airborne pink petals","Universal Render Pipeline/Lit","#edb2cc");petalMat.SetFloat("_Cull",0);var v=new List<Vector3>{new Vector3(-.5f,0,0),new Vector3(0,.15f,.65f),new Vector3(.5f,0,0),new Vector3(0,-.05f,-.4f)};var petal=Save("Curved blossom petal",v,new List<int>{0,1,2,0,2,3});
        foreach(var p in new[]{new Vector3(108,18,-100),new Vector3(-145,17,-90),new Vector3(-45,20,-227)})
        {var g=new GameObject("Drifting blossom petals");g.transform.parent=group;g.transform.position=p;var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.loop=true;main.startLifetime=new ParticleSystem.MinMaxCurve(12,18);main.startSpeed=.1f;main.startSize=new ParticleSystem.MinMaxCurve(.07f,.16f);main.maxParticles=260;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=.025f;var em=ps.emission;em.rateOverTime=10;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(32,6,40);var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.x=.45f;velocity.y=-.5f;velocity.z=.2f;var rot=ps.rotationOverLifetime;rot.enabled=true;rot.z=1.4f;var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=petal;renderer.sharedMaterial=petalMat;}
        for(int i=0;i<18;i++){float z=Random.Range(-220,-50),x=PathX(z)+Random.Range(-7,7);var g=Model("Woodland butterfly",petal,foliage[i%5],new Vector3(x,ReferenceCastleBuilder.Height(x,z)+1.3f,z),Quaternion.identity,new Vector3(.15f,.15f,.15f));var life=g.AddComponent<WoodlandLife>();life.kind=WoodlandLife.Kind.Butterfly;life.seed=i;}
    }
    public static void Refine()
    {
        root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;id=10000;
        var houses=GameObject.Find("13 • Inhabited timber and stone hamlets").transform.Cast<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")).ToArray();
        var retained=new List<Vector3>();foreach(var tree in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Mature woodland tree")).ToArray())
        {var p=tree.position;bool houseClear=houses.Any(h=>new Vector2(h.position.x-p.x,h.position.z-p.z).magnitude<10);bool pathClear=p.z<-30&&p.z>-230&&Mathf.Abs(p.x-PathX(p.z))<24;bool dense=retained.Any(q=>new Vector2(q.x-p.x,q.z-p.z).magnitude<10);if(houseClear||pathClear||dense)UnityEngine.Object.DestroyImmediate(tree.gameObject);else retained.Add(p);}
        bark=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceRealism/Oak bark.mat");bark.shader=Shader.Find("Aurelia/Surface Detail");bark.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Realism/Rock.png"));bark.SetColor("_BaseColor",C("#b8a485"));bark.SetFloat("_Scale",.35f);bark.SetFloat("_Relief",.12f);EditorUtility.SetDirty(bark);
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Fine mixed meadow grass blades"||t.name=="White daisies, pink flowers and buttercups").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        var controller=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();var rs=controller.avatar.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);controller.avatar.position+=Vector3.up*(controller.walker.position.y-bounds.min.y);
        Group("23 • Soft curved forest grasses and flowering path edges");flowerMaterial=AssetDatabase.LoadAssetAtPath<Material>(D+"/Living coloured petals.mat");Random.InitState(228);
        var grass=Mat("Fine woodland grass","Aurelia/Meadow","#ffffff");var vtx=new List<Vector3>();var tri=new List<int>();var colors=new List<Color>();
        for(int i=0;i<26000;i++)
        {
            float z=Random.Range(-235,-20),x=PathX(z)+Random.Range(-28,28);if(Mathf.Abs(x-PathX(z))<2.25f)continue;float y=ReferenceCastleBuilder.Height(x,z),h=Random.Range(.14f,.38f);Vector3 p=new Vector3(x,y+.02f,z),side=Quaternion.Euler(0,Random.Range(0,360),0)*Vector3.right*.018f;int n=vtx.Count;
            for(int j=0;j<4;j++){float u=j/3f;Vector3 c=p+Vector3.up*(h*u)+Vector3.right*(u*u*.12f);vtx.Add(c-side*(1-u));vtx.Add(c+side*(1-u));if(j<3){int k=n+j*2;tri.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}colors.Add(C("#6b963e"));colors.Add(C("#9ab95b"));}
        }
        Model("Fine curved forest grass",Save("Curved grass blades",vtx,tri,null,colors),grass,Vector3.zero,Quaternion.identity,Vector3.one);
        blooms=new Mesh[9];for(int i=0;i<9;i++)blooms[i]=AssetDatabase.LoadAssetAtPath<Mesh>(D+"/Meshes/"+(10+i)+".asset");
        for(int side=-1;side<=1;side+=2)for(int i=0;i<380;i++){float z=Random.Range(-223,-35),x=PathX(z)+side*Random.Range(2.6f,5.3f);Bloom(new Vector3(x,ReferenceCastleBuilder.Height(x,z)+.03f,z),i%9,Random.Range(.65f,1.25f));}
        Combine(group);controller.WalkAt(controller.spawn);EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    public static void Capture()
    {
        var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();string dir="D:/GAME/artifacts/living-world";Directory.CreateDirectory(dir);
        c.WalkAt(c.forestPoint);Shot(dir+"/forest-player.png");c.WalkAt(c.blossomPoint);c.Orbit(-35,-3);c.RefreshCamera();Shot(dir+"/blossom-grove.png");c.WalkAt(c.cottagePoint);Shot(dir+"/cottage-gardens.png");c.WalkAt(c.waterfallPoint);c.Orbit(-20,-10);c.RefreshCamera();Shot(dir+"/castle-cascades.png");c.Picture();Shot(dir+"/kingdom.png");c.WalkAt(c.spawn);
    }
    static void Shot(string path)
    {var cam=Camera.main;var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var texture=new Texture2D(1600,1000,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,1000),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(rt);}
    public static void Validate()
    {
        ReferenceCastleBuilder.Validate();var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();var report=new System.Text.StringBuilder(File.ReadAllText("D:/GAME/artifacts/reference-castle/editor-validation.txt"));
        var trees=UnityEngine.Object.FindObjectsByType<CapsuleCollider>().Where(t=>t.name.Contains("•")).ToArray();int high=trees.Count(t=>{var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b.size.y>20;});report.AppendLine((high>100?"PASS":"FAIL")+" mature trees over 20m: "+high+"; total="+trees.Length);report.AppendLine((c.avatar&&c.avatar.GetComponentInChildren<Animator>()&&c.startWithPlayer?"PASS":"FAIL")+" visible animated third-person traveller is default");
        int covered=0;var crowns=UnityEngine.Object.FindObjectsByType<MeshRenderer>().Where(r=>r.name=="Layered natural canopy").ToArray();for(int i=0;i<30;i++){float z=-215+i*5.7f,x=PathX(z);if(crowns.Any(r=>r.bounds.min.x<x&&r.bounds.max.x>x&&r.bounds.min.z<z&&r.bounds.max.z>z&&r.bounds.max.y>ReferenceCastleBuilder.Height(x,z)+10))covered++;}report.AppendLine((covered>23?"PASS":"FAIL")+" forest path canopy bounds above traveller: "+covered+" / 30 samples");
        Directory.CreateDirectory("D:/GAME/artifacts/living-world");File.WriteAllText("D:/GAME/artifacts/living-world/editor-validation.txt",report.ToString());Debug.Log(report.ToString());
    }
    public static void Cabins()
    {
        root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;id=12000;wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceRealism/Seasoned structural oak.mat");var houses=GameObject.Find("13 • Inhabited timber and stone hamlets").transform;
        int count=0;foreach(var house in houses.Cast<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")).OrderBy(t=>t.position.z).ToArray())
        {
            var body=house.Find("Solid lime plaster walls");if(!body||house.Find("Cabin rounded log cladding")||body.localScale.y>6||count>=7)continue;count++;float w=body.localScale.x,h=body.localScale.y,d=body.localScale.z;body.GetComponent<Renderer>().sharedMaterial=wood;
            for(float y=1;y<h+.7f;y+=.28f)foreach(float side in new[]{-1f,1f})
            {
                foreach(bool front in new[]{true,false})
                {var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name="Cabin rounded log cladding";g.transform.SetParent(house,false);g.transform.localPosition=front?new Vector3(0,y,side*(d/2+.02f)):new Vector3(side*(w/2+.02f),y,0);g.transform.localScale=new Vector3(.19f,(front?w:d)/2+.14f,.19f);g.transform.localRotation=Quaternion.Euler(front?0:90,0,front?90:0);g.GetComponent<Renderer>().sharedMaterial=wood;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}
            }
        }
        var old=houses.Find("Cottage material batches");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var old2=houses.Find("Garden material batches");if(old2)UnityEngine.Object.DestroyImmediate(old2.gameObject);foreach(var r in houses.GetComponentsInChildren<MeshRenderer>())r.enabled=true;group=houses;Combine(houses);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
}
