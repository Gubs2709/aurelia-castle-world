using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Adds ivory palace wings, gabled halls, corner turrets and teal spires on the castle plateau so the silhouette
// reads as a wide tiered citadel instead of a single narrow keep. Idempotent: recreates its own group.
public static class CastleWingsPass
{
    const string GroupName="25 • Castle wings, halls and turrets",D="Assets/LivingWorld";
    class Batch{public List<Vector3> v=new List<Vector3>();public List<int> t=new List<int>();public List<Vector2> uv=new List<Vector2>();}
    static Batch body,roof,trim,glass,flag;
    static void Quad(Batch b,Vector3 a,Vector3 c,Vector3 d,Vector3 e){int k=b.v.Count;b.v.AddRange(new[]{a,c,d,e});b.uv.AddRange(new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)});b.t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
    static void Tri(Batch b,Vector3 a,Vector3 c,Vector3 d){int k=b.v.Count;b.v.AddRange(new[]{a,c,d});b.uv.AddRange(new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(.5f,1)});b.t.AddRange(new[]{k,k+1,k+2});}
    static Vector3 P(Vector3 origin,float yaw,Vector3 local)=>origin+Quaternion.Euler(0,yaw,0)*local;
    static void Box(Batch b,Vector3 centre,Vector3 size,float yaw)
    {
        var h=size/2;Vector3[] c=new Vector3[8];int n=0;for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)c[n++]=P(centre,yaw,new Vector3(h.x*x,h.y*y,h.z*z));
        // index: x(-1:0-3,+1:4-7) y(-1:+0,+1:+2) z(-1:+0,+1:+1)
        Quad(b,c[0],c[2],c[6],c[4]);Quad(b,c[1],c[5],c[7],c[3]);Quad(b,c[0],c[1],c[3],c[2]);Quad(b,c[4],c[6],c[7],c[5]);Quad(b,c[2],c[3],c[7],c[6]);Quad(b,c[0],c[4],c[5],c[1]);
    }
    static void Cylinder(Batch b,Vector3 baseP,float r,float h,int sides)
    {for(int i=0;i<sides;i++){float a0=i*Mathf.PI*2/sides,a1=(i+1)*Mathf.PI*2/sides;Vector3 p0=baseP+new Vector3(Mathf.Sin(a0)*r,0,Mathf.Cos(a0)*r),p1=baseP+new Vector3(Mathf.Sin(a1)*r,0,Mathf.Cos(a1)*r);Quad(b,p0,p1,p1+Vector3.up*h,p0+Vector3.up*h);}}
    static void Cone(Batch b,Vector3 baseP,float r,float h,int sides,float flare=0)
    {for(int i=0;i<sides;i++){float a0=i*Mathf.PI*2/sides,a1=(i+1)*Mathf.PI*2/sides;Vector3 p0=baseP+new Vector3(Mathf.Sin(a0)*r,0,Mathf.Cos(a0)*r),p1=baseP+new Vector3(Mathf.Sin(a1)*r,0,Mathf.Cos(a1)*r);Vector3 mid0=baseP+new Vector3(Mathf.Sin(a0)*r*(.4f+flare),h*.5f,Mathf.Cos(a0)*r*(.4f+flare)),mid1=baseP+new Vector3(Mathf.Sin(a1)*r*(.4f+flare),h*.5f,Mathf.Cos(a1)*r*(.4f+flare));Vector3 tip=baseP+Vector3.up*h;Quad(b,p0,p1,mid1,mid0);Tri(b,mid0,mid1,tip);}}
    static void Turret(Vector3 baseP,float r,float h,float roofH)
    {
        Cylinder(body,baseP,r,h,14);Cylinder(trim,baseP+Vector3.up*(h-1.2f),r+.35f,.9f,14);
        for(int k=0;k<2;k++)Cylinder(trim,baseP+Vector3.up*(h*(.35f+k*.3f)),r+.12f,.45f,14);
        Cone(roof,baseP+Vector3.up*h,r+.9f,roofH,14,.18f);
        for(int w=0;w<3;w++){float a=w*Mathf.PI*2/3+.4f;for(int f=0;f<Mathf.Max(1,(int)(h/6.5f));f++){var p=baseP+new Vector3(Mathf.Sin(a)*(r+.02f),h*.25f+f*6f,Mathf.Cos(a)*(r+.02f));Box(glass,p,new Vector3(.8f,2.4f,.3f),a*Mathf.Rad2Deg);}}
        var fp=baseP+Vector3.up*(h+roofH);Box(trim,fp+Vector3.up*2,new Vector3(.12f,4f,.12f),0);Quad(flag,fp+Vector3.up*3.8f,fp+Vector3.up*3.8f+new Vector3(2.4f,-.3f,0),fp+Vector3.up*2.5f+new Vector3(2.2f,-.5f,0),fp+Vector3.up*2.5f);
    }
    static void Hall(Vector3 c0,float w,float d,int floors,float yaw,bool gable)
    {
        float fh=5.4f,h=floors*fh;Vector3 c=c0;
        Box(trim,c+Vector3.up*.6f,new Vector3(w+1.2f,1.2f,d+1.2f),yaw);
        Box(body,c+Vector3.up*(h/2+.6f),new Vector3(w,h,d),yaw);
        for(int f=1;f<floors;f++)Box(trim,c+Vector3.up*(f*fh+.6f),new Vector3(w+.35f,.45f,d+.35f),yaw);
        Box(trim,c+Vector3.up*(h+.9f),new Vector3(w+1.3f,.8f,d+1.3f),yaw);
        // windows on all four faces
        for(int face=0;face<4;face++)
        {
            float span=face%2==0?w:d,depth=face%2==0?d:w;int n=Mathf.Max(2,(int)(span/3.3f));float fy=face*90;
            for(int f=0;f<floors;f++)for(int i=0;i<n;i++){float off=(i-(n-1)/2f)*(span/n);var local=Quaternion.Euler(0,fy,0)*new Vector3(off,0,depth/2+.04f);var p=c+Quaternion.Euler(0,yaw,0)*local+Vector3.up*(f*fh+3.1f);
                Box(trim,p,new Vector3(1.6f,3.3f,.18f),yaw+fy);Box(glass,p+Quaternion.Euler(0,yaw+fy,0)*new Vector3(0,0,.07f),new Vector3(1f,2.6f,.15f),yaw+fy);}
        }
        float top=c.y+h+1.3f;float rh=Mathf.Min(w,d)*.62f;bool alongX=w>=d;
        // gabled roof with eaves
        float L=(alongX?w:d)/2+.9f,W=(alongX?d:w)/2+.9f;
        Vector3 a=new Vector3(-L,0,-W),b=new Vector3(L,0,-W),cc=new Vector3(L,0,W),dd=new Vector3(-L,0,W);Vector3 r0=new Vector3(-L,rh,0),r1=new Vector3(L,rh,0);float rot=yaw+(alongX?0:90);var o=new Vector3(c.x,top,c.z);
        Quad(roof,P(o,rot,b),P(o,rot,a),P(o,rot,r0),P(o,rot,r1));Quad(roof,P(o,rot,dd),P(o,rot,cc),P(o,rot,r1),P(o,rot,r0));Tri(trim,P(o,rot,cc),P(o,rot,b),P(o,rot,r1));Tri(trim,P(o,rot,a),P(o,rot,dd),P(o,rot,r0));
        Box(trim,P(o,rot,new Vector3(0,rh+.1f,0)),new Vector3(L*2,.35f,.5f),rot);
        // corner turrets rising above the cornice
        foreach(var sx in new[]{-1,1})foreach(var sz in new[]{-1,1}){var p=c+Quaternion.Euler(0,yaw,0)*new Vector3(sx*w/2,0,sz*d/2);Turret(p,1.9f,h+Random.Range(4f,12f),Random.Range(8f,13f));}
    }
    static Material Mat(string path)=>AssetDatabase.LoadAssetAtPath<Material>(path);
    static void Emit(Transform g,Batch b,string name,Material m)
    {
        if(b.v.Count==0)return;var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(b.v);mesh.SetTriangles(b.t,0);mesh.SetUVs(0,b.uv);mesh.RecalculateNormals();mesh.RecalculateBounds();
        string p=D+"/Meshes/Wings_"+name+".asset";var ex=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(ex){ex.Clear();EditorUtility.CopySerialized(mesh,ex);Object.DestroyImmediate(mesh);mesh=ex;}else AssetDatabase.CreateAsset(mesh,p);
        var o=new GameObject(name);o.transform.SetParent(g,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=m;
    }
    public static string Build()
    {
        var root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;var old=GameObject.Find(GroupName);if(old)Object.DestroyImmediate(old);
        var g=new GameObject(GroupName).transform;g.SetParent(root,false);Random.InitState(2026);
        body=new Batch();roof=new Batch();trim=new Batch();glass=new Batch();flag=new Batch();
        int count=0;
        foreach(float side in new[]{-1f,1f})
        {
            float[] zs={116,132,150,170,192};
            for(int row=0;row<zs.Length;row++)
            {
                int n=row<2?4:3;float x=56+Random.Range(0,6);
                for(int i=0;i<n;i++)
                {
                    float w=Random.Range(11,18),d=Random.Range(10,16);float cx=side*(x+w/2),cz=zs[row]+Random.Range(-3,3);x+=w+Random.Range(2.5f,6);if(x+w>118)break;
                    float y=Mathf.Min(ReferenceCastleBuilder.Height(cx-w/2,cz-d/2),ReferenceCastleBuilder.Height(cx+w/2,cz+d/2),ReferenceCastleBuilder.Height(cx-w/2,cz+d/2),ReferenceCastleBuilder.Height(cx+w/2,cz-d/2))-.5f;
                    int floors=3+Mathf.Clamp((int)((1-(Mathf.Abs(cx)-55)/65f)*3)+row/2,0,3)+(Random.value<.25f?1:0);
                    Hall(new Vector3(cx,y,cz),w,d,floors,Random.Range(-4f,4f),true);count++;
                }
            }
        }
        Emit(g,body,"Wing walls",Mat("Assets/ReferenceCastle/Cool ivory limestone.mat"));
        Emit(g,roof,"Wing roofs",Mat("Assets/ReferenceCastle/Sea glass turquoise slates.mat"));
        Emit(g,trim,"Wing trim",Mat("Assets/ReferenceCastle/Sunlit carved limestone.mat"));
        Emit(g,glass,"Wing windows",Mat("Assets/ReferenceCastle/Recessed blue windows.mat"));
        var fl=Mat("Assets/ReferenceCastle/Azure pennant.mat");Emit(g,flag,"Wing pennants",fl);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "halls "+count+" verts "+(body.v.Count+roof.v.Count+trim.v.Count+glass.v.Count);
    }
}
