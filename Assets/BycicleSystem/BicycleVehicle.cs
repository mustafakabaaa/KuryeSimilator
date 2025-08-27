using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BicycleVehicle : MonoBehaviour
{
    private Vector2 moveInput;
    private bool brakeInput;
    private bool interactInput;
    float horizontalInput;
    float verticalInput;

    public Transform handle;
    bool braking;
    Rigidbody rb;

    public Vector3 COG;

    [SerializeField] float motorforce;
    [SerializeField] float brakeForce;
    float currentbrakeForce;

    float steeringAngle;
    [SerializeField] float currentSteeringAngle;
    [Range(0f, 0.1f)][SerializeField] float speedteercontrolTime;
    [SerializeField] float maxSteeringAngle;
    [Range(0.000001f, 1)][SerializeField] float turnSmoothing;

    [SerializeField] float maxlayingAngle = 45f;
    public float targetlayingAngle;
    [Range(-40, 40)] public float layingammount;
    [Range(0.000001f, 1)][SerializeField] float leanSmoothing;

    [Header("Wheels Collider")]
    [SerializeField] WheelCollider frontWheel;
    [SerializeField] WheelCollider backWheel;

    [Header("Wheels Transform")]
    [SerializeField] Transform frontWheeltransform;
    [SerializeField] Transform backWheeltransform;

    [Header("Trail Settings")]
    [SerializeField] TrailRenderer fronttrail;
    [SerializeField] TrailRenderer rearttrail;

    [Header("Camera & DropOff Point Offset")]
    [SerializeField] private Transform _dropOfPoint;
    [SerializeField] private GameObject _vehicleCamera;
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private GameObject _player;

    [Header("Coasting Settings")]
    [SerializeField] private float coastingDrag = 0.5f;
    [SerializeField] private float normalDrag = 0.1f;
    [SerializeField] private float minSpeedThreshold = 0.5f;
    [SerializeField] private float autoBrakeForce = 50f;

    private BicycleControlsA controls;
    [SerializeField] private bool isPlayerOnBoard = false;
    public bool frontGrounded;
    public bool rearGrounded;



    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private LayerMask playerLayer;
    private bool isPlayerInRange = false;
    private GameObject currentPlayer;

    void Awake()
    {
        controls = new BicycleControlsA();

        controls.Bicycle.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Bicycle.Move.canceled += ctx => moveInput = Vector2.zero;
        controls.Bicycle.Brake.performed += ctx => brakeInput = true;
        controls.Bicycle.Brake.canceled += ctx => brakeInput = false;
        controls.Bicycle.Interact.performed += ctx => HandleInteraction();
    }

    void OnEnable()
    {
        controls.Bicycle.Enable();
    }

    void OnDisable()
    {
        controls.Bicycle.Disable();
    }

    void Start()
    {
        StopEmitTrail();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = COG;
    }

    void Update()
    {
        CheckPlayerInRange();
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
                InteractionManager.Instance.RegisterVehicle(this, transform, "Bin (F)", true);
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
            MountBicycle();
        }
        else if (isPlayerOnBoard)
        {
            DismountBicycle();
        }
    }

    public void MountBicycle()
    {
        if (currentPlayer == null) return;

        isPlayerOnBoard = true;
        _player = currentPlayer;

        changeCamera();
        playerStatue();

        // InteractionManager'dan kaydı kaldır
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.UnregisterVehicle(this);
        }
    }

    public void DismountBicycle()
    {
        isPlayerOnBoard = false;

        changeCamera();
        playerStatue();

        // İndikten sonra InteractionManager'a tekrar kayıt ol
        if (InteractionManager.Instance != null && currentPlayer != null)
        {
            InteractionManager.Instance.RegisterVehicle(this, transform, "Bin (F)", true);
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
            LayOnTurn();

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
                ReleaseBrakibg();
            }

            UpdateWheels();
            UpdateHandle();
            LayOnTurn();
            DownPresureOnSpeed();
        }
    }

    public void GetInput()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
        braking = brakeInput;
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
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            backWheel.motorTorque = verticalInput * motorforce;
            rb.drag = normalDrag;
            ReleaseBrakibg();
        }
        else
        {
            backWheel.motorTorque = 0f;

            if (rb.velocity.magnitude > minSpeedThreshold)
            {
                rb.drag = coastingDrag;
                backWheel.brakeTorque = autoBrakeForce;
                frontWheel.brakeTorque = autoBrakeForce;
            }
            else
            {
                rb.velocity = Vector3.zero;
                backWheel.brakeTorque = brakeForce;
                frontWheel.brakeTorque = brakeForce;
            }
        }

        if (braking)
        {
            ApplyBraking();
        }
    }

    public void ApplyBraking()
    {
        frontWheel.brakeTorque = brakeForce;
        backWheel.brakeTorque = brakeForce;
    }

    public void ReleaseBrakibg()
    {
        frontWheel.brakeTorque = 0;
        backWheel.brakeTorque = 0;
    }

    public void SpeedSteerinReductor()
    {
        float speed = rb.velocity.magnitude;

        if (speed < 5)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 50, speedteercontrolTime);
        else if (speed < 10)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 30, speedteercontrolTime);
        else if (speed < 15)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 15, speedteercontrolTime);
        else if (speed < 20)
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 10, speedteercontrolTime);
        else
            maxSteeringAngle = Mathf.Lerp(maxSteeringAngle, 5, speedteercontrolTime);
    }

    public void HandleSteering()
    {
        SpeedSteerinReductor();

        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * horizontalInput, turnSmoothing);
        frontWheel.steerAngle = currentSteeringAngle;

        targetlayingAngle = maxlayingAngle * -horizontalInput;
    }

    private void LayOnTurn()
    {
        Vector3 currentRot = transform.rotation.eulerAngles;

        if (rb.velocity.magnitude < 1)
        {
            layingammount = Mathf.LerpAngle(layingammount, 0f, 0.05f);
            transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, layingammount);
            return;
        }

        if (Mathf.Abs(currentSteeringAngle) < 0.5f)
        {
            layingammount = Mathf.LerpAngle(layingammount, 0f, leanSmoothing);
        }
        else
        {
            layingammount = Mathf.LerpAngle(layingammount, targetlayingAngle, leanSmoothing);
            rb.centerOfMass = new Vector3(rb.centerOfMass.x, COG.y, rb.centerOfMass.z);
        }

        transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, layingammount);
    }

    public void DownPresureOnSpeed()
    {
        Vector3 downforce = Vector3.down;
        float downpressure;
        if (rb.velocity.magnitude > 5)
        {
            downpressure = rb.velocity.magnitude;
            rb.AddForce(downforce * downpressure, ForceMode.Force);
        }
    }

    public void UpdateWheels()
    {
        UpdateSingleWheel(frontWheel, frontWheeltransform);
        UpdateSingleWheel(backWheel, backWheeltransform);
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

    private void EmitTrail()
    {
        frontGrounded = frontWheel.GetGroundHit(out WheelHit Fhit);
        rearGrounded = backWheel.GetGroundHit(out WheelHit Rhit);

        if (frontGrounded)
        {
            fronttrail.emitting = true;
        }
        else
        {
            fronttrail.emitting = false;
        }

        if (rearGrounded)
        {
            rearttrail.emitting = true;
        }
        else
        {
            rearttrail.emitting = false;
        }
    }

    private void StopEmitTrail()
    {
        fronttrail.emitting = false;
        rearttrail.emitting = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}