using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

// Rebuilds the castle escarpment: a displaced, terraced, moss-capped cliff with real flowing cascades,
// plunge pools and cliff greenery. Idempotent: it deletes and recreates its own group each run.
public static class EscarpmentPass
{
    const string D="Assets/LivingWorld",GroupName="24 • Sculpted escarpment, terraces and flowing cascades";
    static readonly float[] FallX={-108,-86,-62,-38,38,62,86,108};
    static readonly float[] FallW={9,14,8,12,12,8,14,9};
    static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
    static float Fbm(float x,float y){float s=0,a=.5f;for(int i=0;i<4;i++){s+=a*Mathf.PerlinNoise(x+17.3f*i+100,y-9.1f*i+100);x*=2.03f;y*=2.03f;a*=.5f;}return s/.9375f;}
    static float SS(float a,float b,float t){t=Mathf.Clamp01((t-a)/(b-a));return t*t*(3-2*t);}
    static float TopY(float x)=>ReferenceCastleBuilder.Height(x,100);
    const float BaseY=-2.5f;
    static float FaceZ(float x,float y)
    {
        float lo=60,hi=140;if(ReferenceCastleBuilder.Height(x,60)>=y)return 84;
        for(int i=0;i<22;i++){float m=(lo+hi)/2;if(ReferenceCastleBuilder.Height(x,m)<y)lo=m;else hi=m;}return (lo+hi)/2;
    }
    // Wall surface point for column x and height fraction u.
    static Vector3 Wall(float x,float u,float[] fx=null)
    {
        float top=TopY(x),y=Mathf.Lerp(BaseY,top,u);float z=FaceZ(x,y);
        float edge=Mathf.Clamp01((258-Mathf.Abs(x))/22);
        float rough=Fbm(x*.045f,u*3.1f),fine=Fbm(x*.19f,u*9f);
        float cap=1-Mathf.Pow(u,3f);
        float bulge=cap*(2.2f+6.5f*rough)*edge;
        float s=u*9+Fbm(x*.03f,3.3f)*4;float ledge=SS(.58f,.94f,s-Mathf.Floor(s))*2.4f*cap;
        float bite=Mathf.Pow(fine,2)*2.2f*cap;
        float zz=z-bulge-ledge+bite*.0f-(fine-.5f)*1.1f;
        for(int i=0;i<FallX.Length;i++){float d=(x-FallX[i])/(FallW[i]*.75f);zz+=Mathf.Exp(-d*d)*4.2f*Mathf.Lerp(1,.4f,u);}
        return new Vector3(x,y,zz);
    }
    static Transform MakeGroup(Transform root)
    {var old=GameObject.Find(GroupName);if(old)Object.DestroyImmediate(old);var g=new GameObject(GroupName).transform;g.SetParent(root,false);return g;}
    static Mesh SaveMesh(Mesh m,string name){string p=D+"/Meshes/Escarp_"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(p);if(existing){existing.Clear();EditorUtility.CopySerialized(m,existing);Object.DestroyImmediate(m);return existing;}AssetDatabase.CreateAsset(m,p);return m;}
    static Material Mat(string name,string shader){string p=D+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,p);}m.shader=Shader.Find(shader);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;}
    static GameObject Obj(string name,Mesh mesh,Material mat,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;}

    public static string Build()
    {
        var root=GameObject.Find("THE CITADEL OF FALLING WATER • reference composition").transform;Random.InitState(4411);
        Directory.CreateDirectory(D+"/Meshes");
        // clear the flat slab dressing this replaces
        int removed=0;foreach(var tr in root.GetComponentsInChildren<Transform>(true).ToArray()){if(!tr)continue;string n=tr.name;bool fall=n=="Broad falling water curtain"||n=="Turquoise plunge pool"||n=="Cascade spray and whitewater"||n=="Feeder stream over cliff lip"||n=="Surging waterfall impact foam"||n=="Stratified cliff outcrop";bool buttress=(n=="Eroded cliff buttress"||n=="Hanging cliff foliage")&&tr.position.z>55&&tr.position.z<125&&Mathf.Abs(tr.position.x)<140;if(fall||buttress){Object.DestroyImmediate(tr.gameObject);removed++;}}
        var greyRock=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Stratified grey cliff.mat");if(greyRock){greyRock.SetColor("_BaseColor",C("#a08c78"));greyRock.color=C("#a08c78");EditorUtility.SetDirty(greyRock);}
        var g=MakeGroup(root);
        var rock=Mat("Escarpment rock","Aurelia/Surface Detail");rock.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Realism/Rock.png"));rock.SetColor("_BaseColor",C("#b79d82"));rock.SetFloat("_Scale",.085f);rock.SetFloat("_Relief",.22f);rock.SetFloat("_Smoothness",.12f);rock.SetFloat("_Terrain",1);rock.SetFloat("_Cull",0);
        // ---- the cliff
        foreach(float side in new[]{-1f,1f})
        {
            var v=new List<Vector3>();var t=new List<int>();var colors=new List<Color>();int cols=0;const int rows=70;const float step=1.1f;
            for(float ax=14;ax<=262;ax+=step){float x=side*ax;for(int r=0;r<=rows;r++){float u=r/(float)rows;v.Add(Wall(x,u));float m=Fbm(x*.08f+3,u*5);float moss=SS(.6f,.78f,Fbm(x*.21f,u*2.4f))*.85f*Mathf.Lerp(1,.6f,u)+SS(.9f,1f,u)*.55f;var mc=Color.Lerp(C("#4f8a31"),C("#9cc455"),m);mc.a=1-Mathf.Clamp01(moss);colors.Add(mc);}cols++;}
            for(int c=0;c<cols-1;c++)for(int r=0;r<rows;r++){int a=c*(rows+1)+r,b=(c+1)*(rows+1)+r;if(side>0)t.AddRange(new[]{a,a+1,b,b,a+1,b+1});else t.AddRange(new[]{a,b,a+1,b,b+1,a+1});}
            var mesh=new Mesh{name="Escarpment "+side,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=SaveMesh(mesh,side<0?"West":"East");
            Obj("Terraced mossy escarpment "+(side<0?"west":"east"),mesh,rock,g);
        }
        // ---- cascades, pools, feeders
        var river=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceCastle/Turquoise river.mat");
        var foamMat=AssetDatabase.LoadAssetAtPath<Material>(D+"/Bright impact whitewater.mat");
        for(int f=0;f<FallX.Length;f++)
        {
            float xf=FallX[f],w=FallW[f];float top=TopY(xf);float poolY=1.7f;float u0=(poolY-BaseY)/(top-BaseY);
            var mat=Mat("Cascade "+f,"Aurelia/Cascade");mat.SetColor("_BaseColor",C("#62bfcc"));mat.SetFloat("_Width",w);mat.SetFloat("_Height",top-poolY);mat.SetFloat("_Seed",f*7.3f);
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();int cs=14,rs=64;
            for(int r=0;r<=rs;r++){float u=Mathf.Lerp(u0,1,r/(float)rs);float spread=1+.22f*Mathf.Pow(1-r/(float)rs,2);for(int c=0;c<=cs;c++){float cx=(c/(float)cs-.5f)*w*spread;var p=Wall(xf+cx,u);p.z-=.55f+Mathf.Pow(1-r/(float)rs,3)*1.8f;v.Add(p);uv.Add(new Vector2(c/(float)cs,r/(float)rs));}}
            for(int r=0;r<rs;r++)for(int c=0;c<cs;c++){int a=r*(cs+1)+c,b=a+cs+1;t.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            var mesh=new Mesh{name="Cascade "+f,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=SaveMesh(mesh,"Cascade"+f);
            Obj("Flowing cascade "+f,mesh,mat,g);
            var basePoint=Wall(xf,u0);
            // plunge pool, flowing outward from the foot of the fall
            var pv=new List<Vector3>();var pt=new List<int>();var puv=new List<Vector2>();int seg=28;float pz=basePoint.z-5;
            pv.Add(new Vector3(xf,poolY,pz));puv.Add(new Vector2(.5f,0));for(int k=0;k<=seg;k++){float a=k*Mathf.PI*2/seg;pv.Add(new Vector3(xf+Mathf.Cos(a)*w*.95f,poolY,pz+Mathf.Sin(a)*11-2));puv.Add(new Vector2(.5f+Mathf.Cos(a)*w*.95f/54,(pz+Mathf.Sin(a)*11-2-60)/15+20));}
            for(int k=0;k<seg;k++)pt.AddRange(new[]{0,k+2,k+1});
            var pm=new Mesh{name="Pool "+f,indexFormat=IndexFormat.UInt32};pm.SetVertices(pv);pm.SetTriangles(pt,0);pm.SetUVs(0,puv);pm.RecalculateNormals();pm.RecalculateBounds();pm=SaveMesh(pm,"Pool"+f);
            Obj("Plunge pool "+f,pm,river,g);
            if(foamMat){var q=new List<Vector3>();var qt=new List<int>();var quv=new List<Vector2>{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};float hw=w*.9f+3;q.AddRange(new[]{new Vector3(-hw,0,-9),new Vector3(-hw,0,5),new Vector3(hw,0,5),new Vector3(hw,0,-9)});qt.AddRange(new[]{0,1,2,0,2,3});var fm=new Mesh{name="Foam "+f};fm.SetVertices(q);fm.SetTriangles(qt,0);fm.SetUVs(0,quv);fm.RecalculateNormals();fm=SaveMesh(fm,"Foam"+f);var fo=Obj("Churning whitewater "+f,fm,foamMat,g);fo.transform.position=new Vector3(xf,poolY+.12f,pz+1);}
        }
        Greenery(g);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "escarpment built, removed "+removed;
    }

    // Shrubs, wild trees and hanging foliage clustered on ledges and the cliff crown, one combined mesh per species.
    static void Greenery(Transform g)
    {
        string[] names={"Summer oak","Pink cherry blossom","Crimson maple","Lavender wisteria","Golden woodland"};
        var batches=new List<CombineInstance>[5];for(int i=0;i<5;i++)batches[i]=new List<CombineInstance>();
        var canopy=new Mesh[5];for(int k=0;k<5;k++)canopy[k]=AssetDatabase.LoadAssetAtPath<Mesh>(D+"/Meshes/"+(k*2+1)+".asset");
        var trunk=new Mesh[5];for(int k=0;k<5;k++)trunk[k]=AssetDatabase.LoadAssetAtPath<Mesh>(D+"/Meshes/"+(k*2)+".asset");
        var barkBatch=new List<CombineInstance>();
        for(int n=0;n<700;n++)
        {
            float x=(Random.value<.5f?-1:1)*Random.Range(15,126),u=Random.value<.35f?Random.Range(.93f,1f):Random.Range(.04f,.97f);var p=Wall(x,u);
            // only on genuinely stepped places: compare against the surface a little below
            var lower=Wall(x,Mathf.Max(0,u-.03f));if(p.z-lower.z<-.2f&&u<.9f)continue;
            int kind=Random.value<.68f?0:Random.value<.5f?4:Random.value<.5f?1:Random.value<.5f?2:3;
            float size=u>.9f?Random.Range(.35f,.8f):Random.Range(.25f,.6f);
            var m=Matrix4x4.TRS(p-Vector3.up*size*6+Vector3.back*(size*3),Quaternion.Euler(0,Random.Range(0,360),0),new Vector3(size,size*.85f,size));
            batches[kind].Add(new CombineInstance{mesh=canopy[kind],transform=m});
        }
        // a few windswept cliff-top trees with real trunks
        for(int n=0;n<26;n++)
        {
            float x=(n%2==0?-1:1)*Random.Range(18,120);var p=Wall(x,1f);float h=Random.Range(10,22);int kind=n%3==0?4:0;float s=h/10;
            var m=Matrix4x4.TRS(p+new Vector3(0,-.4f,Random.Range(2,8)),Quaternion.Euler(0,Random.Range(0,360),0),new Vector3(s*1.1f,s,s*1.1f));
            batches[kind].Add(new CombineInstance{mesh=canopy[kind],transform=m});barkBatch.Add(new CombineInstance{mesh=trunk[kind],transform=m});
        }
        for(int k=0;k<5;k++){if(batches[k].Count==0)continue;var mesh=new Mesh{name="Cliff greenery "+k,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(batches[k].ToArray(),true,true);mesh=SaveMesh(mesh,"Greenery"+k);Obj("Cliff shrubs and hanging foliage • "+names[k],mesh,AssetDatabase.LoadAssetAtPath<Material>(D+"/"+names[k]+".mat")??AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceRealism/Oak canopy.mat"),g);}
        if(barkBatch.Count>0){var mesh=new Mesh{name="Cliff tree trunks",indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(barkBatch.ToArray(),true,true);mesh=SaveMesh(mesh,"GreeneryTrunks");Obj("Windswept cliff-top tree trunks",mesh,AssetDatabase.LoadAssetAtPath<Material>("Assets/ReferenceRealism/Oak bark.mat"),g);}
    }
}
