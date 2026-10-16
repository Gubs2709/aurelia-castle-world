using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;
using Random=UnityEngine.Random;

public static class V8Harbor
{
    public static List<Vector3> Boardwalk()
    {var p=new List<Vector3>();for(float z=126;z<=288;z+=2)p.Add(new Vector3(V8Landscape.Shore(z)+.3f,3.10f,z));return p;}
    static GameObject Dock(string name,Vector3 a,Vector3 b,float width,Transform parent)
    {
        var mb=new V3.MB(3);var dir=(b-a).normalized;var right=Vector3.Cross(Vector3.up,dir);var q=Quaternion.LookRotation(dir);float length=Vector3.Distance(a,b);int count=Mathf.CeilToInt(length/.23f);
        for(int i=0;i<count;i++){var p=Vector3.Lerp(a,b,(i+.5f)/count);V8Kit.Box(mb,p-Vector3.up*.11f,new Vector3(width,.22f,length/count-.012f),q,i%7==0?1:0);}
        for(float d=0;d<=length;d+=3.8f)foreach(float side in new[]{-1f,1f})
        {
            var p=a+dir*d+right*side*(width*.5f-.12f);V8Grove.Tube(mb,new List<Vector3>{p-Vector3.up*5,p+Vector3.up*.65f},.19f,.17f,1,9);
            V8Kit.Box(mb,p+Vector3.up*.65f,new Vector3(.34f,.035f,.34f),q,0);
            if(d+3.8f<=length) V8Grove.Tube(mb,new List<Vector3>{p+Vector3.up*.5f,p+dir*1.9f+Vector3.up*.25f,p+dir*3.8f+Vector3.up*.5f},.025f,.025f,2,6);
        }
        return V8Kit.Obj(name,mb,new[]{V8Kit.Material("wood_light"),V8Kit.Material("wood_dark"),V8Kit.Material("rope")},parent,true);
    }
    static void People(Transform g,List<Vector3> shore)
    {
        for(int i=0;i<22;i++)
        {
            int index=4+i*3;var p=shore[Mathf.Min(index,shore.Count-3)];var facing=Vector3.right;
            var npc=V8Kit.Place("V8_Resident",p+new Vector3((i%3-1)*.75f,.01f,0),facing,g,.93f,false);npc.name="Harbour neighbour "+i;
            var life=npc.AddComponent<HarbourResidentMotion>();life.phase=i*1.7f;life.walking=i%4==0;
            if(life.walking){int j=Mathf.Min(index+6,shore.Count-2);life.route=new[]{shore[Mathf.Min(index,shore.Count-3)]+Vector3.up*.01f,shore[j]+Vector3.up*.01f};}
            else if(i%3==1)
            {
                var rod=new V3.MB(2);var baseP=npc.transform.position+Vector3.right*.4f+Vector3.up*1.1f;var tip=baseP+new Vector3(2.0f,1.6f,.12f);
                V8Grove.Tube(rod,new List<Vector3>{baseP,baseP+new Vector3(.8f,1,.08f),tip},.028f,.012f,0,6);V8Grove.Tube(rod,new List<Vector3>{tip,tip+new Vector3(1.1f,-.5f,.04f),new Vector3(tip.x+2,1.7f,tip.z+.1f)},.008f,.008f,1,4);
                V8Kit.Obj("Fishing rod and line "+i,rod,new[]{V8Kit.Material("wood_dark"),V8Kit.Surface("Fishing line","#c5d6d2")},g);
            }
        }
    }
    public static string Build()
    {
        var old=GameObject.Find("35 • Open eastern ocean and seaward harbour");if(old)UnityEngine.Object.DestroyImmediate(old);
        var g=V3.FreshGroup("35 • Sheltered turquoise bay and cliff village");Random.InitState(881);
        var ocean=V3.Mat("V8 turquoise harbour gradient","Aurelia/Anime Ocean");ocean.SetColor("_Deep",V3.C("#086989"));ocean.SetColor("_Shallow",V3.C("#24bfb2"));ocean.SetColor("_Crest",V3.C("#d9f7e9"));
        var watermesh=AssetDatabase.LoadAssetAtPath<Mesh>(V3.Dir+"/Meshes/V7Ocean.asset");var sea=V3.Obj("Turquoise bay to open ocean",watermesh,new[]{ocean},g);sea.layer=4;
        var shore=Boardwalk();
        for(int i=0;i<shore.Count-1;i++)Dock("Coastal wooden promenade segment "+i,shore[i],shore[i+1],4.1f,g);
        V8Kit.Path("Continuous winding flower edged hillside path",V8Landscape.Route,3.8f,g);
        foreach(var plot in V8Landscape.Plots)
        {
            var go=V8Kit.Place("V8_BayHouse_"+plot.type,plot.p,plot.face,g);go.name="Colourful cliff house • "+plot.type;
            var nearest=V8Landscape.Route.OrderBy(p=>(new Vector2(p.x-plot.p.x,p.z-plot.p.z)).sqrMagnitude).First();if(plot.p.y<5)nearest=new Vector3(V8Landscape.Shore(plot.p.z)+.3f,3.1f,plot.p.z);
            var a=plot.p+plot.face*3.2f;a.y=plot.p.y+.03f;var pts=V8Landscape.Sample(new[]{a,Vector3.Lerp(a,nearest,.5f),nearest},1.5f);V8Kit.Path("Cottage garden approach "+plot.type+"_"+plot.p.z.ToString("F0"),pts,1.7f,g,false);
            var flowers=new V3.MB();for(int i=0;i<18;i++){float angle=i*Mathf.PI*2/18;var p=plot.p+new Vector3(Mathf.Sin(angle)*5,0,Mathf.Cos(angle)*4.2f);p.y=V3.H(p.x,p.z);V8Kit.FlowerCard(flowers,p,Random.Range(.35f,.7f),angle);}V8Kit.Obj("Cliff cottage flower garden "+plot.p.z.ToString("F0"),flowers,new[]{V8Kit.Flowers},g);
        }
        int boats=0;
        for(int i=0;i<5;i++)
        {
            float z=142+i*30.5f;float x=V8Landscape.Shore(z);Vector3 a=new Vector3(x+.5f,3.10f,z),dir=new Vector3(1,0,(i-2)*.14f).normalized,b=a+dir*(i%2==0?24:18);
            Dock("Small timber landing stage "+i,a,b,3.0f,g);
            V8Kit.Place("V8_DockDetails",a+dir*7+new Vector3(0,.02f,.7f),dir,g,.75f,false);
            for(int j=0;j<2;j++)
            {var p=Vector3.Lerp(a,b,.60f)+new Vector3(j==0?2:-2,0,(j==0?-1:1)*4.3f);p.y=1.68f;var bo=V8Kit.Place(j==0?"V8_FishingBoat":"V8_SailingBoat",p,new Vector3(.1f,0,1),g,.7f,false);bo.name="Painted moored boat "+boats++;bo.AddComponent<BoatBob>().amp=.055f;}
        }
        for(int i=0;i<3;i++){var bo=V8Kit.Place("V8_SailingBoat",new Vector3(377+i*28,1.67f,168+i*38),new Vector3(-.4f,0,1),g,.9f,false);bo.name="Sailing boat in open bay "+i;var bob=bo.AddComponent<BoatBob>();bob.amp=.10f;bob.drift=.65f;boats++;}
        // Lighthouse on a low rocky spur, white rather than the previous candy stripe.
        float lx=V8Landscape.Shore(119);var lighthouse=V4City.Place("Prop_Lighthouse",new Vector3(lx-2,3.1f,119),Vector3.right,g,.90f,true);lighthouse.name="White lighthouse on the bay headland";
        foreach(var r in lighthouse.GetComponentsInChildren<MeshRenderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m&&m.name.Contains("lighthouse")?V8Kit.Surface("Lighthouse ivory","#e6e6d7"):m).ToArray();
        for(int i=0;i<14;i++){float a=i*Mathf.PI*2/14;V8Kit.Rock(new Vector3(lx-2+Mathf.Sin(a)*5,1.2f,119+Mathf.Cos(a)*6),new Vector3(3,2,3),g,i);}
        string[] stall={"Prop_FishStallRed","Prop_FishStallBlue","Prop_NetRack","Prop_CrateFish"};for(int i=0;i<12;i++){float z=139+i*12;V4City.Place(stall[i%4],new Vector3(V8Landscape.Shore(z)-3.2f,3.1f,z),Vector3.right,g,.85f,false);}
        People(g,shore);V8Grove.CoastTrees(g);V8Grove.Headland(V3.Root);
        var gull=new GameObject("Gulls over the sheltered bay").AddComponent<GullFlock>();gull.transform.SetParent(g,false);gull.material=V3.Foliage("Gull","deco_gull");gull.count=26;gull.centre=new Vector3(354,2,210);gull.height=17;gull.radius=73;
        var cloud=V3.Root.GetComponentsInChildren<CloudDrift>().First(t=>t.name.StartsWith("Slow towering"));for(int i=0;i<5;i++){var o=UnityEngine.Object.Instantiate(cloud.gameObject,g);o.name="Drifting bay cumulus "+i;o.transform.position=new Vector3(1150+i*180,260+i%2*130,-800+i*580);o.transform.rotation=Quaternion.Euler(0,90,0);o.transform.localScale=new Vector3(420+i*20,480+i%2*90,1);o.GetComponent<CloudDrift>().speed=.8f+i*.16f;}
        var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();c.harbourPoint=shore[35]+Vector3.up*.2f;EditorUtility.SetDirty(c);EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());return "V8 sheltered bay: "+V8Landscape.Plots.Count+" hillside houses, "+boats+" boats, five small piers, 22 residents, no stair flights";
    }
}
