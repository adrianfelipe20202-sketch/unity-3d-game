using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Crea la escena Nivel2 y la agrega a Build Settings.
/// Menu: Semana9 > Crear Nivel 2
/// </summary>
public class ConfiguradorNivel2
{
    const string RUTA_ESCENA = "Assets/Scenes/Nivel2.unity";
    const float  MAPA_MITAD  = 28f;

    // Materiales reutilizados del Nivel 1 (misma carpeta Materials)
    static Material matSuelo2, matPared2, matPlataforma, matJugador2;
    static Material matEnemigo2, matMadera2, matMetal2, matColec2, matPiel2, matOjos2;
    static Material matGema2;

    // ── ENTRY POINT ────────────────────────────────────────────────────────
    [MenuItem("Semana9/Crear Nivel 2")]
    static void CrearNivel2()
    {
        // Guardar escena actual
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // Crear escena nueva vacía
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Construir contenido
        GenerarMateriales();
        ConfigurarAmbiente();
        CrearSuelo();
        CrearLimites();
        CrearLuces();
        CrearJugador();
        CrearNPC();
        CrearPlataformas();
        CrearPuerta();
        CrearCofre();
        CrearEnemigos();
        CrearPrefabColeccionable();
        CrearSpawner();
        CrearGema();
        CrearUI();

        // Guardar escena
        EditorSceneManager.SaveScene(escena, RUTA_ESCENA);

        // Agregar ambas escenas a Build Settings
        AgregarABuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== NIVEL 2 CREADO Y GUARDADO ===");
    }

    // ── BUILD SETTINGS ─────────────────────────────────────────────────────
    static void AgregarABuildSettings()
    {
        var lista = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        string[] rutas = {
            "Assets/Scenes/SampleScene.unity",
            RUTA_ESCENA
        };

        foreach (string ruta in rutas)
        {
            bool yaExiste = false;
            foreach (var s in lista)
                if (s.path == ruta) { yaExiste = true; break; }

            if (!yaExiste)
            {
                lista.Add(new EditorBuildSettingsScene(ruta, true));
                Debug.Log($"Agregado a Build Settings: {ruta}");
            }
        }

        EditorBuildSettings.scenes = lista.ToArray();
        Debug.Log("=== Build Settings actualizados: " + lista.Count + " escenas ===");
    }

    // ── MATERIALES ─────────────────────────────────────────────────────────
    static void GenerarMateriales()
    {
        // Textura de suelo oscuro (roca/piedra)
        Texture2D texSuelo = TexRoca(512, 512);
        GuardarTex(texSuelo, "TexturaSuelo2");
        AssetDatabase.Refresh();
        var ts2 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/TexturaSuelo2.png");

        Texture2D texPlat = TexPiedra(256, 256);
        GuardarTex(texPlat, "TexturaPlataforma");
        AssetDatabase.Refresh();
        var tp = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/TexturaPlataforma.png");
        var tm = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/TexturaMadera.png");

        matSuelo2     = Mat("MatSuelo2",      new Color(0.20f, 0.20f, 0.22f), ts2, 0f,  0.3f, 8f);
        matPared2     = Mat("MatPared2",      new Color(0.18f, 0.18f, 0.20f), null, 0f, 0.2f, 1f);
        matPlataforma = Mat("MatPlataforma",  Color.white,                    tp,  0f,  0.35f, 2f);
        matJugador2   = Mat("MatJugador",     new Color(0.15f, 0.35f, 0.90f), null, 0.1f,0.6f, 1f);
        matEnemigo2   = Mat("MatEnemigo2",    new Color(0.55f, 0.05f, 0.65f), null, 0.1f,0.4f, 1f); // violeta
        matMadera2    = Mat("MatMadera",      Color.white,                    tm,  0f,  0.3f, 1f);
        matMetal2     = Mat("MatMetal",       new Color(0.9f,  0.75f, 0.15f), null, 0.85f,0.85f,1f);
        matColec2     = Mat("MatColeccionable",new Color(1f,  0.85f, 0f),     null, 0.7f,0.95f,1f);
        matPiel2      = Mat("MatPiel",        new Color(0.95f, 0.78f, 0.58f), null, 0f,  0.3f, 1f);
        matOjos2      = Mat("MatOjos",        Color.black,                    null, 0f,  0.2f, 1f);
        matGema2      = MatEmisivo("MatGema2", new Color(0.9f, 0.2f, 1f),
                                   new Color(0.5f, 0.05f, 0.6f)); // violeta brillante
    }

    static Material Mat(string n, Color c, Texture2D tex, float met, float smo, float til)
    {
        string path = $"Assets/Materials/{n}.mat";
        Material m  = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader s    = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (m == null) { m = new Material(s); AssetDatabase.CreateAsset(m, path); }
        else m.shader = s;

        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c); else m.color = c;
        if (tex != null)
        {
            if (m.HasProperty("_BaseMap")) { m.SetTexture("_BaseMap", tex); m.SetTextureScale("_BaseMap", new Vector2(til, til)); }
            else { m.mainTexture = tex; m.mainTextureScale = new Vector2(til, til); }
        }
        if (m.HasProperty("_Metallic"))   m.SetFloat("_Metallic", met);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smo);
        else if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smo);
        EditorUtility.SetDirty(m);
        return m;
    }

    static Material MatEmisivo(string n, Color base_, Color emision)
    {
        Material m = Mat(n, base_, null, 0.3f, 0.9f, 1f);
        m.EnableKeyword("_EMISSION");
        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", emision);
        EditorUtility.SetDirty(m);
        return m;
    }

    // ── TEXTURAS ──────────────────────────────────────────────────────────
    static Texture2D TexRoca(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGB24, true);
        Color c1 = new Color(0.12f, 0.11f, 0.13f);
        Color c2 = new Color(0.28f, 0.27f, 0.30f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.03f, y * 0.03f)
                        + Mathf.PerlinNoise(x * 0.09f + 7f, y * 0.09f + 7f) * 0.3f;
                t.SetPixel(x, y, Color.Lerp(c1, c2, Mathf.Clamp01(n)));
            }
        t.Apply(); return t;
    }

    static Texture2D TexPiedra(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGB24, true);
        Color c1 = new Color(0.35f, 0.33f, 0.38f);
        Color c2 = new Color(0.52f, 0.50f, 0.54f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.04f, y * 0.04f)
                        + Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.25f;
                float jx = Mathf.Abs(Mathf.Sin(x * 0.28f)) * 0.06f;
                float jy = Mathf.Abs(Mathf.Sin(y * 0.38f)) * 0.06f;
                t.SetPixel(x, y, Color.Lerp(c1, c2, Mathf.Clamp01(n - jx - jy)));
            }
        t.Apply(); return t;
    }

    static void GuardarTex(Texture2D t, string n)
        => File.WriteAllBytes(Application.dataPath + $"/Materials/{n}.png", t.EncodeToPNG());

    // ── AMBIENTE ──────────────────────────────────────────────────────────
    static void ConfigurarAmbiente()
    {
        // Cielo de atardecer/oscuro para mayor dificultad
        Material sky = new Material(Shader.Find("Skybox/Procedural"));
        if (sky != null && sky.shader.name != "Hidden/InternalErrorShader")
        {
            sky.SetFloat("_SunSize", 0.06f);
            sky.SetFloat("_AtmosphereThickness", 0.9f);
            sky.SetColor("_SkyTint",   new Color(0.25f, 0.12f, 0.38f)); // violeta oscuro
            sky.SetColor("_GroundColor", new Color(0.10f, 0.08f, 0.12f));
            sky.SetFloat("_Exposure", 0.9f);
            AssetDatabase.CreateAsset(sky, "Assets/Materials/SkyboxNivel2.mat");
            RenderSettings.skybox = sky;
        }

        RenderSettings.ambientLight     = new Color(0.22f, 0.18f, 0.30f);
        RenderSettings.fog              = true;
        RenderSettings.fogColor         = new Color(0.18f, 0.10f, 0.28f);
        RenderSettings.fogMode          = FogMode.Linear;
        RenderSettings.fogStartDistance = 25f;
        RenderSettings.fogEndDistance   = 55f;
        DynamicGI.UpdateEnvironment();
    }

    // ── SUELO ─────────────────────────────────────────────────────────────
    static void CrearSuelo()
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Plane);
        s.name = "Suelo";
        s.transform.position = Vector3.zero;
        s.transform.localScale = new Vector3(6f, 1f, 6f);
        s.GetComponent<Renderer>().sharedMaterial = matSuelo2;
    }

    // ── LÍMITES ───────────────────────────────────────────────────────────
    static void CrearLimites()
    {
        var limites = new GameObject("LimitesMapa");
        float h = 5f, e = 1.5f, m = MAPA_MITAD;
        Muro(limites.transform, new Vector3(0, h/2f,  m), new Vector3(m*2f+e*2, h, e));
        Muro(limites.transform, new Vector3(0, h/2f, -m), new Vector3(m*2f+e*2, h, e));
        Muro(limites.transform, new Vector3( m, h/2f, 0), new Vector3(e, h, m*2f));
        Muro(limites.transform, new Vector3(-m, h/2f, 0), new Vector3(e, h, m*2f));
    }

    static void Muro(Transform p, Vector3 pos, Vector3 esc)
    {
        var m = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m.name = "Muro"; m.transform.SetParent(p);
        m.transform.position = pos; m.transform.localScale = esc;
        m.GetComponent<Renderer>().sharedMaterial = matPared2;
    }

    // ── LUCES ─────────────────────────────────────────────────────────────
    static void CrearLuces()
    {
        // Luz principal tenue (atardecer)
        var luzGo = new GameObject("LuzPrincipal");
        var luz = luzGo.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(1f, 0.65f, 0.40f); // naranja
        luz.intensity = 0.8f;
        luz.shadows = LightShadows.Soft;
        luzGo.transform.rotation = Quaternion.Euler(38f, -50f, 0f);

        // Luz de relleno azulada
        var rellGo = new GameObject("LuzRelleno");
        var rell = rellGo.AddComponent<Light>();
        rell.type = LightType.Directional;
        rell.color = new Color(0.35f, 0.40f, 0.80f);
        rell.intensity = 0.4f;
        rell.shadows = LightShadows.None;
        rellGo.transform.rotation = Quaternion.Euler(20f, 130f, 0f);
    }

    // ── JUGADOR ───────────────────────────────────────────────────────────
    static void CrearJugador()
    {
        var jugador = new GameObject("Jugador");
        jugador.tag = "Player";
        jugador.transform.position = new Vector3(0f, 0f, -18f);

        var cc = jugador.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.38f; cc.center = new Vector3(0f, 1f, 0f);
        var j = jugador.AddComponent<Jugador>();

        P(jugador.transform, "Cuerpo",  PrimitiveType.Capsule,
            new Vector3(0f, 1f, 0f), new Vector3(0.72f, 1f, 0.72f), matJugador2);
        P(jugador.transform, "Cabeza",  PrimitiveType.Sphere,
            new Vector3(0f, 2.18f, 0f), Vector3.one * 0.54f, matPiel2);
        P(jugador.transform, "OjoD",    PrimitiveType.Sphere,
            new Vector3( 0.12f, 2.26f, 0.24f), Vector3.one * 0.09f, matOjos2);
        P(jugador.transform, "OjoI",    PrimitiveType.Sphere,
            new Vector3(-0.12f, 2.26f, 0.24f), Vector3.one * 0.09f, matOjos2);

        var cam = new GameObject("MainCamera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>().fieldOfView = 75f;
        cam.AddComponent<AudioListener>();
        cam.transform.SetParent(jugador.transform);
        cam.transform.localPosition = new Vector3(0f, 2.08f, 0.10f);
        j.camara = cam.transform;
    }

    // ── NPC ───────────────────────────────────────────────────────────────
    static void CrearNPC()
    {
        var npc = new GameObject("NPC");
        npc.transform.position = new Vector3(-4f, 0f, -14f);
        var t = npc.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2.2f; t.center = new Vector3(0,1,0);

        var n = npc.AddComponent<NPC>();
        n.lineasDeDialogo = new string[]
        {
            "¡Bienvenido al nivel 2! Este lugar es más peligroso.",
            "Hay 2 enemigos aquí. Son más rápidos que los del primer nivel.",
            "Las plataformas te ayudarán a alcanzar el cofre en las alturas.",
            "Recoge 8 orbes dorados y la gema violeta para abrir el cofre."
        };

        P(npc.transform, "Cuerpo", PrimitiveType.Capsule,
            new Vector3(0,1,0), new Vector3(0.72f,1f,0.72f), matJugador2);
        P(npc.transform, "Cabeza", PrimitiveType.Sphere,
            new Vector3(0,2.18f,0), Vector3.one*0.54f, matPiel2);

        npc.transform.LookAt(new Vector3(0,0,-18f));
    }

    // ── PLATAFORMAS ───────────────────────────────────────────────────────
    static void CrearPlataformas()
    {
        // Plataformas flotantes a distintas alturas para llegar al cofre alto
        var plataformas = new (Vector3 pos, Vector3 esc)[]
        {
            (new Vector3( 4f, 2f,  2f), new Vector3(4f, 0.5f, 4f)),
            (new Vector3(-5f, 4f,  6f), new Vector3(4f, 0.5f, 4f)),
            (new Vector3( 6f, 6f, 10f), new Vector3(4f, 0.5f, 4f)),
            (new Vector3(-4f, 8f, 14f), new Vector3(4f, 0.5f, 4f)),
            (new Vector3( 0f,10f, 18f), new Vector3(5f, 0.5f, 5f)),  // final con cofre
            // Plataformas laterales alternativas
            (new Vector3(-10f,3f,  5f), new Vector3(3f, 0.5f, 3f)),
            (new Vector3( 10f,5f,  8f), new Vector3(3f, 0.5f, 3f)),
            (new Vector3(-8f, 7f, 13f), new Vector3(3f, 0.5f, 3f)),
        };

        var padre = new GameObject("Plataformas");
        for (int i = 0; i < plataformas.Length; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Plataforma_{i+1}";
            cube.transform.SetParent(padre.transform);
            cube.transform.position   = plataformas[i].pos;
            cube.transform.localScale = plataformas[i].esc;
            cube.GetComponent<Renderer>().sharedMaterial = matPlataforma;
        }
    }

    // ── PUERTA ────────────────────────────────────────────────────────────
    static void CrearPuerta()
    {
        var piv = new GameObject("Puerta");
        piv.transform.position = new Vector3(-0.5f, 0f, -5f);

        var t = piv.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2.5f; t.center = new Vector3(0.5f,1.5f,0);
        piv.AddComponent<Puerta>();

        P(piv.transform, "Panel", PrimitiveType.Cube,
            new Vector3(0.5f,1.5f,0), new Vector3(1f,3f,0.12f), matMadera2);
        Marco2(piv.transform, new Vector3(-0.07f,1.5f,0),  new Vector3(0.13f,3.15f,0.16f));
        Marco2(piv.transform, new Vector3( 1.07f,1.5f,0),  new Vector3(0.13f,3.15f,0.16f));
        Marco2(piv.transform, new Vector3(0.5f,3.07f,0),   new Vector3(1.28f,0.13f,0.16f));
        P(piv.transform, "Pomo", PrimitiveType.Sphere,
            new Vector3(0.82f,1.5f,0.09f), Vector3.one*0.12f, matMetal2);

        var col = piv.AddComponent<BoxCollider>();
        col.center = new Vector3(0.5f,1.5f,0); col.size = new Vector3(1f,3f,0.14f);
    }

    static void Marco2(Transform p, Vector3 pos, Vector3 esc)
        => P(p, "Marco", PrimitiveType.Cube, pos, esc, matMetal2);

    // ── COFRE (en lo alto de la última plataforma) ────────────────────────
    static void CrearCofre()
    {
        var cofre = new GameObject("CofreEscena");
        cofre.transform.position = new Vector3(0f, 10.5f, 18f); // encima plataforma final

        var t = cofre.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2f; t.center = new Vector3(0,0.5f,0);
        var c = cofre.AddComponent<Cofre>();
        c.itemsRequeridos = 8; // más difícil

        P(cofre.transform, "Base",  PrimitiveType.Cube, new Vector3(0,0.35f,0), new Vector3(1.1f,0.7f,0.75f), matMadera2);
        P(cofre.transform, "Tapa",  PrimitiveType.Cube, new Vector3(0,0.82f,0), new Vector3(1.1f,0.35f,0.75f), matMadera2);
        P(cofre.transform, "Franja",PrimitiveType.Cube, new Vector3(0,0.675f,0),new Vector3(1.12f,0.075f,0.77f),matMetal2);
        P(cofre.transform, "Lock",  PrimitiveType.Cube, new Vector3(0,0.38f,0.395f),new Vector3(0.22f,0.22f,0.065f),matMetal2);

        var col = cofre.AddComponent<BoxCollider>();
        col.center = new Vector3(0,0.5f,0); col.size = new Vector3(1.1f,1f,0.75f);
    }

    // ── ENEMIGOS (x2, más rápidos) ────────────────────────────────────────
    static void CrearEnemigos()
    {
        Material matCab = Mat("MatCabEnemigo2", new Color(0.38f,0.03f,0.48f), null, 0.1f,0.4f,1f);
        Material matOj  = Mat("MatOjosEn2",    new Color(1f,0.2f,0.2f),       null, 0.2f,0.9f,1f);

        CrearEnemigo(new Vector3( 8f, 0f, 5f),  3.5f, 15, matCab, matOj);
        CrearEnemigo(new Vector3(-9f, 0f, 10f), 3.5f, 15, matCab, matOj);
    }

    static void CrearEnemigo(Vector3 pos, float velocidad, int danio, Material matCab, Material matOj)
    {
        var en = new GameObject("Enemigo");
        en.transform.position = pos;

        var rb = en.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        var col = en.AddComponent<BoxCollider>();
        col.center = new Vector3(0,1,0); col.size = new Vector3(0.75f,2f,0.75f);

        var script = en.AddComponent<Enemigo>();
        // Velocidad y daño via SerializedObject para poder asignar campos privados
        var so = new SerializedObject(script);
        so.FindProperty("_velocidad").floatValue = velocidad;
        so.FindProperty("_danio").intValue       = danio;
        so.ApplyModifiedProperties();

        P(en.transform, "Cuerpo",   PrimitiveType.Cube,
            new Vector3(0,1,0),    new Vector3(0.75f,1.5f,0.75f), matEnemigo2);
        P(en.transform, "Cabeza",   PrimitiveType.Cube,
            new Vector3(0,2.05f,0), new Vector3(0.70f,0.70f,0.70f), matCab);
        P(en.transform, "OjoD",     PrimitiveType.Cube,
            new Vector3( 0.17f,2.12f,0.37f), new Vector3(0.18f,0.13f,0.05f), matOj);
        P(en.transform, "OjoI",     PrimitiveType.Cube,
            new Vector3(-0.17f,2.12f,0.37f), new Vector3(0.18f,0.13f,0.05f), matOj);
    }

    // ── PREFAB COLECCIONABLE ──────────────────────────────────────────────
    static void CrearPrefabColeccionable()
    {
        var e = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        e.name = "Coleccionable";
        e.transform.localScale = Vector3.one * 0.38f;
        e.GetComponent<Renderer>().sharedMaterial = matColec2;
        e.GetComponent<SphereCollider>().isTrigger = true;
        var rb = e.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;
        e.AddComponent<Coleccionable>();
        PrefabUtility.SaveAsPrefabAsset(e, "Assets/Prefabs/Coleccionable.prefab");
        Object.DestroyImmediate(e);
    }

    // ── SPAWNER ───────────────────────────────────────────────────────────
    static void CrearSpawner()
    {
        var sp = new GameObject("SpawnerItems");
        sp.transform.position = new Vector3(0f, 0f, 0f);
        var gen = sp.AddComponent<GeneradorItems>();
        gen.rangoSpawn = 20f;
        gen.maxItems   = 8;  // más items en nivel 2
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coleccionable.prefab");
        if (prefab != null) gen.prefabItem = prefab;
    }

    // ── GEMA ESPECIAL ─────────────────────────────────────────────────────
    static void CrearGema()
    {
        var gema = new GameObject("GemaEspecial");
        gema.transform.position = new Vector3(-10f, 9f, 14f); // en plataforma lateral alta

        var col = gema.AddComponent<SphereCollider>();
        col.isTrigger = true; col.radius = 0.5f;
        gema.AddComponent<ItemEspecial>();

        P(gema.transform, "GemaSup", PrimitiveType.Sphere,
            new Vector3(0,  0.18f, 0), new Vector3(0.45f,0.45f,0.45f), matGema2);
        P(gema.transform, "GemaInf", PrimitiveType.Sphere,
            new Vector3(0, -0.12f, 0), new Vector3(0.45f,0.25f,0.45f), matGema2);
    }

    // ── UI ────────────────────────────────────────────────────────────────
    static void CrearUI()
    {
        var canv = new GameObject("Canvas");
        var cv   = canv.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        canv.AddComponent<CanvasScaler>();
        canv.AddComponent<GraphicRaycaster>();

        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Panel HUD
        var panel = new GameObject("PanelHUD");
        panel.transform.SetParent(canv.transform, false);
        var rp = panel.AddComponent<RectTransform>();
        rp.anchorMin = rp.anchorMax = rp.pivot = new Vector2(0,1);
        rp.anchoredPosition = new Vector2(10,-10);
        rp.sizeDelta = new Vector2(340, 95);
        panel.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.5f);

        T(panel.transform, "TextoContador",   new Vector2(0,1), new Vector2(0,1),
            new Vector2(12,-8),  new Vector2(320,30), "Items: 0/8", 22, Color.white, f);
        T(panel.transform, "TextoObjetivo",   new Vector2(0,1), new Vector2(0,1),
            new Vector2(12,-40), new Vector2(320,26), "Recoge 8 orbes + gema → abre cofre", 14,
            new Color(1,1,0.6f), f);
        T(panel.transform, "TextoGema",       new Vector2(0,1), new Vector2(0,1),
            new Vector2(12,-64), new Vector2(320,22), "Gema: ✗  Busca la gema violeta", 13,
            new Color(0.85f,0.4f,1f), f);

        // Crosshair
        T(canv.transform, "Crosshair", new Vector2(.5f,.5f), new Vector2(.5f,.5f),
            Vector2.zero, new Vector2(20,20), "·", 28, new Color(1,1,1,0.7f), f);

        // ── PANEL STATS vida + estamina ──
        var ps = new GameObject("PanelStats");
        ps.transform.SetParent(canv.transform, false);
        var rs = ps.AddComponent<RectTransform>();
        rs.anchorMin = rs.anchorMax = new Vector2(0,0);
        rs.pivot = new Vector2(0,0);
        rs.anchoredPosition = new Vector2(14,14);
        rs.sizeDelta = new Vector2(230,80);
        ps.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.5f);

        T(ps.transform, "LabelVida", new Vector2(0,1), new Vector2(0,1),
            new Vector2(8,-8), new Vector2(60,18), "❤ VIDA", 13, new Color(1f,0.35f,0.35f), f);
        T(ps.transform, "TextoVida", new Vector2(1,1), new Vector2(1,1),
            new Vector2(-8,-8), new Vector2(60,18), "100/100", 13, Color.white, f);
        Barra(ps.transform, "BarraVida", "RellenoVida",
            new Vector2(8,-28), new Vector2(214,18),
            new Color(0.25f,0.05f,0.05f), new Color(0.9f,0.15f,0.15f));

        T(ps.transform, "LabelEstamina", new Vector2(0,1), new Vector2(0,1),
            new Vector2(8,-50), new Vector2(90,18), "⚡ ESTAMINA", 13, new Color(0.3f,1f,0.4f), f);
        Barra(ps.transform, "BarraEstamina", "RellenoEstamina",
            new Vector2(8,-70), new Vector2(214,14),
            new Color(0.05f,0.18f,0.05f), new Color(0.2f,0.85f,0.25f));

        // Mensaje temporal
        var msg = T(canv.transform, "TextoMensajeTemporal",
            new Vector2(.5f,.35f), new Vector2(.5f,.35f),
            Vector2.zero, new Vector2(560,80), "", 24, Color.white, f);
        msg.SetActive(false);

        // Victoria
        var vic = T(canv.transform, "TextoVictoria",
            new Vector2(.5f,.5f), new Vector2(.5f,.5f),
            new Vector2(0,60), new Vector2(580,130),
            "¡NIVEL 2 COMPLETADO!\n¡Eres el mejor!", 44, new Color(1f,0.88f,0f), f);
        vic.SetActive(false);

        var cnt = canv.AddComponent<ContadorItems>();
        cnt.metaItems = 8;
        canv.AddComponent<HUDJugador>();
    }

    static void Barra(Transform padre, string bgNombre, string fillNombre,
        Vector2 pos, Vector2 size, Color bg, Color fill)
    {
        var bgGo = new GameObject(bgNombre);
        bgGo.transform.SetParent(padre, false);
        var rb = bgGo.AddComponent<RectTransform>();
        rb.anchorMin = rb.anchorMax = new Vector2(0,1);
        rb.pivot = new Vector2(0,1);
        rb.anchoredPosition = pos; rb.sizeDelta = size;
        bgGo.AddComponent<UnityEngine.UI.Image>().color = bg;

        var fillGo = new GameObject(fillNombre);
        fillGo.transform.SetParent(bgGo.transform, false);
        var rf = fillGo.AddComponent<RectTransform>();
        rf.anchorMin = Vector2.zero; rf.anchorMax = Vector2.one;
        rf.offsetMin = rf.offsetMax = Vector2.zero;
        var img = fillGo.AddComponent<UnityEngine.UI.Image>();
        img.color = fill;
        img.type  = UnityEngine.UI.Image.Type.Filled;
        img.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        img.fillAmount = 1f;
    }

    // ── HELPERS ───────────────────────────────────────────────────────────
    static void P(Transform padre, string nombre, PrimitiveType tipo,
                  Vector3 pos, Vector3 esc, Material mat)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale    = esc;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
    }

    static GameObject T(Transform padre, string nombre, Vector2 aMin, Vector2 aMax,
        Vector2 pos, Vector2 size, string texto, int fs, Color color, Font font)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax; r.pivot = aMin;
        r.anchoredPosition = pos; r.sizeDelta = size;
        var t = go.AddComponent<Text>();
        t.text = texto; t.fontSize = fs; t.color = color; t.font = font;
        t.alignment = TextAnchor.MiddleCenter;
        return go;
    }
}
