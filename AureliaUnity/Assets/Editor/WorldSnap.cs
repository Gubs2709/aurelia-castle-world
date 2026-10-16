using System.IO;
using UnityEngine;
using UnityEditor;
using Aurelia;

public static class WorldSnap
{
    public static string Info()
    {
        var sb=new System.Text.StringBuilder();
        foreach(var n in new[]{"Brown horse on the flower overlook","Long continuous stone bridge deck"}){var g=GameObject.Find(n);sb.AppendLine(n+": "+(g?g.transform.position.ToString():"none"));}
        var c=Object.FindAnyObjectByType<CastleReferenceView>();sb.AppendLine("spawn "+c.spawn+" camdist "+c.cameraDistance);
        foreach(Transform t in GameObject.Find("THE CITADEL OF FALLING WATER Ã¢â‚¬Â¢ reference composition").transform){var rs=t.GetComponentsInChildren<Renderer>();Bounds b=rs.Length>0?rs[0].bounds:new Bounds();foreach(var r in rs)b.Encapsulate(r.bounds);sb.AppendLine(t.name+" children="+t.childCount+" bounds="+b.center+" "+b.size);}
        return sb.ToString();
    }
    public static void Player(string name,float x,float z,float yawOffset=0,float pitch=5)
    {
        var c=Object.FindAnyObjectByType<CastleReferenceView>();c.WalkAt(new Vector3(x,ReferenceCastleBuilder.Height(x,z)+.15f,z));c.Orbit(yawOffset,pitch-5);c.RefreshCamera();Render(name);
    }
    public static void Render(string name)
    {
        var cam=Camera.main;Directory.CreateDirectory("D:/GAME/artifacts/living-world");
        var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes("D:/GAME/artifacts/living-world/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);
    }
    public static string Spawn(){Player("spawn",-20f,-262f);return "ok";}
    public static void Snap(string name,float x,float y,float z,float tx,float ty,float tz,float fov=60)
    {
        var cam=Camera.main;cam.fieldOfView=fov;cam.transform.position=new Vector3(x,y,z);cam.transform.LookAt(new Vector3(tx,ty,tz));
        Directory.CreateDirectory("D:/GAME/artifacts/living-world");
        var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes("D:/GAME/artifacts/living-world/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);
    }
    public static string Backup(){AssetDatabase.CopyAsset("Assets/Scenes/ReferenceCastle.unity","Assets/Scenes/ReferenceCastle_backup_2005.unity");return "ok";}
    public static string Views1()
    {
        Snap("cliff1",-30f,7f,12f,0f,45f,95f,62f);Snap("cliff2",55f,6f,30f,60f,30f,90f,60f);return "ok";
    }
    public static string Views2()
    {
        Snap("castle_far",0f,25f,-150f,0f,110f,100f,52f);Snap("falls_close",70f,4f,45f,70f,35f,90f,55f);return "ok";
    }
    public static string MossStats()
    {
        var m=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/LivingWorld/Meshes/Escarp_West.asset");var c=m.colors;float mn=9,mx=-9,sum=0;foreach(var k in c){mn=Mathf.Min(mn,k.a);mx=Mathf.Max(mx,k.a);sum+=k.a;}
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/LivingWorld/Escarpment rock.mat");
        return "n="+c.Length+" alpha min "+mn+" max "+mx+" avg "+sum/c.Length+" shader="+mat.shader.name+" terrain="+mat.GetFloat("_Terrain")+" tint="+mat.GetColor("_BaseColor");
    }
    public static string Outcrops()
    {
        var g=GameObject.Find("12 â€¢ Fractured rock outcrops and escarpment");var sb=new System.Text.StringBuilder();
        foreach(Transform t in g.transform){var r=t.GetComponent<Renderer>();sb.AppendLine(t.name+" "+(r?r.sharedMaterial.name:"-")+" "+(r?r.bounds.center.ToString():t.position.ToString()));}
        return sb.ToString();
    }
    public static string Views3()
    {
        Snap("castle_side",-260f,120f,100f,0f,130f,150f,55f);Snap("castle_air",0f,330f,-60f,0f,100f,150f,60f);Snap("castle_mid",-90f,150f,0f,0f,170f,160f,60f);return "ok";
    }
    public static string Views4()
    {
        Snap("river1",-34f,6f,-112f,0f,0f,-70f,60f);Snap("river2",-20f,18f,-20f,10f,0f,30f,60f);return "ok";
    }
    public static string Houses()
    {
        var g=GameObject.Find("13 â€¢ Inhabited timber and stone hamlets").transform;var sb=new System.Text.StringBuilder();int i=0;
        foreach(Transform t in g){if(!t.name.StartsWith("Inhabited valley cottage"))continue;var b=t.Find("Solid lime plaster walls");sb.AppendLine((i++)+" "+t.position+" rot "+t.eulerAngles.y+" walls "+(b?b.localScale.ToString():"-")+" children "+t.childCount);}
        return sb.ToString();
    }
    public static string Views5()
    {
        Snap("house1",-107f,6.5f,-100f,-107f,5.5f,-80f,55f);Snap("house2",-120f,7f,-100f,-100f,5f,-85f,60f);return "ok";
    }
    public static string HouseMats()
    {
        var h=GameObject.Find("13 â€¢ Inhabited timber and stone hamlets").transform;var sb=new System.Text.StringBuilder();
        foreach(var f in h.GetComponentsInChildren<MeshRenderer>(true)){if(f.transform.name!="Garden flowers, ivy and joinery"&&!f.transform.parent.name.Contains("batches"))continue;var mf=f.GetComponent<MeshFilter>();sb.AppendLine(f.name+" mat="+f.sharedMaterial.name+" shader="+f.sharedMaterial.shader.name+" verts="+(mf&&mf.sharedMesh?mf.sharedMesh.vertexCount:0)+" parent="+f.transform.parent.name+"/"+f.transform.parent.parent.name);}
        return sb.ToString();
    }
    // Walk test along waypoints: capsule sweep for obstacles + ground continuity.
    public static string PathCheck()
    {
        Physics.SyncTransforms();var sb=new System.Text.StringBuilder();
        Vector3[] w={new Vector3(-20,16,-262),new Vector3(30,16,-250),new Vector3(50,15,-245),new Vector3(38,15,-190),new Vector3(25,15,-90),new Vector3(10,15,0),new Vector3(2,15,60)};
        int problems=0;
        for(int i=0;i<w.Length-1;i++)
        {
            Vector3 a=w[i],b=w[i+1];float len=Vector3.Distance(a,b);Vector3 dir=(b-a).normalized;float lastY=float.NaN;
            for(float d=0;d<len;d+=1f)
            {
                Vector3 p=a+dir*d;
                if(Physics.Raycast(p+Vector3.up*10,Vector3.down,out var g,40,~0,QueryTriggerInteraction.Ignore)){if(!float.IsNaN(lastY)&&Mathf.Abs(g.point.y-lastY)>.7f&&problems<30){sb.AppendLine("STEP "+p+" ground "+lastY.ToString("F2")+"->"+g.point.y.ToString("F2")+" on "+g.collider.name);problems++;}lastY=g.point.y;
                    Vector3 bottom=new Vector3(p.x,g.point.y+.5f,p.z),top=bottom+Vector3.up*1.3f;
                    if(Physics.CapsuleCast(bottom,top,.4f,dir,out var h,1f,~0,QueryTriggerInteraction.Ignore)&&problems<30){sb.AppendLine("BLOCK at "+p+" by "+h.collider.name+" ("+h.collider.GetType().Name+") at "+h.point);problems++;}}
                else if(problems<30){sb.AppendLine("NOGROUND "+p);problems++;}
            }
        }
        return "problems="+problems+"|"+sb.ToString().Replace((char)10,(char)59);
    }
    public static string Walk(string name,float x,float z,float lookX,float lookZ,float pitch=6,float dist=5)
    {
        var c=Object.FindAnyObjectByType<CastleReferenceView>();c.lookTarget=new Vector3(lookX,90,lookZ);c.cameraDistance=dist;c.WalkAt(new Vector3(x,ReferenceCastleBuilder.Height(x,z)+.15f,z));c.Orbit(0,pitch-5);c.RefreshCamera();Render(name);return "ok";
    }
    public static string DeckCheck()
    {
        Physics.SyncTransforms();var sb=new System.Text.StringBuilder();int problems=0;
        for(float z=-240;z<62;z+=1f)
        {
            float x=50-(z+245)*(50f/309f);var p=new Vector3(x,12.6f,z);var dir=new Vector3(-50f/309f,0,1).normalized;
            if(Physics.CapsuleCast(p,p+Vector3.up*1.3f,.4f,dir,out var h,1.2f,~0,QueryTriggerInteraction.Ignore)&&problems<25){sb.Append("BLOCK z="+z.ToString("F0")+" "+h.collider.name+"/"+h.collider.transform.parent?.name+" at "+h.point+"; ");problems++;}
            if(!Physics.Raycast(p+Vector3.up*2,Vector3.down,out var g,6)&&problems<25){sb.Append("GAP z="+z.ToString("F0")+"; ");problems++;}else if(g.point.y<11.8f||g.point.y>13.2f){sb.Append("DECKY z="+z.ToString("F0")+" y="+g.point.y.ToString("F2")+" "+g.collider.name+"; ");problems++;}
        }
        return "problems "+problems+" :: "+sb;
    }
}
