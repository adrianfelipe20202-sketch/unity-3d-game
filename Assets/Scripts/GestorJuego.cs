using UnityEngine;

/// <summary>Estado global del juego compartido entre escenas.</summary>
public static class GestorJuego
{
    public static bool cofreAbierto    = false;
    public static bool gemaRecogida    = false;
    public static int  itemsMeta       = 5;

    public static bool PuedeAbrirCofre =>
        ContadorItems.itemsRecogidos >= itemsMeta && gemaRecogida;

    public static void Reiniciar()
    {
        cofreAbierto = false;
        gemaRecogida = false;
        ContadorItems.itemsRecogidos = 0;
    }
}
