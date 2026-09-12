using UnityEngine;
using System.Collections;

public class BicycleController : MonoBehaviour, Iinterectable
{
    [Header("Bisiklet Temel Ayarlar�")]
    [SerializeField] private float _maxSpeed = 10f; // Maksimum hareket h�z�
    [SerializeField] private float _acceleration = 5f; // H�zlanma g�c�


    [SerializeField] private float _gravity = -9.81f; // Yer�ekimi kuvveti
    [SerializeField] private float _cameraSensitivity = 100f; // Kamera hassasiyeti

    [Header("D�n�� Ayarlar�")]
    [SerializeField] private float _handlingThreshold = 3f; // D�n�� hassasiyeti e�ik de�eri
    [SerializeField][Range(0.1f, 2f)] private float _minHandling = 0.5f; // Minimum d�n�� hassasiyeti
    [SerializeField][Range(1f, 5f)] private float _maxHandling = 1.5f; // Maksimum d�n�� hassasiyeti
    [SerializeField] private float _tiltAngleMultiplier = 30f; // E�im a�� �arpan�

    [Header("Referanslar")]
    [SerializeField] private Transform _dropOfPoint; // �nme noktas�
    [SerializeField] private GameObject _vehicleCamera; // Ara� kameras�

    [Header("Serbest Bisiklet Fizi�i")]
    [SerializeField] private float _dragWhenAbandoned = 0.3f; // S�rt�nme katsay�s�
    [SerializeField] private float _angularDragWhenAbandoned = 0.1f; // D�nme s�rt�nmesi

    [Header("Bisiklet D�zeltme Ayarlar�")]
    [SerializeField] private float _standUpSpeed = 2f; // D�zeltme h�z�
    [SerializeField] private float _maxTiltAngleToMount = 30f; // Binilebilir maksimum e�im a��s

    [Header("Collider Ayarlar�")]
    [SerializeField] private float _groundContactOffset = 0.1f; // Collider'�n zeminden y�ksekli�i
    [SerializeField] private float _minGroundContact = 0.3f; // Minimum temas oran� (0.3 = %30)

    private Rigidbody _rb;
    private float _currentSpeed = 0f;
    private float _xRotation = 0f;
    private float _handling;
    private bool _playerOnboard = false;
    private GameObject _player;
   
    private bool _isStandingUp = false; // Bisiklet d�zeltiliyor mu?
    [SerializeField] private MinimapCameraFollow minimapCameraFollow;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate; // Yumu�ak fizik hareketi
        _rb.freezeRotation = true; // Fiziksel d�n��� kilitle
        _player = GameObject.FindGameObjectWithTag("Player");
        _vehicleCamera.SetActive(false); // Ba�lang��ta kamera kapal�
    }

    private void FixedUpdate()
    {
        if (!_playerOnboard)
        {
            // Bisiklet terk edilmi�se, yava� yava� durmas�n� sa�la (s�rt�nme uygula)
            _rb.linearDamping = _dragWhenAbandoned; 
            _rb.angularDamping = _angularDragWhenAbandoned; 
        }
        else
        {
            // Biniliyken s�rt�nmeyi s�f�rla (normal fizik kurallar� ge�erli)
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0.05f;
        }
    }

    public void Interact()
    {
        // burada karakter bisiklette de�ilse ve bisiklet d�zeltme i�lemi yap�lm�yorsa bisiklete bin
        if (!_playerOnboard && CanMountBicycle()) // bisikletin bulundu�u e�imin binmeye uygun olup olmad���n� kontrol eden fonk
        {
            EnterVehicle(); // �artlar sa�land�ysa bin
        }
    }

    private void Update()
    {
        if (_playerOnboard)
        {
            HandleVehicleMovement(); // 
            HandleCameraLook();      // 
            if (Input.GetKeyDown(KeyCode.E)) ExitVehicle(); // "keyCode" -> "KeyCode" d�zeltildi
        }
        else
        {
            // Bisiklet d�zeltme kontrol� ama burada niye �arts�z ko�ulsuz d�zeltme koyduk?
            if (Input.GetKeyDown(KeyCode.R)) // Eksik parantez eklendi
            {
                StartCoroutine(StandUpBicycle()); // "StartCorouting" -> "StartCoroutine" d�zeltildi
            }
        }

        Debug.Log("D�n�� Hassasiyeti:" + _handling + "    H�z:" + _currentSpeed); // Eksik string birle�tirme d�zeltildi
    }

    private bool CanMountBicycle()
    {
        // Bisikletin e�im a��s�n� kontrol et
        float tiltAngle = Mathf.Abs(transform.rotation.eulerAngles.z);
        tiltAngle = tiltAngle > 180f ? 360f - tiltAngle : tiltAngle;

        return tiltAngle <= _maxTiltAngleToMount && !_isStandingUp;
    }

    private IEnumerator StandUpBicycle()
    {
        _isStandingUp = true;
        _rb.freezeRotation = false; // D�zeltme s�ras�nda rotasyon kilidini a�

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

        _rb.freezeRotation = true; // ��lem bitince tekrar kilitle
        _isStandingUp = false;
    }

    private void HandleVehicleMovement()
    {
        bool isGrounded = IsGrounded();

        // Yer�ekimi uygula (daha ger�ek�i)
        if (!isGrounded)
        {
            _rb.AddForce(Vector3.down * _gravity * 2f, ForceMode.Acceleration);
        }

        // Hareket sadece yeterli zemine temas varsa
        if (isGrounded)
        {
            float moveZ = Input.GetAxis("Vertical");
            // ... (h�z hesaplamalar� ayn�)

            // D�n�� i�in rigidbody kullan
            float turn = Input.GetAxis("Horizontal") * _handling;
            _rb.AddTorque(transform.up * turn * 10f, ForceMode.Force);
        }

        // E�im efekti (optimize edilmi�)
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
        // Fare giri�ini al
        float mouseY = Input.GetAxis("Mouse Y") * _cameraSensitivity * Time.deltaTime;

        // Dikey kamera hareketi (yukar�-a�a��)
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -75f, 40f);
        _vehicleCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    private void EnterVehicle()
    {
        StartCoroutine(GroundBicycle());
        _playerOnboard = true;

        // T�m fiziksel k�s�tlamalar� kald�r
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.freezeRotation = false;

        // H�z ve rotasyonu s�f�rla
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Karakter ayarlar�
        _player.transform.SetParent(_dropOfPoint);
        _player.transform.localPosition = Vector3.zero;
        _player.transform.localRotation = Quaternion.identity;
        _player.SetActive(false);

        // Kamera ve kontrol
        _vehicleCamera.SetActive(true);
        _player.GetComponent<CharrController>().SetControlEnabled(false);

        // Bisiklet rotasyonunu d�zelt
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        // Hareket de�i�kenlerini resetle
        _currentSpeed = 0f;
        _handling = _minHandling;

        Debug.Log("Bisiklete binildi - Fizik aktif");
    }

    private void ExitVehicle()
    {
        _playerOnboard = false;

        // Karakteri ay�r
        _player.transform.SetParent(null);
        _player.transform.position = _dropOfPoint.position + transform.right * 2f;
        _player.transform.rotation = Quaternion.identity;
        _player.SetActive(true);
        _player.GetComponent<CharrController>().SetControlEnabled(true);
        _vehicleCamera.SetActive(false);

        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.freezeRotation = false;

        Debug.Log("Bisiklet serbest kald�! H�z: " + _rb.linearVelocity.magnitude);
    }

    private IEnumerator GroundBicycle()
    {
        yield return new WaitForFixedUpdate();

        // Daha hassas zemin kontrol�
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
        // Zemin kontrol ray'lerini g�ster
        Gizmos.color = Color.green;

        // Ray ba�lang�� noktalar�n� tan�mla (IsGrounded() fonksiyonuyla ayn� olmal�)
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