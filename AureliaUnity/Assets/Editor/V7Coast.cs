using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;

// A physically lower ocean port at the foot of the eastern castle cliffs.
// The previous elevated inland basin is retained as a separate lake.
public static class V7Coast
{
    public const float Sea=1.6f,Quay=3.0f;
    public static float Height(float x,float z,float old)
    {
        float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(270,318,x));
        float north=Mathf.SmoothStep(0,1,Mathf.InverseLerp(70,115,z));
        float south=Mathf.SmoothStep(0,1,Mathf.InverseLerp(390,335,z));
        float shelf=2.65f+Mathf.PerlinNoise(x*.04f,z*.04f)*.3f;
        float shoreline=348+Mathf.Sin(z*.037f)*5;
        float depth=Mathf.SmoothStep(0,1,Mathf.InverseLerp(shoreline-4,shoreline+20,x));
        float coast=Mathf.Lerp(shelf,-24,depth);
        return Mathf.Lerp(old,coast,blend*north*south);
    }
    static void Box(V3.MB b,Vector3 c,Vector3 s,Quaternion q)
    {
        Vector3 P(float x,float y,float z)=>c+q*Vector3.Scale(new Vector3(x,y,z),s)*.5f;
        Vector2 a=Vector2.zero,d=Vector2.one;
        void F(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3)=>b.Quad(0,p0,p1,p2,p3,a,Vector2.right,d,Vector2.up,Color.white);
        F(P(-1,1,-1),P(-1,1,1),P(1,1,1),P(1,1,-1));F(P(-1,-1,1),P(-1,-1,-1),P(1,-1,-1),P(1,-1,1));
        F(P(-1,-1,-1),P(-1,-1,1),P(-1,1,1),P(-1,1,-1));F(P(1,-1,1),P(1,-1,-1),P(1,1,-1),P(1,1,1));
        F(P(1,-1,-1),P(-1,-1,-1),P(-1,1,-1),P(1,1,-1));F(P(-1,-1,1),P(1,-1,1),P(1,1,1),P(-1,1,1));
    }
    static void Ramp(Transform g,Vector3 a,Vector3 b,Material stone,int index)
    {
        var delta=b-a;var flat=new Vector3(delta.x,0,delta.z).normalized;var right=Vector3.Cross(Vector3.up,flat);int count=Mathf.CeilToInt(Mathf.Abs(delta.y)/.20f);float len=new Vector2(delta.x,delta.z).magnitude;
        var mb=new V3.MB();Quaternion yaw=Quaternion.LookRotation(flat);
        for(int i=0;i<count;i++){float t=(i+.5f)/count;var c=Vector3.Lerp(a,b,t);Box(mb,c-Vector3.up*.25f,new Vector3(4.8f,.5f,len/count+.08f),yaw);}
        var o=V3.Obj("Stone harbour stair flight "+index,V3.SaveMesh(mb.Build("Harbour stairs"),"V7Stairs"+index),new[]{stone},g);
        // Smooth walkable ramp collider under visible steps, max slope ~30deg.
        var col=new GameObject("Continuous stair collision "+index);col.transform.SetParent(g,false);var m=new V3.MB();
        m.Quad(0,a-right*2.4f,b-right*2.4f,b+right*2.4f,a+right*2.4f,Vector2.zero,Vector2.right,Vector2.one,Vector2.up,Color.white,Vector3.up);
        var mesh=V3.SaveMesh(m.Build("Stair ramp"),"V7StairCollider"+index);var mc=col.AddComponent<MeshCollider>();mc.sharedMesh=mesh;
        var rails=new V3.MB();
        for(int side=-1;side<=1;side+=2)
        {
            for(int i=0;i<=12;i++){var p=Vector3.Lerp(a,b,i/12f)+right*side*2.25f;Box(rails,p+Vector3.up*.65f,new Vector3(.14f,1.3f,.14f),Quaternion.identity);}
            var p0=a+right*side*2.25f+Vector3.up*1.2f;var p1=b+right*side*2.25f+Vector3.up*1.2f;
            Box(rails,(p0+p1)*.5f,new Vector3(.15f,.15f,Vector3.Distance(p0,p1)),Quaternion.LookRotation((p1-p0).normalized));
        }
        V3.Obj("Carved stair balustrade "+index,V3.SaveMesh(rails.Build("Stair balustrade"),"V7StairRails"+index),new[]{stone},g);
    }
    public static string Build()
    {
        var g=V3.FreshGroup("35 • Open eastern ocean and seaward harbour");
        var sea=V3.Mat("V7 open ocean","Aurelia/Anime Ocean");sea.SetColor("_Deep",V3.C("#135c79"));sea.SetColor("_Shallow",V3.C("#38b8ad"));sea.SetColor("_Crest",V3.C("#b9f1ed"));
        // Dense mesh near shore; the far sea reaches a real distant horizon.
        float[] xs={300,315,325,335,345,355,365,380,400,430,470,530,650,850,1200,2000,4000,8000,14000};
        float[] zs={-1400,-500,0,70,100,130,160,190,220,250,280,320,380,480,700,1200,2500,5000,9000,14000};
        var mb=new V3.MB();for(int z=0;z<zs.Length;z++)for(int x=0;x<xs.Length;x++)mb.Vert(new Vector3(xs[x],Sea,zs[z]),Vector3.up,new Vector2(xs[x]*.01f,zs[z]*.01f),Color.white);
        for(int z=0;z<zs.Length-1;z++)for(int x=0;x<xs.Length-1;x++){int i=z*xs.Length+x;mb.Tri(0,i,i+xs.Length,i+1);mb.Tri(0,i+1,i+xs.Length,i+xs.Length+1);}
        V3.Obj("Vast animated eastern ocean",V3.SaveMesh(mb.Build("Eastern ocean"),"V7Ocean"),new[]{sea},g);
        var stone=V3.Lit("V7 salt weathered quay","wall_stone","wall_stone_n",.09f,new Vector2(2,2),"#d8d3bf");
        var quay=new V3.MB();Box(quay,new Vector3(333,1.9f,194),new Vector3(18,2.2f,116),Quaternion.identity);
        var qo=V3.Obj("Walkable ocean quay",V3.SaveMesh(quay.Build("Quay"),"V7Quay"),new[]{stone},g);qo.AddComponent<MeshCollider>().sharedMesh=qo.GetComponent<MeshFilter>().sharedMesh;
        Ramp(g,new Vector3(254,V3.H(254,146)+.15f,146),new Vector3(298,34.1f,195),stone,0);Ramp(g,new Vector3(298,34.1f,195),new Vector3(327,3.05f,145),stone,1);
        var landing=new V3.MB();Box(landing,new Vector3(298,33.7f,195),new Vector3(8,.8f,8),Quaternion.identity);var lo=V3.Obj("Stair turnaround landing",V3.SaveMesh(landing.Build("Landing"),"V7Landing"),new[]{stone},g);lo.AddComponent<MeshCollider>().sharedMesh=lo.GetComponent<MeshFilter>().sharedMesh;
        int boats=0;
        for(int i=0;i<3;i++)
        {
            float z=155+i*37;var pos=new Vector3(373,2.05f,z);var dock=V4City.Place("Dock_8",pos,Vector3.right,g,1,false);
            if(dock){dock.name="Seaward pier "+i;dock.transform.localScale=new Vector3(2,1,9);var col=dock.AddComponent<BoxCollider>();col.center=new Vector3(0,1,0);col.size=new Vector3(3.2f,.3f,8);}
            for(int j=0;j<4;j++)
            {
                var bp=new Vector3(354+j*13,Sea-.12f,z+(j%2==0?-1:1)*7.5f);var bo=V4City.Place(j==3?"Boat_Sail":j%2==0?"Boat_FishBlue":"Boat_FishRed",bp,Vector3.right,g,1,false);
                if(bo){bo.name="Ocean fishing vessel "+boats++;bo.AddComponent<BoatBob>().amp=.16f;}
            }
        }
        for(int i=0;i<5;i++)
        {
            var bo=V4City.Place(i%2==0?"Boat_Sail":"Boat_SailRed",new Vector3(475+i*48,Sea-.1f,128+i*35),new Vector3(.4f,0,.8f),g,1.5f,false);
            if(bo){bo.name="Offshore sailing boat "+i;var bob=bo.AddComponent<BoatBob>();bob.amp=.22f;bob.drift=1.4f;boats++;}
        }
        V4City.Place("Prop_Lighthouse",new Vector3(335,Quay,251),Vector3.right,g,1.4f,true);
        string[] stalls={"Prop_FishStallBlue","Prop_FishStallRed","Prop_NetRack","Prop_CrateFish","Prop_Barrel","Prop_Crates","Prop_Lamp"};
        for(int i=0;i<19;i++)V4City.Place(stalls[i%stalls.Length],new Vector3(327,Quay,144+i*5.2f),Vector3.right,g,1, i%7<2);
        for(int i=0;i<6;i++){float z=172+i*12.2f;V4City.Place("Harbor_"+(char)('A'+i),new Vector3(316,V3.H(316,z),z),Vector3.right,g,.9f,true);}
        for(int i=0;i<8;i++)V4City.Place("Prop_Lamp",new Vector3(339,Quay,145+i*14),Vector3.right,g,1,false);
        var gull=new GameObject("Ocean gull flock").AddComponent<GullFlock>();gull.transform.SetParent(g,false);gull.material=V3.Foliage("Gull","deco_gull");gull.count=24;gull.centre=new Vector3(395,Sea,195);gull.height=22;gull.radius=90;
        // Remove only the mountain range directly blocking the open ocean view.
        var east=V3.Root.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Snow brushed mountain range 1");if(east)east.gameObject.SetActive(false);
        var source=V3.Root.GetComponentsInChildren<CloudDrift>().FirstOrDefault(t=>t.name.StartsWith("Slow towering"));
        if(source)for(int i=0;i<5;i++)
        {
            var cloud=UnityEngine.Object.Instantiate(source.gameObject,g);cloud.name="Ocean horizon drifting cumulus "+i;cloud.transform.position=new Vector3(1300+i*250,300+i%2*130,-700+i*650);cloud.transform.rotation=Quaternion.Euler(0,90,0);cloud.transform.localScale=new Vector3(480+i*40,620+i%2*120,1);var d=cloud.GetComponent<CloudDrift>();d.speed=1.5f+i*.2f;d.range=2400;
        }
        Camera.main.farClipPlane=16000;
        var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();c.harbourPoint=new Vector3(333,4,192);c.autumnPoint=new Vector3(-106,V3.H(-106,-190)+.2f,-190);
        EditorUtility.SetDirty(c);EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());return "Open ocean: 14km horizon, three long piers, walkable stair descent, "+boats+" boats";
    }
}
