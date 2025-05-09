using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BicycleVehicle : MonoBehaviour, Iinterectable
{
	float horizontalInput;
	float vereticallInput;

	public Transform handle;
	bool braking;
	Rigidbody rb;

	public Vector3 COG;

	[SerializeField] float motorforce;
	[SerializeField] float brakeForce;
	float currentbrakeForce;

	float steeringAngle;
	[SerializeField] float currentSteeringAngle;
	[Range(0f, 0.1f)] [SerializeField] float speedteercontrolTime;
	[SerializeField] float maxSteeringAngle;
	[Range(0.000001f, 1)] [SerializeField] float turnSmoothing;

	[SerializeField]float maxlayingAngle = 45f;
	public float targetlayingAngle;
	[Range(-40, 40)]public float layingammount;
	[Range(0.000001f, 1 )] [SerializeField] float leanSmoothing;

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
    [SerializeField] private Transform _dropOfPoint; // Ýnme noktasý
    [SerializeField] private GameObject _vehicleCamera; // Araç kamerasý
    [SerializeField] private GameObject _playerCamera; // Oyuncu kamerasý
    [SerializeField] private GameObject _player; // Oyuncu transformu



    [Header("Coasting Settings")]
    [SerializeField] private float coastingDrag = 0.5f; // Yavaþlama sürtünmesi
    [SerializeField] private float normalDrag = 0.1f; // Normal sürtünme
    [SerializeField] private float minSpeedThreshold = 0.5f; // Tam durma eþiði
    [SerializeField] private float autoBrakeForce = 50f; // Otomatik fren kuvveti

    private GameObject Player;

    [SerializeField] private bool isPlayerOnBoard = false; // Player is on board or not
    public bool frontGrounded;
	public bool rearGrounded;
    [SerializeField] private MinimapPlayerIcon minimapIcon;

    // Start is called before the first frame update
    void Start()
	{
		StopEmitTrail();
		rb = GetComponent<Rigidbody>();		
	}


	void Update()
	{
        isPlayerWannaExitBicycle();
		//Debug.Log(rb.velocity.magnitude);
    }

	// Update is called once per frame
	void FixedUpdate()
	{
		if(!isPlayerOnBoard)
        {
            // Bisiklet boþtayken yavaþça durmasý için
            CoastToStop();
            UpdateWheels(); // Tekerleklerin görsel güncellemesi devam etmeli
            LayOnTurn(); // Dengeyi koru
            return;
        }
		else
		{
            GetInput();
            HandleEngine(); // hareketin gerçekleþtiði yer
            HandleSteering(); // gidon eðimi
            UpdateWheels(); // tekerleklerin rot ve pos'u
            UpdateHandle(); // gidonun konumu ön tekerleðe göre yapýlýr
            LayOnTurn(); // gidon eðimine göre eðilmeyi ayarlýyoruz
            DownPresureOnSpeed(); // hýz ve cisim aðýrlýðýna göre yere basma kuvveti uyguluyoruz
            EmitTrail();
			
        }	
	}

	public void GetInput()
	{
		horizontalInput = Input.GetAxis("Horizontal");
		vereticallInput = Input.GetAxis("Vertical");
		braking = Input.GetKey(KeyCode.Space);
	}

	public void HandleEngine()
	{
        if (Mathf.Abs(vereticallInput) > 0.1f)
        {
            // Input varsa normal hareket
            backWheel.motorTorque = vereticallInput * motorforce;
            rb.drag = normalDrag;
            ReleaseBrakibg();
        }
        else
        {
            // Input yoksa motoru kapat
            backWheel.motorTorque = 0f;

            // Yavaþlamayý fizik kurallarýna býrak (coasting)
            if (rb.velocity.magnitude > minSpeedThreshold)
            {
                rb.drag = coastingDrag;
                backWheel.brakeTorque = autoBrakeForce;
                frontWheel.brakeTorque = autoBrakeForce;
            }
            else
            {
                // Tamamen dur
                rb.velocity = Vector3.zero;
                backWheel.brakeTorque = brakeForce;
                frontWheel.brakeTorque = brakeForce;
            }
        }

        // Manuel fren kontrolü
        if (braking)
        {
            ApplyBraking();
        }
    }


    private void CoastToStop()
    {
        // Motor ve fren ayarlarý
        backWheel.motorTorque = 0f;
        frontWheel.motorTorque = 0f;

        // Yavaþlama fizikleri
        if (rb.velocity.magnitude > minSpeedThreshold)
        {
            rb.drag = coastingDrag;
            backWheel.brakeTorque = autoBrakeForce * 0.25f; // Boþtayken daha hafif fren
            frontWheel.brakeTorque = autoBrakeForce * 0.25f;
        }
        else
        {
            // Tamamen dur
            rb.velocity = Vector3.zero;
            backWheel.brakeTorque = brakeForce;
            frontWheel.brakeTorque = brakeForce;
        }

        // Havada asýlý kalmamasý için ek kontrol
        if (!frontWheel.isGrounded && !backWheel.isGrounded)
        {
            rb.drag = 0f;
        }
    }

    public void StopEngine()
	{

	}


    // "Eðer bisiklet 5 m/s hýzdan hýzlýysa, hýza ve cismin aðýrlýðýna  baðlý olarak yere daha çok basýlsýn."
    public void DownPresureOnSpeed()
	{
		Vector3 downforce = Vector3.down; // bu (0, -1, 0) vektörü aþaðýyý gösteriyor.
        float downpressure;
		if (rb.velocity.magnitude > 5)
		{
			downpressure = rb.velocity.magnitude;
			rb.AddForce(downforce * downpressure, ForceMode.Force); // f = m * a 'dan gelen kademeli ivmelenme veya baský uygulama
			
		}

	}

	public void ApplyBraking()
	{
		//frontWheel.brakeTorque = currentbrakeForce/2;
		frontWheel.brakeTorque = currentbrakeForce;
		backWheel.brakeTorque = currentbrakeForce;
	}
	public void ReleaseBrakibg()
	{
		frontWheel.brakeTorque = 0;
		backWheel.brakeTorque = 0;
	}

	
	// hýza göre alýnabilecek eðimi kýsýyoruz bisiklette
	public void SpeedSteerinReductor() 
	{
		if (rb.velocity.magnitude < 5 ) //We set the limiting factor for the steering thus allowing how much steer we give to the player in relation to the speed
		{			
			maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 50, speedteercontrolTime);
		}
		if (rb.velocity.magnitude > 5 && rb.velocity.magnitude < 10 )
		{			
			maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 30, speedteercontrolTime);
		}
		if (rb.velocity.magnitude > 10 && rb.velocity.magnitude < 15 )
		{			
			maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 15, speedteercontrolTime);
		}
		if (rb.velocity.magnitude > 15 && rb.velocity.magnitude < 20 )
		{			
			maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle,  10, speedteercontrolTime);
		}
		if (rb.velocity.magnitude > 20)
		{			
			maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle,  5, speedteercontrolTime);
		}			
	}


	// gidon eðimini ayarlýyoruz. 
	public void HandleSteering()
	{
		SpeedSteerinReductor();

		currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * horizontalInput, turnSmoothing);
		frontWheel.steerAngle = currentSteeringAngle;

		//We set the target laying angle to the + or - input value of our steering 
		//We invert our input for rotating in the ocrrect axis
		targetlayingAngle = maxlayingAngle * -horizontalInput;		
	}


	// bisikletin genel eðimini ayarlýyoruz bunu da 
	private void LayOnTurn()
	{
		Vector3 currentRot = transform.rotation.eulerAngles;


		// hýz 1'den küçükse bisiklet eðimi max seviyede ama smooth þekilde gerçekleþiyor. 
		if (rb.velocity.magnitude < 1)
		{
			layingammount = Mathf.LerpAngle(layingammount, 0f, 0.05f);		
			transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, layingammount);
			return;
		}

		// gidon eðimi fazla deðilse 
		if (currentSteeringAngle < 0.5f && currentSteeringAngle > -0.5  ) //We're stright
		{
			layingammount =  Mathf.LerpAngle(layingammount, 0f, leanSmoothing);			
		}
		else //We're turning yani gidon eðiminin 0.5f'ten fazla olduðu durumlar
		{
			layingammount = Mathf.LerpAngle(layingammount, targetlayingAngle, leanSmoothing );		
			rb.centerOfMass = new Vector3(rb.centerOfMass.x, COG.y, rb.centerOfMass.z);
		}

		transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, layingammount);
	}

	public void UpdateWheels()
	{
		UpdateSingleWheel(frontWheel, frontWheeltransform);
		UpdateSingleWheel(backWheel, backWheeltransform);
	}


	// gidonun konumunu ön tekerleðe göre alýyoruz. 
	public void UpdateHandle()
	{		
		Quaternion sethandleRot;
		sethandleRot = frontWheeltransform.rotation;		
		handle.localRotation = Quaternion.Euler(handle.localRotation.eulerAngles.x, currentSteeringAngle, handle.localRotation.eulerAngles.z);
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

		//fronttrail.emitting = true;
		//rearttrail.emitting = true;
	}
	private void StopEmitTrail() 
	{
		fronttrail.emitting = false;
		rearttrail.emitting = false;
	}

    private void changeCamera()
    {
        if (isPlayerOnBoard)
        {
            _vehicleCamera.SetActive(true);
            _playerCamera.SetActive(false);
            playerStatue();

            // Minimap bisikleti takip etsin
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

    private void isPlayerWannaExitBicycle()
	{
        if (isPlayerOnBoard && Input.GetKeyDown(KeyCode.E))
        {
           isPlayerOnBoard = !isPlayerOnBoard; // Toggle the player's presence on the bicycle
            changeCamera();
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
            // Player'ý bisikletten ayýr ve rotasyonunu sýfýrla
            _player.transform.SetParent(null);
            _player.transform.position = _dropOfPoint.position;

            // Player'ýn rotasyonunu sýfýrla (eðimli kalmamasý için)
            _player.transform.rotation = Quaternion.identity;

            _player.SetActive(true);

            // Eðer player'ýn kendi kamerasý varsa, onun rotasyonunu da sýfýrla
            if (_playerCamera != null)
            {
                _playerCamera.transform.localRotation = Quaternion.identity;
            }
        }
	}

    // harekete göre bisikletin rotasyon ve pozisyonunu ayarlýyoruz
    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
	{
		Vector3 pos;
		Quaternion rot;
		wheelCollider.GetWorldPose(out pos, out rot);
		wheelTransform.rotation = rot;
		wheelTransform.position = pos;
	}

    public void Interact()
    {
        isPlayerOnBoard = !isPlayerOnBoard; // Toggle the player's presence on the bicycle
		changeCamera();
    }

    public string GetInteractionText()
    {

		if (isPlayerOnBoard)
		{
        return "Press E to ride the bicycle";
		}
		else
		{
			return "Press E to get off the bicycle";
		}
    }

    public bool CanInteract()
    {
		return true;
    }
}
