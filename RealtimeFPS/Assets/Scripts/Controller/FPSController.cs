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

	// Cinemachine FreeLook ������Ʈ�� �����ϱ� ���� ����
	public CinemachineFreeLook freeLookCamera;


	ParticleSystem particle_Muzzle;
	
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		cameraParent = this.transform.Search("CameraParent");

		ShootPos = this.transform.Search(nameof(ShootPos));

		hitLayers = LayerMask.GetMask("Default");

		freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();


		bulletCount = maxBulletCount;


		particle_Muzzle = FindObjectOfType<ParticleSystem>();


		GameClientManager.Instance.Client.packetHandler.AddHandler(S_ATTACKED);
	}

	void Update()
	{
		if (isCameraLock) return;

		Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
		transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		verticalLookRotation -= mouseY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
		cameraParent.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
		transform.rotation *= Quaternion.Euler(Vector3.up * mouseX);

		// ���� ������ ��쿡�� �߻�
		if (Input.GetMouseButton(0) && Time.time > nextFireTime)
		{
			if (bulletCount <= 0)
			{
				DebugManager.ClearLog("�������� �ʿ��մϴ�.");
				return;
			}

			Shoot();
			nextFireTime = Time.time + fireRate;
		}

		UpdateCameraInput();

		if (Input.GetKeyDown(KeyCode.R))
		{
			Reload();
		}
	}






	public float shootDistance = 1000f;
	public LayerMask hitLayers;

	public int bulletCount = 30;
	public int maxBulletCount = 30;

	private void Reload()
	{
		DebugManager.ClearLog("Reload");

		bulletCount = maxBulletCount;

		GameManager.UI.FetchPanel<Panel_HUD>().UpdateBulletCount(maxBulletCount);
	}

	void Shoot()
	{
		Debug.Log("Shooot");

		Ray ray = new Ray(ShootPos.position, ShootPos.forward); // �� ���������� ������Ʈ�� ��ġ�� ���� ������ ����մϴ�.

		RaycastHit hitInfo;

		if (Physics.Raycast(ray, out hitInfo, 1000, hitLayers))
		{
			// ��Ÿ ó��
			Vector3 rayOrigin = ShootPos.position; // ��� ��ġ
			Vector3 rayDirection = (hitInfo.point - rayOrigin).normalized; // ���� ����

			Protocol.C_SHOT enter = new()
			{
				Position = NetworkUtils.UnityVector3ToProtocolVector3(rayOrigin),
				Direction = NetworkUtils.UnityVector3ToProtocolVector3(rayDirection),
			};

			GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(enter));

			//DebugManager.Log("������ �߻�!! : " + rayOrigin + " " + rayDirection);
		}

		else
		{
			// ��Ÿ ó��

			Protocol.C_SHOT enter = new()
			{
				Position = NetworkUtils.UnityVector3ToProtocolVector3(ShootPos.position),
				Direction = NetworkUtils.UnityVector3ToProtocolVector3(ShootPos.forward.normalized),
			};

			GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(enter));

			//DebugManager.Log("�ϴÿ� �߻�!! : " + ShootPos.position + " " + ShootPos.forward.normalized);
		}

		particle_Muzzle.Play();

		GameManager.UI.FetchPanel<Panel_HUD>().UpdateBulletCount(bulletCount);
	}

	private void OnDrawGizmos()
	{
		if (ShootPos == null)
            return;

		// ����� ���� ����
		Gizmos.color = Color.red;

		// ���� ĳ��Ʈ�� �����ϴ� ��ġ
		Vector3 rayOrigin = ShootPos.position;

		// ���� ĳ��Ʈ�� ����
		Vector3 rayDirection = ShootPos.forward;

		// ���� ĳ��Ʈ�� �ð������� ǥ��
		Gizmos.DrawRay(rayOrigin, rayDirection * 1000f);
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

		Debug.Log("Mine : " + GameClientManager.Instance.Client.GetPlayerId());

		if (packet.Playerid == GameClientManager.Instance.Client.GetPlayerId())
		{
			Debug.Log("It was me.");

			GameManager.UI.FetchPanel<Panel_HUD>().UpdateHealth(packet.Hp);

			if (packet.Hp <= 0)
			{
				Debug.Log("I am Dead..!");
			}
		}
	}
	  

	bool isCameraLock = false;

	public void LockCameraInput(bool _lock)
	{
		isCameraLock = _lock;
	}
}