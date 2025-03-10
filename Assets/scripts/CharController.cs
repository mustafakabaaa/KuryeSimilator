using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharrController : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f; // Hareket h�z�
    public float mouseSensitivity = 100f; // Fare hassasiyeti
    public float jumpForce = 1.5f; // Z�plama kuvveti
    private bool isJumping = false; // Z�plama durumu
    private InventoryUIController inventoryUIController;

    [Header("References")]
    public Transform playerCamera; // Kamera referans�

    private CharacterController characterController;
    private float xRotation = 0f; // Kamera X rotasyonu
    private Vector3 velocity; // Yer�ekimi i�in h�z

    private bool canInteract = false; // Etkile�ime girilebilecek mi?
    public TextMeshProUGUI interactText; // Etkile�im metni

    private Animator animator;
    public GameObject inventoryGameobject;

    private Transform _cameraTarget; // Kamera hedefi (oyuncu veya bisiklet)
    private bool _isControlEnabled = true; // Oyuncu kontrol� etkin mi?
    public SCInventory playerInventory;

    private void Awake()
    {
        inventoryUIController = GetComponent<InventoryUIController>();
    }
    void Start()
    {
        // Component kontrol�
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned.");
        }

        // Fareyi kilitle
        Cursor.lockState = CursorLockMode.Locked;
        interactText.gameObject.SetActive(false);

        _cameraTarget = transform;

        inventoryGameobject.SetActive(false );
        Cursor.visible = false;
        
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        RaycastController();
        InventoryOpenAndClose();
    }
    public void InventoryOpenAndClose()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryGameobject.activeSelf == false)
            {
                // Envanteri aç
                
                inventoryGameobject.SetActive(true);
                inventoryUIController.OpenPlayerInventory();
                inventoryUIController.SwitchToPlayerInventory();
                // İmleci serbest bırak ve görünür yap
                Cursor.lockState = CursorLockMode.None; // İmleci serbest bırak
                Cursor.visible = true; // İmleci göster
                inventoryUIController.ClearSelectedButton();
            }
            else
            {
                // Envanteri kapat
                inventoryGameobject.SetActive(false);
                inventoryUIController.OpenPlayerInventory();


                // İmleci kilitle ve gizle
                Cursor.lockState = CursorLockMode.Locked; // İmleci kilitle
                Cursor.visible = false; // İmleci gizle
                inventoryUIController.ClearSelectedButton(); // Butonların Selected durumunu sıfırla
                Inventory inventory = GetComponent<Inventory>();
                if (inventory != null)
                {
                    inventory.ResetSwap();
                }
            }
        }
    }
    void RaycastController()
    {
        float raycastDistance = 3f; // Raycast'in maksimum mesafesi
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Raycast'i g�rsel olarak �izin (�rne�in k�rm�z� renk)
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        // Raycast'i belirli bir mesafeye kadar kontrol et
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // E tu�u ile etkile�im (�rne�in kap� a�ma)
            if (hit.collider.TryGetComponent(out Iinterectable interactable))
            {
                // Mesaj� g�ster
                interactText.gameObject.SetActive(true);
                canInteract = true;

                // E tu�una bas�ld���nda etkile�ime gir
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
            else
            {
                // Etkile�ime girilebilecek nesne de�ilse mesaj� gizle
                interactText.gameObject.SetActive(false);
                canInteract = false;
            }

            // Fare sol t�klamas� ile sald�r� (�rne�in NPC'ye sald�rma)
            if (hit.collider.TryGetComponent(out IAttackable attackable))
            {
                // Sol t�klama ile sald�r
                if (Input.GetMouseButtonDown(0)) // Sol t�klama
                {
                    attackable.Attack();
                }
            }
        }
        else
        {
            // Hi�bir nesneye bak�lm�yorsa mesaj� gizle
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
    }

    void HandleMouseLook()
    {
        // Fare girdisi
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Kamera rotasyonu (X ekseni)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Yukar�-a�a�� bak�� limiti

        // Kamera ve oyuncu d�n���
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        
    }

   

    void HandleMovement()
    {
        // Klavye girdisi
        float moveX = Input.GetAxis("Horizontal"); // A/D veya Sol/Sa� ok tu�lar�
        float moveZ = Input.GetAxis("Vertical");   // W/S veya Yukar�/A�a�� ok tu�lar�

        // Hareket y�n� (normalize edilmi�)
        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        // Hareketi uygula
        characterController.Move(move * moveSpeed * Time.deltaTime);
        //Debug.Log("Move Magnitude: " + move.magnitude);

        // Y�r�me animasyonunu kontrol et
        if (move.magnitude > 0.1f) // E�er karakter hareket ediyorsa
        {
            animator.SetBool("isWalking", true); // Y�r�me animasyonunu ba�lat
        }
        else
        {
            animator.SetBool("isWalking", false); // Y�r�me animasyonunu durdur
        }

        // Yer�ekimi kontrol�
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f; // Hafif bir sabit kuvvet uygulay�n

            // Z�plama tu�una bas�ld���nda
            if (Input.GetButtonDown("Jump"))
            {
                Debug.Log("Jump Force: " + jumpForce);
                Debug.Log("Gravity: " + Physics.gravity.y);
                Debug.Log("Velocity Y: " + velocity.y);
                velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y); // Z�plama kuvveti uygula
                animator.SetBool("isJumping", true); // Z�plama animasyonunu ba�lat
                isJumping = true;
            }
            else if (isJumping)
            {
                animator.SetBool("isJumping", false); // Z�plama animasyonunu durdur
                isJumping = false;
            }
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime; // Yer�ekimi ekleyin
        }

        // Yer�ekimini uygula
        characterController.Move(velocity * Time.deltaTime);
    }

    bool IsGrounded()
    {
        // Karakterin alt�na bir Raycast g�nder
        float raycastDistance = 0.2f; // Karakterin ayaklar�ndan ne kadar a�a��ya bak�laca��
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance);
    }
}