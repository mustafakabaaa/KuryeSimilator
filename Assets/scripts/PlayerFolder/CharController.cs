using TMPro;
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;

public class CharrController : MonoBehaviour, ISaveable
{
    private const string UiTable = "UI";
    [Header("Player Settings")]
    public float moveSpeed = 5f; // Hareket hizi

   
    private InventoryUIController inventoryUIController;

    [Header("References")]
    public Transform playerCamera; // Kamera referansı
    public GameObject crosshairImage; // Canvas'taki nokta görüntüsü

    private CharacterController characterController;

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

    private PlayerInputs _playerInputs;
    private Vector2 _moveInput;


    private Vector2 _lookInput;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    private float xRotation = 0f;


    private bool isMoving = false;
    public float runningStaminaCost = 3f;
    private bool _isCameraLocked = false;
    private SaveManager saveManager;

    private void Awake()
    {
        _playerInputs = new PlayerInputs();

        _playerInputs.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _playerInputs.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        _playerInputs.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _playerInputs.Player.Look.canceled += ctx => _lookInput = Vector2.zero;

        inventoryUIController = GetComponent<InventoryUIController>();
        characterController = GetComponent<CharacterController>();
         // SaveManager'a kendimizi kaydediyoruz
        
    }

    private void OnEnable()
    {
        _playerInputs.Enable();
    }

    private void OnDisable()
    {
        _playerInputs.Disable();
    }

    void Start()
    {
        saveManager = FindObjectOfType<SaveManager>(); // Veya Inspector'dan bağla
       

        // Component kontrolu
        animator = GetComponent<Animator>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned.");
        }
        if (SaveManager.Instance != null)
            SaveManager.Instance.RegisterSystem(this);
        //SaveManager.Instance.LoadGame();
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
            // Etkileşim kontrolü
            if (hit.collider.TryGetComponent(out Iinterectable interactable))
            {
                if (interactable.CanInteract())
                {
                    interactText.text = LocalizationHelper.Localize(UiTable, interactable.GetInteractionText());
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

            // Saldırı kontrolü - DEĞİŞTİRİLEN KISIM
            if (Input.GetMouseButtonDown(0) && hit.collider.TryGetComponent(out IAttackable attackable))
            {
                // NPC kontrolü doğrudan component üzerinden
                if (hit.collider.TryGetComponent(out NPCController npcController))
                {
                    if (npcController.IsVulnerable())
                    {
                        attackable.Attack();
                    }
                    else
                    {
                        Debug.Log("NPC şu anda hasar alamaz durumda!");
                    }
                }
                else
                {
                    // Eğer NPC değilse (başka bir IAttackable nesnesi) direkt saldır
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
        if (!_isControlEnabled) return;

        float mouseX = _lookInput.x * mouseSensitivity;
        float mouseY = _lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
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
        Vector3 move = (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized;

        characterController.Move(move * moveSpeed * Time.deltaTime);

        animator.SetBool("isWalking", move.magnitude > 0.1f);

        // Zıplama ile ilgili kodu tamamen kaldırın ve yerine sadece yerçekimi ekleyin
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f; // Yere sabitle
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }

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

    public void SaveData(GameData data)
    {
        if (data == null) return;

        Vector3 currentPosition = transform.position;
        //Debug.Log($"[SAVE] Player Position: {currentPosition}");

        Quaternion currentRotation = transform.rotation;

        characterController.enabled = false;
        transform.position = currentPosition;
        transform.rotation = currentRotation;
        characterController.enabled = true;

        data.playerPosition = new GameData.Vector3Serializable(transform.position);
        data.playerRotationY = transform.eulerAngles.y;
        data.currentStamina = sleepStaminaSystem != null ? sleepStaminaSystem.currentStamina : 100f;
    }


    public void LoadData(GameData data)
    {
        characterController.enabled = false;

        transform.position = data.playerPosition.ToVector3();
        transform.eulerAngles = new Vector3(0, data.playerRotationY, 0);
        velocity = Vector3.zero;

        characterController.enabled = true;

        

        if (sleepStaminaSystem != null)
            sleepStaminaSystem.currentStamina = data.currentStamina;
    }

}