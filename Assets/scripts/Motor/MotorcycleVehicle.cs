using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MotorcycleVehicle : MonoBehaviour
{
    float horizontalInput;
    float verticalInput;
    private Vector2 moveInput;
    private bool brakeInput;
    private bool interactInput;
    public Transform handle;
    bool braking;

    
    private MotorcycleControls controls;
    Rigidbody rb;
    public Vector3 COG;

    [Header("Engine Settings")]
    [SerializeField] float motorForce = 2000f;
    [SerializeField] float maxSpeed = 120f;
    [SerializeField] float brakeForce = 3000f;
    float currentbrakeForce;

    [Header("Steering Settings")]
    float steeringAngle;
    [SerializeField] float currentSteeringAngle;
    [Range(0f, 0.1f)][SerializeField] float speedSteerControlTime = 0.05f;
    [SerializeField] float maxSteeringAngle = 25f;
    [Range(0.000001f, 1)][SerializeField] float turnSmoothing = 0.1f;

    [Header("Leaning Settings")]
    [SerializeField] float maxLeaningAngle = 45f;
    public float targetLeaningAngle;
    [Range(-40, 40)] public float leaningAmount;
    [Range(0.000001f, 1)][SerializeField] float leanSmoothing = 0.05f;
    [SerializeField] float leanAtSpeedFactor = 0.5f;

    [Header("Wheels Collider")]
    [SerializeField] WheelCollider frontWheel;
    [SerializeField] WheelCollider backWheel;

    [Header("Wheels Transform")]
    [SerializeField] Transform frontWheelTransform;
    [SerializeField] Transform backWheelTransform;

    [Header("Odometer")]
    [SerializeField] private float totalKilometre = 0f;
    private Vector3 lastPosition;
    
    public float TotalKilometre => totalKilometre;

    [Header("Ariza Sistemi")]
    [SerializeField] private ArizaSeviyesi arizaSeviyesi = ArizaSeviyesi.Saglam;
    [SerializeField] private float sonArizaKontrolKm = 0f;
    [SerializeField] private float motorGucuCarpani = 1f;
    private const float ARIZA_KONTROL_ARALIGI = 2f;
    
    public ArizaSeviyesi ArizaSeviyesi => arizaSeviyesi;
    public float MotorGucuCarpani => motorGucuCarpani;

    [Header("Camera & DropOff Point Offset")]
    [SerializeField] private Transform _dropOfPoint;
    [SerializeField] private GameObject _vehicleCamera;
    [SerializeField] public GameObject _playerCamera;
    [SerializeField] public GameObject _player;

    [Header("Coasting Settings")]
    [SerializeField] private float coastingDrag = 0.5f;
    [SerializeField] private float normalDrag = 0.1f;
    [SerializeField] private float minSpeedThreshold = 0.5f;
    [SerializeField] private float autoBrakeForce = 100f;

    [SerializeField] private bool isPlayerOnBoard = false;
    public bool frontGrounded;
    public bool rearGrounded;

    [Header("Motor Sound")]
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 2.0f;

    [SerializeField] private float currentSpeed;
    private float engineRPM;
    [SerializeField] private float maxRPM = 8000f;
    [SerializeField] private float idleRPM = 1000f;
    [SerializeField] private AnimationCurve torqueCurve;



    [Header("Reverse Settings")]
    [SerializeField] private float maxReverseSpeed = 10f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private LayerMask playerLayer;
    private bool isPlayerInRange = false;
    private GameObject currentPlayer;

    public float EngineRPM => engineRPM;
    public bool IsEngineRunning => isPlayerOnBoard;
    public float CurrentSpeed => currentSpeed;
    public bool IsPlayerOnBoard => isPlayerOnBoard;

    void Awake()
    {
        controls = new MotorcycleControls();
        
        controls.Motorcycle.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Motorcycle.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Motorcycle.Brake.performed += ctx => brakeInput = true;
        controls.Motorcycle.Brake.canceled += ctx => brakeInput = false;
        controls.Motorcycle.Interact.performed += ctx => HandleInteraction();
    }

    void OnEnable()
    {
        controls.Motorcycle.Enable();
    }

    void OnDisable()
    {
        controls.Motorcycle.Disable();
    }

    void Start()
    {
        
        WheelStartSettings();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = COG;
        lastPosition = transform.position;

        if (engineAudioSource != null)
        {
            engineAudioSource.loop = true;
            engineAudioSource.playOnAwake = true;
            engineAudioSource.Play();
        }
    }

    private void WheelStartSettings()
    {
        WheelFrictionCurve forwardFriction = new WheelFrictionCurve
        {
            extremumSlip = 0.4f,
            extremumValue = 1.2f,
            asymptoteSlip = 0.8f,
            asymptoteValue = 0.6f,
            stiffness = 1.4f
        };

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve
        {
            extremumSlip = 0.2f,
            extremumValue = 1.0f,
            asymptoteSlip = 0.5f,
            asymptoteValue = 0.5f,
            stiffness = 1.4f
        };

        frontWheel.forwardFriction = forwardFriction;
        frontWheel.sidewaysFriction = sidewaysFriction;
        backWheel.forwardFriction = forwardFriction;
        backWheel.sidewaysFriction = sidewaysFriction;
    }

    void Update()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
        braking = brakeInput;
        currentSpeed = rb.velocity.magnitude * 3.6f;

        CheckPlayerInRange();
        UpdateEngineSound();
    }

    private void CheckPlayerInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRadius, playerLayer);
        isPlayerInRange = hitColliders.Length > 0;

        if (isPlayerInRange && hitColliders.Length > 0)
        {
            currentPlayer = hitColliders[0].gameObject;

            if (!isPlayerOnBoard && InteractionManager.Instance != null)
            {
                // InteractionManager'a kayıt ol
                InteractionManager.Instance.RegisterVehicle(this, transform, "ui.interact_drive", true);
            }
        }
        else
        {
            currentPlayer = null;
            if (InteractionManager.Instance != null)
            {
                InteractionManager.Instance.UnregisterVehicle(this);
            }
        }
    }

    private void HandleInteraction()
    {
        if (isPlayerInRange && !isPlayerOnBoard && currentPlayer != null)
        {
            MountMotorcycle();
        }
        else if (isPlayerOnBoard)
        {
            DismountMotorcycle();
        }
    }

    public void MountMotorcycle()
    {
        if (currentPlayer == null) return;

        isPlayerOnBoard = true;
        _player = currentPlayer;
        
        // Son kullanılan aracı kaydet
        
        changeCamera();
        playerStatue();

        if (MotorEventManager.Instance != null)
        {
            MotorEventManager.Instance.TriggerMountEvent(this);
        }

        // InteractionManager'dan kaydı kaldır
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.UnregisterVehicle(this);
        }
        

    }

    public void DismountMotorcycle()
    {
        isPlayerOnBoard = false;

        changeCamera();
        playerStatue();

        // İndikten sonra InteractionManager'a tekrar kayıt ol
        if (InteractionManager.Instance != null && currentPlayer != null)
        {
            InteractionManager.Instance.RegisterVehicle(this, transform, "ui.interact_drive", true);
        }

        if (MotorEventManager.Instance != null)
        {
            MotorEventManager.Instance.TriggerDismountEvent();
        }
    }

    private void UpdateEngineSound()
    {
        if (engineAudioSource != null)
        {
            float rpmPercent = Mathf.InverseLerp(idleRPM, maxRPM, engineRPM);
            engineAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmPercent);
        }
    }

    void FixedUpdate()
    {
        WheelHit hit;
        bool isGrounded = frontWheel.GetGroundHit(out hit) || backWheel.GetGroundHit(out hit);

        if (!isPlayerOnBoard)
        {
            CoastToStop();
            UpdateWheels();
            MaintainBalance();

            if (!isGrounded)
            {
                rb.AddForce(Vector3.down * 1500f, ForceMode.Force);
            }
            return;
        }
        else
        {
            if (!isGrounded)
            {
                backWheel.motorTorque = 0f;
                frontWheel.brakeTorque = 3000f;
                rb.AddForce(Vector3.down * 2000f, ForceMode.Force);
            }
            else
            {
                GetInput();
                HandleEngine();
                HandleSteering();
                ReleaseBraking();
            }

            UpdateWheels();
            UpdateHandle();
            LeanOnTurn();
            ApplyDownforce();
        }
        
        UpdateOdometer();
    }

    private void UpdateOdometer()
    {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(lastPosition, currentPosition);
        
        if (distance > 0.001f && (frontWheel.isGrounded || backWheel.isGrounded))
        {
            totalKilometre += distance / 1000f;
            
            // 50km'de bir ariza kontrolü
            if (totalKilometre - sonArizaKontrolKm >= ARIZA_KONTROL_ARALIGI)
            {
                CheckArizaRisk();
                sonArizaKontrolKm = totalKilometre;
            }
        }
        
        lastPosition = currentPosition;
    }

    public void SetTotalKilometre(float km)
    {
        totalKilometre = km;
    }

    public void GetInput()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
        braking = brakeInput;
        
        if (braking && verticalInput > 0.1f)
        {
            verticalInput = 0f;
        }
    }

    private void CoastToStop()
    {
        backWheel.motorTorque = 0f;
        frontWheel.motorTorque = 0f;

        if (rb.velocity.magnitude > minSpeedThreshold)
        {
            rb.drag = coastingDrag;
            backWheel.brakeTorque = autoBrakeForce * 0.25f;
            frontWheel.brakeTorque = autoBrakeForce * 0.25f;
        }
        else
        {
            rb.velocity = Vector3.zero;
            backWheel.brakeTorque = brakeForce;
            frontWheel.brakeTorque = brakeForce;
        }

        if (!frontWheel.isGrounded && !backWheel.isGrounded)
        {
            rb.drag = 0f;
        }
    }

    public void HandleEngine()
    {
        currentSpeed = rb.velocity.magnitude * 3.6f;
        engineRPM = Mathf.Lerp(idleRPM, maxRPM, currentSpeed / maxSpeed);

        if (braking)
        {
            backWheel.motorTorque = 0f;
            ApplyBraking();

            if (currentSpeed < 0.5f)
            {
                rb.velocity = Vector3.zero;
                engineRPM = idleRPM;
            }
            return;
        }

        if (verticalInput < -0.1f)
        {
            float speed = rb.velocity.magnitude * 3.6f;
            bool isMovingForward = Vector3.Dot(rb.velocity.normalized, transform.forward) > 0.1f;

            if (isMovingForward && speed > 1f)
            {
                backWheel.motorTorque = 0f;
                ApplyAutoBrake();
                return;
            }

            if (speed < maxReverseSpeed)
            {
                backWheel.motorTorque = verticalInput * motorForce * 0.3f;
            }
            else
            {
                backWheel.motorTorque = 0f;
            }

            rb.drag = normalDrag;
            ReleaseBraking();
            return;
        }

        if (verticalInput > 0.1f)
        {
            float availableTorque = torqueCurve.Evaluate(engineRPM / maxRPM) * motorForce * motorGucuCarpani;

            if (currentSpeed < maxSpeed * motorGucuCarpani)
            {
                backWheel.motorTorque = verticalInput * availableTorque;
            }
            else
            {
                backWheel.motorTorque = 0f;
            }

            rb.drag = normalDrag;
            ReleaseBraking();
        }
        else
        {
            backWheel.motorTorque = 0f;

            if (rb.velocity.magnitude > minSpeedThreshold)
            {
                rb.drag = coastingDrag;
                ApplyAutoBrake();
            }
            else
            {
                rb.velocity = Vector3.zero;
                engineRPM = idleRPM;
            }
        }
    }

    private void ApplyAutoBrake()
    {
        backWheel.brakeTorque = autoBrakeForce;
        frontWheel.brakeTorque = autoBrakeForce * 0.7f;
    }

    private void ApplyFullBrake()
    {
        backWheel.brakeTorque = brakeForce;
        frontWheel.brakeTorque = brakeForce * 0.7f;
    }

    public void ApplyBraking()
    {
        frontWheel.brakeTorque = brakeForce;
        backWheel.brakeTorque = brakeForce;
    }

    public void ReleaseBraking()
    {
        frontWheel.brakeTorque = 0;
        backWheel.brakeTorque = 0;
    }

    public void HandleSteering()
    {
        SpeedSteeringReductor();

        if (Mathf.Abs(horizontalInput) < 0.01f)
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, 0f, turnSmoothing * 2f);
        }
        else
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle,
                maxSteeringAngle * horizontalInput,
                turnSmoothing * Time.fixedDeltaTime * 50f);
        }

        frontWheel.steerAngle = currentSteeringAngle;
        targetLeaningAngle = maxLeaningAngle * -horizontalInput * Mathf.Clamp01(currentSpeed / 30f) * leanAtSpeedFactor;
    }

    public void SpeedSteeringReductor()
    {
        if (currentSpeed < 20f)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 25f, speedSteerControlTime);
        else if (currentSpeed < 40f)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 20f, speedSteerControlTime);
        else if (currentSpeed < 60f)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 15f, speedSteerControlTime);
        else if (currentSpeed < 80f)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 10f, speedSteerControlTime);
        else
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 5f, speedSteerControlTime);
    }

    private void LeanOnTurn()
    {
        Vector3 currentRot = transform.rotation.eulerAngles;

        if (currentSpeed < 5f)
        {
            leaningAmount = Mathf.LerpAngle(leaningAmount, 0f, 0.1f);
            transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, leaningAmount);
            return;
        }

        if (Mathf.Abs(currentSteeringAngle) < 0.5f)
        {
            leaningAmount = Mathf.LerpAngle(leaningAmount, 0f, leanSmoothing * 0.5f);
        }
        else
        {
            leaningAmount = Mathf.LerpAngle(leaningAmount, targetLeaningAngle, leanSmoothing * Time.fixedDeltaTime * 50f);
        }

        transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, leaningAmount);

        if (currentSpeed > 50f)
        {
            rb.AddTorque(transform.up * -horizontalInput * currentSpeed * 0.1f);
        }
    }

    private void MaintainBalance()
    {
        if (!frontGrounded || !rearGrounded) return;

        Vector3 currentRot = transform.rotation.eulerAngles;
        leaningAmount = Mathf.LerpAngle(leaningAmount, 0f, leanSmoothing);
        transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, leaningAmount);
    }

    private void ApplyDownforce()
    {
        if (currentSpeed > 30f)
        {
            float downforceAmount = Mathf.Pow(currentSpeed / 100f, 2) * 100f;
            rb.AddForce(Vector3.down * downforceAmount, ForceMode.Force);
        }
    }

    public void UpdateWheels()
    {
        UpdateSingleWheel(frontWheel, frontWheelTransform);
        UpdateSingleWheel(backWheel, backWheelTransform);
        frontGrounded = frontWheel.isGrounded;
        rearGrounded = backWheel.isGrounded;
    }

    public void UpdateHandle()
    {
        handle.localRotation = Quaternion.Euler(
            handle.localRotation.eulerAngles.x,
            currentSteeringAngle * 1.5f,
            handle.localRotation.eulerAngles.z
        );
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void changeCamera()
    {
        if (isPlayerOnBoard)
        {
            _vehicleCamera.SetActive(true);
            _playerCamera.SetActive(false);
            playerStatue();
            MinimapTargetManager.Instance.SetTarget(transform);
            FindObjectOfType<MinimapPlayerIcon>().SetTarget(this.transform);
        }
        else
        {
            _vehicleCamera.SetActive(false);
            _playerCamera.SetActive(true);
            playerStatue();
            MinimapTargetManager.Instance.SetTarget(_player.transform);
            FindObjectOfType<MinimapPlayerIcon>().SetTarget(_player.transform);
        }
    }
    
    private void playerStatue()
    {
        if (isPlayerOnBoard)
        {
            _player.transform.SetParent(this.transform);
            _player.SetActive(false);
        }
        else
        {
            _player.transform.SetParent(null);
            _player.transform.position = _dropOfPoint.position;
            _player.transform.rotation = Quaternion.identity;
            _player.SetActive(true);

            if (_playerCamera != null)
            {
                _playerCamera.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    private void CheckArizaRisk()
    {
        if (arizaSeviyesi == ArizaSeviyesi.Agir) return; // Ağır arıza zaten var

        float arizaOlasiligi = GetArizaOlasiligi();
        float rastgele = Random.Range(0f, 1f);

        if (rastgele < arizaOlasiligi)
        {
            ArizaOlustur();
        }
    }

    private float GetArizaOlasiligi()
    {
        if (totalKilometre < 500f) return 0.02f;      // %2
        if (totalKilometre < 1000f) return 0.07f;      // %7
        if (totalKilometre < 4000f) return 0.18f;     // %18
        return 0.30f;                                   // %30+
    }

    private void ArizaOlustur()
    {
        float rastgele = Random.Range(0f, 1f);

        if (rastgele < 0.70f) // %70 hafif
        {
            arizaSeviyesi = ArizaSeviyesi.Hafif;
            motorGucuCarpani = 0.9f;
            Debug.Log("Hafif arıza oluştu! Motor gücü %10 düştü.");
        }
        else if (rastgele < 0.95f) // %25 orta
        {
            arizaSeviyesi = ArizaSeviyesi.Orta;
            motorGucuCarpani = 0.7f;
            Debug.Log("Orta arıza oluştu! Motor gücü %30 düştü.");
        }
        else // %5 ağır
        {
            arizaSeviyesi = ArizaSeviyesi.Agir;
            motorGucuCarpani = 0f;
            Debug.Log("Ağır arıza oluştu! Motor stop etti!");
        }

        // UI'ı haberdar et
        var motorcycleShop = FindObjectOfType<MotorcycleShop>();
        if (motorcycleShop != null)
        {
            motorcycleShop.UpdateAllUI();
        }
    }

    public void TamirEt()
    {
        arizaSeviyesi = ArizaSeviyesi.Saglam;
        motorGucuCarpani = 1f;
        Debug.Log("Motor tamir edildi!");
    }

    public void SetArizaVerisi(ArizaSeviyesi seviye, float sonKontrol)
    {
        arizaSeviyesi = seviye;
        sonArizaKontrolKm = sonKontrol;
        
        // Arıza seviyesine göre güç çarpanını ayarla
        switch (arizaSeviyesi)
        {
            case ArizaSeviyesi.Saglam:
                motorGucuCarpani = 1f;
                break;
            case ArizaSeviyesi.Hafif:
                motorGucuCarpani = 0.9f;
                break;
            case ArizaSeviyesi.Orta:
                motorGucuCarpani = 0.7f;
                break;
            case ArizaSeviyesi.Agir:
                motorGucuCarpani = 0f;
                break;
        }
    }

    // Test Metotları
    [ContextMenu("Ariza Riski Test Et")]
    void TestArizaRiski() {
        Debug.Log($"📍 Mevcut KM: {totalKilometre:F1}");
        Debug.Log($"🎯 Ariza Olasılığı: {GetArizaOlasiligi()*100:F1}%");
        Debug.Log($"⚠️ Mevcut Ariza: {arizaSeviyesi}");
        Debug.Log($"🔧 Motor Gücü Çarpanı: {motorGucuCarpani*100:F0}%");
        CheckArizaRisk();
    }

    [ContextMenu("Ariza Sansı Dene (100 Test)")]
    void ArizaSansiDene() {
        Debug.Log($"=== {totalKilometre:F1} KM'DE 100 ARIZA TESTİ ===");
        
        int saglamSayisi = 0;
        int hafifAriza = 0;
        int ortaAriza = 0;
        int agirAriza = 0;
        
        float arizaOlasiligi = GetArizaOlasiligi();
        
        for (int i = 0; i < 100; i++)
        {
            float rastgele = Random.Range(0f, 1f);
            
            if (rastgele < arizaOlasiligi)
            {
                float arizaRastgele = Random.Range(0f, 1f);
                if (arizaRastgele < 0.70f) hafifAriza++;
                else if (arizaRastgele < 0.95f) ortaAriza++;
                else agirAriza++;
            }
            else
            {
                saglamSayisi++;
            }
        }
        
        Debug.Log($"🎯 Teorik Risk: {arizaOlasiligi*100:F1}%");
        Debug.Log($"📊 Sonuçlar:");
        Debug.Log($"   ✅ Sağlam: {saglamSayisi}%");
        Debug.Log($"   🟡 Hafif Arıza: {hafifAriza}%");
        Debug.Log($"   🟠 Orta Arıza: {ortaAriza}%");
        Debug.Log($"   🔴 Ağır Arıza: {agirAriza}%");
        Debug.Log($"=== TEST BİTTİ ===");
    }

    [ContextMenu("KM 500 Yap (Düşük Risk)")]
    void Km500Yap() {
        SetTotalKilometre(500f);
        sonArizaKontrolKm = 450f;
        Debug.Log($"📍 Kilometre 500 yapıldı - Risk: {GetArizaOlasiligi()*100:F1}%");
    }

    [ContextMenu("KM 5000 Yap (Orta Risk)")]
    void Km5000Yap() {
        SetTotalKilometre(5000f);
        sonArizaKontrolKm = 4999f; // 1km aralık ile
        Debug.Log($"📍 Kilometre 5000 yapıldı - Risk: {GetArizaOlasiligi()*100:F1}%");
    }

    [ContextMenu("KM 15000 Yap (Yüksek Risk)")]
    void Km15000Yap() {
        SetTotalKilometre(15000f);
        sonArizaKontrolKm = 14999f; // 1km aralık ile
        Debug.Log($"📍 Kilometre 15000 yapıldı - Risk: {GetArizaOlasiligi()*100:F1}%");
    }

    [ContextMenu("50km Sür (Ariza Kontrolü Tetikle)")]
    void Km50Sur() {
        SetTotalKilometre(totalKilometre + 50f);
        Debug.Log($"📍 50km sürüldü - Toplam: {totalKilometre:F1}km");
        CheckArizaRisk();
    }

    [ContextMenu("Arızayı Tamir Et")]
    void ArizayiTamirEt() {
        TamirEt();
        Debug.Log($"🔧 Motor tamir edildi - Ariza: {arizaSeviyesi}");
    }

    [ContextMenu("Tüm Durumları Test Et")]
    void TumDurumlariTestEt() {
        Debug.Log("=== ARIZA SİSTEMİ TEST BAŞLATILIYOR ===");
        
        // 500km test
        Km500Yap();
        TestArizaRiski();
        
        // 5000km test  
        Km5000Yap();
        TestArizaRiski();
        
        // 15000km test
        Km15000Yap();
        TestArizaRiski();
        
        Debug.Log("=== ARIZA SİSTEMİ TEST BİTTİ ===");
    }
}