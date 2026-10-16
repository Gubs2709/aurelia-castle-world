using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Aurelia
{
    public class WorldV7Benchmark:MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init(){if(Environment.GetCommandLineArgs().Any(a=>a=="-world-v7-check"||a=="-world-v8-check"))new GameObject("World native check").AddComponent<WorldV7Benchmark>();}
        string dir;System.Text.StringBuilder report=new System.Text.StringBuilder();CastleReferenceView c;
        void Check(bool ok,string text){report.AppendLine((ok?"PASS ":"FAIL ")+text);}
        IEnumerator Shot(string name){ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.15f);}
        IEnumerator Fps(string name)
        {
            var times=new List<float>();float end=Time.realtimeSinceStartup+3;
            while(Time.realtimeSinceStartup<end){yield return new WaitForEndOfFrame();times.Add(Time.unscaledDeltaTime);}
            times.Sort();report.AppendLine(name+" FPS="+(times.Count/times.Sum()).ToString("F1")+" p95ms="+(times[(int)(times.Count*.95f)]*1000).ToString("F2"));
        }
        IEnumerator Start()
        {
            bool v8=Environment.GetCommandLineArgs().Contains("-world-v8-check");dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../artifacts/"+(v8?"world-v8":"world-v7")+"/native"));Directory.CreateDirectory(dir);c=FindAnyObjectByType<CastleReferenceView>();yield return new WaitForSeconds(3);
            report.AppendLine(SystemInfo.graphicsDeviceName+" | "+Screen.width+"x"+Screen.height);QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            if(v8)Check(UnityEngine.Rendering.VolumeManager.instance.stack.GetComponent<UnityEngine.Rendering.Universal.Bloom>().intensity.value==0,"effective runtime bloom is zero (including pipeline defaults)");
            var anim=c.avatar.GetComponentInChildren<Animator>();Check(anim&&c.walker.gameObject.activeSelf,"visible 3D animated knight at startup");
            yield return Shot("spawn-player");
            var old=c.view.transform.position;float yaw=c.CameraYaw;c.Orbit(75,0);yield return new WaitForSeconds(.4f);Check(Mathf.Abs(c.CameraYaw-yaw)>60&&Vector3.Distance(old,c.view.transform.position)>1,"third person orbit");
            var skin=c.avatar.GetComponentInChildren<SkinnedMeshRenderer>();Check(skin&&skin.bones.Length>=40,"skinned knight bones="+(skin?skin.bones.Length:0));
            var motion=c.avatar.GetComponent<KnightV3Motion>();c.enabled=false;if(motion)motion.enabled=false;
            c.avatar.rotation=Quaternion.identity;var p=c.walker.position;c.view.transform.position=p+new Vector3(0,1.2f,4.2f);c.view.transform.LookAt(p+Vector3.up*1.05f);
            anim.SetFloat("Speed",0);yield return new WaitForSeconds(.3f);yield return Shot("knight-front-idle");
            var shin=skin.bones.FirstOrDefault(b=>b.name=="Shin.L");anim.SetBool("Airborne",false);anim.SetFloat("Speed",.5f);anim.Play("Locomotion",0,0);yield return new WaitForSeconds(.1f);var r0=shin.localRotation;float angle=0;for(int i=0;i<20;i++){yield return new WaitForSeconds(.08f);angle=Mathf.Max(angle,Quaternion.Angle(r0,shin.localRotation));}Check(angle>2,"walk clip knee motion over full cycle="+angle.ToString("F2")+"deg; Speed="+anim.GetFloat("Speed"));c.view.transform.position=p+new Vector3(3,1.2f,3);c.view.transform.LookAt(p+Vector3.up*1.05f);yield return Shot("knight-walk");
            anim.SetFloat("Speed",1);yield return new WaitForSeconds(.3f);yield return Shot("knight-run");
            anim.SetBool("Airborne",true);yield return new WaitForSeconds(.25f);Check(anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"),"jump state");yield return Shot("knight-jump");anim.SetBool("Airborne",false);yield return new WaitForSeconds(.3f);
            anim.SetTrigger("Attack");yield return new WaitForSeconds(.25f);Check(anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"),"attack state");yield return Shot("knight-attack");yield return new WaitForSeconds(.8f);
            if(motion)motion.enabled=true;c.enabled=true;c.WalkAt(c.harbourPoint);c.Orbit(-c.CameraYaw,0);yield return new WaitForSeconds(.8f);
            Check(c.walker.GetComponent<CharacterController>().isGrounded,"ocean quay player is grounded");yield return Shot("harbour-player");
            // A straight landing stage isolates speed from steering around the curved promenade's posts.
            if(v8){c.WalkAt(new Vector3(318,3.3f,202.3f));c.Orbit(90-c.CameraYaw,0);yield return new WaitForSeconds(.5f);}
            var original=Keyboard.current;var kb=InputSystem.AddDevice<Keyboard>();kb.MakeCurrent();
            var start=c.walker.position;float end=Time.realtimeSinceStartup+1;while(Time.realtimeSinceStartup<end){InputSystem.QueueStateEvent(kb,new KeyboardState(Key.W));yield return null;}InputSystem.QueueStateEvent(kb,new KeyboardState());yield return null;float dist=Vector3.Distance(start,c.walker.position);Check(dist>4&&dist<6,"walk input "+dist.ToString("F2")+"m");
            start=c.walker.position;end=Time.realtimeSinceStartup+1;while(Time.realtimeSinceStartup<end){InputSystem.QueueStateEvent(kb,new KeyboardState(Key.W,Key.LeftShift));yield return null;}InputSystem.QueueStateEvent(kb,new KeyboardState());yield return null;dist=Vector3.Distance(start,c.walker.position);Check(dist>8&&dist<12,"sprint input "+dist.ToString("F2")+"m");
            InputSystem.RemoveDevice(kb);if(original!=null)original.MakeCurrent();
            var boats=FindObjectsByType<BoatBob>();var clouds=FindObjectsByType<CloudDrift>();var b0=boats.Select(b=>b.transform.position).ToArray();var cl0=clouds.Select(b=>b.transform.position).ToArray();yield return new WaitForSeconds(1);
            Check(boats.Where((b,i)=>Vector3.Distance(b0[i],b.transform.position)>.001f).Count()>20,"lake and ocean boat animation");Check(clouds.Where((b,i)=>Vector3.Distance(cl0[i],b.transform.position)>.1f).Count()>=20,"large and small drifting clouds");
            c.enabled=false;c.view.transform.position=v8?new Vector3(390,22,142):new Vector3(296,54,104);c.view.transform.LookAt(v8?new Vector3(298,29,222):new Vector3(389,5,211));yield return new WaitForSeconds(.4f);yield return Shot("ocean-overview");yield return Fps("ocean harbour");
            if(v8)
            {
                var residents=FindObjectsByType<HarbourResidentMotion>();Check(residents.Length==22,"22 harbour residents");var walker=residents.FirstOrDefault(n=>n.walking);var npcStart=walker.transform.position;yield return new WaitForSeconds(.6f);Check(Vector3.Distance(npcStart,walker.transform.position)>.1f,"quayside resident walks on route");
                c.enabled=true;c.WalkAt(c.retreatPoint);yield return new WaitForSeconds(.5f);Check(c.walker.GetComponent<CharacterController>().isGrounded,"river retreat player grounded");yield return Shot("river-player");yield return Fps("river retreat");
                c.WalkAt(c.headlandPoint);yield return new WaitForSeconds(.5f);Check(c.walker.GetComponent<CharacterController>().isGrounded,"sea guardian player grounded");c.Orbit(45-c.CameraYaw,0);yield return new WaitForSeconds(.4f);yield return Shot("guardian-player");yield return Fps("guardian cliff");
                c.enabled=false;c.view.transform.position=new Vector3(316,44,321);c.view.transform.LookAt(new Vector3(356,44,361));yield return new WaitForSeconds(.4f);yield return Shot("guardian-overview");
            }
            c.enabled=true;c.WalkAt(c.autumnPoint);yield return new WaitForSeconds(.5f);yield return Shot("autumn-player");yield return Fps("under canopy");
            c.Picture();yield return new WaitForSeconds(.3f);yield return Shot("castle-vista");yield return Fps("castle vista");
            File.WriteAllText(Path.Combine(dir,"validation.txt"),report.ToString());Application.Quit();
        }
    }
}
