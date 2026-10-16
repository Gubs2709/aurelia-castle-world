using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Aurelia;

public static class V4Knight
{
    public static string Install()
    {
        var c=Object.FindAnyObjectByType<CastleReferenceView>();var walker=c.walker;
        foreach(var old in walker.Cast<Transform>().ToArray())Object.DestroyImmediate(old.gameObject);
        var src=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Village/Knight.fbx");
        var go=(GameObject)PrefabUtility.InstantiatePrefab(src,walker);go.name="Knight";go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;go.transform.localScale=Vector3.one;
        foreach(var r in go.GetComponentsInChildren<MeshRenderer>(true)){var ms=r.sharedMaterials;r.sharedMaterials=ms.Select(m=>V4Mats.Get(m?m.name:"")).ToArray();r.gameObject.layer=2;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;}
        foreach(var t in go.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
        go.AddComponent<KnightAnimator>();c.avatar=go.transform;EditorUtility.SetDirty(c);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());return "knight installed";
    }
}
