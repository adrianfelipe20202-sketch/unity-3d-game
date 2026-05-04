using UnityEngine;

public class GeneradorItems : MonoBehaviour
{
    public GameObject prefabItem;
    public float      rangoSpawn = 18f;
    public float      intervalo  = 2.5f;   // solo para el primer spawn con delay
    public int        maxItems   = 5;

    private int _spawneados = 0;
    private bool _hayUnoActivo = false;

    void OnEnable()  => Coleccionable.OnRecogido += AlRecogerUno;
    void OnDisable() => Coleccionable.OnRecogido -= AlRecogerUno;

    void Start()
    {
        // Pequeño delay para que la escena cargue completamente
        Invoke(nameof(SpawnSiguiente), 1f);
    }

    void AlRecogerUno()
    {
        _hayUnoActivo = false;
        if (ContadorItems.itemsRecogidos < maxItems)
            Invoke(nameof(SpawnSiguiente), 0.8f); // breve pausa antes del siguiente
    }

    void SpawnSiguiente()
    {
        if (_hayUnoActivo) return;
        if (prefabItem == null) return;
        if (ContadorItems.itemsRecogidos >= maxItems) return;

        Vector3 pos;
        int intentos = 0;
        do
        {
            pos = transform.position + new Vector3(
                Random.Range(-rangoSpawn, rangoSpawn),
                1f,
                Random.Range(-rangoSpawn, rangoSpawn)
            );
            intentos++;
        }
        while (pos.magnitude > 26f && intentos < 10); // mantenerse dentro del mapa

        Instantiate(prefabItem, pos, Quaternion.identity);
        _hayUnoActivo = true;
        _spawneados++;
    }
}
