using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharrController : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f; // Hareket hizi
    public float mouseSensitivity = 100f; // Fare hassasiyeti
    public float jumpForce = 1.5f; // Z�plama kuvveti
    private bool isJumping = false; // Z�plama durumu
    private InventoryUIController inventoryUIController;

    [Header("References")]
    public Transform playerCamera; // Kamera referansı
    public GameObject crosshairImage; // Canvas'taki nokta görüntüsü

    private CharacterController characterController;
    private float xRotation = 0f; // Kamera X rotasyonu
    private Vector3 velocity; // Yercekimi icin hiz

    private bool canInteract = false; // Etkilesime girilebilecek mi?
    public TextMeshProUGUI interactText; // Etkilesim metni

    private Animator animator;

    private Transform _cameraTarget; // Kamera hedefi (oyuncu veya bisiklet)
    private bool _isControlEnabled = true; // Oyuncu kontrolu etkin mi?

    // CharrController'a ekleyin:
    [Header("Game Data")]
    [SerializeField] private GameDataSO gameData; // Inspector'dan bağlayın
    [SerializeField] private CharacterStat speedStat; // Speed Stat SO'sunu Inspector'dan bağlayın
    [SerializeField] private CharacterStat jumpForceStat; // Speed Stat SO'sunu Inspector'dan bağlayın


    private void Awake()
    {
        inventoryUIController = GetComponent<InventoryUIController>();
    }

    void Start()
    {
        // Component kontrolu
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned.");
        }

       
    }

    void Update()
    {
        
        if (_isControlEnabled)
        {
            HandleMouseLook();
            HandleMovement();
            RaycastController();
            GameDataUpdate();
        }
        
    }
    public void GameDataUpdate()
    {
        if (gameData != null && speedStat != null)
        {
            moveSpeed = gameData.GetCurrentStatValue(speedStat);

        }
        if (gameData != null && jumpForceStat != null)
        {
            jumpForce=gameData.GetCurrentStatValue(jumpForceStat);
        }
    }

   
    void RaycastController()
    {
        if (!_isControlEnabled) return; // Kontrol devre dışıysa RaycastController'ı çalıştırma

        float raycastDistance = 3f; // Raycast'in maksimum mesafesi
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Raycast'i gorsel olarak cizin (ornegin kirmizi renk)
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        // Raycast'i belirli bir mesafeye kadar kontrol et
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // E tusu ile etkilesim (ornegin kapi acma)
            if (hit.collider.TryGetComponent(out Iinterectable interactable))
            {
                // Mesajı göster
                interactText.gameObject.SetActive(true);
                canInteract = true;

                // E tusuna basildiginda etkilesime gir
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
            else
            {
                // Etkilesime girilebilecek nesne degilse mesaji gizle
                interactText.gameObject.SetActive(false);
                canInteract = false;
            }

            // Fare sol tiklamasi ile saldiri (ornegin NPC'ye saldirma)
            if (hit.collider.TryGetComponent(out IAttackable attackable))
            {
                // Sol tiklama ile saldir
                if (Input.GetMouseButtonDown(0)) // Sol tiklama
                {
                    attackable.Attack();
                }
            }
        }
        else
        {
            // Hicbir nesneye bakilmiyorsa mesaji gizle
            interactText.gameObject.SetActive(false);
            canInteract = false;
        }
    }

    public void SetCameraTarget(Transform target)
    {
        _cameraTarget = target;
    }

    public void SetControlEnabled(bool isEnabled)
    {
        _isControlEnabled = isEnabled;

        // Kontrol devre dışıysa "Press E" yazısını ve nokta görüntüsünü gizle
        if (!isEnabled)
        {
            interactText.gameObject.SetActive(false);
            if (crosshairImage != null) // crosshairImage, Canvas'taki nokta görüntüsüdür
            {
                crosshairImage.SetActive(false);
            }
        }
        else
        {
            // Kontrol etkinse nokta görüntüsünü göster
            if (crosshairImage != null)
            {
                crosshairImage.SetActive(true);
            }
        }
    }

    void HandleMouseLook()
    {
        if (!_isControlEnabled) return;

        // Fare girdisi
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Kamera rotasyonu (X ekseni)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Yukari-asagi bakis limiti

        // Kamera ve oyuncu donusu
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (!_isControlEnabled) return;

        // Klavye girdisi
        float moveX = Input.GetAxis("Horizontal"); // A/D veya Sol/Sag ok tuslari
        float moveZ = Input.GetAxis("Vertical");   // W/S veya Yukari/Asagi ok tuslari

        // Hareket yonu (normalize edilmis)
        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        // Hareketi uygula
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Yurume animasyonunu kontrol et
        if (move.magnitude > 0.1f) // Eger karakter hareket ediyorsa
        {
            animator.SetBool("isWalking", true); // Yurume animasyonunu baslat
        }
        else
        {
            animator.SetBool("isWalking", false); // Yurume animasyonunu durdur
        }

        // Yer�ekimi kontrol�
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f; // Hafif bir sabit kuvvet uygulayin

            // Ziplama tusuna basildiginda
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y); // Ziplama kuvveti uygula
                animator.SetBool("isJumping", true); // Ziplama animasyonunu baslat
                isJumping = true;
            }
            else if (isJumping)
            {
                animator.SetBool("isJumping", false); // Ziplama animasyonunu durdur
                isJumping = false;
            }
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime; // Yercekimi ekleyin
        }

        // Yercekimini uygula
        characterController.Move(velocity * Time.deltaTime);
    }

    bool IsGrounded()
    {
        // Karakterin altina bir Raycast gonder
        float raycastDistance = 0.2f; // Karakterin ayaklarindan ne kadar asagiya bakilacagi
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance);
    }
}