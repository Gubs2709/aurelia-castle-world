using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Aurelia;

public static class V7Knight
{
    const string Path="Assets/Models/KnightV3/ReferenceKnight.fbx";
    static bool Match(string clip,string key)=>!clip.StartsWith("__preview__")&&clip.EndsWith("|"+key);
    public static string Install()
    {
        AssetDatabase.ImportAsset(Path);var imp=(ModelImporter)AssetImporter.GetAtPath(Path);imp.animationType=ModelImporterAnimationType.Generic;imp.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;imp.importAnimation=true;imp.importBlendShapes=false;imp.importCameras=false;imp.importLights=false;imp.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
        var settings=imp.defaultClipAnimations;
        foreach(var clip in settings){clip.loopTime=clip.name.EndsWith("Idle")||clip.name.EndsWith("Walk")||clip.name.EndsWith("Run");clip.lockRootPositionXZ=true;clip.lockRootHeightY=true;clip.lockRootRotation=true;}
        imp.clipAnimations=settings;imp.SaveAndReimport();
        var clips=AssetDatabase.LoadAllAssetsAtPath(Path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
        AnimationClip Clip(string name)=>clips.First(c=>Match(c.name,name));
        const string cp="Assets/Models/KnightV3/ReferenceKnight.controller";var existing=AssetDatabase.LoadAssetAtPath<AnimatorController>(cp);if(existing)AssetDatabase.DeleteAsset(cp);
        var controller=AnimatorController.CreateAnimatorControllerAtPath(cp);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);controller.AddParameter("Airborne",AnimatorControllerParameterType.Bool);controller.AddParameter("Attack",AnimatorControllerParameterType.Trigger);
        var sm=controller.layers[0].stateMachine;var locomotion=sm.AddState("Locomotion");var blend=new BlendTree{name="Idle walk run",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(blend,controller);blend.AddChild(Clip("Idle"),0);blend.AddChild(Clip("Walk"),.5f);blend.AddChild(Clip("Run"),1);locomotion.motion=blend;sm.defaultState=locomotion;
        var jump=sm.AddState("Jump");jump.motion=Clip("Jump");var atk=sm.AddState("Attack");atk.motion=Clip("Attack");
        var tj=locomotion.AddTransition(jump);tj.hasExitTime=false;tj.duration=.1f;tj.AddCondition(AnimatorConditionMode.If,0,"Airborne");var land=jump.AddTransition(locomotion);land.hasExitTime=false;land.duration=.15f;land.AddCondition(AnimatorConditionMode.IfNot,0,"Airborne");
        var ta=sm.AddAnyStateTransition(atk);ta.hasExitTime=false;ta.duration=.1f;ta.canTransitionToSelf=false;ta.AddCondition(AnimatorConditionMode.If,0,"Attack");var done=atk.AddTransition(locomotion);done.hasExitTime=true;done.exitTime=.92f;done.duration=.15f;
        var c=UnityEngine.Object.FindAnyObjectByType<CastleReferenceView>();var walker=c.walker;
        foreach(var old in walker.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Path),walker);go.name="Animated reference knight V3";go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.identity;
        var anim=go.GetComponent<Animator>();if(!anim)anim=go.AddComponent<Animator>();anim.runtimeAnimatorController=controller;anim.applyRootMotion=false;anim.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        anim.avatar=AssetDatabase.LoadAllAssetsAtPath(Path).OfType<Avatar>().FirstOrDefault();
        if(!anim.avatar)
        {
            string ap="Assets/Models/KnightV3/ReferenceKnightGeneric.asset";var av=AvatarBuilder.BuildGenericAvatar(go,"");av.name="ReferenceKnightGeneric";var oldAvatar=AssetDatabase.LoadAssetAtPath<Avatar>(ap);if(oldAvatar){EditorUtility.CopySerialized(av,oldAvatar);UnityEngine.Object.DestroyImmediate(av);av=oldAvatar;}else AssetDatabase.CreateAsset(av,ap);anim.avatar=av;
        }
        foreach(var tr in go.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=2;
        foreach(var r in go.GetComponentsInChildren<Renderer>())
        {
            var materials=r.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                var src=materials[i];string name=src?src.name:"Knight cloth";string mp="Assets/Models/KnightV3/"+name.Replace('/','_')+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(mp);
                if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));if(src&&src.HasProperty("_Color"))m.SetColor("_BaseColor",src.GetColor("_Color"));else if(src&&src.HasProperty("_BaseColor"))m.SetColor("_BaseColor",src.GetColor("_BaseColor"));AssetDatabase.CreateAsset(m,mp);}
                m.SetFloat("_Smoothness",.22f);m.SetFloat("_Metallic",name.Contains("silver")||name.Contains("plate")?.35f:0);m.SetFloat("_Cull",0);
                m.shader=Shader.Find("Aurelia/Anime Character");
                if(name.Contains("Cyan")||name.Contains("Sword light")||name.Contains("Blade core")){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",new Color(.18f,.65f,.8f)*1.5f);}
                materials[i]=m;EditorUtility.SetDirty(m);
            }
            r.sharedMaterials=materials;
        }
        go.AddComponent<KnightV3Motion>();c.avatar=go.transform;c.startWithPlayer=true;EditorUtility.SetDirty(c);
        EditorUtility.SetDirty(controller);EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());V3Run.Save();
        return "3D knight installed: "+string.Join(",",clips.Select(x=>x.name))+"; skinned renderers="+go.GetComponentsInChildren<SkinnedMeshRenderer>().Length;
    }
}
