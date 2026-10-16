using System;
using System.IO;
using System.Collections;
using UnityEngine;

namespace Aurelia
{
    // Opt-in release verification; normal play does not create this component or write a report.
    public class DisplayModeCheck:MonoBehaviour
    {
        [Serializable] class Report {public bool fullscreen;public string mode;public int width,height,desktopWidth,desktopHeight;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-display-check")>=0)new GameObject("Display mode verification").AddComponent<DisplayModeCheck>();}
        IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(3);
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../artifacts/release"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"display-mode.json"),JsonUtility.ToJson(new Report{
                fullscreen=Screen.fullScreen,mode=Screen.fullScreenMode.ToString(),width=Screen.width,height=Screen.height,
                desktopWidth=Display.main.systemWidth,desktopHeight=Display.main.systemHeight},true));
            Destroy(gameObject);
        }
    }
}
