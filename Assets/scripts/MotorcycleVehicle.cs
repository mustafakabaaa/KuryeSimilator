using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MotorcycleVehicle : MonoBehaviour, Iinterectable
{
    float horizontalInput;
    float verticalInput;

    public Transform handle;
    bool braking;
    Rigidbody rb;

    public Vector3 COG;

    [Header("Engine Settings")]
    [SerializeField] float motorForce = 2000f; // Increased for motorcycle
    [SerializeField] float maxSpeed = 120f; // Added speed limit
    [SerializeField] float brakeForce = 3000f; // Increased braking force
    float currentbrakeForce;

    [Header("Steering Settings")]
    float steeringAngle;
    [SerializeField] float currentSteeringAngle;
    [Range(0f, 0.1f)][SerializeField] float speedSteerControlTime = 0.05f;
    [SerializeField] float maxSteeringAngle = 25f; // Reduced for motorcycle
    [Range(0.000001f, 1)][SerializeField] float turnSmoothing = 0.1f;

    [Header("Leaning Settings")]
    [SerializeField] float maxLeaningAngle = 45f;
    public float targetLeaningAngle;
    [Range(-40, 40)] public float leaningAmount;
    [Range(0.000001f, 1)][SerializeField] float leanSmoothing = 0.05f;
    [SerializeField] float leanAtSpeedFactor = 0.5f; // How much speed affects leaning

    [Header("Wheels Collider")]
    [SerializeField] WheelCollider frontWheel;
    [SerializeField] WheelCollider backWheel;

    [Header("Wheels Transform")]
    [SerializeField] Transform frontWheelTransform;
    [SerializeField] Transform backWheelTransform;

    [Header("Camera & DropOff Point Offset")]
    [SerializeField] private Transform _dropOfPoint;
    [SerializeField] private GameObject _vehicleCamera;
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private GameObject _player;

    [Header("Coasting Settings")]
    [SerializeField] private float coastingDrag = 0.5f;
    [SerializeField] private float normalDrag = 0.1f;
    [SerializeField] private float minSpeedThreshold = 0.5f;
    [SerializeField] private float autoBrakeForce = 100f; // Increased for motorcycle

    private bool isPlayerOnBoard = false;
    public bool frontGrounded;
    public bool rearGrounded;

    // Motorcycle-specific variables
    [SerializeField]
    private float currentSpeed;
    private float engineRPM;
    [SerializeField] private float maxRPM = 8000f;
    [SerializeField] private float idleRPM = 1000f;
    [SerializeField] private AnimationCurve torqueCurve; // Torque curve based on RPM

    void Start()
    {  // WheelCollider'ýn yerleþik sürtünme ayarlarýný kullan
        WheelStartSettings();

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = COG;
    }

    private void WheelStartSettings()
    {
        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = 0.4f;
        forwardFriction.extremumValue = 1.2f;
        forwardFriction.asymptoteSlip = 0.8f;
        forwardFriction.asymptoteValue = 0.6f;
        forwardFriction.stiffness = 1.4f; // Ana sürtünme çarpaný

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = 0.2f;
        sidewaysFriction.extremumValue = 1.0f;
        sidewaysFriction.asymptoteSlip = 0.5f;
        sidewaysFriction.asymptoteValue = 0.5f;
        sidewaysFriction.stiffness = 1.4f;

        // Tekerleklere uygula
        frontWheel.forwardFriction = forwardFriction;
        frontWheel.sidewaysFriction = sidewaysFriction;

        backWheel.forwardFriction = forwardFriction;
        backWheel.sidewaysFriction = sidewaysFriction;
    }

    void Update()
    {

        Debug.Log("COG World Position: " + transform.TransformPoint(COG));

        isPlayerWannaExitBicycle(); // Orijinal bisiklet kodundaki isimle çaðýr
        currentSpeed = rb.velocity.magnitude * 3.6f;
    }
    void OnDrawGizmos()
    {
        // COG noktasýný kýrmýzý küre ile iþaretle
        Gizmos.color = Color.red;
        Vector3 worldCOG = transform.TransformPoint(COG);
        Gizmos.DrawSphere(worldCOG, 0.1f);

        // Motorun merkezinden COG'ye çizgi çek
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, worldCOG);

        // Koordinat eksenlerini çiz (X: Kýrmýzý, Y: Yeþil, Z: Mavi)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(worldCOG, worldCOG + transform.right * 0.5f); // X ekseni
        Gizmos.color = Color.green;
        Gizmos.DrawLine(worldCOG, worldCOG + transform.up * 0.5f); // Y ekseni
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(worldCOG, worldCOG + transform.forward * 0.5f); // Z ekseni
    }
    
    void FixedUpdate()
    {
        // Mevcut FixedUpdate kodlarýnýzýn üstüne veya uygun bir yerine ekleyin
        WheelHit hit;
        bool isGrounded = frontWheel.GetGroundHit(out hit) || backWheel.GetGroundHit(out hit);

        if (!isPlayerOnBoard)
        {
            CoastToStop();
            UpdateWheels();
            MaintainBalance();

            // Havada kaldýysa ekstra downforce uygula
            if (!isGrounded)
            {
                rb.AddForce(Vector3.down * 1500f, ForceMode.Force);
            }
            return;
        }
        else
        {
            // Tekerlekler yerden kesilirse gücü kes
            if (!isGrounded)
            {
                backWheel.motorTorque = 0f;
                frontWheel.brakeTorque = 3000f;
                rb.AddForce(Vector3.down * 2000f, ForceMode.Force); // Daha agresif downforce
            }
            else
            {
                GetInput();
                HandleEngine();
                HandleSteering();
                ReleaseBraking(); // Yere temas varsa freni serbest býrak
            }

            UpdateWheels();
            UpdateHandle();
            LeanOnTurn();
            ApplyDownforce();
        }
      
    }
   
    private void isPlayerWannaExitBicycle()
    {
        if (isPlayerOnBoard && Input.GetKeyDown(KeyCode.E))
        {
            isPlayerOnBoard = !isPlayerOnBoard;
            changeCamera();
            playerStatue();
        }
    }
    public void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        braking = Input.GetKey(KeyCode.Space);
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
        // Calculate RPM based on speed and gear ratio (simplified)
        engineRPM = Mathf.Lerp(idleRPM, maxRPM, currentSpeed / maxSpeed);

        // Get torque from curve based on RPM
        float availableTorque = torqueCurve.Evaluate(engineRPM / maxRPM) * motorForce;

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            // Apply torque with RPM consideration
            backWheel.motorTorque = verticalInput * availableTorque;
            rb.drag = normalDrag;
            ReleaseBraking();

            // Limit max speed
            if (currentSpeed > maxSpeed)
            {
                backWheel.motorTorque = 0;
            }
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
                ApplyFullBrake();
            }
        }

        if (braking)
        {
            ApplyBraking();
        }
    }

    private void ApplyAutoBrake()
    {
        backWheel.brakeTorque = autoBrakeForce;
        frontWheel.brakeTorque = autoBrakeForce * 0.7f; // More brake on rear for motorcycle
    }

    private void ApplyFullBrake()
    {
        backWheel.brakeTorque = brakeForce;
        frontWheel.brakeTorque = brakeForce * 0.7f;
    }

    public void ApplyBraking()
    {
        // More braking power on front wheel (70/30 distribution)
        frontWheel.brakeTorque = brakeForce * 0.7f;
        backWheel.brakeTorque = brakeForce * 0.3f;
    }

    public void ReleaseBraking()
    {
        frontWheel.brakeTorque = 0;
        backWheel.brakeTorque = 0;
    }

    public void HandleSteering()
    {
        SpeedSteeringReductor();

        // Duruþ anýnda steeringAngle = 0 olmalý (input == 0)
        if (Mathf.Abs(horizontalInput) < 0.01f) // Giriþ yoksa
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, 0f, turnSmoothing * 2f); // Hýzlýca sýfýrla
        }
        else
        {
            // Normal direksiyon kontrolü
            currentSteeringAngle = Mathf.Lerp(
                currentSteeringAngle,
                maxSteeringAngle * horizontalInput,
                turnSmoothing * Time.fixedDeltaTime * 50f
            );
        }

        frontWheel.steerAngle = currentSteeringAngle;
        targetLeaningAngle = maxLeaningAngle * -horizontalInput * Mathf.Clamp01(currentSpeed / 30f) * leanAtSpeedFactor;
    }

    public void SpeedSteeringReductor()
    {
        if (currentSpeed < 20f)
        {
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 25f, speedSteerControlTime);
        }
        else if (currentSpeed < 40f)
        {
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 20f, speedSteerControlTime);
        }
        else if (currentSpeed < 60f)
        {
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 15f, speedSteerControlTime);
        }
        else if (currentSpeed < 80f)
        {
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 10f, speedSteerControlTime);
        }
        else
        {
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 5f, speedSteerControlTime);
        }
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

        // More natural leaning for motorcycle
        if (Mathf.Abs(currentSteeringAngle) < 0.5f)
        {
            // Gradually return to upright position
            leaningAmount = Mathf.LerpAngle(leaningAmount, 0f, leanSmoothing * 0.5f);
        }
        else
        {
            // Lean into the turn
            leaningAmount = Mathf.LerpAngle(leaningAmount,
                                          targetLeaningAngle,
                                          leanSmoothing * Time.fixedDeltaTime * 50f);
        }

        // Apply the leaning rotation
        transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, leaningAmount);

        // Small counter-steering effect at high speeds
        if (currentSpeed > 50f)
        {
            rb.AddTorque(transform.up * -horizontalInput * currentSpeed * 0.1f);
        }
    }

    private void MaintainBalance()
    {
        // Motorcycle should stay upright when not ridden
        if (!frontGrounded || !rearGrounded) return;

        Vector3 currentRot = transform.rotation.eulerAngles;
        leaningAmount = Mathf.LerpAngle(leaningAmount, 0f, leanSmoothing);
        transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, leaningAmount);
    }

    private void ApplyDownforce()
    {
        // More downforce at higher speeds for motorcycle
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

        // Check if wheels are grounded
        frontGrounded = frontWheel.isGrounded;
        rearGrounded = backWheel.isGrounded;
    }

    public void UpdateHandle()
    {
        handle.localRotation = Quaternion.Euler(
            handle.localRotation.eulerAngles.x,
            currentSteeringAngle * 1.5f, // Handle turns more than wheel
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
        }
        else
        {
            _vehicleCamera.SetActive(false);
            _playerCamera.SetActive(true);
            playerStatue();
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

    public void Interact()
    {
        isPlayerOnBoard = !isPlayerOnBoard;
        changeCamera();
    }

    public string GetInteractionText()
    {
        return "Bin (E)";
    }

    public bool CanInteract()
    {
        return true;
    }

}