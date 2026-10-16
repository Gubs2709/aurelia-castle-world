using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Huge crenellated walls: a crown wall along the cliff rim, a ring wall around the palace, towers, and three extra gatehouses.
public static class V5Walls
{
    static Transform grp;
    static GameObject Wall(string model,Vector3 pos,Vector3 facing,float yScale=1,float xzScale=1)
    {
        var g=V4City.Place(model,pos,facing,grp,1,true);if(g)g.transform.localScale=new Vector3(xzScale,yScale,xzScale);return g;
    }
    static float RimZ(float x){float lo=60,hi=150;for(int i=0;i<24;i++){float m=(lo+hi)/2;if(V3.H(x,m)<62)lo=m;else hi=m;}return (lo+hi)/2;}
    static void GateColliders(GameObject gate,float scale)
    {
        foreach(var c in gate.GetComponents<Collider>())Object.DestroyImmediate(c);
        var rs=gate.GetComponentsInChildren<MeshRenderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);
        var tr=gate.transform;Vector3 across=tr.right;float W=Mathf.Abs(Vector3.Dot(b.size,across)),H=b.size.y;float depth=Mathf.Abs(Vector3.Dot(b.size,tr.forward));
        float pass=6.8f*scale;float tw=(W-pass)/2;
        foreach(int s in new[]{-1,1}){var go=new GameObject("Gate tower collider");go.transform.SetParent(tr,false);go.transform.position=b.center+across*s*(pass/2+tw/2);go.transform.rotation=tr.rotation;var bc=go.AddComponent<BoxCollider>();bc.size=new Vector3(tw/tr.lossyScale.x,H/tr.lossyScale.y,depth/tr.lossyScale.z);}
        var lt=new GameObject("Gate lintel collider");lt.transform.SetParent(tr,false);lt.transform.position=new Vector3(b.center.x,b.min.y+H*.72f,b.center.z);lt.transform.rotation=tr.rotation;var lc=lt.AddComponent<BoxCollider>();lc.size=new Vector3(pass/tr.lossyScale.x,H*.56f/tr.lossyScale.y,depth/tr.lossyScale.z);
    }
    public static string Build()
    {
        foreach(var n in new[]{"34 • Crown walls, ring walls and gates"})foreach(var old in Resources.FindObjectsOfTypeAll<GameObject>().Where(go=>go.name==n&&go.scene.IsValid()).ToArray())if(old)Object.DestroyImmediate(old);
        grp=V3.FreshGroup("34 • Crown walls, ring walls and gates");Random.InitState(808);int walls=0,towers=0;
        // 1. crown wall on the cliff rim
        int idx=0;
        for(float x=-246;x<=246;x+=12f)
        {
            if(Mathf.Abs(x)<17)continue;float z=RimZ(x)+2.4f;float y=Mathf.Max(V3.H(x,z+4),V3.H(x,z-1))-2.5f;
            Wall("Wall_Section",new Vector3(x,y,z),new Vector3(0,0,-1));walls++;
            if(idx++%4==2){Wall("Wall_Tower",new Vector3(x,y-1f,z+1.5f),Vector3.back);towers++;}
        }
        // 2. palace ring wall
        float R=V3Layout.Ring;float z0=100,z1=240;
        foreach(var c in new[]{new Vector2(-R,z0),new Vector2(R,z0),new Vector2(-R,z1),new Vector2(R,z1)}){Wall("Wall_Tower",new Vector3(c.x,V3.H(c.x,c.y)-1.5f,c.y),Vector3.forward,1.35f,1.35f);towers++;}
        void Side(Vector3 a,Vector3 b,Vector3 outward,float gateAt)
        {
            float len=Vector3.Distance(a,b);int n=Mathf.RoundToInt(len/12f);var dir=(b-a).normalized;float step=len/n;
            for(int i=0;i<n;i++)
            {
                float t=(i+.5f)*step;if(Mathf.Abs(t-gateAt)<15)continue;var p=a+dir*t;
                var go=Wall("Wall_Section",new Vector3(p.x,V3.H(p.x,p.z)-1.5f,p.z),outward);walls++;
                if(go)go.transform.localScale=new Vector3(1,1,1);
                if(i%3==1&&Mathf.Abs(t-gateAt)>30){Wall("Wall_Tower",new Vector3(p.x,V3.H(p.x,p.z)-1f,p.z),outward);towers++;}
            }
        }
        Side(new Vector3(-R,0,z0),new Vector3(-R,0,z1),Vector3.left,66);
        Side(new Vector3(R,0,z0),new Vector3(R,0,z1),Vector3.right,66);
        Side(new Vector3(-R,0,z1),new Vector3(R,0,z1),Vector3.forward,R);
        // 3. gatehouses on west, east and north
        float gs=1.6f;
        var gw=V4City.Place("Prop_Gate",new Vector3(-R,V3.H(-R,166)-.3f,166),Vector3.left,grp,gs,false);var ge=V4City.Place("Prop_Gate",new Vector3(R,V3.H(R,166)-.3f,166),Vector3.right,grp,gs,false);var gn=V4City.Place("Prop_Gate",new Vector3(0,V3.H(0,z1)-.3f,z1),Vector3.forward,grp,gs,false);
        foreach(var gt in new[]{gw,ge,gn})if(gt){gt.transform.localScale=Vector3.one*gs;GateColliders(gt,gs);}
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "walls "+walls+" towers "+towers;
    }
}
