using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Procedural village architecture with real UVs, timber framing, tiled roofs, dormers, chimneys, porches and balconies.
public static class V3House
{
    // structure submeshes
    public const int WALL=0,TIMBER=1,ROOF=2,STONE=3,GLASS=4,ACCENT=5,LIGHT=6,N_STRUCT=7;
    // plant submeshes
    public const int P_IVY=0,P_PINK=1,P_YEL=2,P_BUSH=3,P_LAV=4,N_PLANT=5;
    public class Kind{public string name;public float w,d;public int floors;public float pitch,overhang;public bool frame,porch,balcony,dormers,hip;public int wallType;public float ivy;public bool logs;}
    static readonly Color White=Color.white;

    // ---------- primitives
    static void Face(V3.MB m,int sub,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector2 ua,Vector2 ub,Vector2 uc,Vector2 ud)=>m.Quad(sub,a,b,c,d,ua,ub,uc,ud,White);
    public static void Box(V3.MB m,int sub,Vector3 centre,Vector3 size,Quaternion? rot=null,float tile=2.4f,bool swapUV=false)
    {
        var q=rot??Quaternion.identity;var h=size/2;
        Vector3 P(float x,float y,float z)=>centre+q*new Vector3(x*h.x,y*h.y,z*h.z);
        Vector2 U(float a,float b){var u=new Vector2(a/tile,b/tile);return swapUV?new Vector2(u.y,u.x):u;}
        float cx=centre.x,cy=centre.y,cz=centre.z;
        // +X
        Face(m,sub,P(1,-1,1),P(1,-1,-1),P(1,1,-1),P(1,1,1),U(cz+h.z,cy-h.y),U(cz-h.z,cy-h.y),U(cz-h.z,cy+h.y),U(cz+h.z,cy+h.y));
        Face(m,sub,P(-1,-1,-1),P(-1,-1,1),P(-1,1,1),P(-1,1,-1),U(cz-h.z,cy-h.y),U(cz+h.z,cy-h.y),U(cz+h.z,cy+h.y),U(cz-h.z,cy+h.y));
        Face(m,sub,P(-1,-1,1),P(1,-1,1),P(1,1,1),P(-1,1,1),U(cx-h.x,cy-h.y),U(cx+h.x,cy-h.y),U(cx+h.x,cy+h.y),U(cx-h.x,cy+h.y));
        Face(m,sub,P(1,-1,-1),P(-1,-1,-1),P(-1,1,-1),P(1,1,-1),U(cx+h.x,cy-h.y),U(cx-h.x,cy-h.y),U(cx-h.x,cy+h.y),U(cx+h.x,cy+h.y));
        Face(m,sub,P(-1,1,1),P(1,1,1),P(1,1,-1),P(-1,1,-1),U(cx-h.x,cz+h.z),U(cx+h.x,cz+h.z),U(cx+h.x,cz-h.z),U(cx-h.x,cz-h.z));
        Face(m,sub,P(-1,-1,-1),P(1,-1,-1),P(1,-1,1),P(-1,-1,1),U(cx-h.x,cz-h.z),U(cx+h.x,cz-h.z),U(cx+h.x,cz+h.z),U(cx-h.x,cz+h.z));
    }
    public static void Cyl(V3.MB m,int sub,Vector3 baseC,float r,float height,int sides=14,float tile=2.4f,float topR=-1)
    {
        if(topR<0)topR=r;
        for(int i=0;i<sides;i++)
        {
            float a0=i*Mathf.PI*2/sides,a1=(i+1)*Mathf.PI*2/sides;Vector3 d0=new Vector3(Mathf.Cos(a0),0,Mathf.Sin(a0)),d1=new Vector3(Mathf.Cos(a1),0,Mathf.Sin(a1));
            Vector3 p0=baseC+d0*r,p1=baseC+d1*r,t0=baseC+Vector3.up*height+d0*topR,t1=baseC+Vector3.up*height+d1*topR;
            float u0=a0*r/tile,u1=a1*r/tile;
            m.Quad(sub,p1,p0,t0,t1,new Vector2(u1,0),new Vector2(u0,0),new Vector2(u0,height/tile),new Vector2(u1,height/tile),White,((d0+d1)*.5f).normalized);
            m.Tri3(sub,baseC+Vector3.up*height,t1,t0,Vector2.zero,Vector2.right,Vector2.up,White);
        }
    }

    // ---------- the house
    public static void Build(V3.MB m,V3.MB pl,Kind k,int seed,out Vector2 footprint)
    {
        Random.InitState(seed);float W=k.w,D=k.d,fh=2.75f,plinth=.55f;float Y=plinth+fh*k.floors;footprint=new Vector2(W,D);
        float tan=Mathf.Tan(k.pitch*Mathf.Deg2Rad);float o=k.overhang;float ridgeY=Y+(D/2)*tan;
        // stone plinth + walls
        Box(m,STONE,new Vector3(0,plinth/2,0),new Vector3(W+.18f,plinth,D+.18f),null,1.8f);
        float wallTile=k.logs?1f:2.4f;
        Box(m,WALL,new Vector3(0,plinth+fh*k.floors/2,0),new Vector3(W,fh*k.floors,D),null,wallTile,k.logs);
        // gable infill above eave line
        foreach(int sx in new[]{-1,1})
        {
            float x=sx*(W/2+.005f);Vector3 a=new Vector3(x,Y,sx*(D/2)),b=new Vector3(x,Y,-sx*(D/2)),c=new Vector3(x,ridgeY,0);
            m.Tri3(WALL,a,b,c,new Vector2(a.z/wallTile,a.y/wallTile),new Vector2(b.z/wallTile,b.y/wallTile),new Vector2(c.z/wallTile,c.y/wallTile),White);
            if(k.frame){Box(m,TIMBER,new Vector3(x+sx*.03f,(Y+ridgeY)/2-.1f,0),new Vector3(.1f,.14f,D*.8f),null,1);Box(m,TIMBER,new Vector3(x+sx*.03f,(Y+ridgeY)/2,0),new Vector3(.1f,ridgeY-Y,.14f),null,1);
                float ang=Mathf.Atan2(ridgeY-Y,D/2)*Mathf.Rad2Deg;for(int s2=-1;s2<=1;s2+=2)Box(m,TIMBER,new Vector3(x+sx*.03f,(Y+ridgeY)/2,s2*D*.25f),new Vector3(.1f,.14f,Mathf.Sqrt((ridgeY-Y)*(ridgeY-Y)+D*D/4)*.98f),Quaternion.Euler(s2*ang*sx,0,0),1);}
        }
        // eaves
        float eaveY=Y-o*tan;float o2=o*.8f;
        Vector3 f0=new Vector3(-W/2-o2,eaveY,D/2+o),f1=new Vector3(W/2+o2,eaveY,D/2+o),r1=new Vector3(W/2+o2,ridgeY,0),r0=new Vector3(-W/2-o2,ridgeY,0);
        Vector3 b1=new Vector3(W/2+o2,eaveY,-D/2-o),b0=new Vector3(-W/2-o2,eaveY,-D/2-o);
        float slope=Vector3.Distance(f0,r0),len=W+o2*2;float rt=k.wallType==2?1.5f:1.8f;
        if(k.hip)
        {
            // hipped roof: ridge shortened
            float rl=Mathf.Max(.4f,W-D);Vector3 hr0=new Vector3(-rl/2,ridgeY,0),hr1=new Vector3(rl/2,ridgeY,0);
            m.Quad(ROOF,f0,f1,hr1,hr0,new Vector2(0,0),new Vector2(len/rt,0),new Vector2(rl/rt,slope/rt),new Vector2(0,slope/rt),White);
            m.Quad(ROOF,b1,b0,hr0,hr1,new Vector2(0,0),new Vector2(len/rt,0),new Vector2(rl/rt,slope/rt),new Vector2(0,slope/rt),White);
            m.Tri3(ROOF,f1,b1,hr1,new Vector2(0,0),new Vector2((D+o*2)/rt,0),new Vector2((D+o*2)/rt/2,slope/rt),White);
            m.Tri3(ROOF,b0,f0,hr0,new Vector2(0,0),new Vector2((D+o*2)/rt,0),new Vector2((D+o*2)/rt/2,slope/rt),White);
        }
        else
        {
            m.Quad(ROOF,f0,f1,r1,r0,new Vector2(0,0),new Vector2(len/rt,0),new Vector2(len/rt,slope/rt),new Vector2(0,slope/rt),White);
            m.Quad(ROOF,b1,b0,r0,r1,new Vector2(0,0),new Vector2(len/rt,0),new Vector2(len/rt,slope/rt),new Vector2(0,slope/rt),White);
        }
        // underside + thick ridge cap + fascia
        Vector3 dn=Vector3.down*.1f;
        m.Quad(TIMBER,f1+dn,f0+dn,r0+dn,r1+dn,new Vector2(0,0),new Vector2(len/2,0),new Vector2(len/2,slope/2),new Vector2(0,slope/2),White);
        m.Quad(TIMBER,b0+dn,b1+dn,r1+dn,r0+dn,new Vector2(0,0),new Vector2(len/2,0),new Vector2(len/2,slope/2),new Vector2(0,slope/2),White);
        if(!k.hip){Box(m,TIMBER,new Vector3(0,ridgeY+.05f,0),new Vector3(len,.2f,.34f),null,1);}
        float pitchAng=k.pitch;Box(m,TIMBER,new Vector3(0,eaveY-.06f,D/2+o),new Vector3(len,.16f,.1f),null,1);Box(m,TIMBER,new Vector3(0,eaveY-.06f,-D/2-o),new Vector3(len,.16f,.1f),null,1);
        if(!k.hip)foreach(int sx in new[]{-1,1}){var cen=new Vector3(sx*(W/2+o2),(eaveY+ridgeY)/2,0);float bl=slope;for(int s2=-1;s2<=1;s2+=2)Box(m,TIMBER,new Vector3(cen.x,(eaveY+ridgeY)/2,s2*(D/2+o)/2),new Vector3(.14f,.18f,bl*.99f),Quaternion.Euler(-s2*pitchAng,0,0),1);}
        // timber frame on wall faces
        if(k.frame)
        {
            float beam=.14f;
            for(int fl=0;fl<=k.floors;fl++){float y=plinth+fl*fh;foreach(int sz in new[]{-1,1}){Box(m,TIMBER,new Vector3(0,y,sz*(D/2+.025f)),new Vector3(W+.1f,beam,.1f),null,1);}foreach(int sx in new[]{-1,1})Box(m,TIMBER,new Vector3(sx*(W/2+.025f),y,0),new Vector3(.1f,beam,D+.1f),null,1);}
            int vx=Mathf.Max(2,Mathf.RoundToInt(W/1.6f)),vz=Mathf.Max(2,Mathf.RoundToInt(D/1.6f));
            for(int fl=0;fl<k.floors;fl++){float y=plinth+fl*fh+fh/2;
                for(int i=0;i<=vx;i++){float x=-W/2+W*i/vx;foreach(int sz in new[]{-1,1})Box(m,TIMBER,new Vector3(x,y,sz*(D/2+.025f)),new Vector3(beam,fh,.1f),null,1);}
                for(int i=0;i<=vz;i++){float z=-D/2+D*i/vz;foreach(int sx in new[]{-1,1})Box(m,TIMBER,new Vector3(sx*(W/2+.025f),y,z),new Vector3(.1f,fh,beam),null,1);}
                if(Random.value<.8f){int b=Random.Range(0,vx);float x=-W/2+W*(b+.5f)/vx;float bl=Mathf.Sqrt(fh*fh+(W/vx)*(W/vx))*.98f;float sgn=Random.value<.5f?1:-1;Box(m,TIMBER,new Vector3(x,y,-(D/2+.025f)),new Vector3(beam,bl,.1f),Quaternion.Euler(0,0,sgn*Mathf.Atan2(W/vx,fh)*Mathf.Rad2Deg),1);}
            }
        }
        else
        {
            // corner posts for plain houses
            foreach(int sx in new[]{-1,1})foreach(int sz in new[]{-1,1})Box(m,k.logs?WALL:TIMBER,new Vector3(sx*(W/2),plinth+fh*k.floors/2,sz*(D/2)),new Vector3(.24f,fh*k.floors+.1f,.24f),null,1);
        }
        // door on the +Z face
        float doorX=Random.Range(-W*.18f,W*.18f);if(k.porch)doorX=0;
        Box(m,STONE,new Vector3(doorX,plinth+1.05f,D/2+.05f),new Vector3(1.5f,2.15f,.12f),null,1.5f);
        Box(m,ACCENT,new Vector3(doorX,plinth+1f,D/2+.12f),new Vector3(1.05f,2f,.08f),null,1.2f);
        Box(m,TIMBER,new Vector3(doorX,plinth+1.06f,D/2+.17f),new Vector3(.06f,1.95f,.04f),null,1);Box(m,TIMBER,new Vector3(doorX+.34f,plinth+1.0f,D/2+.2f),new Vector3(.08f,.08f,.06f),null,1);
        for(int i=0;i<3;i++)Box(m,STONE,new Vector3(doorX,plinth-.12f-i*0,D/2+.45f+i*.0f),new Vector3(1.8f,.12f+.0f,.7f),null,1.5f);
        if(k.porch)
        {
            float pw=2.8f,pd=1.9f;float py=plinth+2.45f;Box(m,LIGHT,new Vector3(doorX,plinth*.5f,D/2+pd/2),new Vector3(pw,.12f,pd),null,1.5f);
            foreach(int sx in new[]{-1,1})Box(m,LIGHT,new Vector3(doorX+sx*(pw/2-.1f),(py)/2,D/2+pd-.12f),new Vector3(.16f,py,.16f),null,1);
            Box(m,TIMBER,new Vector3(doorX,py+.05f,D/2+pd/2),new Vector3(pw+.2f,.14f,pd+.1f),null,1);
            // small gable over porch
            float ph=.7f;Vector3 e0=new Vector3(doorX-pw/2-.2f,py,D/2+pd+.1f),e1=new Vector3(doorX+pw/2+.2f,py,D/2+pd+.1f),rr1=new Vector3(doorX+pw/2+.2f,py+ph,D/2),rr0=new Vector3(doorX-pw/2-.2f,py+ph,D/2);
            m.Quad(ROOF,e0,e1,rr1,rr0,new Vector2(0,0),new Vector2(3,0),new Vector2(3,1.3f),new Vector2(0,1.3f),White);
            m.Tri3(ROOF,e1,e0,new Vector3(doorX,py+ph+.0f,D/2+pd+.1f),new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,.5f),White);
        }
        // windows
        void Window(Vector3 pos,int face,float s=1f,bool flowers=false)
        {
            // face: 0 +Z,1 -Z,2 +X,3 -X
            Quaternion fr=face==0?Quaternion.identity:face==1?Quaternion.Euler(0,180,0):face==2?Quaternion.Euler(0,90,0):Quaternion.Euler(0,-90,0);
            Vector3 L(float x,float y,float z)=>pos+fr*new Vector3(x,y,z);
            Box(m,TIMBER,L(0,0,.05f),new Vector3(1.0f*s+.18f,1.3f*s+.18f,.1f),fr,1);
            Box(m,GLASS,L(0,0,.1f),new Vector3(.9f*s,1.2f*s,.04f),fr,1);
            Box(m,TIMBER,L(0,0,.13f),new Vector3(.06f,1.25f*s,.04f),fr,1);Box(m,TIMBER,L(0,0,.13f),new Vector3(.95f*s,.06f,.04f),fr,1);
            Box(m,STONE,L(0,-.7f*s,.1f),new Vector3(1.2f*s,.1f,.28f),fr,1.2f);
            foreach(int sx in new[]{-1,1}){Box(m,ACCENT,L(sx*(.5f*s+.26f),0,.1f),new Vector3(.5f,1.4f*s,.06f),fr,1.2f);Box(m,TIMBER,L(sx*(.5f*s+.26f),.2f*s,.14f),new Vector3(.4f,.04f,.03f),fr,1);}
            if(flowers)
            {
                Box(m,TIMBER,L(0,-.88f*s,.28f),new Vector3(1.15f*s,.24f,.3f),fr,1);
                for(int i=0;i<6;i++){var p=L((i-2.5f)*.17f*s,-.72f*s,.3f);float sz=Random.Range(.35f,.55f);int sub=i%3==0?P_YEL:P_PINK;Card(pl,sub,p+Vector3.up*sz*.45f,sz,sz,face);}
                Card(pl,P_BUSH,L(0,-.75f*s,.3f)+Vector3.up*.18f,.9f,.45f,face);
            }
        }
        List<float> frontXs=new List<float>();int perFloorFront=Mathf.Max(2,Mathf.RoundToInt(W/3.4f));
        for(int fl=0;fl<k.floors;fl++)
        {
            float y=plinth+fl*fh+1.6f;
            for(int i=0;i<perFloorFront;i++){float x=-W/2+W*(i+.5f)/perFloorFront;if(fl==0&&Mathf.Abs(x-doorX)<1.3f)continue;Window(new Vector3(x,y,D/2),0,1f,fl==0&&Random.value<.7f||fl==1&&k.balcony&&false);}
            int pz=Mathf.Max(1,Mathf.RoundToInt(D/3.6f));for(int i=0;i<pz;i++){float z=-D/2+D*(i+.5f)/pz;Window(new Vector3(W/2,y,z),2,.9f,Random.value<.35f);Window(new Vector3(-W/2,y,z),3,.9f,Random.value<.35f);}
            for(int i=0;i<perFloorFront;i++){float x=-W/2+W*(i+.5f)/perFloorFront;Window(new Vector3(x,y,-D/2),1,.9f,false);}
        }
        // attic window in front gable if any
        // balcony
        if(k.balcony&&k.floors>=2)
        {
            float by=plinth+fh,bw=Mathf.Min(W*.7f,3.6f),bd=1.3f;Box(m,LIGHT,new Vector3(doorX*0,by,D/2+bd/2),new Vector3(bw,.14f,bd),null,1.6f);
            for(int i=0;i<=8;i++){float x=-bw/2+bw*i/8;Box(m,LIGHT,new Vector3(x,by+.55f,D/2+bd-.05f),new Vector3(.07f,1f,.07f),null,1);}
            for(int sx=-1;sx<=1;sx+=2)for(int i=0;i<=3;i++)Box(m,LIGHT,new Vector3(sx*(bw/2-.04f),by+.55f,D/2+.05f+bd*i/3),new Vector3(.07f,1f,.07f),null,1);
            Box(m,LIGHT,new Vector3(0,by+1.07f,D/2+bd-.05f),new Vector3(bw+.1f,.1f,.12f),null,1);foreach(int sx in new[]{-1,1})Box(m,LIGHT,new Vector3(sx*(bw/2-.04f),by+1.07f,D/2+bd/2),new Vector3(.1f,.1f,bd),null,1);
            for(int i=0;i<7;i++)Card(pl,i%2==0?P_PINK:P_YEL,new Vector3(-bw/2+.3f+i*(bw-.6f)/6,by+.55f,D/2+bd-.1f),.5f,.55f,0);
            Box(m,ACCENT,new Vector3(0,by+1.2f,D/2+.05f),new Vector3(.95f,1.9f,.07f),null,1);
        }
        // chimney
        float cx=(Random.value<.5f?-1:1)*(W/2-.9f);float chTop=ridgeY+.9f;Box(m,STONE,new Vector3(cx,(Y+chTop)/2-.2f,0),new Vector3(.8f,chTop-Y+.4f,.8f),null,1.2f);
        Box(m,STONE,new Vector3(cx,chTop+.12f,0),new Vector3(1f,.2f,1f),null,1.2f);Box(m,TIMBER,new Vector3(cx,chTop+.34f,0),new Vector3(.5f,.24f,.5f),null,1);
        // dormers (front slope)
        if(k.dormers&&W>6)
        {
            int n=W>8?2:1;for(int i=0;i<n;i++)
            {
                float x=n==1?0:(i==0?-W*.22f:W*.22f);float zf=D*.22f;float yb=ridgeY-zf*tan;float dw=1.5f,dh=1.5f,dd=1.5f;
                Box(m,WALL,new Vector3(x,yb+dh/2,zf),new Vector3(dw,dh,.14f),null,2.4f);Box(m,TIMBER,new Vector3(x,yb+.5f,zf+.1f),new Vector3(dw+.2f,dh*.75f,.08f),null,1);
                Box(m,GLASS,new Vector3(x,yb+.62f,zf+.16f),new Vector3(.8f,.85f,.04f),null,1);Box(m,TIMBER,new Vector3(x,yb+.62f,zf+.2f),new Vector3(.05f,.9f,.03f),null,1);
                Box(m,ACCENT,new Vector3(x-.58f,yb+.62f,zf+.18f),new Vector3(.3f,.95f,.05f),null,1);Box(m,ACCENT,new Vector3(x+.58f,yb+.62f,zf+.18f),new Vector3(.3f,.95f,.05f),null,1);
                float rh=.8f;Vector3 e0=new Vector3(x-dw/2-.2f,yb+dh,zf+.2f),e1=new Vector3(x+dw/2+.2f,yb+dh,zf+.2f),ee1=new Vector3(x+dw/2+.2f,yb+dh,zf-dd),ee0=new Vector3(x-dw/2-.2f,yb+dh,zf-dd),mid0=new Vector3(x,yb+dh+rh,zf+.2f),mid1=new Vector3(x,yb+dh+rh,zf-dd);
                m.Quad(ROOF,e0,mid0,mid1,ee0,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1.4f),new Vector2(0,1.4f),White);m.Quad(ROOF,mid0,e1,ee1,mid1,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1.4f),new Vector2(0,1.4f),White);
                m.Tri3(WALL,e0,e1,mid0,new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,.4f),White);
                Box(m,WALL,new Vector3(x,yb+dh/2-.2f,zf-dd/2),new Vector3(dw-.1f,dh,dd),null,2.4f);
            }
        }
        // ivy and shrubs
        if(k.ivy>0)
        {
            int cards=Mathf.RoundToInt(k.ivy*18);for(int i=0;i<cards;i++)
            {
                int face=Random.Range(0,4);float y=Random.Range(.6f,Y-.2f);float span=(face<2?W:D)*.5f;float u=Random.Range(-span*.9f,span*.9f);float off=.07f;
                Vector3 p=face==0?new Vector3(u,y,D/2+off):face==1?new Vector3(u,y,-D/2-off):face==2?new Vector3(W/2+off,y,u):new Vector3(-W/2-off,y,u);Card(pl,P_IVY,p,Random.Range(1.2f,2.1f),Random.Range(1.2f,2.1f),face);
            }
        }
        for(int i=0;i<Mathf.RoundToInt((W+D)*1.4f);i++){int face=Random.Range(0,4);float span=(face<2?W:D)*.5f;float u=Random.Range(-span,span);float off=.45f;Vector3 p=face==0?new Vector3(u,.1f,D/2+off):face==1?new Vector3(u,.1f,-D/2-off):face==2?new Vector3(W/2+off,.1f,u):new Vector3(-W/2-off,.1f,u);
            if(face==0&&Mathf.Abs(u-doorX)<1.4f)continue;int sub=Random.value<.5f?P_BUSH:Random.value<.5f?P_PINK:P_LAV;Card(pl,sub,p,Random.Range(.9f,1.5f),Random.Range(.7f,1.3f),face);Card(pl,P_BUSH,p+new Vector3(Random.Range(-.3f,.3f),0,Random.Range(-.3f,.3f)),Random.Range(.8f,1.2f),Random.Range(.6f,1f),face);}
    }
    // a vertical sprite quad (cross of two so it holds from any side)
    static void Card(V3.MB pl,int sub,Vector3 baseP,float w,float h,int face)
    {
        for(int i=0;i<2;i++)
        {
            float yaw=(face*90+i*70+Random.Range(-15,15))*Mathf.Deg2Rad;Vector3 r=new Vector3(Mathf.Cos(yaw),0,-Mathf.Sin(yaw))*w*.5f;
            pl.Quad(sub,baseP-r,baseP+r,baseP+r+Vector3.up*h,baseP-r+Vector3.up*h,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),new Color(Random.Range(.88f,1.05f),Random.Range(.9f,1.05f),Random.Range(.88f,1.05f),.15f),(Vector3.up*.6f+Vector3.Cross(r,Vector3.up).normalized*.4f).normalized);
        }
    }
}
