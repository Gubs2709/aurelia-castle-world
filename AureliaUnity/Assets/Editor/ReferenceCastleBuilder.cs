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

// Dedicated, fully three-dimensional interpretation of the user's waterfall castle reference.
// Geometry is built through Unity APIs. The earlier Aurelia scene is preserved.
public static class ReferenceCastleBuilder
{
    const string Dir="Assets/ReferenceCastle";
    static Transform root,group;
    static Material stone,ivory,roof,glass,rock,grass,water,foam,wood,gold,dark,plaster;
    static Material[] flags;
    static int serial;
    static Color C(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
    static Material Mat(string name,string hex,string shader="Universal Render Pipeline/Lit")
    {
        var m=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/"+name+".mat");if(!m){m=new Material(Shader.Find(shader)){name=name};AssetDatabase.CreateAsset(m,Dir+"/"+name+".mat");}m.color=C(hex);m.enableInstancing=true;m.SetFloat("_Smoothness",.19f);EditorUtility.SetDirty(m);return m;
    }
    static Transform Group(string name){group=new GameObject(name).transform;group.parent=root;return group;}
    static GameObject Shape(string name,PrimitiveType type,Vector3 p,Vector3 scale,Material m,bool solid=false)
    {
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(group);g.transform.position=p;g.transform.localScale=scale;
        g.GetComponent<Renderer>().sharedMaterial=m;if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static GameObject Box(string name,Vector3 p,Vector3 s,Material m,bool solid=false)=>Shape(name,PrimitiveType.Cube,p,s,m,solid);
    static void Beam(string name,Vector3 a,Vector3 b,float r,Material m){var g=Box(name,(a+b)/2,new Vector3(r,(a-b).magnitude,r),m);g.transform.rotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    static GameObject MeshObject(string name,List<Vector3> v,List<int> t,Material m,List<Color> colors=null,List<Vector2> uv=null,bool solid=false)
    {
        var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(t,0);if(colors!=null)mesh.SetColors(colors);if(uv!=null)mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Dir+"/Meshes/"+(serial++)+".asset");var g=new GameObject(name);g.transform.parent=group;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=m;if(solid)g.AddComponent<MeshCollider>().sharedMesh=mesh;return g;
    }
    static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=v.Count;v.AddRange(new[]{a,b,c,d});t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
    static float N(float x,float z)=>Mathf.PerlinNoise(x*.017f+27,z*.017f+19);
    static float RiverX(float z)=>18+Mathf.Sin((z+80)*.023f)*42;
    public static float Height(float x,float z)
    {
        float hill=Mathf.Pow(Mathf.Clamp01((Mathf.Abs(x)-120)/180),1.6f)*(35+65*N(x,z));
        float y=3+N(x,z)*3+hill;
        float plateau=Mathf.SmoothStep(0,1,Mathf.InverseLerp(84,92,z))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(275,245,Mathf.Abs(x)))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(345,318,z));
        y=Mathf.Lerp(y,65+N(x,z)*3,plateau);
        if(z<60)y=Mathf.Lerp(y,-3,Mathf.Exp(-Mathf.Pow((x-RiverX(z))/21,2)));
        // A real central passage leaves the river gate and uphill ramp accessible.
        if(z>57&&z<156&&Mathf.Abs(x)<9)y=Mathf.Min(y,12+Mathf.Clamp01((z-63)/89)*54-1);
        if(z<-213)y+=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-210,-290,z))*12;
        if(z<58)y=Mathf.Lerp(y,-3,Mathf.Exp(-Mathf.Pow((x-RiverX(z))/27,2)));
        if(z>20&&z<84)y=Mathf.Lerp(y,-2,Mathf.Exp(-Mathf.Pow((z-59)/24,6))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(130,100,Mathf.Abs(x))));
        y=V3Layout.CityLevel(x,z,y);
        y=V3Layout.LakeCarve(x,z,y);
        return V8Landscape.Height(x,z,y);
    }
    [MenuItem("Aurelia/Build reference waterfall citadel")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play mode first.");
        serial=0;Random.InitState(151022);Directory.CreateDirectory(Dir+"/Meshes");
        // Only regenerate assets owned by this builder; never touch the earlier world.
        foreach(var path in AssetDatabase.FindAssets("",new[]{Dir}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!AssetDatabase.IsValidFolder(p)&&!p.EndsWith("PaintedSky.png")&&!p.EndsWith(".md")).OrderByDescending(p=>p.Length).ToArray())AssetDatabase.DeleteAsset(path);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);root=new GameObject("THE CITADEL OF FALLING WATER • reference composition").transform;
        stone=Mat("Cool ivory limestone","#c5d3d2");ivory=Mat("Sunlit carved limestone","#eee9d5");roof=Mat("Sea glass turquoise slates","#37868e");glass=Mat("Recessed blue windows","#31596c");rock=Mat("Stratified grey cliff","#708a7f");grass=Mat("Valley vertex palette","#ffffff","Aurelia/Meadow");water=Mat("Turquoise river","#39b6b2","Aurelia/River");foam=Mat("White flowing cascade","#bde6dc","Aurelia/Reference Waterfall");wood=Mat("Weathered oak","#685443");gold=Mat("Golden finials","#dccb79");dark=Mat("Deep arch interior","#275864");plaster=Mat("Farmhouse plaster","#ddd9b2");
        flags=new[]{Mat("Rose pennant","#ee8ea7"),Mat("Lemon pennant","#eee689"),Mat("Mint pennant","#8bdaa7"),Mat("Azure pennant","#8ed8dc")};
        stone.shader=Shader.Find("Aurelia/Reference Masonry");
        Lighting();Group("01 • Valley, cliffs, river and snowy mountains");Landscape();
        Group("02 • Nested castle terraces and soaring towers");Castle();
        Group("03 • Rivergate, arched bridge and falling water");BridgeAndFalls();
        Group("04 • Farms, hamlets and wooded slopes");Village();Vegetation();
        Group("05 • Towering painted cumulus and birds");Clouds();Birds();
        Group("06 • Flower overlook and mounted traveller");Rider();Flowers();
        CameraAndControls();CombineArchitecture();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/ReferenceCastle.unity",true)};
        PlayerSettings.productName="Aurelia • Citadel of Falling Water";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=1000;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/ReferenceCastle.unity");AssetDatabase.SaveAssets();
        Debug.Log("REFERENCE_CASTLE_READY renderers="+root.GetComponentsInChildren<Renderer>().Length+" meshes="+serial);
    }
    static void Lighting()
    {
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=C("#a4cce1");RenderSettings.ambientEquatorColor=C("#a6b9af");RenderSettings.ambientGroundColor=C("#6c7858");
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.00074f;RenderSettings.fogColor=C("#a8ced8");
        RenderSettings.skybox=null;
        var sun=new GameObject("Warm light from the right").AddComponent<Light>();sun.transform.parent=root;sun.type=LightType.Directional;sun.color=C("#fff0ce");sun.intensity=1.65f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38,-48,0);RenderSettings.sun=sun;
        var urp=UniversalRenderPipeline.asset;if(urp){urp.shadowDistance=420;urp.msaaSampleCount=4;urp.renderScale=1;urp.supportsHDR=true;EditorUtility.SetDirty(urp);}
        var p=ScriptableObject.CreateInstance<VolumeProfile>();p.Add<Tonemapping>().mode.Override(TonemappingMode.Neutral);var adj=p.Add<ColorAdjustments>();adj.saturation.Override(8);adj.contrast.Override(4);var b=p.Add<Bloom>();b.intensity.Override(.12f);b.threshold.Override(1.3f);
        AssetDatabase.CreateAsset(p,Dir+"/Atmosphere.asset");foreach(var component in p.components)AssetDatabase.AddObjectToAsset(component,p);var volume=new GameObject("Pastel afternoon atmosphere").AddComponent<Volume>();volume.transform.parent=root;volume.isGlobal=true;volume.sharedProfile=p;
    }
    static void Landscape()
    {
        int n=280;var v=new List<Vector3>();var t=new List<int>();var colors=new List<Color>();
        for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)
        {float px=-450+x*900f/n,pz=-350+z*900f/n,y=Height(px,pz);v.Add(new Vector3(px,y,pz));float noise=N(px,pz);Color c=Color.Lerp(C("#4c7f35"),C("#afd268"),noise);if(pz>58&&pz<115&&Mathf.Abs(px)<125&&y>18)c=Color.Lerp(C("#6c8583"),C("#88ab65"),Mathf.Pow(noise,2));colors.Add(c);if(x<n&&z<n){int k=z*(n+1)+x;t.AddRange(new[]{k,k+n+1,k+1,k+1,k+n+1,k+n+2});}}
        MeshObject("Sculpted river valley and planted escarpment",v,t,grass,colors,null,true);
        v=new List<Vector3>();t=new List<int>();var uv=new List<Vector2>();
        for(int i=0;i<=240;i++){float z=-355+i*420f/240,x=RiverX(z);v.Add(new Vector3(x-24,1.6f,z));v.Add(new Vector3(x+24,1.6f,z));uv.Add(new Vector2(0,i/15f));uv.Add(new Vector2(1,i/15f));if(i<240){int k=i*2;t.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}}
        MeshObject("Meandering turquoise river",v,t,water,null,uv);
        // Two ridged ranges frame the central silhouette without covering it.
        for(int side=-1;side<=1;side+=2)
        {
            v=new List<Vector3>();t=new List<int>();colors=new List<Color>();int nx=90,nz=45;
            for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++)
            {float px=side*(95+x*6),pz=260+z*8;float ridge=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(z-19-Mathf.Sin(x*.15f)*7)/23),1.2f);float h=ridge*(80+100*Mathf.Abs(Mathf.Sin(x*.057f+.8f))+44*N(px*2,pz*2));v.Add(new Vector3(px,h,pz));float snow=Mathf.SmoothStep(.57f,.83f,h/200+N(px*5,pz*5)*.15f);colors.Add(Color.Lerp(C("#7fabb5"),C("#f3f2e2"),snow));if(x<nx&&z<nz){int k=z*(nx+1)+x;if(side>0)t.AddRange(new[]{k,k+nx+1,k+1,k+1,k+nx+1,k+nx+2});else t.AddRange(new[]{k,k+1,k+nx+1,k+1,k+nx+2,k+nx+1});}}
            MeshObject("Snow brushed mountain range "+side,v,t,grass,colors);
        }
    }
    static void Ring(string name,float x,float z,float y,float r,float h,Material m,int sides=24)
    {Cylinder(name,new Vector3(x,y,z),r,r,h,m,sides);}
    static void Cylinder(string name,Vector3 basePoint,float r0,float r1,float height,Material mat,int sides=24)
    {
        var v=new List<Vector3>();var t=new List<int>();
        for(int i=0;i<sides;i++){float a=i*2*Mathf.PI/sides,b=(i+1)*2*Mathf.PI/sides;Quad(v,t,basePoint+new Vector3(Mathf.Sin(a)*r0,0,Mathf.Cos(a)*r0),basePoint+new Vector3(Mathf.Sin(b)*r0,0,Mathf.Cos(b)*r0),basePoint+new Vector3(Mathf.Sin(b)*r1,height,Mathf.Cos(b)*r1),basePoint+new Vector3(Mathf.Sin(a)*r1,height,Mathf.Cos(a)*r1));int k=v.Count;v.AddRange(new[]{basePoint+Vector3.up*height,basePoint+new Vector3(Mathf.Sin(a)*r1,height,Mathf.Cos(a)*r1),basePoint+new Vector3(Mathf.Sin(b)*r1,height,Mathf.Cos(b)*r1)});t.AddRange(new[]{k,k+1,k+2});}
        MeshObject(name,v,t,mat);
    }
    static void Spire(float x,float z,float y,float r,float height)
    {
        // True stepped slate courses give the roof the illustrated striped taper.
        int steps=Mathf.CeilToInt(height/1.4f);for(int i=0;i<steps;i++){float a=i/(float)steps,b=(i+1f)/steps;Cylinder("Overlapping turquoise roof course",new Vector3(x,y+a*height,z),r*(1-a)+.18f,r*(1-b),height/steps,roof,24);}
        Ring("Stone eave",x,z,y-.8f,r*1.04f,.8f,ivory);Beam("Spire gold needle",new Vector3(x,y+height,z),new Vector3(x,y+height+4,z),.13f,gold);
    }
    static void Pennant(float x,float z,float y,int index)
    {
        Beam("Pennant pole",new Vector3(x,y-1,z),new Vector3(x,y+8,z),.13f,gold);
        var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<12;i++){float u=i/12f,w=(i+1f)/12f;Quad(v,t,new Vector3(x+u*10,y+6-Mathf.Sin(u*4)*.7f,z+Mathf.Sin(u*7)*.6f),new Vector3(x+w*10,y+6-Mathf.Sin(w*4)*.7f,z+Mathf.Sin(w*7)*.6f),new Vector3(x+w*10,y+4.7f+w*.9f,z+Mathf.Sin(w*7)*.6f),new Vector3(x+u*10,y+4.7f+u*.9f,z+Mathf.Sin(u*7)*.6f));}var m=flags[index%flags.Length];m.SetFloat("_Cull",0);MeshObject("Silk pennant "+index,v,t,m);
    }
    static void Window(Vector3 p,float width,float height,float angle)
    {
        var rot=Quaternion.Euler(0,angle,0);var g=Box("Deep lancet window",p,new Vector3(width,height,.16f),glass);g.transform.rotation=rot;
        foreach(float s in new[]{-1f,1f}){g=Box("Window stone jamb",p+rot*new Vector3(s*(width/2+.15f),0,-.13f),new Vector3(.22f,height+.3f,.25f),ivory);g.transform.rotation=rot;}
        Beam("Pointed window arch",p+rot*new Vector3(-width/2,height/2,-.13f),p+rot*new Vector3(0,height/2+width*.6f,-.13f),.22f,ivory);Beam("Pointed window arch",p+rot*new Vector3(width/2,height/2,-.13f),p+rot*new Vector3(0,height/2+width*.6f,-.13f),.22f,ivory);
        g=Box("Window mullion",p+rot*new Vector3(0,0,-.16f),new Vector3(.13f,height,.18f),ivory);g.transform.rotation=rot;
    }
    static void Tower(float x,float z,float bottom,float radius,float height,float cap,int flag=0)
    {
        Ring("Limestone tower",x,z,bottom,radius,height,stone);Ring("Flared tower foot",x,z,bottom,radius*1.08f,1.8f,ivory);
        for(float y=bottom+10;y<bottom+height-3;y+=11)
        {Ring("Carved stringcourse",x,z,y,radius*1.025f,.45f,ivory);for(int i=0;i<8;i++){float a=i*45f*Mathf.Deg2Rad;Window(new Vector3(x+Mathf.Sin(a)*(radius+.06f),y-4,z-Mathf.Cos(a)*(radius+.06f)),Mathf.Min(1.7f,radius*.28f),3.6f,-i*45);}}
        Ring("Projecting machicolation gallery",x,z,bottom+height-3,radius*1.13f,2.2f,ivory);
        for(int i=0;i<16;i++){float a=i*Mathf.PI/8;Box("Gallery corbel",new Vector3(x+Mathf.Sin(a)*radius,z==0?bottom:bottom+height-4,z+Mathf.Cos(a)*radius),new Vector3(.6f,2,.7f),ivory);}
        Spire(x,z,bottom+height,radius*1.18f,cap);if(flag>=0)Pennant(x,z,bottom+height+cap+2,flag);
    }
    static void Wall(float x,float z,float y,float width,float height,float depth=5,bool gate=false)
    {
        if(gate){Box("Curtain left of gate",new Vector3(x-width/4-4,y+height/2,z),new Vector3(width/2-8,height,depth),stone,true);Box("Curtain right of gate",new Vector3(x+width/4+4,y+height/2,z),new Vector3(width/2-8,height,depth),stone,true);Box("Gate lintel",new Vector3(x,y+height-2,z),new Vector3(16,4,depth),ivory,true);}
        else Box("Limestone curtain wall",new Vector3(x,y+height/2,z),new Vector3(width,height,depth),stone,true);
        Box("Continuous parapet ledge",new Vector3(x,y+height,z),new Vector3(width+1,.8f,depth+1),ivory);
        for(float dx=-width/2;dx<width/2;dx+=3.2f){Box("Crenellated merlon",new Vector3(x+dx,y+height+1.5f,z),new Vector3(1.65f,2.3f,depth),ivory);Box("Wall vertical buttress",new Vector3(x+dx,y+height*.45f,z-depth/2-.35f),new Vector3(.65f,height*.9f,1),ivory);}
        for(float dx=-width/2+5;dx<width/2;dx+=9)if(!gate||Mathf.Abs(dx)>10)Window(new Vector3(x+dx,y+height*.65f,z-depth/2-.12f),1,3,0);
    }
    static void Palace(float x,float z,float y,float w,float d,float h)
    {
        Box("Grand palace mass",new Vector3(x,y+h/2,z),new Vector3(w,h,d),stone,true);
        for(float floor=8;floor<h;floor+=11)
        {
            Box("Palace carved cornice",new Vector3(x,y+floor,z),new Vector3(w+1,.65f,d+1),ivory);
            for(float dx=-w/2+4;dx<w/2-2;dx+=5.5f)Window(new Vector3(x+dx,y+floor-3,z-d/2-.13f),1.8f,4.8f,0);
            for(float dz=-d/2+4;dz<d/2-2;dz+=6)foreach(float side in new[]{-1f,1f})Window(new Vector3(x+side*(w/2+.13f),y+floor-3,z+dz),1.6f,4.8f,side>0?-90:90);
        }
        var v=new List<Vector3>();var t=new List<int>();Quad(v,t,new Vector3(x-w/2-1,y+h,z-d/2-1),new Vector3(x-w/2-1,y+h,z+d/2+1),new Vector3(x,y+h+20,z+d/2+1),new Vector3(x,y+h+20,z-d/2-1));Quad(v,t,new Vector3(x,y+h+20,z-d/2-1),new Vector3(x,y+h+20,z+d/2+1),new Vector3(x+w/2+1,y+h,z+d/2+1),new Vector3(x+w/2+1,y+h,z-d/2-1));MeshObject("Steep palace slate roof",v,t,roof);
        for(float dz=-d/2;dz<=d/2;dz+=3)Beam("Fine slate roof seam",new Vector3(x-w/2-1,y+h+.15f,z+dz),new Vector3(x,y+h+20.15f,z+dz),.13f,ivory);
    }
    static void Castle()
    {
        Wall(0,101,66,193,19,6,true);Wall(0,141,88,139,25,6,true);
        Palace(0,191,108,62,47,67);Palace(-22,152,90,35,30,47);Palace(37,169,94,29,30,56);
        Palace(4,211,169,19,21,56);
        Tower(-10,209,174,7.2f,73,42,1);Tower(7,206,190,4.5f,38,28,-1);
        Tower(-30,177,110,8,79,33,0);Tower(32,179,121,8.2f,79,34,0);Tower(-48,159,89,9,64,32,1);Tower(52,161,90,7.5f,64,30,2);
        Tower(-65,137,77,8,48,28,2);Tower(70,143,77,7,48,26,0);
        Tower(-92,104,66,9,31,25,2);Tower(93,106,66,9,30,25,1);
        Tower(-59,105,66,6,31,24,0);Tower(54,107,66,6,34,26,3);
        Tower(-112,88,62,5.5f,18,19,-1);Tower(113,91,62,5.5f,20,20,-1);
        Tower(-19,147,115,4.5f,47,24,3);Tower(18,146,111,4,43,24,-1);
        Tower(-56,195,97,6,45,28,-1);Tower(60,200,93,6,55,28,0);
        for(int i=0;i<9;i++){float x=-45+i*11;Tower(x,127,88,2.3f,18+(i%3)*5,13,-1);}
        // Delicate upper lantern, balconies, flying buttresses and royal heraldry.
        for(int i=0;i<4;i++){float x=-15+i*9;Tower(x,167,167,2,12,12,-1);}
        for(float y=180;y<226;y+=11){Box("Central keep balcony",new Vector3(4,y,198),new Vector3(23,.7f,4),ivory);for(float x=-7;x<=15;x+=1.8f)Box("Balustrade spindle",new Vector3(x,y+1.2f,196.5f),new Vector3(.25f,2,.25f),ivory);}
        foreach(float side in new[]{-1f,1f})for(int i=0;i<4;i++){float z=174+i*11;Beam("Flying palace buttress",new Vector3(side*31,160,z),new Vector3(side*43,132,z),1.2f,ivory);}
        for(int i=0;i<12;i++){float x=-80+i*14;Box("Garden terrace parapet",new Vector3(x,87,97),new Vector3(10,1,1),ivory);}
    }
    static Vector3 BridgePoint(float t)=>Vector3.Lerp(new Vector3(100,12,-184),new Vector3(0,12,64),t);
    static void BridgeAndFalls()
    {
        Vector3 a=BridgePoint(0),b=BridgePoint(1),dir=(b-a).normalized,right=Vector3.Cross(Vector3.up,dir);float len=(b-a).magnitude;
        var deck=Box("Long continuous stone bridge deck",(a+b)/2,new Vector3(14,1,len+4),stone,true);deck.transform.rotation=Quaternion.LookRotation(dir);
        for(int i=0;i<=13;i++)
        {
            float f=i/13f;var p=BridgePoint(f);Box("Bridge stone pier",p-new Vector3(0,7,0),new Vector3(3,14,6),stone);
            foreach(float side in new[]{-1f,1f}){var post=Box("Bridge parapet pier",p+right*side*6.6f+Vector3.up*1.5f,new Vector3(1.3f,3,1.3f),ivory);Box("Bridge post capital",post.transform.position+Vector3.up*1.7f,new Vector3(1.8f,.4f,1.8f),ivory);}
            if(i<13)
            {
                var next=BridgePoint((i+1)/13f);foreach(float side in new[]{-1f,1f})
                {
                    var rail=Box("Stone bridge parapet",(p+next)/2+right*side*6.6f+Vector3.up*.85f,new Vector3(.75f,1.5f,len/13),ivory,true);rail.transform.rotation=Quaternion.LookRotation(dir);
                    var v=new List<Vector3>();var t=new List<int>();float span=len/13;for(int k=0;k<18;k++){float u=k/18f,w=(k+1f)/18;Vector3 p0=p+dir*u*span+right*side*5.8f,p1=p+dir*w*span+right*side*5.8f;float h0=-7+Mathf.Sin(u*Mathf.PI)*6.1f,h1=-7+Mathf.Sin(w*Mathf.PI)*6.1f;Quad(v,t,p0+Vector3.up*h0,p1+Vector3.up*h1,p1,p0);}MeshObject("Open stone bridge arch",v,t,stone);
                }
            }
        }
        Wall(0,65,12,34,18,8,true);Tower(-17,65,12,4,23,13,-1);Tower(17,65,12,4,23,13,-1);
        Wall(0,105,34,31,23,6,true);Tower(-16,105,34,3.8f,29,17,-1);Tower(16,105,34,3.8f,29,17,-1);
        // Accessible sloping processional causeway through the escarpment corridor.
        var ramp=Box("Uphill castle causeway",new Vector3(0,39,108),new Vector3(13,1,104),stone,true);ramp.transform.rotation=Quaternion.Euler(-31.2f,0,0);
        for(int j=0;j<11;j++){float z=66+j*8.7f,y=12+(z-63)/89*54;foreach(float s in new[]{-1f,1f})Box("Processional balustrade",new Vector3(s*7,y+1,z),new Vector3(.6f,2,9),ivory,true);}
        foreach(float x in new[]{-106f,-83,-60,-39,37,60,83,106})
        {
            float top=Height(x,109),bottom=2,z=78+Mathf.Abs(x)*.06f;float width=Mathf.Abs(x)<50?6:10;
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();for(int k=0;k<=30;k++){float u=k/30f;float y=Mathf.Lerp(bottom,top,u);float zz=z+Mathf.Pow(1-u,3)*-6;v.Add(new Vector3(x-width/2,y,zz));v.Add(new Vector3(x+width/2,y,zz));uv.Add(new Vector2(0,u));uv.Add(new Vector2(1,u));if(k<30){int n=k*2;t.AddRange(new[]{n,n+1,n+2,n+1,n+3,n+2});}}MeshObject("Broad falling water curtain",v,t,foam,null,uv);
            var pool=Shape("Turquoise plunge pool",PrimitiveType.Cylinder,new Vector3(x,1.65f,z-7),new Vector3(width*2.7f,.1f,18),water);
            for(int j=0;j<7;j++)Shape("Cascade spray and whitewater",PrimitiveType.Sphere,new Vector3(x+Random.Range(-width,width),2+Random.value*2,z-5-Random.value*7),new Vector3(2+Random.value*3,.4f,1.5f),ivory);
            // A feeder on the terrace makes every fall visibly originate at the cliff lip.
            Box("Feeder stream over cliff lip",new Vector3(x,top+.08f,z+10),new Vector3(width,.15f,20),water);
        }
    }
    static void Village()
    {
        for(int i=0;i<31;i++)
        {float z=Random.Range(-190,40),x=(i%2==0?-1:1)*Random.Range(100,205);if(Mathf.Abs(x-RiverX(z))<38)continue;float y=Height(x,z);House(x,z,y,Random.Range(3.5f,6));}
        for(int i=0;i<18;i++)
        {
            float z=-200+(i/2)*27,x=i%2==0?-135:130,y=Height(x,z)+.08f;float w=Random.Range(15,27),d=18;
            var earth=Mat("Field earth "+i,i%3==0?"#9a9e55":"#7d9851");Box("Cultivated field",new Vector3(x,y,z),new Vector3(w,.12f,d),earth);
            var crop=Mat("Crop rows "+i,i%3==0?"#c1c67a":"#8db557");for(float row=-w/2;row<w/2;row+=1.35f)Box("Parallel crop furrow",new Vector3(x+row,y+.15f,z),new Vector3(.5f,.25f,d),crop);
            foreach(float side in new[]{-1f,1f})for(float dz=-d/2;dz<=d/2;dz+=4){Box("Farm fence post",new Vector3(x+side*w/2,y+.8f,z+dz),new Vector3(.17f,1.6f,.17f),wood);}
            foreach(float side in new[]{-1f,1f})Box("Farm fence rail",new Vector3(x+side*w/2,y+.9f,z),new Vector3(.12f,.12f,d),wood);
        }
    }
    static void House(float x,float z,float y,float size)
    {
        float h=size*.9f;Box("Valley farmhouse",new Vector3(x,y+h/2,z),new Vector3(size,h,size*.8f),plaster,true);
        for(float dx=-size/2;dx<=size/2;dx+=size/2)Box("Timber farmhouse upright",new Vector3(x+dx,y+h/2,z-size*.4f-.05f),new Vector3(.18f,h,.18f),wood);
        Box("Oak farmhouse door",new Vector3(x,y+1,z-size*.4f-.1f),new Vector3(.9f,2,.15f),wood);
        var v=new List<Vector3>();var t=new List<int>();Quad(v,t,new Vector3(x-size*.6f,y+h,z-size*.5f),new Vector3(x-size*.6f,y+h,z+size*.5f),new Vector3(x,y+h+size*.65f,z+size*.5f),new Vector3(x,y+h+size*.65f,z-size*.5f));Quad(v,t,new Vector3(x,y+h+size*.65f,z-size*.5f),new Vector3(x,y+h+size*.65f,z+size*.5f),new Vector3(x+size*.6f,y+h,z+size*.5f),new Vector3(x+size*.6f,y+h,z-size*.5f));MeshObject("Farmhouse pitched roof",v,t,wood);
        Box("Stone chimney",new Vector3(x+size*.27f,y+h+size*.4f,z+.4f),new Vector3(.7f,3,.7f),stone);
    }
    static GameObject Plant(string path,Vector3 p,float height,float yaw=0)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)return null;var g=(GameObject)PrefabUtility.InstantiatePrefab(asset);g.transform.SetParent(group);g.transform.position=p;g.transform.rotation=Quaternion.Euler(0,yaw,0)*g.transform.rotation;
        var renderers=g.GetComponentsInChildren<Renderer>();var bounds=new Bounds(p,Vector3.zero);foreach(var r in renderers)bounds.Encapsulate(r.bounds);float scale=height/Mathf.Max(.01f,bounds.size.y);g.transform.localScale*=scale;bounds=new Bounds(p,Vector3.zero);foreach(var r in renderers)bounds.Encapsulate(r.bounds);g.transform.position+=Vector3.up*(p.y-bounds.min.y);
        foreach(var r in renderers){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){string n=mats[i]?mats[i].name:"";var m=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Reference textured "+n+".mat");if(m)mats[i]=m;}r.sharedMaterials=mats;}return g;
    }
    static void Vegetation()
    {
        string[] trees={"NormalTree_1","NormalTree_2","NormalTree_3","BirchTree_1"};
        for(int i=0;i<680;i++)
        {
            float x=Random.Range(-330,330),z=Random.Range(-320,330),y=Height(x,z);if(y<2||Mathf.Abs(x)<125&&z>60&&z<247)continue;
            if(z<45&&Mathf.Abs(x)<185&&Random.value<.82f)continue;
            if(z<-215&&Mathf.Abs(x)<58)continue;
            Plant("Assets/Art/StylizedNature/"+trees[i%4]+".fbx",new Vector3(x,y-.15f,z),Random.Range(6,15),Random.Range(0,360));
        }
        for(int i=0;i<100;i++)
        {float x=Random.Range(-110,110),z=Random.Range(96,126);if(Mathf.Abs(x)<12)continue;Plant("Assets/Art/StylizedNature/Bush_Large.fbx",new Vector3(x,Height(x,z),z),Random.Range(1.4f,3),Random.Range(0,360));}
        // Foreground framing rocks have faceted contours rather than primitive spheres.
        for(int i=0;i<21;i++)
        {float x=(i%2==0?-1:1)*Random.Range(42,100),z=Random.Range(-285,-198),y=Height(x,z);var g=Shape("Overlook weathered boulder",PrimitiveType.Sphere,new Vector3(x,y+Random.Range(0,3),z),new Vector3(Random.Range(5,15),Random.Range(3,8),Random.Range(4,12)),rock);g.transform.rotation=Random.rotation;}
    }
    static void Flowers()
    {
        var v=new List<Vector3>();var t=new List<int>();var colors=new List<Color>();
        for(int i=0;i<6800;i++)
        {float x=Random.Range(-100,100),z=Random.Range(-318,-215);if(Mathf.Abs(x+18)<10&&z>-283&&z<-230)continue;float y=Height(x,z);if(y<2)continue;float h=Random.Range(.25f,.75f);Vector3 p=new Vector3(x,y+h,z);int start=v.Count;Color c=i%7==0?C("#e6a9c8"):i%5==0?C("#e6d67a"):C("#f3f1d9");for(int k=0;k<5;k++){float a=k*Mathf.PI*.4f;Vector3 q=p+new Vector3(Mathf.Sin(a)*.12f,0,Mathf.Cos(a)*.12f);Quad(v,t,q+new Vector3(-.16f,0,0),q+new Vector3(0,.14f,0),q+new Vector3(.16f,0,0),q+new Vector3(0,-.14f,0));}while(colors.Count<v.Count)colors.Add(c);}
        MeshObject("White daisies, pink flowers and buttercups",v,t,grass,colors);
        var path=Mat("Overlook warm footpath","#b7aa7b");Box("Foreground traveller path",new Vector3(-18,Height(-18,-262)+.12f,-262),new Vector3(60,.2f,7),path);
    }
    static void Clouds()
    {
        // Soft volumetric impostors: ray-depth shaded unions of many lobes, generated locally.
        // They form tall cumulus columns in world space, leaving the castle fully three-dimensional.
        int size=512;var tex=new Texture2D(size,size,TextureFormat.RGBA32,false);var pixels=new Color[size*size];var lobes=new List<Vector3>();Random.InitState(418);
        for(int i=0;i<95;i++){float y=Random.Range(.1f,.9f),width=Mathf.Lerp(.44f,.23f,y);lobes.Add(new Vector3(.5f+Random.Range(-width,width)*.6f,y,Random.Range(.055f,.145f)));}
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {float px=x/(float)size,py=y/(float)size,depth=-1,edge=0,total=0;Vector3 best=Vector3.zero;foreach(var l in lobes){float dx=(px-l.x)*1.05f,dy=py-l.y,d2=dx*dx+dy*dy;float rr=l.z*(.94f+.1f*Mathf.PerlinNoise(px*88,py*88));if(d2<rr*rr){float dz=Mathf.Sqrt(rr*rr-d2);float weight=Mathf.Exp(dz*55)*Mathf.SmoothStep(0,.025f,rr-Mathf.Sqrt(d2));best+=new Vector3(dx,dy,dz)/rr*weight;total+=weight;depth=Mathf.Max(depth,dz);edge=Mathf.Max(edge,Mathf.Clamp01((rr-Mathf.Sqrt(d2))*230));}}if(depth<0){pixels[y*size+x]=Color.clear;continue;}best=best.normalized;float light=Mathf.Clamp01(Vector3.Dot(best,new Vector3(.75f,.5f,.55f).normalized)*.55f+.4f);float noise=(Mathf.PerlinNoise(px*90,py*90)-.5f)*.075f;Color c=Color.Lerp(C("#9ab6d7"),C("#fff1cf"),Mathf.Clamp01(light+noise));c.a=edge;pixels[y*size+x]=c;}
        tex.SetPixels(pixels);tex.Apply();File.WriteAllBytes(Dir+"/Cumulus.png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(Dir+"/Cumulus.png");
        var importer=(TextureImporter)AssetImporter.GetAtPath(Dir+"/Cumulus.png");importer.alphaIsTransparency=true;importer.sRGBTexture=true;importer.mipmapEnabled=true;importer.SaveAndReimport();
        var m=Mat("Cumulus sunlight","#ffffff","Universal Render Pipeline/Unlit");m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/Cumulus.png");m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.SetFloat("_Cull",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;EditorUtility.SetDirty(m);
        foreach(var entry in new[]{new Vector4(-227,232,420,1.2f),new Vector4(246,180,470,1.04f),new Vector4(-390,132,580,.9f),new Vector4(410,111,580,.82f)})
        {float w=235*entry.w,h=365*entry.w;var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};Quad(v,t,new Vector3(entry.x-w/2,entry.y-h/2,entry.z),new Vector3(entry.x+w/2,entry.y-h/2,entry.z),new Vector3(entry.x+w/2,entry.y+h/2,entry.z),new Vector3(entry.x-w/2,entry.y+h/2,entry.z));var g=MeshObject("Towering cream and lavender cumulus",v,t,m,null,uv);g.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}
    }
    static void Birds()
    {
        var v=new List<Vector3>();var t=new List<int>();for(int i=0;i<42;i++){var p=new Vector3(Random.Range(-95,90),Random.Range(190,310),Random.Range(110,280));float s=Random.Range(.6f,1.5f);int n=v.Count;v.AddRange(new[]{p+new Vector3(-s,.4f,0),p,p+new Vector3(-s*.3f,.05f,-.25f),p,p+new Vector3(s,.4f,0),p+new Vector3(s*.3f,.05f,-.25f)});t.AddRange(new[]{n,n+1,n+2,n+3,n+4,n+5});}MeshObject("White birds circling the upper spires",v,t,ivory);
    }
    static void Rider()
    {
        Vector3 p=new Vector3(-23,Height(-23,-253),-253);var horse=Plant("Assets/Art/Animals/Horse.fbx",p,4.3f,5);
        if(horse)
        {
            horse.name="Brown horse on the flower overlook";
            foreach(var r in horse.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){var original=mats[i];var m=Mat("Horse material "+serial++,"#875e42");if(original&&original.HasProperty("_Color"))m.color=original.color;r.sharedMaterials=mats;mats[i]=m;}r.sharedMaterials=mats;}
        }
        // Scenic armoured rider, seated astride the imported horse, facing the citadel.
        Material steel=Mat("Rider blue armour","#406a7e"),cloak=Mat("Rider dark blue cloak","#30596d");
        Vector3 seat=p+new Vector3(0,3,0);Shape("Mounted knight breastplate",PrimitiveType.Capsule,seat+Vector3.up*.5f,new Vector3(.85f,1,.6f),steel);Shape("Mounted knight closed helmet",PrimitiveType.Sphere,seat+Vector3.up*1.55f,new Vector3(.72f,.82f,.72f),steel);
        foreach(float s in new[]{-1f,1f}){Beam("Rider upper arm",seat+new Vector3(s*.5f,.8f,0),seat+new Vector3(s*.5f,.1f,.4f),.24f,steel);Beam("Rider seated thigh",seat+new Vector3(s*.3f,0,0),seat+new Vector3(s*.8f,-.4f,.5f),.32f,steel);Beam("Rider boot",seat+new Vector3(s*.8f,-.4f,.5f),seat+new Vector3(s*.8f,-1.2f,.25f),.28f,wood);}
        var v=new List<Vector3>();var t=new List<int>();Quad(v,t,seat+new Vector3(-.5f,1,-.35f),seat+new Vector3(.5f,1,-.35f),seat+new Vector3(.65f,-1,-.5f),seat+new Vector3(-.65f,-1,-.5f));cloak.SetFloat("_Cull",0);MeshObject("Mounted traveller cloak",v,t,cloak);
    }
    static void CameraAndControls()
    {
        var cam=new GameObject("Main Camera • reference overlook").AddComponent<Camera>();cam.tag="MainCamera";cam.nearClipPlane=.15f;cam.farClipPlane=1800;cam.backgroundColor=C("#62add8");cam.clearFlags=CameraClearFlags.SolidColor;cam.allowHDR=true;cam.GetUniversalAdditionalCameraData().renderPostProcessing=true;cam.gameObject.AddComponent<AudioListener>();
        var controller=new GameObject("Reference view and exploration controls").AddComponent<CastleReferenceView>();controller.view=cam;var walker=new GameObject("Walking visitor");walker.transform.position=BridgePoint(.03f)+Vector3.up*1.5f;var body=walker.AddComponent<CharacterController>();body.height=1.8f;body.radius=.35f;body.center=Vector3.up*.9f;body.stepOffset=.35f;body.slopeLimit=48;walker.SetActive(false);controller.walker=walker.transform;
        cam.transform.position=controller.picturePosition;cam.transform.LookAt(controller.pictureTarget);cam.fieldOfView=48;EditorUtility.SetDirty(controller);
    }
    static void CombineArchitecture()
    {
        // Bake fine repeated masonry into material batches while retaining editable source objects.
        var source=GameObject.Find("02 • Nested castle terraces and soaring towers");var batch=new GameObject("Castle • efficient material batches");batch.transform.parent=root;
        var meshes=source.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<MeshRenderer>()).GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial);
        foreach(var set in meshes)
        {var combines=set.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray();var mesh=new Mesh{name="Citadel masonry batch",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combines,true,true);AssetDatabase.CreateAsset(mesh,Dir+"/Meshes/Batch"+(serial++)+".asset");var g=new GameObject(set.Key.name);g.transform.parent=batch.transform;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=set.Key;foreach(var f in set)f.GetComponent<MeshRenderer>().enabled=false;}
    }
    public static void Capture()
    {
        var cam=Camera.main;var old=cam.targetTexture;var rt=new RenderTexture(1000,1400,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(1000,1400,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1000,1400),0,0);t.Apply();Directory.CreateDirectory("D:/GAME/artifacts/reference-castle");File.WriteAllBytes("D:/GAME/artifacts/reference-castle/portrait.png",t.EncodeToPNG());cam.targetTexture=old;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);Debug.Log("REFERENCE_CAPTURED");
    }
    public static void Refine()
    {
        serial=10000;root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;
        var terrain=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshFilter>().sharedMesh;var v=terrain.vertices;for(int i=0;i<v.Length;i++)v[i].y=Height(v[i].x,v[i].z);terrain.vertices=v;terrain.RecalculateNormals();terrain.RecalculateBounds();EditorUtility.SetDirty(terrain);var col=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshCollider>();col.sharedMesh=null;col.sharedMesh=terrain;
        var cache=new Dictionary<string,Material>();foreach(var r in root.GetComponentsInChildren<Renderer>())
        {
            var mats=r.sharedMaterials;bool changed=false;for(int i=0;i<mats.Length;i++)
            {
                string n=mats[i]?mats[i].name.Replace(" (Instance)",""):"";if(!n.Contains("Tree")&&!n.Contains("Birch")&&!n.Contains("Bush"))continue;
                string texture=n.Contains("Bush")?"Bush_Leaves":n;var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/StylizedNature/"+texture+".png");if(!tex)continue;
                if(!cache.TryGetValue(n,out var m)){m=Mat("Reference textured "+n,"#ffffff");m.mainTexture=tex;if(n.Contains("Leaves")||n.Contains("Bush")){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.45f);m.SetFloat("_Cull",0);m.EnableKeyword("_ALPHATEST_ON");m.SetOverrideTag("RenderType","TransparentCutout");m.renderQueue=2450;m.color=new Color(.83f,.94f,.77f);}cache[n]=m;EditorUtility.SetDirty(m);}mats[i]=m;changed=true;
            }if(changed)r.sharedMaterials=mats;
        }
        group=GameObject.Find("05 • Towering painted cumulus and birds").transform;foreach(var tr in group.GetComponentsInChildren<Transform>().Where(t=>t.name=="Towering cream and lavender cumulus").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);Clouds();
        var sky=Mat("Reference blue and golden sky","#ffffff","Aurelia/Reference Sky");RenderSettings.skybox=sky;Camera.main.clearFlags=CameraClearFlags.Skybox;
        var controls=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();controls.picturePosition=new Vector3(-8,18,-330);controls.pictureTarget=new Vector3(0,85,125);controls.Picture();EditorUtility.SetDirty(controls);
        // Remove bright feeders floating over the earlier sloped cliff; the new vertical lip is at z=84.
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Feeder stream over cliff lip").ToArray()){var p=tr.position;p.z=94;tr.position=p;}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("REFERENCE_REFINED");
    }
    public static void FinishComposition()
    {
        serial=11000;Random.InitState(19032);root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;
        stone=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Cool ivory limestone.mat");ivory=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Sunlit carved limestone.mat");grass=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Valley vertex palette.mat");rock=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Stratified grey cliff.mat");
        RenderSettings.ambientSkyColor=C("#698fae");RenderSettings.ambientEquatorColor=C("#788e7c");RenderSettings.ambientGroundColor=C("#3e5336");RenderSettings.sun.intensity=1.5f;RenderSettings.sun.transform.rotation=Quaternion.Euler(38,-130,0);RenderSettings.fogDensity=.00055f;
        var sky=RenderSettings.skybox;if(sky)sky.shader=Shader.Find("Aurelia/Reference Sky");EditorUtility.SetDirty(sky);
        var terrain=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshFilter>().sharedMesh;var verts=terrain.vertices;var colors=terrain.colors;
        for(int i=0;i<verts.Length;i++){float x=verts[i].x,z=verts[i].z;verts[i].y=Height(x,z);float n=N(x,z);Color c=Color.Lerp(C("#3b752e"),C("#98be55"),n);if(z>80&&z<99&&Mathf.Abs(x)<125&&verts[i].y>17)c=Color.Lerp(C("#58716d"),C("#6d9846"),Mathf.Pow(n,2));colors[i]=c;}terrain.vertices=verts;terrain.colors=colors;terrain.RecalculateNormals();terrain.RecalculateBounds();EditorUtility.SetDirty(terrain);var collider=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=terrain;
        // Raise the central silhouette and slim the broad lower curtain in the reference's proportions.
        foreach(string name in new[]{"02 • Nested castle terraces and soaring towers","Castle • efficient material batches"})
        {var g=GameObject.Find(name).transform;g.position=new Vector3(0,-13.2f,0);g.localScale=new Vector3(.86f,1.2f,1);}
        var controls=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();controls.picturePosition=new Vector3(-8,30,-330);controls.pictureTarget=new Vector3(0,96,125);controls.Picture();Camera.main.clearFlags=CameraClearFlags.Skybox;EditorUtility.SetDirty(controls);
        Group("07 • Escarpment ledges, hanging gardens and river foam");
        var greens=new[]{Mat("Cliff greenery moss","#4d803d"),Mat("Cliff sunlit greenery","#83a84c"),Mat("Cliff blue shadow","#627e73"),Mat("Cliff exposed limestone","#9aab95")};
        for(int i=0;i<180;i++)
        {
            float x=Random.Range(-118,118);if(Mathf.Abs(x)<21)continue;float z=Random.Range(84,94),y=Random.Range(5,64);if(new[]{-106f,-83,-60,-39,37,60,83,106}.Any(f=>Mathf.Abs(x-f)<5))continue;
            // Jagged vertical slabs use tapered irregular rings, avoiding smooth primitive cliff faces.
            float radius=Random.Range(2,6),h=Random.Range(5,19);Cylinder("Eroded cliff buttress",new Vector3(x,y,z),radius,radius*Random.Range(.55f,.95f),h,greens[i%4],7);
            if(i%3==0){var foliage=Shape("Hanging cliff foliage",PrimitiveType.Sphere,new Vector3(x,y+h,z-2),new Vector3(radius*2,1.6f,radius*1.5f),greens[i%2]);foliage.transform.rotation=Random.rotation;}
        }
        // Fine ledges and creeping planted ribbons anchor the castle into the rock.
        for(int side=-1;side<=1;side+=2)for(int i=0;i<28;i++)
        {float x=side*Random.Range(25,108),z=Random.Range(92,99);Plant("Assets/Art/StylizedNature/Bush_Large.fbx",new Vector3(x,65,z),Random.Range(1.5f,3.7f),Random.Range(0,360));}
        // Visible valley houses and field strips occupy the two banks inside the portrait frame.
        wood=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Weathered oak.mat");plaster=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Farmhouse plaster.mat");
        for(int i=0;i<20;i++){float z=Random.Range(-175,25),x=(i%2==0?-1:1)*Random.Range(55,105);if(Mathf.Abs(x-RiverX(z))<38)continue;House(x,z,Height(x,z),Random.Range(2.5f,4));}
        var farmland=Mat("Sunlit valley grain","#a7b85b");for(int i=0;i<24;i++){float z=Random.Range(-180,15),x=(i%2==0?-1:1)*Random.Range(65,108);if(Mathf.Abs(x-RiverX(z))<36)continue;for(int k=0;k<8;k++)Box("Valley grain furrow",new Vector3(x+k*.8f,Height(x+k*.8f,z)+.1f,z),new Vector3(.35f,.2f,10),farmland);}
        // The foreground framing should never close the central water vista.
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("NormalTree")||t.name.StartsWith("BirchTree")).ToArray())if(tr.position.z<-60&&Mathf.Abs(tr.position.x-RiverX(tr.position.z))<34)UnityEngine.Object.DestroyImmediate(tr.gameObject);
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Towering cream and lavender cumulus")){var p=tr.position;p.x*=.85f;tr.position=p;}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("REFERENCE_COMPOSITION_FINISHED");
    }
    public static void WaterAndLight()
    {
        serial=12000;Random.InitState(108);root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;RenderSettings.sun.transform.rotation=Quaternion.Euler(38,-48,0);RenderSettings.sun.intensity=1.5f;
        var terrain=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshFilter>().sharedMesh;var vv=terrain.vertices;for(int i=0;i<vv.Length;i++)vv[i].y=Height(vv[i].x,vv[i].z);terrain.vertices=vv;terrain.RecalculateNormals();EditorUtility.SetDirty(terrain);var tc=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshCollider>();tc.sharedMesh=null;tc.sharedMesh=terrain;
        water=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Turquoise river.mat");water.shader=Shader.Find("Aurelia/Reference River");water.color=C("#2eac98");EditorUtility.SetDirty(water);
        group=GameObject.Find("03 • Rivergate, arched bridge and falling water").transform;
        var v=new List<Vector3>();var t=new List<int>();v.Add(new Vector3(0,1.65f,59));for(int i=0;i<=80;i++){float a=i*Mathf.PI*2/80;v.Add(new Vector3(Mathf.Sin(a)*126,1.65f,59+Mathf.Cos(a)*23));if(i<80)t.AddRange(new[]{0,i+2,i+1});}MeshObject("Wide connected waterfall basin",v,t,water);
        rock=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Stratified grey cliff.mat");rock.color=C("#7a9284");EditorUtility.SetDirty(rock);
        foreach(var f in root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Eroded cliff buttress"))
        {
            f.GetComponent<Renderer>().sharedMaterial=rock;var mesh=f.sharedMesh;var pts=mesh.vertices;for(int i=0;i<pts.Length;i++){pts[i].y=Mathf.Clamp(pts[i].y,3,65);pts[i].z=80+pts[i].y*.13f+Mathf.PerlinNoise(pts[i].x*.16f,pts[i].y*.1f)*5;pts[i].x+=Mathf.PerlinNoise(pts[i].y*.2f,pts[i].x*.1f)*1.5f;}mesh.vertices=pts;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
        }
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(tr=>tr.name=="Hanging cliff foliage").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        // Keep the mounted traveller fully inside the lower-left edge of the composition.
        var overview=GameObject.Find("06 • Flower overlook and mounted traveller").transform;
        foreach(var tr in overview.Cast<Transform>().Where(tr=>tr.name.Contains("horse")||tr.name.StartsWith("Mounted")||tr.name.StartsWith("Rider")))tr.position+=new Vector3(-2,0,30);
        var controls=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();controls.picturePosition=new Vector3(-8,24,-330);controls.pictureTarget=new Vector3(0,89,125);controls.Picture();Camera.main.clearFlags=CameraClearFlags.Skybox;EditorUtility.SetDirty(controls);
        // New bushes use the same texture setup as the established nature assets.
        foreach(var r in root.GetComponentsInChildren<Renderer>())
        {var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){string n=mats[i]?mats[i].name:"";if(n.Contains("Bush")&&!n.StartsWith("Reference")){var m=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Reference textured "+n+".mat");if(!m){m=Mat("Reference textured "+n,"#779a50");m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/StylizedNature/Bush_Leaves.png");m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.45f);m.SetFloat("_Cull",0);m.EnableKeyword("_ALPHATEST_ON");m.renderQueue=2450;EditorUtility.SetDirty(m);}mats[i]=m;}}r.sharedMaterials=mats;}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("REFERENCE_WATER_LIGHT_FINISHED");
    }
    public static void ScenicPolish()
    {
        serial=13000;Random.InitState(443);root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;
        var terrain=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshFilter>().sharedMesh;var vv=terrain.vertices;for(int i=0;i<vv.Length;i++)vv[i].y=Height(vv[i].x,vv[i].z);terrain.vertices=vv;terrain.RecalculateNormals();EditorUtility.SetDirty(terrain);var tc=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshCollider>();tc.sharedMesh=null;tc.sharedMesh=terrain;
        var river=GameObject.Find("Meandering turquoise river").GetComponent<MeshFilter>().sharedMesh;vv=river.vertices;for(int i=0;i<vv.Length;i++)vv[i].x=RiverX(vv[i].z)+(i%2==0?-27:27);river.vertices=vv;river.RecalculateBounds();EditorUtility.SetDirty(river);
        foreach(var f in root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Snow brushed mountain")))
        {var mesh=f.sharedMesh;var pts=mesh.vertices;var c=mesh.colors;for(int i=0;i<pts.Length;i++){float snow=Mathf.SmoothStep(0,1,Mathf.InverseLerp(100,152,pts[i].y+N(pts[i].x*4,pts[i].z*4)*28));c[i]=Color.Lerp(C("#5f9eb4"),C("#f3f7ed"),snow);}mesh.colors=c;EditorUtility.SetDirty(mesh);}
        var controls=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();controls.picturePosition=new Vector3(-8,35,-330);controls.pictureTarget=new Vector3(0,85,125);controls.Picture();Camera.main.clearFlags=CameraClearFlags.Skybox;EditorUtility.SetDirty(controls);
        var overview=GameObject.Find("06 • Flower overlook and mounted traveller").transform;var horse=GameObject.Find("Brown horse on the flower overlook").transform;
        Vector3 offset=new Vector3(-23,Height(-23,-253),-253)-horse.position;
        foreach(var tr in overview.Cast<Transform>().Where(tr=>tr.name.Contains("horse")||tr.name.StartsWith("Mounted")||tr.name.StartsWith("Rider")))tr.position+=offset;
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Overlook warm footpath").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        var bridgeSource=GameObject.Find("03 • Rivergate, arched bridge and falling water").transform;var bridge=new GameObject("Reference diagonal bridge assembly").transform;bridge.parent=root;bridge.position=new Vector3(0,12,64);var oldDir=new Vector3(-100,0,248);bridge.rotation=Quaternion.LookRotation(oldDir);
        foreach(var tr in bridgeSource.Cast<Transform>().Where(tr=>tr.name.Contains("bridge")||tr.name=="Bridge stone pier"||tr.name.StartsWith("Bridge parapet")||tr.name=="Bridge post capital").ToArray())tr.SetParent(bridge,true);
        var newDir=new Vector3(-50,0,309);bridge.rotation=Quaternion.LookRotation(newDir);bridge.localScale=new Vector3(1,1,newDir.magnitude/oldDir.magnitude);controls.walker.position=new Vector3(48.5f,13.5f,-236);
        Group("08 • Dense valley woodland and foreground gardens");
        foreach(var tr in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("NormalTree")||t.name.StartsWith("BirchTree")).ToArray())if(tr.position.z<-70&&Mathf.Abs(tr.position.x)<55)UnityEngine.Object.DestroyImmediate(tr.gameObject);
        for(int i=0;i<270;i++)
        {
            float z=Random.Range(-250,62),x=(i%2==0?-1:1)*Random.Range(66,145);if(Mathf.Abs(x-RiverX(z))<34)continue;
            Plant("Assets/Art/StylizedNature/NormalTree_"+(i%3+1)+".fbx",new Vector3(x,Height(x,z),z),Random.Range(4,10),Random.Range(0,360));
        }
        rock=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Stratified grey cliff.mat");
        foreach(float side in new[]{-1f,1f})for(int i=0;i<9;i++)
        {float x=side*Random.Range(24,44),z=Random.Range(-290,-258);Shape("Flower overlook framing rock",PrimitiveType.Sphere,new Vector3(x,Height(x,z)+1,z),new Vector3(Random.Range(4,9),Random.Range(2,5),Random.Range(3,7)),rock);}
        // Fill the upper gate's sheltered interior; opening remains clear for the walking route.
        group=GameObject.Find("02 • Nested castle terraces and soaring towers").transform;dark=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Deep arch interior.mat");
        Box("Shadow inside the upper gate",new Vector3(0,72,113),new Vector3(15,18,.1f),dark);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("REFERENCE_SCENIC_POLISH_DONE");
    }
    public static void LivingBanks()
    {
        serial=14000;Random.InitState(332);root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;Group("09 • Riverbank thickets and meadow detail");
        for(int i=0;i<260;i++)
        {
            float z=Random.Range(-200,65),x=Random.Range(-100,112);float distance=Mathf.Abs(x-RiverX(z));if(distance<31||distance>68)continue;
            if(N(x*3,z*3)<.42f)continue;Plant("Assets/Art/StylizedNature/NormalTree_"+(i%3+1)+".fbx",new Vector3(x,Height(x,z),z),Random.Range(7,13),Random.Range(0,360));
        }
        grass=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Valley vertex palette.mat");var v=new List<Vector3>();var t=new List<int>();var colors=new List<Color>();
        for(int i=0;i<38000;i++)
        {float x=Random.Range(-120,120),z=Random.Range(-315,60),y=Height(x,z);if(y<2||Mathf.Abs(x-RiverX(z))<29)continue;float height=Random.Range(.22f,.65f),w=.08f;var p=new Vector3(x,y,z);int n=v.Count;v.AddRange(new[]{p-Vector3.right*w,p+Vector3.right*w,p+new Vector3(.12f,height,0)});t.AddRange(new[]{n,n+1,n+2});Color c=Color.Lerp(C("#56883c"),C("#a6bd62"),Random.value);colors.Add(c);colors.Add(c);colors.Add(c);}
        MeshObject("Fine mixed meadow grass blades",v,t,grass,colors);
        // A shaded vaulted entry has sufficient depth to stay dark from the overlook.
        var interior=Mat("Gate vaulted interior","#244b51");Box("Upper gate inner vault",new Vector3(0,80,112),new Vector3(14,30,2),interior);
        // An unlit vertex palette preserves the distant range's cool blue and white snow.
        foreach(var f in root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Snow brushed mountain")))
        {var mesh=f.sharedMesh;var pts=mesh.vertices;var c=mesh.colors;for(int i=0;i<pts.Length;i++){float snow=Mathf.SmoothStep(0,1,Mathf.InverseLerp(120,170,pts[i].y+N(pts[i].x*4,pts[i].z*4)*28));c[i]=Color.Lerp(C("#4d8ea9"),C("#edf4ec"),snow);}mesh.colors=c;EditorUtility.SetDirty(mesh);}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("REFERENCE_BANKS_FINISHED");
    }
    [MenuItem("Aurelia/Rebuild complete reference citadel")]
    public static void BuildCompleteReference()
    {
        Build();Refine();FinishComposition();WaterAndLight();ScenicPolish();LivingBanks();CloseRoofSeams();
        RenderSettings.skybox.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(Dir+"/PaintedSky.png"));EditorUtility.SetDirty(RenderSettings.skybox);
        foreach(var tr in GameObject.Find("05 • Towering painted cumulus and birds").GetComponentsInChildren<Transform>().Where(tr=>tr.name=="Towering cream and lavender cumulus").ToArray())UnityEngine.Object.DestroyImmediate(tr.gameObject);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    public static void Validate()
    {
        var scene=EditorSceneManager.GetActiveScene();EditorSceneManager.SaveScene(scene);EditorSceneManager.OpenScene(scene.path);
        var report=new System.Text.StringBuilder();var objects=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        int missing=objects.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));report.AppendLine((missing==0?"PASS":"FAIL")+" scene save/reopen: missing scripts="+missing);
        var controls=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();report.AppendLine((controls&&controls.walker&&controls.view?"PASS":"FAIL")+" camera and walker references persisted");
        int broken=UnityEngine.Object.FindObjectsByType<Renderer>().Count(r=>r.sharedMaterials.Any(m=>m==null||m.shader==null||m.shader.name=="Hidden/InternalErrorShader"));report.AppendLine((broken==0?"PASS":"FAIL")+" material/shader references: broken="+broken);
        foreach(string n in new[]{"Broad falling water curtain","White birds circling the upper spires","Brown horse on the flower overlook","Valley farmhouse","Farm fence post","Fine mixed meadow grass blades","Long continuous stone bridge deck"})report.AppendLine((objects.Any(o=>o.name==n||(n=="Valley farmhouse"&&o.name.StartsWith("Inhabited valley cottage")))?"PASS":"FAIL")+" feature present: "+n);
        // Downward samples verify a continuous, collision-bearing bridge and processional ramp.
        Physics.SyncTransforms();int gaps=0;for(int i=0;i<=65;i++){float f=i/65f;Vector3 p=Vector3.Lerp(new Vector3(50,12,-245),new Vector3(0,12,64),f);if(!Physics.Raycast(p+Vector3.up*5,Vector3.down,out var hit,10)||hit.point.y<11)gaps++;}report.AppendLine((gaps==0?"PASS":"FAIL")+" bridge centreline collision: gaps="+gaps);
        gaps=0;for(int i=0;i<=40;i++){float z=64+i*88f/40,y=12+(z-63)/89*54;if(!Physics.Raycast(new Vector3(0,y+3,z),Vector3.down,out var hit,6))gaps++;}report.AppendLine((gaps==0?"PASS":"FAIL")+" uphill causeway collision: gaps="+gaps);
        Directory.CreateDirectory("D:/GAME/artifacts/reference-castle");File.WriteAllText("D:/GAME/artifacts/reference-castle/editor-validation.txt",report.ToString());Debug.Log(report.ToString());
    }
    public static void CloseRoofSeams()
    {
        var source=GameObject.Find("02 • Nested castle terraces and soaring towers");
        foreach(var f in source.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Overlapping turquoise roof course"))
        {
            var mesh=f.sharedMesh;var v=mesh.vertices.ToList();var t=mesh.triangles.ToList();if(v.Count!=168)continue;
            Vector3 centre=new Vector3(mesh.bounds.center.x,mesh.bounds.min.y,mesh.bounds.center.z);
            for(int i=0;i<24;i++){int n=v.Count;v.AddRange(new[]{centre,v[i*7+1],v[i*7]});t.AddRange(new[]{n,n+1,n+2});}mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
        }
        foreach(var f in source.GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Steep palace slate roof"))
        {var mesh=f.sharedMesh;var t=mesh.triangles.ToList();if(t.Count==12){t.AddRange(new[]{0,4,7,1,6,5});mesh.SetTriangles(t,0);mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);}}
        var batch=GameObject.Find("Castle • efficient material batches").transform;foreach(Transform child in batch.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
        int id=15000;foreach(var set in source.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<MeshRenderer>()).GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial))
        {var m=new Mesh{name="Closed roof citadel batch",indexFormat=IndexFormat.UInt32};m.CombineMeshes(set.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=batch.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray(),true,true);AssetDatabase.CreateAsset(m,Dir+"/Meshes/Closed"+(id++)+".asset");var g=new GameObject(set.Key.name);g.transform.SetParent(batch,false);g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=set.Key;foreach(var f in set)f.GetComponent<MeshRenderer>().enabled=false;}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();Debug.Log("REFERENCE_ROOFS_CLOSED");
    }
    public static void CaptureWide()
    {
        var cam=Camera.main;var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var t=new Texture2D(1600,1000,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1600,1000),0,0);t.Apply();File.WriteAllBytes("D:/GAME/artifacts/reference-castle/wide-editor.png",t.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(rt);
    }
}
