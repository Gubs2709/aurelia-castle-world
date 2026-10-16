using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Aurelia;

public static class WorldFinishing
{
    static Material Material(string name,Color color,float emission=0)
    {var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};m.SetFloat("_Smoothness",.33f);if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);}AssetDatabase.CreateAsset(m,AssetDatabase.GenerateUniqueAssetPath("Assets/World/ArtPass/"+name+".mat"));return m;}
    static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 p,Vector3 scale,Material mat)
    {var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.parent=parent;g.transform.localPosition=p;g.transform.localRotation=Quaternion.identity;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
    public static void Apply()
    {
        var e=UnityEngine.Object.FindAnyObjectByType<ValleyExplorer>();e.pitch=4;
        var old=e.avatar.Find("Traveller armour & cloak");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var animator=e.avatar.GetComponentInChildren<Animator>();if(animator)animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/World/Knight.controller");
        var hero=new GameObject("Traveller armour & cloak").transform;hero.parent=e.avatar;hero.localPosition=Vector3.zero;hero.localRotation=Quaternion.identity;hero.localScale=Vector3.one/e.avatar.localScale.x;
        var steel=Material("Blue tempered steel",new Color(.26f,.37f,.42f));var brass=Material("Heraldic brass",new Color(.65f,.45f,.21f));var dark=Material("Shadowed visor",new Color(.035f,.07f,.085f));var fabric=Material("Traveller teal cloak",new Color(.15f,.36f,.39f));fabric.SetFloat("_Cull",0);
        Shape("Closed steel helmet",PrimitiveType.Sphere,hero,new Vector3(0,1.68f,0),new Vector3(.56f,.64f,.54f),steel);
        Shape("Visor slit",PrimitiveType.Cube,hero,new Vector3(0,1.72f,.26f),new Vector3(.36f,.055f,.07f),dark);
        Shape("Nasal guard",PrimitiveType.Cube,hero,new Vector3(0,1.59f,.27f),new Vector3(.055f,.25f,.06f),brass);
        foreach(float side in new[]{-1f,1f})Shape("Pauldron",PrimitiveType.Sphere,hero,new Vector3(side*.32f,1.35f,0),new Vector3(.39f,.24f,.38f),steel);
        var shield=Shape("Round travel shield",PrimitiveType.Cylinder,hero,new Vector3(-.4f,.95f,-.05f),new Vector3(.63f,.055f,.63f),steel);shield.transform.localRotation=Quaternion.Euler(90,0,0);
        Shape("Shield cross",PrimitiveType.Cube,hero,new Vector3(-.4f,.95f,-.115f),new Vector3(.055f,.45f,.025f),brass);Shape("Shield crossbar",PrimitiveType.Cube,hero,new Vector3(-.4f,1.02f,-.115f),new Vector3(.38f,.055f,.025f),brass);
        Shape("Sheathed blade",PrimitiveType.Cube,hero,new Vector3(.35f,.62f,-.12f),new Vector3(.075f,.8f,.06f),steel);Shape("Sword crossguard",PrimitiveType.Cube,hero,new Vector3(.35f,1.02f,-.12f),new Vector3(.3f,.05f,.08f),brass);
        var vertices=new Vector3[30];var uv=new Vector2[30];var tri=new int[4*5*6];int t=0;
        for(int y=0;y<6;y++)for(int x=0;x<5;x++){int k=y*5+x;float h=y/5f;vertices[k]=new Vector3((x/4f-.5f)*(.5f+h*.23f),1.4f-h*1.05f,-.23f-h*.14f+Mathf.Sin(x*2)*.035f);uv[k]=new Vector2(x/4f,h);if(x<4&&y<5)foreach(int q in new[]{k,k+1,k+5,k+1,k+6,k+5})tri[t++]=q;}
        var mesh=new Mesh{vertices=vertices,uv=uv,triangles=tri};mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath("Assets/World/ArtPass/Traveller cloak.asset"));var cape=new GameObject("Woven travelling cloak");cape.transform.parent=hero;cape.transform.localPosition=Vector3.zero;cape.transform.localRotation=Quaternion.identity;cape.AddComponent<MeshFilter>().sharedMesh=mesh;cape.AddComponent<MeshRenderer>().sharedMaterial=fabric;
        foreach(var tr in hero.GetComponentsInChildren<Transform>())tr.gameObject.layer=2;
        // Restore the illustrated grove palette with textured leaves, not coloured primitive crowns.
        var grove=GameObject.Find("05 • Whispering Grove");int index=0;
        foreach(var r in grove.GetComponentsInChildren<MeshRenderer>())
        {
            var mats=r.sharedMaterials;
            for(int j=0;j<mats.Length;j++)if(mats[j].name.Contains("Leaves"))
            {
                var m=new Material(mats[j]);m.name="Grove leaf tint "+index;var tex=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/StylizedNature/Leaves_BW.png");if(tex)m.mainTexture=tex;
                m.color=new[]{new Color(.30f,.66f,.59f),new Color(.52f,.44f,.66f),new Color(.65f,.58f,.40f)}[index++%3];AssetDatabase.CreateAsset(m,AssetDatabase.GenerateUniqueAssetPath("Assets/World/ArtPass/Grove leaf.mat"));mats[j]=m;
            }
            r.sharedMaterials=mats;
        }
        var volume=UnityEngine.Object.FindAnyObjectByType<Volume>();volume.enabled=true;
        if(volume.sharedProfile.TryGet<Bloom>(out var bloom)){bloom.intensity.Override(.08f);bloom.threshold.Override(2.2f);bloom.clamp.Override(3);EditorUtility.SetDirty(bloom);}
        EditorUtility.SetDirty(e);if(animator)EditorUtility.SetDirty(animator);EditorSceneManager.MarkSceneDirty(e.gameObject.scene);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveOpenScenes();
    }
}
