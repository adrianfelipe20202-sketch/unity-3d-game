using UnityEngine;
using UnityEngine.UI;

public class HUDJugador : MonoBehaviour
{
    RectTransform _rtVida;
    RectTransform _rtEstamina;
    Text          _textoVida;
    Jugador       _jugador;

    void Start()
    {
        _jugador    = FindObjectOfType<Jugador>();
        _rtVida     = BuscarRT("RellenoVida");
        _rtEstamina = BuscarRT("RellenoEstamina");
        _textoVida  = BuscarTexto("TextoVida");
    }

    void Update()
    {
        if (_jugador == null) { _jugador = FindObjectOfType<Jugador>(); return; }

        ActualizarBarra(_rtVida,     _jugador.VidaNormalizada);
        ActualizarBarra(_rtEstamina, _jugador.EstaminaNormalizada);

        // Color estamina: verde → amarillo → rojo
        var imgEst = _rtEstamina?.GetComponent<Image>();
        if (imgEst != null)
        {
            float t = _jugador.EstaminaNormalizada;
            imgEst.color = t > 0.5f
                ? Color.Lerp(new Color(0.9f, 0.85f, 0f), new Color(0.15f, 0.85f, 0.2f), (t - 0.5f) * 2f)
                : Color.Lerp(new Color(0.9f, 0.1f, 0.05f), new Color(0.9f, 0.85f, 0f), t * 2f);
        }

        if (_textoVida != null)
            _textoVida.text = $"{_jugador.Vida} / {_jugador.VidaMax}";
    }

    // Mueve el borde derecho del relleno usando anchorMax — no depende de fillAmount
    void ActualizarBarra(RectTransform rt, float valor)
    {
        if (rt == null) return;
        valor = Mathf.Clamp01(valor);
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(valor, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    RectTransform BuscarRT(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        return go != null ? go.GetComponent<RectTransform>() : null;
    }

    Text BuscarTexto(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        return go != null ? go.GetComponent<Text>() : null;
    }
}
