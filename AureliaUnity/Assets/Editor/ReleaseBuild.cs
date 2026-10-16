using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class ReleaseBuild
{
    [Serializable] class Result
    {
        public string result,version,output,fullscreen;
        public int errors,warnings;
        public ulong bytes;
    }

    [MenuItem("Aurelia/Build Windows release (fullscreen)")]
    public static void Build()
    {
        string root=Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
        string output=Path.Combine(root,"Builds/ReferenceCastle/Citadel.exe");
        PlayerSettings.bundleVersion="0.8.1";
        PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultIsNativeResolution=true;
        PlayerSettings.resizableWindow=true;
        PlayerSettings.runInBackground=true;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/ReferenceCastle.unity",true)};
        AssetDatabase.SaveAssets();
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=new[]{"Assets/Scenes/ReferenceCastle.unity"},locationPathName=output,
            target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        string dir=Path.Combine(root,"artifacts/release");Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir,"build-summary.json"),JsonUtility.ToJson(new Result{
            result=report.summary.result.ToString(),version=PlayerSettings.bundleVersion,output=output,
            fullscreen=PlayerSettings.fullScreenMode.ToString(),errors=report.summary.totalErrors,
            warnings=report.summary.totalWarnings,bytes=report.summary.totalSize},true));
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows release build failed: "+report.summary.result);
        Debug.Log("AURELIA_RELEASE_BUILD_SUCCEEDED "+output);
    }
}
