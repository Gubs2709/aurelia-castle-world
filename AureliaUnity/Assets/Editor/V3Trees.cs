using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Real branching trees: textured bark tubes + clusters of painted leaf cards (from tools/texgen sprites).
public static class V3Trees
{
    class Spec{public string name;public float trunkR,bare,lean,crown,card,flare=1.7f;public int limbs;public Vector2 limbAngle;public float droop;public bool hanging;public int cardsPerTip=4;public string bark;}
    static readonly Spec[] Specs={
        new Spec{name="tall",trunkR=.5f,bare=.60f,lean=.5f,crown=7.5f,card=6.2f,limbs=6,limbAngle=new Vector2(18,40),droop=0,cardsPerTip=4,bark="bark_dark"},
        new Spec{name="oak",trunkR=.95f,bare=.28f,lean=1.4f,crown=13f,card=7.4f,limbs=5,limbAngle=new Vector2(38,70),droop=.12f,cardsPerTip=4,bark="bark_oak",flare=2.1f},
        new Spec{name="sakura",trunkR=.68f,bare=.20f,lean=2.4f,crown=14f,card=6.6f,limbs=5,limbAngle=new Vector2(55,82),droop=.35f,cardsPerTip=4,bark="bark_cherry",flare=1.9f},
        new Spec{name="maple",trunkR=.6f,bare=.40f,lean=1.0f,crown=9.5f,card=6.0f,limbs=6,limbAngle=new Vector2(28,52),droop=.1f,cardsPerTip=4,bark="bark_oak"},
        new Spec{name="birch",trunkR=.34f,bare=.46f,lean=.8f,crown=5.8f,card=4.6f,limbs=7,limbAngle=new Vector2(14,32),droop=.18f,cardsPerTip=3,bark="bark_birch",flare=1.4f},
        new Spec{name="wisteria",trunkR=.7f,bare=.24f,lean=2.2f,crown=12.5f,card=6.4f,limbs=5,limbAngle=new Vector2(55,80),droop=.3f,hanging=true,cardsPerTip=3,bark="bark_cherry",flare=1.9f}};
    const int Variants=3;const float H=30f;

    static V3.MB Make(Spec s,int seed)
    {
        Random.InitState(seed*131+s.name.GetHashCode()%997);var b=new V3.MB(3);var tips=new List<(Vector3 p,Vector3 d,float depth)>();
        void Tube(Vector3 a,Vector3 dirA,Vector3 bp,Vector3 dirB,float r0,float r1,float flare,float uvTiles)
        {
            int rings=Mathf.Max(4,(int)(Vector3.Distance(a,bp)/.9f)),sides=r0>.35f?12:7;int baseIdx=b.v.Count;
            float len=Vector3.Distance(a,bp);
            for(int i=0;i<=rings;i++)
            {
                float t=i/(float)rings;Vector3 c=Hermite(a,dirA*len*.5f,bp,dirB*len*.5f,t);float wob=Mathf.Sin(t*Mathf.PI)*len*.045f;c+=new Vector3(Mathf.PerlinNoise(c.y*.35f+a.x,c.z*.35f+7f)-.5f,Mathf.PerlinNoise(c.x*.3f+3f,c.z*.3f)-.5f,Mathf.PerlinNoise(c.x*.35f,c.y*.35f+a.z)-.5f)*2*wob;Vector3 tan=(Hermite(a,dirA*len*.5f,bp,dirB*len*.5f,Mathf.Min(1,t+.02f))-Hermite(a,dirA*len*.5f,bp,dirB*len*.5f,Mathf.Max(0,t-.02f))).normalized;
                Vector3 side=Vector3.Cross(tan,Mathf.Abs(tan.y)>.95f?Vector3.right:Vector3.up).normalized,fwd=Vector3.Cross(side,tan);
                float rr=Mathf.Lerp(r0,r1,t)*(1+(flare-1)*Mathf.Pow(1-t,5));
                for(int k=0;k<=sides;k++)
                {
                    float ang=k/(float)sides*Mathf.PI*2;Vector3 nrm=(Mathf.Cos(ang)*side+Mathf.Sin(ang)*fwd);float bump=1+.09f*Mathf.Sin(ang*3+c.y*.6f)+.05f*Mathf.Sin(ang*5-c.y*.9f);
                    b.Vert(c+nrm*rr*bump,nrm,new Vector2(k/(float)sides*Mathf.Max(1,Mathf.Round(rr*6.28f/1.5f)),t*len/3f),Color.white);
                }
            }
            for(int i=0;i<rings;i++)for(int k=0;k<sides;k++){int a0=baseIdx+i*(sides+1)+k,a1=a0+1,b0=a0+sides+1,b1=b0+1;b.Tri(0,a0,b0,a1);b.Tri(0,a1,b0,b1);}
        }
        float forkY=H*s.bare;Vector3 basePt=Vector3.zero;
        // trunk: gentle S curve
        Vector3 lean=new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f)).normalized*s.lean;
        Vector3 topPt=new Vector3(lean.x*.6f,forkY,lean.z*.6f);
        Tube(basePt,Vector3.up,topPt,(Vector3.up*3+lean*.3f).normalized,s.trunkR,s.trunkR*.72f,s.flare,1);
        // root buttresses
        for(int i=0;i<6;i++){float a=i*Mathf.PI/3+Random.value*.4f;Vector3 dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));Tube(dir*s.trunkR*.3f+Vector3.up*1.2f,(dir*.6f+Vector3.up*-.8f).normalized,dir*s.trunkR*2.4f-Vector3.up*.1f,dir,s.trunkR*.42f,s.trunkR*.06f,1,1);}
        void Limb(Vector3 p,Vector3 d,float len,float r,int depth)
        {
            Vector3 end=p+d*len+new Vector3(0,s.droop*-len*.25f,0);Vector3 d2=(d+Vector3.up*(depth==0?.35f:.05f)-Vector3.up*s.droop*.5f).normalized;
            float r1=Mathf.Max(.04f,r*(depth==0?.34f:.4f));Tube(p,d,end,d2,r,r1,1,1);
            if(depth>=3||len<2.2f){tips.Add((end,d2,depth));return;}
            int kids=depth==0?3:2;for(int i=0;i<kids;i++)
            {
                float tt=depth==0?Random.Range(.35f,.95f):Random.Range(.5f,1f);Vector3 q=Vector3.Lerp(p,end,tt);
                Vector3 nd=(d2+Random.insideUnitSphere*.75f);nd.y+=Random.Range(-.1f,.35f);nd.Normalize();Limb(q,nd,len*Random.Range(.5f,.72f),r1*.78f,depth+1);
            }
            tips.Add((end,d2,depth));
            if(depth==0){tips.Add((Vector3.Lerp(p,end,.6f),d2,depth));}
        }
        for(int i=0;i<s.limbs;i++)
        {
            float az=(i+Random.Range(-.25f,.25f))/s.limbs*Mathf.PI*2+Random.value*.6f,ang=Random.Range(s.limbAngle.x,s.limbAngle.y)*Mathf.Deg2Rad;
            Vector3 d=new Vector3(Mathf.Sin(ang)*Mathf.Cos(az),Mathf.Cos(ang),Mathf.Sin(ang)*Mathf.Sin(az)).normalized;
            float len=(H-forkY)*Random.Range(.55f,.8f)*(1+Mathf.Sin(ang)*.35f);if(s.crown>11)len*=1.1f;
            Limb(topPt+d*.15f,d,len,s.trunkR*.6f,0);
        }
        // leader up the middle for tall trees
        if(s.name=="tall"||s.name=="birch"||s.name=="maple")Limb(topPt,Vector3.up,H-forkY,s.trunkR*.6f,0);
        // foliage cards
        Vector3 centre=new Vector3(0,forkY+(H-forkY)*.55f,0);
        foreach(var tp in tips)
        {
            for(int k=0;k<s.cardsPerTip;k++)
            {
                float size=s.card*Random.Range(.8f,1.3f);Vector3 off=Random.insideUnitSphere*size*.45f;Vector3 pos=tp.p+off+Vector3.up*size*.1f;
                bool hang=s.hanging&&k==0;
                Vector3 nrm=((pos-centre).normalized*.75f+Vector3.up*.45f).normalized;float sway=Mathf.Clamp01(Vector3.Distance(pos,topPt)/H);
                float shade=Mathf.Clamp01(.62f+.5f*(pos.y/H)+Random.Range(-.08f,.08f));var col=new Color(shade,shade,shade,sway);
                int sub=hang?2:1;
                Vector3 up,right;
                if(hang){float yaw=Random.value*Mathf.PI;right=new Vector3(Mathf.Cos(yaw),0,Mathf.Sin(yaw))*size*.55f;up=Vector3.up*size*1.1f;Vector3 p0=pos;b.Quad(sub,p0-right-up,p0+right-up,p0+right,p0-right,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),col,Vector3.up*.4f+new Vector3(Mathf.Cos(yaw+1.57f),0,Mathf.Sin(yaw+1.57f)).normalized*.6f);continue;}
                Quaternion q=Quaternion.LookRotation(nrm+Random.insideUnitSphere*.55f,Random.insideUnitSphere.normalized+Vector3.up*.4f);q*=Quaternion.Euler(0,0,Random.Range(0,360));
                right=q*Vector3.right*size*.5f;up=q*Vector3.up*size*.5f;
                b.Quad(sub,pos-right-up,pos+right-up,pos+right+up,pos-right+up,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),col,nrm);
                // back-to-back so sparse alpha looks full from any side
            }
        }
        return b;
    }
    static Vector3 Hermite(Vector3 p0,Vector3 m0,Vector3 p1,Vector3 m1,float t)
    {float t2=t*t,t3=t2*t;return (2*t3-3*t2+1)*p0+(t3-2*t2+t)*m0+(-2*t3+3*t2)*p1+(t3-t2)*m1;}

    static Mesh[,] templates;
    static Material[] bark;static Dictionary<string,Material> leaf=new Dictionary<string,Material>();static Material hangMat;
    static void Prep()
    {
        bark=Specs.Select(s=>V3.Lit("Bark "+s.bark,s.bark,s.bark+"_n",.08f,null,"#d6d0c8")).Distinct().ToArray();
        leaf.Clear();foreach(var n in new[]{"oak","broad","maple_red","maple_orange","gold","crimson","willow","cherry","magenta","white"})leaf[n]=V3.Foliage("Leaf "+n,"leaf_"+n);
        hangMat=V3.Foliage("Leaf wisteria","leaf_wisteria","#ffffff",.4f);
        templates=new Mesh[Specs.Length,Variants];
        for(int s=0;s<Specs.Length;s++)for(int v=0;v<Variants;v++){var mb=Make(Specs[s],v+1);templates[s,v]=V3.SaveMesh(mb.Build("Tree "+Specs[s].name+" "+v),"Tree_"+Specs[s].name+"_"+v);}
    }
    static string Pick(params (string name,float w)[] o){float t=o.Sum(x=>x.w),r=Random.value*t;foreach(var x in o){r-=x.w;if(r<=0)return x.name;}return o[0].name;}

    public static string Build()
    {
        Prep();var group=GameObject.Find("17 • Towering mixed forests and blossom groves").transform;Random.InitState(9001);
        var village=V3Layout.VillageCentre;var roads=V3Layout.Roads();int n=0,removed=0;var list=group.Cast<Transform>().Where(t=>t.name.Contains("•")).ToList();
        foreach(var t in list)
        {
            var p=t.position;bool kill=V3Layout.ZoneOf(p.x,p.z,-16)>=0||V3Layout.ViaductDist(p.x,p.z)<(p.z<62?15:6)||(p.z>-262&&p.z<-232&&p.x>5&&p.x<60);
            if(!kill)foreach(var r in roads){if(r.name=="Village square")continue;foreach(var q in r.pts){if(Mathf.Abs(q.x-p.x)<9&&Mathf.Abs(q.y-p.z)<9&&Vector2.Distance(q,new Vector2(p.x,p.z))<r.half+4.5f){kill=true;break;}}if(kill)break;}
            if(kill){Object.DestroyImmediate(t.gameObject);removed++;continue;}
            // stable height stored in the name ("h48")
            float h;string nm=t.name;int hi=nm.IndexOf(" h");if(hi>0&&float.TryParse(nm.Substring(hi+2),out var parsed)){h=parsed;nm=nm.Substring(0,hi);}else{h=t.localScale.y*10;nm=nm.TrimEnd('‡',' ');}
            Random.InitState(Mathf.RoundToInt(p.x*13+p.z*7));
            string lower=nm.ToLower();int spec;string main;
            if(lower.Contains("ancient")){spec=1;main="oak";h=Mathf.Max(h,62);}
            else if(lower.Contains("blossom")){spec=Random.value<.18f?5:Random.value<.2f?3:2;main=Pick(("cherry",5),("magenta",2.2f),("white",1.4f));if(spec==3)main="maple_orange";if(spec==5)main="broad";}
            else if(lower.Contains("crimson")){spec=Random.value<.6f?3:0;main=Pick(("maple_red",5),("crimson",3),("maple_orange",3));}
            else if(lower.Contains("cathedral")){spec=Random.value<.55f?0:Random.value<.6f?3:4;main=Pick(("oak",4),("broad",3),("gold",2),("maple_orange",2.4f),("maple_red",1.8f));if(spec==4)main="broad";}
            else if(lower.Contains("cherry")){spec=2;main=Pick(("cherry",5),("magenta",2));}
            else if(lower.Contains("wisteria")){spec=5;main="broad";}
            else if(lower.Contains("maple")){spec=3;main=Pick(("maple_red",4),("maple_orange",3));}
            else if(lower.Contains("golden")){spec=1;main="gold";}
            else{spec=Random.value<.5f?0:Random.value<.4f?1:Random.value<.5f?4:3;main=Pick(("oak",4),("broad",3),("gold",1.2f),("maple_orange",1.2f));if(spec==4)main="broad";}
            foreach(Transform c in t.Cast<Transform>().ToArray())Object.DestroyImmediate(c.gameObject);
            var mesh=templates[spec,Random.Range(0,Variants)];var sp=Specs[spec];
            var mats=new[]{Material_Bark(sp),leaf[main],hangMat};
            var go=new GameObject("Tree");go.transform.SetParent(t,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=mats;
            float scale=h/H*Random.Range(.92f,1.08f);t.localScale=new Vector3(scale,scale,scale);t.localRotation=Quaternion.Euler(0,Random.Range(0,360),0);
            var cap=t.GetComponent<CapsuleCollider>();if(!cap)cap=t.gameObject.AddComponent<CapsuleCollider>();cap.radius=sp.trunkR*1.05f;cap.height=14;cap.center=Vector3.up*7;
            t.name=nm+" h"+Mathf.RoundToInt(h);
            var y=V3.H(p.x,p.z);t.position=new Vector3(p.x,y-.15f,p.z);n++;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "trees rebuilt "+n+", removed "+removed;
    }
    static Material Material_Bark(Spec s)=>V3.Lit("Bark "+s.bark,s.bark,s.bark+"_n",.08f,null,"#d6d0c8");
}
