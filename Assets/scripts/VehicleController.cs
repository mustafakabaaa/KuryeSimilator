using UnityEngine;

public class BicycleController : MonoBehaviour, Iinterectable
{
    [Header("Araç Özellikleri")]
    [SerializeField] private float _speed = 10f; // Bisiklet hýzý
    [SerializeField] private float _handling = 5f; // Dönüþ hassasiyeti
    [SerializeField] private float _brakePower = 5f; // Fren gücü

    private bool _isPlayerRiding = false; // Oyuncu bisiklete bindi mi?
    private Transform _playerTransform; // Oyuncunun transformu
    private CharrController _playerController; // Oyuncu kontrol scripti
    private Rigidbody _rb; // Bisikletin Rigidbody bileþeni

    private void Start()
    {
        // Rigidbody bileþenini al
        _rb = GetComponent<Rigidbody>();

        // Bisikletin baþlangýçta fizik motoru etkin olmasýn
        _rb.isKinematic = true;
    }

    public void Interact()
    {
        if (!_isPlayerRiding)
        {
            // Oyuncu bisiklete biniyor
            EnterVehicle();
        }
        else
        {
            // Oyuncu bisikletten iniyor
            ExitVehicle();
        }
    }

    private void EnterVehicle()
    {
        // Oyuncuyu bul
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _playerController = _playerTransform.GetComponent<CharrController>();

        // Oyuncuyu gizle
        _playerTransform.gameObject.SetActive(false);

        // Kamera ve kontrolü bisiklete geçir
        _playerController.SetCameraTarget(transform); // Kamera bisiklete odaklanýr
        _playerController.SetControlEnabled(false); // Oyuncu kontrolünü devre dýþý býrak

        // Bisiklet kontrolünü etkinleþtir
        _isPlayerRiding = true;
        _rb.isKinematic = false; // Fizik motorunu etkinleþtir
    }

    private void ExitVehicle()
    {
        // Oyuncuyu bisikletin yanýnda belirgin hale getir
        _playerTransform.position = transform.position + transform.right * 2f; // Bisikletin yanýnda belirir
        _playerTransform.gameObject.SetActive(true);

        // Kamera ve kontrolü oyuncuya geri ver
        _playerController.SetCameraTarget(_playerTransform); // Kamera oyuncuya odaklanýr
        _playerController.SetControlEnabled(true); // Oyuncu kontrolünü etkinleþtir

        // Bisiklet kontrolünü devre dýþý býrak
        _isPlayerRiding = false;
        _rb.isKinematic = true; // Fizik motorunu devre dýþý býrak
    }

    private void Update()
    {
        if (_isPlayerRiding)
        {
            // Bisiklet kontrolü
            HandleVehicleMovement();
        }
    }

    private void HandleVehicleMovement()
    {
        // Bisiklet hareketi
        float moveZ = Input.GetAxis("Vertical"); // W/S veya Yukarý/Aþaðý ok tuþlarý
        float moveX = Input.GetAxis("Horizontal"); // A/D veya Sol/Sað ok tuþlarý

        // Hareket yönü
        Vector3 move = transform.forward * moveZ * _speed;
        _rb.velocity = new Vector3(move.x, _rb.velocity.y, move.z);

        // Bisiklet dönüþü
        float turn = moveX * _handling;
        transform.Rotate(0, turn, 0);

        // Frenleme
        if (Input.GetKey(KeyCode.Space))
        {
            _rb.velocity = Vector3.Lerp(_rb.velocity, Vector3.zero, _brakePower * Time.deltaTime);
        }
    }

    private bool IsGrounded()
    {
        // Bisikletin altýna bir Raycast gönder
        float raycastDistance = 0.2f; // Bisikletin altýndan ne kadar aþaðýya bakýlacaðý
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance);
    }
}