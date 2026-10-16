using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Accessible stone ramp from the village up to the viaduct deck, plus an arched causeway from the overlook to the deck start.
public static class V3Bridge
{
    const int TOP=0,STONE=1,TRIM=2;
    static void Q(V3.MB m,int sub,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 outward,Vector2 ua,Vector2 ub,Vector2 uc,Vector2 ud)
    {
        // wind so the visible side faces `outward`
        if(Vector3.Dot(Vector3.Cross(b-a,d-a),outward)<0){m.Quad(sub,a,d,c,b,ua,ud,uc,ub,Color.white,outward.normalized);}
        else m.Quad(sub,a,b,c,d,ua,ub,uc,ud,Color.white,outward.normalized);
    }
    static Vector2 UVxz(Vector3 p,float t=2.4f)=>new Vector2((p.x+p.z)/t,p.y/t);
    static void WallQuad(V3.MB m,int sub,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 outward)=>Q(m,sub,a,b,c,d,outward,UVxz(a),UVxz(b),UVxz(c),UVxz(d));
    static float DeckTopAt(float x,float z)
    {
        Physics.SyncTransforms();if(Physics.Raycast(new Vector3(x,40,z),Vector3.down,out var h,60,~0,QueryTriggerInteraction.Ignore))return h.point.y;return 12.5f;
    }
    // Sloped stone ramp with parapets from A (ground) to B (deck edge)
    static Mesh Ramp(Vector3 A,Vector3 B,float half)
    {
        var m=new V3.MB(3);int n=44;Vector3 dir=new Vector3(B.x-A.x,0,B.z-A.z).normalized,side=Vector3.Cross(Vector3.up,dir).normalized;
        float total=Vector3.Distance(new Vector3(A.x,0,A.z),new Vector3(B.x,0,B.z));
        Vector3 P(float t,float lat,float up=0){var c=Vector3.Lerp(A,B,t);return c+side*lat+Vector3.up*up;}
        for(int i=0;i<n;i++)
        {
            float t0=i/(float)n,t1=(i+1)/(float)n;
            Vector3 l0=P(t0,-half),r0=P(t0,half),l1=P(t1,-half),r1=P(t1,half);
            // top surface (flagstone, world UV)
            Q(m,TOP,l0,r0,r1,l1,Vector3.up,new Vector2(l0.x/3,l0.z/3),new Vector2(r0.x/3,r0.z/3),new Vector2(r1.x/3,r1.z/3),new Vector2(l1.x/3,l1.z/3));
            // outer walls to the ground
            foreach(int s in new[]{-1,1})
            {
                Vector3 a0=s<0?l0:r0,a1=s<0?l1:r1;float g0=Mathf.Min(V3.H(a0.x,a0.z),a0.y)-3.5f,g1=Mathf.Min(V3.H(a1.x,a1.z),a1.y)-3.5f;
                WallQuad(m,STONE,new Vector3(a0.x,g0,a0.z),new Vector3(a1.x,g1,a1.z),a1,a0,side*s);
                // parapet: inner face, top, outer face
                float pw=.5f,ph=1.0f;Vector3 inner0=a0-side*s*pw,inner1=a1-side*s*pw;
                WallQuad(m,TRIM,inner0,inner1,inner1+Vector3.up*ph,inner0+Vector3.up*ph,-side*s);
                WallQuad(m,TRIM,a0,a1,a1+Vector3.up*ph,a0+Vector3.up*ph,side*s);
                Q(m,TRIM,inner0+Vector3.up*ph,inner1+Vector3.up*ph,a1+Vector3.up*ph,a0+Vector3.up*ph,Vector3.up,Vector2.zero,Vector2.right*.5f,new Vector2(.5f,.5f),Vector2.up*.5f);
            }
        }
        // end cap at the ground end (steps up from road)
        return m.Build("Viaduct ramp");
    }
    // Arched causeway between two points (horizontal), deck top at topY
    static Mesh ArchRun(Vector3 a,Vector3 b,float width,int spans,float topY,float baseY)
    {
        var m=new V3.MB(3);Vector3 d=new Vector3(b.x-a.x,0,b.z-a.z);float L=d.magnitude;d.Normalize();Vector3 n=Vector3.Cross(Vector3.up,d).normalized;float pw=1.9f,clear=(L-(spans+1)*pw)/spans,R=clear/2,spring=3.6f;
        Vector3 Pt(float u,float lat,float y)=>new Vector3(a.x,0,a.z)+d*u+n*lat+Vector3.up*y;
        var cols=new List<(float u,float y)>();
        for(int k=0;k<spans;k++)
        {
            float uc=pw+clear/2+k*(pw+clear);
            cols.Add((uc-R-pw,baseY));cols.Add((uc-R,baseY));cols.Add((uc-R,spring));
            for(int j=1;j<=10;j++){float th=Mathf.PI*j/10;cols.Add((uc-R*Mathf.Cos(th),spring+R*Mathf.Sin(th)*(j==10?0:1)));}
            // arch ends at uc+R spring -> pier base
            cols.Add((uc+R,baseY));
        }
        cols.Add((L,baseY));cols.Sort((p,q)=>p.u!=q.u?p.u.CompareTo(q.u):0);
        // The sort above keeps insertion order for equal u (List.Sort is unstable) so rebuild explicitly in order instead.
        cols.Clear();cols.Add((0,baseY));
        for(int k=0;k<spans;k++)
        {
            float uc=pw+clear/2+k*(pw+clear);cols.Add((uc-R,baseY));cols.Add((uc-R,spring));
            for(int j=1;j<=12;j++){float th=Mathf.PI*j/12;cols.Add((uc-R*Mathf.Cos(th),spring+R*Mathf.Sin(th)));}
            cols.Add((uc+R,baseY));
        }
        cols.Add((L,baseY));
        float wallTop=topY-.05f;
        foreach(int s in new[]{-1,1})
        {
            float lat=s*width/2;
            for(int i=0;i<cols.Count-1;i++)
            {
                var c0=cols[i];var c1=cols[i+1];if(Mathf.Approximately(c0.u,c1.u))continue;
                // inside an arch opening (consecutive columns on the curve) the wall starts at the curve, elsewhere at base
                WallQuad(m,STONE,Pt(c0.u,lat,c0.y),Pt(c1.u,lat,c1.y),Pt(c1.u,lat,wallTop),Pt(c0.u,lat,wallTop),n*s);
            }
            // parapet
            float pwid=.55f,ph=1.05f;
            WallQuad(m,TRIM,Pt(0,lat-s*pwid,wallTop),Pt(L,lat-s*pwid,wallTop),Pt(L,lat-s*pwid,wallTop+ph),Pt(0,lat-s*pwid,wallTop+ph),-n*s);
            WallQuad(m,TRIM,Pt(0,lat,wallTop),Pt(L,lat,wallTop),Pt(L,lat,wallTop+ph),Pt(0,lat,wallTop+ph),n*s);
            Q(m,TRIM,Pt(0,lat-s*pwid,wallTop+ph),Pt(L,lat-s*pwid,wallTop+ph),Pt(L,lat,wallTop+ph),Pt(0,lat,wallTop+ph),Vector3.up,Vector2.zero,new Vector2(L/2,0),new Vector2(L/2,.3f),new Vector2(0,.3f));
        }
        // intrados (underside of the arch) and pier faces inside openings
        for(int k=0;k<spans;k++)
        {
            float uc=pw+clear/2+k*(pw+clear);
            for(int j=0;j<12;j++)
            {
                float t0=Mathf.PI*j/12,t1=Mathf.PI*(j+1)/12;Vector3 p0=Pt(uc-R*Mathf.Cos(t0),-width/2,spring+R*Mathf.Sin(t0)),p1=Pt(uc-R*Mathf.Cos(t1),-width/2,spring+R*Mathf.Sin(t1));
                Vector3 q0=p0+n*width,q1=p1+n*width;Vector3 mid=Pt(uc,0,spring);Vector3 outwardDir=((p0+p1+q0+q1)/4-mid).normalized*-1;Q(m,STONE,p0,p1,q1,q0,-((p0+p1+q0+q1)/4-Pt(uc,0,spring)),new Vector2(0,0),new Vector2(.5f,0),new Vector2(.5f,width/2.4f),new Vector2(0,width/2.4f));
            }
            foreach(int e in new[]{-1,1})
            {
                float u=uc+e*R;Vector3 a0=Pt(u,-width/2,baseY),a1=Pt(u,width/2,baseY),a2=Pt(u,width/2,spring),a3=Pt(u,-width/2,spring);
                Q(m,STONE,a0,a1,a2,a3,-d*e,new Vector2(0,0),new Vector2(width/2.4f,0),new Vector2(width/2.4f,3),new Vector2(0,3));
            }
        }
        // deck top (flagstone)
        Q(m,TOP,Pt(0,-width/2,topY),Pt(L,-width/2,topY),Pt(L,width/2,topY),Pt(0,width/2,topY),Vector3.up,new Vector2(a.x/3,a.z/3),new Vector2(b.x/3,b.z/3),new Vector2(b.x/3,b.z/3+width/3),new Vector2(a.x/3,a.z/3+width/3));
        return m.Build("Causeway arches");
    }
    public static string Build()
    {
        var g=V3.FreshGroup("30 • Village ramp and overlook causeway");
        var top=V3.Lit("Bridge flagstone","path_flagstone","path_flagstone_n",.08f,null,"#e8e2d2");var wall=V3.Lit("Bridge stone","wall_stone","wall_stone_n",.05f,null,"#efe8d8");var trim=V3.Lit("Bridge trim","wall_stone","wall_stone_n",.05f,null,"#f6f0e2");
        var mats=new[]{top,wall,trim};
        // ramp: ground end near the village street, deck edge at the viaduct
        float deckY=DeckTopAt(7.1f,20f);Vector3 A=new Vector3(-40,V3.H(-40,17)+.1f,17),B=new Vector3(1.0f,deckY,19.6f);
        var ramp=V3.SaveMesh(Ramp(A,B,3.5f),"Viaduct_ramp");var go=V3.Obj("Stone ramp to the viaduct",ramp,mats,g);go.AddComponent<MeshCollider>().sharedMesh=ramp;
        // causeway from the overlook to the deck start
        float deckY2=DeckTopAt(50.0f,-244.5f);Vector3 s0=new Vector3(21,0,-253.2f),s1=new Vector3(48.4f,0,-246.9f);
        var arch=V3.SaveMesh(ArchRun(s0,s1,7.2f,3,deckY2,-3.6f),"Overlook_causeway");var g2=V3.Obj("Overlook causeway with arches",arch,mats,g);g2.AddComponent<MeshCollider>().sharedMesh=arch;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "ramp deckY="+deckY+" causewayY="+deckY2;
    }
}
