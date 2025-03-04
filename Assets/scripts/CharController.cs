using TMPro;
using UnityEngine;

public class CharrController : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 5f; // Hareket hýzý
    public float mouseSensitivity = 100f; // Fare hassasiyeti
    public float jumpForce = 1.5f; // Zýplama kuvveti
    private bool isJumping = false; // Zýplama durumu

    [Header("References")]
    public Transform playerCamera; // Kamera referansý

    private CharacterController characterController;
    private float xRotation = 0f; // Kamera X rotasyonu
    private Vector3 velocity; // Yerçekimi için hýz

    private bool canInteract = false; // Etkileþime girilebilecek mi?
    public TextMeshProUGUI interactText; // Etkileþim metni

    private Animator animator;


    private Transform _cameraTarget; // Kamera hedefi (oyuncu veya bisiklet)
    private bool _isControlEnabled = true; // Oyuncu kontrolü etkin mi?


    void Start()
    {
        // Component kontrolü
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
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        RaycastController();
    }

    void RaycastController()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Raycast'i görsel olarak çizin (örneðin kýrmýzý renk)
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.TryGetComponent(out Iinterectable interactObjects))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactObjects.Interact();
                }
            }

            // Eðer bakýlan nesne etkileþime girilebilir bir nesne ise
            if (hit.collider.TryGetComponent(out Iinterectable interactable))
            {
                // Mesajý göster
                interactText.gameObject.SetActive(true);
                canInteract = true;
            }
            else
            {
                // Etkileþime girilebilecek nesne deðilse mesajý gizle
                interactText.gameObject.SetActive(false);
                canInteract = false;
            }
        }
        else
        {
            // Hiçbir nesneye bakýlmýyorsa mesajý gizle
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
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Yukarý-aþaðý bakýþ limiti

        // Kamera ve oyuncu dönüþü
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        
    }

   

    void HandleMovement()
    {
        // Klavye girdisi
        float moveX = Input.GetAxis("Horizontal"); // A/D veya Sol/Sað ok tuþlarý
        float moveZ = Input.GetAxis("Vertical");   // W/S veya Yukarý/Aþaðý ok tuþlarý

        // Hareket yönü (normalize edilmiþ)
        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        // Hareketi uygula
        characterController.Move(move * moveSpeed * Time.deltaTime);
        //Debug.Log("Move Magnitude: " + move.magnitude);

        // Yürüme animasyonunu kontrol et
        if (move.magnitude > 0.1f) // Eðer karakter hareket ediyorsa
        {
            animator.SetBool("isWalking", true); // Yürüme animasyonunu baþlat
        }
        else
        {
            animator.SetBool("isWalking", false); // Yürüme animasyonunu durdur
        }

        // Yerçekimi kontrolü
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f; // Hafif bir sabit kuvvet uygulayýn

            // Zýplama tuþuna basýldýðýnda
            if (Input.GetButtonDown("Jump"))
            {
                Debug.Log("Jump Force: " + jumpForce);
                Debug.Log("Gravity: " + Physics.gravity.y);
                Debug.Log("Velocity Y: " + velocity.y);
                velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y); // Zýplama kuvveti uygula
                animator.SetBool("isJumping", true); // Zýplama animasyonunu baþlat
                isJumping = true;
            }
            else if (isJumping)
            {
                animator.SetBool("isJumping", false); // Zýplama animasyonunu durdur
                isJumping = false;
            }
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime; // Yerçekimi ekleyin
        }

        // Yerçekimini uygula
        characterController.Move(velocity * Time.deltaTime);
    }

    bool IsGrounded()
    {
        // Karakterin altýna bir Raycast gönder
        float raycastDistance = 0.2f; // Karakterin ayaklarýndan ne kadar aþaðýya bakýlacaðý
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance);
    }
}