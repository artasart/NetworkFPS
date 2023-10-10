using UnityEngine;
using Cinemachine;
using Framework.Network;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;

    [Header("Shooting")]
    public float fireRate = 0.1f;
    public float shootDistance = 1000f;
    public LayerMask hitLayers;

    [Header("Ammunition")]
    public int maxBulletCount = 30;

    private float nextFireTime = 0f;
    private Transform cameraParent;
    private float verticalLookRotation = 0f;
    private Transform ShootPos;
    private ParticleSystem particle_Muzzle;

    private int bulletCount;

    public CinemachineFreeLook freeLookCamera;

    bool isCameraLock = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cameraParent = transform.Search("CameraParent");
        ShootPos = transform.Search(nameof(ShootPos));
        hitLayers = LayerMask.GetMask("Default");
        freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();
        bulletCount = maxBulletCount;
        particle_Muzzle = FindObjectOfType<ParticleSystem>();

        GameClientManager.Instance.Client.packetHandler.AddHandler(S_ATTACKED);
    }

    private void Update()
    {
        if (isCameraLock) return;

        HandleMovementInput();
        HandleMouseInput();
        HandleShootingInput();
        HandleCameraInput();
        HandleReloadInput();
    }

    private void HandleMovementInput()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        cameraParent.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
        transform.rotation *= Quaternion.Euler(Vector3.up * mouseX);
    }

    private void HandleShootingInput()
    {
        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
        {
            if (bulletCount <= 0)
            {
                DebugManager.ClearLog("재장전이 필요합니다.");
                return;
            }

            Shoot();

            nextFireTime = Time.time + fireRate;
        }
    }

    private void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    private void Shoot()
    {
        Ray ray = new Ray(ShootPos.position, ShootPos.forward);
        RaycastHit hitInfo;

        Vector3 rayOrigin = ray.origin;
        Vector3 rayDirection = ray.direction;

        if (Physics.Raycast(ray, out hitInfo, shootDistance, hitLayers))
        {
            Protocol.C_SHOT enter = new Protocol.C_SHOT
            {
                Position = NetworkUtils.UnityVector3ToProtocolVector3(rayOrigin),
                Direction = NetworkUtils.UnityVector3ToProtocolVector3(rayDirection)
            };

            GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(enter));
        }

        else
        {
            Protocol.C_SHOT enter = new Protocol.C_SHOT
            {
                Position = NetworkUtils.UnityVector3ToProtocolVector3(ShootPos.position),
                Direction = NetworkUtils.UnityVector3ToProtocolVector3(ShootPos.forward.normalized)
            };

            GameClientManager.Instance.Client.Send(PacketManager.MakeSendBuffer(enter));
        }

        particle_Muzzle.Play();

        GameManager.UI.FetchPanel<Panel_HUD>().UpdateBulletCount(--bulletCount);
    }

    private void Reload()
    {
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

    private void S_ATTACKED(Protocol.S_ATTACKED packet)
    {
        Debug.Log(packet.Playerid + " is attacked..! : " + packet.Damage);

        if (packet.Playerid == GameClientManager.Instance.Client.GetPlayerId())
        {
            GameManager.UI.FetchPanel<Panel_HUD>().UpdateHealth(packet.Hp);

            Debug.Log("It was me.");

            if (packet.Hp <= 0)
            {
                Debug.Log("I am Dead..!");
            }
        }
    }

    public void LockCameraInput(bool _lock)
    {
        isCameraLock = _lock;
    }
}