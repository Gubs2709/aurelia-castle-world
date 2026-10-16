using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Aurelia
{
    public class CastleSceneBenchmark:MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-castle-check")>=0)new GameObject("Castle native validation").AddComponent<CastleSceneBenchmark>();}
        static float Diff(Texture2D a,Texture2D b,float x0,float x1,float y0,float y1)
        {int w=a.width,h=a.height,changed=0,total=0;for(int y=(int)(y0*h);y<(int)(y1*h);y+=3)for(int x=(int)(x0*w);x<(int)(x1*w);x+=3){var p=a.GetPixel(x,y);var q=b.GetPixel(x,y);if(Mathf.Max(Mathf.Abs(p.r-q.r),Mathf.Abs(p.g-q.g),Mathf.Abs(p.b-q.b))>.04f)changed++;total++;}return total==0?0:changed/(float)total;}
        IEnumerator Start()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../artifacts/living-world"));Directory.CreateDirectory(dir);
            var view=FindAnyObjectByType<CastleReferenceView>();var report=new System.Text.StringBuilder();yield return new WaitForSeconds(3);
            report.AppendLine("Native reference castle | "+SystemInfo.graphicsDeviceName+" | "+Screen.width+"x"+Screen.height);
            report.AppendLine((view.avatar&&view.avatar.GetComponentInChildren<Animator>()&&view.walker.gameObject.activeSelf?"PASS":"FAIL")+" visible animated player starts in third person");
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,"native-spawn.png"));yield return new WaitForEndOfFrame();
            var fa=ScreenCapture.CaptureScreenshotAsTexture();yield return new WaitForSeconds(.4f);yield return new WaitForEndOfFrame();var fb=ScreenCapture.CaptureScreenshotAsTexture();
            report.AppendLine("Flow check, river region (0.45-1.0 x 0.42-0.70): "+Diff(fa,fb,.45f,1f,.42f,.7f).ToString("P1")+" of pixels changed in 0.4 s; whole frame "+Diff(fa,fb,0,1,0,1).ToString("P1"));Destroy(fa);Destroy(fb);
            var originalView=view.view.transform.position;float originalYaw=view.CameraYaw;view.Orbit(70,-6);yield return new WaitForSeconds(.25f);report.AppendLine((Mathf.Abs(view.CameraYaw-originalYaw)>60&&Vector3.Distance(originalView,view.view.transform.position)>1?"PASS":"FAIL")+" third-person camera can orbit around player");
            var clouds=FindObjectsByType<CloudDrift>();var cloudStart=new Vector3[clouds.Length];for(int i=0;i<clouds.Length;i++)cloudStart[i]=clouds[i].transform.position;
            var original=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F,Key.H));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            var start=view.view.transform.position;float end=Time.realtimeSinceStartup+1;
            while(Time.realtimeSinceStartup<end){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return null;}
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;float flight=Vector3.Distance(start,view.view.transform.position);report.AppendLine((flight>14&&flight<18?"PASS":"FAIL")+" free flight keyboard movement: "+flight.ToString("F2")+" m");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.P));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            report.AppendLine((Vector3.Distance(view.view.transform.position,view.picturePosition)<.01f?"PASS":"FAIL")+" reference viewpoint reset");
            view.WalkAt(new Vector3(48.5f,13.5f,-236));InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Enter));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSeconds(.5f);
            start=view.walker.position;end=Time.realtimeSinceStartup+2;while(Time.realtimeSinceStartup<end){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return null;}InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            float walk=Vector3.Distance(start,view.walker.position);report.AppendLine((walk>8&&walk<12?"PASS":"FAIL")+" bridge walking: "+walk.ToString("F2")+" m, y="+view.walker.position.y.ToString("F2"));
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.P));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            InputSystem.RemoveDevice(keyboard);if(original!=null)original.MakeCurrent();QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,"native-clouds-t0.png"));yield return new WaitForEndOfFrame();
            var samples=new List<float>();end=Time.realtimeSinceStartup+5;
            while(Time.realtimeSinceStartup<end){yield return new WaitForEndOfFrame();samples.Add(Time.unscaledDeltaTime);}
            samples.Sort();float sum=0;foreach(var dt in samples)sum+=dt;report.AppendLine("Rendered vista: "+(samples.Count/sum).ToString("F1")+" average FPS, p95="+(samples[(int)(samples.Count*.95f)]*1000).ToString("F2")+" ms, frames="+samples.Count);
            int moving=0;for(int i=0;i<clouds.Length;i++)if(Vector3.Distance(cloudStart[i],clouds[i].transform.position)>2)moving++;report.AppendLine((moving==clouds.Length&&moving>=20?"PASS":"FAIL")+" animated large/small clouds: "+moving+" of "+clouds.Length+" moved more than 2 m");
            report.AppendLine((view.view.GetComponent<RiverReflections>()?"PASS":"FAIL")+" planar river reflections attached");
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,"native-landscape.png"));yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.4f);
            view.WalkAt(view.blossomPoint);yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(dir,"native-blossom-player.png"));yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.3f);
            view.WalkAt(view.cottagePoint);yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(dir,"native-garden-player.png"));yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.3f);
            view.WalkAt(view.waterfallPoint);yield return new WaitForSeconds(1);ScreenCapture.CaptureScreenshot(Path.Combine(dir,"native-cascades-player.png"));yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.3f);
            yield return new WaitForEndOfFrame();var ca=ScreenCapture.CaptureScreenshotAsTexture();yield return new WaitForSeconds(.4f);yield return new WaitForEndOfFrame();var cb=ScreenCapture.CaptureScreenshotAsTexture();
            report.AppendLine("Flow check, cascade view band (0.1-0.9 x 0.30-0.62): "+Diff(ca,cb,.1f,.9f,.3f,.62f).ToString("P1")+" of pixels changed in 0.4 s");Destroy(ca);Destroy(cb);
            view.WalkAt(view.forestPoint);yield return new WaitForSeconds(1);samples.Clear();end=Time.realtimeSinceStartup+4;while(Time.realtimeSinceStartup<end){yield return new WaitForEndOfFrame();samples.Add(Time.unscaledDeltaTime);}sum=0;foreach(var dt in samples)sum+=dt;report.AppendLine("Inside forest: "+(samples.Count/sum).ToString("F1")+" rendered average FPS");
            File.WriteAllText(Path.Combine(dir,"native-validation.txt"),report.ToString());Application.Quit();
        }
    }
}
