using System.Collections;
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
    [SerializeField] private SleepStaminaSystem sleepStaminaSystem; // Inspector'dan bağlayın

    



    private bool isMoving = false;
    public float runningStaminaCost = 3f;
    private bool _isCameraLocked = false;

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
        if (_isControlEnabled && !UIManager.Instance.IsAnyUIOpen())
        {
            HandleMouseLook();
            HandleMovement();
            RaycastController();
            GameDataUpdate();

            ApplyStaminaPenalty();

            // Hareket ederken stamina tüket
            if (isMoving && sleepStaminaSystem != null)
            {
                float staminaCost = sleepStaminaSystem.walkingStaminaCost * Time.deltaTime;

                // Eğer koşuyorsa (Shift basılıysa) ekstra tüket
               

                sleepStaminaSystem.currentStamina = Mathf.Max(0, sleepStaminaSystem.currentStamina - staminaCost);
            }
        }
    }
    private void ApplyStaminaPenalty()
    {
        if (sleepStaminaSystem != null && sleepStaminaSystem.IsPenaltyActive)
        {
            moveSpeed = gameData.GetCurrentStatValue(speedStat) * 0.5f; // %50 yavaşlat
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
        if (!_isControlEnabled) return;

        float raycastDistance = 3f;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            if (hit.collider.TryGetComponent(out Iinterectable interactable))
            {
                if (interactable.CanInteract())
                {
                    interactText.text = interactable.GetInteractionText(); // Dinamik metin
                    interactText.gameObject.SetActive(true);
                    canInteract = true;

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        interactable.Interact();
                    }
                }
                else
                {
                    interactText.gameObject.SetActive(false);
                    canInteract = false;
                }
            }
            else
            {
                interactText.gameObject.SetActive(false);
                canInteract = false;
            }

            // Saldırı kodu burada kalabilir (değişmeden)
            if (hit.collider.TryGetComponent(out IAttackable attackable))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    attackable.Attack();
                }
            }
        }
        else
        {
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
        if (!_isControlEnabled || _isCameraLocked || UIManager.Instance.IsAnyUIOpen()) return;

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
    public void LookAtTarget(Transform target)
    {
        if (target == null) return;

        StartCoroutine(SmoothLookAt(target));
    }

    private IEnumerator SmoothLookAt(Transform target)
    {
        LockCamera(true);

        Vector3 direction = target.position - transform.position;
        direction.y = 0;
        Quaternion targetRot = Quaternion.LookRotation(direction);

        float duration = 0.5f;
        float elapsed = 0f;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRot;
    }
    public void LockCamera(bool lockStatus)
    {
        _isCameraLocked = lockStatus;

        // İmleç kontrolü
        Cursor.visible = lockStatus;
        Cursor.lockState = lockStatus ? CursorLockMode.None : CursorLockMode.Locked;

        // Crosshair kontrolü
        if (crosshairImage != null)
            crosshairImage.SetActive(!lockStatus);
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

        if (sleepStaminaSystem != null && move.magnitude > 0.1f)
        {
            sleepStaminaSystem.currentStamina -= sleepStaminaSystem.walkingStaminaCost * Time.deltaTime;
            sleepStaminaSystem.currentStamina = Mathf.Max(0, sleepStaminaSystem.currentStamina);
        }
    }

    bool IsGrounded()
    {
        // Karakterin altina bir Raycast gonder
        float raycastDistance = 0.2f; // Karakterin ayaklarindan ne kadar asagiya bakilacagi
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance);
    }

  
}