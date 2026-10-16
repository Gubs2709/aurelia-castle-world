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

public static class V8Run
{
    public const string Out="D:/GAME/artifacts/world-v8";
    static Queue<Func<string>> queue;
    public static string Start()
    {
        if(queue!=null)throw new Exception("Already building");if(EditorApplication.isPlaying)throw new Exception("Stop play mode first");Directory.CreateDirectory(Out);File.WriteAllText(Out+"/progress.txt","Started "+DateTime.Now+"\n");
        queue=new Queue<Func<string>>(new Func<string>[]{()=>{V8Kit.Import();return "New reference textures imported";},()=>"Terrain "+V3Run.Terrain(),()=>{V8Grove.Prep();return "Broad green tree assets ready";},()=>V8Harbor.Build(),()=>V8Scenery.Riverside(),()=>V8Scenery.Mountains(),()=>Repair(),()=>V8Scenery.Lighting(),()=>V3Run.Save(),()=>Validate(),()=>Capture()});EditorApplication.update+=Step;return "V8 build queued";
    }
    static void Step()
    {
        try{if(queue.Count==0){EditorApplication.update-=Step;queue=null;File.AppendAllText(Out+"/progress.txt","COMPLETE\n");return;}string result=queue.Dequeue()();File.AppendAllText(Out+"/progress.txt",result+"\n");}
        catch(Exception ex){EditorApplication.update-=Step;queue=null;File.AppendAllText(Out+"/progress.txt","FAILED "+ex+"\n");Debug.LogException(ex);}
    }
    public static string Repair()
    {
        var group=V3.FreshGroup("41 • Repaired house foundations and cottage details");var buildings=GameObject.Find("31 • City of Briarwood").transform.Find("Buildings");var foundation=new V3.MB();int repaired=0;
        foreach(Transform home in buildings)
        {
            if(home.position.x>244&&home.position.z>100){home.gameObject.SetActive(false);continue;}
            // Skirt each house to the real terrain instead of leaving a floating edge.
            var points=new Vector3[4];for(int i=0;i<4;i++)points[i]=home.TransformPoint(new Vector3(i<2?-2.8f:2.8f,0,i%2==0?-2.5f:2.5f));
            int[] order={0,2,3,1};for(int i=0;i<4;i++){var a=points[order[i]];var b=points[order[(i+1)%4]];var ta=a+Vector3.up*.20f;var tb=b+Vector3.up*.20f;a.y=Mathf.Min(a.y,V3.H(a.x,a.z))-.1f;b.y=Mathf.Min(b.y,V3.H(b.x,b.z))-.1f;foundation.Quad(0,a,b,tb,ta,Vector2.zero,Vector2.right,Vector2.one,Vector2.up,Color.white);}repaired++;
        }
        V8Kit.Obj("Grounded village stone plinths",foundation,new[]{V8Kit.Surface("House foundation limestone","#c5c5b1","wall_stone",.42f)},group);
        foreach(var m in Resources.FindObjectsOfTypeAll<Material>())if(m.name.StartsWith("M4 roof red")||m.name.StartsWith("M4 roof terracotta")){m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(V8Kit.Art+"RoofTiles.png"));m.SetColor("_BaseColor",Color.white);m.SetFloat("_Smoothness",.05f);m.DisableKeyword("_NORMALMAP");EditorUtility.SetDirty(m);}
        foreach(var t in V3.Root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Prop_Lighthouse"&&Vector2.Distance(new Vector2(t.position.x,t.position.z),new Vector2(252,148))<4).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        var terrain=GameObject.Find("Sculpted river valley and planted escarpment").GetComponent<MeshRenderer>().sharedMaterial;if(terrain.HasProperty("_Rock")){terrain.SetTexture("_Rock",AssetDatabase.LoadAssetAtPath<Texture2D>(V8Kit.Art+"CliffStone.png"));EditorUtility.SetDirty(terrain);}
        foreach(var tr in V3.Root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Snow brushed mountain range")))tr.gameObject.SetActive(false);
        var forest=GameObject.Find("17 • Towering mixed forests and blossom groves");if(forest)foreach(Transform tr in forest.transform)if(tr.position.x>244&&tr.position.z>95)tr.gameObject.SetActive(false);
        var bark=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(V3.Dir+"/V8 Warm sculpted bark.mat");if(bark){bark.SetColor("_BaseColor",new Color(1.1f,1.12f,1.02f));EditorUtility.SetDirty(bark);}
        var ocean=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(V3.Dir+"/V8 turquoise harbour gradient.mat");ocean.SetColor("_Deep",V3.C("#168fa9"));ocean.SetColor("_Shallow",V3.C("#45d5ce"));EditorUtility.SetDirty(ocean);
        ClearPicnicGrass();
        var retreat=GameObject.Find("38 • Cottage river retreat and old stone bridge");
        if(retreat){var picnic=retreat.transform.Find("V8_Picnic");if(picnic)foreach(var patch in V3.Root.GetComponentsInChildren<Transform>().Where(t=>t!=picnic&&t.parent!=picnic&&t.name.Contains("Grass")&&Vector2.Distance(new Vector2(t.position.x,t.position.z),new Vector2(-48,-190))<4))patch.gameObject.SetActive(false);}
        return "Repaired "+repaired+" house foundations, red tile material and old duplicated cliff lighthouse";
    }
    static void ClearPicnicGrass()
    {
        foreach(var mf in V3.Root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Grass tufts")||f.name=="Fine curved forest grass"||f.name=="Daisies cosmos and cornflowers along river meadow"))
        {
            var mesh=mf.sharedMesh;var vs=mesh.vertices;bool changed=false;var output=new List<int[]>();
            for(int sub=0;sub<mesh.subMeshCount;sub++){var indices=mesh.GetTriangles(sub);var keep=new List<int>();for(int i=0;i<indices.Length;i+=3){var p=mf.transform.TransformPoint((vs[indices[i]]+vs[indices[i+1]]+vs[indices[i+2]])/3);if(Mathf.Abs(p.x+48)<2.8f&&Mathf.Abs(p.z+190)<2.5f){changed=true;continue;}keep.Add(indices[i]);keep.Add(indices[i+1]);keep.Add(indices[i+2]);}output.Add(keep.ToArray());}
            if(changed){var copy=UnityEngine.Object.Instantiate(mesh);for(int i=0;i<output.Count;i++)copy.SetTriangles(output[i],i);mf.sharedMesh=V3.SaveMesh(copy,"V8_picnic_clearing_"+mf.name.Replace(' ','_'));}
        }
    }
    public static string Validate()
    {
        Directory.CreateDirectory(Out);var lines=new List<string>();void Check(bool p,string s)=>lines.Add((p?"PASS ":"FAIL ")+s);
        Check(!V3.Root.GetComponentsInChildren<Transform>().Any(t=>t.name.StartsWith("Stone harbour stair")||t.name.StartsWith("Carved stair balustrade")||t.name=="Stair turnaround landing"),"old harbour stairs and balustrades removed");
        float grade=0;for(int i=0;i<V8Landscape.Route.Count-1;i++){var d=V8Landscape.Route[i+1]-V8Landscape.Route[i];grade=Mathf.Max(grade,Mathf.Atan2(Mathf.Abs(d.y),new Vector2(d.x,d.z).magnitude)*Mathf.Rad2Deg);}Check(grade<26,"continuous path max grade="+grade.ToString("F1")+"deg");
        Physics.SyncTransforms();int hits=0;for(int i=0;i<V8Landscape.Route.Count;i+=3){var p=V8Landscape.Route[i];if(Physics.Raycast(p+Vector3.up*.8f,Vector3.down,out var hit,1.2f))hits++;}int expected=(V8Landscape.Route.Count+2)/3;Check(hits==expected,"hillside path collision "+hits+"/"+expected);
        Check(GameObject.Find("Old arched riverside stone bridge"),"true arched bridge exists");Check(GameObject.Find("The sea guardian • broad rooted green tree"),"tree on the sea cliff exists");Check(V3.Root.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Colourful cliff house"))==V8Landscape.Plots.Count,"cliff house count="+V8Landscape.Plots.Count);
        Check(V3.Root.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Small timber landing stage"))==5,"five small landing stages");Check(UnityEngine.Object.FindObjectsByType<HarbourResidentMotion>().Length>=20,"articulated harbour residents");
        Check(!UnityEngine.Object.FindObjectsByType<Volume>().Any(v=>v.sharedProfile&&v.sharedProfile.TryGet<Bloom>(out var b)&&b.active&&b.intensity.value>0),"no bloom glare");
        int missing=V3.Root.GetComponentsInChildren<Renderer>(true).Count(r=>r.sharedMaterials.Any(m=>!m||!m.shader||m.shader.name=="Hidden/InternalErrorShader"));Check(missing==0,"missing/error materials="+missing);
        foreach(var p in new[]{new Vector3(348,36,349),new Vector3(V3.RiverX(-168),12,-168),new Vector3(V8Landscape.Shore(194),5,194)})Check(Physics.Raycast(p,Vector3.down,out var h,8),"walkable new scene at "+p);
        File.WriteAllLines(Out+"/editor-validation.txt",lines);return string.Join("; ",lines);
    }
    static void Shot(string name,Vector3 pos,Vector3 target,float fov=53)
    {
        var cam=Camera.main;cam.transform.position=pos;cam.transform.LookAt(target);cam.fieldOfView=fov;foreach(var cloud in UnityEngine.Object.FindObjectsByType<CloudDrift>())cloud.FaceCamera(cam);var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Out+"/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
    }
    public static string Capture()
    {
        Directory.CreateDirectory(Out);Shot("bay-reference",new Vector3(403,19,259),new Vector3(311,20,182));Shot("hillside-path",new Vector3(279,45,185),new Vector3(304,25,231),60);
        Shot("riverside-reference",new Vector3(-46,V3.H(-46,-195)+2.5f,-195),new Vector3(-47,5,-148),64);Shot("guardian-reference",new Vector3(316,44,321),new Vector3(356,44,361),56);Shot("mountains-reference",new Vector3(75,150,410),new Vector3(0,200,1050),65);
        var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();c.WalkAt(c.spawn);return "Five actual Unity reference views captured";
    }
}
