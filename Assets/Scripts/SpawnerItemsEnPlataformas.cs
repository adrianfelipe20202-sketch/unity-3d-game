using System.Collections.Generic;
using UnityEngine;

public class SpawnerItemsEnPlataformas : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject prefabItem;

    [Header("Plataformas (targets)")]
    public List<Transform> plataformas = new List<Transform>();

    [Header("Reglas")]
    public int maxItems = 10;
    public float alturaSobrePlataforma = 0.65f;
    public float margenBorde = 0.6f;
    public float reintentoDelay = 0.5f;

    bool _hayUnoActivo;

    void OnEnable()  => Coleccionable.OnRecogido += AlRecogerUno;
    void OnDisable() => Coleccionable.OnRecogido -= AlRecogerUno;

    void Start()
    {
        Invoke(nameof(SpawnSiguiente), 0.8f);
    }

    void AlRecogerUno()
    {
        _hayUnoActivo = false;
        if (ContadorItems.itemsRecogidos < maxItems)
            Invoke(nameof(SpawnSiguiente), 0.5f);
    }

    void SpawnSiguiente()
    {
        if (_hayUnoActivo) return;
        if (prefabItem == null) return;
        if (ContadorItems.itemsRecogidos >= maxItems) return;
        if (plataformas == null || plataformas.Count == 0) return;

        // Escoger plataforma válida
        Transform plat = null;
        for (int i = 0; i < 10; i++)
        {
            var candidata = plataformas[Random.Range(0, plataformas.Count)];
            if (candidata != null) { plat = candidata; break; }
        }
        if (plat == null) return;

        Vector3 spawnPos = CalcularPosEnPlataforma(plat);
        var item = Instantiate(prefabItem, spawnPos, Quaternion.identity, plat);

        // Si el prefab tiene rigidbody, lo dejamos kinematic para evitar física rara sobre plataformas.
        var rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        _hayUnoActivo = true;
    }

    Vector3 CalcularPosEnPlataforma(Transform plat)
    {
        // Intento: usar BoxCollider si existe para obtener el área "encima".
        var bc = plat.GetComponent<BoxCollider>();
        if (bc != null)
        {
            var center = plat.TransformPoint(bc.center);
            var size = Vector3.Scale(bc.size, plat.lossyScale);
            float halfX = Mathf.Max(0.2f, size.x * 0.5f - margenBorde);
            float halfZ = Mathf.Max(0.2f, size.z * 0.5f - margenBorde);

            float x = Random.Range(-halfX, halfX);
            float z = Random.Range(-halfZ, halfZ);
            return new Vector3(center.x + x, center.y + size.y * 0.5f + alturaSobrePlataforma, center.z + z);
        }

        // Fallback: spawn en el centro.
        return plat.position + Vector3.up * alturaSobrePlataforma;
    }
}

