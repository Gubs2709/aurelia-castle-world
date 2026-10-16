using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;
using Random=UnityEngine.Random;

// Lake, docks, boats, lighthouse, fish market, reeds and gulls for the harbour district.
public static class V5Harbor
{
    static Vector3 G(float x,float z)=>new Vector3(x,V3.H(x,z),z);
    static Vector2 Ell(float a,float k){return new Vector2(V3Layout.LakeC.x+Mathf.Cos(a)*(V3Layout.LakeR.x*k),V3Layout.LakeC.y+Mathf.Sin(a)*(V3Layout.LakeR.y*k));}
    public static string Build()
    {
        V4Mats.Rebuild();var g=V3.FreshGroup("33 • Harbour lake, docks and boats");Random.InitState(5150);
        var lake=V3.Mat("Harbour lake","Aurelia/Lake");lake.SetColor("_Shallow",V3.C("#5ee6cf"));lake.SetColor("_Deep",V3.C("#1a8fb4"));lake.SetColor("_Sky",V3.C("#8ec8ff"));lake.SetFloat("_Speed",.55f);lake.renderQueue=3000;EditorUtility.SetDirty(lake);
        // lake surface: concentric rings
        var mb=new V3.MB(1);int seg=72;float rx=V3Layout.LakeR.x+7,rz=V3Layout.LakeR.y+7;float cx=V3Layout.LakeC.x,cz=V3Layout.LakeC.y;
        mb.Vert(new Vector3(cx,V3Layout.LakeY,cz),Vector3.up,new Vector2(.5f,.5f),Color.white);
        for(int ring=1;ring<=4;ring++)for(int k=0;k<seg;k++){float a=k*Mathf.PI*2/seg;float f=ring/4f;mb.Vert(new Vector3(cx+Mathf.Cos(a)*rx*f,V3Layout.LakeY,cz+Mathf.Sin(a)*rz*f),Vector3.up,new Vector2(.5f+Mathf.Cos(a)*f*.5f,.5f+Mathf.Sin(a)*f*.5f),Color.white);}
        for(int k=0;k<seg;k++)mb.Tri(0,0,1+k,1+(k+1)%seg);
        for(int ring=1;ring<4;ring++)for(int k=0;k<seg;k++){int a0=1+(ring-1)*seg+k,a1=1+(ring-1)*seg+(k+1)%seg,b0=1+ring*seg+k,b1=1+ring*seg+(k+1)%seg;mb.Tri(0,a0,b1,a1);mb.Tri(0,a0,b0,b1);}
        var lm=mb.Build("Harbour lake");var tris=lm.triangles;var vs=lm.vertices;for(int i=0;i<tris.Length;i+=3){if(Vector3.Cross(vs[tris[i+1]]-vs[tris[i]],vs[tris[i+2]]-vs[tris[i]]).y<0){int t=tris[i+1];tris[i+1]=tris[i+2];tris[i+2]=t;}}lm.triangles=tris;lm.RecalculateBounds();
        var lakeObj=V3.Obj("Lake surface",V3.SaveMesh(lm,"HarbourLake"),new[]{lake},g);lakeObj.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
        // docks and moored boats
        var props=new GameObject("Docks, boats and quay props").transform;props.SetParent(g,false);
        string[] fishing={"Boat_FishRed","Boat_FishBlue","Boat_FishTeal"};string[] smalls={"Boat_Row","Boat_RowRed"};int boats=0;
        float[] dockAngles={.15f,.85f,1.55f,2.25f,3.55f,4.25f,5.0f,5.7f};
        for(int i=0;i<dockAngles.Length;i++)
        {
            float a=dockAngles[i];var land=Ell(a,1.18f);var inner=Ell(a,.62f);var dir=new Vector3(inner.x-land.x,0,inner.y-land.y).normalized;var mid=(land+inner)*.5f;var pos=new Vector3(mid.x,V3Layout.LakeY-.5f,mid.y);
            var d=V4City.Place("Dock_8",pos,dir,props,1,false);
            if(d){float L=Vector2.Distance(land,inner);d.transform.localScale=new Vector3(1,1,L/8f);d.name="Dock "+i;var col=d.AddComponent<BoxCollider>();col.center=new Vector3(0,1,0);col.size=new Vector3(3.2f,.3f,8f);}
            var side=Vector3.Cross(Vector3.up,dir);
            for(int sd=-1;sd<=1;sd+=2)
            {
                if((i+sd+8)%3==0)continue;var bp=pos+side*sd*3.9f+dir*Random.Range(-1.5f,2.5f);if(!V3Layout.InLake(bp.x,bp.z,-2))continue;
                string m=Random.value<.55f?fishing[Random.Range(0,3)]:smalls[Random.Range(0,2)];var bo=V4City.Place(m,new Vector3(bp.x,V3Layout.LakeY-.12f,bp.z),dir,props,1,false);
                if(bo){var bb=bo.AddComponent<BoatBob>();bb.amp=.07f;bb.roll=2f;boats++;}
            }
        }
        for(int i=0;i<6;i++)
        {
            float a=Random.value*6.28f,r=Random.Range(.12f,.5f);var p=Ell(a,r);if(!V3Layout.InLake(p.x,p.y,-9))continue;
            string m=i<2?(i==0?"Boat_Sail":"Boat_SailRed"):fishing[i%3];var bo=V4City.Place(m,new Vector3(p.x,V3Layout.LakeY-.15f,p.y),new Vector3(Random.value-.5f,0,Random.value-.5f),props,1,false);
            if(bo){var bb=bo.AddComponent<BoatBob>();bb.amp=.1f;bb.drift=1.2f;boats++;}
        }
        // shore props between road and water
        for(int i=0;i<18;i++)
        {
            float a=i*Mathf.PI*2/18+.18f;bool nearDock=dockAngles.Any(dd=>Mathf.Abs(Mathf.DeltaAngle(a*Mathf.Rad2Deg,dd*Mathf.Rad2Deg))<14);if(nearDock)continue;
            var p=Ell(a,1.26f);var inward=new Vector3(V3Layout.LakeC.x-p.x,0,V3Layout.LakeC.y-p.y).normalized;if(V3Layout.InLake(p.x,p.y,1))continue;
            string[] pick={"Prop_FishStallBlue","Prop_FishStallRed","Prop_NetRack","Prop_CrateFish","Prop_Barrel","Prop_Bench","Prop_CrateFish","Prop_Planter"};string m=pick[Random.Range(0,pick.Length)];
            V4City.Place(m,G(p.x,p.y),inward,props,1,m.StartsWith("Prop_Fish"));
        }
        V4City.Place("Prop_Lighthouse",G(252,148),new Vector3(-1,0,-.3f),props,1.15f,true);
        // reeds and gulls
        var reedMats=new[]{V3.Foliage("Reeds A","plant_reed","#ffffff",.5f),V3.Foliage("Reeds B","plant_reed2","#f0ffe8",.5f)};var rbs=new[]{new V3.MB(1),new V3.MB(1)};int reeds=0;
        for(int i=0;i<420;i++)
        {
            float a=Random.value*6.28f;var p=Ell(a,Random.Range(.98f,1.12f));if(dockAngles.Any(dd=>Mathf.Abs(Mathf.DeltaAngle(a*Mathf.Rad2Deg,dd*Mathf.Rad2Deg))<9))continue;
            float y=Mathf.Max(V3.H(p.x,p.y),V3Layout.LakeY-.2f);float w=Random.Range(1.4f,2.4f),h=Random.Range(1.4f,2.8f);float yaw=Random.value*3.14f;var mm=rbs[i%2];
            for(int k=0;k<2;k++){float yy=yaw+k*1.5708f;var r=new Vector3(Mathf.Cos(yy),0,Mathf.Sin(yy))*w*.5f;var q0=new Vector3(p.x,y-.05f,p.y);mm.Quad(0,q0-r,q0+r,q0+r+Vector3.up*h,q0-r+Vector3.up*h,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),new Color(1,1,1,.5f),Vector3.up);}
            reeds++;
        }
        for(int i=0;i<2;i++){var o=V3.Obj("Shore reeds",V3.SaveMesh(rbs[i].Build("Reeds"+i),"HarbourReeds"+i),new[]{reedMats[i]},g);o.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;}
        var gm=V3.Foliage("Gull","deco_gull","#ffffff",.4f);
        var fl=new GameObject("Gulls over the harbour").AddComponent<GullFlock>();fl.transform.SetParent(g,false);fl.material=gm;fl.count=16;fl.centre=new Vector3(V3Layout.LakeC.x,V3Layout.LakeY,V3Layout.LakeC.y);fl.radius=44;fl.height=24;
        var fl2=new GameObject("Gulls over the castle").AddComponent<GullFlock>();fl2.transform.SetParent(g,false);fl2.material=gm;fl2.count=10;fl2.centre=new Vector3(0,70,150);fl2.radius=110;fl2.height=60;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "lake built; boats "+boats+" reeds "+reeds;
    }
}
