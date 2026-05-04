using System.Collections.Generic;
using UnityEngine;

public class MovimientoPingPongLineal : MonoBehaviour
{
    public enum Modo { Lineal, PingPong }

    [Header("Movimiento")]
    public Modo modo = Modo.PingPong;
    public Vector3 puntoA = Vector3.zero;
    public Vector3 puntoB = new Vector3(0f, 0f, 6f);
    public float duracion = 2.5f;
    public bool usarEspacioLocal = false;
    public bool iniciarDesdeA = true;

    [Header("Transportar jugador/rigidbodies")]
    public bool transportarSobrePlataforma = true;
    public float triggerAltura = 0.65f;
    public float triggerMargenX = 0.05f;
    public float triggerMargenZ = 0.05f;

    Vector3 _posAnterior;
    BoxCollider _trigger;
    readonly HashSet<CharacterController> _ccs = new HashSet<CharacterController>();
    readonly HashSet<Rigidbody> _rbs = new HashSet<Rigidbody>();

    void Awake()
    {
        _posAnterior = transform.position;

        if (!transportarSobrePlataforma) return;

        // Trigger superior para detectar quién está "encima"
        var go = new GameObject("TriggerPlataforma");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.up * triggerAltura;

        _trigger = go.AddComponent<BoxCollider>();
        _trigger.isTrigger = true;
        AjustarTrigger();

        go.AddComponent<DetectorEncima>().Init(this);
    }

    void OnValidate()
    {
        if (_trigger != null) AjustarTrigger();
    }

    void AjustarTrigger()
    {
        // Si el objeto tiene collider, usamos su tamaño como referencia.
        var col = GetComponent<Collider>();
        if (col is BoxCollider bc)
        {
            var s = bc.size;
            _trigger.size = new Vector3(
                Mathf.Max(0.1f, s.x - triggerMargenX * 2f),
                0.8f,
                Mathf.Max(0.1f, s.z - triggerMargenZ * 2f)
            );
        }
        else
        {
            _trigger.size = new Vector3(3f, 0.8f, 3f);
        }
    }

    void Update()
    {
        float t = duracion <= 0.01f ? 1f : (Time.time / duracion);

        Vector3 a = puntoA;
        Vector3 b = puntoB;
        if (iniciarDesdeA == false)
        {
            a = puntoB;
            b = puntoA;
        }

        float u;
        if (modo == Modo.PingPong)
            u = Mathf.PingPong(t, 1f);
        else
            u = t - Mathf.Floor(t); // loop 0..1

        Vector3 objetivo = Vector3.LerpUnclamped(a, b, u);
        if (usarEspacioLocal) transform.localPosition = objetivo;
        else transform.position = objetivo;
    }

    void LateUpdate()
    {
        if (!transportarSobrePlataforma) { _posAnterior = transform.position; return; }

        Vector3 delta = transform.position - _posAnterior;
        if (delta.sqrMagnitude < 0.0000001f) return;

        foreach (var cc in _ccs)
        {
            if (cc == null) continue;
            cc.Move(delta);
        }

        foreach (var rb in _rbs)
        {
            if (rb == null) continue;
            rb.MovePosition(rb.position + delta);
        }

        _posAnterior = transform.position;
    }

    void Registrar(Collider c)
    {
        if (c == null) return;
        var cc = c.GetComponentInParent<CharacterController>();
        if (cc != null) _ccs.Add(cc);

        var rb = c.GetComponentInParent<Rigidbody>();
        if (rb != null && rb.isKinematic == false) _rbs.Add(rb);
    }

    void Desregistrar(Collider c)
    {
        if (c == null) return;
        var cc = c.GetComponentInParent<CharacterController>();
        if (cc != null) _ccs.Remove(cc);

        var rb = c.GetComponentInParent<Rigidbody>();
        if (rb != null) _rbs.Remove(rb);
    }

    class DetectorEncima : MonoBehaviour
    {
        MovimientoPingPongLineal _m;
        public void Init(MovimientoPingPongLineal m) => _m = m;
        void OnTriggerEnter(Collider other) => _m?.Registrar(other);
        void OnTriggerExit(Collider other) => _m?.Desregistrar(other);
    }
}

