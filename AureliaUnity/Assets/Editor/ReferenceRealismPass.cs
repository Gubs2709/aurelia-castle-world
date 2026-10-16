using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Aurelia;
using Random=UnityEngine.Random;

// Material and model revision requested after the first reference scene review.
// All assets are original generated textures, procedural meshes, or existing project assets.
public static class ReferenceRealismPass
{
    const string D="Assets/ReferenceRealism",A="Assets/Art/Realism";
    static Transform root,parent;
    static Material limestone,trim,rock,wood,plaster,slate,glass,leaves,bark,iron;
    static int serial;
    static Color C(string h){ColorUtility.TryParseHtmlString(h,out var c);return c;}
    static Material Mat(string name,string shader,string color)
    {var p=D+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find(shader)){name=name};AssetDatabase.CreateAsset(m,p);}m.shader=Shader.Find(shader);m.SetColor("_BaseColor",C(color));m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
    static Transform Group(string name){var g=GameObject.Find(name);if(g)UnityEngine.Object.DestroyImmediate(g);parent=new GameObject(name).transform;parent.parent=root;return parent;}
    static Mesh Save(Mesh m,string name){m.name=name;m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,D+"/Meshes/"+name+"_"+(serial++)+".asset");return m;}
    static GameObject Model(string name,Mesh mesh,Material mat,Transform p,Vector3 pos,Quaternion rotation,Vector3 scale)
    {var g=new GameObject(name);g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localRotation=rotation;g.transform.localScale=scale;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;}
    static GameObject Box(string name,Vector3 p,Vector3 s,Material m,bool collision=false,Quaternion rotation=default)
    {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=s;g.transform.localRotation=rotation==default?Quaternion.identity:rotation;g.GetComponent<Renderer>().sharedMaterial=m;if(!collision)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    static void Beam(string name,Vector3 a,Vector3 b,float width,Material m)=>Box(name,(a+b)/2,new Vector3(width,(b-a).magnitude,width),m,false,Quaternion.FromToRotation(Vector3.up,b-a));
    static void Quad(List<Vector3> v,List<int> t,List<Vector2> uv,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
    {int n=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});uv?.AddRange(new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)});}
    static void Texture(Material m,string file,float scale=.25f,float relief=.06f)
    {m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(A+"/"+file+".png"));m.SetFloat("_Scale",scale);m.SetFloat("_Relief",relief);m.SetFloat("_Smoothness",.08f);EditorUtility.SetDirty(m);}
    static void ImportTextures()
    {
        AssetDatabase.Refresh();foreach(string name in new[]{"Cloud","LeafBranch","Limestone","Rock"})
        {var path=A+"/"+name+".png";var imp=(TextureImporter)AssetImporter.GetAtPath(path);if(imp==null)throw new Exception("Missing generated texture "+path);imp.maxTextureSize=2048;imp.mipmapEnabled=true;imp.sRGBTexture=true;imp.alphaIsTransparency=name=="Cloud"||name=="LeafBranch";imp.wrapMode=imp.alphaIsTransparency?TextureWrapMode.Clamp:TextureWrapMode.Repeat;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.anisoLevel=8;imp.SaveAndReimport();}
    }
    [MenuItem("Aurelia/Apply natural materials and living sky")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play mode first");
        root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;
        Directory.CreateDirectory(D+"/Meshes");serial=0;Random.InitState(7139);ImportTextures();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/ReferenceCastle-before-realism.unity",true);
        Materials();MovingClouds();Trees();Rocks();Houses();Windows();Water();WaterMist();
        ReferenceCastleBuilder.CloseRoofSeams();CloudAndBanks();ArchitectureAndFields();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        Debug.Log("REFERENCE_REALISM_PASS_COMPLETE");
    }
    static void Materials()
    {
        limestone=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Cool ivory limestone.mat");limestone.shader=Shader.Find("Aurelia/Surface Detail");limestone.color=C("#edece1");Texture(limestone,"Limestone",.25f,.13f);
        trim=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Sunlit carved limestone.mat");trim.shader=Shader.Find("Aurelia/Surface Detail");trim.color=C("#f0e9d5");Texture(trim,"Limestone",.32f,.045f);trim.SetFloat("_TextureStrength",.33f);
        rock=Mat("Fractured limestone and lichen","Aurelia/Surface Detail","#acb6af");Texture(rock,"Rock",.18f,.2f);
        plaster=Mat("Weathered lime plaster","Aurelia/Surface Detail","#c9c2a9");Texture(plaster,"Rock",.6f,.09f);plaster.SetFloat("_TextureStrength",.16f);
        wood=Mat("Seasoned structural oak","Aurelia/Surface Detail","#504337");Texture(wood,"Rock",.8f,.08f);wood.SetFloat("_TextureStrength",.4f);
        slate=Mat("Natural blue grey slate","Aurelia/Surface Detail","#54737b");Texture(slate,"Rock",.65f,.06f);
        var castleRoof=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Sea glass turquoise slates.mat");castleRoof.shader=Shader.Find("Aurelia/Surface Detail");castleRoof.SetColor("_BaseColor",C("#7aabad"));Texture(castleRoof,"Rock",.55f,.045f);
        glass=Mat("Leaded reflective blue glass","Aurelia/Leaded Glass","#203a3e");iron=Mat("Wrought iron","Universal Render Pipeline/Lit","#333b37");iron.SetFloat("_Metallic",.7f);iron.SetFloat("_Smoothness",.3f);
        leaves=Mat("Natural oak leaf clusters","Aurelia/Natural Foliage","#c1d3bd");leaves.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(A+"/LeafBranch.png"));leaves.SetFloat("_Cutoff",.38f);leaves.SetFloat("_AlphaClip",1);leaves.EnableKeyword("_ALPHATEST_ON");leaves.SetFloat("_Cull",0);
        bark=Mat("Oak bark","Universal Render Pipeline/Lit","#6b6656");bark.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/StylizedNature/NormalTree_Bark.png"));bark.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/StylizedNature/NormalTree_Bark_Normal.png"));bark.EnableKeyword("_NORMALMAP");bark.SetFloat("_BumpScale",1.4f);bark.SetFloat("_Smoothness",.08f);
        var terrain=Mat("Grass and exposed rock landscape","Aurelia/Surface Detail","#c0c5b4");Texture(terrain,"Rock",.13f,.16f);terrain.SetFloat("_Terrain",1);GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<Renderer>().sharedMaterial=terrain;
        // The same natural stone detail now covers the bridge and palace, instead of a drawn grid.
        foreach(var r in root.GetComponentsInChildren<Renderer>())if(r.sharedMaterial&&r.sharedMaterial.name=="Stratified grey cliff")r.sharedMaterial=rock;
    }
    static void MovingClouds()
    {
        Group("10 • Drifting cumulus towers and smaller clouds");var mat=Mat("Living cloud billows","Aurelia/Drifting Cloud","#ffffff");mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(A+"/Cloud.png"));
        var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();Quad(v,t,uv,new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0));var mesh=Save(new Mesh{vertices=v.ToArray(),triangles=t.ToArray(),uv=uv.ToArray()},"Cloud card");
        Vector4[] towers={new Vector4(-255,226,470,1.12f),new Vector4(266,190,560,1.03f),new Vector4(-540,170,850,.8f),new Vector4(545,140,940,.9f)};
        for(int i=0;i<towers.Length;i++){var e=towers[i];var g=Model("Slow towering cumulus "+i,mesh,mat,parent,new Vector3(e.x,e.y,e.z),Quaternion.identity,new Vector3(240*e.w,390*e.w,1));var d=g.AddComponent<CloudDrift>();d.speed=.55f+i*.08f;d.phase=i*2;}
        for(int i=0;i<17;i++){float z=Random.Range(630,1050);var g=Model("Small wind driven cloud "+i,mesh,mat,parent,new Vector3(Random.Range(-850,850),Random.Range(170,365),z),Quaternion.Euler(0,0,Random.Range(-7,7)),new Vector3(Random.Range(65,160),Random.Range(24,62),1));var d=g.AddComponent<CloudDrift>();d.speed=Random.Range(1.2f,2.2f);d.phase=i;d.billow=.3f;}
    }
    static void Tube(List<Vector3> v,List<int> t,List<Vector2> uv,Vector3 a,Vector3 b,float ra,float rb,int sides=9)
    {
        var rot=Quaternion.FromToRotation(Vector3.up,b-a);int n=v.Count;for(int j=0;j<2;j++)for(int i=0;i<=sides;i++){float angle=i*Mathf.PI*2/sides;v.Add((j==0?a:b)+rot*new Vector3(Mathf.Sin(angle)*(j==0?ra:rb),0,Mathf.Cos(angle)*(j==0?ra:rb)));uv.Add(new Vector2(i/(float)sides*2,j*(a-b).magnitude*.5f));}
        for(int i=0;i<sides;i++){int k=n+i;t.AddRange(new[]{k,k+1,k+sides+1,k+1,k+sides+2,k+sides+1});}
    }
    static Mesh[] TreeMeshes(int variant)
    {
        Random.InitState(719+variant*71);var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();var leafV=new List<Vector3>();var leafT=new List<int>();var leafUV=new List<Vector2>();
        Vector3 top=new Vector3(Random.Range(-.4f,.4f),7.5f,Random.Range(-.4f,.4f));Tube(v,t,uv,Vector3.zero,top,.42f,.09f,12);
        for(int b=0;b<16;b++)
        {
            float angle=b*2.4f+variant,y=2.7f+b*.24f,r=Random.Range(2.1f,3.7f);Vector3 a=new Vector3(0,y,0),end=new Vector3(Mathf.Sin(angle)*r,Random.Range(6.2f,9.6f),Mathf.Cos(angle)*r);Vector3 mid=Vector3.Lerp(a,end,.6f)+Vector3.up*.35f;Tube(v,t,uv,a,mid,.14f,.065f);Tube(v,t,uv,mid,end,.065f,.014f);
            for(int j=0;j<3;j++)
            {Vector3 twig=Vector3.Lerp(mid,end,j/3f)+new Vector3(Random.Range(-.6f,.6f),Random.Range(.3f,1.4f),Random.Range(-.6f,.6f));Tube(v,t,uv,Vector3.Lerp(a,end,.7f),twig,.035f,.008f,6);
                for(int k=0;k<5;k++)
                {Vector3 p=twig+Random.insideUnitSphere*.65f;Quaternion q=Quaternion.Euler(Random.Range(-65,65),Random.Range(0,360),Random.Range(-40,40));float s=Random.Range(1.1f,1.75f);Quad(leafV,leafT,leafUV,p+q*new Vector3(-s/2,-s/2,0),p+q*new Vector3(s/2,-s/2,0),p+q*new Vector3(s/2,s/2,0),p+q*new Vector3(-s/2,s/2,0));}
            }
        }
        return new[]{Save(new Mesh{vertices=v.ToArray(),triangles=t.ToArray(),uv=uv.ToArray()},"Oak branching wood "+variant),Save(new Mesh{vertices=leafV.ToArray(),triangles=leafT.ToArray(),uv=leafUV.ToArray()},"Oak natural canopy "+variant)};
    }
    static void Trees()
    {
        var old=root.GetComponentsInChildren<Transform>().Where(t=>((t.name.StartsWith("NormalTree_")||t.name.StartsWith("BirchTree_"))&&PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject))||t.name=="Oak • irregular trunk and leaf clusters").ToArray();
        var sites=old.Select(t=>{var rs=t.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return new Vector4(t.position.x,t.position.y,t.position.z,Mathf.Clamp(b.size.y,6,15));}).ToArray();foreach(var tr in old)UnityEngine.Object.DestroyImmediate(tr.gameObject);
        Group("11 • Natural branching oak woodland");var variants=Enumerable.Range(0,6).Select(TreeMeshes).ToArray();Random.InitState(931);
        for(int i=0;i<sites.Length;i++)
        {var s=sites[i];float y=ReferenceCastleBuilder.Height(s.x,s.z);if(y<2)continue;var g=new GameObject("Oak • irregular trunk and leaf clusters");g.transform.SetParent(parent,false);g.transform.localPosition=new Vector3(s.x,y-.1f,s.z);g.transform.localRotation=Quaternion.Euler(0,Random.Range(0,360),0);g.transform.localScale=Vector3.one*s.w/10;var m=variants[i%6];Model("Branching oak trunk",m[0],bark,g.transform,Vector3.zero,Quaternion.identity,Vector3.one);Model("Wind moved oak foliage",m[1],leaves,g.transform,Vector3.zero,Quaternion.identity,Vector3.one);}
    }
    static Mesh RockMesh(int variant)
    {
        var v=new List<Vector3>();var t=new List<int>();int n=12;Random.InitState(75+variant);Vector3 shift=new Vector3(Random.value*17,Random.value*17,Random.value*17);
        for(int face=0;face<6;face++)
        {int start=v.Count;for(int y=0;y<=n;y++)for(int x=0;x<=n;x++){float u=x*2f/n-1,w=y*2f/n-1;Vector3 p=face==0?new Vector3(1,u,w):face==1?new Vector3(-1,w,u):face==2?new Vector3(w,1,u):face==3?new Vector3(u,-1,w):face==4?new Vector3(u,w,1):new Vector3(w,u,-1);p=p.normalized;float noise=Mathf.PerlinNoise(p.x*2.8f+shift.x,p.y*2.3f+p.z+shift.z);float radius=.77f+noise*.4f+Mathf.Abs(Mathf.Sin(p.x*6+p.z*4+variant))*.07f;p*=radius;p.x+=p.y*.12f;p.y=Mathf.Max(.03f,(p.y+1)*.5f);v.Add(p);if(x<n&&y<n){int k=start+y*(n+1)+x;t.AddRange(new[]{k,k+1,k+n+1,k+1,k+n+2,k+n+1});}}}
        var mesh=new Mesh{vertices=v.ToArray(),triangles=t.ToArray()};mesh.RecalculateNormals();if(Vector3.Dot(mesh.normals[0],mesh.vertices[0]-new Vector3(0,.5f,0))<0){var tr=mesh.triangles;for(int i=0;i<tr.Length;i+=3)(tr[i+1],tr[i+2])=(tr[i+2],tr[i+1]);mesh.triangles=tr;}return Save(mesh,"Eroded fractured boulder "+variant);
    }
    static void Rocks()
    {
        var old=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Overlook weathered boulder"||t.name=="Flower overlook framing rock"||t.name=="Weathered natural overlook rock").ToArray();var sites=old.Select(t=>{var b=t.GetComponent<Renderer>().bounds;return (p:t.position,size:b.size);}).ToArray();foreach(var g in old)UnityEngine.Object.DestroyImmediate(g.gameObject);
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Eroded cliff buttress").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        Group("12 • Fractured rock outcrops and escarpment");var meshes=Enumerable.Range(0,6).Select(RockMesh).ToArray();Random.InitState(172);
        for(int i=0;i<sites.Length;i++){var s=sites[i];var p=s.p;p.y=ReferenceCastleBuilder.Height(p.x,p.z)-.4f;Model("Weathered natural overlook rock",meshes[i%6],rock,parent,p,Quaternion.Euler(0,Random.Range(0,360),0),new Vector3(s.size.x*.5f,s.size.y,s.size.z*.5f));}
        float[] falls={-106,-83,-60,-39,37,60,83,106};for(int i=0;i<55;i++){float x=Random.Range(-120,120);if(Mathf.Abs(x)<20||falls.Any(f=>Mathf.Abs(f-x)<4.7f))continue;float h=Random.Range(14,38),y=Random.Range(2,62-h);Model("Stratified cliff outcrop",meshes[i%6],rock,parent,new Vector3(x,y,86),Quaternion.Euler(0,Random.Range(-20,20),0),new Vector3(Random.Range(4,9),h,Random.Range(4,8)));}
    }
    static void Combine(Transform source,string name)
    {
        var batch=new GameObject(name).transform;batch.parent=source;
        foreach(var set in source.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<MeshRenderer>()&&f.GetComponent<MeshRenderer>().enabled).GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial).ToArray())
        {var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(set.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=batch.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray(),true,true);Save(mesh,name+" "+set.Key.name);Model(set.Key.name,mesh,set.Key,batch,Vector3.zero,Quaternion.identity,Vector3.one);foreach(var f in set)f.GetComponent<MeshRenderer>().enabled=false;}
    }
    static void Houses()
    {
        var sites=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Valley farmhouse").Select(t=>new Vector3(t.position.x,ReferenceCastleBuilder.Height(t.position.x,t.position.z),t.position.z)).ToArray();
        if(sites.Length==0)sites=root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")).Select(t=>t.position).ToArray();
        var obsolete=new[]{"Valley farmhouse","Timber farmhouse upright","Oak farmhouse door","Farmhouse pitched roof","Stone chimney"};foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>obsolete.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        Transform houses=Group("13 • Inhabited timber and stone hamlets");Random.InitState(814);
        for(int i=0;i<sites.Length;i++)
        {
            var p=sites[i];if(ReferenceCastleBuilder.Height(p.x,p.z)<2)continue;var house=new GameObject("Inhabited valley cottage "+i).transform;house.parent=houses;house.position=p;house.rotation=Quaternion.Euler(0,Random.Range(-22,22),0);parent=house;
            float w=Random.Range(5.5f,8.5f),d=Random.Range(5,8),h=i%4==0?7.2f:4.5f,roofH=3.6f;
            Box("Rubble foundation",new Vector3(0,.4f,0),new Vector3(w+.3f,.8f,d+.3f),limestone,true);
            Box("Solid lime plaster walls",new Vector3(0,.8f+h/2,0),new Vector3(w,h,d),plaster,true);
            float eave=h+.8f;
            // Solid gable ends close the roof volume, including its sides and ridge.
            var v=new List<Vector3>();var t=new List<int>();foreach(float side in new[]{-1f,1f})
            {int n=v.Count;v.AddRange(new[]{new Vector3(-w/2,eave,side*d/2),new Vector3(w/2,eave,side*d/2),new Vector3(0,eave+roofH,side*d/2)});t.AddRange(side<0?new[]{n,n+2,n+1}:new[]{n,n+1,n+2});}
            Model("Plastered gable ends",Save(new Mesh{vertices=v.ToArray(),triangles=t.ToArray()},"Cottage solid gables"),plaster,parent,Vector3.zero,Quaternion.identity,Vector3.one);
            foreach(float side in new[]{-1f,1f})
            {
                for(float x=-w/2;x<=w/2+.1f;x+=w/4)Box("Structural oak front framing",new Vector3(x,.8f+h/2,side*(d/2+.07f)),new Vector3(.19f,h,.2f),wood);
                foreach(float y in new[]{1.05f,eave-.1f,h>6?4.3f:eave-.1f})Box("Horizontal oak floor beam",new Vector3(0,y,side*(d/2+.1f)),new Vector3(w+.2f,.23f,.22f),wood);
                Beam("Gable bargeboard",new Vector3(-w/2,eave,side*(d/2+.15f)),new Vector3(0,eave+roofH,side*(d/2+.15f)),.22f,wood);Beam("Gable bargeboard",new Vector3(w/2,eave,side*(d/2+.15f)),new Vector3(0,eave+roofH,side*(d/2+.15f)),.22f,wood);
                Box("Gable king post",new Vector3(0,eave+roofH/2,side*(d/2+.15f)),new Vector3(.2f,roofH,.2f),wood);
                for(int bay=0;bay<4;bay++){float x=-w/2+(bay+.5f)*w/4;Beam("Diagonal half timber brace",new Vector3(x-w/8,1.2f,side*(d/2+.09f)),new Vector3(x+w/8,3.6f,side*(d/2+.09f)),.13f,wood);}
            }
            foreach(float side in new[]{-1f,1f})
            {
                Box("Side wall eave beam",new Vector3(side*(w/2+.08f),eave,0),new Vector3(.22f,.25f,d+.4f),wood);
                for(float z=-d/2;z<=d/2;z+=d/3)Box("Side wall upright",new Vector3(side*(w/2+.08f),.8f+h/2,z),new Vector3(.2f,h,.2f),wood);
            }
            float angle=Mathf.Atan2(roofH,w/2+.45f)*Mathf.Rad2Deg,slope=Mathf.Sqrt(roofH*roofH+Mathf.Pow(w/2+.45f,2));int rows=8,columns=Mathf.CeilToInt((d+1)/.62f);
            foreach(float side in new[]{-1f,1f})for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)
            {float u=(row+.5f)/rows;float z=-d/2-.4f+(col+.5f)*((d+.8f)/columns)+(row%2)*.13f;Box("Overlapping natural slate shingle",new Vector3(side*(w/2+.45f)*(1-u),eave+roofH*u+.08f,z),new Vector3(slope/rows+.15f,.065f,(d+.8f)/columns+.04f),slate,false,Quaternion.Euler(0,0,-side*angle));}
            Beam("Ridge coping",new Vector3(0,eave+roofH+.06f,-d/2-.5f),new Vector3(0,eave+roofH+.06f,d/2+.5f),.22f,slate);
            Box("Coursed chimney",new Vector3(w*.27f,eave+roofH*.6f,d*.23f),new Vector3(.9f,4,.85f),limestone);Box("Chimney weathering cap",new Vector3(w*.27f,eave+roofH*.6f+2,d*.23f),new Vector3(1.15f,.22f,1.1f),trim);Box("Soot dark chimney opening",new Vector3(w*.27f,eave+roofH*.6f+2.13f,d*.23f),new Vector3(.63f,.02f,.56f),iron);
            float doorZ=-d/2-.15f;Box("Recessed oak entrance",new Vector3(0,2.1f,doorZ),new Vector3(1.15f,2.65f,.18f),wood);
            foreach(float x in new[]{-.68f,.68f})Box("Dressed stone door jamb",new Vector3(x,2.1f,doorZ-.04f),new Vector3(.2f,2.9f,.3f),limestone);
            Box("Dressed door lintel",new Vector3(0,3.58f,doorZ-.04f),new Vector3(1.6f,.25f,.32f),limestone);Box("Doorstep",new Vector3(0,.75f,doorZ-.25f),new Vector3(1.7f,.2f,.8f),limestone);
            foreach(float y in new[]{1.2f,2.7f})Box("Iron door strap",new Vector3(0,y,doorZ-.11f),new Vector3(1,.075f,.025f),iron);
            Box("Door latch",new Vector3(.37f,2,doorZ-.14f),new Vector3(.12f,.22f,.08f),iron);
            foreach(float x in new[]{-w*.3f,w*.3f})foreach(float y in h>6?new[]{2.7f,5.9f}:new[]{2.7f})CottageWindow(new Vector3(x,y,-d/2-.13f));
        }
        parent=houses;Combine(houses,"Cottage material batches");
    }
    static void CottageWindow(Vector3 p)
    {
        Box("Recessed cottage glass",p,new Vector3(.95f,1.25f,.12f),glass);
        foreach(float side in new[]{-1f,1f})
        {Box("Carved timber window jamb",p+new Vector3(side*.56f,0,-.1f),new Vector3(.16f,1.48f,.22f),wood);Box("Opened plank shutter",p+new Vector3(side*.99f,0,-.03f),new Vector3(.62f,1.3f,.09f),wood,false,Quaternion.Euler(0,side*22,0));}
        foreach(float side in new[]{-1f,1f})Box("Timber window header",p+new Vector3(0,side*.7f,-.1f),new Vector3(1.25f,.16f,.24f),wood);
        Box("Window cross mullion",p+new Vector3(0,0,-.11f),new Vector3(.065f,1.3f,.1f),wood);Box("Window cross mullion",p+new Vector3(0,0,-.11f),new Vector3(1,.065f,.1f),wood);Box("Projecting window sill",p+new Vector3(0,-.8f,-.15f),new Vector3(1.4f,.13f,.4f),limestone);
    }
    static void Windows()
    {
        var sockets=GameObject.Find("Castle window placement sockets");
        if(!sockets)
        {
            sockets=new GameObject("Castle window placement sockets");sockets.transform.parent=root;
            foreach(var old in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Deep lancet window").ToArray())
            {var g=new GameObject("Lancet socket").transform;g.parent=sockets.transform;g.position=old.position;g.rotation=old.rotation;g.localScale=new Vector3(old.lossyScale.x,old.lossyScale.y,1);}
        }
        string[] oldNames={"Deep lancet window","Window stone jamb","Pointed window arch","Window mullion"};foreach(var old in root.GetComponentsInChildren<Transform>().Where(t=>oldNames.Contains(t.name)).ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        var windows=Group("14 • Recessed gothic stone tracery and leaded glass");var cache=new Dictionary<string,Mesh[]>();
        foreach(Transform socket in sockets.transform)
        {
            float w=socket.localScale.x,h=socket.localScale.y;string key=w.ToString("F2")+"-"+h.ToString("F2");
            if(!cache.TryGetValue(key,out var meshes))
            {
                float rim=Mathf.Min(.18f,w*.12f);Vector2[] outer={new Vector2(-w/2,-h/2),new Vector2(w/2,-h/2),new Vector2(w/2,h*.27f),new Vector2(0,h*.58f),new Vector2(-w/2,h*.27f)};Vector2[] inner=outer.Select(p=>new Vector2(p.x*(1-rim*2/w),p.y*(1-rim*2/h))).ToArray();
                var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
                for(int i=0;i<5;i++){int j=(i+1)%5;Quad(v,t,uv,new Vector3(outer[i].x,outer[i].y,-.46f),new Vector3(outer[j].x,outer[j].y,-.46f),new Vector3(inner[j].x,inner[j].y,-.34f),new Vector3(inner[i].x,inner[i].y,-.34f));Quad(v,t,uv,new Vector3(inner[i].x,inner[i].y,-.34f),new Vector3(inner[j].x,inner[j].y,-.34f),new Vector3(inner[j].x,inner[j].y,-.12f),new Vector3(inner[i].x,inner[i].y,-.12f));}
                // Reverse front-facing winding to face outwards along the socket's negative Z.
                for(int k=0;k<t.Count;k+=3)(t[k+1],t[k+2])=(t[k+2],t[k+1]);
                var frame=Save(new Mesh{vertices=v.ToArray(),triangles=t.ToArray(),uv=uv.ToArray()},"Bevelled lancet surround "+key);
                v=inner.Select(p=>new Vector3(p.x,p.y,-.12f)).ToList();t=new List<int>{0,2,1,0,3,2,0,4,3};uv=inner.Select(p=>new Vector2(p.x/w+.5f,(p.y+h/2)/(h*1.08f))).ToList();var pane=Save(new Mesh{vertices=v.ToArray(),triangles=t.ToArray(),uv=uv.ToArray()},"Recessed lancet glass "+key);meshes=new[]{frame,pane};cache[key]=meshes;
            }
            Model("Sculpted pointed stone reveal",meshes[0],trim,windows,socket.position,socket.rotation,Vector3.one);Model("Lead set reflective window",meshes[1],glass,windows,socket.position,socket.rotation,Vector3.one);
        }
        Combine(windows,"Gothic window material batches");
    }
    static void Water()
    {
        var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Turquoise river.mat");m.shader=Shader.Find("Aurelia/Reference River");m.SetColor("_BaseColor",C("#164e4c"));m.SetColor("_ShallowColor",C("#6eae8e"));EditorUtility.SetDirty(m);
        foreach(var r in root.GetComponentsInChildren<Renderer>())if(r.sharedMaterial==m){r.gameObject.layer=4;r.shadowCastingMode=ShadowCastingMode.Off;}
        var data=Camera.main.GetUniversalAdditionalCameraData();data.requiresDepthTexture=true;data.requiresColorTexture=true;
        var urp=UniversalRenderPipeline.asset;urp.supportsCameraDepthTexture=true;urp.supportsCameraOpaqueTexture=true;EditorUtility.SetDirty(urp);
        if(!Camera.main.GetComponent<RiverReflections>())Camera.main.gameObject.AddComponent<RiverReflections>();
    }
    public static void CorrectDetails()
    {
        root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;serial=2000;Materials();Windows();
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Stratified cliff outcrop").ToArray())
        {
            float top=ReferenceCastleBuilder.Height(tr.position.x,100);var bounds=tr.GetComponent<Renderer>().bounds;
            if(top<20){UnityEngine.Object.DestroyImmediate(tr.gameObject);continue;}
            if(bounds.max.y>top+1){var p=tr.position;p.y-=bounds.max.y-top;tr.position=p;}
        }
        WaterMist();ReferenceCastleBuilder.CloseRoofSeams();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    static void WaterMist()
    {
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Cascade spray and whitewater").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        Group("15 • Living cascade mist and spray");var material=Mat("Soft waterfall spray","Aurelia/Drifting Cloud","#d9e8e1");material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(A+"/Cloud.png"));material.SetColor("_BaseColor",new Color(.9f,.96f,.94f,.18f));
        foreach(float x in new[]{-106f,-83,-60,-39,37,60,83,106})
        {
            var g=new GameObject("Waterfall airborne spray");g.transform.parent=parent;g.transform.position=new Vector3(x,2.3f,72+Mathf.Abs(x)*.06f);var ps=g.AddComponent<ParticleSystem>();var main=ps.main;main.duration=5;main.loop=true;main.startLifetime=new ParticleSystem.MinMaxCurve(2,4);main.startSpeed=.5f;main.startSize=new ParticleSystem.MinMaxCurve(3,7);main.startColor=new Color(.94f,.98f,1,.3f);main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=60;var emission=ps.emission;emission.rateOverTime=7;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(9,.3f,4);var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.x=new ParticleSystem.MinMaxCurve(.3f,.7f);velocity.y=new ParticleSystem.MinMaxCurve(.3f,.9f);velocity.z=new ParticleSystem.MinMaxCurve(-.2f,.2f);var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(1,1.8f)));var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.4f,.2f),new GradientAlphaKey(0,1)});color.color=new ParticleSystem.MinMaxGradient(gradient);var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
    }
    public static void CloudAndBanks()
    {
        root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;serial=4000;
        AssetDatabase.ImportAsset(A+"/SmallCloud.png");var imp=(TextureImporter)AssetImporter.GetAtPath(A+"/SmallCloud.png");imp.alphaIsTransparency=true;imp.wrapMode=TextureWrapMode.Clamp;imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.SaveAndReimport();
        var mat=Mat("Small fair weather cloud","Aurelia/Drifting Cloud","#ffffff");mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(A+"/SmallCloud.png"));
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Small wind driven cloud")))
        {tr.GetComponent<Renderer>().sharedMaterial=mat;var s=tr.localScale;s.y=Mathf.Abs(s.x)*.58f;tr.localScale=s;}
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Slow towering cumulus 1")){var s=tr.localScale;s.x=-Mathf.Abs(s.x);tr.localScale=s;}
        var terrain=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshFilter>().sharedMesh;var v=terrain.vertices;for(int i=0;i<v.Length;i++)v[i].y=ReferenceCastleBuilder.Height(v[i].x,v[i].z);terrain.vertices=v;terrain.RecalculateNormals();EditorUtility.SetDirty(terrain);var collider=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=terrain;
        var river=GameObject.Find("Meandering turquoise river").GetComponent<MeshFilter>().sharedMesh;v=river.vertices;for(int i=0;i<v.Length;i++)v[i].x=18+Mathf.Sin((v[i].z+80)*.023f)*42+(i%2==0?-34:34);river.vertices=v;river.RecalculateBounds();EditorUtility.SetDirty(river);
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Oak • irregular trunk and leaf clusters"||t.name.StartsWith("Inhabited valley cottage")))
        {var p=tr.position;p.y=ReferenceCastleBuilder.Height(p.x,p.z)-.05f;tr.position=p;}
        // Re-batch cottage transforms after settling them onto the softer riverbanks.
        var houses=GameObject.Find("13 • Inhabited timber and stone hamlets").transform;var old=houses.Find("Cottage material batches");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);foreach(var r in houses.GetComponentsInChildren<MeshRenderer>())r.enabled=true;Combine(houses,"Cottage material batches");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    public static void CaptureReview()
    {
        string dir="D:/GAME/artifacts/reference-castle/realism";Directory.CreateDirectory(dir);var camera=Camera.main;var p=camera.transform.position;var q=camera.transform.rotation;float fov=camera.fieldOfView;
        try
        {
            Render("Overview",camera,p,p+camera.transform.forward*100,fov);
            Render("Castle-windows",camera,new Vector3(-26,114,113),new Vector3(-14,155,167),52);
            var house=GameObject.Find("13 • Inhabited timber and stone hamlets").transform.Cast<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")).OrderBy(t=>(t.position-new Vector3(-90,0,-100)).sqrMagnitude).First();
            Vector3 target=house.position+Vector3.up*3.2f;Render("Houses",camera,target+house.rotation*new Vector3(11,3,-18),target,52);
            Render("River-and-rocks",camera,new Vector3(-32,8,-219),new Vector3(16,3,-154),58);
        }
        finally{camera.transform.SetPositionAndRotation(p,q);camera.fieldOfView=fov;}
    }
    static void Render(string name,Camera cam,Vector3 p,Vector3 target,float fov)
    {
        cam.transform.position=p;cam.transform.LookAt(target);cam.fieldOfView=fov;var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(1600,1000,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1600,1000),0,0);t.Apply();File.WriteAllBytes("D:/GAME/artifacts/reference-castle/realism/"+name+".png",t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
    }
    public static void ArchitectureAndFields()
    {
        root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;serial=5000;Materials();Random.InitState(73);
        var houses=GameObject.Find("13 • Inhabited timber and stone hamlets").transform;
        foreach(var house in houses.Cast<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")).ToArray())
        {
            var body=house.Find("Solid lime plaster walls");float w=body.localScale.x,h=body.localScale.y,d=body.localScale.z;
            foreach(float side in new[]{-1f,1f})for(int bay=0;bay<2;bay++)
            {var wall=new GameObject("Side casement group").transform;wall.SetParent(house,false);wall.localPosition=new Vector3(side*(w/2+.12f),0,(bay-.5f)*d*.43f);wall.localRotation=Quaternion.Euler(0,side<0?90:-90,0);parent=wall;CottageWindow(new Vector3(0,2.7f,0));if(h>6)CottageWindow(new Vector3(0,5.8f,0));}
            parent=house;
            // A small pitched porch gives the entrance a credible sheltered threshold.
            float z=-d/2-.9f;foreach(float s in new[]{-1f,1f})Box("Porch oak post",new Vector3(s*.98f,1.8f,z-.6f),new Vector3(.13f,2.1f,.13f),wood);
            foreach(float s in new[]{-1f,1f})Box("Porch pitched canopy",new Vector3(s*.5f,3.25f,z),new Vector3(1.25f,.1f,1.8f),slate,false,Quaternion.Euler(0,0,-s*24));
        }
        var batch=houses.Find("Cottage material batches");if(batch)UnityEngine.Object.DestroyImmediate(batch.gameObject);foreach(var r in houses.GetComponentsInChildren<MeshRenderer>())r.enabled=true;Combine(houses,"Cottage material batches");
        // Replace raised crop bars with terrain-following soil and individual grain stems.
        var sites=root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Cultivated field").Select(t=>new Vector4(t.position.x,t.position.z,t.localScale.x,t.localScale.z)).ToList();
        if(sites.Count==0)sites.AddRange(new[]{new Vector4(-92,-95,17,14),new Vector4(83,-76,15,13),new Vector4(-78,-153,12,15)});
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Valley grain furrow"||t.name=="Parallel crop furrow"||t.name=="Cultivated field").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        Group("16 • Natural planted fields and cottage paths");var soil=Mat("Cultivated soil","Aurelia/Surface Detail","#5d513b");Texture(soil,"Rock",.8f,.07f);soil.SetFloat("_TextureStrength",.25f);
        var crop=Mat("Golden green ripening grain","Aurelia/Meadow","#ffffff");var v=new List<Vector3>();var t=new List<int>();var color=new List<Color>();
        foreach(var field in sites)
        {
            var fieldV=new List<Vector3>();var fieldT=new List<int>();var fieldUV=new List<Vector2>();
            int nx=8,nz=8;for(int iz=0;iz<=nz;iz++)for(int ix=0;ix<=nx;ix++)
            {float x=field.x+(ix/(float)nx-.5f)*field.z,z=field.y+(iz/(float)nz-.5f)*field.w;fieldV.Add(new Vector3(x,ReferenceCastleBuilder.Height(x,z)+.025f,z));fieldUV.Add(new Vector2(ix,iz));if(ix<nx&&iz<nz){int k=iz*(nx+1)+ix;fieldT.AddRange(new[]{k,k+nx+1,k+1,k+1,k+nx+1,k+nx+2});}}
            Model("Terrain conforming tilled soil",Save(new Mesh{vertices=fieldV.ToArray(),triangles=fieldT.ToArray(),uv=fieldUV.ToArray()},"Tilled field"),soil,parent,Vector3.zero,Quaternion.identity,Vector3.one);
            for(float x=field.x-field.z*.48f;x<field.x+field.z*.48f;x+=.6f)for(float z=field.y-field.w*.48f;z<field.y+field.w*.48f;z+=.38f)
            {float height=Random.Range(.38f,.7f),y=ReferenceCastleBuilder.Height(x,z);if(y<2)continue;Vector3 p=new Vector3(x+Random.Range(-.09f,.09f),y+.04f,z);int n=v.Count;v.AddRange(new[]{p+Vector3.left*.025f,p+Vector3.right*.025f,p+new Vector3(.06f,height,0)});t.AddRange(new[]{n,n+1,n+2});color.Add(C("#667a35"));color.Add(C("#667a35"));color.Add(C("#a5ab56"));n=v.Count;v.AddRange(new[]{p+new Vector3(-.05f,height-.17f,0),p+new Vector3(.08f,height-.15f,0),p+new Vector3(.06f,height+.07f,0)});t.AddRange(new[]{n,n+1,n+2});color.Add(C("#bab469"));color.Add(C("#bab469"));color.Add(C("#ccc187"));}
        }
        var grain=new Mesh{indexFormat=IndexFormat.UInt32};grain.SetVertices(v);grain.SetTriangles(t,0);grain.SetColors(color);Model("Rows of individual ripening grain",Save(grain,"Natural crops"),crop,parent,Vector3.zero,Quaternion.identity,Vector3.one);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    public static void Validate()
    {
        ReferenceCastleBuilder.Validate();root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;
        var s=new System.Text.StringBuilder();s.Append(File.ReadAllText("D:/GAME/artifacts/reference-castle/editor-validation.txt"));
        var clouds=root.GetComponentsInChildren<CloudDrift>();int big=clouds.Count(c=>c.name.StartsWith("Slow towering")),small=clouds.Count(c=>c.name.StartsWith("Small wind"));s.AppendLine((big==4&&small==17?"PASS":"FAIL")+" living cloud layers: large="+big+", small="+small);
        int trees=root.GetComponentsInChildren<Transform>().Count(t=>t.name=="Oak • irregular trunk and leaf clusters");int houses=root.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Inhabited valley cottage"));s.AppendLine((trees>100&&houses>20?"PASS":"FAIL")+" natural tree/house models: trees="+trees+", houses="+houses);
        var sockets=GameObject.Find("Castle window placement sockets");s.AppendLine((sockets&&sockets.transform.childCount>100?"PASS":"FAIL")+" framed gothic window sockets="+(sockets?sockets.transform.childCount:0));
        int spray=root.GetComponentsInChildren<ParticleSystem>().Length;s.AppendLine((spray==8?"PASS":"FAIL")+" waterfall spray emitters="+spray);
        s.AppendLine((Camera.main.GetComponent<RiverReflections>()?"PASS":"FAIL")+" saved river reflection component");File.WriteAllText("D:/GAME/artifacts/reference-castle/editor-validation.txt",s.ToString());Debug.Log(s.ToString());
    }
    public static void PrepareBuild()
    {
        root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;Materials();
        var view=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();view.Picture();Camera.main.clearFlags=CameraClearFlags.Skybox;
        foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Slow towering cumulus 0")){t.localScale=new Vector3(280,470,1);var p=t.position;p.y=235;t.position=p;}
        foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Small wind driven cloud"))){int i=int.Parse(t.name.Split(' ').Last());var p=t.position;p.x=-650+i*81.25f;p.y=265+Mathf.Sin(i*1.7f)*65;t.position=p;}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
}
