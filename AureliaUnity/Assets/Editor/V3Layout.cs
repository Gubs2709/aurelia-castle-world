using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Single source of truth for roads, village footprint and gates, shared by ground splat, village and bridge passes.
public static class V3Layout
{
    public class Road{public string name;public List<Vector2> pts=new List<Vector2>();public float half;public bool flag;public string district;public bool rows;public string[] models;public float gap=2.5f;public float setback=1.6f;public bool noHouses;public int onlySide;}
    // City zones are levelled so houses sit on flat ground. (cx,cz,rx,rz,planeY)
    public static readonly float[][] Zones={new[]{-112f,-42f,104f,92f,4.2f},new[]{140f,-62f,60f,80f,5.0f}};
    public static float CityLevel(float x,float z,float y)
    {
        foreach(var q in Zones){float dx=(x-q[0])/q[2],dz=(z-q[1])/q[3];float d=Mathf.Sqrt(dx*dx+dz*dz);if(d<1){float t=Mathf.Clamp01((1-d)/.28f);t=t*t*(3-2*t);y=Mathf.Lerp(y,q[4],t);}}
        return y;
    }
    // Rect districts on the extended plateau: 2 west market, 3 harbour, 4 back town. (x0,x1,z0,z1)
    public static readonly float[][] Rects={new[]{-246f,-136f,100f,232f},new[]{136f,262f,100f,232f},new[]{-98f,98f,252f,312f}};
    public static readonly Vector3 LakeC=new Vector3(194,166,0);public static readonly Vector2 LakeR=new Vector2(32,28);
    public const float LakeY=61.4f,PlateauY=65f,Ring=124f;
    public static bool InLake(float x,float z,float margin=0){float dx=(x-LakeC.x)/(LakeR.x+margin),dz=(z-LakeC.y)/(LakeR.y+margin);return dx*dx+dz*dz<1;}
    public static float LakeCarve(float x,float z,float y)
    {
        float dx=(x-LakeC.x)/(LakeR.x+9),dz=(z-LakeC.y)/(LakeR.y+8);float d=Mathf.Sqrt(dx*dx+dz*dz);
        if(d<1){float t=Mathf.Clamp01((1-d)/.45f);t=t*t*(3-2*t);y=Mathf.Lerp(y,LakeY-6.5f,t);}
        return y;
    }
    public static int ZoneOf(float x,float z,float margin=0)
    {
        for(int i=0;i<Zones.Length;i++){var q=Zones[i];float dx=(x-q[0])/(q[2]-margin),dz=(z-q[1])/(q[3]-margin);if(dx*dx+dz*dz<1)return i;}
        for(int i=0;i<Rects.Length;i++){var r=Rects[i];if(x>r[0]+margin&&x<r[1]-margin&&z>r[2]+margin&&z<r[3]-margin)return 2+i;}
        return -1;
    }
    public static readonly Vector2 VillageCentre=new Vector2(-100,-18);
    public static readonly Vector2 ViaductRampTop=new Vector2(1,19.5f);
    public static float ViaductDist(float x,float z){Vector2 a=new Vector2(50,-245),b=new Vector2(0,64),p=new Vector2(x,z);Vector2 ab=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);return Vector2.Distance(p,a+ab*t);}
    static List<Vector2> P(params float[] xz){var l=new List<Vector2>();for(int i=0;i<xz.Length;i+=2)l.Add(new Vector2(xz[i],xz[i+1]));return l;}
    static List<Vector2> Sample(List<Vector2> pts,float step)
    {
        // Catmull-Rom resample so roads curve smoothly
        var o=new List<Vector2>();for(int i=0;i<pts.Count-1;i++)
        {
            Vector2 p0=pts[Mathf.Max(0,i-1)],p1=pts[i],p2=pts[i+1],p3=pts[Mathf.Min(pts.Count-1,i+2)];float len=Vector2.Distance(p1,p2);int n=Mathf.Max(2,(int)(len/step));
            for(int k=0;k<n;k++){float t=k/(float)n,t2=t*t,t3=t2*t;o.Add(.5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t2+(-p0+3*p1-3*p2+p3)*t3));}
        }
        o.Add(pts[pts.Count-1]);return o;
    }
    static float[] CirclePts(float cx,float cz,float r,int n){var l=new List<float>();for(int i=0;i<=n;i++){float a=i*Mathf.PI*2/n;l.Add(cx+Mathf.Cos(a)*r);l.Add(cz+Mathf.Sin(a)*r);}return l.ToArray();}
    static float[] EllipsePts(float cx,float cz,float rx,float rz,int n){var l=new List<float>();for(int i=0;i<=n;i++){float a=i*Mathf.PI*2/n;l.Add(cx+Mathf.Cos(a)*rx);l.Add(cz+Mathf.Sin(a)*rz);}return l.ToArray();}
    static List<Road> cache;
    public static List<Road> Roads(){return cache??(cache=MakeRoads());}
    public static void Reset(){cache=null;}
    static Road R(string name,float half,bool flag,string district,string[] models,float gap,float setback,bool rows,params float[] xz)
    {var r=new Road{name=name,half=half,flag=flag,district=district,models=models,gap=gap,setback=setback,rows=rows};r.pts=Sample(P(xz),1.2f);return r;}
    static List<Road> MakeRoads()
    {
        var r=new List<Road>();
        string[] rowsM={"House_RowA","House_RowB","House_RowC","House_RowD","House_ShopRed","House_ShopGreen","House_ShopBlue","House_TudorTall"};
        string[] mixed={"House_TudorTall","House_TudorBlue","House_ShopRed","House_ShopGreen","House_ShopBlue","House_Cottage","House_RowA","House_RowC"};
        string[] cozy={"House_Cottage","House_CottageSmall","House_Farm","House_TudorBlue","House_CottageSmall"};
        string[] harbor={"Harbor_A","Harbor_B","Harbor_C","Harbor_D","Harbor_E","Harbor_F","Harbor_G","Harbor_H","Harbor_I"};
        string[] tav={"House_Tavern","House_TudorTall","House_ShopRed","House_ShopBlue","House_TudorBlue"};
        // forest trail from the overlook down to the north gate of the city
        var a=new Road{name="Forest trail",half=2.1f,noHouses=true};var pts=P(-20,-262,-34,-244,-62,-226,-90,-214);
        for(float z=-205;z<=-134;z+=22)pts.Add(new Vector2(V3.PathX(z)+(z>-150?(z+150)*-.15f:0),z));pts.Add(new Vector2(-104,-122));
        a.pts=Sample(pts,1.2f);r.Add(a);
        // ----- Briarwood: the great market city
        r.Add(R("Great Avenue",3.9f,true,"A",rowsM,.0f,1.2f,true,-104,-122,-104,-90,-106,-70,-104,-50,-103,-28,-101,0,-98,18,-92,30));
        r.Last().noHouses=false;
        r.Add(R("Market Cross",3.5f,true,"A",mixed,.5f,1.3f,true,-196,-46,-165,-47,-134,-44,-110,-45,-96,-45,-70,-47,-46,-44,-30,-48));
        r.Add(R("Artisan Lane",2.7f,true,"A",mixed,1.2f,1.4f,false,-186,-90,-158,-86,-132,-92,-106,-90,-82,-86,-58,-94,-40,-100));
        r.Add(R("Tavern Row",2.7f,true,"A",tav,1.5f,1.4f,false,-184,-14,-152,-10,-126,-16,-103,-14,-80,-10,-56,-14,-38,-22));
        r.Add(R("Garden Lane",2.0f,false,"A",cozy,5.5f,2.0f,false,-186,22,-164,16,-142,24,-120,14,-104,8));
        r.Add(R("West Hill Lane",2.3f,false,"A",cozy,4.5f,2.0f,false,-170,-108,-180,-80,-174,-48,-178,-14,-170,12));
        r.Add(R("East Lane",2.6f,true,"A",mixed,1.0f,1.4f,true,-56,-112,-62,-86,-56,-48,-63,-14,-58,14));
        r.Add(R("Castle Road",3.5f,true,"A",new string[0],0,0,false,-92,30,-70,26,-50,20,-30,17,-10,18,0,19.5f));
        r.Last().noHouses=true;
        var sq=new Road{name="Market Square",half=14f,flag=true,district="A",noHouses=true};sq.pts=Sample(P(-104,-45,-103.5f,-44),1f);r.Add(sq);
        // ----- Riverside terrace: a looser, farm-and-cottage quarter across the river
        r.Add(R("Riverside Road",2.8f,true,"B",cozy,3.5f,1.8f,false,64,-136,84,-122,108,-102,126,-84,138,-62,140,-36,130,-10,112,8,88,20));
        var loop=new List<float>();for(int i=0;i<=12;i++){float ang=i*Mathf.PI*2/12;loop.Add(150+Mathf.Cos(ang)*28);loop.Add(-62+Mathf.Sin(ang)*34);}
        r.Add(R("Orchard Loop",2.1f,false,"B",cozy,6f,2.0f,false,loop.ToArray()));
        r.Add(R("Mill Track",1.9f,false,"B",new string[0],0,0,false,140,-30,160,-12,172,6));
        r.Last().noHouses=true;
        r.Add(R("East Link",2.2f,false,"B",new string[0],0,0,false,138,-100,150,-124,164,-140));
        r.Last().noHouses=true;
        // ----- West Market (district D): bazaar plaza, ring lane and craft streets
        r.Add(R("West Gate Road",3.6f,true,"D",mixed,.8f,1.4f,true,-124,166,-146,166,-164,166));
        r.Add(R("Bazaar Ring",2.8f,true,"D",mixed,1.2f,1.4f,true,CirclePts(-180,166,27,12)));
        r.Last().onlySide=-1;
        var bp=new Road{name="Bazaar Plaza",half=14f,flag=true,district="D",noHouses=true};bp.pts=Sample(P(-180,166,-179.5f,166.5f),1f);r.Add(bp);
        r.Add(R("Bazaar North",2.6f,true,"D",rowsM,.4f,1.4f,true,-180,140,-178,122,-166,106));
        r.Add(R("Bazaar South",2.6f,true,"D",rowsM,.4f,1.4f,true,-180,192,-178,210,-166,226));
        r.Add(R("Weavers Lane",2.2f,true,"D",cozy,2.5f,1.6f,false,-206,166,-224,150,-228,124));
        r.Add(R("Dyers Lane",2.2f,true,"D",cozy,2.5f,1.6f,false,-206,166,-224,184,-228,212));
        // ----- Harbour (district E): shore road round the lake, docks, fishermen's colourful houses
        r.Add(R("East Gate Road",3.6f,true,"E",new string[0],0,0,false,124,166,142,166,154,166));
        r.Last().noHouses=true;
        r.Add(R("Shore Road",3.0f,true,"E",harbor,1.0f,1.6f,true,EllipsePts(LakeC.x,LakeC.y,LakeR.x+11,LakeR.y+10,16)));
        r.Last().onlySide=-1;
        r.Add(R("Fish Market Road",2.6f,true,"E",harbor,1.2f,1.6f,true,158,166,156,140,162,114));
        r.Add(R("Net Lane",2.4f,true,"E",harbor,1.2f,1.6f,true,158,166,156,192,162,218));
        r.Add(R("Lighthouse Spur",2.0f,false,"E",new string[0],0,0,false,226,166,244,160,254,146));
        r.Last().noHouses=true;
        r.Add(R("Market Row North",2.4f,true,"D",mixed,1.5f,1.4f,true,-138,128,-190,124,-238,130));
        r.Add(R("Market Row South",2.4f,true,"D",mixed,1.5f,1.4f,true,-138,204,-190,210,-238,206));
        r.Add(R("Palace Walk West",3.4f,true,"D",new string[0],0,0,false,-124,166,-110,166,-96,166));r.Last().noHouses=true;
        r.Add(R("Palace Walk East",3.4f,true,"E",new string[0],0,0,false,124,166,110,166,96,166));r.Last().noHouses=true;
        r.Add(R("Palace Walk North",3.4f,true,"F",new string[0],0,0,false,0,240,0,230,0,222));r.Last().noHouses=true;
        r.Add(R("Hill Road",2.6f,true,"E",harbor,1.2f,1.6f,true,EllipsePts(LakeC.x,LakeC.y,LakeR.x+34,LakeR.y+31,22)));
        r.Last().onlySide=-1;
        // ----- Back town (district F) north of the palace
        r.Add(R("North Gate Road",3.8f,true,"F",mixed,.8f,1.4f,true,0,240,0,262,0,288));
        r.Add(R("Back Cross",3.2f,true,"F",mixed,1.0f,1.4f,true,-98,272,-50,270,0,272,50,270,98,272));
        r.Add(R("Back Lane West",2.4f,true,"F",cozy,2.0f,1.6f,false,-70,254,-72,280,-66,306));
        r.Add(R("Back Lane East",2.4f,true,"F",cozy,2.0f,1.6f,false,70,254,72,280,66,306));
        var fp=new Road{name="Back Plaza",half=12f,flag=true,district="F",noHouses=true};fp.pts=Sample(P(0,272,.5f,272),1f);r.Add(fp);
        // overlook approach to the viaduct
        var b=new Road{name="Viaduct approach",half=3.4f,flag=true,noHouses=true};b.pts=Sample(P(-20,-262,0,-256,22,-251,40,-248),1.2f);r.Add(b);
        // farm tracks to the outer homesteads
        r.Add(R("Far West Track",1.6f,false,"C",new string[0],0,0,false,-196,-46,-230,-70,-262,-110));
        r.Last().noHouses=true;
        r.Add(R("Far East Track",1.6f,false,"C",new string[0],0,0,false,172,6,208,30,236,48));
        r.Last().noHouses=true;
        return r;
    }
}
