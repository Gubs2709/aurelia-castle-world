using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Aurelia
{
    public class WorldBenchmark : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-aurelia-benchmark")>=0)new GameObject("Benchmark recorder").AddComponent<WorldBenchmark>();
        }
        IEnumerator Start()
        {
            var explorer=FindAnyObjectByType<ValleyExplorer>();QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;Application.runInBackground=true;
            yield return new WaitForSeconds(4);
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../artifacts/unity"));Directory.CreateDirectory(dir);
            var checks=new System.Text.StringBuilder();var originalKeyboard=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();keyboard.MakeCurrent();
            explorer.SetPaused(false);explorer.Teleport(explorer.spawn);explorer.yaw=0;yield return new WaitForSeconds(.5f);
            var start=explorer.transform.position;float deadline=Time.realtimeSinceStartup+2;
            while(Time.realtimeSinceStartup<deadline){InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return null;}
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            float distance=Vector3.Distance(start,explorer.transform.position);checks.AppendLine((distance>9.5f&&distance<11.7f?"PASS":"FAIL")+" native keyboard movement over 2s: "+distance.ToString("F2")+" m");
            explorer.Teleport(explorer.spawn);yield return new WaitForSeconds(.5f);float baseY=explorer.transform.position.y,peak=baseY;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;InputSystem.QueueStateEvent(keyboard,new KeyboardState());deadline=Time.realtimeSinceStartup+1.7f;
            while(Time.realtimeSinceStartup<deadline){peak=Mathf.Max(peak,explorer.transform.position.y);yield return null;}
            checks.AppendLine((peak>baseY+.8f&&Mathf.Abs(explorer.transform.position.y-baseY)<.2f?"PASS":"FAIL")+" native jump/landing: rise="+(peak-baseY).ToString("F2")+" m");
            explorer.SetPaused(true);start=explorer.transform.position;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));yield return new WaitForSeconds(.3f);checks.AppendLine((Vector3.Distance(start,explorer.transform.position)<.01f?"PASS":"FAIL")+" paused movement blocked");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.RemoveDevice(keyboard);if(originalKeyboard!=null)originalKeyboard.MakeCurrent();File.WriteAllText(Path.Combine(dir,"native-controls.txt"),checks.ToString());
            explorer.enabled=false;
            var frames=new List<float>();string[] names={"Village","Waterfall castle vista","Grove","Gardens"};
            Vector3[] views={new Vector3(0,5,-167),new Vector3(-52,25,3),new Vector3(113,11,-64),new Vector3(-22,39,153)};
            Vector3[] targets={new Vector3(0,25,140),new Vector3(0,66,200),new Vector3(145,17,-24),new Vector3(0,68,220)};
            var report=new System.Text.StringBuilder();report.AppendLine("Unity standalone benchmark | "+SystemInfo.graphicsDeviceName+" | "+Screen.width+"x"+Screen.height);
            for(int i=0;i<views.Length;i++)
            {
                explorer.view.transform.position=views[i];explorer.view.transform.LookAt(targets[i]);yield return new WaitForSeconds(2);frames.Clear();float end=Time.realtimeSinceStartup+7;
                while(Time.realtimeSinceStartup<end){yield return new WaitForEndOfFrame();frames.Add(Time.unscaledDeltaTime);}
                frames.Sort();float sum=0;foreach(float dt in frames)sum+=dt;report.AppendLine(names[i]+": avgFPS="+(frames.Count/sum).ToString("F1")+", p95FrameMs="+(frames[(int)(frames.Count*.95f)]*1000).ToString("F2")+", frames="+frames.Count);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,"native-"+i+".png"));yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.3f);
            }
            File.WriteAllText(Path.Combine(dir,"standalone-benchmark.txt"),report.ToString());Application.Quit();
        }
    }
}
