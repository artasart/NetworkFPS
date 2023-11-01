using UnityEngine;
using Cinemachine;
using Framework.Network;

public class FPSController : MonoBehaviour
{
	[Header("Movement Settings")]
    public float walkSpeed = 5.0f;
    public float dashSpeed = 10.0f;
    public float jumpHeight = 2.0f;
    public float gravityValue = -9.81f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private float playerSpeed;
    private bool isDashing;

    public LayerMask groundLayer;
    public float groundCheckDistance = 0.16f;

    [Header("Shooting Settings")]
	public float fireRate = 0.1f;
	public float shootDistance = 1000f;
	public LayerMask hitLayers;

	[Header("Ammo Settings")]
	public int maxBulletCount = 30;

	[Header("References")]
	public Transform cameraParent;
	public Transform shootPos;
	public CinemachineFreeLook freeLookCamera;
	public ParticleSystem particleMuzzle;

	private int bulletCount;
	private float verticalLookRotation = 0f;
	private float nextFireTime = 0f;
	private bool isCameraLocked = false;

    [Range(0, 1)] public float airControlPercent;

    private void Awake()
	{
		cameraParent = this.transform.Search("CameraParent");
		shootPos = this.transform.Search("ShootPos");
		freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();
		particleMuzzle = FindObjectOfType<ParticleSystem>();
		hitLayers = LayerMask.GetMask("Default");

        controller = GetComponent<CharacterController>();
    }

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		bulletCount = maxBulletCount;
	}

	private void Update()
	{
		if (isCameraLocked) return;

        HandleMovement();
		HandleMouseLook();
		HandleShooting();
		HandleCameraInput();
		HandleReload();
	}

    private void HandleMovement()
	{
        isGrounded = CheckIfGrounded();
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = transform.TransformDirection(move);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
			print("JUMP!!");
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
		else if(Input.GetButtonDown("Jump") && !isGrounded)
		{
			print("Jump pressed, but not grounded");
		}

        if (Input.GetKeyDown(KeyCode.LeftShift))
			isDashing = true;
        else if (Input.GetKeyUp(KeyCode.LeftShift))
			isDashing = false;

        playerSpeed = isDashing ? dashSpeed : walkSpeed;
        controller.Move(move * Time.deltaTime * playerSpeed);


        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private bool CheckIfGrounded()
    {
        // 캐릭터의 바닥 중심점에서 아래로 Ray를 쏴서 지면에 닿는지 확인합니다.
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, groundCheckDistance, groundLayer))
        {
            return true;
        }
        return false;
    }

    private void HandleMouseLook()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		verticalLookRotation -= mouseY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
		cameraParent.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
		transform.rotation *= Quaternion.Euler(Vector3.up * mouseX);
	}

	private void HandleShooting()
	{
		if (Input.GetMouseButton(0) && Time.time > nextFireTime)
		{
			if (bulletCount <= 0)
			{
				DebugManager.ClearLog("재장전이 필요합니다.");
				return;
			}

			Fire();
			nextFireTime = Time.time + fireRate;
		}
	}

	private void HandleReload()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Reload();
		}
	}

	private void Fire()
	{
		Debug.Log("Shoot");

		Vector3 rayOrigin = shootPos.position;
		Vector3 rayDirection = (Physics.Raycast(shootPos.position, shootPos.forward, out RaycastHit hitInfo, 1000, hitLayers))
			? (hitInfo.point - rayOrigin).normalized
			: shootPos.forward.normalized;

		Protocol.C_SHOT enter = new Protocol.C_SHOT
		{
			Position = NetworkUtils.UnityVector3ToProtocolVector3(rayOrigin),
			Direction = NetworkUtils.UnityVector3ToProtocolVector3(rayDirection)
		};

		GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(enter));

		particleMuzzle.Play();

		GameManager.UI.FetchPanel<Panel_HUD>().UpdateBulletCount(--bulletCount);
	}

	private void Reload()
	{
		DebugManager.ClearLog("Reload");
		bulletCount = maxBulletCount;
		GameManager.UI.FetchPanel<Panel_HUD>().UpdateBulletCount(maxBulletCount);
	}

	private void HandleCameraInput()
	{
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		freeLookCamera.m_XAxis.Value += mouseX * freeLookCamera.m_XAxis.m_MaxSpeed;
		freeLookCamera.m_YAxis.Value += mouseY * freeLookCamera.m_YAxis.m_MaxSpeed;
	}

	public void LockCameraInput(bool _lock)
	{
		isCameraLocked = _lock;
	}
}