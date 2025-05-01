using UnityEngine;
using System.Collections;

public class BicycleController : MonoBehaviour, Iinterectable
{
    [Header("Bisiklet Temel Ayarlarý")]
    [SerializeField] private float _maxSpeed = 10f; // Maksimum hareket hýzý
    [SerializeField] private float _acceleration = 5f; // Hýzlanma gücü


    [SerializeField] private float _gravity = -9.81f; // Yerçekimi kuvveti
    [SerializeField] private float _cameraSensitivity = 100f; // Kamera hassasiyeti

    [Header("Dönüþ Ayarlarý")]
    [SerializeField] private float _handlingThreshold = 3f; // Dönüþ hassasiyeti eþik deðeri
    [SerializeField][Range(0.1f, 2f)] private float _minHandling = 0.5f; // Minimum dönüþ hassasiyeti
    [SerializeField][Range(1f, 5f)] private float _maxHandling = 1.5f; // Maksimum dönüþ hassasiyeti
    [SerializeField] private float _tiltAngleMultiplier = 30f; // Eðim açý çarpaný

    [Header("Referanslar")]
    [SerializeField] private Transform _dropOfPoint; // Ýnme noktasý
    [SerializeField] private GameObject _vehicleCamera; // Araç kamerasý

    [Header("Serbest Bisiklet Fiziði")]
    [SerializeField] private float _dragWhenAbandoned = 0.3f; // Sürtünme katsayýsý
    [SerializeField] private float _angularDragWhenAbandoned = 0.1f; // Dönme sürtünmesi

    [Header("Bisiklet Düzeltme Ayarlarý")]
    [SerializeField] private float _standUpSpeed = 2f; // Düzeltme hýzý
    [SerializeField] private float _maxTiltAngleToMount = 30f; // Binilebilir maksimum eðim açýs

    [Header("Collider Ayarlarý")]
    [SerializeField] private float _groundContactOffset = 0.1f; // Collider'ýn zeminden yüksekliði
    [SerializeField] private float _minGroundContact = 0.3f; // Minimum temas oraný (0.3 = %30)

    private Rigidbody _rb;
    private float _currentSpeed = 0f;
    private float _xRotation = 0f;
    private float _handling;
    private bool _playerOnboard = false;
    private GameObject _player;
    private bool _isStandingUp = false; // Bisiklet düzeltiliyor mu?

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate; // Yumuþak fizik hareketi
        _rb.freezeRotation = true; // Fiziksel dönüþü kilitle
        _player = GameObject.FindGameObjectWithTag("Player");
        _vehicleCamera.SetActive(false); // Baþlangýçta kamera kapalý
    }

    private void FixedUpdate()
    {
        if (!_playerOnboard)
        {
            // Bisiklet terk edilmiþse, yavaþ yavaþ durmasýný saðla (sürtünme uygula)
            _rb.drag = _dragWhenAbandoned; 
            _rb.angularDrag = _angularDragWhenAbandoned; 
        }
        else
        {
            // Biniliyken sürtünmeyi sýfýrla (normal fizik kurallarý geçerli)
            _rb.drag = 0f;
            _rb.angularDrag = 0.05f;
        }
    }

    public void Interact()
    {
        // burada karakter bisiklette deðilse ve bisiklet düzeltme iþlemi yapýlmýyorsa bisiklete bin
        if (!_playerOnboard && CanMountBicycle()) // bisikletin bulunduðu eðimin binmeye uygun olup olmadýðýný kontrol eden fonk
        {
            EnterVehicle(); // þartlar saðlandýysa bin
        }
    }

    private void Update()
    {
        if (_playerOnboard)
        {
            HandleVehicleMovement(); // 
            HandleCameraLook();      // 
            if (Input.GetKeyDown(KeyCode.E)) ExitVehicle(); // "keyCode" -> "KeyCode" düzeltildi
        }
        else
        {
            // Bisiklet düzeltme kontrolü ama burada niye þartsýz koþulsuz düzeltme koyduk?
            if (Input.GetKeyDown(KeyCode.R)) // Eksik parantez eklendi
            {
                StartCoroutine(StandUpBicycle()); // "StartCorouting" -> "StartCoroutine" düzeltildi
            }
        }

        Debug.Log("Dönüþ Hassasiyeti:" + _handling + "    Hýz:" + _currentSpeed); // Eksik string birleþtirme düzeltildi
    }

    private bool CanMountBicycle()
    {
        // Bisikletin eðim açýsýný kontrol et
        float tiltAngle = Mathf.Abs(transform.rotation.eulerAngles.z);
        tiltAngle = tiltAngle > 180f ? 360f - tiltAngle : tiltAngle;

        return tiltAngle <= _maxTiltAngleToMount && !_isStandingUp;
    }

    private IEnumerator StandUpBicycle()
    {
        _isStandingUp = true;
        _rb.freezeRotation = false; // Düzeltme sýrasýnda rotasyon kilidini aç

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _rb.freezeRotation = true; // Ýþlem bitince tekrar kilitle
        _isStandingUp = false;
    }

    private void HandleVehicleMovement()
    {
        bool isGrounded = IsGrounded();

        // Yerçekimi uygula (daha gerçekçi)
        if (!isGrounded)
        {
            _rb.AddForce(Vector3.down * _gravity * 2f, ForceMode.Acceleration);
        }

        // Hareket sadece yeterli zemine temas varsa
        if (isGrounded)
        {
            float moveZ = Input.GetAxis("Vertical");
            // ... (hýz hesaplamalarý ayný)

            // Dönüþ için rigidbody kullan
            float turn = Input.GetAxis("Horizontal") * _handling;
            _rb.AddTorque(transform.up * turn * 10f, ForceMode.Force);
        }

        // Eðim efekti (optimize edilmiþ)
        if (isGrounded && Mathf.Abs(_currentSpeed) > 0.1f)
        {
            float tilt = -Input.GetAxis("Horizontal") * _tiltAngleMultiplier;
            Quaternion targetRot = Quaternion.Euler(0, transform.eulerAngles.y, tilt);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }
    }

    private bool IsGrounded()
    {
        float rayLength = 0.5f;
        int totalRays = 5;
        int hitCount = 0;

        Vector3[] rayOrigins = new Vector3[] {
        transform.position,
        transform.position + transform.forward * 0.3f,
        transform.position - transform.forward * 0.3f,
        transform.position + transform.right * 0.2f,
        transform.position - transform.right * 0.2f
    };

        foreach (var origin in rayOrigins)
        {
            if (Physics.Raycast(origin, Vector3.down, rayLength))
                hitCount++;
        }

        return (float)hitCount / totalRays >= _minGroundContact;
    }

   

    private void HandleCameraLook()
    {
        // Fare giriþini al
        float mouseY = Input.GetAxis("Mouse Y") * _cameraSensitivity * Time.deltaTime;

        // Dikey kamera hareketi (yukarý-aþaðý)
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -75f, 40f);
        _vehicleCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    private void EnterVehicle()
    {
        StartCoroutine(GroundBicycle());
        _playerOnboard = true;

        // Tüm fiziksel kýsýtlamalarý kaldýr
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.freezeRotation = false;

        // Hýz ve rotasyonu sýfýrla
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Karakter ayarlarý
        _player.transform.SetParent(_dropOfPoint);
        _player.transform.localPosition = Vector3.zero;
        _player.transform.localRotation = Quaternion.identity;
        _player.SetActive(false);

        // Kamera ve kontrol
        _vehicleCamera.SetActive(true);
        _player.GetComponent<CharrController>().SetControlEnabled(false);

        // Bisiklet rotasyonunu düzelt
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        // Hareket deðiþkenlerini resetle
        _currentSpeed = 0f;
        _handling = _minHandling;

        Debug.Log("Bisiklete binildi - Fizik aktif");
    }

    private void ExitVehicle()
    {
        _playerOnboard = false;

        // Karakteri ayýr
        _player.transform.SetParent(null);
        _player.transform.position = _dropOfPoint.position + transform.right * 2f;
        _player.transform.rotation = Quaternion.identity;
        _player.SetActive(true);
        _player.GetComponent<CharrController>().SetControlEnabled(true);
        _vehicleCamera.SetActive(false);

        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.freezeRotation = false;

        Debug.Log("Bisiklet serbest kaldý! Hýz: " + _rb.velocity.magnitude);
    }

    private IEnumerator GroundBicycle()
    {
        yield return new WaitForFixedUpdate();

        // Daha hassas zemin kontrolü
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 1.5f))
        {
            transform.position = hit.point + Vector3.up * 0.05f;
        }

        // 0.5 saniye sonra rotasyon kilitle
        yield return new WaitForSeconds(0.5f);
        _rb.freezeRotation = true;

        Debug.Log("Bisiklet zemine oturtuldu");
    }

    public string GetInteractionText()
    {
        return "E tusuna basin";
    }

    public bool CanInteract()
    {
        return true;
    }


    private void OnDrawGizmos()
    {
        // Zemin kontrol ray'lerini göster
        Gizmos.color = Color.green;

        // Ray baþlangýç noktalarýný tanýmla (IsGrounded() fonksiyonuyla ayný olmalý)
        Vector3[] rayOrigins = new Vector3[] {
        transform.position,
        transform.position + transform.forward * 0.3f,
        transform.position - transform.forward * 0.3f,
        transform.position + transform.right * 0.2f,
        transform.position - transform.right * 0.2f
    };

        foreach (var origin in rayOrigins)
        {
            Gizmos.DrawLine(origin, origin + Vector3.down * 0.5f);
        }
    }

}