using UnityEngine;
using System.Collections;

public class BicycleController : MonoBehaviour, Iinterectable
{
    [Header("Bisiklet Temel Ayarlarý")]
    [SerializeField] private float _maxSpeed = 10f;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _deceleration = 2f;
    [SerializeField] private float _brakePower = 5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _cameraSensitivity = 100f;

    [Header("Handling Eðrisi Ayarlarý")]
    [SerializeField] private float _handlingThreshold = 3f;
    [SerializeField][Range(0.1f, 2f)] private float _minHandling = 0.5f;
    [SerializeField][Range(0.1f, 10f)] private float _maxHandling = 2;
    [SerializeField] private float _tiltAngleMultiplier = 30f;

    [Header("Referanslar")]
    [SerializeField] private Transform _dropOfPoint;
    [SerializeField] private GameObject _vehicleCamera;

    private Rigidbody _rb;
    private float _currentSpeed = 0f;
    private float _xRotation = 0f;
    private float _handling;
    private bool _playerOnboard = false;
    private GameObject _player;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.freezeRotation = true;
        _player = GameObject.FindGameObjectWithTag("Player");
        _vehicleCamera.SetActive(false);
    }

    public void Interact()
    {
        if (!_playerOnboard) EnterVehicle();
        else ExitVehicle();
    }

    private void Update()
    {
        if (_playerOnboard)
        {
            HandleVehicleMovement();
            HandleCameraLook();
            if (Input.GetKeyDown(KeyCode.E)) ExitVehicle();
        }
       
    }

    private void HandleVehicleMovement()
    {
        // Hýz kontrolü
        float moveZ = Input.GetAxis("Vertical");
        if (moveZ > 0)
        {
            _currentSpeed = Mathf.Min(_currentSpeed + _acceleration * Time.deltaTime, _maxSpeed);
        }
        else if (moveZ < 0)
        {
            _currentSpeed = Mathf.Max(_currentSpeed - _deceleration * Time.deltaTime, -_maxSpeed / 2);
        }
        else
        {
            if (_currentSpeed > 0) _currentSpeed = Mathf.Max(_currentSpeed - _deceleration * Time.deltaTime, 0);
            else if (_currentSpeed < 0) _currentSpeed = Mathf.Min(_currentSpeed + _deceleration * Time.deltaTime, 0);
        }

        // Handling hesaplama
        CalculateHandling();

        // Fizik uygulama
        Vector3 move = transform.forward * _currentSpeed;
        _rb.velocity = new Vector3(move.x, _rb.velocity.y, move.z);

        // Dönüþ uygula
        float turn = Input.GetAxis("Horizontal") * _handling;
        transform.Rotate(0, turn, 0);

        // Egim efekti
        float tiltAngle = Input.GetAxis("Horizontal") * _tiltAngleMultiplier * (_rb.velocity.magnitude / _maxSpeed);
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, -tiltAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Frenleme
        if (Input.GetKey(KeyCode.Space))
        {
            _currentSpeed = Mathf.Lerp(_currentSpeed, 0, _brakePower * Time.deltaTime);
        }

        ApplyGravity();
    }

    private void CalculateHandling()
    {
        float absSpeed = Mathf.Abs(_currentSpeed);

        // 1. Tam duruþ kontrolü
        if (absSpeed < 0.1f)
        {
            _handling = 0f;
            return;
        }

        // 2. Düþük hýz bölgesi (lineer artýþ)
        if (absSpeed <= _handlingThreshold)
        {
            _handling = Mathf.Lerp(_minHandling, _minHandling * 2f, absSpeed / _handlingThreshold);
            return;
        }

        // 3. Yüksek hýz bölgesi (parabolik eðri)
        float peakPoint = (_handlingThreshold + _maxSpeed) / 2f;
        float normalizedSpeed;

        if (absSpeed <= peakPoint)
        {
            // Threshold'dan tepe noktasýna çýkýþ
            normalizedSpeed = (absSpeed - _handlingThreshold) / (peakPoint - _handlingThreshold);
            _handling = Mathf.Lerp(_minHandling * 2f, _maxHandling, normalizedSpeed * normalizedSpeed);
        }
        else
        {
            // Tepe noktasýndan maxSpeed'e iniþ
            normalizedSpeed = (absSpeed - peakPoint) / (_maxSpeed - peakPoint);
            _handling = Mathf.Lerp(_maxHandling, _minHandling, normalizedSpeed * normalizedSpeed);
        }
    }

    private void HandleCameraLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _cameraSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _cameraSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        _vehicleCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
    }

    private void ApplyGravity()
    {
        if (!IsGrounded())
        {
            _rb.velocity += Vector3.up * _gravity * Time.deltaTime;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.2f);
    }

    private void EnterVehicle()
    {
        _playerOnboard = true;
        _player.transform.SetParent(_dropOfPoint);
        _player.transform.localPosition = Vector3.zero;
        _player.transform.localRotation = Quaternion.identity;
        _player.SetActive(false);

        _vehicleCamera.SetActive(true);
        _player.GetComponent<CharrController>().SetControlEnabled(false);
        _rb.isKinematic = false;
    }

    private void ExitVehicle()
    {
        _playerOnboard = false;
        _rb.isKinematic = true;

        _player.transform.SetParent(null);
        _player.transform.position = _dropOfPoint.position + transform.right * 2f;
        _player.transform.rotation = _dropOfPoint.rotation;
        _player.SetActive(true);

        _player.GetComponent<CharrController>().SetControlEnabled(true);
        _vehicleCamera.SetActive(false);

        StartCoroutine(DisableKinematicAfterDelay(0.5f));
    }

    private IEnumerator DisableKinematicAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _rb.isKinematic = false;
    }
}