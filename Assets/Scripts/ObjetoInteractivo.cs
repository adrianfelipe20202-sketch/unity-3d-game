using UnityEngine;

// Clase base para todos los objetos interactivos del escenario
// REFACTORIZACION: el rango de interaccion y el indicador visual
// estaban repetidos en Puerta y Cofre — se movieron aqui
public class ObjetoInteractivo : MonoBehaviour
{
    [Header("Interaccion")]
    public float rangoInteraccion = 2f;
    public string textoIndicador = "Presiona E para interactuar";

    private bool _jugadorCerca = false;

    protected virtual void Update()
    {
        if (_jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            Interactuar();
        }
    }

    public virtual void Interactuar()
    {
        Debug.Log("Interaccion generica con " + gameObject.name);
    }

    public virtual void MostrarIndicador()
    {
        Debug.Log(textoIndicador);
    }

    public virtual void OcultarIndicador()
    {
        // Ocultar UI
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _jugadorCerca = true;
            MostrarIndicador();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _jugadorCerca = false;
            OcultarIndicador();
        }
    }
}
