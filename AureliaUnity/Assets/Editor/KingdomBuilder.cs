using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Aurelia;
using Random = UnityEngine.Random;

// Run through Unity's editor API. All generated geometry/materials are saved assets.
public static class KingdomBuilder
{
    const string Gen = "Assets/World";
    static Transform root, area;
    static Material limestone, trim, slate, wood, plaster, road, gold, leaf, trunk, water, glow, pink, cloth, glass, rock;
    static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
    static Dictionary<string, Material> materials = new Dictionary<string, Material>();
    static int meshId;
    static Color Hex(string s) { ColorUtility.TryParseHtmlString(s, out var c); return c; }
    static float Noise(float x, float z) => Mathf.PerlinNoise(x * .013f + 40, z * .013f + 90);
    public static float Height(float x, float z)
    {
        float hill = Mathf.Pow(Mathf.Clamp01((Mathf.Abs(x) - 75) / 240), 1.4f) * (22 + Noise(x, z) * 65);
        float baseH = .8f + Noise(x, z) * 2.4f + hill;
        float castle = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(85, 137, z)) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(115, 70, Mathf.Abs(x)));
        baseH = Mathf.Lerp(baseH, 34, castle);
        float river = Mathf.Exp(-Mathf.Pow((z - 35 - Mathf.Sin(x * .013f) * 13) / 17, 4));
        return Mathf.Lerp(baseH, -4, river);
    }
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop Play mode before rebuilding.");
        Random.InitState(71421); meshId = 0; meshes.Clear(); materials.Clear();
        Directory.CreateDirectory(Gen); Directory.CreateDirectory(Gen + "/Meshes"); Directory.CreateDirectory(Gen + "/Materials"); Directory.CreateDirectory(Gen + "/Textures");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        root = new GameObject("AURELIA • Kingdom beyond the falls").transform;
        area = root;
        MakeMaterials();
        Lighting();
        Group("01 • The living valley"); Landscape();
        Group("02 • Crown of Aurelia"); Castle();
        Group("03 • Rivergate viaduct & waterfalls"); Viaduct();
        Group("04 • Briarwood lantern quarter"); Village();
        Group("05 • Whispering Grove"); Grove();
        Group("06 • Wildflower meadows"); Vegetation();
        Group("07 • Distant peaks and clouds"); DistanceScenery();
        Group("08 • Travellers & wings"); Dragon();
        Explorer();
        foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
        {
            if (r.GetComponentInParent<ValleyMotion>() == null) GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Aurelia.unity", true) };
        PlayerSettings.companyName = "Aurelia"; PlayerSettings.productName = "Aurelia — Beyond the Falls";
        PlayerSettings.defaultScreenWidth = 1920; PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Aurelia.unity");
        AssetDatabase.SaveAssets();
        SceneView.lastActiveSceneView?.LookAt(new Vector3(0, 30, 110), Quaternion.Euler(18, 0, 0), 210);
        Debug.Log("AURELIA_WORLD_READY: " + root.GetComponentsInChildren<Renderer>().Length + " renderers; all world assets saved.");
    }
    [MenuItem("Aurelia/Rebuild complete world (replaces scene)")]
    public static void BuildComplete()
    {
        Build(); WorldValidation.BridgeEntryRamp(); GroveApproach(); WorldArtPass.Apply(); WorldFinishing.Apply();
        Debug.Log(WorldValidation.CheckRoutes());
    }
    static void Group(string name) { area = new GameObject(name).transform; area.parent = root; }
    static Material Mat(string name, string color, float rough = .7f, float metallic = 0, float emission = 0)
    {
        if (materials.TryGetValue(name, out var found)) return found;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = Hex(color), enableInstancing = true };
        mat.SetFloat("_Smoothness", 1 - rough); mat.SetFloat("_Metallic", metallic);
        if (emission > 0) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", Hex(color) * emission); }
        var path = Gen + "/Materials/" + name + ".mat";
        AssetDatabase.CreateAsset(mat, AssetDatabase.GenerateUniqueAssetPath(path)); materials[name] = mat; return mat;
    }
    static void MakeMaterials()
    {
        limestone = Mat("Ivory weathered limestone", "#d8d8ca"); trim = Mat("Carved pale cornices", "#eeead8");
        slate = Mat("Patinated blue slate", "#24556a", .43f, .15f); wood = Mat("Old warm oak", "#503d30");
        plaster = Mat("Warm lime plaster", "#dac9a4"); road = Mat("Ancient cobblestones", "#a9a18b");
        gold = Mat("Aged brass", "#b98b43", .32f, .65f); leaf = Mat("Meadow canopy", "#557a39");
        trunk = Mat("Ancient bark", "#524d3d"); water = Mat("Jade flowing water", "#328c91", .15f, .35f);
        glow = Mat("Lantern candle light", "#ffb34d", .5f, 0, 3.5f); pink = Mat("Heather petals", "#c777a1");
        cloth = Mat("Burgundy woven cloth", "#944754"); glass = Mat("Deep leaded glass", "#28475b", .25f, .3f);
        rock = Mat("Cliff face", "#7f8c83");
        Texture(limestone, "stone", 512); Texture(road, "cobbles", 512); Texture(slate, "slates", 512); Texture(wood, "bark", 256); Texture(trunk, "bark", 256); Texture(rock, "rock", 256); Texture(water, "water", 256);
    }
    static void Texture(Material mat, string type, int size)
    {
        var tex = new Texture2D(size, size); var pixels = new Color[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float n = Mathf.PerlinNoise(x * .08f, y * .08f), detail = Mathf.PerlinNoise(x * .6f, y * .6f);
            float v = .78f + n * .22f + detail * .06f;
            if (type == "stone" || type == "cobbles" || type == "slates")
            {
                int bh = type == "slates" ? 32 : type == "cobbles" ? 64 : 64;
                int bw = type == "cobbles" ? 64 : 128;
                int row = y / bh, xx = (x + (row % 2) * bw / 2) % bw;
                int edge = Mathf.Min(xx, bw - 1 - xx, y % bh, bh - 1 - y % bh);
                float cell = Mathf.PerlinNoise((x + (row % 2) * bw / 2) / bw * 7.1f, row * 4.7f);
                v = (edge < 2 ? .4f : edge < 4 ? .68f : .81f + cell * .2f) + n * .07f;
            }
            if (type == "bark") v = .6f + Mathf.PerlinNoise(x * .2f, y * .012f) * .45f;
            if (type == "water") v = .6f + Mathf.Pow(Mathf.PerlinNoise(x * .025f, y * .13f), 3) * .8f;
            pixels[y * size + x] = new Color(v, v, v);
        }
        tex.SetPixels(pixels); tex.Apply();
        string path = Gen + "/Textures/" + type + ".png"; File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path); var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        mat.mainTexture = imported; mat.mainTextureScale = Vector2.one;
    }
    static GameObject Node(string name, Vector3 p, Transform parent = null)
    { var go = new GameObject(name); go.transform.parent = parent ?? area; go.transform.localPosition = p; return go; }
    static GameObject Primitive(string name, PrimitiveType type, Vector3 p, Vector3 scale, Material mat, bool collision = false, Transform parent = null)
    {
        var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.parent = parent ?? area; go.transform.localPosition = p; go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        if (!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
    static GameObject Box(string name, Vector3 p, Vector3 scale, Material mat, bool collision = false, Transform parent = null)
    {
        var g=Primitive(name,PrimitiveType.Cube,p,scale,mat,collision,parent);
        if(mat!=limestone && mat!=road && mat!=wood && mat!=slate && mat!=rock)return g;
        string key="Block"+scale.ToString("F2");
        if(!meshes.TryGetValue(key,out var m))
        {
            m=UnityEngine.Object.Instantiate(g.GetComponent<MeshFilter>().sharedMesh);var vertices=m.vertices;var normals=m.normals;var uv=m.uv;
            for(int i=0;i<vertices.Length;i++){Vector3 q=Vector3.Scale(vertices[i],scale);var n=normals[i];uv[i]=Mathf.Abs(n.y)>.5f?new Vector2(q.x,q.z)*.3f:Mathf.Abs(n.x)>.5f?new Vector2(q.z,q.y)*.3f:new Vector2(q.x,q.y)*.3f;}
            m.uv=uv;meshes[key]=SaveMesh(m,"Tiled block");
        }
        g.GetComponent<MeshFilter>().sharedMesh=m;return g;
    }
    static GameObject Ball(string name, Vector3 p, Vector3 scale, Material mat, Transform parent = null) => Primitive(name, PrimitiveType.Sphere, p, scale, mat, false, parent);
    static GameObject Cylinder(string name, Vector3 p, float radius, float h, Material mat, bool collision = false, Transform parent = null) => Primitive(name, PrimitiveType.Cylinder, p, new Vector3(radius * 2, h / 2, radius * 2), mat, collision, parent);
    static Mesh SaveMesh(Mesh m, string name)
    {
        m.name = name; m.RecalculateNormals(); m.RecalculateBounds();
        AssetDatabase.CreateAsset(m, AssetDatabase.GenerateUniqueAssetPath(Gen + "/Meshes/" + (++meshId) + "_" + name + ".asset")); return m;
    }
    static GameObject MeshObject(string name, Mesh mesh, Material mat, Vector3 p, bool collision = false, Transform parent = null)
    {
        var go = Node(name, p, parent); go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        if (collision) go.AddComponent<MeshCollider>().sharedMesh = mesh; return go;
    }
    static Mesh Lathe(string name, Vector2[] profile, int segments = 32)
    {
        if (meshes.TryGetValue(name, out var old)) return old;
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
        for (int i = 0; i < profile.Length; i++) for (int j = 0; j <= segments; j++)
        {
            float a = j * Mathf.PI * 2 / segments;
            v.Add(new Vector3(Mathf.Cos(a) * profile[i].x, profile[i].y, Mathf.Sin(a) * profile[i].x)); uv.Add(new Vector2(j * profile[i].x * .35f / segments, profile[i].y * .18f));
            if (i < profile.Length - 1 && j < segments) { int k = i * (segments + 1) + j; tri.AddRange(new[] { k, k + segments + 1, k + 1, k + 1, k + segments + 1, k + segments + 2 }); }
        }
        var mesh = new Mesh(); mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tri, 0); meshes[name] = SaveMesh(mesh, name); return mesh;
    }
    static void Lighting()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Hex("#87b2cb"); RenderSettings.ambientEquatorColor = Hex("#91a0a0"); RenderSettings.ambientGroundColor = Hex("#4c6045");
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .00165f; RenderSettings.fogColor = Hex("#9dbdc7");
        var sky = new Material(Shader.Find("Aurelia/Painted Sky")); sky.SetColor("_SkyTint", Hex("#7ba8c8")); sky.SetFloat("_Exposure", 1.1f);
        AssetDatabase.CreateAsset(sky, AssetDatabase.GenerateUniqueAssetPath(Gen + "/Materials/Kingdom sky.mat")); RenderSettings.skybox = sky;
        var sun = Node("Sun • warm afternoon", Vector3.zero).AddComponent<Light>(); sun.type = LightType.Directional; sun.color = Hex("#fff0e3"); sun.intensity = 1.65f; sun.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(43, -32, 0); sun.shadowBias = .035f; sun.shadowNormalBias = .3f; RenderSettings.sun = sun;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
        var bloom = profile.Add<Bloom>(); bloom.intensity.Override(.35f); bloom.threshold.Override(1.1f); bloom.scatter.Override(.65f);
        var color = profile.Add<ColorAdjustments>(); color.saturation.Override(5); color.contrast.Override(8); color.postExposure.Override(0);
        profile.Add<Vignette>().intensity.Override(.16f);
        AssetDatabase.CreateAsset(profile, AssetDatabase.GenerateUniqueAssetPath(Gen + "/Kingdom atmosphere.asset"));
        foreach (var comp in profile.components) AssetDatabase.AddObjectToAsset(comp, profile);
        var volume = Node("Atmosphere • filmic light", Vector3.zero).AddComponent<Volume>(); volume.isGlobal = true; volume.sharedProfile = profile;
        var urp = UniversalRenderPipeline.asset;
        if (urp) { urp.supportsHDR = true; urp.shadowDistance = 160; urp.msaaSampleCount = 4; urp.renderScale = 1; EditorUtility.SetDirty(urp); }
        QualitySettings.shadowDistance = 160; QualitySettings.lodBias = 1.5f;
    }
    static void Landscape()
    {
        int n = 240; var v = new Vector3[(n + 1) * (n + 1)]; var uv = new Vector2[v.Length]; var colors = new Color[v.Length]; var tri = new int[n * n * 6];
        for (int z = 0; z <= n; z++) for (int x = 0; x <= n; x++)
        { int k = z * (n + 1) + x; float xx = (x / (float)n - .5f) * 800, zz = (z / (float)n - .5f) * 800; v[k] = new Vector3(xx, Height(xx, zz), zz); uv[k] = new Vector2(xx * .05f, zz * .05f); colors[k] = Color.Lerp(Hex("#4f713b"), Hex("#97a85a"), Noise(xx * 3, zz * 3)); }
        int t = 0; for (int z = 0; z < n; z++) for (int x = 0; x < n; x++) { int k = z * (n + 1) + x; foreach (int i in new[] { k, k + n + 1, k + 1, k + 1, k + n + 1, k + n + 2 }) tri[t++] = i; }
        var mesh = new Mesh { indexFormat = IndexFormat.UInt32 }; mesh.vertices = v; mesh.uv = uv; mesh.colors = colors; mesh.triangles = tri;
        var terrainMat = new Material(Shader.Find("Aurelia/Meadow")); terrainMat.name = "Painted meadow ground"; AssetDatabase.CreateAsset(terrainMat, AssetDatabase.GenerateUniqueAssetPath(Gen + "/Materials/Terrain.mat"));
        MeshObject("Rolling valley ground", SaveMesh(mesh, "Valley terrain"), terrainMat, Vector3.zero, true);
        // Winding river ribbon; real banks descend into the water.
        var rv = new List<Vector3>(); var rt = new List<int>(); var ru = new List<Vector2>();
        for (int i = 0; i <= 120; i++) { float x = -400 + i * 800f / 120; float z = 35 + Mathf.Sin(x * .013f) * 13; rv.Add(new Vector3(x, -.3f, z - 13)); rv.Add(new Vector3(x, -.3f, z + 13)); ru.Add(new Vector2(x * .04f, 0)); ru.Add(new Vector2(x * .04f, 1)); if (i < 120) { int k = i * 2; rt.AddRange(new[] { k, k + 1, k + 2, k + 2, k + 1, k + 3 }); } }
        var river = new Mesh(); river.SetVertices(rv); river.SetUVs(0, ru); river.SetTriangles(rt, 0);
        var riverMat=new Material(Shader.Find("Aurelia/River"));riverMat.name="Flowing turquoise river";AssetDatabase.CreateAsset(riverMat,AssetDatabase.GenerateUniqueAssetPath(Gen+"/Materials/River.mat"));
        MeshObject("River • jade water", SaveMesh(river, "River"), riverMat, Vector3.zero);
        Path(new[] { new Vector3(0, 0, -220), new Vector3(0, 0, -170), new Vector3(0, 0, -100), new Vector3(0, 0, -30) }, 8);
        Path(new[] { new Vector3(0, 0, -35), new Vector3(45, 0, -35), new Vector3(95, 0, -45), new Vector3(137, 0, -34) }, 4.5f);
    }
    static void Path(Vector3[] points, float width)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tr = new List<int>(); int count = 0;
        for (int s = 0; s < points.Length - 1; s++)
        {
            Vector3 a = points[s], b = points[s + 1], side = Vector3.Cross((b - a).normalized, Vector3.up) * width / 2;
            int steps = Mathf.CeilToInt(Vector3.Distance(a, b) / 2);
            for (int i = 0; i <= steps; i++) { Vector3 p = Vector3.Lerp(a, b, i / (float)steps); foreach (float sign in new[] { -1f, 1f }) { Vector3 q = p + side * sign; q.y = Height(q.x, q.z) + .09f; v.Add(q); uv.Add(new Vector2(sign < 0 ? 0 : width * .22f, count * .4f)); } if (i < steps) { int k = count * 2; tr.AddRange(new[] { k, k + 2, k + 1, k + 1, k + 2, k + 3 }); } count++; }
        }
        tr.Reverse();var m = new Mesh(); m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tr, 0); MeshObject("Worn stone footpath", SaveMesh(m, "Path"), road, Vector3.zero);
    }
    public static void GroveApproach()
    {
        root=GameObject.Find("AURELIA • Kingdom beyond the falls").transform;area=root;
        var old=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name=="Worn stone footpath"&&r.bounds.center.x>30).ToArray();foreach(var r in old)UnityEngine.Object.DestroyImmediate(r.gameObject);
        road=GameObject.Find("Continuous viaduct deck").GetComponent<Renderer>().sharedMaterial;
        Path(new[]{new Vector3(0,0,-35),new Vector3(45,0,-35),new Vector3(95,0,-45),new Vector3(137,0,-34)},4.5f);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveOpenScenes();
    }
    static void Beam(Vector3 a, Vector3 b, float width, Material mat, Transform parent = null, string name = "Timber")
    { var g = Box(name, (a + b) / 2, new Vector3(width, Vector3.Distance(a, b), width), mat, false, parent); g.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a); }
    static void Tower(float x, float z, float y, float radius, float height, bool spire = true)
    {
        var g = Node("Carved watchtower", new Vector3(x, y, z)).transform;
        var profile = new[] { new Vector2(radius * 1.15f, 0), new Vector2(radius * 1.15f, 1.3f), new Vector2(radius, 2), new Vector2(radius, height - 3), new Vector2(radius * 1.13f, height - 1.8f), new Vector2(radius * 1.13f, height) };
        MeshObject("Limestone drum", Lathe($"Tower{radius}_{height}", profile), limestone, Vector3.zero, true, g);
        for (float h = 3; h < height; h += 7) Cylinder("Carved string course", new Vector3(0, h, 0), radius + .15f, .32f, trim, false, g);
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI / 6;
            var battlement = Box("Crenellation", new Vector3(Mathf.Sin(a) * radius, height + .65f, Mathf.Cos(a) * radius), new Vector3(radius * .45f, 1.4f, 1.15f), trim, false, g); battlement.transform.localRotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
        }
        for (float h = 7; h < height - 4; h += 9) for (int i = 0; i < 6; i++)
        {
            float a = i * Mathf.PI / 3; var window = Node("Gothic lancet", new Vector3(Mathf.Sin(a) * (radius + .025f), h, Mathf.Cos(a) * (radius + .025f)), g).transform; window.localRotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
            ArchWindow(window, radius > 4 ? 1.2f : .7f, 3.5f);
        }
        if (spire)
        {
            float rh = radius * 3.7f;
            MeshObject("Swept slate spire", Lathe("Roof" + radius, new[] { new Vector2(radius + 1, 0), new Vector2(radius + 1, .45f), new Vector2(radius * .8f, 1.3f), new Vector2(radius * .48f, rh * .45f), new Vector2(radius * .25f, rh * .75f), new Vector2(.05f, rh) }), slate, new Vector3(0, height + .1f, 0), false, g);
            for (int j = 0; j < 5; j++) Cylinder("Slate ridge band", new Vector3(0, height + .4f + j * rh / 5, 0), Mathf.Lerp(radius + .9f, .2f, j / 5f), .16f, slate, false, g);
            Cylinder("Gold finial", new Vector3(0, height + rh + 1, 0), .1f, 2.5f, gold, false, g);
            Flag(new Vector3(0, height + rh + 2, 0), g, 1.2f);
        }
    }
    static void ArchWindow(Transform parent, float width, float height)
    {
        Box("Recessed glass", new Vector3(0, height / 2, 0), new Vector3(width, height, .12f), glass, false, parent);
        foreach (float s in new[] { -1f, 1f }) Box("Stone jamb", new Vector3(s * (width / 2 + .1f), height / 2, .03f), new Vector3(.18f, height + .3f, .25f), trim, false, parent);
        Beam(new Vector3(-width / 2 - .1f, height, .05f), new Vector3(0, height + width * .65f, .05f), .2f, trim, parent);
        Beam(new Vector3(width / 2 + .1f, height, .05f), new Vector3(0, height + width * .65f, .05f), .2f, trim, parent);
        Box("Transom", new Vector3(0, height * .45f, .1f), new Vector3(width, .11f, .17f), gold, false, parent);
        Box("Mullion", new Vector3(0, height / 2, .1f), new Vector3(.09f, height, .17f), trim, false, parent);
        Box("Sill", new Vector3(0, -.15f, .1f), new Vector3(width + .6f, .3f, .5f), trim, false, parent);
    }
    static void Flag(Vector3 p, Transform parent, float scale = 1)
    {
        Cylinder("Banner staff", p, .07f, 6 * scale, gold, false, parent);
        var f = Node("Wind-blown pennant", p + Vector3.up * 2.5f * scale, parent).transform;
        var vertices = new Vector3[20]; var uv = new Vector2[20]; var tr = new List<int>();
        for (int i = 0; i < 10; i++) for (int j = 0; j < 2; j++) { int k = i * 2 + j; vertices[k] = new Vector3(i / 9f * 4.5f * scale, -j * 1.4f * scale + Mathf.Sin(i * .6f) * .15f, Mathf.Sin(i * .8f) * .25f); uv[k] = new Vector2(i / 9f, j); if (i < 9 && j == 0) tr.AddRange(new[] { k, k + 1, k + 2, k + 2, k + 1, k + 3, k + 2, k + 1, k, k + 3, k + 1, k + 2 }); }
        var m = new Mesh(); m.vertices = vertices; m.uv = uv; m.SetTriangles(tr, 0); MeshObject("Silk", SaveMesh(m, "Pennant"), cloth, Vector3.zero, false, f);
        f.gameObject.AddComponent<ValleyMotion>().kind = ValleyMotion.Kind.Flag;
    }
    static void Castle()
    {
        const float y = 34;
        // Three ascending masses establish the monumental silhouette from the references.
        Box("Clifftop terrace", new Vector3(0, y - 1, 186), new Vector3(114, 3, 104), limestone, true);
        foreach (float x in new[] { -44f, 44f })
        {
            Box("Curtain wall", new Vector3(x, y + 9, 180), new Vector3(3, 18, 90), limestone, true);
            for (int i = 0; i < 25; i++) Box("Wall merlon", new Vector3(x, y + 18.6f, 138 + i * 3.6f), new Vector3(3.8f, 1.3f, 1.8f), trim);
            Tower(x, 138, y, 5, 28); Tower(x, 224, y, 6, 39);
        }
        foreach (float x in new[] { -27f, 27f }) Box("Gate wing", new Vector3(x, y + 10, 138), new Vector3(28, 20, 4), limestone, true);
        Arch(new Vector3(0, y, 138), 13, 17, 4, true);
        foreach (float x in new[] { -14f, 14f }) Tower(x, 138, y, 4.8f, 39);
        foreach (float x in new[] { -27f, 27f })
        {
            for (float xx = x - 12; xx < x + 13; xx += 3) Box("Gate merlon", new Vector3(xx, y + 20.7f, 138), new Vector3(1.5f, 1.5f, 4.5f), trim);
            Banner(new Vector3(x, y + 16, 135.85f), 3.2f, 10);
        }
        Box("Royal keep", new Vector3(0, y + 24, 218), new Vector3(60, 48, 32), limestone, true);
        Box("Keep cornice", new Vector3(0, y + 46, 218), new Vector3(62, 1.5f, 34), trim);
        Roof(new Vector3(0, y + 48, 218), 64, 36, 17, slate);
        foreach (float x in new[] { -30f, 30f }) Tower(x, 205, y, 7, 54);
        Tower(0, 232, y + 38, 9, 54); Tower(-13, 239, y + 38, 4.5f, 35); Tower(14, 239, y + 38, 4, 42);
        for (float x = -22; x <= 22; x += 7.3f) for (float h = 8; h <= 34; h += 13)
        { var w = Node("Keep window", new Vector3(x, y + h, 201.8f)).transform; w.rotation = Quaternion.Euler(0, 180, 0); ArchWindow(w, 2.1f, 5); }
        // Terraced formal flower garden with traversable aisles.
        Box("Courtyard pavement", new Vector3(0, y + .16f, 174), new Vector3(84, .3f, 65), road);
        for (int side = -1; side <= 1; side += 2) for (int j = 0; j < 3; j++)
        {
            Vector3 p = new Vector3(side * 27, y + .5f, 152 + j * 16);
            Box("Sunken parterre", p, new Vector3(19, .8f, 12), Mat("Garden soil", "#514b37"));
            foreach (float s in new[] { -1f, 1f }) { Box("Clipped box hedge", p + new Vector3(s * 9.5f, .6f, 0), new Vector3(.9f, 1.4f, 13), leaf); Box("Clipped box hedge", p + new Vector3(0, .6f, s * 6), new Vector3(19, 1.4f, .9f), leaf); }
            Flowers(p, 17, 10, 180, true);
        }
        Fountain(new Vector3(0, y + .3f, 178), 1.5f);
        for (int i = 0; i < 6; i++) foreach (float side in new[] { -1f, 1f }) Lantern(new Vector3(side * 8, y, 146 + i * 10));
        // Rock escarpment is wide and textured; falling water starts at the terrace.
        for (int i = 0; i < 20; i++)
        {
            float x = -101 + i * 10.5f; float h = 16 + Random.value * 13;
            var r = Asset("Nature", "rock_tall" + "ABCDEFGHIJ"[i % 10], new Vector3(x, -3, 115 + Mathf.Abs(x) * .1f), 1);
            if (r) { Fit(r, new Vector3(19, h + 15, 25)); }
        }
        foreach (float x in new[] { -65f, -79f, 67f }) Waterfall(new Vector3(x, y - 1, 118), 6, 33);
    }
    static void Roof(Vector3 p, float w, float d, float h, Material mat, Transform parent = null)
    {
        // Two sloped roof faces, end gables, projecting eaves and ridge cap.
        var v = new[] { new Vector3(-w/2,0,-d/2),new Vector3(w/2,0,-d/2),new Vector3(0,h,-d/2),new Vector3(-w/2,0,d/2),new Vector3(w/2,0,d/2),new Vector3(0,h,d/2), new Vector3(-w/2,0,-d/2),new Vector3(0,h,-d/2),new Vector3(0,h,d/2),new Vector3(-w/2,0,d/2), new Vector3(0,h,-d/2),new Vector3(w/2,0,-d/2),new Vector3(w/2,0,d/2),new Vector3(0,h,d/2) };
        var tr = new[] { 0,2,1,3,4,5,6,9,7,7,9,8,10,13,11,11,13,12 };
        var uv = v.Select(q => new Vector2((q.x + q.y) * .25f, q.z * .25f)).ToArray();
        mat.SetFloat("_Cull",0);var m = new Mesh { vertices = v, triangles = tr, uv = uv }; MeshObject("Steep tiled roof", SaveMesh(m, "Gabled roof"), mat, p, false, parent);
        Box("Ridge cap", p + Vector3.up * h, new Vector3(.3f, .3f, d + .3f), mat, false, parent);
        foreach (float s in new[] { -1f, 1f }) Box("Deep eave", p + new Vector3(s * w/2, -.15f, 0), new Vector3(.3f, .4f, d), mat, false, parent);
    }
    static void Banner(Vector3 p, float width, float height)
    {
        Box("Heraldic hanging banner", p - Vector3.up * height / 2, new Vector3(width, height, .09f), cloth);
        foreach (float side in new[] { -1f, 1f }) Box("Woven gold edge", p + new Vector3(side * (width / 2 - .14f), -height / 2, -.07f), new Vector3(.1f, height, .03f), gold);
        var emblem = Box("Diamond heraldry", p - Vector3.up * height * .42f + Vector3.back * .1f, new Vector3(width * .33f, width * .33f, .06f), gold); emblem.transform.rotation = Quaternion.Euler(0, 0, 45);
    }
    static void Arch(Vector3 p, float width, float height, float depth, bool collision)
    {
        float r = width / 2; float baseY = height - r;
        for (int i = 0; i < 18; i++)
        {
            float a = i * Mathf.PI / 18, b = (i + 1) * Mathf.PI / 18;
            var v = new List<Vector3>();
            foreach (float z in new[] { -depth / 2, depth / 2 }) foreach (float rr in new[] { r, r + 2 }) foreach (float angle in new[] { a, b }) v.Add(new Vector3(Mathf.Cos(angle) * rr, baseY + Mathf.Sin(angle) * rr, z));
            int[] tri = {0,1,2,1,3,2,4,6,5,5,6,7,0,4,1,1,4,5,2,3,6,3,7,6,0,2,4,2,6,4,1,5,3,3,5,7};
            var m = new Mesh { vertices = v.ToArray(), triangles = tri, uv = v.Select(q => new Vector2(q.x * .2f, q.y * .2f)).ToArray() };
            MeshObject("Arch voussoir", SaveMesh(m, "Arch stone"), i % 3 == 0 ? trim : limestone, p, collision);
        }
        foreach (float s in new[] { -1f, 1f }) Box("Arch pier", p + new Vector3(s * (r + 1), baseY / 2, 0), new Vector3(2, baseY, depth), limestone, collision);
    }
    static float BridgeY(float z) => Mathf.Lerp(3, 34, Mathf.InverseLerp(-28, 136, z));
    static void Viaduct()
    {
        // Continuous ramped deck and arched supports, with an unobstructed walking lane.
        float length = 164, rise = 31; float angle = -Mathf.Atan2(rise, length) * Mathf.Rad2Deg;
        var deck = Box("Continuous viaduct deck", new Vector3(0, 18.5f, 54), new Vector3(13, 1.4f, Mathf.Sqrt(length * length + rise * rise)), limestone, true); deck.transform.rotation = Quaternion.Euler(angle, 0, 0);
        var top = Box("Viaduct paving", new Vector3(0, 19.24f, 54), new Vector3(11.8f, .12f, Mathf.Sqrt(length * length + rise * rise)), road); top.transform.rotation = deck.transform.rotation;
        for (int i = 0; i < 9; i++)
        {
            float z = -20 + i * 18.1f, y = BridgeY(z);
            foreach (float side in new[] { -1f, 1f })
            {
                Box("Stone balustrade", new Vector3(side * 6.05f, y + 1.65f, z + 7.5f), new Vector3(.65f, 1.7f, 17.8f), limestone, true).transform.rotation = deck.transform.rotation;
                Box("Coping", new Vector3(side * 6.05f, y + 2.6f, z + 7.5f), new Vector3(.85f, .2f, 18), trim).transform.rotation = deck.transform.rotation;
                Box("Bridge buttress", new Vector3(side * 5, y / 2 - 1, z), new Vector3(3.2f, y + 4, 3.5f), limestone, true);
                Lantern(new Vector3(side * 6, y + .6f, z), .8f);
            }
            if (i > 1) Arch(new Vector3(0, -4, z + 8), 10, y + 1, 2, false);
        }
        // Sparkling foam at the waterfall basins.
        for (int i = 0; i < 80; i++)
        {
            float x = Random.Range(-115, 115), z = 35 + Mathf.Sin(x * .013f) * 13 + Random.Range(-10, 10);
            Box("Water glint", new Vector3(x, -.22f, z), new Vector3(Random.Range(.2f, 1.4f), .015f, .08f), Mat("Water foam", "#b9e2d7", .5f, 0, .3f));
        }
    }
    static void Waterfall(Vector3 top, float width, float height)
    {
        var mat = Mat("Falling silvery water", "#9bd7d7", .18f, .1f, .4f); mat.mainTexture = water.mainTexture; mat.mainTextureScale = new Vector2(2, 10);
        var f = Box("Waterfall veil", top - Vector3.up * height / 2, new Vector3(width, height, .35f), mat);
        f.AddComponent<ValleyMotion>().kind = ValleyMotion.Kind.Water;
        for (int i = 0; i < 9; i++) { float x = (i / 8f - .5f) * width; Box("Whitewater strand", top + new Vector3(x, -height / 2, -.22f), new Vector3(.1f + Random.value * .2f, height, .12f), Mat("Mist white", "#d4eee0", .5f, 0, .5f)); }
        var particles = Node("Waterfall spray", top - Vector3.up * (height - .5f)).AddComponent<ParticleSystem>();
        var main = particles.main; main.startLifetime = 3; main.startSpeed = 1.2f; main.startSize = 2.2f; main.startColor = new Color(.8f, .94f, .94f, .3f); main.maxParticles = 100; main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = particles.emission; emission.rateOverTime = 22; var shape = particles.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = width * .6f;
        particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial();
    }
    static Material ParticleMaterial()
    {
        if (materials.TryGetValue("Soft luminous motes", out var m)) return m;
        m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")); m.name = "Soft luminous motes"; m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0); m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.SetFloat("_ZWrite", 0); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = 3000;
        var tex = new Texture2D(64,64); for(int y=0;y<64;y++) for(int x=0;x<64;x++){float a=Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f);tex.SetPixel(x,y,new Color(1,1,1,a*a));} tex.Apply();
        File.WriteAllBytes(Gen + "/Textures/soft-particle.png", tex.EncodeToPNG()); AssetDatabase.ImportAsset(Gen + "/Textures/soft-particle.png"); m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Gen + "/Textures/soft-particle.png");
        AssetDatabase.CreateAsset(m, AssetDatabase.GenerateUniqueAssetPath(Gen + "/Materials/Soft motes.mat")); materials[m.name] = m; return m;
    }
    static void Village()
    {
        // Dense, irregular street walls and warm shop fronts frame the distant castle.
        for (int i = 0; i < 7; i++) foreach (int side in new[] { -1, 1 })
        {
            float x = side * (15 + Random.value * 4), z = -145 + i * 14;
            House(new Vector3(x, Height(x, z), z), 9 + Random.value * 3, 10 + Random.value * 6, 10 + Random.value * 3, side == 1 ? 270 : 90, i);
        }
        for (int i = 0; i < 7; i++)
        {
            float z = -141 + i * 15;
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * 6.7f; var stall = Asset("FantasyTown", i % 2 == 0 ? "stall-red" : "stall-green", new Vector3(x, Height(x, z), z), 1);
                if (stall) { Fit(stall, new Vector3(4.2f, 3.2f, 3)); stall.transform.rotation = Quaternion.Euler(0, side == 1 ? 270 : 90, 0); }
                for (int j = 0; j < 8; j++) Ball("Market apples", new Vector3(x + Random.Range(-1.4f,1.4f), Height(x,z)+1.2f,z+Random.Range(-.7f,.7f)), Vector3.one*.28f, j%3==0 ? pink : gold);
            }
            Lantern(new Vector3(-8, Height(-8,z), z)); Lantern(new Vector3(8, Height(8,z), z));
            // Sagging lantern strings overhead.
            for (int j = 0; j < 11; j++)
            {
                float x = -12 + j * 2.4f, y = 8 + Mathf.Pow(Mathf.Abs(x) / 12, 2) * 2;
                Beam(new Vector3(x,y,z),new Vector3(x+2.4f,8+Mathf.Pow(Mathf.Abs(x+2.4f)/12,2)*2,z),.04f,wood);
                Box("Hanging amber lantern",new Vector3(x,y-.55f,z),new Vector3(.4f,.65f,.4f),glow);
                Box("Lantern cap",new Vector3(x,y-.16f,z),new Vector3(.55f,.12f,.55f),wood);
            }
        }
        Fountain(new Vector3(-35, Height(-35,-85), -85), 1);
        var cart = Asset("FantasyTown", "cart-high", new Vector3(10,Height(10,-155),-155),1.5f);
        // Quayside garden, small boat and wooden dock.
        float dockY = Height(-52,-7);
        Box("River overlook deck",new Vector3(-55,dockY,-4),new Vector3(17,.5f,12),wood,true);
        for(int i=0;i<12;i++) Box("Dock plank",new Vector3(-63+i*1.5f,dockY+.3f,-4),new Vector3(1.45f,.15f,12),wood);
        foreach(float x in new[]{-63f,-47f}) foreach(float z in new[]{-10f,2f}) Cylinder("Dock post",new Vector3(x,dockY/2-1,z),.2f,dockY+4,wood);
        Asset("Nature","canoe",new Vector3(-62,-.1f,23),3);
    }
    static void House(Vector3 p, float width, float height, float depth, float yaw, int seed)
    {
        var g = Node("Briarwood • timber town house",p).transform; g.rotation=Quaternion.Euler(0,yaw,0);
        var stucco=Mat("Plaster "+seed%4,new[]{"#d6b99a","#d7cdb1","#b7c1b2","#d3ad94"}[seed%4]);
        Box("Stone footing",new Vector3(0,1,0),new Vector3(width+.5f,2,depth+.5f),limestone,true,g);
        Box("Lime plaster walls",new Vector3(0,height/2,0),new Vector3(width,height,depth),stucco,true,g);
        Roof(new Vector3(0,height,0),width+1.5f,depth+1.6f,5.5f,seed%3==0?Mat("Terracotta tiles","#8d584a"):slate,g);
        foreach(float s in new[]{-1f,1f})
        {
            foreach(float x in new[]{-width/2+.2f,0,width/2-.2f}) Box("Exposed oak upright",new Vector3(x,height/2,s*(depth/2+.08f)),new Vector3(.25f,height,.24f),wood,false,g);
            for(float y=2;y<height;y+=4) Box("Floor beam",new Vector3(0,y,s*(depth/2+.12f)),new Vector3(width+.25f,.3f,.3f),wood,false,g);
            for(float y=4;y<height-1;y+=4.5f) foreach(float x in new[]{-width*.27f,width*.27f})
            {
                var win=Node("Leaded warm window",new Vector3(x,y,s*(depth/2+.16f)),g).transform; win.localRotation=Quaternion.Euler(0,s<0?180:0,0); ArchWindow(win,1.25f,1.8f);
                Box("Window candle",new Vector3(x,y+.8f,s*(depth/2+.24f)),new Vector3(.85f,1.2f,.06f),Mat("Warm windows","#e8a663",.5f,0,1.1f),false,g);
                Box("Flower box",new Vector3(x,y-.2f,s*(depth/2+.45f)),new Vector3(2,.4f,.7f),wood,false,g);
                for(int f=0;f<6;f++) Ball("Balcony flowers",new Vector3(x-.8f+f*.32f,y+.12f,s*(depth/2+.5f)),new Vector3(.4f,.4f,.45f),f%2==0?pink:leaf,g);
            }
            Beam(new Vector3(-width/2,height,s*depth/2),new Vector3(0,height+5.2f,s*depth/2),.25f,wood,g);
            Beam(new Vector3(width/2,height,s*depth/2),new Vector3(0,height+5.2f,s*depth/2),.25f,wood,g);
        }
        Box("Oak door",new Vector3(0,1.7f,depth/2+.14f),new Vector3(1.8f,3.3f,.22f),wood,false,g);
        Box("Doorstep",new Vector3(0,.15f,depth/2+.6f),new Vector3(2.6f,.3f,1.4f),trim,false,g);
        Box("Chimney",new Vector3(width*.25f,height+5.4f,-depth*.2f),new Vector3(1.3f,5,1.3f),limestone,false,g);
        Box("Chimney crown",new Vector3(width*.25f,height+8,-depth*.2f),new Vector3(1.7f,.4f,1.7f),trim,false,g);
        // Cantilevered upper balcony and awning.
        Box("Balcony floor",new Vector3(0,5,depth/2+1),new Vector3(width*.7f,.25f,2),wood,false,g);
        for(int j=0;j<8;j++) Box("Balcony spindle",new Vector3((j/7f-.5f)*width*.7f,5.7f,depth/2+1.9f),new Vector3(.12f,1.4f,.12f),wood,false,g);
        Box("Balcony rail",new Vector3(0,6.4f,depth/2+1.9f),new Vector3(width*.75f,.15f,.2f),wood,false,g);
        var awning=Box("Shop canvas",new Vector3(0,4.4f,depth/2+1.25f),new Vector3(width*.8f,.12f,3),seed%2==0?cloth:Mat("Sage canvas","#638a7d"),false,g); awning.transform.localRotation=Quaternion.Euler(12,0,0);
        foreach(float x in new[]{-width*.4f,width*.4f}) Box("Awning post",new Vector3(x,2,depth/2+2.5f),new Vector3(.1f,4,.1f),wood,false,g);
    }
    static void Lantern(Vector3 p,float scale=1)
    {
        var g=Node("Iron lantern",p).transform;g.localScale=Vector3.one*scale;
        Cylinder("Forged post",new Vector3(0,2.4f,0),.07f,4.8f,wood,false,g);
        Box("Lantern housing",new Vector3(0,4.3f,0),new Vector3(.7f,1,.7f),gold,false,g);
        Box("Amber glass",new Vector3(0,4.35f,0),new Vector3(.56f,.78f,.56f),glow,false,g);
        MeshObject("Lantern roof",Lathe("Lantern roof",new[]{new Vector2(.55f,0),new Vector2(.1f,.5f)}),slate,new Vector3(0,4.85f,0),false,g);
        var light=Node("Candle glow",new Vector3(0,4.2f,0),g).AddComponent<Light>(); light.type=LightType.Point;light.color=Hex("#ffb557");light.intensity=2;light.range=5;light.shadows=LightShadows.None;
    }
    static void Fountain(Vector3 p,float s)
    {
        var g=Node("Carved garden fountain",p).transform;g.localScale=Vector3.one*s;
        MeshObject("Fountain basin",Lathe("Fountain bowl",new[]{new Vector2(4,0),new Vector2(4,.7f),new Vector2(3.7f,1),new Vector2(3.3f,.7f),new Vector2(3.3f,.3f)}),trim,Vector3.zero,true,g);
        Cylinder("Still pool",new Vector3(0,.65f,0),3.4f,.08f,water,false,g);
        MeshObject("Tiered fountain",Lathe("Fountain tiers",new[]{new Vector2(.7f,0),new Vector2(.5f,2),new Vector2(2,2.1f),new Vector2(2,2.4f),new Vector2(.4f,2.5f),new Vector2(.3f,4),new Vector2(1,4.1f),new Vector2(1,4.3f),new Vector2(.1f,4.5f)}),limestone,Vector3.zero,false,g);
        Ball("Fountain finial",new Vector3(0,4.7f,0),Vector3.one*.45f,gold,g);
        for(int j=0;j<12;j++){float a=j*Mathf.PI/6;Beam(new Vector3(Mathf.Sin(a)*1.9f,2.4f,Mathf.Cos(a)*1.9f),new Vector3(Mathf.Sin(a)*2.6f,.7f,Mathf.Cos(a)*2.6f),.045f,Mat("Fountain droplets","#b1e2da",.1f,0,.5f),g);}
    }
    static GameObject Asset(string pack,string name,Vector3 p,float scale)
    {
        var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/"+pack+"/"+name+".fbx");if(!model)return null;
        var g=(GameObject)PrefabUtility.InstantiatePrefab(model);g.transform.SetParent(area);g.transform.position=p;g.transform.localScale=Vector3.one*scale;
        if(pack=="StylizedNature")foreach(var r in g.GetComponentsInChildren<Renderer>())
        {
            r.sharedMaterials=r.sharedMaterials.Select(source=>NatureMaterial(source.name)).ToArray();
        }
        if(pack=="Nature")foreach(var r in g.GetComponentsInChildren<Renderer>())
        {
            r.sharedMaterials=r.sharedMaterials.Select(source=>name.StartsWith("rock")?rock:source).ToArray();
        }
        return g;
    }
    static Material NatureMaterial(string name)
    {
        string key="Textured "+name;if(materials.TryGetValue(key,out var m))return m;
        m=Mat(key,"#ffffff",.85f);string textureName=name;
        if(name.Contains("Flower"))textureName="Flowers";
        if(name.Contains("Grass"))textureName="Grass";
        if(name.Contains("Bush"))textureName="Bush_Leaves";
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/StylizedNature/"+textureName+".png");
        m.mainTexture=texture;
        if(name.Contains("Leaves")||name.Contains("Grass")||name.Contains("Flower")||name.Contains("Bush"))
        {m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.45f);m.SetFloat("_Cull",0);m.EnableKeyword("_ALPHATEST_ON");m.SetOverrideTag("RenderType","TransparentCutout");m.renderQueue=2450;}
        if(name.Contains("Bark"))m.color=new Color(.75f,.8f,.75f);
        return m;
    }
    public static void OptimizeTextures()
    {
        foreach(var path in Directory.GetFiles("Assets/Art/StylizedNature","*.png"))
        {var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=1024;importer.alphaIsTransparency=true;if(path.Contains("_Normal"))importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();}
    }
    static void Fit(GameObject g,Vector3 desired)
    { var renderers=g.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return;var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);g.transform.localScale*=desired.y/Mathf.Max(.001f,b.size.y); }
    static void Grove()
    {
        var moss=Mat("Ancient moss","#5a7d48");var lavender=Mat("Lavender canopy","#a19bb8");var teal=Mat("Turquoise canopy","#629e94");
        // Cathedral-scale trunks and roots; the trail stays open beneath the canopy.
        for(int i=0;i<21;i++)
        {
            float a=i*Mathf.PI*2/21;float x=128+Mathf.Cos(a)*Random.Range(20,42),z=-39+Mathf.Sin(a)*Random.Range(27,55);float y=Height(x,z);float h=Random.Range(29,49);float r=Random.Range(1.4f,2.5f);
            var detailed=Asset("StylizedNature",i%3==0?"BirchTree_1":i%3==1?"NormalTree_1":"MapleTree_1",new Vector3(x,y,z),1);
            if(detailed){Fit(detailed,new Vector3(h*.65f,h,h*.65f));detailed.transform.rotation=Quaternion.Euler(0,Random.value*360,0)*detailed.transform.rotation;TreeCollider(detailed,new Vector3(x,y,z),r,h*.65f);continue;}
            var g=Node("Ancient grove tree",new Vector3(x,y,z)).transform;
            MeshObject("Fluted ancient trunk",Lathe("Ancient trunk"+i,new[]{new Vector2(r*1.6f,0),new Vector2(r,4),new Vector2(r*.7f,h*.6f),new Vector2(r*.35f,h)}),trunk,Vector3.zero,true,g);
            for(int j=0;j<5;j++){float aa=j*Mathf.PI*2/5;Vector3 end=new Vector3(Mathf.Sin(aa)*7,.2f,Mathf.Cos(aa)*7);Beam(new Vector3(0,2,0),end,.7f,trunk,g,"Buttress root");}
            for(int j=0;j<6;j++)
            { Vector3 off=new Vector3(Random.Range(-7,7),h+Random.Range(-4,5),Random.Range(-7,7)); Beam(new Vector3(0,h*.65f,0),off,.6f,trunk,g,"Crown branch");Ball("Layered canopy",off,new Vector3(12,6,12),i%3==0?lavender:i%3==1?teal:leaf,g); }
            if(i%2==0)for(int j=0;j<5;j++) { Vector3 v=new Vector3(Random.Range(-7,7),h-5,Random.Range(-7,7)); Beam(v,v+Vector3.down*Random.Range(6,13),.09f,moss,g,"Hanging vine"); }
        }
        // Friendly moss guardian, deliberately part of the scenery in this world-first build.
        var guardian=Node("Sleeping moss guardian",new Vector3(145,Height(145,-26),-26)).transform;
        Ball("Stone torso",new Vector3(0,4.6f,0),new Vector3(5,6,3.5f),rock,guardian);Ball("Guardian head",new Vector3(0,8.7f,0),new Vector3(3.6f,3.4f,3),rock,guardian);
        foreach(float side in new[]{-1f,1f}){Ball("Stone leg",new Vector3(side*1.4f,1.6f,0),new Vector3(1.8f,3.8f,2),rock,guardian);Ball("Stone arm",new Vector3(side*3,4.8f,0),new Vector3(1.8f,5,2),rock,guardian);Box("Gentle blue eye",new Vector3(side*.7f,9,-1.5f),new Vector3(.6f,.2f,.08f),Mat("Guardian eyes","#75e4e6",.5f,0,3),false,guardian);Ball("Shoulder moss",new Vector3(side*2.5f,7.1f,0),new Vector3(2.5f,.9f,2.5f),moss,guardian);}
        Ball("Moss crown",new Vector3(0,10.3f,0),new Vector3(4,1,3.6f),moss,guardian);
        for(int i=0;i<65;i++){float x=Random.Range(88,171),z=Random.Range(-85,1);if(Mathf.Abs(z+55)<4)continue;var g=Asset("Nature",i%2==0?"mushroom_redGroup":"mushroom_redTall",new Vector3(x,Height(x,z),z),Random.Range(1,3));}
        var motes=Node("Grove fireflies",new Vector3(128,4,-39)).AddComponent<ParticleSystem>();var main=motes.main;main.startLifetime=12;main.startSpeed=.12f;main.startSize=.13f;main.startColor=new Color(.45f,1,.83f);main.maxParticles=160;var shape=motes.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(70,9,80);motes.GetComponent<ParticleSystemRenderer>().sharedMaterial=ParticleMaterial();
        Flowers(new Vector3(128,0,-39),65,75,1300,false);
    }
    static void Vegetation()
    {
        for(int i=0;i<420;i++)
        {
            float x=Random.Range(-280,280),z=Random.Range(-250,280);
            if(Mathf.Abs(x)<60 && z>-170 && z<260)continue;if(x>75&&x<180&&z>-100&&z<20)continue;if(Mathf.Abs(z-35-Mathf.Sin(x*.013f)*13)<22)continue;
            string name=new[]{"NormalTree_1","NormalTree_2","NormalTree_3","BirchTree_1","MapleTree_1"}[i%5];
            var g=Asset("StylizedNature",name,new Vector3(x,Height(x,z),z),1);if(!g)continue;
            float h=Random.Range(9,20);Fit(g,new Vector3(h*.68f,h,h*.68f));g.transform.rotation=Quaternion.Euler(0,Random.value*360,0)*g.transform.rotation;
            var bounds=new Bounds();foreach(var r in g.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
            TreeCollider(g,new Vector3(x,Height(x,z),z),.45f,5);
        }
        for(int z=-220;z<120;z+=40)foreach(float side in new[]{-1f,1f})Flowers(new Vector3(side*Random.Range(22,78),0,z),48,43,700,false);
        // Flower-lined road; no uniformly empty flat verge.
        for(int z=-210;z<-30;z+=20)foreach(float s in new[]{-1f,1f})Flowers(new Vector3(s*5.7f,0,z),2.5f,20,170,false);
        for(int i=0;i<110;i++){float x=Random.Range(-190,190),z=Random.Range(-220,130);if(Mathf.Abs(x)<48||Mathf.Abs(z-35)<26)continue;var g=Asset("Nature","rock_large"+"ABCDEF"[i%6],new Vector3(x,Height(x,z)-.2f,z),Random.Range(1,3));}
        for(int i=0;i<260;i++){float x=Random.Range(-160,175),z=Random.Range(-220,110);if(Mathf.Abs(x)<10||Mathf.Abs(z-35)<25||(Mathf.Abs(x)<25&&z>-160&&z<-40))continue;Asset("StylizedNature",i%3==0?"Bush_Flowers":"Bush_Large_Flowers",new Vector3(x,Height(x,z),z),Random.Range(1.2f,2.7f));}
    }
    static void Flowers(Vector3 center,float width,float depth,int count,bool fixedY)
    {
        var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();var colors=new List<Color>();
        Color[] palette={Hex("#d695b1"),Hex("#e9cf8e"),Hex("#929dc3"),Hex("#eee3c3"),Hex("#a97bb4")};
        for(int i=0;i<count;i++)
        {
            float x=center.x+Random.Range(-width/2,width/2),z=center.z+Random.Range(-depth/2,depth/2),y=fixedY?center.y+.5f:Height(x,z)+.05f;
            if(!fixedY && (Mathf.Abs(x)<4.8f || Mathf.Abs(z-35-Mathf.Sin(x*.013f)*13)<20))continue;
            float h=Random.Range(.28f,.9f);Vector3 p=new Vector3(x,y,z);Color flower=palette[i%palette.Length];
            // Upright leaf blades plus horizontal five-petal rosettes, vertex-coloured.
            for(int j=0;j<3;j++){float a=j*Mathf.PI/3+Random.value;Vector3 side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.13f;int k=v.Count;v.AddRange(new[]{p-side,p+side,p+Vector3.up*h});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.up});colors.AddRange(Enumerable.Repeat(Hex("#66853e"),3));tri.AddRange(new[]{k,k+2,k+1,k+1,k+2,k});}
            if(i%4!=0)for(int j=0;j<5;j++){float a=j*Mathf.PI*2/5;Vector3 c=p+Vector3.up*h;Vector3 dir=new Vector3(Mathf.Cos(a),.1f,Mathf.Sin(a));Vector3 side=Vector3.Cross(dir,Vector3.up)*.065f;int k=v.Count;v.AddRange(new[]{c,c+dir*.2f+side,c+dir*.28f,c+dir*.2f-side});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});colors.AddRange(Enumerable.Repeat(flower,4));tri.AddRange(new[]{k,k+1,k+2,k,k+2,k+3,k+2,k+1,k,k+3,k+2,k});}
        }
        var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);
        var mat=materials.TryGetValue("Wildflower petals",out var m)?m:new Material(Shader.Find("Aurelia/Meadow"));
        if(!materials.ContainsKey("Wildflower petals")){mat.name="Wildflower petals";AssetDatabase.CreateAsset(mat,AssetDatabase.GenerateUniqueAssetPath(Gen+"/Materials/Flowers.mat"));materials[mat.name]=mat;}
        MeshObject("Wildflower ribbon",SaveMesh(mesh,"Flowers"),mat,Vector3.zero);
    }
    static void TreeCollider(GameObject tree,Vector3 p,float radius,float h)
    {var g=Node("Tree trunk collision",p);var c=g.AddComponent<CapsuleCollider>();c.radius=radius;c.height=h;c.center=Vector3.up*h/2;}
    static void DistanceScenery()
    {
        var mountain=Mat("Distant alpine blue","#79959e");var snow=Mat("Summit snow","#dce6dc");
        for(int i=0;i<24;i++)
        {
            float a=i*Mathf.PI*2/31;Vector3 p=new Vector3(Mathf.Sin(a)*520,-5,Mathf.Cos(a)*520);float h=Random.Range(100,245),r=Random.Range(45,88);
            Mountain(p,r*1.9f,h,i);
        }
        var cloud=Mat("Sunlit cloud","#e9eadd");
        for(int i=0;i<0;i++)
        {
            Vector3 p=new Vector3(Random.Range(-650,650),Random.Range(100,195),Random.Range(200,650));var g=Node("Towering cumulus",p).transform;
            for(int j=0;j<9;j++)Ball("Cloud billow",new Vector3(Random.Range(-35,35),Random.Range(-8,22),Random.Range(-16,16)),new Vector3(Random.Range(25,55),Random.Range(20,42),Random.Range(22,40)),cloud,g);
            g.gameObject.AddComponent<ValleyMotion>().kind=ValleyMotion.Kind.Cloud;
        }
    }
    static void Mountain(Vector3 p,float radius,float h,int seed)
    {
        int rings=18,sides=64;var v=new List<Vector3>();var colors=new List<Color>();var tri=new List<int>();
        for(int j=0;j<=rings;j++)for(int i=0;i<=sides;i++)
        {
            float a=i*Mathf.PI*2/sides;float t=j/(float)rings;float r=radius*t;float ridge=.7f+Mathf.PerlinNoise(Mathf.Cos(a)*2+seed*9,Mathf.Sin(a)*2+seed*4)*.5f;
            float yy=h*Mathf.Pow(1-t,1.7f)*ridge;yy+=Mathf.PerlinNoise(Mathf.Cos(a)*t*6+seed,Mathf.Sin(a)*t*6+seed)*h*.14f*(1-t);
            v.Add(new Vector3(Mathf.Cos(a)*r,yy,Mathf.Sin(a)*r));colors.Add(Color.Lerp(Hex("#6d8791"),Hex("#c9dedc"),Mathf.SmoothStep(.58f,.85f,yy/h)));
            if(j<rings&&i<sides){int k=j*(sides+1)+i;tri.AddRange(new[]{k,k+1,k+sides+1,k+1,k+sides+2,k+sides+1});}
        }
        var mesh=new Mesh();mesh.SetVertices(v);mesh.SetColors(colors);mesh.SetTriangles(tri,0);var mat=materials.TryGetValue("Alpine faces",out var m)?m:new Material(Shader.Find("Aurelia/Meadow"));if(!materials.ContainsKey("Alpine faces")){mat.name="Alpine faces";materials[mat.name]=mat;AssetDatabase.CreateAsset(mat,AssetDatabase.GenerateUniqueAssetPath(Gen+"/Materials/Mountains.mat"));}MeshObject("Eroded alpine massif",SaveMesh(mesh,"Alpine massif"),mat,p);
    }
    static void Dragon()
    {
        var g=Node("Red dragon above Aurelia",new Vector3(-30,115,215)).transform;
        var scales=Mat("Dragon oxblood scales","#813e4e");var membrane=Mat("Dragon wing membrane","#b66a70");
        Ball("Powerful body",Vector3.zero,new Vector3(3,3,8),scales,g);Ball("Long neck",new Vector3(0,.7f,4.5f),new Vector3(1.7f,2,5),scales,g);Ball("Dragon skull",new Vector3(0,1.3f,7.2f),new Vector3(2,1.4f,3),scales,g);
        foreach(float s in new[]{-1f,1f}){Beam(new Vector3(s*.7f,1.9f,6.7f),new Vector3(s*1.3f,3.1f,5.9f),.22f,gold,g,"Dragon horn");Ball("Eye",new Vector3(s*.95f,1.7f,7.7f),Vector3.one*.22f,glow,g);Beam(new Vector3(s*1.1f,-.7f,0),new Vector3(s*1.7f,-2.5f,1),.6f,scales,g,"Hind leg");}
        for(int j=0;j<9;j++){float t=j/8f;Ball("Tapering tail",new Vector3(Mathf.Sin(t*2)*2,-t,-4-t*9),Vector3.one*Mathf.Lerp(2,.15f,t),scales,g);}
        foreach(float side in new[]{-1f,1f})
        {
            Vector3[] v={new Vector3(side*1,1,1),new Vector3(side*5,4,2),new Vector3(side*13,2,-2),new Vector3(side*10,1,-4),new Vector3(side*7,.3f,-3),new Vector3(side*5,0,-6),new Vector3(side*2,0,-3)};
            var tr=new[]{0,1,2,0,2,3,0,3,4,0,4,5,0,5,6,2,1,0,3,2,0,4,3,0,5,4,0,6,5,0};var mesh=new Mesh{vertices=v,triangles=tr,uv=v.Select(q=>new Vector2(q.x,q.z)).ToArray()};MeshObject("Broad leathery wing",SaveMesh(mesh,"Dragon wing"),membrane,Vector3.zero,false,g);
            foreach(int j in new[]{2,4,5})Beam(v[0],v[j],.15f,scales,g,"Wing finger");
        }
        g.gameObject.AddComponent<ValleyMotion>().kind=ValleyMotion.Kind.Dragon;
    }
    static void Explorer()
    {
        area=root;var p=Node("Traveller",new Vector3(0,Height(0,-175)+.2f,-175));p.layer=2;
        var cc=p.AddComponent<CharacterController>();cc.height=1.9f;cc.radius=.33f;cc.center=Vector3.up*.96f;cc.stepOffset=.35f;cc.slopeLimit=50;
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Knight/knight.gltf");
        if(!source)throw new Exception("Knight glTF has not imported. Refresh assets and rerun.");
        var avatar=(GameObject)PrefabUtility.InstantiatePrefab(source);avatar.name="Knight • Quaternius";avatar.transform.SetParent(p.transform);avatar.transform.localPosition=Vector3.zero;
        var bounds=new Bounds(avatar.transform.position,Vector3.zero);foreach(var r in avatar.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);float s=1.9f/Mathf.Max(.1f,bounds.size.y);avatar.transform.localScale=Vector3.one*s;avatar.transform.localPosition=Vector3.up*(avatar.transform.position.y-bounds.min.y)*s;
        foreach(var t in p.GetComponentsInChildren<Transform>())t.gameObject.layer=2;
        var camera=Node("Main Camera",new Vector3(0,4,-182)).AddComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=58;camera.nearClipPlane=.12f;camera.farClipPlane=1500;camera.allowHDR=true;camera.AdditionalData().renderPostProcessing=true;camera.AdditionalData().antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;camera.gameObject.AddComponent<AudioListener>();
        var explorer=p.AddComponent<ValleyExplorer>();explorer.avatar=avatar.transform;explorer.view=camera;explorer.sun=RenderSettings.sun;explorer.sky=RenderSettings.skybox;explorer.spawn=p.transform.position;
        camera.transform.LookAt(p.transform.position+Vector3.up*2);
    }
    static UniversalAdditionalCameraData AdditionalData(this Camera c)=>c.GetUniversalAdditionalCameraData();
    public static string Inspect()
    {
        var e=UnityEngine.Object.FindFirstObjectByType<ValleyExplorer>();var missing=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Count(r=>r.sharedMaterials.Any(m=>m==null||m.shader==null||m.shader.name=="Hidden/InternalErrorShader"));
        return "Scene="+EditorSceneManager.GetActiveScene().path+"; explorer="+(e!=null)+"; missingMaterials="+missing+"; meshes="+UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Length;
    }
    public static void Capture()
    {
        Directory.CreateDirectory("../artifacts/unity");var camera=Camera.main;var originalP=camera.transform.position;var originalR=camera.transform.rotation;
        var points=new[]{new Vector3(0,6,-175),new Vector3(-83,37,5),new Vector3(110,7,-76),new Vector3(-22,39,153)};
        var targets=new[]{new Vector3(0,28,145),new Vector3(0,61,200),new Vector3(143,20,-22),new Vector3(0,72,220)};
        var names=new[]{"village","castle-waterfalls","grove","gardens"};
        for(int i=0;i<points.Length;i++)
        {camera.transform.position=points[i];camera.transform.LookAt(targets[i]);var rt=new RenderTexture(1600,1000,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes("../artifacts/unity/"+names[i]+".png",tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
        camera.transform.SetPositionAndRotation(originalP,originalR);
    }
    public static void BuildWindows()
    {
        Directory.CreateDirectory("../Builds/Aurelia");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Aurelia.unity"},locationPathName="../Builds/Aurelia/Aurelia.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        File.WriteAllText("../artifacts/unity/build-result.txt",report.summary.result+"; errors="+report.summary.totalErrors+"; size="+report.summary.totalSize);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);
    }
}
