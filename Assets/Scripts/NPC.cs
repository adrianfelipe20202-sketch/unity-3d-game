using UnityEngine;

// Nuevo objeto pedido por la actividad: NPC
// Hereda de ObjetoInteractivo (usa el sistema de tecla E ya implementado)
public class NPC : ObjetoInteractivo
{
    [Header("Dialogo")]
    [TextArea(2, 5)]
    public string[] lineasDeDialogo = {
        "Hola aventurero, bienvenido a esta zona.",
        "Ten cuidado con los enemigos que rondan por aqui.",
        "Dicen que hay un cofre escondido detras de la puerta grande."
    };

    private int _lineaActual = 0;

    private void Start()
    {
        textoIndicador = "Presiona E para hablar";
    }

    public override void Interactuar()
    {
        if (lineasDeDialogo.Length == 0) return;

        Debug.Log($"NPC: {lineasDeDialogo[_lineaActual]}");
        _lineaActual = (_lineaActual + 1) % lineasDeDialogo.Length;
    }
}
