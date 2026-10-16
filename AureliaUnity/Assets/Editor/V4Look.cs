using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;

// Colourful anime-leaning grade: saturated warm light, lifted shadows, soft bloom, gentle vignette.
public static class V4Look
{
    public static string Apply()
    {
        var vol=Object.FindObjectsByType<Volume>(FindObjectsInactive.Include).First(v=>v.sharedProfile!=null);var p=vol.sharedProfile;
        T Get<T>() where T:VolumeComponent{if(!p.TryGet<T>(out var c)){c=p.Add<T>(true);AssetDatabase.AddObjectToAsset(c,p);}return c;}
        var ca=Get<ColorAdjustments>();ca.active=true;ca.saturation.Override(14);ca.contrast.Override(6);ca.postExposure.Override(.05f);ca.colorFilter.Override(V3.C("#fffaf2"));
        var bl=Get<Bloom>();bl.active=true;bl.intensity.Override(.34f);bl.threshold.Override(.95f);bl.scatter.Override(.72f);bl.tint.Override(V3.C("#fff0dc"));
        var vg=Get<Vignette>();vg.active=true;vg.intensity.Override(.2f);vg.smoothness.Override(.6f);
        var tm=Get<Tonemapping>();tm.active=true;tm.mode.Override(TonemappingMode.ACES);
        var lift=Get<LiftGammaGain>();lift.active=true;lift.lift.Override(new Vector4(1.02f,1.0f,1.06f,0.03f));lift.gain.Override(new Vector4(1.04f,1.0f,.96f,.06f));
        EditorUtility.SetDirty(p);
        var sun=RenderSettings.sun;if(sun){sun.color=V3.C("#fff0d0");sun.intensity=1.8f;sun.shadowStrength=.85f;sun.transform.rotation=Quaternion.Euler(42,-38,0);EditorUtility.SetDirty(sun);}
        RenderSettings.ambientSkyColor=V3.C("#bcd8f0");RenderSettings.ambientEquatorColor=V3.C("#c8c0a8");RenderSettings.ambientGroundColor=V3.C("#7c8a60");RenderSettings.fogColor=V3.C("#bcdcf0");RenderSettings.fogDensity=.0006f;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());return "look applied";
    }
}
