using UnityEngine;
using UnityEngine.SceneManagement;

public class Puerta : ObjetoInteractivo
{
    private bool _abierta  = false;
    private bool _cargando = false;

    private void Start()
    {
        textoIndicador = "Presiona E para abrir la puerta";
    }

    public override void Interactuar()
    {
        if (_cargando) return;

        if (GestorJuego.cofreAbierto)
        {
            _cargando = true;
            string nombreSiguiente = NombreSiguienteEscena() ?? "siguiente nivel";
            ContadorItems.MostrarMensajeTemporal($"¡FELICIDADES! Cargando {nombreSiguiente}...", 3f);
            Invoke(nameof(CargarNivel2), 1.5f);
            return;
        }

        _abierta = !_abierta;
        transform.Rotate(0f, _abierta ? 90f : -90f, 0f);
    }

    void CargarNivel2()
    {
        GestorJuego.Reiniciar();
        // Carga por índice — siempre funciona si Build Settings está correcto
        int indiceActual = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(indiceActual + 1);
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
