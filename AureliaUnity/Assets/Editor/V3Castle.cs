using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class V3Castle
{
    public static string Build()
    {
        var root=V3.Root;var source=GameObject.Find("02 • Nested castle terraces and soaring towers");int removed=0;
        foreach(var t in source.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Wall vertical buttress").ToArray()){Object.DestroyImmediate(t.gameObject);removed++;}
        // real materials
        void SD(string path,string tex,string normal,float scale,string tint,float relief,float smooth=.08f)
        {
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m)return;m.shader=Shader.Find("Aurelia/Surface Detail");m.SetTexture("_BaseMap",V3.T(tex));m.SetColor("_BaseColor",V3.C(tint));m.SetFloat("_Scale",scale);m.SetFloat("_Relief",relief);m.SetFloat("_Smoothness",smooth);m.SetFloat("_TextureStrength",1);m.SetFloat("_Terrain",0);m.SetFloat("_Cull",2);EditorUtility.SetDirty(m);
        }
        SD("Assets/ReferenceCastle/Cool ivory limestone.mat","wall_stone","wall_stone_n",.16f,"#fff8ea",.5f);
        SD("Assets/ReferenceCastle/Sunlit carved limestone.mat","wall_stone","wall_stone_n",.2f,"#fffdf4",.4f);
        SD("Assets/ReferenceCastle/Sea glass turquoise slates.mat","roof_teal","roof_teal_n",.28f,"#ffffff",.45f,.25f);
        // rebatch (without buttresses)
        var batch=GameObject.Find("Castle • efficient material batches").transform;foreach(Transform child in batch.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
        int id=0;foreach(var set in source.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.GetComponent<MeshRenderer>()).GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial))
        {
            var combine=set.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=batch.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray();
            var mesh=new Mesh{name="Castle batch "+set.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combine,true,true);
            var saved=V3.SaveMesh(mesh,"CastleBatch_"+(id++));var g=new GameObject(set.Key.name);g.transform.SetParent(batch,false);g.AddComponent<MeshFilter>().sharedMesh=saved;g.AddComponent<MeshRenderer>().sharedMaterial=set.Key;
            foreach(var f in set)f.GetComponent<MeshRenderer>().enabled=false;
        }
        // escarpment: real rock
        var rock=V3.Mat("Escarpment rock V3","Aurelia/RockTri");rock.SetTexture("_Rock",V3.T("rock_stone"));rock.SetTexture("_RockN",V3.T("rock_stone_n"));rock.SetFloat("_Scale",.14f);rock.SetFloat("_Moss",.85f);rock.SetColor("_Tint",V3.C("#fff3e0"));EditorUtility.SetDirty(rock);
        foreach(var t in Resources.FindObjectsOfTypeAll<MeshRenderer>().Where(r=>r.gameObject.scene.IsValid()&&r.name.StartsWith("Terraced mossy escarpment")))t.sharedMaterial=rock;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "removed "+removed+" batches "+id;
    }
}
