using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Configura la escena completa de la Semana 9 con URP.
/// Menu: Semana9 > Configurar Escena Completa
/// </summary>
public class ConfiguradorSemana9
{
    // ─────────────────────────────────────────────────────────────────────
    //  CONSTANTES
    // ─────────────────────────────────────────────────────────────────────
    const float MAPA_MITAD = 28f;   // El plano escala 6 = 60m, dejamos margen

    // ─────────────────────────────────────────────────────────────────────
    //  MATERIALES (caché)
    // ─────────────────────────────────────────────────────────────────────
    static Material matSuelo, matJugador, matNPC, matEnemigo;
    static Material matMadera, matMetal, matColeccionable;
    static Material matPiel, matOjos, matSombrero;
    static Material matPared, matSkybox;

    // ─────────────────────────────────────────────────────────────────────
    //  ENTRY POINT
    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Semana9/Configurar Escena Completa")]
    static void ConfigurarTodo()
    {
        LimpiarEscena();
        GenerarTexturas();
        CrearMateriales();
        ConfigurarAmbiente();
        CrearSuelo();
        CrearLimitesMapa();
        CrearLuces();
        CrearJugador();
        CrearNPC();
        CrearPuerta();
        CrearCofre();
        CrearEnemigo();
        CrearPrefabColeccionable();
        CrearSpawner();
        CrearGema();
        CrearUI();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("=== ESCENA LISTA. Pulsa Play! ===");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  LIMPIAR
    // ─────────────────────────────────────────────────────────────────────
    static void LimpiarEscena()
    {
        // Destruir TODOS los objetos de la escena para empezar completamente limpio
        // excepto los que Unity requiere internamente
        var todosLosObjetos = Object.FindObjectsOfType<GameObject>();
        foreach (var go in todosLosObjetos)
        {
            if (go != null && go.transform.parent == null) // solo raíz
                Object.DestroyImmediate(go);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  TEXTURAS PROCEDURALES
    // ─────────────────────────────────────────────────────────────────────
    static Texture2D texMadera, texPiedra, texSuelo;

    static void GenerarTexturas()
    {
        texMadera = GenerarTexturaMadera(256, 256);
        texPiedra = GenerarTexturaPiedra(256, 256);
        texSuelo  = GenerarTexturaSuelo(512, 512);

        GuardarTextura(texMadera, "TexturaMadera");
        GuardarTextura(texPiedra, "TexturaPiedra");
        GuardarTextura(texSuelo,  "TexturaSuelo");

        AssetDatabase.Refresh();

        texMadera = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/TexturaMadera.png");
        texPiedra = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/TexturaPiedra.png");
        texSuelo  = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/TexturaSuelo.png");
    }

    static Texture2D GenerarTexturaMadera(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGB24, true);
        Color c1 = new Color(0.28f, 0.14f, 0.04f);
        Color c2 = new Color(0.62f, 0.38f, 0.13f);
        Color c3 = new Color(0.50f, 0.28f, 0.08f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float veta  = Mathf.PerlinNoise(x * 0.025f, y * 0.11f);
                float veta2 = Mathf.PerlinNoise(x * 0.04f + 5f, y * 0.22f + 3f) * 0.4f;
                float nudo  = Mathf.PerlinNoise(x * 0.08f + 10f, y * 0.08f + 10f) * 0.18f;
                float v = Mathf.Clamp01(veta * 0.6f + veta2 + nudo);
                Color col = v < 0.5f ? Color.Lerp(c1, c3, v * 2f) : Color.Lerp(c3, c2, (v - 0.5f) * 2f);
                tex.SetPixel(x, y, col);
            }
        tex.Apply();
        return tex;
    }

    static Texture2D GenerarTexturaPiedra(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGB24, true);
        Color c1 = new Color(0.35f, 0.35f, 0.37f);
        Color c2 = new Color(0.55f, 0.53f, 0.50f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n1 = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                float n2 = Mathf.PerlinNoise(x * 0.12f + 7f, y * 0.12f + 3f) * 0.3f;
                // Juntas de piedra
                float jx = Mathf.Abs(Mathf.Sin(x * 0.25f)) * 0.07f;
                float jy = Mathf.Abs(Mathf.Sin(y * 0.35f)) * 0.07f;
                float v = Mathf.Clamp01(n1 * 0.7f + n2 - jx - jy);
                tex.SetPixel(x, y, Color.Lerp(c1, c2, v));
            }
        tex.Apply();
        return tex;
    }

    static Texture2D GenerarTexturaSuelo(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGB24, true);
        Color verde1 = new Color(0.22f, 0.45f, 0.12f);
        Color verde2 = new Color(0.35f, 0.60f, 0.20f);
        Color verde3 = new Color(0.18f, 0.38f, 0.09f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n1 = Mathf.PerlinNoise(x * 0.02f, y * 0.02f);
                float n2 = Mathf.PerlinNoise(x * 0.08f + 5f, y * 0.08f + 5f) * 0.3f;
                float v = Mathf.Clamp01(n1 + n2);
                Color col = v < 0.5f ? Color.Lerp(verde3, verde1, v * 2f) : Color.Lerp(verde1, verde2, (v - 0.5f) * 2f);
                tex.SetPixel(x, y, col);
            }
        tex.Apply();
        return tex;
    }

    static void GuardarTextura(Texture2D tex, string nombre)
    {
        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + $"/Materials/{nombre}.png", png);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  MATERIALES URP
    // ─────────────────────────────────────────────────────────────────────
    static void CrearMateriales()
    {
        // Busca el shader URP Lit, con fallback a Standard
        string shaderLit = "Universal Render Pipeline/Lit";

        matSuelo         = MatURP("MatSuelo",         new Color(0.30f, 0.55f, 0.18f), texSuelo,  shaderLit, 0f, 0.4f, 6f);
        matJugador       = MatURP("MatJugador",        new Color(0.15f, 0.35f, 0.90f), null,      shaderLit, 0.1f, 0.6f, 1f);
        matNPC           = MatURP("MatNPC",            new Color(0.50f, 0.15f, 0.65f), null,      shaderLit, 0f, 0.5f, 1f);
        matEnemigo       = MatURP("MatEnemigo",        new Color(0.80f, 0.08f, 0.08f), null,      shaderLit, 0.1f, 0.4f, 1f);
        matMadera        = MatURP("MatMadera",         Color.white,                    texMadera, shaderLit, 0f, 0.3f, 1f);
        matMetal         = MatURP("MatMetal",          new Color(0.90f, 0.75f, 0.15f), null,      shaderLit, 0.85f, 0.85f, 1f);
        matColeccionable = MatURP("MatColeccionable",  new Color(1.00f, 0.85f, 0.00f), null,      shaderLit, 0.7f, 0.95f, 1f);
        matPiel          = MatURP("MatPiel",           new Color(0.95f, 0.78f, 0.58f), null,      shaderLit, 0f, 0.3f, 1f);
        matOjos          = MatURP("MatOjos",           Color.black,                    null,      shaderLit, 0f, 0.2f, 1f);
        matSombrero      = MatURP("MatSombrero",       new Color(0.25f, 0.05f, 0.38f), null,      shaderLit, 0f, 0.3f, 1f);
        matPared         = MatURP("MatPared",          Color.white,                    texPiedra, shaderLit, 0f, 0.25f, 3f);
    }

    /// <summary>Crea o reutiliza un material URP con los parámetros dados.</summary>
    static Material MatURP(string nombre, Color color, Texture2D tex,
                            string shaderName, float metallic, float smoothness, float tiling)
    {
        string path = $"Assets/Materials/{nombre}.mat";
        Material m  = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (m == null)
        {
            Shader s = Shader.Find(shaderName) ?? Shader.Find("Standard");
            m = new Material(s);
            AssetDatabase.CreateAsset(m, path);
        }
        else
        {
            Shader s = Shader.Find(shaderName) ?? Shader.Find("Standard");
            m.shader = s;
        }

        // Propiedades URP
        if (m.HasProperty("_BaseColor"))   m.SetColor("_BaseColor", color);
        else                               m.color = color;

        if (tex != null)
        {
            if (m.HasProperty("_BaseMap"))
            {
                m.SetTexture("_BaseMap", tex);
                m.SetTextureScale("_BaseMap", new Vector2(tiling, tiling));
            }
            else
            {
                m.mainTexture = tex;
                m.mainTextureScale = new Vector2(tiling, tiling);
            }
        }

        if (m.HasProperty("_Metallic"))   m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
        else if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);

        EditorUtility.SetDirty(m);
        return m;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  AMBIENTE Y CIELO
    // ─────────────────────────────────────────────────────────────────────
    static void ConfigurarAmbiente()
    {
        // Skybox procedural (funciona en todos los pipelines)
        Material sky = new Material(Shader.Find("Skybox/Procedural"));
        if (sky != null && sky.shader.name != "Hidden/InternalErrorShader")
        {
            sky.SetFloat("_SunSize",             0.03f);
            sky.SetFloat("_SunSizeConvergence",  10f);
            sky.SetFloat("_AtmosphereThickness",  1.4f);
            sky.SetColor("_SkyTint",              new Color(0.38f, 0.65f, 1.00f));
            sky.SetColor("_GroundColor",          new Color(0.28f, 0.22f, 0.13f));
            sky.SetFloat("_Exposure",             1.4f);
            AssetDatabase.CreateAsset(sky, "Assets/Materials/SkyboxMat.mat");
            RenderSettings.skybox = sky;
        }

        RenderSettings.ambientLight    = new Color(0.45f, 0.50f, 0.58f);
        RenderSettings.fogColor        = new Color(0.65f, 0.78f, 0.92f);
        RenderSettings.fog             = true;
        RenderSettings.fogMode         = FogMode.Linear;
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance   = 70f;

        DynamicGI.UpdateEnvironment();
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SUELO
    // ─────────────────────────────────────────────────────────────────────
    static void CrearSuelo()
    {
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Plane);
        s.name = "Suelo";
        s.transform.position = Vector3.zero;
        s.transform.localScale = new Vector3(6f, 1f, 6f);
        s.GetComponent<Renderer>().sharedMaterial = matSuelo;
        Undo.RegisterCreatedObjectUndo(s, "Suelo");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  LÍMITES DEL MAPA
    // ─────────────────────────────────────────────────────────────────────
    static void CrearLimitesMapa()
    {
        GameObject limites = new GameObject("LimitesMapa");

        // Norte, Sur, Este, Oeste
        float h = 4f;   // altura de la muralla
        float e = 1.5f; // espesor
        float m = MAPA_MITAD;

        // Norte  (z+)
        CrearMuro(limites.transform, new Vector3(0, h / 2f,  m),       new Vector3(m * 2f + e * 2f, h, e));
        // Sur    (z-)
        CrearMuro(limites.transform, new Vector3(0, h / 2f, -m),       new Vector3(m * 2f + e * 2f, h, e));
        // Este   (x+)
        CrearMuro(limites.transform, new Vector3( m, h / 2f, 0),       new Vector3(e, h, m * 2f));
        // Oeste  (x-)
        CrearMuro(limites.transform, new Vector3(-m, h / 2f, 0),       new Vector3(e, h, m * 2f));

        Undo.RegisterCreatedObjectUndo(limites, "LimitesMapa");
    }

    static void CrearMuro(Transform padre, Vector3 pos, Vector3 escala)
    {
        GameObject muro = GameObject.CreatePrimitive(PrimitiveType.Cube);
        muro.name = "Muro";
        muro.transform.SetParent(padre);
        muro.transform.position = pos;
        muro.transform.localScale = escala;
        muro.GetComponent<Renderer>().sharedMaterial = matPared;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  LUCES
    // ─────────────────────────────────────────────────────────────────────
    static void CrearLuces()
    {
        // Reutilizar la luz direccional si existe
        GameObject luzVieja = GameObject.Find("Directional Light");
        Light sol = luzVieja != null
            ? luzVieja.GetComponent<Light>()
            : new GameObject("LuzSol").AddComponent<Light>();

        sol.type      = LightType.Directional;
        sol.color     = new Color(1.00f, 0.94f, 0.82f);
        sol.intensity = 1.4f;
        sol.shadows   = LightShadows.Soft;
        sol.gameObject.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

        if (luzVieja == null)
            Undo.RegisterCreatedObjectUndo(sol.gameObject, "LuzSol");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  JUGADOR
    // ─────────────────────────────────────────────────────────────────────
    static void CrearJugador()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null) return;

        GameObject jugador = new GameObject("Jugador");
        jugador.tag = "Player";
        jugador.transform.position = new Vector3(0f, 0f, -10f);

        CharacterController cc = jugador.AddComponent<CharacterController>();
        cc.height = 2f; cc.radius = 0.38f;
        cc.center = new Vector3(0f, 1f, 0f);
        Jugador j = jugador.AddComponent<Jugador>();

        // Cuerpo
        Primitiva(jugador.transform, "Cuerpo", PrimitiveType.Capsule,
            new Vector3(0f, 1f, 0f), new Vector3(0.72f, 1f, 0.72f), matJugador, false);

        // Cabeza
        Primitiva(jugador.transform, "Cabeza", PrimitiveType.Sphere,
            new Vector3(0f, 2.18f, 0f), Vector3.one * 0.54f, matPiel, false);

        // Ojos
        OjoPar(jugador.transform, 2.26f, 0.24f, matOjos);

        // Camara FPS (primera persona)
        GameObject camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 75f;
        cam.nearClipPlane = 0.05f;
        camObj.AddComponent<AudioListener>();
        camObj.transform.SetParent(jugador.transform);
        camObj.transform.localPosition = new Vector3(0f, 2.08f, 0.10f);
        camObj.transform.localRotation = Quaternion.identity;

        j.camara = camObj.transform;
        Undo.RegisterCreatedObjectUndo(jugador, "Jugador");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  NPC
    // ─────────────────────────────────────────────────────────────────────
    static void CrearNPC()
    {
        GameObject npc = new GameObject("NPC");
        npc.transform.position = new Vector3(-5f, 0f, -6f);

        SphereCollider t = npc.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2.2f;
        t.center = new Vector3(0f, 1f, 0f);
        npc.AddComponent<NPC>();

        // Cuerpo tunica
        Primitiva(npc.transform, "Cuerpo", PrimitiveType.Capsule,
            new Vector3(0f, 1f, 0f), new Vector3(0.72f, 1f, 0.72f), matNPC, false);

        // Cabeza
        Primitiva(npc.transform, "Cabeza", PrimitiveType.Sphere,
            new Vector3(0f, 2.18f, 0f), Vector3.one * 0.54f, matPiel, false);

        OjoPar(npc.transform, 2.26f, 0.24f, matOjos);

        // Sombrero de mago
        Primitiva(npc.transform, "SombreroAla", PrimitiveType.Cylinder,
            new Vector3(0f, 2.38f, 0f), new Vector3(0.5f, 0.04f, 0.5f), matSombrero, false);
        Primitiva(npc.transform, "SombreroCopa", PrimitiveType.Cylinder,
            new Vector3(0f, 2.72f, 0f), new Vector3(0.28f, 0.18f, 0.28f), matSombrero, false);
        Primitiva(npc.transform, "SombreroPunta", PrimitiveType.Sphere,
            new Vector3(0f, 2.95f, 0f), new Vector3(0.20f, 0.28f, 0.20f), matSombrero, false);

        npc.transform.LookAt(new Vector3(0f, 0f, -10f));
        Undo.RegisterCreatedObjectUndo(npc, "NPC");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  PUERTA
    // ─────────────────────────────────────────────────────────────────────
    static void CrearPuerta()
    {
        // Pivote en borde izquierdo para que rote bien
        GameObject pivote = new GameObject("Puerta");
        pivote.transform.position = new Vector3(-0.5f, 0f, 8f);

        SphereCollider t = pivote.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2.5f;
        t.center = new Vector3(0.5f, 1.5f, 0f);
        pivote.AddComponent<Puerta>();

        // Panel madera
        Primitiva(pivote.transform, "PanelPuerta", PrimitiveType.Cube,
            new Vector3(0.5f, 1.5f, 0f), new Vector3(1f, 3f, 0.12f), matMadera, false);

        // Marcos dorados
        Marco(pivote.transform, new Vector3(-0.07f, 1.5f,  0f), new Vector3(0.13f, 3.15f, 0.16f));
        Marco(pivote.transform, new Vector3( 1.07f, 1.5f,  0f), new Vector3(0.13f, 3.15f, 0.16f));
        Marco(pivote.transform, new Vector3( 0.5f,  3.07f, 0f), new Vector3(1.28f, 0.13f, 0.16f));

        // Tabla horizontal decorativa
        Primitiva(pivote.transform, "TablaH", PrimitiveType.Cube,
            new Vector3(0.5f, 1.5f, -0.07f), new Vector3(0.88f, 0.08f, 0.06f), matMetal, false);

        // Pomo
        Primitiva(pivote.transform, "Pomo", PrimitiveType.Sphere,
            new Vector3(0.82f, 1.5f, 0.09f), Vector3.one * 0.12f, matMetal, false);

        // Collider físico
        BoxCollider col = pivote.AddComponent<BoxCollider>();
        col.center = new Vector3(0.5f, 1.5f, 0f);
        col.size   = new Vector3(1f, 3f, 0.14f);

        Undo.RegisterCreatedObjectUndo(pivote, "Puerta");
    }

    static void Marco(Transform p, Vector3 pos, Vector3 esc)
    {
        Primitiva(p, "Marco", PrimitiveType.Cube, pos, esc, matMetal, false);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  COFRE
    // ─────────────────────────────────────────────────────────────────────
    static void CrearCofre()
    {
        GameObject cofre = new GameObject("CofreEscena");
        cofre.transform.position = new Vector3(0f, 0f, 14f);

        SphereCollider t = cofre.AddComponent<SphereCollider>();
        t.isTrigger = true; t.radius = 2f;
        t.center = new Vector3(0f, 0.5f, 0f);
        cofre.AddComponent<Cofre>();

        // Base
        Primitiva(cofre.transform, "CofreBase", PrimitiveType.Cube,
            new Vector3(0f, 0.35f, 0f), new Vector3(1.1f, 0.7f, 0.75f), matMadera, false);
        // Tapa
        Primitiva(cofre.transform, "CofreTapa", PrimitiveType.Cube,
            new Vector3(0f, 0.82f, 0f), new Vector3(1.1f, 0.35f, 0.75f), matMadera, false);
        // Franja horizontal
        Primitiva(cofre.transform, "Franja", PrimitiveType.Cube,
            new Vector3(0f, 0.675f, 0f), new Vector3(1.12f, 0.075f, 0.77f), matMetal, false);
        // Cerradura
        Primitiva(cofre.transform, "Cerradura", PrimitiveType.Cube,
            new Vector3(0f, 0.38f, 0.395f), new Vector3(0.22f, 0.22f, 0.065f), matMetal, false);
        // Esquineros
        Esquinero(cofre.transform, new Vector3( 0.56f, 0.35f,  0.385f));
        Esquinero(cofre.transform, new Vector3(-0.56f, 0.35f,  0.385f));
        Esquinero(cofre.transform, new Vector3( 0.56f, 0.35f, -0.385f));
        Esquinero(cofre.transform, new Vector3(-0.56f, 0.35f, -0.385f));

        BoxCollider col = cofre.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 0.5f, 0f);
        col.size   = new Vector3(1.1f, 1f, 0.75f);

        PrefabUtility.SaveAsPrefabAssetAndConnect(cofre,
            "Assets/Prefabs/Cofre.prefab", InteractionMode.AutomatedAction);
        Undo.RegisterCreatedObjectUndo(cofre, "Cofre");
    }

    static void Esquinero(Transform p, Vector3 pos)
    {
        Primitiva(p, "Esquinero", PrimitiveType.Cube, pos,
            new Vector3(0.1f, 0.72f, 0.08f), matMetal, false);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  ENEMIGO
    // ─────────────────────────────────────────────────────────────────────
    static void CrearEnemigo()
    {
        Material matCabEn = MatURP("MatCabEnemigo", new Color(0.55f, 0.04f, 0.04f),
            null, "Universal Render Pipeline/Lit", 0.1f, 0.35f, 1f);
        Material matOjEn  = MatURP("MatOjosEnemigo", new Color(1f, 0.9f, 0.0f),
            null, "Universal Render Pipeline/Lit", 0.3f, 0.8f, 1f);

        GameObject en = new GameObject("Enemigo");
        en.transform.position = new Vector3(10f, 0f, 8f);

        Rigidbody rb = en.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        BoxCollider col = en.AddComponent<BoxCollider>();
        col.center = new Vector3(0f, 1f, 0f);
        col.size   = new Vector3(0.75f, 2f, 0.75f);
        en.AddComponent<Enemigo>();

        // Cuerpo
        Primitiva(en.transform, "CuerpoEn", PrimitiveType.Cube,
            new Vector3(0f, 1f, 0f), new Vector3(0.75f, 1.5f, 0.75f), matEnemigo, false);
        // Cabeza
        Primitiva(en.transform, "CabezaEn", PrimitiveType.Cube,
            new Vector3(0f, 2.05f, 0f), new Vector3(0.70f, 0.70f, 0.70f), matCabEn, false);
        // Ojos amarillos brillantes
        Primitiva(en.transform, "OjoD", PrimitiveType.Cube,
            new Vector3( 0.17f, 2.12f, 0.37f), new Vector3(0.18f, 0.13f, 0.05f), matOjEn, false);
        Primitiva(en.transform, "OjoI", PrimitiveType.Cube,
            new Vector3(-0.17f, 2.12f, 0.37f), new Vector3(0.18f, 0.13f, 0.05f), matOjEn, false);

        Undo.RegisterCreatedObjectUndo(en, "Enemigo");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  PREFAB COLECCIONABLE
    // ─────────────────────────────────────────────────────────────────────
    static void CrearPrefabColeccionable()
    {
        GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esfera.name = "Coleccionable";
        esfera.transform.localScale = Vector3.one * 0.38f;
        esfera.GetComponent<Renderer>().sharedMaterial = matColeccionable;

        esfera.GetComponent<SphereCollider>().isTrigger = true;

        Rigidbody rb = esfera.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;

        esfera.AddComponent<Coleccionable>();

        PrefabUtility.SaveAsPrefabAsset(esfera, "Assets/Prefabs/Coleccionable.prefab");
        Object.DestroyImmediate(esfera);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  SPAWNER
    // ─────────────────────────────────────────────────────────────────────
    static void CrearSpawner()
    {
        GameObject sp = new GameObject("SpawnerItems");
        sp.transform.position = new Vector3(0f, 0f, 2f);

        GeneradorItems gen = sp.AddComponent<GeneradorItems>();
        gen.rangoSpawn = 18f;
        gen.intervalo  = 2.5f;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coleccionable.prefab");
        if (prefab != null) gen.prefabItem = prefab;

        Undo.RegisterCreatedObjectUndo(sp, "SpawnerItems");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  UI
    // ─────────────────────────────────────────────────────────────────────
    static void CrearUI()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas cv = canvasObj.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Font fnt = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Panel semitransparente arriba izquierda
        GameObject panel = new GameObject("PanelHUD");
        panel.transform.SetParent(canvasObj.transform, false);
        RectTransform rp = panel.AddComponent<RectTransform>();
        rp.anchorMin = rp.anchorMax = rp.pivot = new Vector2(0, 1);
        rp.anchoredPosition = new Vector2(10, -10);
        rp.sizeDelta = new Vector2(320, 70);
        var img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0.45f);

        // Contador items
        TextoUI(panel.transform, "TextoContador",
            new Vector2(0,1), new Vector2(0,1), new Vector2(12,-8),
            new Vector2(300, 32), "Items: 0/5", 22, Color.white, fnt);

        // Objetivo
        TextoUI(panel.transform, "TextoObjetivo",
            new Vector2(0,1), new Vector2(0,1), new Vector2(12,-40),
            new Vector2(300, 26), "Recoge 5 items y abre el cofre", 15,
            new Color(1f,1f,0.6f), fnt);

        // Crosshair (punto de mira)
        TextoUI(canvasObj.transform, "Crosshair",
            new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero,
            new Vector2(20,20), "·", 28, new Color(1,1,1,0.7f), fnt);

        // Indicador de gema
        TextoUI(panel.transform, "TextoGema",
            new Vector2(0,1), new Vector2(0,1), new Vector2(12,-62),
            new Vector2(300, 22), "Gema: ✗  Busca la gema cyan", 14,
            new Color(0.4f, 1f, 0.9f), fnt);

        // Ajustar altura del panel principal
        rp.sizeDelta = new Vector2(320, 90);

        // ── PANEL STATS (abajo izquierda: vida + estamina) ──────────────
        GameObject panelStats = new GameObject("PanelStats");
        panelStats.transform.SetParent(canvasObj.transform, false);
        RectTransform rs = panelStats.AddComponent<RectTransform>();
        rs.anchorMin = rs.anchorMax = new Vector2(0, 0);
        rs.pivot     = new Vector2(0, 0);
        rs.anchoredPosition = new Vector2(14, 14);
        rs.sizeDelta = new Vector2(230, 80);
        panelStats.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.5f);

        // Barra de Vida
        TextoUI(panelStats.transform, "LabelVida",
            new Vector2(0,1), new Vector2(0,1), new Vector2(8,-8),
            new Vector2(60,18), "❤ VIDA", 13, new Color(1f,0.35f,0.35f), fnt);
        TextoUI(panelStats.transform, "TextoVida",
            new Vector2(1,1), new Vector2(1,1), new Vector2(-8,-8),
            new Vector2(60,18), "100/100", 13, Color.white, fnt);

        CrearBarra(panelStats.transform, "BarraVida", "RellenoVida",
            new Vector2(8,-28), new Vector2(214,18),
            new Color(0.25f,0.05f,0.05f), new Color(0.9f,0.15f,0.15f));

        // Barra de Estamina
        TextoUI(panelStats.transform, "LabelEstamina",
            new Vector2(0,1), new Vector2(0,1), new Vector2(8,-50),
            new Vector2(90,18), "⚡ ESTAMINA", 13, new Color(0.3f,1f,0.4f), fnt);

        CrearBarra(panelStats.transform, "BarraEstamina", "RellenoEstamina",
            new Vector2(8,-70), new Vector2(214,14),
            new Color(0.05f,0.18f,0.05f), new Color(0.2f,0.85f,0.25f));

        // Icono sprint (abajo derecha de la barra estamina)
        TextoUI(panelStats.transform, "IconoSprint",
            new Vector2(1,0), new Vector2(1,0), new Vector2(-8,8),
            new Vector2(60,16), "[SHIFT]", 11, new Color(1,1,1,0.5f), fnt);

        // ── CROSSHAIR ───────────────────────────────────────────────────
        TextoUI(canvasObj.transform, "Crosshair",
            new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero,
            new Vector2(20,20), "·", 28, new Color(1,1,1,0.7f), fnt);

        // ── MENSAJE TEMPORAL ────────────────────────────────────────────
        GameObject msgObj = TextoUI(canvasObj.transform, "TextoMensajeTemporal",
            new Vector2(.5f,.35f), new Vector2(.5f,.35f), Vector2.zero,
            new Vector2(560, 80), "", 24, Color.white, fnt);
        msgObj.SetActive(false);

        // ── VICTORIA ────────────────────────────────────────────────────
        GameObject victoria = TextoUI(canvasObj.transform, "TextoVictoria",
            new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0, 60),
            new Vector2(580, 130), "¡NIVEL COMPLETADO!\n¡Pasa por la puerta!", 44,
            new Color(1f, 0.88f, 0f), fnt);
        victoria.SetActive(false);

        ContadorItems contador = canvasObj.AddComponent<ContadorItems>();
        contador.metaItems = 5;
        canvasObj.AddComponent<HUDJugador>();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Canvas");
    }

    static void CrearBarra(Transform padre, string nombreBg, string nombreFill,
        Vector2 pos, Vector2 size, Color colorBg, Color colorFill)
    {
        // Fondo
        GameObject bg = new GameObject(nombreBg);
        bg.transform.SetParent(padre, false);
        RectTransform rb = bg.AddComponent<RectTransform>();
        rb.anchorMin = rb.anchorMax = new Vector2(0,1);
        rb.pivot = new Vector2(0,1);
        rb.anchoredPosition = pos;
        rb.sizeDelta = size;
        bg.AddComponent<UnityEngine.UI.Image>().color = colorBg;

        // Relleno
        GameObject fill = new GameObject(nombreFill);
        fill.transform.SetParent(bg.transform, false);
        RectTransform rf = fill.AddComponent<RectTransform>();
        rf.anchorMin = Vector2.zero;
        rf.anchorMax = Vector2.one;
        rf.offsetMin = rf.offsetMax = Vector2.zero;
        var imgFill = fill.AddComponent<UnityEngine.UI.Image>();
        imgFill.color = colorFill;
        imgFill.type  = UnityEngine.UI.Image.Type.Filled;
        imgFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        imgFill.fillAmount = 1f;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  GEMA ESPECIAL
    // ─────────────────────────────────────────────────────────────────────
    static void CrearGema()
    {
        Material matGema = MatURP("MatGema", new Color(0.2f, 0.95f, 0.85f),
            null, "Universal Render Pipeline/Lit", 0.4f, 0.95f, 1f);
        // Emision cyan
        matGema.EnableKeyword("_EMISSION");
        if (matGema.HasProperty("_EmissionColor"))
            matGema.SetColor("_EmissionColor", new Color(0.1f, 0.6f, 0.55f));

        // Cuerpo de la gema: dos piramides (esferas comprimidas rotadas)
        GameObject gema = new GameObject("GemaEspecial");
        gema.transform.position = new Vector3(-8f, 1.2f, 5f);

        SphereCollider col = gema.AddComponent<SphereCollider>();
        col.isTrigger = true; col.radius = 0.5f;
        gema.AddComponent<ItemEspecial>();

        // Mitad superior
        Primitiva(gema.transform, "GemaSup", PrimitiveType.Sphere,
            new Vector3(0f,  0.18f, 0f), new Vector3(0.45f, 0.45f, 0.45f), matGema, false);
        // Mitad inferior (aplastada)
        Primitiva(gema.transform, "GemaInf", PrimitiveType.Sphere,
            new Vector3(0f, -0.12f, 0f), new Vector3(0.45f, 0.25f, 0.45f), matGema, false);

        Undo.RegisterCreatedObjectUndo(gema, "GemaEspecial");
        Debug.Log("Gema especial creada en (-8, 1.2, 5).");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Crea una primitiva como hijo, sin collider propio.</summary>
    static GameObject Primitiva(Transform padre, string nombre, PrimitiveType tipo,
        Vector3 posLocal, Vector3 escala, Material mat, bool mantenerCollider)
    {
        GameObject go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre);
        go.transform.localPosition = posLocal;
        go.transform.localScale    = escala;
        go.GetComponent<Renderer>().sharedMaterial = mat;

        if (!mantenerCollider)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }
        return go;
    }

    static void OjoPar(Transform padre, float y, float z, Material mat)
    {
        Primitiva(padre, "OjoD", PrimitiveType.Sphere,
            new Vector3( 0.12f, y, z), Vector3.one * 0.09f, mat, false);
        Primitiva(padre, "OjoI", PrimitiveType.Sphere,
            new Vector3(-0.12f, y, z), Vector3.one * 0.09f, mat, false);
    }

    static GameObject TextoUI(Transform padre, string nombre,
        Vector2 ancMin, Vector2 ancMax, Vector2 pos, Vector2 size,
        string contenido, int fontSize, Color color, Font font)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        RectTransform r = obj.AddComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.pivot = ancMin; r.anchoredPosition = pos; r.sizeDelta = size;

        Text t = obj.AddComponent<Text>();
        t.text = contenido; t.fontSize = fontSize;
        t.color = color; t.font = font;
        t.alignment = TextAnchor.MiddleCenter;
        return obj;
    }
}
