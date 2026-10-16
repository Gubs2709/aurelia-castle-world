using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;

public static class V7Run
{
    const string Out="D:/GAME/artifacts/world-v7";
    static Queue<Func<string>> jobs;
    public static string Start()
    {
        if(EditorApplication.isPlaying)throw new Exception("Stop play mode first");
        if(jobs!=null)throw new Exception("World pass already running");
        Directory.CreateDirectory(Out);File.WriteAllText(Out+"/progress.txt","Started "+DateTime.Now+"\n");
        V3Layout.Reset();jobs=new Queue<Func<string>>(new Func<string>[]{
            ()=>"terrain "+V3Run.Terrain(),
            ()=>V7Forest.Build(),
            ()=>"ground "+V3Run.Ground(),
            ()=>"city "+V3Run.City(),
            ()=>"lake "+V3Run.Harbor(),
            ()=>"walls "+V3Run.Walls(),
            ()=>"portal "+V3Run.Portal(),
            ()=>V7Coast.Build(),
            ()=>"save "+V3Run.Save(),
            ()=>Validate(),
            ()=>Capture()
        });EditorApplication.update+=Step;return "World pass queued; see artifacts/world-v7/progress.txt";
    }
    static void Step()
    {
        if(jobs==null)return;
        try{if(jobs.Count==0){EditorApplication.update-=Step;jobs=null;File.AppendAllText(Out+"/progress.txt","COMPLETE\n");return;}var result=jobs.Dequeue()();File.AppendAllText(Out+"/progress.txt",result+"\n");}
        catch(Exception ex){EditorApplication.update-=Step;jobs=null;File.AppendAllText(Out+"/progress.txt","FAILED "+ex+"\n");Debug.LogException(ex);}
    }
    public static string Validate()
    {
        Directory.CreateDirectory(Out);var lines=new List<string>();
        void Check(bool pass,string s)=>lines.Add((pass?"PASS ":"FAIL ")+s);
        var roots=GameObject.Find("17 • Towering mixed forests and blossom groves").GetComponentsInChildren<LODGroup>();var heights=roots.Select(r=>r.GetLODs()[0].renderers[0].bounds.size.y).OrderBy(v=>v).ToArray();
        Check(heights.Length>300,"trees="+heights.Length);Check(heights.Length>0&&heights[heights.Length/2]>35,"tree median height="+(heights.Length>0?heights[heights.Length/2]:0));Check(roots.All(t=>t.lodCount==2),"two mesh detail levels on all trees");
        var meshes=roots.Select(t=>t.GetLODs()[0].renderers[0].GetComponent<MeshFilter>().sharedMesh).Distinct().ToArray();Check(meshes.Length>=20,"distinct branching and root templates="+meshes.Length);
        Check(GameObject.Find("Vast animated eastern ocean"),"open ocean exists");Check(GameObject.Find("Upper gate portal"),"upper castle gate portal placed");
        Check(V3.Root.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Seaward pier"))==3,"three ocean piers");
        Check(V3.Root.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Ocean fishing vessel"))>=10,"moored fishing vessels");
        var missing=V3.Root.GetComponentsInChildren<Renderer>(true).Count(r=>r.sharedMaterials.Any(m=>!m||!m.shader||m.shader.name=="Hidden/InternalErrorShader"));Check(missing==0,"missing/error materials="+missing);
        int scripts=V3.Root.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));Check(scripts==0,"missing scripts="+scripts);
        Physics.SyncTransforms();foreach(var p in new[]{new Vector3(333,12,192),new Vector3(373,12,155),new Vector3(298,44,195)})Check(Physics.Raycast(p,Vector3.down,out var h,25),"port walkable collision "+p);
        foreach(var p in new[]{Vector3.Lerp(new Vector3(254,V3.H(254,146)+.15f,146),new Vector3(298,34.1f,195),.5f),Vector3.Lerp(new Vector3(298,34.1f,195),new Vector3(327,3.05f,145),.5f)})Check(Physics.Raycast(p+Vector3.up*2,Vector3.down,out var h,2.5f),"stair flight upward facing collision "+p);
        File.WriteAllLines(Out+"/editor-validation.txt",lines);return string.Join("; ",lines);
    }
    static void Shot(string name,Vector3 pos,Vector3 target)
    {
        var cam=Camera.main;cam.transform.position=pos;cam.transform.LookAt(target);cam.fieldOfView=53;var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Out+"/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
    }
    public static string Capture()
    {
        Directory.CreateDirectory(Out);var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();
        Shot("forest",new Vector3(V3.PathX(-198),V3.H(V3.PathX(-198),-198)+2.2f,-198),new Vector3(V3.PathX(-155),20,-155));
        Shot("ocean-harbour",new Vector3(296,54,104),new Vector3(389,5,211));
        Shot("seaward-quay",new Vector3(330,7,146),new Vector3(414,4,193));
        Shot("castle-and-city",new Vector3(-340,180,-160),new Vector3(0,80,155));
        c.WalkAt(c.spawn);return "Captured forest, ocean harbour, quay and castle city";
    }
}
