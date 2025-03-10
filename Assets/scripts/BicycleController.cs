using UnityEngine;
using System.Collections; 
public class BicycleController : MonoBehaviour, Iinterectable
{
    [Header("Araba Ozellikleri")]
    [SerializeField] private float _speed = 10f; // Bisiklet hizi
    [SerializeField] private float _handling = 5f; // Manevra hassasiyeti
    [SerializeField] private float _brakePower = 5f; // Fren gucu
    [SerializeField] private float _gravity = -9.81f; // Yerçekimi kuvveti
    [SerializeField] private float _cameraSensitivity = 100f; // Kamera hassasiyeti

    public bool playerOnboard = false;
    public Transform dropOfPoint;
    public GameObject vehicleCamera;
    public GameObject player;

    private Rigidbody _rb;
    private Vector3 _velocity; // Yerçekimi için hız vektörü
    private float _xRotation = 0f; // Kamera X rotasyonu

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            Debug.LogError("Rigidbody component not found.");
        }

        // Player objesini başlangıçta atayın
        player = GameObject.FindGameObjectWithTag("Player");

        // Başlangıçta bisiklet kamerasını devre dışı bırak
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

        // Karakteri devre dışı bırak
        if (player != null)
        {
            player.transform.parent = dropOfPoint;
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.SetActive(false);
        }

        // Bisiklet kamerasını aktif hale getir
        if (vehicleCamera != null)
            vehicleCamera.SetActive(true);

        // Karakter kontrolünü devre dışı bırak
        if (player != null)
        {
            CharrController charController = player.GetComponent<CharrController>();
            if (charController != null)
            {
                charController.SetControlEnabled(false);
            }
        }

        // Bisikletin fizik motoru tarafından kontrol edilmesini sağla
        _rb.isKinematic = false; // Bu satırı ekleyin
    }

    private void ExitVehicle()
    {
        if (!playerOnboard) return; // Eğer zaten bisiklette değilse çıkış yap

        playerOnboard = false;

        // Bisikletin Rigidbody'sini geçici olarak kinematic yap
        _rb.isKinematic = true;

        // Karakteri aktif hale getir
        if (player != null)
        {
            // Karakteri bisikletin yanında belirle
            player.transform.parent = null;
            player.transform.position = dropOfPoint.position + transform.right * 2f; // Bisikletin yanında belirle (2 birim sağa)
            player.transform.rotation = dropOfPoint.rotation; // Bisikletin yönüne göre ayarla
            player.SetActive(true);
        }

        // Karakter kontrolünü aktif hale getir
        if (player != null)
        {
            CharrController charController = player.GetComponent<CharrController>();
            if (charController != null)
            {
                charController.SetControlEnabled(true);
            }
        }

        // Bisiklet kamerasını devre dışı bırak
        if (vehicleCamera != null)
            vehicleCamera.SetActive(false);

        // Bisikletin Rigidbody'sini tekrar kinematic olmaktan çıkar
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

            // Bisikletteyken "E" tuşuna basıldığında in
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

    // Hareket yonu
    Vector3 move = transform.forward * moveZ * _speed;

    // Yatay hareketi uygula (X ve Z eksenleri)
    _rb.velocity = new Vector3(move.x, _rb.velocity.y, move.z);

    // Bisiklet donusu
    float turn = moveX * _handling;
    transform.Rotate(0, turn, 0);

    // Bisikletin eğimini hesapla
   float tiltAngle = moveX * 30f * (_rb.velocity.magnitude / _speed); // Eğim açısı (örneğin, 30 derece maksimum eğim)
    Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, -tiltAngle); // Eğim rotasyonu

    // Eğimi yumuşak bir şekilde uygula
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

    // Frenleme
    if (Input.GetKey(KeyCode.Space))
    {
        _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, _brakePower * Time.deltaTime);
    }

    // Yerçekimi uygula
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
        transform.Rotate(Vector3.up * mouseX);
    }

    private void ApplyGravity()
    {
        if (IsGrounded())
        {
            // Yere temas ettiğinde yerçekimini sıfırla
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        }
        else
        {
            // Yere temas etmiyorsa yerçekimini uygula
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