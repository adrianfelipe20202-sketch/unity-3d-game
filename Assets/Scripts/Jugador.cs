using UnityEngine;

public class Jugador : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float _velocidad       = 6f;
    [SerializeField] private float _velocidadSprint = 11f;
    [SerializeField] private float _gravedad        = -20f;
    [SerializeField] private float _alturaSalto     = 3.5f;

    [Header("Camara")]
    public Transform camara;
    public float sensibilidadRaton = 120f;

    [Header("Vida")]
    [SerializeField] private int _vidaMaxima = 100;
    [SerializeField] private int _vida       = 100;

    [Header("Estamina")]
    [SerializeField] private float _estaminaMax      = 100f;
    [SerializeField] private float _estamina         = 100f;
    [SerializeField] private float _consumoEstamina  = 40f;
    [SerializeField] private float _recargarEstamina = 8f;
    [SerializeField] private float _estaminaMinSprint = 15f;

    // Propiedades para el HUD
    public int   Vida                => _vida;
    public int   VidaMax             => _vidaMaxima;
    public float Estamina            => _estamina;
    public float EstaminaMax         => _estaminaMax;
    public float VidaNormalizada     => (float)_vida / _vidaMaxima;
    public float EstaminaNormalizada => _estamina / _estaminaMax;
    public bool  EstaVivo            => _vida > 0;
    public bool  EstaSprintando      => _sprintando;

    private CharacterController _cc;
    private Vector3 _velVertical;
    private float   _rotacionX;
    private bool    _sprintando;
    private bool    _puedeSprintar = true;
    private bool    _estabaMuerto  = false;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        if (!EstaVivo)
        {
            if (!_estabaMuerto)
            {
                _estabaMuerto = true;
                PantallaDerrota.Mostrar();
            }
            return;
        }

        ManejarSprint();
        Mover();
        MirarConRaton();
    }

    void ManejarSprint()
    {
        bool quiereSprint = Input.GetKey(KeyCode.LeftShift);

        if (quiereSprint && _puedeSprintar && _estamina > 0f)
        {
            _sprintando = true;
            _estamina  -= _consumoEstamina * Time.deltaTime;
            if (_estamina <= 0f)
            {
                _estamina      = 0f;
                _sprintando    = false;
                _puedeSprintar = false;
            }
        }
        else
        {
            _sprintando = false;
            _estamina  += _recargarEstamina * Time.deltaTime;
            _estamina   = Mathf.Min(_estamina, _estaminaMax);
            if (!_puedeSprintar && _estamina >= _estaminaMinSprint)
                _puedeSprintar = true;
        }
    }

    void Mover()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = transform.right * h + transform.forward * v;
        _cc.Move(dir * (_sprintando ? _velocidadSprint : _velocidad) * Time.deltaTime);

        // Gravedad
        bool pisando = EstaEnSuelo();
        if (pisando && _velVertical.y < 0f)
            _velVertical.y = -4f;

        // Salto — funciona tanto con isGrounded como con raycast
        if ((pisando || _cc.isGrounded) && Input.GetButtonDown("Jump"))
            _velVertical.y = Mathf.Sqrt(_alturaSalto * -2f * _gravedad);

        _velVertical.y += _gravedad * Time.deltaTime;
        _cc.Move(_velVertical * Time.deltaTime);
    }

    // Raycast al suelo más fiable que isGrounded solo
    bool EstaEnSuelo()
    {
        Vector3 origen = transform.position + Vector3.up * 0.1f;
        return Physics.SphereCast(origen, 0.35f, Vector3.down, out _, 0.3f,
            ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore);
    }

    void MirarConRaton()
    {
        if (camara == null) return;
        float mx = Input.GetAxis("Mouse X") * sensibilidadRaton * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * sensibilidadRaton * Time.deltaTime;
        _rotacionX -= my;
        _rotacionX  = Mathf.Clamp(_rotacionX, -80f, 80f);
        camara.localRotation = Quaternion.Euler(_rotacionX, 0f, 0f);
        transform.Rotate(Vector3.up * mx);
    }

    public void RecibirDanio(int cantidad)
    {
        if (!EstaVivo) return;
        _vida = Mathf.Clamp(_vida - cantidad, 0, _vidaMaxima);
    }

    public void Curar(int cantidad)
    {
        _vida = Mathf.Clamp(_vida + cantidad, 0, _vidaMaxima);
    }
}
