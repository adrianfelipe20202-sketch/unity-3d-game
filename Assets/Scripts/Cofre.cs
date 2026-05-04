using UnityEngine;

public class Cofre : ObjetoInteractivo
{
    public int itemsRequeridos = 5;
    private bool _abierto = false;

    private void Start()
    {
        textoIndicador = $"Presiona E para abrir el cofre";
    }

    public override void Interactuar()
    {
        if (_abierto)
        {
            ContadorItems.MostrarMensajeTemporal("¡Puedes continuar al siguiente nivel!\nPasa por la puerta.");
            return;
        }

        if (ContadorItems.itemsRecogidos < itemsRequeridos)
        {
            int faltan = itemsRequeridos - ContadorItems.itemsRecogidos;
            ContadorItems.MostrarMensajeTemporal($"Faltan {faltan} esferas doradas.");
            return;
        }

        if (!GestorJuego.gemaRecogida)
        {
            ContadorItems.MostrarMensajeTemporal("Falta la gema especial. ¡Búscala!");
            return;
        }

        // ¡Abrir cofre!
        _abierto = true;
        GestorJuego.cofreAbierto = true;
        AbrirAnimacion();
        ContadorItems.MostrarVictoria();
        Debug.Log("¡Cofre abierto! Nivel completado.");
    }

    void AbrirAnimacion()
    {
        // Rotar la tapa
        Transform tapa = transform.Find("CofreTapa");
        if (tapa != null)
            tapa.Rotate(-65f, 0f, 0f);

        // Efecto de particulas doradas
        GameObject go = new GameObject("FX_Cofre");
        go.transform.position = transform.position + Vector3.up;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main        = ps.main;
        main.startColor    = new Color(1f, 0.85f, 0.1f);
        main.startSize     = 0.2f;
        main.startSpeed    = 5f;
        main.startLifetime = 1f;
        main.duration      = 0.5f;
        main.loop          = false;
        main.maxParticles  = 60;
        main.gravityModifier = -0.3f;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = new Vector3(1f, 0.5f, 0.75f);

        ps.Play();
        Destroy(go, 2.5f);
    }
}
