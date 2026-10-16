using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Random=UnityEngine.Random;

public static class V8Grove
{
    static Mesh[] trees=new Mesh[4];static Mesh grassRock;
    public static void Tube(V3.MB b,List<Vector3> p,float r0,float r1,int sub=0,int sides=10)
    {
        int start=b.v.Count;float len=0;
        for(int i=0;i<p.Count;i++)
        {if(i>0)len+=Vector3.Distance(p[i],p[i-1]);float t=i/(float)(p.Count-1);var d=(p[Mathf.Min(i+1,p.Count-1)]-p[Mathf.Max(i-1,0)]).normalized;var right=Vector3.Cross(d,Mathf.Abs(d.y)>.9f?Vector3.forward:Vector3.up).normalized;var up=Vector3.Cross(right,d);
            for(int j=0;j<=sides;j++){float a=j*Mathf.PI*2/sides;var n=right*Mathf.Cos(a)+up*Mathf.Sin(a);float radius=Mathf.Lerp(r0,r1,t)*(1+.14f*Mathf.Sin(a*5+p[i].y));b.Vert(p[i]+n*radius,n,new Vector2(j/(float)sides*2,len*.35f),Color.white);}}
        for(int i=0;i<p.Count-1;i++)for(int j=0;j<sides;j++){int k=start+i*(sides+1)+j;b.Tri(sub,k,k+sides+1,k+1);b.Tri(sub,k+1,k+sides+1,k+sides+2);}
    }
    static List<Vector3> Curve(Vector3 a,Vector3 b,Vector3 c,int n=10)
    {var p=new List<Vector3>();for(int i=0;i<=n;i++){float t=i/(float)n;p.Add(a*(1-t)*(1-t)+b*2*t*(1-t)+c*t*t);}return p;}
    static Mesh MakeTree(int seed)
    {
        Random.InitState(919+seed*37);var mb=new V3.MB(2);var tips=new List<Vector3>();float bend=Random.Range(.5f,1.7f);var fork=new Vector3(bend,5.8f,.4f);
        Tube(mb,Curve(Vector3.zero,new Vector3(-bend*.7f,3,.2f),fork),.77f,.48f);
        for(int i=0;i<9;i++){float a=i*Mathf.PI*2/9+Random.value*.2f;var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));Tube(mb,Curve(Vector3.up*.9f,d*1.6f+Vector3.up*.2f,d*Random.Range(2.7f,4.2f)-Vector3.up*.12f,7),Random.Range(.19f,.32f),.025f);}
        for(int i=0;i<8;i++)
        {
            float a=i*Mathf.PI*2/8+Random.Range(-.3f,.3f);var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));var end=fork+d*Random.Range(5.2f,8.5f)+Vector3.up*Random.Range(5.5f,9.5f);
            var mid=fork+d*2.5f+Vector3.up*Random.Range(3,6);Tube(mb,Curve(fork,mid,end),.34f,.10f);
            for(int j=0;j<4;j++)
            {
                var p=Vector3.Lerp(mid,end,Random.Range(.3f,.9f));var q=p+d*Random.Range(1,3)+Random.insideUnitSphere*2.1f+Vector3.up*Random.Range(1,3);
                Tube(mb,Curve(p,p+Vector3.up*1.5f,q,7),.095f,.022f,0,7);tips.Add(q);tips.Add(Vector3.Lerp(p,q,.5f));
            }
            tips.Add(end);
        }
        foreach(var tip in tips)for(int i=0;i<10;i++)
        {
            var p=tip+Random.insideUnitSphere*1.6f;float size=Random.Range(2.0f,3.7f);var n=i%3==0?Vector3.up:(Random.onUnitSphere+Vector3.up*.3f).normalized;var rot=Quaternion.LookRotation(n,Random.onUnitSphere);var r=rot*Vector3.right*size*.7f;var u=rot*Vector3.up*size*.45f;float shade=Mathf.Lerp(.76f,1.1f,Mathf.Clamp01((p.y-7)/11));mb.Quad(1,p-r-u,p+r-u,p+r+u,p-r+u,Vector2.zero,Vector2.right,Vector2.one,Vector2.up,new Color(shade,shade,shade,.75f),n);
        }
        return V3.SaveMesh(mb.Build("Broad crowned shade tree"),"V8_GreenTree_"+seed);
    }
    public static void Prep(){for(int i=0;i<4;i++)trees[i]=MakeTree(i);}
    public static Transform Tree(Vector3 pos,float height,Transform parent,int variant=0,string name="Broad crowned green shade tree")
    {
        if(!trees[0])Prep();var g=new GameObject(name).transform;g.SetParent(parent,false);g.position=pos;g.rotation=Quaternion.Euler(0,variant*137.5f,0);float scale=height/trees[variant%4].bounds.max.y;g.localScale=Vector3.one*scale;
        var bark=V3.Mat("V8 Warm sculpted bark","Universal Render Pipeline/Lit");bark.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/AnimeWorld/PaintedBark.png"));bark.SetColor("_BaseColor",new Color(1.8f,1.5f,1.1f));bark.SetFloat("_Smoothness",.05f);
        V3.Obj("Root fans branching limbs and green leaf sprays",trees[variant%4],new[]{bark,V8Kit.LeafMat("Sunlit green foliage","GreenBough","#d5e8bc")},g);
        var cap=g.gameObject.AddComponent<CapsuleCollider>();cap.radius=.65f;cap.height=6;cap.center=Vector3.up*3;return g;
    }
    public static string CoastTrees(Transform g)
    {
        Random.InitState(824);int n=0;
        for(int i=0;i<110;i++)
        {float x=Random.Range(257,334),z=Random.Range(117,326);var p=new Vector3(x,V3.H(x,z),z);if(x>V8Landscape.Shore(z)-14||p.y<5)continue;if(Mathf.Abs(V3.H(x+2,z)-V3.H(x-2,z))>2.5f||Mathf.Abs(V3.H(x,z+2)-V3.H(x,z-2))>2.5f)continue;V8Landscape.Near(V8Landscape.Route,p,out float d,out var _);if(d<8||V8Landscape.Plots.Any(q=>Vector2.Distance(new Vector2(x,z),new Vector2(q.p.x,q.p.z))<10))continue;Tree(p,Random.Range(13,24),g,i);n++;}
        return "green coastal trees="+n;
    }
    public static string Headland(Transform parent)
    {
        var g=V3.FreshGroup("39 • Sea guardian tree and rocky headland",parent);Vector3 centre=new Vector3(348,34,349);int sides=30;var rock=new V3.MB();var top=new V3.MB();
        for(int j=0;j<sides;j++)
        {
            float a=j*Mathf.PI*2/sides,b=(j+1)*Mathf.PI*2/sides;
            Vector3 P(float angle,float level,float r){float edge=1+.085f*Mathf.Sin(angle*5)+.035f*Mathf.Sin(angle*13);return new Vector3(centre.x+Mathf.Sin(angle)*24*r*edge,level,centre.z+Mathf.Cos(angle)*15*r*edge);}
            var p0=P(a,34,1);var p1=P(b,34,1);Vector3 q0=P(a,16,1.13f),q1=P(b,16,1.13f),r0=P(a,-5,.72f),r1=P(b,-5,.72f);
            rock.Quad(0,p0,q0,q1,p1,Vector2.zero,Vector2.up,Vector2.one,Vector2.right,Color.white);rock.Quad(0,q0,r0,r1,q1,Vector2.zero,Vector2.up,Vector2.one,Vector2.right,Color.white);
            top.Tri3(0,centre,p0,p1,Vector2.zero,Vector2.one,Vector2.right,Color.Lerp(V3.C("#68a347"),V3.C("#8dba53"),Mathf.Sin(a*4)*.5f+.5f));
        }
        V8Kit.Obj("Angular grey sea cliff",rock,new[]{V8Kit.Surface("Blue grey coastal cliff","#b4c0c6","CliffStone",.10f)},g,true);
        var grass=V3.Mat("V8 headland meadow","Aurelia/Meadow");grass.SetColor("_BaseColor",Color.white);V8Kit.Obj("Grassy guardian headland top",top,new[]{grass},g,true);
        Tree(centre+new Vector3(1,0,0),27,g,2,"The sea guardian • broad rooted green tree");
        var meadow=new V3.MB();for(int i=0;i<470;i++){var p=centre+new Vector3(Random.Range(-22,22),.035f,Random.Range(-13,13));if(Mathf.Pow((p.x-centre.x)/22,2)+Mathf.Pow((p.z-centre.z)/13,2)>.9f||Vector3.Distance(p,centre)<3)continue;V8Kit.FlowerCard(meadow,p,Random.Range(.22f,.52f),i*2.4f);}V8Kit.Obj("Tiny headland daisies and meadow tufts",meadow,new[]{V8Kit.Flowers},g);
        V8Kit.Path("Meandering path to the sea guardian",V8Landscape.Headland,3.2f,g);
        for(int i=0;i<18;i++){float a=i*2.4f;var p=centre+new Vector3(Mathf.Sin(a)*Random.Range(6,18),.04f,Mathf.Cos(a)*Random.Range(5,11));V8Kit.Rock(p,new Vector3(1.3f,.7f,1.0f),g,i);}
        return "sea guardian tree and walkable cliff top built";
    }
}
