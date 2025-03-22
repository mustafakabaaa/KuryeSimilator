using UnityEngine;
using System.Collections;
public class BicycleController : MonoBehaviour, Iinterectable
{
    [Header("Bisiklet Ozellikleri")]
    [SerializeField] private float _maxSpeed = 10f; // Maksimum hiz
    [SerializeField] private float _acceleration = 5f; // Hizlanma ivmesi
    [SerializeField] private float _deceleration = 2f; // Yavaslama ivmesi
    [SerializeField] private float _handling = 5f; // Manevra hassasiyeti
    [SerializeField] private float _brakePower = 5f; // Fren gucu
    [SerializeField] private float _gravity = -9.81f; // Yercekimi kuvveti
    [SerializeField] private float _cameraSensitivity = 100f; // Kamera hassasiyeti
    [SerializeField] private bool playerOnboard = false;
    public Transform dropOfPoint;
    public GameObject vehicleCamera;
    public GameObject player;

    private Rigidbody _rb;
    private float _currentSpeed = 0f; // Mevcut hiz
    private float _xRotation = 0f; // Kamera X rotasyonu

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("Rigidbody component not found.");
        }

        // Rigidbody ayarlari
        _rb.interpolation = RigidbodyInterpolation.Interpolate; // Titremeyi azaltir
        _rb.freezeRotation = true; // Fizik motorunun rotasyonu degistirmesini engeller

        // Player objesini baslangicta atayin
        player = GameObject.FindGameObjectWithTag("Player");

        // Baslangicta bisiklet kamerasini devre disi birak
        if (vehicleCamera != null)
            vehicleCamera.SetActive(false);
    }

    public void Interact()
    {
        if (!playerOnboard) EnterVehicle();
        else ExitVehicle();
    }

    private void EnterVehicle()
    {
        playerOnboard = true;

        // Karakteri devre disi birak
        if (player != null)
        {
            player.transform.parent = dropOfPoint;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.SetActive(false);
        }

        // Bisiklet kamerasini aktif hale getir
        if (vehicleCamera != null)
            vehicleCamera.SetActive(true);

        // Karakter kontrolunu devre disi birak
        if (player != null)
        {
            CharrController charController = player.GetComponent<CharrController>();
            if (charController != null)
            {
                charController.SetControlEnabled(false);
            }
        }

        // Bisikletin fizik motoru tarafindan kontrol edilmesini sagla
        _rb.isKinematic = false;
    }

    private void ExitVehicle()
    {
        if (!playerOnboard) return; // Eger zaten bisiklette degilse cikis yap

        playerOnboard = false;

        // Bisikletin Rigidbody'sini gecici olarak kinematic yap
        _rb.isKinematic = true;

        // Karakteri aktif hale getir
        if (player != null)
        {
            // Karakteri bisikletin yaninda belirle
            player.transform.parent = null;
            player.transform.position = dropOfPoint.position + transform.right * 2f; // Bisikletin yaninda belirle (2 birim saga)
            player.transform.rotation = dropOfPoint.rotation; // Bisikletin yonune gore ayarla
            player.SetActive(true);
        }

        // Karakter kontrolunu aktif hale getir
        if (player != null)
        {
            CharrController charController = player.GetComponent<CharrController>();
            if (charController != null)
            {
                charController.SetControlEnabled(true);
            }
        }

        // Bisiklet kamerasini devre disi birak
        if (vehicleCamera != null)
            vehicleCamera.SetActive(false);

        // Bisikletin Rigidbody'sini tekrar kinematic olmaktan cikar
        StartCoroutine(DisableKinematicAfterDelay(0.5f)); // 0.5 saniye sonra kinematic'i kapat
    }

    private IEnumerator DisableKinematicAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _rb.isKinematic = false;
    }

    private void Update()
    {
        if (playerOnboard)
        {
            // Bisiklet kontrolu
            HandleVehicleMovement();
            HandleCameraLook();

            // Bisikletteyken "E" tusuna basildiginda in
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitVehicle();
            }
        }
    }

    private void HandleVehicleMovement()
    {
        // Bisiklet hareketi
        float moveZ = Input.GetAxis("Vertical"); // W/S veya Yukari/Asagi ok tuslari
        float moveX = Input.GetAxis("Horizontal"); // A/D veya Sol/Sag ok tuslari

        // Hizlanma ve yavaslama mantigi
        if (moveZ > 0)
        {
            // Hizlanma
            _currentSpeed = Mathf.Min(_currentSpeed + _acceleration * Time.deltaTime, _maxSpeed);
        }
        else if (moveZ < 0)
        {
            // Geri gitme (yavaslama)
            _currentSpeed = Mathf.Max(_currentSpeed - _deceleration * Time.deltaTime, -_maxSpeed / 2);
        }
        else
        {
            // Yavaslama (hicbir tusa basilmiyorsa)
            if (_currentSpeed > 0)
            {
                _currentSpeed = Mathf.Max(_currentSpeed - _deceleration * Time.deltaTime, 0);
            }
            else if (_currentSpeed < 0)
            {
                _currentSpeed = Mathf.Min(_currentSpeed + _deceleration * Time.deltaTime, 0);
            }
        }

        // Hiza bagli donus hassasiyeti
        float minHandling = 0.5f; // Yavasken donus hassasiyeti
        float maxHandling = 2f;   // Hizliyken maksimum donus hassasiyeti
        float handlingThreshold = 3f; // Hizin bu degerin altinda olmasi durumunda minHandling kullanilir

        if (Mathf.Abs(_currentSpeed) < handlingThreshold)
        {
            // Hiz 3f'den dusukse, handling sabit 0.5f olur
            _handling = minHandling;
        }
        else
        {
            // Hiz 3f'den yuksekse, handling hiza gore azalir (ters orantili)
            float handlingRange = maxHandling - minHandling;
            _handling = maxHandling - (Mathf.Abs(_currentSpeed) - handlingThreshold) / (_maxSpeed - handlingThreshold) * handlingRange;
        }

        // Bisiklet durdugunda donmesin
        if (Mathf.Abs(_currentSpeed) < 0.1f) // Hiz cok dusukse (neredeyse duruyorsa)
        {
            _handling = 0f; // Donus hassasiyetini sifirla
        }

        // Hareket yonu
        Vector3 move = transform.forward * _currentSpeed;

        // Yatay hareketi uygula (X ve Z eksenleri)
        _rb.velocity = new Vector3(move.x, _rb.velocity.y, move.z);

        // Bisiklet donusu
        float turn = moveX * _handling;
        transform.Rotate(0, turn, 0);

        // Bisikletin egimini hesapla
        float tiltAngle = moveX * 30f * (_rb.velocity.magnitude / _maxSpeed); // Egim acisi (ornegin, 30 derece maksimum egim)
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, -tiltAngle); // Egim rotasyonu

        // Egimi yumusak bir sekilde uygula
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Frenleme
        if (Input.GetKey(KeyCode.Space))
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0, _brakePower * Time.deltaTime);
        }

        // Yercekimi uygula
        ApplyGravity();
    }

    private void HandleCameraLook()
    {
        // Fare girdisi
        float mouseX = Input.GetAxis("Mouse X") * _cameraSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _cameraSensitivity * Time.deltaTime;

        // Kamera rotasyonu (X ekseni)
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // Yukari-asagi bakis limiti

        // Kamera ve bisiklet donusu
        vehicleCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        //transform.Rotate(Vector3.up * mouseX);
    }

    private void ApplyGravity()
    {
        if (IsGrounded())
        {
            // Yere temas ettiginde yercekimini sifirla
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        }
        else
        {
            // Yere temas etmiyorsa yercekimini uygula
            _rb.velocity += Vector3.up * _gravity * Time.deltaTime;
        }
    }

    private bool IsGrounded()
    {
        // Bisikletin altina bir Raycast gonder
        float raycastDistance = 0.2f; // Bisikletin altindan ne kadar asagiya bakilacagi
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance);
    }
}