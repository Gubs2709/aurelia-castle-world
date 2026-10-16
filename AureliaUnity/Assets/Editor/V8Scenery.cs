using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Aurelia;
using Random=UnityEngine.Random;

public static class V8Scenery
{
    public static string Lighting()
    {
        foreach(var vol in UnityEngine.Object.FindObjectsByType<Volume>())
        {
            var p=vol.sharedProfile;if(!p)continue;
            // Keep an active zero override; an inactive component falls back to the URP default profile's bloom.
            if(p.TryGet<Bloom>(out var bloom)){bloom.active=true;bloom.intensity.Override(0);}
            if(p.TryGet<ColorAdjustments>(out var c)){c.saturation.Override(4);c.contrast.Override(4);c.postExposure.Override(-.07f);c.colorFilter.Override(Color.white);}
            if(p.TryGet<LiftGammaGain>(out var gain))gain.active=false;
            if(p.TryGet<Vignette>(out var vig)){vig.active=false;vig.intensity.Override(0);}
            EditorUtility.SetDirty(p);
        }
        if(RenderSettings.sun){RenderSettings.sun.intensity=1.3f;RenderSettings.sun.color=V3.C("#fff6df");RenderSettings.sun.transform.rotation=Quaternion.Euler(38,-35,0);EditorUtility.SetDirty(RenderSettings.sun);}
        RenderSettings.ambientSkyColor=V3.C("#a1c8de");RenderSettings.ambientEquatorColor=V3.C("#aebcaa");RenderSettings.ambientGroundColor=V3.C("#728761");RenderSettings.fogColor=V3.C("#a9c8d6");RenderSettings.fogDensity=.00037f;
        return "Bloom explicitly overridden to zero; gain disabled; blue gradient sky; soft daytime grade";
    }
    static List<Vector3> GroundPath(Vector3[] points)
    {return V8Landscape.Sample(points,1.6f).Select(p=>new Vector3(p.x,V3.H(p.x,p.z)+.05f,p.z)).ToList();}
    public static string Riverside()
    {
        var g=V3.FreshGroup("38 • Cottage river retreat and old stone bridge");Random.InitState(835);
        // Clear a deliberate riverside clearing; preserve the large autumn forest farther west.
        var old=GameObject.Find("17 • Towering mixed forests and blossom groves");foreach(var t in old.transform.Cast<Transform>().Where(t=>Vector2.Distance(new Vector2(t.position.x,t.position.z),new Vector2(-54,-164))<39).ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        float z=-168,cx=V3.RiverX(z),half=28,width=3.9f;var bridge=new V3.MB(2);int count=60;
        float Deck(float u)=>4.15f+4.8f*Mathf.Sin(Mathf.PI*u);float Arch(float u)=>1.75f+5.05f*Mathf.Sin(Mathf.PI*u);
        for(int i=0;i<count;i++)
        {
            float u=i/(float)count,v=(i+1)/(float)count;float x0=cx-half+u*half*2,x1=cx-half+v*half*2;float y0=Deck(u),y1=Deck(v),a0=Arch(u),a1=Arch(v);
            bridge.Quad(0,new Vector3(x0,y0,z-width/2),new Vector3(x0,y0,z+width/2),new Vector3(x1,y1,z+width/2),new Vector3(x1,y1,z-width/2),Vector2.zero,Vector2.right,Vector2.one,Vector2.up,Color.white,Vector3.up);
            foreach(float side in new[]{-1f,1f})
            {
                float zz=z+side*width*.5f;bridge.Quad(0,new Vector3(x0,a0,zz),new Vector3(x1,a1,zz),new Vector3(x1,y1,zz),new Vector3(x0,y0,zz),Vector2.zero,Vector2.right,Vector2.one,Vector2.up,Color.white);
                V8Kit.Beam(bridge,new Vector3(x0,y0+.43f,zz),new Vector3(x1,y1+.43f,zz),.55f,0);V8Kit.Beam(bridge,new Vector3(x0,y0+.75f,zz),new Vector3(x1,y1+.75f,zz),.58f,1);
                // Radial voussoir outlines around the real open arch.
                if(i%3==0)V8Kit.Beam(bridge,new Vector3(x0,a0+.05f,zz+side*.035f),new Vector3(x0+(u-.5f)*.42f,a0+.85f,zz+side*.035f),.055f,1);
            }
            bridge.Quad(0,new Vector3(x0,a0,z-width/2),new Vector3(x0,a0,z+width/2),new Vector3(x1,a1,z+width/2),new Vector3(x1,a1,z-width/2),Vector2.zero,Vector2.right,Vector2.one,Vector2.up,Color.white);
        }
        V8Kit.Obj("Old arched riverside stone bridge",bridge,new[]{V8Kit.Surface("Old bridge masonry","#c1c5b3","wall_stone",.32f),V8Kit.Surface("Bridge carved coping","#e1dec9","CliffStone",.42f)},g,true);
        var cottagePos=new Vector3(-63,V3.H(-63,-146),-146);var cottage=V8Kit.Place("V8_RiverCottage",cottagePos,new Vector3(.7f,0,-.7f),g,1.05f,true);cottage.name="Ivy cottage above the river garden";
        V8Kit.Path("Winding riverbank cottage path",GroundPath(new[]{new Vector3(-98,0,-185),new Vector3(-84,0,-178),new Vector3(-69,0,-166),new Vector3(-63,0,-153)}),2.3f,g);
        var bridgeApproach=GroundPath(new[]{new Vector3(-68,0,-166),new Vector3(cx-half-8,0,z-1),new Vector3(cx-half,0,z)});bridgeApproach[bridgeApproach.Count-1]=new Vector3(cx-half,Deck(0),z);V8Kit.Path("Stone edged bridge approach",bridgeApproach,2.9f,g);
        var bridgeExit=GroundPath(new[]{new Vector3(cx+half,0,z),new Vector3(cx+half+9,0,z+1),new Vector3(cx+half+24,0,z+13)});bridgeExit[0]=new Vector3(cx+half,Deck(1),z);V8Kit.Path("East meadow path from bridge",bridgeExit,2.9f,g);
        var heroPos=new Vector3(-58,V3.H(-58,-190),-190);V8Grove.Tree(heroPos,28,g,1,"The lantern tree • broad riverside canopy");
        for(int i=0;i<13;i++){float angle=i*2.4f;float x=-66+Mathf.Sin(angle)*Random.Range(12,29),zz=-156+Mathf.Cos(angle)*Random.Range(15,32);if(x>cx-29)continue;V8Grove.Tree(new Vector3(x,V3.H(x,zz),zz),Random.Range(14,24),g,i+3);}
        var picnicPos=new Vector3(-48,V3.H(-48,-190)+.045f,-190);V8Kit.Place("V8_Picnic",picnicPos,Vector3.forward,g,1.4f,false);
        var lampPos=heroPos+new Vector3(4.2f,4.6f,-1);V8Kit.Place("V8_HangingLantern",lampPos,Vector3.forward,g,1.3f,false);var rope=new V3.MB();V8Grove.Tube(rope,new List<Vector3>{lampPos+Vector3.up*.85f,lampPos+new Vector3(.2f,3,-.2f),heroPos+new Vector3(4.1f,8,-1.3f)},.022f,.022f);V8Kit.Obj("Lantern rope to the old tree",rope,new[]{V8Kit.Material("rope")},g);
        var flowers=new V3.MB();for(int i=0;i<780;i++)
        {
            float zz=Random.Range(-212,-126),x=Random.Range(-88,-39);float y=V3.H(x,zz);if(y<2.25f||Vector2.Distance(new Vector2(x,zz),new Vector2(cottagePos.x,cottagePos.z))<5.5f)continue;
            V8Kit.FlowerCard(flowers,new Vector3(x,y+.02f,zz),Random.Range(.28f,.68f),Random.value*Mathf.PI);
        }
        V8Kit.Obj("Daisies cosmos and cornflowers along river meadow",flowers,new[]{V8Kit.Flowers},g);
        for(int i=0;i<48;i++){float zz=Random.Range(-205,-121),x=V3.RiverX(zz)-Random.Range(24,29);float y=V3.H(x,zz);V8Kit.Rock(new Vector3(x,Mathf.Max(1.25f,y),zz),new Vector3(Random.Range(1,2.5f),Random.Range(.6f,1.8f),Random.Range(1,2)),g,i);}
        // A narrow planted stone retaining wall around the cottage's river side.
        var wall=new V3.MB();for(int i=0;i<14;i++){var p=cottagePos+new Vector3(4.5f,-.25f,-6+i*.85f);V8Kit.Box(wall,p,new Vector3(.65f,.8f,.85f),Quaternion.identity);}V8Kit.Obj("Cottage garden river retaining wall",wall,new[]{V8Kit.Surface("Cottage garden stone","#c1c8af","wall_stone",.5f)},g);
        var boat=V8Kit.Place("V8_FishingBoat",new Vector3(cx,1.65f,-191),Vector3.forward,g,.36f,false);boat.name="Small river skiff";var bob=boat.AddComponent<BoatBob>();bob.amp=.035f;bob.roll=.6f;
        var river=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Turquoise river.mat");river.SetColor("_BaseColor",V3.C("#208e9f"));river.SetColor("_ShallowColor",V3.C("#61cfc4"));EditorUtility.SetDirty(river);
        var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();c.cottagePoint=new Vector3(-47,V3.H(-47,-198)+.25f,-198);c.retreatPoint=c.cottagePoint;c.headlandPoint=new Vector3(342,34.2f,339);EditorUtility.SetDirty(c);
        return "Riverside retreat: cottage, true arched bridge, broad trees, picnic, hanging lantern, flowers and stones";
    }
    public static string Mountains()
    {
        var g=V3.FreshGroup("40 • Layered snow peaks and alpine ridges");Random.InitState(826);var material=V3.Mat("V8 painted alpine mountain palette","Aurelia/Alpine Painted");material.SetColor("_BaseColor",Color.white);
        var summits=new List<Vector4>();for(int i=0;i<12;i++)summits.Add(new Vector4(-1450+i*138,930+(i%3)*175,Random.Range(290,490)+(i==6?150:0),Random.Range(180,280)));
        float Relief(float x,float z)
        {
            float h=0;foreach(var peak in summits){float dx=(x-peak.x)/peak.w,dz=(z-peak.y)/(peak.w*.67f);float r=Mathf.Sqrt(dx*dx+dz*dz);h=Mathf.Max(h,peak.z*Mathf.Exp(-Mathf.Pow(r,1.45f)*1.65f));}
            float ridge=0,amp=1,freq=.011f;for(int oct=0;oct<5;oct++){ridge+=(1-Mathf.Abs(Mathf.PerlinNoise(x*freq+17,z*freq+41)*2-1))*amp;amp*=.48f;freq*=2.1f;}
            return 20+h*(.60f+ridge*.32f);
        }
        int nx=240,nz=115;var mountain=new V3.MB();
        for(int iz=0;iz<=nz;iz++)for(int ix=0;ix<=nx;ix++)
        {
            float x=-1600+ix*8,z=560+iz*10,h=Relief(x,z);float dx=Relief(x+3,z)-Relief(x-3,z),dz=Relief(x,z+3)-Relief(x,z-3);var n=new Vector3(-dx,6,-dz).normalized;
            float weather=Mathf.PerlinNoise(x*.045f+17,z*.045f)*.45f+Mathf.PerlinNoise(x*.11f,z*.11f)*.2f;
            float snow=Mathf.SmoothStep(0,1,(h-150)/140+weather-.1f)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.08f,.65f,n.y));var rock=Color.Lerp(V3.C("#3c759f"),V3.C("#99bad0"),weather);var col=Color.Lerp(rock,V3.C("#f6faf7"),snow);
            mountain.Vert(new Vector3(x,h,z),n,Vector2.zero,col);
        }
        for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int a=z*(nx+1)+x;mountain.Tri(0,a,a+nx+1,a+1);mountain.Tri(0,a+1,a+nx+1,a+nx+2);}
        V8Kit.Obj("Twelve connected alpine summits carved by gullies and snow",mountain,new[]{material},g);
        // Conifer foothill silhouettes, clustered below the snowy range.
        var conifers=new V3.MB();for(int i=0;i<170;i++)
        {
            float x=Random.Range(-460,430),z=Random.Range(390,650),h=Random.Range(10,27),y=V3.H(x,Mathf.Min(z,540));
            for(int tier=0;tier<4;tier++)for(int k=0;k<7;k++)
            {float a=k*Mathf.PI*2/7,b=(k+1)*Mathf.PI*2/7,r=h*(.23f-tier*.035f),baseY=y+tier*h*.17f;conifers.Tri3(0,new Vector3(x+Mathf.Sin(a)*r,baseY,z+Mathf.Cos(a)*r),new Vector3(x,baseY+h*.48f,z),new Vector3(x+Mathf.Sin(b)*r,baseY,z+Mathf.Cos(b)*r),Vector2.zero,Vector2.up,Vector2.right,V3.C(tier%2==0?"#2f6555":"#417c5c"));}
        }
        V8Kit.Obj("Deep green conifer foothills",conifers,new[]{material},g);return "twelve distinct snow peaks and conifer foothills";
    }
}
