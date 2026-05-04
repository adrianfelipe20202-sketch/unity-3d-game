using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Crea la escena Nivel3 y la agrega a Build Settings.
/// Menu: Semana9 > Crear Nivel 3
/// </summary>
public class ConfiguradorNivel3
{
    const string RUTA_ESCENA = "Assets/Scenes/Nivel3.unity";
    const float MAPA_MITAD = 28f;

    static Material matSuelo3, matPared3, matPlataforma3, matJugador3;
    static Material matMadera3, matMetal3, matColec3, matPiel3, matOjos3, matGema3;

    [MenuItem("Semana9/Crear Nivel 3")]
    static void CrearNivel3()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CargarOMateriales();
        ConfigurarAmbiente();
        CrearSuelo();
        CrearLimites();
        CrearLuces();
        CrearJugador();
        CrearNPC();

        var plataformas = CrearPlataformasEnMovimiento();
        CrearSprite2DMovimientoDemo();

        CrearPuerta();
        CrearCofre(plataformas);
        CrearGema();

        CrearSpawner(plataformas);
        CrearUI();

        EditorSceneManager.SaveScene(escena, RUTA_ESCENA);
        AgregarABuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== NIVEL 3 CREADO Y GUARDADO ===");
    }

    static void AgregarABuildSettings()
    {
        var lista = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] rutas =
        {
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/Nivel2.unity",
            RUTA_ESCENA
        };

        foreach (string ruta in rutas)
        {
            bool yaExiste = false;
            foreach (var s in lista)
                if (s.path == ruta) { yaExiste = true; break; }
            if (!yaExiste)
                lista.Add(new EditorBuildSettingsScene(ruta, true));
        }

        EditorBuildSettings.scenes = lista.ToArray();
        Debug.Log("=== Build Settings actualizados: " + lista.Count + " escenas ===");
    }

    // ── Materiales/ambiente ───────────────────────────────────────────────
    static void CargarOMateriales()
    {
        // Reutiliza materiales existentes si están, si no los crea (URP Lit / Standard).
        Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        matSuelo3      = CargarOMat(s, "MatSuelo2",      new Color(0.16f, 0.18f, 0.20f));
        matPared3      = CargarOMat(s, "MatPared2",      new Color(0.12f, 0.12f, 0.14f));
        matPlataforma3 = CargarOMat(s, "MatPlataforma",  Color.white);
        matJugador3    = CargarOMat(s, "MatJugador",     new Color(0.15f, 0.35f, 0.90f));
        matMadera3     = CargarOMat(s, "MatMadera",      Color.white);
        matMetal3      = CargarOMat(s, "MatMetal",       new Color(0.9f, 0.75f, 0.15f));
        matColec3      = CargarOMat(s, "MatColeccionable", new Color(1f, 0.85f, 0f));
        matPiel3       = CargarOMat(s, "MatPiel",        new Color(0.95f, 0.78f, 0.58f));
        matOjos3       = CargarOMat(s, "MatOjos",        Color.black);

        // Gema: emisivo (si no existe, se crea)
        matGema3 = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MatGema2.mat");
        if (matGema3 == null) matGema3 = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MatGema.mat");
        if (matGema3 == null)
        {
            matGema3 = new Material(s);
            if (matGema3.HasProperty("_BaseColor")) matGema3.SetColor("_BaseColor", new Color(0.2f, 0.95f, 0.85f));
            else matGema3.color = new Color(0.2f, 0.95f, 0.85f);
            matGema3.EnableKeyword("_EMISSION");
            if (matGema3.HasProperty("_EmissionColor")) matGema3.SetColor("_EmissionColor", new Color(0.1f, 0.6f, 0.55f));
            AssetDatabase.CreateAsset(matGema3, "Assets/Materials/MatGema3.mat");
        }
    }

    static Material CargarOMat(Shader s, string nombre, Color fallbackColor)
    {
        string path = $"Assets/Materials/{nombre}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(s);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", fallbackColor);
            else m.color = fallbackColor;
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    static void ConfigurarAmbiente()
    {
        RenderSettings.ambientLight = new Color(0.20f, 0.22f, 0.28f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.10f, 0.12f, 0.16f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 22f;
        RenderSettings.fogEndDistance = 60f;
        DynamicGI.UpdateEnvironment();
    }

    // ── Mundo ─────────────────────────────────────────────────────────────
    static void CrearSuelo()
    {
        var s = GameObject.CreatePrimitive(PrimitiveType.Plane);
        s.name = "Suelo";
        s.transform.position = Vector3.zero;
        s.transform.localScale = new Vector3(6f, 1f, 6f);
        s.GetComponent<Renderer>().sharedMaterial = matSuelo3;
    }

    static void CrearLimites()
    {
        var limites = new GameObject("LimitesMapa");
        float h = 5f, e = 1.5f, m = MAPA_MITAD;
        Muro(limites.transform, new Vector3(0, h / 2f, m), new Vector3(m * 2f + e * 2, h, e));
        Muro(limites.transform, new Vector3(0, h / 2f, -m), new Vector3(m * 2f + e * 2, h, e));
        Muro(limites.transform, new Vector3(m, h / 2f, 0), new Vector3(e, h, m * 2f));
        Muro(limites.transform, new Vector3(-m, h / 2f, 0), new Vector3(e, h, m * 2f));
    }

    static void Muro(Transform p, Vector3 pos, Vector3 esc)
    {
        var m = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m.name = "Muro";
        m.transform.SetParent(p);
        m.transform.position = pos;
        m.transform.localScale = esc;
        m.GetComponent<Renderer>().sharedMaterial = matPared3;
    }

    static void CrearLuces()
    {
        var luzGo = new GameObject("LuzPrincipal");
        var luz = luzGo.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(0.9f, 0.95f, 1f);
        luz.intensity = 1.0f;
        luz.shadows = LightShadows.Soft;
        luzGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var rellGo = new GameObject("LuzRelleno");
        var rell = rellGo.AddComponent<Light>();
        rell.type = LightType.Directional;
        rell.color = new Color(0.35f, 0.50f, 0.95f);
        rell.intensity = 0.35f;
        rell.shadows = LightShadows.None;
        rellGo.transform.rotation = Quaternion.Euler(25f, 130f, 0f);
    }

    // ── Player/NPC ────────────────────────────────────────────────────────
    static void CrearJugador()
    {
        var jugador = new GameObject("Jugador");
        jugador.tag = "Player";
        jugador.transform.position = new Vector3(0f, 0f, -20f);

        var cc = jugador.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.38f; cc.center = new Vector3(0f, 1f, 0f);
        var j = jugador.AddComponent<Jugador>();

        P(jugador.transform, "Cuerpo", PrimitiveType.Capsule,
            new Vector3(0f, 1f, 0f), new Vector3(0.72f, 1f, 0.72f), matJugador3);
        P(jugador.transform, "Cabeza", PrimitiveType.Sphere,
            new Vector3(0f, 2.18f, 0f), Vector3.one * 0.54f, matPiel3);
        P(jugador.transform, "OjoD", PrimitiveType.Sphere,
            new Vector3(0.12f, 2.26f, 0.24f), Vector3.one * 0.09f, matOjos3);
        P(jugador.transform, "OjoI", PrimitiveType.Sphere,
            new Vector3(-0.12f, 2.26f, 0.24f), Vector3.one * 0.09f, matOjos3);

        var cam = new GameObject("MainCamera");
        cam.tag = "MainCamera";
        cam.AddComponent<Camera>().fieldOfView = 75f;
        cam.AddComponent<AudioListener>();
        cam.transform.SetParent(jugador.transform);
        cam.transform.localPosition = new Vector3(0f, 2.08f, 0.10f);
        j.camara = cam.transform;
    }

    static void CrearNPC()
    {
        var npc = new GameObject("NPC");
        npc.transform.position = new Vector3(-6f, 0f, -16f);
        var t = npc.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2.2f; t.center = new Vector3(0, 1, 0);

        var n = npc.AddComponent<NPC>();
        n.lineasDeDialogo = new[]
        {
            "¡Nivel 3! Aquí todo se mueve.",
            "Hay plataformas con movimiento lineal y otras con movimiento ping-pong.",
            "Las esferas aparecen encima de plataformas en movimiento.",
            "Salta y recógelas para abrir el cofre."
        };

        P(npc.transform, "Cuerpo", PrimitiveType.Capsule,
            new Vector3(0, 1, 0), new Vector3(0.72f, 1f, 0.72f), matJugador3);
        P(npc.transform, "Cabeza", PrimitiveType.Sphere,
            new Vector3(0, 2.18f, 0), Vector3.one * 0.54f, matPiel3);

        npc.transform.LookAt(new Vector3(0, 0, -20f));
    }

    // ── Plataformas móviles ───────────────────────────────────────────────
    static List<Transform> CrearPlataformasEnMovimiento()
    {
        var padre = new GameObject("PlataformasMoviles");
        var lista = new List<Transform>();

        // Base (estática) para empezar
        lista.Add(CrearPlataforma(padre.transform, "BaseInicio",
            new Vector3(0f, 1.5f, -16f), new Vector3(8f, 0.6f, 6f), null));

        // PingPong (ida y vuelta)
        lista.Add(CrearPlataforma(padre.transform, "PingPong_1",
            new Vector3(-10f, 3.0f, -10f), new Vector3(5f, 0.6f, 4f),
            m =>
            {
                m.modo = MovimientoPingPongLineal.Modo.PingPong;
                m.puntoA = new Vector3(-10f, 3.0f, -10f);
                m.puntoB = new Vector3(-2f, 3.0f, -10f);
                m.duracion = 3.2f;
            }));

        lista.Add(CrearPlataforma(padre.transform, "PingPong_2",
            new Vector3(8f, 4.8f, -6f), new Vector3(5f, 0.6f, 4f),
            m =>
            {
                m.modo = MovimientoPingPongLineal.Modo.PingPong;
                m.puntoA = new Vector3(8f, 4.8f, -6f);
                m.puntoB = new Vector3(8f, 4.8f, 2f);
                m.duracion = 2.8f;
            }));

        // Lineal (loop)
        lista.Add(CrearPlataforma(padre.transform, "Lineal_1",
            new Vector3(-6f, 6.5f, 2f), new Vector3(5f, 0.6f, 4f),
            m =>
            {
                m.modo = MovimientoPingPongLineal.Modo.Lineal;
                m.puntoA = new Vector3(-6f, 6.5f, 2f);
                m.puntoB = new Vector3(6f, 6.5f, 2f);
                m.duracion = 4.0f;
            }));

        lista.Add(CrearPlataforma(padre.transform, "Lineal_2",
            new Vector3(0f, 8.2f, 10f), new Vector3(6f, 0.6f, 5f),
            m =>
            {
                m.modo = MovimientoPingPongLineal.Modo.Lineal;
                m.puntoA = new Vector3(0f, 8.2f, 10f);
                m.puntoB = new Vector3(-10f, 8.2f, 16f);
                m.duracion = 4.8f;
            }));

        // Plataforma final (estática) para cofre
        lista.Add(CrearPlataforma(padre.transform, "Final",
            new Vector3(0f, 10.5f, 20f), new Vector3(8f, 0.6f, 7f), null));

        return lista;
    }

    static Transform CrearPlataforma(Transform padre, string nombre, Vector3 pos, Vector3 esc,
        System.Action<MovimientoPingPongLineal> configurarMovimiento)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = nombre;
        cube.transform.SetParent(padre);
        cube.transform.position = pos;
        cube.transform.localScale = esc;
        cube.GetComponent<Renderer>().sharedMaterial = matPlataforma3;

        // Movimiento
        if (configurarMovimiento != null)
        {
            var mov = cube.AddComponent<MovimientoPingPongLineal>();
            configurarMovimiento(mov);
            mov.transportarSobrePlataforma = true;
            mov.triggerAltura = esc.y * 0.55f + 0.25f;
        }

        return cube.transform;
    }

    // ── Sprite 2D demo (lineal y ping-pong) ───────────────────────────────
    static void CrearSprite2DMovimientoDemo()
    {
        var root = new GameObject("Sprites2D_Demo");

        var sprite = CrearSpriteAssetSiNoExiste();

        CrearSpriteMover(root.transform, "Sprite_PingPong",
            sprite, new Vector3(-12f, 2.0f, -2f),
            MovimientoPingPongLineal.Modo.PingPong,
            new Vector3(-12f, 2.0f, -2f),
            new Vector3(-4f, 2.0f, -2f),
            2.4f);

        CrearSpriteMover(root.transform, "Sprite_Lineal",
            sprite, new Vector3(12f, 2.0f, -2f),
            MovimientoPingPongLineal.Modo.Lineal,
            new Vector3(12f, 2.0f, -2f),
            new Vector3(4f, 2.0f, -2f),
            2.8f);
    }

    static Sprite CrearSpriteAssetSiNoExiste()
    {
        const string texPath = "Assets/Materials/SpriteNivel3.png";
        const string assetPath = texPath;

        if (!File.Exists(Application.dataPath + "/Materials/SpriteNivel3.png"))
        {
            var t = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var cFondo = new Color(0.10f, 0.10f, 0.12f, 0f);
            var c1 = new Color(0.95f, 0.35f, 0.95f, 1f);
            var c2 = new Color(0.25f, 0.95f, 0.95f, 1f);

            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float dx = (x - 32) / 32f;
                    float dy = (y - 32) / 32f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    Color col = r <= 0.95f ? Color.Lerp(c1, c2, (dx + 1f) * 0.5f) : cFondo;
                    col.a = r <= 0.95f ? 1f : 0f;
                    t.SetPixel(x, y, col);
                }
            t.Apply();
            File.WriteAllBytes(Application.dataPath + "/Materials/SpriteNivel3.png", t.EncodeToPNG());
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    static void CrearSpriteMover(Transform padre, string nombre, Sprite sprite, Vector3 pos,
        MovimientoPingPongLineal.Modo modo, Vector3 a, Vector3 b, float dur)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10;
        go.transform.localScale = Vector3.one * 2.0f;

        var mov = go.AddComponent<MovimientoPingPongLineal>();
        mov.transportarSobrePlataforma = false;
        mov.modo = modo;
        mov.puntoA = a;
        mov.puntoB = b;
        mov.duracion = dur;
    }

    // ── Objetivo/Progresión ───────────────────────────────────────────────
    static void CrearPuerta()
    {
        var piv = new GameObject("Puerta");
        piv.transform.position = new Vector3(-0.5f, 0f, -6f);

        var t = piv.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2.5f; t.center = new Vector3(0.5f, 1.5f, 0);
        piv.AddComponent<Puerta>();

        P(piv.transform, "Panel", PrimitiveType.Cube,
            new Vector3(0.5f, 1.5f, 0), new Vector3(1f, 3f, 0.12f), matMadera3);
        P(piv.transform, "Marco", PrimitiveType.Cube,
            new Vector3(-0.07f, 1.5f, 0), new Vector3(0.13f, 3.15f, 0.16f), matMetal3);
        P(piv.transform, "Marco2", PrimitiveType.Cube,
            new Vector3(1.07f, 1.5f, 0), new Vector3(0.13f, 3.15f, 0.16f), matMetal3);
        P(piv.transform, "Marco3", PrimitiveType.Cube,
            new Vector3(0.5f, 3.07f, 0), new Vector3(1.28f, 0.13f, 0.16f), matMetal3);
        P(piv.transform, "Pomo", PrimitiveType.Sphere,
            new Vector3(0.82f, 1.5f, 0.09f), Vector3.one * 0.12f, matMetal3);

        var col = piv.AddComponent<BoxCollider>();
        col.center = new Vector3(0.5f, 1.5f, 0); col.size = new Vector3(1f, 3f, 0.14f);
    }

    static void CrearCofre(List<Transform> plataformas)
    {
        var cofre = new GameObject("CofreEscena");
        Vector3 pos = new Vector3(0f, 11.0f, 20f);
        if (plataformas != null)
        {
            foreach (var p in plataformas)
                if (p != null && p.name == "Final") { pos = p.position + Vector3.up * 0.6f; break; }
        }
        cofre.transform.position = pos;

        var t = cofre.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2f; t.center = new Vector3(0, 0.5f, 0);
        var c = cofre.AddComponent<Cofre>();
        c.itemsRequeridos = 10;

        // Reusar jerarquía que busca Cofre.cs ("CofreTapa")
        P(cofre.transform, "CofreBase", PrimitiveType.Cube,
            new Vector3(0, 0.35f, 0), new Vector3(1.1f, 0.7f, 0.75f), matMadera3);
        P(cofre.transform, "CofreTapa", PrimitiveType.Cube,
            new Vector3(0, 0.82f, 0), new Vector3(1.1f, 0.35f, 0.75f), matMadera3);
        P(cofre.transform, "Franja", PrimitiveType.Cube,
            new Vector3(0, 0.675f, 0), new Vector3(1.12f, 0.075f, 0.77f), matMetal3);
        P(cofre.transform, "Lock", PrimitiveType.Cube,
            new Vector3(0, 0.38f, 0.395f), new Vector3(0.22f, 0.22f, 0.065f), matMetal3);

        var col = cofre.AddComponent<BoxCollider>();
        col.center = new Vector3(0, 0.5f, 0); col.size = new Vector3(1.1f, 1f, 0.75f);
    }

    static void CrearGema()
    {
        var gema = new GameObject("GemaEspecial");
        gema.transform.position = new Vector3(-10f, 9.2f, 16f);

        var col = gema.AddComponent<SphereCollider>();
        col.isTrigger = true; col.radius = 0.5f;
        gema.AddComponent<ItemEspecial>();

        P(gema.transform, "GemaSup", PrimitiveType.Sphere,
            new Vector3(0, 0.18f, 0), new Vector3(0.45f, 0.45f, 0.45f), matGema3);
        P(gema.transform, "GemaInf", PrimitiveType.Sphere,
            new Vector3(0, -0.12f, 0), new Vector3(0.45f, 0.25f, 0.45f), matGema3);
    }

    // ── Spawner items en plataformas ──────────────────────────────────────
    static void CrearSpawner(List<Transform> plataformas)
    {
        var sp = new GameObject("SpawnerItemsPlataformas");
        var gen = sp.AddComponent<SpawnerItemsEnPlataformas>();
        gen.maxItems = 10;
        gen.alturaSobrePlataforma = 0.65f;
        gen.margenBorde = 0.8f;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coleccionable.prefab");
        if (prefab != null) gen.prefabItem = prefab;

        if (plataformas != null)
            foreach (var p in plataformas)
                if (p != null && p.name != "BaseInicio" && p.name != "Final")
                    gen.plataformas.Add(p);
    }

    // ── UI ────────────────────────────────────────────────────────────────
    static void CrearUI()
    {
        var canv = new GameObject("Canvas");
        var cv = canv.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        canv.AddComponent<CanvasScaler>();
        canv.AddComponent<GraphicRaycaster>();

        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var panel = new GameObject("PanelHUD");
        panel.transform.SetParent(canv.transform, false);
        var rp = panel.AddComponent<RectTransform>();
        rp.anchorMin = rp.anchorMax = rp.pivot = new Vector2(0, 1);
        rp.anchoredPosition = new Vector2(10, -10);
        rp.sizeDelta = new Vector2(360, 100);
        panel.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.5f);

        T(panel.transform, "TextoContador", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(12, -8), new Vector2(340, 30), "Items: 0/10", 22, Color.white, f);
        T(panel.transform, "TextoObjetivo", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(12, -40), new Vector2(340, 26), "Recoge 10 esferas + gema → abre cofre", 14,
            new Color(1, 1, 0.6f), f);
        T(panel.transform, "TextoGema", new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(12, -64), new Vector2(340, 22), "Gema: ✗  Busca la gema especial", 13,
            new Color(0.4f, 1f, 0.9f), f);

        T(canv.transform, "Crosshair", new Vector2(.5f, .5f), new Vector2(.5f, .5f),
            Vector2.zero, new Vector2(20, 20), "·", 28, new Color(1, 1, 1, 0.7f), f);

        var msg = T(canv.transform, "TextoMensajeTemporal",
            new Vector2(.5f, .35f), new Vector2(.5f, .35f),
            Vector2.zero, new Vector2(560, 80), "", 24, Color.white, f);
        msg.SetActive(false);

        var vic = T(canv.transform, "TextoVictoria",
            new Vector2(.5f, .5f), new Vector2(.5f, .5f),
            new Vector2(0, 60), new Vector2(580, 130),
            "¡NIVEL 3 COMPLETADO!\n¡Ve a la puerta!", 44, new Color(1f, 0.88f, 0f), f);
        vic.SetActive(false);

        var cnt = canv.AddComponent<ContadorItems>();
        cnt.metaItems = 10;
        canv.AddComponent<HUDJugador>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static void P(Transform padre, string nombre, PrimitiveType tipo, Vector3 pos, Vector3 esc, Material mat)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = pos;
        go.transform.localScale = esc;
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

