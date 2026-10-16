using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;

public static class WorldValidation
{
    public static string CheckRoutes()
    {
        var log=new StringBuilder();
        var probe=new GameObject("Temporary navigation probe");probe.layer=2;
        var cc=probe.AddComponent<CharacterController>();cc.height=1.9f;cc.radius=.33f;cc.center=Vector3.up*.96f;cc.stepOffset=.35f;cc.slopeLimit=50;
        var explorer=UnityEngine.Object.FindFirstObjectByType<ValleyExplorer>();var originalEnabled=explorer.GetComponent<CharacterController>().enabled;explorer.GetComponent<CharacterController>().enabled=false;
        try
        {
            Run("Village to castle",new Vector3(0,KingdomBuilder.Height(0,-175)+.1f,-175),new[]{new Vector3(0,0,-40),new Vector3(0,0,132),new Vector3(0,0,161)},cc,log);
            Run("Castle to village",new Vector3(0,35,161),new[]{new Vector3(0,0,132),new Vector3(0,0,-40),new Vector3(0,0,-165)},cc,log);
            Run("Village to grove",new Vector3(0,KingdomBuilder.Height(0,-35)+.1f,-35),new[]{new Vector3(45,0,-35),new Vector3(95,0,-45),new Vector3(132,0,-39)},cc,log);
            cc.enabled=false;probe.transform.position=new Vector3(0,KingdomBuilder.Height(0,-180)+.1f,-180);cc.enabled=true;Physics.SyncTransforms();
            for(int i=0;i<30;i++)cc.Move(Vector3.down*.05f);
            float start=probe.transform.position.y,highest=start,velocity=8;
            for(int i=0;i<150;i++){velocity-=22/60f;cc.Move(Vector3.up*velocity/60f);highest=Mathf.Max(highest,probe.transform.position.y);}
            log.AppendLine((highest>start+1&&Mathf.Abs(probe.transform.position.y-start)<.15f?"PASS":"FAIL")+" Jump and landing: rise="+(highest-start).ToString("F2")+", ground error="+Mathf.Abs(probe.transform.position.y-start).ToString("F3"));
        }
        finally {UnityEngine.Object.DestroyImmediate(probe);explorer.GetComponent<CharacterController>().enabled=originalEnabled;}
        string result=log.ToString();Directory.CreateDirectory("../artifacts/unity");File.WriteAllText("../artifacts/unity/navigation.txt",result);Debug.Log(result);return result;
    }
    static void Run(string name,Vector3 start,Vector3[] waypoints,CharacterController cc,StringBuilder log)
    {
        cc.enabled=false;cc.transform.position=start;cc.enabled=true;Physics.SyncTransforms();
        bool success=true;string message="";
        foreach(var target in waypoints)
        {
            int stationary=0;
            for(int i=0;i<6000;i++)
            {
                Vector3 d=target-cc.transform.position;d.y=0;if(d.magnitude<.35f)break;
                var before=cc.transform.position;cc.Move(d.normalized*(5.3f/60)+Vector3.down*.12f);Physics.SyncTransforms();
                if(Vector2.Distance(new Vector2(before.x,before.z),new Vector2(cc.transform.position.x,cc.transform.position.z))<.001f)stationary++;else stationary=0;
                if(stationary>60||cc.transform.position.y< -10){success=false;message="blocked at "+cc.transform.position;break;}
            }
            if(!success)break;
        }
        log.AppendLine((success?"PASS ":"FAIL ")+name+"; "+message+"; end="+cc.transform.position);
    }
    // Live fixes are intentionally targeted; do not rebuild a complete world to fix a route.
    public static void BridgeEntryRamp()
    {
        float z=-34,ground=KingdomBuilder.Height(0,-40),top=3.72f;
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="South viaduct entry ramp";
        go.transform.position=new Vector3(0,(ground+top)/2-.2f,z);go.transform.localScale=new Vector3(12,.4f,12.4f);
        go.transform.rotation=Quaternion.Euler(-Mathf.Atan2(top-ground,12)*Mathf.Rad2Deg,0,0);
        var source=GameObject.Find("Continuous viaduct deck");go.GetComponent<Renderer>().sharedMaterial=source.GetComponent<Renderer>().sharedMaterial;
        go.transform.SetParent(source.transform.parent);EditorSceneManager.MarkSceneDirty(go.scene);EditorSceneManager.SaveOpenScenes();
    }
}
