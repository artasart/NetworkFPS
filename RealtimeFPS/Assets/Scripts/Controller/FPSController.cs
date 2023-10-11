using UnityEngine;
using Cinemachine;
using Framework.Network;

public class FPSController : MonoBehaviour
{
	[Header("Movement Settings")]
	public float moveSpeed = 5f;
	public float mouseSensitivity = 2f;

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

	private void Awake()
	{
		cameraParent = this.transform.Search("CameraParent");
		shootPos = this.transform.Search("ShootPos");
		freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();
		particleMuzzle = FindObjectOfType<ParticleSystem>();
		hitLayers = LayerMask.GetMask("Default");
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		bulletCount = maxBulletCount;

		//GameClientManager.Instance.Client.packetHandler.AddHandler(S_ATTACKED);
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
		Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
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

	//private void S_ATTACKED(Protocol.S_ATTACKED packet)
	//{
	//	Debug.Log(packet.Playerid + " is attacked..! : " + packet.Damage);
	//	Debug.Log("Mine : " + GameClientManager.Instance.Client.GetPlayerId());

	//	if (packet.Playerid == GameClientManager.Instance.Client.GetPlayerId())
	//	{
	//		Debug.Log("It was me.");

	//		GameManager.UI.FetchPanel<Panel_HUD>().UpdateHealth(packet.Hp);

	//		if (packet.Hp <= 0)
	//		{
	//			Debug.Log("I am Dead..!");
	//		}
	//	}
	//}

	public void LockCameraInput(bool _lock)
	{
		isCameraLocked = _lock;
	}
}