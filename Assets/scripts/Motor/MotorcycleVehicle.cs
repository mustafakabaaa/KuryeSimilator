using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Camera & DropOff Point Offset")]
    [SerializeField] private Transform _dropOfPoint;
    [SerializeField] private GameObject _vehicleCamera;
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private GameObject _player;

    [Header("Coasting Settings")]
    [SerializeField] private float coastingDrag = 0.5f;
    [SerializeField] private float normalDrag = 0.1f;
    [SerializeField] private float minSpeedThreshold = 0.5f;
    [SerializeField] private float autoBrakeForce = 100f;

    private bool isPlayerOnBoard = false;
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
    // MotorcycleVehicle sýnýfýna bu property'leri ekleyin
    public float EngineRPM => engineRPM;
    public bool IsEngineRunning => isPlayerOnBoard;
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI interactText; // Inspector'dan baðlayýn


    [Header("Reverse Settings")]
    [SerializeField] private float maxReverseSpeed = 10f; // Geri gitme maksimum hýzý (km/h)
    void Start()
    {
        WheelStartSettings();

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = COG;

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
        isPlayerWannaExitBicycle();
        currentSpeed = rb.velocity.magnitude * 3.6f;

        
    }

    private void UpdateEngineSound()
    {
        if (engineAudioSource != null)
        {
            float rpmPercent = Mathf.InverseLerp(idleRPM, maxRPM, engineRPM);
            engineAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmPercent);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 worldCOG = transform.TransformPoint(COG);
        Gizmos.DrawSphere(worldCOG, 0.1f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, worldCOG);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(worldCOG, worldCOG + transform.right * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(worldCOG, worldCOG + transform.up * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(worldCOG, worldCOG + transform.forward * 0.5f);
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
        UpdateEngineSound(); // <-- Her frame motor sesi güncelleniyor
    }

    private void isPlayerWannaExitBicycle()
    {
        if (isPlayerOnBoard && Input.GetKeyDown(KeyCode.E))
        {
            if (currentSpeed > 11f)
            {
                ToastManager.Instance.ShowToast("Hýzlýsýn, inemezsin!");
                return; // 11 km/h üstü hýzda inemez
            }

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
        currentSpeed = rb.velocity.magnitude * 3.6f;
        engineRPM = Mathf.Lerp(idleRPM, maxRPM, currentSpeed / maxSpeed);

        float forwardDot = Vector3.Dot(rb.velocity.normalized, transform.forward);

        if (verticalInput < -0.1f)
        {
            float speed = rb.velocity.magnitude * 3.6f; // m/s to km/h
            bool isMovingForward = forwardDot > 0.1f;

            if (isMovingForward)
            {
                if (speed > 15f)
                {
                    backWheel.motorTorque = 0f;
                    ApplyFullBrake();
                    return;
                }
                else
                {
                    float reverseTorque = verticalInput * motorForce * 0.3f;
                    backWheel.motorTorque = reverseTorque;
                    rb.drag = normalDrag;
                    ReleaseBraking();
                    return;
                }
            }

            // Eðer zaten ileri gitmiyorsak ve geri gidiyorsak, hýz sýnýrýna bak
            if (speed < maxReverseSpeed)
            {
                backWheel.motorTorque = verticalInput * motorForce * 0.1f;
            }
            else
            {
                backWheel.motorTorque = 0f;
            }

            rb.drag = normalDrag;
            ReleaseBraking();
            return;
        }



        // --- Ýleri gitme isteði (W tuþu) ---
        if (verticalInput > 0.1f)
        {
            float availableTorque = torqueCurve.Evaluate(engineRPM / maxRPM) * motorForce;

            if (currentSpeed < maxSpeed)
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
        else // Gaz verilmiyor (yavaþlama)
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
                ApplyFullBrake();
            }
        }

        // --- Manuel fren (Space tuþu) ---
        if (braking)
        {
            ApplyBraking();
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
        frontWheel.brakeTorque = brakeForce ;
        backWheel.brakeTorque = brakeForce ;
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
            // Minimap motoru takip etsin
            MinimapTargetManager.Instance.SetTarget(transform); // bisiklet
            FindObjectOfType<MinimapPlayerIcon>().SetTarget(this.transform);
        }
        else
        {
            _vehicleCamera.SetActive(false);
            _playerCamera.SetActive(true);
            playerStatue();
            // Minimap tekrar oyuncuyu takip etsin
            MinimapTargetManager.Instance.SetTarget(_player.transform); // oyuncu
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

    public void Interact()
    {
        isPlayerOnBoard = !isPlayerOnBoard;
        changeCamera();

        // Motora binildiðinde UI metnini gizle
        if (interactText != null)
        {
            interactText.gameObject.SetActive(false);
        }
    }

    public string GetInteractionText()
    {
      
        
            return "Bin (E)"; // Binme metni göster
        
    }

    public bool CanInteract()
    {
        return true;
    }
}
