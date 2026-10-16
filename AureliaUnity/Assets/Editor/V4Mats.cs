using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Material library for the Blender-authored models. Slot names in the FBX files map to these materials.
public static class V4Mats
{
    static Dictionary<string,Material> lib;
    static Color C(string h){return V3.C(h);}
    static Material Lit(string n,string tex,string nrm,float smooth,float tile,string tint="#ffffff")
    {
        var m=V3.Lit("M4 "+n,tex,nrm,smooth,new Vector2(tile,tile),tint);return m;
    }
    static Material Flat(string n,string hex,float smooth=.12f,float metal=0,string emission=null,float emissionK=1)
    {
        var m=V3.Mat("M4 "+n,"Universal Render Pipeline/Lit");m.SetColor("_BaseColor",C(hex));m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);
        m.SetFloat("_Cull",2);
        if(emission!=null){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",C(emission)*emissionK);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;}
        EditorUtility.SetDirty(m);return m;
    }
    public static void Rebuild(){lib=null;Get("x");}
    public static Material Get(string name)
    {
        if(lib==null)
        {
            lib=new Dictionary<string,Material>();
            lib["wall_plaster"]=Lit("plaster","wall_plaster","wall_plaster_n",.04f,1f,"#fff6e4");
            lib["wall_stone"]=Lit("stone","wall_stone","wall_stone_n",.05f,1f,"#f2ece0");
            lib["stone_light"]=Lit("stone light","wall_stone","wall_stone_n",.06f,1f,"#ffffff");
            lib["timber"]=Lit("timber","wood_beam","wood_beam_n",.08f,1f,"#e8c8a8");
            lib["wood_light"]=Lit("wood light","wood_light","wood_light_n",.08f,1f,"#ffffff");
            lib["wood_dark"]=Lit("wood dark","wood_light","wood_light_n",.08f,1f,"#a87c58");
            lib["door_wood"]=Lit("door","wood_light","wood_light_n",.12f,1f,"#9a6a44");
            lib["crate_wood"]=Lit("crate","wood_light","wood_light_n",.06f,1f,"#e8c590");
            lib["roof_terracotta"]=Lit("roof terracotta","roof_terracotta","roof_terracotta_n",.12f,.8f,"#ffe2d0");
            lib["roof_slate"]=Lit("roof slate","roof_slate","roof_slate_n",.2f,.8f,"#dce8ff");
            lib["roof_shingle"]=Lit("roof shingle","roof_shingle","roof_shingle_n",.06f,.8f,"#ffe8c8");
            lib["roof_teal"]=Lit("roof teal","roof_teal","roof_teal_n",.22f,.8f,"#ffffff");
            lib["path_flagstone"]=Lit("flagstone","path_flagstone","path_flagstone_n",.08f,1f,"#ffffff");
            foreach(var kv in new Dictionary<string,string>{{"paint_yellow","#fbd35a"},{"paint_blue","#5b93d4"},{"paint_red","#e0584a"},{"paint_pink","#f4a0c0"},{"paint_green","#6dc079"},{"paint_teal","#4ac6c0"},{"paint_orange","#f59c4a"},{"paint_purple","#9c7ad0"},{"paint_cream","#fff0d0"}})
                lib[kv.Key]=Lit(kv.Key,"wall_plaster","wall_plaster_n",.06f,1f,kv.Value);
            lib["roof_red"]=Lit("roof red","roof_terracotta","roof_terracotta_n",.14f,.8f,"#ffb09c");
            lib["roof_blue"]=Lit("roof blue","roof_slate","roof_slate_n",.22f,.8f,"#8cb8ff");
            lib["trim_white"]=Flat("trim white","#f8f4ea",.08f);
            foreach(var kv in new Dictionary<string,string>{{"hull_red","#d23c30"},{"hull_blue","#3068b4"},{"hull_wood","#a67448"},{"hull_teal","#30b0aa"},{"hull_yellow","#eebc3c"},{"sail","#faf6ea"},{"net","#cdbc8e"},{"fish","#b4cede"},{"lighthouse_red","#dc4038"}})
                lib[kv.Key]=Flat(kv.Key,kv.Value,.15f);
            lib["glass"]=Flat("glass","#3a5f86",.92f,.05f,"#ffcf8a",.35f);
            foreach(var kv in new Dictionary<string,string>{
                {"shutter_green","#4e8f5a"},{"shutter_blue","#4f7fb8"},{"shutter_red","#b8483a"},{"shutter_yellow","#e0b040"},
                {"awning_red","#d4503c"},{"awning_blue","#3f6cb0"},{"awning_yellow","#ecc85a"},{"awning_green","#40905c"},{"awning_white","#f6f2e4"},{"cloth_white","#f6f2e4"},
                {"flower_pink","#f58ab8"},{"flower_yellow","#ffd53c"},{"flower_red","#e8402e"},{"flower_purple","#a470e0"},{"flower_white","#fffaf0"},{"leaf","#58a840"},
                {"banner_blue","#3052a8"},{"banner_yellow","#ecb83a"},{"banner_red","#c03c2e"},{"fruit_orange","#f68a20"},{"fruit_red","#dc3820"},{"fruit_green","#84bc3c"},{"fruit_yellow","#f4d03c"},
                {"rope","#cdb482"},{"hay","#e6c866"},{"gold","#f0c850"},{"trim_gold","#f0c850"},{"cloth_blue","#3050a0"}})
                lib[kv.Key]=Flat(kv.Key,kv.Value,.1f);
            lib["iron"]=Flat("iron","#2c2c34",.35f,.6f);
            lib["lantern_glow"]=Flat("lantern glow","#ffd78a",.3f,0,"#ffb040",2.2f);
            lib["water"]=Flat("fountain water","#58b8d0",.9f);
            // knight
            lib["armor_silver"]=Flat("armor silver","#cfd8e2",.75f,.65f);
            lib["armor_dark"]=Flat("armor dark","#33363f",.55f,.5f);
            lib["steel_dark"]=Flat("steel dark","#6a707c",.6f,.7f);
            lib["black_armor"]=Flat("black armor","#17171d",.7f,.6f);
            lib["cape_teal"]=Flat("cape teal","#1fa6a2",.2f);
            lib["skin"]=Flat("skin","#f4cba4",.18f);
            lib["hair"]=Flat("hair","#9a6438",.25f);
            lib["eye_green"]=Flat("eyes","#8ae04e",.5f,0,"#6ad030",.6f);
            lib["leather"]=Flat("leather","#6a4630",.12f);
            lib["robe_white"]=Flat("robe white","#eef2fa",.1f);
        }
        foreach(var kv in lib)if(kv.Value&&kv.Value.HasProperty("_Cull")&&kv.Value.GetFloat("_Cull")!=0){kv.Value.SetFloat("_Cull",0);EditorUtility.SetDirty(kv.Value);}
        if(lib.TryGetValue(name,out var m))return m;
        // unknown slot: strip FBX suffixes like ".001"
        int dot=name.IndexOf('.');if(dot>0&&lib.TryGetValue(name.Substring(0,dot),out m))return m;
        return lib["wood_dark"];
    }
}
