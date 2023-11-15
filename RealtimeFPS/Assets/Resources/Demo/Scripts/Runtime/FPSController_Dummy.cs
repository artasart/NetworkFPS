using Demo.Scripts.Runtime;
using FrameWork.Network;
using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.ParticleSystem;

public class FPSController_Dummy : FPSAnimController
{
    [SerializeField] public NetworkObject networkObject;

    [Header("General")]
    [Tab("Animation")]
    [SerializeField] private Animator animator;

    [SerializeField] private float turnInPlaceAngle;
    [SerializeField] private AnimationCurve turnCurve = new AnimationCurve(new Keyframe(0f, 0f));
    [SerializeField] private float turnSpeed = 1f;

    [Header("Dynamic Motions")]
    [SerializeField] private IKAnimation aimMotionAsset;
    [SerializeField] private IKAnimation leanMotionAsset;
    [SerializeField] private IKAnimation crouchMotionAsset;
    [SerializeField] private IKAnimation unCrouchMotionAsset;
    [SerializeField] private IKAnimation onJumpMotionAsset;
    [SerializeField] private IKAnimation onLandedMotionAsset;

    //// Animation Layers
    //[SerializeField][HideInInspector] private LookLayer lookLayer;
    [SerializeField][HideInInspector] private AdsLayer adsLayer;
    [SerializeField][HideInInspector] private SwayLayer swayLayer;
    //[SerializeField][HideInInspector] private LocomotionLayer locoLayer;
    [SerializeField][HideInInspector] private SlotLayer slotLayer;
    //[SerializeField][HideInInspector] private WeaponCollision collisionLayer;
    //// Animation Layers

    [Header("General")]
    [Tab("Controller")]
    [SerializeField] private float timeScale = 1f;
    [SerializeField] private float equipDelay = 0f;

    [Header("Movement")]
    [SerializeField] private FPSMovement_Dummy movementComponent;

    [SerializeField][Tab("Weapon")] private List<Weapon> weapons;
    public Transform weaponBone;

    private Vector2 _playerInput;

    // Used for free-look
    private Vector2 _freeLookInput;

    private int _index;
    private int _lastIndex;

    private float _fireTimer = -1f;
    private int _bursts;
    private bool _aiming;
    private bool _freeLook;
    private bool _hasActiveAction;

    private FPSActionState actionState;

    private float smoothCurveAlpha = 0f;

    private bool _equipTimerActive = false;
    private float _equipTimer = 0f;

    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int OverlayType = Animator.StringToHash("OverlayType");
    private static readonly int TurnRight = Animator.StringToHash("TurnRight");
    private static readonly int TurnLeft = Animator.StringToHash("TurnLeft");
    private static readonly int Equip = Animator.StringToHash("Equip");
    private static readonly int UnEquip = Animator.StringToHash("Unequip");

    private void Start()
    {
        InitLayers();
        EquipWeapon();

        movementComponent = GetComponent<FPSMovement_Dummy>();

        networkObject.Client.packetHandler.AddHandler(OnReload);
        networkObject.Client.packetHandler.AddHandler(OnFire);
        networkObject.Client.packetHandler.AddHandler(OnLook);
        networkObject.Client.packetHandler.AddHandler(OnLean);
        networkObject.Client.packetHandler.AddHandler(OnChangeWeapon);
        networkObject.Client.packetHandler.AddHandler(OnAim);
    }

    public void OnAim(Protocol.S_AIM pkt)
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        ToggleAim();
    }

    public void ToggleAim()
    {
        if (!GetGun().canAim) return;

        _aiming = !_aiming;

        if (_aiming)
        {
            actionState = FPSActionState.Aiming;
            adsLayer.SetAds(true);
            swayLayer.SetFreeAimEnable(false);
            swayLayer.SetLayerAlpha(0.3f);
            slotLayer.PlayMotion(aimMotionAsset);
            OnInputAim(_aiming);
        }
        else
        {
            DisableAim();
        }

        recoilComponent.isAiming = _aiming;
    }

    private void DisableAim()
    {
        if (!GetGun().canAim) return;

        _aiming = false;
        OnInputAim(_aiming);

        actionState = FPSActionState.None;
        adsLayer.SetAds(false);
        adsLayer.SetPointAim(false);
        swayLayer.SetFreeAimEnable(true);
        swayLayer.SetLayerAlpha(1f);
        slotLayer.PlayMotion(aimMotionAsset);
    }

    public void OnChangeWeapon(Protocol.S_CHANGE_WEAPON pkt)
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        ChangeWeapon_Internal();
    }

    private void ChangeWeapon_Internal()
    {
        if (movementComponent.PoseState == FPSPoseState.Prone
            || movementComponent.MovementState == FPSMovementState.Sprinting) return;

        OnFireReleased();

        int newIndex = _index;
        newIndex++;
        if (newIndex > weapons.Count - 1)
        {
            newIndex = 0;
        }

        _lastIndex = _index;
        _index = newIndex;

        StartWeaponChange();
    }

    private void StartWeaponChange()
    {
        animator.CrossFade(UnEquip, 0.1f);
        _equipTimerActive = true;
        _equipTimer = 0f;
    }

    float prevLean = 0f;

    public void OnLean( Protocol.S_LEAN pkt )
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        if(pkt.Value != prevLean)
            slotLayer.PlayMotion(leanMotionAsset);

        charAnimData.SetLeanInput(pkt.Value);
        prevLean = pkt.Value;
    }

    public void OnReload(Protocol.S_RELOAD pkt)
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        TryReload();
    }

    private void TryReload()
    {
        if (movementComponent.MovementState == FPSMovementState.Sprinting) return;

        var reloadClip = GetGun().reloadClip;

        if (reloadClip == null) return;

        OnFireReleased();
        //DisableAim();

        PlayAnimation(reloadClip);
        GetGun().Reload();
    }

    private void Update()
    {
        UpdateFiring();

        UpdateTimer();

        UpdateAnimController();
    }

    private void UpdateTimer()
    {
        if (!_equipTimerActive) return;

        if (_equipTimer > equipDelay)
        {
            EquipWeapon();

            _equipTimerActive = false;
            _equipTimer = 0f;
            return;
        }

        _equipTimer += Time.deltaTime;
    }

    private void UpdateFiring()
    {
        if (recoilComponent == null) return;

        if (recoilComponent.fireMode != FireMode.Semi && _fireTimer >= 60f / GetGun().fireRate)
        {
            Fire();

            if (recoilComponent.fireMode == FireMode.Burst)
            {
                _bursts--;

                if (_bursts == 0)
                {
                    _fireTimer = -1f;
                    OnFireReleased();
                }
                else
                {
                    _fireTimer = 0f;
                }
            }
            else
            {
                _fireTimer = 0f;
            }
        }

        if (_fireTimer >= 0f)
        {
            _fireTimer += Time.deltaTime;
        }
    }

    public void OnFire(Protocol.S_FIRE pkt)
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        if(pkt.IsFiring)
            OnFirePressed();
        else
            OnFireReleased();
    }

    private void OnFirePressed()
    {
        if (weapons.Count == 0) return;

        Fire();
        _bursts = GetGun().burstAmount - 1;
        _fireTimer = 0f;
    }

    private void OnFireReleased()
    {
        if (weapons.Count == 0) return;

        if (recoilComponent != null)
        {
            recoilComponent.Stop();
        }

        _fireTimer = -1f;
    }

    private void Fire()
    {
        GetGun().OnFire();
        PlayAnimation(GetGun().fireClip);

        if (recoilComponent != null)
        {
            recoilComponent.Play();
        }
    }

    private Weapon GetGun()
    {
        if (weapons.Count == 0) return null;

        return weapons[_index];
    }

    private float _jumpState = 0f;

    public void OnLook(Protocol.S_LOOK pkt)
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        _playerInput.x = pkt.X;
        _playerInput.y = pkt.Y;
        float deltaMouseX = pkt.DeltaX;

        float proneWeight = animator.GetFloat("ProneWeight");
        Vector2 pitchClamp = Vector2.Lerp(new Vector2(-90f, 90f), new Vector2(-30, 0f), proneWeight);

        _playerInput.y = Mathf.Clamp(_playerInput.y, pitchClamp.x, pitchClamp.y);
        moveRotation *= Quaternion.Euler(0f, deltaMouseX, 0f);
        TurnInPlace();

        _jumpState = Mathf.Lerp(_jumpState, movementComponent.IsInAir() ? 1f : 0f,
            FPSAnimLib.ExpDecayAlpha(10f, Time.deltaTime));

        float moveWeight = Mathf.Clamp01(movementComponent.AnimatorVelocity.magnitude);
        transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, moveWeight);
        transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, _jumpState);
        _playerInput.x *= 1f - moveWeight;
        _playerInput.x *= 1f - _jumpState;

        charAnimData.SetAimInput(_playerInput);
        charAnimData.AddDeltaInput(new Vector2(deltaMouseX, charAnimData.deltaAimInput.y));
    }

    private Quaternion desiredRotation;
    private Quaternion moveRotation;
    private float turnProgress = 1f;
    private bool isTurning = false;

    private void TurnInPlace()
    {
        float turnInput = _playerInput.x;
        _playerInput.x = Mathf.Clamp(_playerInput.x, -90f, 90f);
        turnInput -= _playerInput.x;

        float sign = Mathf.Sign(_playerInput.x);
        if (Mathf.Abs(_playerInput.x) > turnInPlaceAngle)
        {
            if (!isTurning)
            {
                turnProgress = 0f;

                animator.ResetTrigger(TurnRight);
                animator.ResetTrigger(TurnLeft);

                animator.SetTrigger(sign > 0f ? TurnRight : TurnLeft);
            }

            isTurning = true;
        }

        transform.rotation *= Quaternion.Euler(0f, turnInput, 0f);

        float lastProgress = turnCurve.Evaluate(turnProgress);
        turnProgress += Time.deltaTime * turnSpeed;
        turnProgress = Mathf.Min(turnProgress, 1f);

        float deltaProgress = turnCurve.Evaluate(turnProgress) - lastProgress;

        _playerInput.x -= sign * turnInPlaceAngle * deltaProgress;

        transform.rotation *= Quaternion.Slerp(Quaternion.identity,
            Quaternion.Euler(0f, sign * turnInPlaceAngle, 0f), deltaProgress);

        if (Mathf.Approximately(turnProgress, 1f) && isTurning)
        {
            isTurning = false;
        }
    }

    private void InitLayers()
    {
        InitAnimController();

        animator = GetComponentInChildren<Animator>();
        //lookLayer = GetComponentInChildren<LookLayer>();
        adsLayer = GetComponentInChildren<AdsLayer>();
        //locoLayer = GetComponentInChildren<LocomotionLayer>();
        swayLayer = GetComponentInChildren<SwayLayer>();
        slotLayer = GetComponentInChildren<SlotLayer>();
        //collisionLayer = GetComponentInChildren<WeaponCollision>();
    }

    public void EquipWeapon()
    {
        if (weapons.Count == 0) return;

        weapons[_lastIndex].gameObject.SetActive(false);
        var gun = weapons[_index];

        _bursts = gun.burstAmount;

        StopAnimation(0.1f);
        InitWeapon(gun);
        gun.gameObject.SetActive(true);

        animator.SetFloat(OverlayType, (float)gun.overlayType);
    }
}
