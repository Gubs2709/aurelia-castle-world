using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Dense reference forests: deterministic diverse silhouettes, root fans and multiple branch orders.
public static class V7Forest
{
    const string Art="Assets/Art/AnimeWorld/";
    static Mesh[,,] mesh=new Mesh[6,4,2];
    static Material[] bark,leaves;
    static float R(float a,float b)=>Random.Range(a,b);
    static void Tube(V3.MB mb,List<Vector3> p,float r0,float r1,int sides,int sub=0)
    {
        int start=mb.v.Count;float length=0;
        for(int i=0;i<p.Count;i++)
        {
            if(i>0)length+=Vector3.Distance(p[i],p[i-1]);float u=i/(float)(p.Count-1);
            Vector3 tangent=(p[Mathf.Min(i+1,p.Count-1)]-p[Mathf.Max(i-1,0)]).normalized;
            Vector3 side=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.96f?Vector3.forward:Vector3.up).normalized,up=Vector3.Cross(tangent,side);
            float radius=Mathf.Lerp(r0,r1,u);
            for(int k=0;k<=sides;k++)
            {
                float a=k*Mathf.PI*2/sides;var n=side*Mathf.Cos(a)+up*Mathf.Sin(a);
                float fluting=1+.12f*Mathf.Sin(a*5+p[i].y*.75f)+.045f*Mathf.Sin(a*11-p[i].y*.4f);
                mb.Vert(p[i]+n*radius*fluting,n,new Vector2(k/(float)sides*Mathf.Max(1,radius*3),length*.32f),Color.white);
            }
        }
        for(int i=0;i<p.Count-1;i++)for(int k=0;k<sides;k++){int a=start+i*(sides+1)+k;mb.Tri(sub,a,a+sides+1,a+1);mb.Tri(sub,a+1,a+sides+1,a+sides+2);}
    }
    static List<Vector3> Curve(Vector3 a,Vector3 b,Vector3 c,int count)
    {var p=new List<Vector3>();for(int i=0;i<=count;i++){float t=i/(float)count;p.Add((1-t)*(1-t)*a+2*(1-t)*t*b+t*t*c);}return p;}
    static Mesh Make(int kind,int variant,int lod)
    {
        Random.InitState(1729+kind*137+variant*31);var mb=new V3.MB(2);var tips=new List<Vector3>();
        bool tall=kind==2,ancient=kind==5;float radius=ancient?1.1f:kind==0?.64f:kind==3?.55f:.7f;radius*=R(.8f,1.15f);
        float fork=tall?17:R(8,13);float bend=kind==1?3.5f:kind==3?2.8f:R(.6f,2.1f);
        Vector3 lean=new Vector3(R(-1,1),0,R(-1,1)).normalized*bend;
        var trunk=Curve(Vector3.zero,new Vector3(-lean.x*.6f,fork*.6f,-lean.z*.6f),lean+Vector3.up*fork,lod==0?16:8);
        // Root flare is a continuous trunk expansion plus uneven buttress roots.
        for(int i=0;i<trunk.Count;i++)
        {
            float u=i/(float)(trunk.Count-1);trunk[i]+=new Vector3(Mathf.Sin(u*8+variant)*.12f,0,Mathf.Cos(u*7+kind)*.15f)*Mathf.Sin(u*Mathf.PI);
        }
        Tube(mb,trunk,radius*1.2f,radius*.62f,lod==0?14:9);
        int roots=variant+7;
        for(int i=0;i<roots;i++)
        {
            float a=i*Mathf.PI*2/roots+R(-.2f,.2f);Vector3 d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));float len=radius*R(3,6);
            Tube(mb,Curve(Vector3.up*R(.6f,1.5f),d*len*.5f+Vector3.up*.22f,d*len-Vector3.up*R(.08f,.3f),lod==0?7:4),radius*R(.28f,.48f),.025f,lod==0?9:6);
        }
        void Branch(Vector3 start,Vector3 dir,float len,float r,int depth)
        {
            Vector3 end=start+dir*len;end.y+=len*(depth==0?.16f:-.04f);
            Vector3 middle=Vector3.Lerp(start,end,.5f)+Vector3.up*(kind==3?len*.05f:len*.13f)+Random.insideUnitSphere*len*.06f;
            Tube(mb,Curve(start,middle,end,lod==0?Mathf.Max(4,7-depth):3),r,Mathf.Max(.018f,r*.28f),lod==0?(depth==0?10:7):5);
            if(depth>=3){tips.Add(end);return;}
            int kids=depth==0?3:2;
            for(int k=0;k<kids;k++)
            {
                float u=R(.5f,.94f);Vector3 p=(1-u)*(1-u)*start+2*(1-u)*u*middle+u*u*end;
                Vector3 nd=(dir+Random.insideUnitSphere*.85f+Vector3.up*.16f).normalized;
                if(nd.y<-.15f)nd.y=Mathf.Abs(nd.y)*.4f;
                Branch(p,nd.normalized,len*R(.48f,.68f),r*.43f,depth+1);
            }
            tips.Add(end);if(depth==1)tips.Add(Vector3.Lerp(middle,end,.6f));
        }
        int limbs=tall?8:ancient?11:9;
        for(int i=0;i<limbs;i++)
        {
            float a=i*Mathf.PI*2/limbs+R(-.2f,.2f);float angle=R(tall?20:40,tall?44:79)*Mathf.Deg2Rad;
            Vector3 d=new Vector3(Mathf.Cos(a)*Mathf.Sin(angle),Mathf.Cos(angle),Mathf.Sin(a)*Mathf.Sin(angle));
            Vector3 p=trunk[trunk.Count-1]-Vector3.up*R(0,2.5f);float len=R(tall?8:10,tall?13:16);
            Branch(p,d,len,radius*R(.38f,.62f),0);
        }
        if(tall||kind==0)Branch(trunk.Last(),Vector3.up,13,radius*.5f,0);
        int cards=lod==0?6:2;
        foreach(var tip in tips)
        {
            for(int i=0;i<cards;i++)
            {
                float size=R(2.7f,4.2f)*(lod==1?1.3f:1);Vector3 p=tip+Random.insideUnitSphere*size*.45f;
                Vector3 outward=(p-new Vector3(0,12,0)).normalized;
                Vector3 n=i%3==0?(outward*.35f+Vector3.up*.65f).normalized:i%3==1?new Vector3(outward.x,.08f,outward.z).normalized:(Random.onUnitSphere+outward*.3f).normalized;
                Quaternion q=Quaternion.LookRotation(n,Random.onUnitSphere);Vector3 right=q*Vector3.right*size*.5f,up=q*Vector3.up*size*.5f;
                float shade=R(.78f,1.06f);Color c=new Color(shade,shade,shade,Mathf.Clamp01(p.y/30));
                mb.Quad(1,p-right-up,p+right-up,p+right+up,p-right+up,Vector2.zero,Vector2.right,Vector2.one,Vector2.up,c,n);
            }
        }
        return V3.SaveMesh(mb.Build("Reference branching tree"),"V7Tree_"+kind+"_"+variant+"_"+lod);
    }
    static void Prep()
    {
        foreach(string name in new[]{"PaintedBark","AutumnMaple","BlossomBough"})
        {
            string p=Art+name+".png";AssetDatabase.ImportAsset(p);var imp=AssetImporter.GetAtPath(p) as TextureImporter;if(!imp)throw new Exception("Missing generated asset "+p);
            bool alpha=name!="PaintedBark";imp.alphaIsTransparency=alpha;imp.mipmapEnabled=true;imp.mipMapsPreserveCoverage=alpha;imp.alphaTestReferenceValue=.32f;imp.wrapMode=alpha?TextureWrapMode.Clamp:TextureWrapMode.Repeat;imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.CompressedHQ;imp.SaveAndReimport();
        }
        bark=new Material[4];string[] tint={"#bab0aa","#8e929e","#bfa085","#998a98"};
        for(int i=0;i<4;i++){bark[i]=V3.Mat("V7 painted bark "+i,"Universal Render Pipeline/Lit");bark[i].SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"PaintedBark.png"));bark[i].SetColor("_BaseColor",V3.C(tint[i])*1.9f);bark[i].SetFloat("_Smoothness",.08f);}
        leaves=new Material[6];string[] names={"Scarlet maple","Golden orange","Rose blossom","Pale blossom","Forest green","Crimson red"};string[] tints={"#ffb3a4","#ffdf77","#f6bedc","#fff8f0","#a3d99b","#d590a1"};
        for(int i=0;i<6;i++){leaves[i]=V3.Mat("V7 "+names[i],"Aurelia/Natural Foliage");leaves[i].SetTexture("_BaseMap",i==2||i==3?AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"BlossomBough.png"):i==4?V3.T("leaf_oak"):AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"AutumnMaple.png"));leaves[i].SetColor("_BaseColor",V3.C(tints[i]));leaves[i].SetFloat("_Cutoff",.32f);leaves[i].SetFloat("_Lift",.4f);leaves[i].SetFloat("_Wind",1.3f);}
        leaves[1].SetFloat("_HueShift",.095f);leaves[1].SetFloat("_Saturation",.85f);leaves[3].SetFloat("_Saturation",.28f);leaves[5].SetFloat("_HueShift",-.035f);
        for(int k=0;k<6;k++)for(int v=0;v<4;v++)for(int lod=0;lod<2;lod++)mesh[k,v,lod]=Make(k,v,lod);
    }
    public static string Build()
    {
        Prep();var old=GameObject.Find("17 • Towering mixed forests and blossom groves");var sites=old.transform.Cast<Transform>().Where(t=>t.name.Contains("•")).Select(t=>{string name=t.name;var lg=t.GetComponent<LODGroup>();if(lg){var mats=lg.GetLODs()[0].renderers[0].sharedMaterials;if(mats[1].name.Contains("blossom"))name+=" blossom";}if(name.Contains("• 5 "))name+=" ancient";return (p:t.position,name:name);}).ToList();
        var group=V3.FreshGroup("17 • Towering mixed forests and blossom groves");Random.InitState(7741);
        // A genuine under-canopy route, extending the old short forest into a sizeable grove.
        for(int side=-1;side<=1;side+=2)for(int j=0;j<24;j++)
        {float z=-244+j*5.1f;float x=z<-214?Mathf.Lerp(-34,-90,(z+244)/30):V3.PathX(z);x+=side*R(9,18);if(V3Layout.ZoneOf(x,z,-12)<0&&!sites.Any(p=>Vector2.Distance(new Vector2(p.p.x,p.p.z),new Vector2(x,z))<6))sites.Add((new Vector3(x,V3.H(x,z),z),"Reference canopy • cathedral"));}
        for(int j=0;j<80;j++)
        {float z=R(-278,-145),x=R(-195,-62);if(V3Layout.ZoneOf(x,z,-15)>=0||V3Layout.ViaductDist(x,z)<20||sites.Any(p=>Vector2.Distance(new Vector2(p.p.x,p.p.z),new Vector2(x,z))<7))continue;sites.Add((new Vector3(x,V3.H(x,z),z),"Reference woodland • autumn"));}
        var roads=V3Layout.Roads();int n=0;foreach(var s in sites)
        {
            if(V3Layout.ZoneOf(s.p.x,s.p.z,-12)>=0||V3Layout.ViaductDist(s.p.x,s.p.z)<15)continue;
            if(s.p.x>275&&s.p.z>90)continue;
            if(roads.Any(rd=>rd.pts.Any(p=>Vector2.Distance(p,new Vector2(s.p.x,s.p.z))<rd.half+3.8f)))continue;
            Random.InitState(4000+Mathf.RoundToInt(s.p.x*37+s.p.z*71));string lower=s.name.ToLower();bool pink=lower.Contains("blossom")||lower.Contains("cherry");int kind=lower.Contains("ancient")?5:Random.Range(0,5),variant=Random.Range(0,4),colour=pink?Random.Range(2,4):lower.Contains("crimson")?5:Random.value<.16f?4:Random.Range(0,2);
            float desired=kind==5?R(64,78):kind==2?R(45,64):R(36,56);if(s.p.z>85)desired*=.8f;
            float sy=desired/mesh[kind,variant,0].bounds.max.y;var root=new GameObject("Reference tree • "+kind+" variant "+variant+" h"+Mathf.RoundToInt(desired)).transform;root.SetParent(group,false);root.position=new Vector3(s.p.x,V3.H(s.p.x,s.p.z)-.18f,s.p.z);root.rotation=Quaternion.Euler(R(-1.5f,1.5f),R(0,360),R(-1,1));root.localScale=new Vector3(sy*R(.86f,1.16f),sy,sy*R(.9f,1.15f));
            var lods=new LOD[2];int barkKind=Random.Range(0,4);
            for(int l=0;l<2;l++){var o=V3.Obj("Branches, roots and dense foliage LOD"+l,mesh[kind,variant,l],new[]{bark[barkKind],leaves[colour]},root);lods[l]=new LOD(l==0?.17f:.012f,new[]{o.GetComponent<Renderer>()});}
            var lg=root.gameObject.AddComponent<LODGroup>();lg.SetLODs(lods);lg.RecalculateBounds();lg.fadeMode=LODFadeMode.None;
            var cap=root.gameObject.AddComponent<CapsuleCollider>();cap.radius=kind==5?1.1f:.62f;cap.height=13;cap.center=Vector3.up*6.5f;n++;
        }
        // True small leaf silhouettes on the forest floor, including the walking trail.
        var litter=new V3.MB();Color[] palette={V3.C("#b74224"),V3.C("#d47a24"),V3.C("#d8af42"),V3.C("#a7353c")};Random.InitState(6721);
        for(int i=0;i<6500;i++)
        {
            float z=R(-267,-132),x=R(-174,-55);if(V3Layout.ZoneOf(x,z,-4)>=0)continue;float size=R(.07f,.22f),yaw=R(0,Mathf.PI*2);var center=new Vector3(x,V3.H(x,z)+.045f,z);int start=litter.Vert(center,Vector3.up,Vector2.one*.5f,palette[i%4]);
            for(int k=0;k<16;k++){float a=yaw+k*Mathf.PI*2/16;float rad=(k%2==0?1:.42f)*size;float px=x+Mathf.Cos(a)*rad,pz=z+Mathf.Sin(a)*rad;litter.Vert(new Vector3(px,V3.H(px,pz)+.04f,pz),Vector3.up,Vector2.zero,palette[i%4]);}
            for(int k=0;k<16;k++)litter.Tri(0,start,start+1+(k+1)%16,start+1+k);
        }
        var lm=V3.Mat("V7 fallen autumn leaves","Aurelia/Meadow");lm.SetColor("_BaseColor",Color.white);V3.Obj("Scattered crimson amber and gold fallen leaves",V3.SaveMesh(litter.Build("Forest leaf litter"),"V7LeafLitter"),new[]{lm},group);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());return "V7 trees="+n+"; 24 different branching/root silhouettes, 36-78m, 2 LODs";
    }
}
