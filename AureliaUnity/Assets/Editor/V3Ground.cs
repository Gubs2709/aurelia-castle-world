using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class V3Ground
{
    const int N=2048;const float X0=-450,Z0=-350,SX=900,SZ=900;
    public static string Build()
    {
        var root=V3.Root;
        var splat=new Color32[N*N];var g=new float[N*N];var lit=new float[N*N];var dirt=new float[N*N];var flag=new float[N*N];
        float cell=SX/N;
        void Stamp(float[] ch,float wx,float wz,float rad,float strength)
        {
            int cx=Mathf.RoundToInt((wx-X0)/cell),cz=Mathf.RoundToInt((wz-Z0)/cell),r=Mathf.CeilToInt(rad/cell);
            for(int dz=-r;dz<=r;dz++)for(int dx=-r;dx<=r;dx++)
            {
                int x=cx+dx,z=cz+dz;if(x<0||z<0||x>=N||z>=N)continue;float d=Mathf.Sqrt(dx*dx+dz*dz)*cell;if(d>rad)continue;float f=1-d/rad;f=f*f*(3-2*f)*strength;
                int i=z*N+x;if(f>ch[i])ch[i]=f;
            }
        }
        // autumn litter under autumn trees
        int trees=0;
        foreach(var t in GameObject.Find("17 • Towering mixed forests and blossom groves").transform.Cast<Transform>())
        {
            string n=t.name;bool autumn=n.Contains("maple")||n.Contains("golden")||n.Contains("cathedral")||n.Contains("Ancient");if(!n.Contains("•"))continue;trees++;
            if(autumn||n.Contains("Mature woodland"))Stamp(lit,t.position.x,t.position.z,Mathf.Clamp(t.localScale.y*10*.28f,9,17),n.Contains("maple")?1f:.7f);
        }
        // roads
        foreach(var r in V3Layout.Roads())foreach(var p in r.pts){var target=r.flag?flag:dirt;Stamp(target,p.x,p.y,r.half+1.2f,1f);}
        // flagstone only inside, dirt on shoulders
        for(int i=0;i<N*N;i++){int x=i%N,z=i/N;float wx=X0+x*cell,wz=Z0+z*cell;float nse=V3.Fbm(wx*.07f,wz*.07f,3);
            float d=Mathf.Clamp01(dirt[i]*(.7f+nse*.7f)),f=Mathf.Clamp01((flag[i]-.12f)*1.6f*(.75f+nse*.5f)),dsh=Mathf.Clamp01(flag[i]*1.3f);
            // river banks mud
            float rd=Mathf.Abs(wx-V3.RiverX(wz));if(wz<62&&rd>25&&rd<31.5f)d=Mathf.Max(d,Mathf.SmoothStep(0,1,1-Mathf.Abs(rd-27.5f)/3f)*.6f*nse*1.4f);
            float l=lit[i]*Mathf.Clamp01(.5f+nse*1.1f);l*=1-Mathf.Clamp01(flag[i]*2+dirt[i]*2);
            float a=f;float b=Mathf.Max(d,dsh*(1-f))*(1-f);
            splat[i]=new Color32(0,(byte)(Mathf.Clamp01(l)*255),(byte)(Mathf.Clamp01(b)*255),(byte)(Mathf.Clamp01(a)*255));}
        var tex=new Texture2D(N,N,TextureFormat.RGBA32,true,true){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear,anisoLevel=4};tex.SetPixels32(splat);tex.Apply(true);
        File.WriteAllBytes(V3.Dir+"/Splat.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(V3.Dir+"/Splat.png");
        var imp=(TextureImporter)AssetImporter.GetAtPath(V3.Dir+"/Splat.png");imp.sRGBTexture=false;imp.mipmapEnabled=true;imp.wrapMode=TextureWrapMode.Clamp;imp.filterMode=FilterMode.Bilinear;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=2048;imp.SaveAndReimport();

        var m=V3.Mat("Terrain valley","Aurelia/Terrain2");
        m.SetTexture("_Splat",AssetDatabase.LoadAssetAtPath<Texture2D>(V3.Dir+"/Splat.png"));
        m.SetTexture("_Grass",V3.T("ground_grass"));m.SetTexture("_GrassN",V3.T("ground_grass_n"));
        m.SetTexture("_Litter",V3.T("ground_leaves"));m.SetTexture("_LitterN",V3.T("ground_leaves_n"));
        m.SetTexture("_Dirt",V3.T("ground_dirt"));m.SetTexture("_DirtN",V3.T("ground_dirt_n"));
        m.SetTexture("_Flag",V3.T("path_flagstone"));m.SetTexture("_FlagN",V3.T("path_flagstone_n"));
        m.SetTexture("_Rock",V3.T("rock_stone"));m.SetTexture("_RockN",V3.T("rock_stone_n"));
        m.SetVector("_SplatRect",new Vector4(X0,Z0,SX,SZ));EditorUtility.SetDirty(m);
        var terrain=GameObject.Find("Sculpted river valley and planted escarpment");terrain.GetComponent<MeshRenderer>().sharedMaterial=m;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "splat done, trees "+trees;
    }
}
