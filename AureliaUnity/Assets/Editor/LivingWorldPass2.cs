using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;

public static class LivingWorldPass2
{
    const string Marker="‡";
    // Idempotent: each tree is processed once (marked by a name suffix) and scaled toward an absolute target height.
    public static string Retall()
    {
        int n=0;var root=GameObject.Find("17 • Towering mixed forests and blossom groves").transform;
        foreach(Transform t in root)
        {
            if(t.name.EndsWith(Marker)||!t.GetComponent<CapsuleCollider>())continue;
            float h=t.localScale.y*10,target=h;
            if(t.name.StartsWith("Ancient"))target=h*1.25f;
            else if(t.name.StartsWith("Forest cathedral"))target=Mathf.Clamp(h*1.55f,48,72);
            else if(t.name.StartsWith("Blossom")||t.name.StartsWith("Crimson"))target=Mathf.Clamp(h*1.5f,38,58);
            else target=Mathf.Clamp(h*1.9f,26,48);
            float ky=target/h,kxz=Mathf.Lerp(1,ky,.4f);
            var s=t.localScale;t.localScale=new Vector3(s.x*kxz,s.y*ky,s.z*kxz);t.name+=Marker;n++;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "retalled "+n;
    }
    public static string SetSpawn(float x,float z)
    {
        var c=Object.FindAnyObjectByType<CastleReferenceView>();float y=ReferenceCastleBuilder.Height(x,z)+.15f;c.spawn=new Vector3(x,y,z);c.lookTarget=new Vector3(0,90,100);EditorUtility.SetDirty(c);
        return c.spawn.ToString();
    }
    public static string Step1()
    {
        var a=Retall();var b=SetSpawn(-20f,-262f);var c=Object.FindAnyObjectByType<CastleReferenceView>();c.WalkAt(c.spawn);return a+" | "+b;
    }
    static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
    public static string Tone()
    {
        void Set(string path,string hex){var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m)return;m.SetColor("_BaseColor",C(hex));EditorUtility.SetDirty(m);}
        Set("Assets/ReferenceCastle/Cool ivory limestone.mat","#ffffff");
        Set("Assets/ReferenceCastle/Sunlit carved limestone.mat","#fffbf0");
        Set("Assets/ReferenceCastle/Sea glass turquoise slates.mat","#7fe0dc");
        RenderSettings.fogColor=C("#b6d8e6");
        return "toned";
    }
    // Per-house wall and roof colours baked into vertex colours of the combined hamlet meshes (nearest house wins).
    public static string TintHouses()
    {
        string[] walls={"#f2e6c9","#efc9b6","#c3dbe8","#d3e5c8","#f2de8d","#f8f5ef","#e6bb78","#e9d3c4"};
        string[] roofs={"#c4663f","#9b5233","#5e7a93","#608c5b","#bfa05a","#4d9c9b","#7b5a72","#a35a3a"};
        var group=GameObject.Find("13 � Inhabited timber and stone hamlets").transform;
        var houses=group.Cast<Transform>().Where(t=>t.name.StartsWith("Inhabited valley cottage")).ToArray();int tinted=0;
        foreach(var r in group.GetComponentsInChildren<MeshRenderer>(true))
        {
            string mn=r.sharedMaterial?r.sharedMaterial.name:"";bool isWall=mn=="Weathered lime plaster",isRoof=mn=="Natural blue grey slate";if(!(isWall||isRoof)||!r.transform.parent.name.Contains("batches"))continue;
            var mf=r.GetComponent<MeshFilter>();var mesh=mf.sharedMesh;var v=mesh.vertices;var cols=new Color[v.Length];
            for(int i=0;i<v.Length;i++)
            {
                var w=r.transform.TransformPoint(v[i]);int best=-1;float bd=14f;for(int h=0;h<houses.Length;h++){var p=houses[h].position;float d=Mathf.Sqrt((p.x-w.x)*(p.x-w.x)+(p.z-w.z)*(p.z-w.z));if(d<bd){bd=d;best=h;}}
                if(best<0){cols[i]=Color.white;continue;}
                int pick=isWall?(best*5+3)%walls.Length:(best*3+1)%roofs.Length;var c=C(isWall?walls[pick]:roofs[pick]);c.a=isWall?.4f:.3f;cols[i]=c;tinted++;
            }
            mesh.colors=cols;EditorUtility.SetDirty(mesh);r.sharedMaterial.SetFloat("_VertexTint",1);EditorUtility.SetDirty(r.sharedMaterial);
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "tinted vertices "+tinted;
    }
    public static string Points()
    {
        var c=Object.FindAnyObjectByType<CastleReferenceView>();
        c.waterfallPoint=new Vector3(-40,ReferenceCastleBuilder.Height(-40,10)+.15f,10);
        c.cottagePoint=new Vector3(-100,ReferenceCastleBuilder.Height(-100,-100)+.15f,-100);
        EditorUtility.SetDirty(c);EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return c.waterfallPoint+" "+c.cottagePoint;
    }
}
