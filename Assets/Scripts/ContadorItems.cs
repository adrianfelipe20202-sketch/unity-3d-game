using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ContadorItems : MonoBehaviour
{
    public static int itemsRecogidos = 0;
    public int metaItems = 5;

    static Text _textoContador;
    static Text _textoObjetivo;
    static Text _textoMensaje;
    static GameObject _panelVictoria;

    static float _tiempoMensaje = 0f;
    static string _mensajePendiente = "";

    void Start()
    {
        itemsRecogidos = 0;
        _textoContador = Buscar("TextoContador");
        _textoObjetivo = Buscar("TextoObjetivo");
        _textoMensaje  = Buscar("TextoMensajeTemporal");
        _panelVictoria = GameObject.Find("TextoVictoria");

        if (_panelVictoria != null) _panelVictoria.SetActive(false);
        if (_textoMensaje  != null) _textoMensaje.gameObject.SetActive(false);
        Actualizar();
    }

    void Update()
    {
        Actualizar();

        // Mensaje temporal
        if (_tiempoMensaje > 0f)
        {
            _tiempoMensaje -= Time.deltaTime;
            if (_tiempoMensaje <= 0f && _textoMensaje != null)
                _textoMensaje.gameObject.SetActive(false);
        }
    }

    void Actualizar()
    {
        if (_textoContador != null)
            _textoContador.text = $"Items: {itemsRecogidos}/{metaItems}";

        if (_textoObjetivo != null)
        {
            if (!GestorJuego.gemaRecogida && itemsRecogidos >= metaItems)
                _textoObjetivo.text = "¡Busca la gema especial!";
            else if (GestorJuego.cofreAbierto)
                _textoObjetivo.text = "¡Ve a la puerta para avanzar!";
            else
                _textoObjetivo.text = "Recoge 5 orbes + la gema → abre el cofre";
        }
    }

    static Text Buscar(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        return go != null ? go.GetComponent<Text>() : null;
    }

    public static void Sumar() => itemsRecogidos++;

    public static void MostrarMensajeTemporal(string msg, float duracion = 3f)
    {
        if (_textoMensaje == null) _textoMensaje = Buscar("TextoMensajeTemporal");
        if (_textoMensaje == null) { Debug.Log(msg); return; }

        _textoMensaje.text = msg;
        _textoMensaje.gameObject.SetActive(true);
        _tiempoMensaje = duracion;
    }

    public static void MostrarVictoria()
    {
        if (_panelVictoria == null) _panelVictoria = GameObject.Find("TextoVictoria");
        if (_panelVictoria != null)
        {
            _panelVictoria.SetActive(true);
            var t = _panelVictoria.GetComponent<UnityEngine.UI.Text>();
            string siguiente = NombreSiguienteEscena() ?? "siguiente nivel";
            if (t != null) t.text = $"¡FELICIDADES!\n¡PUEDES CONTINUAR A {siguiente.ToUpper()}!\nVe a la puerta.";
        }
        {
            string siguiente = NombreSiguienteEscena() ?? "siguiente nivel";
            MostrarMensajeTemporal($"¡FELICIDADES!\nPuedes continuar a {siguiente} — pasa por la puerta.", 999f);
        }
    }

    static string NombreSiguienteEscena()
    {
        int actual = SceneManager.GetActiveScene().buildIndex;
        int sig = actual + 1;
        if (sig < 0 || sig >= SceneManager.sceneCountInBuildSettings) return null;
        string path = SceneUtility.GetScenePathByBuildIndex(sig);
        if (string.IsNullOrEmpty(path)) return null;
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }
}
