using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class CharrController : MonoBehaviour
{
    [Header("Player Settings")]
    public float moveSpeed = 6f; // Hareket hýzý
    public float mouseSensitivity = 100f; // Fare hassasiyeti

    [Header("References")]
    public Transform playerCamera; // Kamera referansý

    private CharacterController characterController;
    private float xRotation = 0f; // Kamera X rotasyonu
    private Vector3 velocity; // Yerçekimi için hýz

    private bool canInteract = false; // Etkileþime girilebileceý?
    public TextMeshProUGUI interactText; // Tex
    void Start()
    {
        // Component kontrolü
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned.");
        }

        // Fareyi kilitle
        Cursor.lockState = CursorLockMode.Locked;
        interactText.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        RaycastController();
    }

    void RaycastController()
    {
        float raycastDistance = 3f; // Raycast'in maksimum mesafesi
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Raycast'i görsel olarak çizin (örneðin kýrmýzý renk)
        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        // Raycast'i belirli bir mesafeye kadar kontrol et
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // E tuþu ile etkileþim (örneðin kapý açma)
            if (hit.collider.TryGetComponent(out Iinterectable interactable))
            {
                // Mesajý göster
                interactText.gameObject.SetActive(true);
                canInteract = true;

                // E tuþuna basýldýðýnda etkileþime gir
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
            else
            {
                // Etkileþime girilebilecek nesne deðilse mesajý gizle
                interactText.gameObject.SetActive(false);
                canInteract = false;
            }

            // Fare sol týklamasý ile saldýrý (örneðin NPC'ye saldýrma)
            if (hit.collider.TryGetComponent(out IAttackable attackable))
            {
                // Sol týklama ile saldýr
                if (Input.GetMouseButtonDown(0)) // Sol týklama
                {
                    attackable.Attack();
                }
            }
        }
        else
        {
            // Hiçbir nesneye bakýlmýyorsa mesajý gizle
            interactText.gameObject.SetActive(false);
            canInteract = false;
        }
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

        // Hareket yönü
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Hareketi uygula
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Yerçekimi kontrolü
        if (characterController.isGrounded)
        {
            velocity.y = -2f; // Hafif bir sabit kuvvet uygulayýn
        }
        else
        {
            velocity.y += Physics.gravity.y * Time.deltaTime; // Yerçekimi ekleyin
        }

        // Yerçekimini uygula
        characterController.Move(velocity * Time.deltaTime);
    }
}
