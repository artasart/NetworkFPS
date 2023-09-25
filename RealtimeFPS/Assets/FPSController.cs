using UnityEngine;
using Cinemachine;
using Framework.Network;

public class FPSController : MonoBehaviour
{
	public float moveSpeed = 5f;
	public float mouseSensitivity = 2f;
	//public GameObject bulletPrefab;
	//public Transform bulletSpawnPoint;
	public float fireRate = 0.1f;

	private float nextFireTime = 0f;
	private Transform cameraParent;
	private float verticalLookRotation = 0f;

	Transform ShootPos;

	// Cinemachine FreeLook 컴포넌트를 참조하기 위한 변수
	private CinemachineFreeLook freeLookCamera;

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		cameraParent = this.transform.Search("CameraParent");

		ShootPos = this.transform.Search(nameof(ShootPos));

		hitLayers = LayerMask.GetMask("Default");

		// Cinemachine FreeLook 컴포넌트 가져오기
		freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();

		GameClientManager.Instance.Client.packetHandler.AddHandler(S_ATTACKED);


		GameManager.UI.StackPanel<Panel_Network>();
	}

	void Update()
	{
		Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		verticalLookRotation -= mouseY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
		cameraParent.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
		transform.rotation *= Quaternion.Euler(Vector3.up * mouseX);

		if (Input.GetMouseButtonDown(0) && Time.time > nextFireTime)
		{
			Shoot();
			nextFireTime = Time.time + fireRate;
		}

		UpdateCameraInput();
	}

	public float shootDistance = 1000f;
	public LayerMask hitLayers;

	void Shoot()
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

		RaycastHit hitInfo;

		if (Physics.Raycast(ray, out hitInfo, shootDistance, hitLayers))
		{
			Protocol.C_SHOT enter = new()
			{
				Position = NetworkUtils.UnityVector3ToProtocolVector3(this.ShootPos.position),
				Direction = NetworkUtils.UnityVector3ToProtocolVector3((hitInfo.point - this.ShootPos.position).normalized),
			};

			GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(enter));

			DebugManager.Log("뭔가에 발사!! : " + ShootPos.position + " " + (hitInfo.point - ShootPos.position).normalized);
		}

		else
		{
			Protocol.C_SHOT enter = new()
			{
				Position = NetworkUtils.UnityVector3ToProtocolVector3(this.ShootPos.position),
				Direction = NetworkUtils.UnityVector3ToProtocolVector3((this.ShootPos.forward).normalized),
			};

			GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(enter));

			DebugManager.Log("하늘에 발사!! : " + ShootPos.position + " " + (ShootPos.position).normalized);
		}
	}

	void UpdateCameraInput()
	{
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		freeLookCamera.m_XAxis.Value += mouseX * freeLookCamera.m_XAxis.m_MaxSpeed;
		freeLookCamera.m_YAxis.Value += mouseY * freeLookCamera.m_YAxis.m_MaxSpeed;
	}

	private void S_ATTACKED(Protocol.S_ATTACKED packet)
	{
		Debug.Log(packet.Playerid + " is attacked..! : " + packet.Damage);
	}
}