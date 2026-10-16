using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;

public static class V6SpriteKnight
{
    public static string Install()
    {
        AssetDatabase.Refresh();
        foreach(var n in new[]{"deco_knight_sheet","deco_slash"}){var p="Assets/Art/Gen/"+n+".png";var imp=(TextureImporter)AssetImporter.GetAtPath(p);imp.textureType=TextureImporterType.Default;imp.sRGBTexture=true;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.wrapMode=TextureWrapMode.Clamp;imp.filterMode=FilterMode.Bilinear;imp.maxTextureSize=2048;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();}
        var m=V3.Mat("Sprite knight","Aurelia/Sprite Billboard");m.SetTexture("_BaseMap",V3.T("deco_knight_sheet"));m.SetFloat("_Cutoff",.45f);m.SetColor("_Tint",new Color(1.08f,1.04f,1f));m.SetVector("_UV",new Vector4(0,1f-1f/3,.2f,1f/3));EditorUtility.SetDirty(m);
        var fx=V3.Mat("Sprite slash","Aurelia/Slash Fx");fx.SetTexture("_BaseMap",V3.T("deco_slash"));EditorUtility.SetDirty(fx);
        var c=Object.FindAnyObjectByType<CastleReferenceView>();var walker=c.walker;
        foreach(var old in walker.Cast<Transform>().ToArray())Object.DestroyImmediate(old.gameObject);
        var go=new GameObject("Sprite Knight");go.transform.SetParent(walker,false);go.transform.localPosition=Vector3.zero;go.layer=2;
        var sk=go.AddComponent<SpriteKnight>();sk.material=m;sk.slashMaterial=fx;
        c.avatar=go.transform;EditorUtility.SetDirty(c);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());return "sprite knight installed";
    }
}
