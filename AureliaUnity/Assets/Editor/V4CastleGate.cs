using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Random=UnityEngine.Random;

public static class V4CastleGate
{
    public static string Build()
    {
        var src=GameObject.Find("02 • Nested castle terraces and soaring towers");int off=0;
        foreach(var t in src.GetComponentsInChildren<Transform>(true))
        {
            if(t.name=="Curtain left of gate"||t.name=="Curtain right of gate"||t.name=="Gate lintel"){var p=t.position;if(Mathf.Abs(p.z-65)<4||Mathf.Abs(p.z-105)<4){Object.DestroyImmediate(t.gameObject);off++;}}
        }
        V3Castle.Build();
        var g=V3.FreshGroup("32 • Castle gates and wall dressing");Random.InitState(77);
        // two gatehouses sized to the old wall openings
        var a=V4City.Place("Prop_Gate",new Vector3(0,12f,65),Vector3.back,g,34f/18f,false);var b=V4City.Place("Prop_Gate",new Vector3(0,34f,105),Vector3.back,g,31f/18f,false);
        // solid blocks either side so the wall reads continuous behind the gatehouses
        int cards=0;
        var ivy=V3.Foliage("Wall ivy","leaf_broad","#ffffff",.45f);var flw=V3.Foliage("Wall flowers","flower_pink","#ffffff",.5f);
        var mb=new V3.MB(1);var fb=new V3.MB(1);
        foreach(var wall in new[]{(z:65f,y:12f,w:34f,h:18f),(z:105f,y:34f,w:31f,h:23f)})
            for(int i=0;i<90;i++)
            {
                float x=Random.Range(-wall.w/2-2,wall.w/2+2);if(Mathf.Abs(x)<8.5f)continue;float y=wall.y+Random.Range(.3f,wall.h*.8f);float z=wall.z-2.6f-.15f;float s=Random.Range(1.6f,3.2f);
                var r=new Vector3(s*.5f,0,0);var u=Vector3.up*s*.8f;float yaw=Random.Range(-.3f,.3f);
                mb.Quad(0,new Vector3(x,y,z)-r,new Vector3(x,y,z)+r,new Vector3(x,y,z)+r+u,new Vector3(x,y,z)-r+u,new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1),new Color(Random.Range(.85f,1f),1,Random.Range(.85f,1f),.2f),Vector3.back);cards++;
            }
        V3.Obj("Castle wall ivy",V3.SaveMesh(mb.Build("Castle ivy"),"CastleIvy"),new[]{ivy},g);
        // hanging banners from the parapets
        foreach(var x in new[]{-24f,-14f,14f,24f})V4City.Place(Random.value<.5f?"Prop_BannerBlue":"Prop_BannerYellow",new Vector3(x,12f+7.5f,62.2f),Vector3.back,g,2.2f,false);
        foreach(var x in new[]{-22f,-12f,12f,22f})V4City.Place(Random.value<.5f?"Prop_BannerBlue":"Prop_BannerYellow",new Vector3(x,34f+10f,102.2f),Vector3.back,g,2.4f,false);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        return "gates placed, removed old openings "+off+", ivy cards "+cards;
    }
}
