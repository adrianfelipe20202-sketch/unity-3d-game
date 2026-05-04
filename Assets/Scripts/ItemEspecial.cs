using UnityEngine;

/// <summary>
/// Gema especial que hay que recoger y llevar al cofre.
/// Flota y brilla. Al recogerla se guarda en GestorJuego.
/// </summary>
public class ItemEspecial : MonoBehaviour
{
    public float velocidadRotacion  = 60f;
    public float alturaFlotacion    = 0.4f;
    public float velocidadFlotacion = 1.5f;

    private Vector3 posInicial;

    void Start() => posInicial = transform.position;

    void Update()
    {
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        transform.Rotate(Vector3.right * (velocidadRotacion * 0.4f) * Time.deltaTime);

        float y = posInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GestorJuego.gemaRecogida = true;
        ContadorItems.MostrarMensajeTemporal("¡Gema recogida! Llévala al cofre.");
        SpawnParticulas();
        Destroy(gameObject);
    }

    void SpawnParticulas()
    {
        GameObject go = new GameObject("FX_Gema");
        go.transform.position = transform.position;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main        = ps.main;
        main.startColor    = new Color(0.3f, 1f, 0.9f);
        main.startSize     = 0.18f;
        main.startSpeed    = 4f;
        main.startLifetime = 0.7f;
        main.duration      = 0.3f;
        main.loop          = false;
        main.maxParticles  = 35;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 35) });

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = 0.3f;

        ps.Play();
        Destroy(go, 1.5f);
    }
}
