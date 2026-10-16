using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class V3Run
{
    public static string Textures(){return V3.ImportTextures();}
    public static string Ground(){return V3Ground.Build();}
    public static string Trees(){return V3Trees.Build();}
    public static string Flora(){return V3Flora.Build();}
    public static string Rocks(){return V3Flora.Rocks();}
    public static string Save()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();EditorSceneManager.SaveOpenScenes();return "saved";
    }
    public static string Spawn()
    {
        // spawn on the forest-trail road for reference-like ground-level views
        var c=Object.FindAnyObjectByType<Aurelia.CastleReferenceView>();
        return c.spawn.ToString();
    }
    public static string Status()
    {
        var sb=new System.Text.StringBuilder();foreach(Transform t in V3.Root){if(t.name.StartsWith("27")||t.name.StartsWith("28")||t.name.StartsWith("29")||t.name.StartsWith("3"))sb.Append(t.name+"="+t.childCount+"; ");}
        return sb.ToString();
    }
    public static string Village(){return V3Village.Build();}
    public static string Cleanup()
    {
        int n=0;foreach(var t in Resources.FindObjectsOfTypeAll<Transform>()){if(!t.gameObject.scene.IsValid())continue;string nm=t.name;if(nm.StartsWith("Brown horse")||nm.StartsWith("Horse")||nm.Contains("Mounted")||nm.Contains("rider")&&!nm.Contains("Restored")){t.gameObject.SetActive(false);n++;}}
        return "hid "+n;
    }
    public static string Bridge(){return V3Bridge.Build();}
    public static string Castle(){return V3Castle.Build();}
    public static string Brighten()
    {
        void B(string path,float r,float g,float b,float sc){var m=AssetDatabase.LoadAssetAtPath<Material>(path);m.SetColor("_BaseColor",new Color(r,g,b,1));m.SetFloat("_Scale",sc);EditorUtility.SetDirty(m);}
        B("Assets/ReferenceCastle/Cool ivory limestone.mat",1.55f,1.5f,1.4f,.1f);B("Assets/ReferenceCastle/Sunlit carved limestone.mat",1.65f,1.62f,1.52f,.12f);B("Assets/ReferenceCastle/Sea glass turquoise slates.mat",1.25f,1.35f,1.35f,.2f);
        var rk=AssetDatabase.LoadAssetAtPath<Material>("Assets/WorldV3/Escarpment rock V3.mat");rk.SetColor("_Tint",new Color(1.5f,1.42f,1.3f,1));EditorUtility.SetDirty(rk);
        return "ok";
    }
    public static string City(){return V4City.Build();}
    public static string Terrain(){return V4City.TerrainRebuild();}
    public static string Probe(){return V4City.Probe();}
    public static string Knight(){return V4Knight.Install();}
    public static string Look(){return V4Look.Apply();}
    public static string CastleGate(){return V4CastleGate.Build();}
    public static string Finish()
    {
        int n=0;foreach(var t in Resources.FindObjectsOfTypeAll<Transform>()){if(t.gameObject.scene.IsValid()&&t.name.StartsWith("Churning whitewater")){t.gameObject.SetActive(false);n++;}}
        Brighten();
        var rk=AssetDatabase.LoadAssetAtPath<Material>("Assets/WorldV3/Escarpment rock V3.mat");rk.SetColor("_Tint",new Color(1.1f,1.05f,.98f,1));EditorUtility.SetDirty(rk);
        var c=Object.FindAnyObjectByType<Aurelia.CastleReferenceView>();c.WalkAt(c.spawn);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();EditorSceneManager.SaveOpenScenes();
        return "finished, hid foam discs "+n;
    }
    public static string Harbor(){return V5Harbor.Build();}
    public static string Walls(){return V5Walls.Build();}
    public static string Escarp(){return EscarpmentPass.Build();}
    public static string Probe2()
    {
        var sb=new System.Text.StringBuilder();var seen=new System.Collections.Generic.HashSet<string>();
        foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude)){var b=r.bounds;if(Mathf.Abs(b.center.x+180)<24&&Mathf.Abs(b.center.z-166)<24&&b.center.y>60){var path=r.transform.name+" <- "+(r.transform.parent?r.transform.parent.name:"")+" <- "+(r.transform.parent&&r.transform.parent.parent?r.transform.parent.parent.name:"");if(seen.Add(path))sb.Append(path+" size "+b.size+"; ");}}
        return sb.ToString();
    }
    public static string Portal()
    {
        foreach(var old in Resources.FindObjectsOfTypeAll<GameObject>().Where(go=>go.name=="Upper gate portal"&&go.scene.IsValid()).ToArray())Object.DestroyImmediate(old);
        var g=V4City.Place("Gate_Portal",new Vector3(0,34f,105.4f),Vector3.back,V3.Root,1,false);g.name="Upper gate portal";
        // stone facing on the cliff: plain masonry instead of earth
        var stone=V3.Mat("Escarpment masonry","Aurelia/Surface Detail");stone.SetTexture("_BaseMap",V3.T("wall_stone"));stone.SetColor("_BaseColor",new Color(1.5f,1.42f,1.3f));stone.SetFloat("_Scale",.09f);stone.SetFloat("_Relief",.5f);stone.SetFloat("_Smoothness",.06f);stone.SetFloat("_TextureStrength",1);stone.SetFloat("_Terrain",0);stone.SetFloat("_Cull",0);EditorUtility.SetDirty(stone);
        int n=0;foreach(var r in Resources.FindObjectsOfTypeAll<MeshRenderer>())if(r.gameObject.scene.IsValid()&&r.name.StartsWith("Terraced mossy escarpment")){r.sharedMaterial=stone;n++;}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());return "portal placed; stoned "+n;
    }
    public static string SpriteKnightInstall(){return V6SpriteKnight.Install();}
}
