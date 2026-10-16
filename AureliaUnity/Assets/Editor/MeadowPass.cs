using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Dense swaying-style grass tufts across the valley, river reeds and extra varied bushes. Idempotent group.
public static class MeadowPass
{
    const string GroupName="26 • Meadow grass tufts, river reeds and shrubs",D="Assets/LivingWorld";
    static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
    static float RiverX(float z)=>18+Mathf.Sin((z+80)*.023f)*42;
    static float PathX(float z)=>-104+Mathf.Sin(z*.022f)*5;
    class Chunk{public List<Vector3> v=new List<Vector3>();public List<int> t=new List<int>();public List<Color> c=new List<Color>();}

    static void Tuft(Chunk ch,Vector3 p,float height,float width,int blades,Color baseCol,Color tipCol)
    {
        for(int b=0;b<blades;b++)
        {
            float yaw=Random.Range(0f,Mathf.PI*2),lean=Random.Range(.05f,.35f);var dir=new Vector3(Mathf.Cos(yaw),0,Mathf.Sin(yaw));var side=new Vector3(-dir.z,0,dir.x);
            float h=height*Random.Range(.55f,1.15f);Vector3 root=p+dir*Random.Range(0,.12f);int n=ch.v.Count;
            for(int j=0;j<3;j++){float u=j/2f;Vector3 c=root+Vector3.up*(h*u)+dir*(lean*u*u*h);float w=width*(1-u*.9f);ch.v.Add(c-side*w);ch.v.Add(c+side*w);var col=Color.Lerp(baseCol,tipCol,u);ch.c.Add(col);ch.c.Add(col);}
            int k=n;ch.t.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3,k+2,k+4,k+3,k+3,k+4,k+5});
        }
    }
    static Mesh SaveMesh(Chunk ch,string name)
    {
        var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(ch.v);m.SetTriangles(ch.t,0);m.SetColors(ch.c);m.RecalculateNormals();m.RecalculateBounds();
        string p=D+"/Meshes/"+name+".asset";var ex=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(ex){ex.Clear();EditorUtility.CopySerialized(m,ex);Object.DestroyImmediate(m);return ex;}AssetDatabase.CreateAsset(m,p);return m;
    }
    public static string Build()
    {
        var root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;var old=GameObject.Find(GroupName);if(old)Object.DestroyImmediate(old);
        var g=new GameObject(GroupName).transform;g.SetParent(root,false);Random.InitState(77);
        var houses=GameObject.Find("13 • Inhabited timber and stone hamlets").transform.Cast<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")).Select(t=>t.position).ToArray();
        var grassMat=AssetDatabase.LoadAssetAtPath<Material>(D+"/Fine woodland grass.mat");
        var chunks=new Dictionary<Vector2Int,Chunk>();var reeds=new Dictionary<Vector2Int,Chunk>();
        int tufts=0,reedClumps=0;
        for(int i=0;i<60000;i++)
        {
            float x=Random.Range(-195f,165f),z=Random.Range(-300f,56f);float y=ReferenceCastleBuilder.Height(x,z);if(y<2.4f)continue;
            if(z<62&&Mathf.Abs(x-RiverX(z))<26.5f)continue;if(z>-235&&z<-18&&Mathf.Abs(x-PathX(z))<2.6f)continue;bool near=false;foreach(var h in houses)if((h.x-x)*(h.x-x)+(h.z-z)*(h.z-z)<20){near=true;break;}if(near)continue;
            // slope check keeps tufts off cliffs
            float slope=Mathf.Abs(ReferenceCastleBuilder.Height(x+2,z)-y)+Mathf.Abs(ReferenceCastleBuilder.Height(x,z+2)-y);if(slope>3.2f)continue;
            var key=new Vector2Int(Mathf.FloorToInt(x/36),Mathf.FloorToInt(z/36));if(!chunks.TryGetValue(key,out var ch))chunks[key]=ch=new Chunk();
            float tone=Random.value;Color b=Color.Lerp(C("#3f6b2b"),C("#5f8f35"),tone),t=Color.Lerp(C("#8fb94c"),C("#d2d56a"),Random.value*.6f);
            Tuft(ch,new Vector3(x,y+.02f,z),Random.Range(.28f,.8f),Random.Range(.02f,.035f),5,b,t);tufts++;
        }
        // tall reeds hugging both river banks
        for(int i=0;i<2600;i++)
        {
            float z=Random.Range(-300f,56f),side=Random.value<.5f?-1:1;float x=RiverX(z)+side*Random.Range(25f,29.5f);float y=Mathf.Max(ReferenceCastleBuilder.Height(x,z),1.6f);
            var key=new Vector2Int(Mathf.FloorToInt(x/36),Mathf.FloorToInt(z/36));if(!reeds.TryGetValue(key,out var ch))reeds[key]=ch=new Chunk();
            Tuft(ch,new Vector3(x,y,z),Random.Range(.9f,1.9f),Random.Range(.03f,.05f),4,C("#4f7a32"),C("#b9c76a"));reedClumps++;
        }
        foreach(var kv in chunks){var mesh=SaveMesh(kv.Value,"Grass_"+kv.Key.x+"_"+kv.Key.y);var o=new GameObject("Grass tufts");o.transform.SetParent(g,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;var mr=o.AddComponent<MeshRenderer>();mr.sharedMaterial=grassMat;mr.shadowCastingMode=ShadowCastingMode.Off;}
        foreach(var kv in reeds){var mesh=SaveMesh(kv.Value,"Reeds_"+kv.Key.x+"_"+kv.Key.y);var o=new GameObject("River reeds");o.transform.SetParent(g,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;var mr=o.AddComponent<MeshRenderer>();mr.sharedMaterial=grassMat;mr.shadowCastingMode=ShadowCastingMode.Off;}
        // extra shrubs by species
        string[] names={"Summer oak","Pink cherry blossom","Crimson maple","Lavender wisteria","Golden woodland"};
        var batches=new List<CombineInstance>[5];for(int k=0;k<5;k++)batches[k]=new List<CombineInstance>();
        var canopy=new Mesh[5];for(int k=0;k<5;k++)canopy[k]=AssetDatabase.LoadAssetAtPath<Mesh>(D+"/Meshes/"+(k*2+1)+".asset");
        int shrubs=0;
        for(int i=0;i<2500&&shrubs<240;i++)
        {
            float x=Random.Range(-190f,160f),z=Random.Range(-290f,50f);float y=ReferenceCastleBuilder.Height(x,z);if(y<2.4f)continue;if(z<62&&Mathf.Abs(x-RiverX(z))<26f)continue;if(z>-235&&z<-18&&Mathf.Abs(x-PathX(z))<3.4f)continue;
            // prefer river banks and house gardens
            bool bank=z<62&&Mathf.Abs(x-RiverX(z))<40;bool garden=false;foreach(var h in houses)if((h.x-x)*(h.x-x)+(h.z-z)*(h.z-z)<260&&(h.x-x)*(h.x-x)+(h.z-z)*(h.z-z)>30){garden=true;break;}if(!bank&&!garden&&Random.value>.18f)continue;
            float r=Random.value;int kind=r<.45f?0:r<.65f?4:r<.8f?1:r<.9f?3:2;float size=Random.Range(.16f,.42f);
            batches[kind].Add(new CombineInstance{mesh=canopy[kind],transform=Matrix4x4.TRS(new Vector3(x,y+.05f-size*6,z),Quaternion.Euler(0,Random.Range(0,360),0),new Vector3(size*Random.Range(.9f,1.5f),size*Random.Range(.7f,1f),size*Random.Range(.9f,1.5f)))});shrubs++;
        }
        for(int k=0;k<5;k++){if(batches[k].Count==0)continue;var mesh=new Mesh{name="Meadow shrubs "+k,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(batches[k].ToArray(),true,true);string p=D+"/Meshes/MeadowShrubs"+k+".asset";var ex=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(ex){ex.Clear();EditorUtility.CopySerialized(mesh,ex);Object.DestroyImmediate(mesh);mesh=ex;}else AssetDatabase.CreateAsset(mesh,p);var o=new GameObject("Shrubs • "+names[k]);o.transform.SetParent(g,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(D+"/"+names[k]+".mat");}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "tufts "+tufts+" reeds "+reedClumps+" shrubs "+shrubs+" chunks "+chunks.Count;
    }
}
