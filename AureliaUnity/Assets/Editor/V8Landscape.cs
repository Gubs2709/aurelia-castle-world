using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class V8Landscape
{
    public static float Shore(float z)=>344-30*Mathf.Exp(-Mathf.Pow((z-204)/70,2))+3*Mathf.Sin(z*.04f);
    static List<Vector3> route,headland;
    public static List<Vector3> Route=>route??(route=Sample(new[]{new Vector3(250,62,135),new Vector3(272,55,113),new Vector3(289,47,143),new Vector3(274,39,185),new Vector3(295,32,226),new Vector3(280,25,268),new Vector3(310,18,300),new Vector3(328,11,266),new Vector3(305,6.5f,229),new Vector3(308,3.2f,194)},5));
    public static List<Vector3> Headland=>headland??(headland=Sample(new[]{new Vector3(310,18,300),new Vector3(319,23,317),new Vector3(326,30,337),new Vector3(348,34,349)},4));
    public class Plot{public Vector3 p;public int type;public Vector3 face;}
    static List<Plot> plots;
    public static List<Plot> Plots
    {
        get
        {
            if(plots!=null)return plots;plots=new List<Plot>();int type=0;
            for(int i=5;i<Route.Count-5;i+=5)
            {
                var p=Route[i];var d=(Route[i+1]-Route[i-1]).normalized;var n=new Vector3(-d.z,0,d.x).normalized;
                foreach(float s in new[]{-1f,1f})
                {
                    var q=p+n*s*(11.8f+(type%3)*1.5f);q.y=p.y-.05f;
                    if(q.x<247||q.x>Shore(q.z)-7||q.z<106||q.z>321)continue;
                    Near(Route,q,out float dist,out var near);if(dist<9.7f||Mathf.Abs(near.y-p.y)>4)continue;
                    if(plots.Any(h=>Vector2.Distance(new Vector2(h.p.x,h.p.z),new Vector2(q.x,q.z))<12.5f))continue;
                    plots.Add(new Plot{p=q,type=type++%6,face=new Vector3(p.x-q.x,0,p.z-q.z).normalized});
                }
            }
            foreach(float z in new[]{144f,166,191,218,245,274})
            {var p=new Vector3(Shore(z)-9,3.2f,z);if(plots.All(h=>Vector2.Distance(new Vector2(h.p.x,h.p.z),new Vector2(p.x,p.z))>12))plots.Add(new Plot{p=p,type=type++%6,face=Vector3.right});}
            return plots;
        }
    }
    public static List<Vector3> Sample(Vector3[] p,float step)
    {
        var o=new List<Vector3>();for(int i=0;i<p.Length-1;i++)
        {var a=p[Mathf.Max(0,i-1)];var b=p[i];var c=p[i+1];var d=p[Mathf.Min(i+2,p.Length-1)];int n=Mathf.Max(3,Mathf.CeilToInt(Vector3.Distance(b,c)/step));for(int j=0;j<n;j++){float t=j/(float)n;float t2=t*t,t3=t2*t;o.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t2+(-a+3*b-3*c+d)*t3));}}o.Add(p.Last());return o;
    }
    public static void Near(List<Vector3> line,Vector3 p,out float distance,out Vector3 point)
    {
        distance=1e9f;point=Vector3.zero;
        for(int i=0;i<line.Count-1;i++)
        {var a=line[i];var b=line[i+1];var ab=new Vector2(b.x-a.x,b.z-a.z);var ap=new Vector2(p.x-a.x,p.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/Mathf.Max(.001f,ab.sqrMagnitude));var q=Vector3.Lerp(a,b,t);float ds=(p.x-q.x)*(p.x-q.x)+(p.z-q.z)*(p.z-q.z);if(ds<distance){distance=ds;point=q;}}
        distance=Mathf.Sqrt(distance);
    }
    public static float Height(float x,float z,float old)
    {
        if(x>Shore(z)+18)return -24;
        if(x<244||z<80||z>404)return V7Coast.Height(x,z,old);
        float shore=Shore(z);float f=Mathf.Clamp01((shore-5-x)/(shore-5-249));f=f*f*(3-2*f);float coast=2.85f+62*f+Mathf.PerlinNoise(x*.03f,z*.03f)*1.4f*f;
        if(x>shore-3)coast=Mathf.Lerp(2.85f,-20,Mathf.SmoothStep(0,1,Mathf.InverseLerp(shore-3,shore+11,x)));
        // Natural grassy headland; its vertical seaward rock face is built as a 3D mesh.
        float hd=Mathf.Sqrt(Mathf.Pow((x-340)/36,2)+Mathf.Pow((z-347)/24,2));
        if(hd<1&&x<shore+18)coast=Mathf.Max(coast,34*Mathf.SmoothStep(0,1,Mathf.InverseLerp(1,.64f,hd)));
        var pos=new Vector3(x,0,z);Near(Route,pos,out float d,out var q);
        // A continuous hillside follows the path's elevation before its narrow tread is cut.
        // This avoids excavating isolated shelves out of a mismatched underlying slope.
        if(x<shore-13&&d<38){float sum=0,weight=0;foreach(var p in Route){float r=((p.x-x)*(p.x-x)+(p.z-z)*(p.z-z))*.004f;float w=1/(1+r*r);sum+=p.y*w;weight+=w;}coast=Mathf.Lerp(coast,sum/weight-.6f+(x-q.x)*-.24f,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(13,38,d)));}
        if(d<9)coast=Mathf.Lerp(coast,q.y-.13f,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.1f,9,d)));
        Near(Headland,pos,out d,out q);if(d<7.2f)coast=Mathf.Lerp(coast,q.y-.13f,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(2.0f,7.2f,d)));
        foreach(var plot in Plots)
        {float pd=Vector2.Distance(new Vector2(x,z),new Vector2(plot.p.x,plot.p.z));if(pd<12)coast=Mathf.Lerp(coast,plot.p.y-.06f,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.9f,12,pd)));}
        float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(244,253,x))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(80,108,z))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(404,376,z));
        return Mathf.Lerp(old,coast,blend);
    }
}
