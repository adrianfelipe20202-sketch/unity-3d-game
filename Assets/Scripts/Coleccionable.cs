using UnityEngine;

public class Coleccionable : MonoBehaviour
{
    [Header("Animacion")]
    public float velocidadRotacion  = 90f;
    public float alturaFlotacion    = 0.3f;
    public float velocidadFlotacion = 2f;

    // GeneradorItems se suscribe aqui para saber cuando spawnear el siguiente
    public static System.Action OnRecogido;

    private Vector3 posicionInicial;
    private bool _usarLocal;

    void Start()
    {
        _usarLocal = transform.parent != null;
        posicionInicial = _usarLocal ? transform.localPosition : transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        float y = posicionInicial.y + Mathf.Sin(Time.time * velocidadFlotacion) * alturaFlotacion;
        if (_usarLocal)
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        else
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ContadorItems.Sumar();
        OnRecogido?.Invoke();
        SpawnParticulas();
        Destroy(gameObject);
    }

    void SpawnParticulas()
    {
        GameObject go = new GameObject("FX");
        go.transform.position = transform.position;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main        = ps.main;
        main.startColor    = new Color(1f, 0.85f, 0f);
        main.startSize     = 0.12f;
        main.startSpeed    = 3.5f;
        main.startLifetime = 0.55f;
        main.duration      = 0.25f;
        main.loop          = false;
        main.maxParticles  = 25;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Sphere;
        sh.radius = 0.2f;

        ps.Play();
        Destroy(go, 1.2f);
    }
}
