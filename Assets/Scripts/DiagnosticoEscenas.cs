using UnityEngine;
using UnityEngine.SceneManagement;

/// Script temporal de diagnostico — borrar despues
public class DiagnosticoEscenas : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"=== DIAGNOSTICO ===");
        Debug.Log($"Escena actual: {SceneManager.GetActiveScene().name} (index {SceneManager.GetActiveScene().buildIndex})");
        Debug.Log($"Total escenas en Build Settings: {SceneManager.sceneCountInBuildSettings}");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log($"  [{i}] {path}");
        }
        Debug.Log($"GestorJuego.cofreAbierto = {GestorJuego.cofreAbierto}");
    }

    void Update()
    {
        // Presiona F1 para ver estado actual en cualquier momento
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"cofreAbierto={GestorJuego.cofreAbierto} | gemaRecogida={GestorJuego.gemaRecogida} | items={ContadorItems.itemsRecogidos}");
        }
    }
}
