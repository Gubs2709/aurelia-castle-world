using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

public static class WorldArtPass
{
    const string Folder="Assets/World/ArtPass";
    static Material wood,stone,moss;
    static Transform group;
    static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
    static GameObject Block(string name,Vector3 p,Vector3 size,Material mat,Transform parent)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.parent=parent;g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static void Beam(Vector3 a,Vector3 b,float size,Material m,Transform parent)
    {var g=Block("Carved oak bracing",(a+b)/2,new Vector3(size,Vector3.Distance(a,b),size),m,parent);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    public static void Apply()
    {
        Random.InitState(7182);Directory.CreateDirectory(Folder);
        wood=GameObject.Find("Oak door").GetComponent<Renderer>().sharedMaterial;
        stone=GameObject.Find("Continuous viaduct deck").GetComponent<Renderer>().sharedMaterial;
        moss=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Weathered cliff & moss",color=C("#727c6c")};AssetDatabase.CreateAsset(moss,AssetDatabase.GenerateUniqueAssetPath(Folder+"/Cliff.mat"));
        group=new GameObject("09 • Landscape and architectural refinement").transform;
        Architecture();FineMeadows();Cliffs();MountainRanges();ForestFloor();
        foreach(var m in Resources.FindObjectsOfTypeAll<Material>())if(m.name.StartsWith("Textured ")&&m.name.Contains("Leaves")){m.color=new Color(.72f,.8f,.75f);EditorUtility.SetDirty(m);}
        UnityEngine.RenderSettings.fogDensity=.0028f;
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        Debug.Log("ART_PASS_COMPLETE");
    }
    static void Architecture()
    {
        foreach(var roof in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(r=>r.name=="Steep tiled roof").ToArray())
        {
            var b=roof.sharedMesh.bounds;float width=b.size.x,depth=b.size.z,height=b.size.y;
            float angle=Mathf.Atan2(height,width/2)*Mathf.Rad2Deg,slope=Mathf.Sqrt(height*height+width*width/4);
            var mat=roof.GetComponent<Renderer>().sharedMaterial;
            foreach(float sign in new[]{-1f,1f}){var panel=Block("Solid overlapping slate roof",new Vector3(sign*width/4,height/2,0),new Vector3(slope+.2f,.25f,depth+.1f),mat,roof.transform);panel.transform.localRotation=Quaternion.Euler(0,0,-sign*angle);}
        }
        foreach(var house in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Briarwood • timber town house").ToArray())
        {
            var body=house.Find("Lime plaster walls");float w=body.localScale.x,h=body.localScale.y,d=body.localScale.z;
            foreach(float side in new[]{-1f,1f})
            {
                for(float y=2;y<h;y+=4)Block("Side floor beam",new Vector3(side*(w/2+.05f),y,0),new Vector3(.23f,.3f,d),wood,house);
                foreach(float z in new[]{-d/2+.1f,0,d/2-.1f})Block("Side upright",new Vector3(side*(w/2+.09f),h/2,z),new Vector3(.22f,h,.25f),wood,house);
                foreach(float z in new[]{-d*.25f,d*.25f})
                {Beam(new Vector3(side*(w/2+.1f),2,z-d*.2f),new Vector3(side*(w/2+.1f),5.9f,z+d*.2f),.2f,wood,house);}
            }
        }
    }
    static void FineMeadows()
    {
        foreach(var f in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.name=="Wildflower ribbon"))
        {
            var mesh=f.sharedMesh;var v=mesh.vertices;var colors=mesh.colors;
            for(int i=0;i+2<v.Length;)
            {
                if(colors[i].g>colors[i].r*1.15f&&colors[i].g>colors[i].b*1.3f)
                {Vector3 mid=(v[i]+v[i+1])/2;v[i]=Vector3.Lerp(mid,v[i],.23f);v[i+1]=Vector3.Lerp(mid,v[i+1],.23f);i+=3;}
                else i+=4;
            }
            mesh.vertices=v;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
        }
    }
    static Mesh JaggedRock(int seed)
    {
        var temp=GameObject.CreatePrimitive(PrimitiveType.Sphere);var mesh=UnityEngine.Object.Instantiate(temp.GetComponent<MeshFilter>().sharedMesh);UnityEngine.Object.DestroyImmediate(temp);
        var v=mesh.vertices;
        for(int i=0;i<v.Length;i++)
        {var p=v[i];float n=Mathf.PerlinNoise(p.x*6+seed*3,p.z*6+p.y*2+seed)*.24f;float strata=Mathf.Sin(p.y*35+seed)*.025f;p*=.85f+n;p.y=Mathf.Clamp(p.y,-.43f,.38f);p.x+=p.y*.16f;v[i]=p;}
        mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath(Folder+"/Crag.asset"));return mesh;
    }
    static void Cliffs()
    {
        var rocks=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name.StartsWith("rock_tall")&&t.parent!=null&&t.parent.name.Contains("Crown")).ToArray();
        for(int i=0;i<rocks.Length;i++)
        {
            var old=rocks[i];var renderers=old.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);old.gameObject.SetActive(false);
            var g=new GameObject("Weathered cliff outcrop");g.transform.parent=group;g.transform.position=b.center;g.transform.localScale=new Vector3(Mathf.Clamp(b.size.x*1.8f,18,30),b.size.y*1.1f,26);
            g.AddComponent<MeshFilter>().sharedMesh=JaggedRock(i);g.AddComponent<MeshRenderer>().sharedMaterial=moss;
        }
    }
    static void MountainRanges()
    {
        foreach(var old in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t=>t.name=="Eroded alpine massif").ToArray())old.gameObject.SetActive(false);
        var mat=new Material(Shader.Find("Aurelia/Meadow")){name="Atmospheric alpine ridges"};AssetDatabase.CreateAsset(mat,AssetDatabase.GenerateUniqueAssetPath(Folder+"/Alpine.mat"));
        for(int mountain=0;mountain<14;mountain++)
        {
            float a=mountain*Mathf.PI*2/14;Vector3 origin=new Vector3(Mathf.Sin(a)*590,-8,Mathf.Cos(a)*590);int n=52;float size=380,height=Random.Range(105,235);var v=new Vector3[(n+1)*(n+1)];var c=new Color[v.Length];var tri=new int[n*n*6];
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)
            {
                float xx=(x/(float)n-.5f)*size,zz=(z/(float)n-.5f)*size;
                float r=Mathf.Sqrt(xx*xx+zz*zz)/(size*.5f);float peaks=Mathf.Exp(-((xx+40)*(xx+40)+(zz-15)*(zz-15))/4400)+.7f*Mathf.Exp(-((xx-60)*(xx-60)+(zz+30)*(zz+30))/2800);
                float ridge=1-Mathf.Abs(Mathf.PerlinNoise(xx*.03f+mountain*7,zz*.03f+19)*2-1);float detail=Mathf.PerlinNoise(xx*.1f+90,zz*.1f+mountain);
                float y=height*peaks*(.6f+ridge*.6f)+detail*13*Mathf.Clamp01(1-r);
                int k=z*(n+1)+x;v[k]=new Vector3(xx,y,zz);c[k]=Color.Lerp(C("#718a97"),C("#d5e3e3"),Mathf.SmoothStep(.45f,.8f,y/height));
            }
            int t=0;for(int z=0;z<n;z++)for(int x=0;x<n;x++){int k=z*(n+1)+x;foreach(int j in new[]{k,k+n+1,k+1,k+1,k+n+1,k+n+2})tri[t++]=j;}
            var mesh=new Mesh{vertices=v,triangles=tri,colors=c};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath(Folder+"/Mountain.asset"));
            var go=new GameObject("Broken alpine ridgeline");go.transform.parent=group;go.transform.position=origin;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
        }
    }
    static void ForestFloor()
    {
        string[] names={"Bush_Small_Flowers","Flower_1_Clump","Flower_2_Clump","Grass_Small"};
        for(int i=0;i<700;i++)
        {
            float x,z;
            if(i<420){x=Random.Range(86,175);z=Random.Range(-94,2);if(Mathf.Abs(z+45)<4&&x<132)continue;}
            else{x=Random.Range(-1,1)>0?Random.Range(8,12):Random.Range(-12,-8);z=Random.Range(-210,-30);}
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/StylizedNature/"+names[i%4]+".fbx");if(!source)continue;
            var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.transform.parent=group;g.transform.position=new Vector3(x,KingdomBuilder.Height(x,z),z);g.transform.localScale*=Random.Range(.5f,1.3f);g.transform.rotation=Quaternion.Euler(0,Random.value*360,0)*g.transform.rotation;
            foreach(var r in g.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials=r.sharedMaterials.Select(m=>{
                    string key="Ground cover "+m.name;var path=Folder+"/"+key+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material)return material;
                    material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=key,color=new Color(.8f,.84f,.78f)};material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/StylizedNature/"+(m.name.Contains("Grass")?"Grass":m.name.Contains("Flower")?"Flowers":"Bush_Leaves")+".png");material.SetFloat("_AlphaClip",1);material.SetFloat("_Cutoff",.4f);material.SetFloat("_Cull",0);material.EnableKeyword("_ALPHATEST_ON");AssetDatabase.CreateAsset(material,path);return material;
                }).ToArray();
            }
        }
    }
}
