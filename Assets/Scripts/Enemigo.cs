using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Estadisticas")]
    [SerializeField] private float _velocidad     = 2.5f;
    [SerializeField] private int   _danio         = 10;
    [SerializeField] private float _rangoAtaque   = 1.3f;
    [SerializeField] private float _cooldownAtaque = 1.2f;  // segundos entre golpes

    private Transform _jugador;
    private Jugador   _scriptJugador;
    private float     _tiempoUltimoGolpe;

    void Start()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            _jugador       = jugadorObj.transform;
            _scriptJugador = jugadorObj.GetComponent<Jugador>();
        }
    }

    void Update()
    {
        if (_jugador == null || _scriptJugador == null || !_scriptJugador.EstaVivo) return;

        float distancia = Vector3.Distance(transform.position, _jugador.position);

        // Perseguir solo en el plano XZ (no vuela ni sube)
        Vector3 dir = (_jugador.position - transform.position);
        dir.y = 0f;
        if (dir.magnitude > 0.1f)
        {
            transform.position += dir.normalized * _velocidad * Time.deltaTime;
            transform.LookAt(new Vector3(_jugador.position.x, transform.position.y, _jugador.position.z));
        }

        // Atacar si está cerca y pasó el cooldown
        if (distancia <= _rangoAtaque && Time.time >= _tiempoUltimoGolpe + _cooldownAtaque)
        {
            _tiempoUltimoGolpe = Time.time;
            _scriptJugador.RecibirDanio(_danio);
            Debug.Log($"Enemigo golpeó al jugador. Vida restante: {_scriptJugador.Vida}");
        }
    }
}
