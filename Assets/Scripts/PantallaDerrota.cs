using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Muestra la pantalla de derrota cuando el jugador muere.
/// Se crea dinámicamente, no necesita configuración previa.
/// </summary>
public class PantallaDerrota : MonoBehaviour
{
    static PantallaDerrota _instancia;
    bool _mostrada = false;

    public static void Mostrar()
    {
        if (_instancia != null) return;

        GameObject go = new GameObject("PantallaDerrota");
        _instancia = go.AddComponent<PantallaDerrota>();
    }

    void Start()
    {
        CrearUI();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void CrearUI()
    {
        // Canvas encima de todo
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        Font fnt = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Fondo rojo oscuro semitransparente
        GameObject fondo = new GameObject("FondoDerrota");
        fondo.transform.SetParent(transform, false);
        var rFondo = fondo.AddComponent<RectTransform>();
        rFondo.anchorMin = Vector2.zero;
        rFondo.anchorMax = Vector2.one;
        rFondo.offsetMin = rFondo.offsetMax = Vector2.zero;
        var imgFondo = fondo.AddComponent<Image>();
        imgFondo.color = new Color(0.55f, 0f, 0f, 0.82f);

        // Texto DERROTADO
        CrearTexto("TextoDerrota", "¡DERROTADO!", 64,
            new Color(1f, 0.15f, 0.15f), new Vector2(0, 60));

        // Subtexto
        CrearTexto("SubtextoDerrota", "El enemigo acabó contigo.", 28,
            new Color(1f, 0.7f, 0.7f), new Vector2(0, -10));

        // Botón reintentar
        CrearTexto("HintReintentar", "Presiona  R  para reintentar", 22,
            new Color(1f, 1f, 0.5f), new Vector2(0, -70));
    }

    void CrearTexto(string nombre, string contenido, int size, Color color, Vector2 offset)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot     = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = offset;
        r.sizeDelta = new Vector2(700, 90);
        var t = go.AddComponent<Text>();
        t.text      = contenido;
        t.fontSize  = size;
        t.color     = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _instancia = null;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
