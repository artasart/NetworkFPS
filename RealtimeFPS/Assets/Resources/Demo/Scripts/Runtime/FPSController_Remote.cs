using Demo.Scripts.Runtime;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class FPSController_Remote : FPSAnimController
{
    [Header("General")]
    [SerializeField][Tab("Animation")] private Animator animator;

    [Header("Dynamic Motions")]
    [SerializeField] private IKAnimation aimMotionAsset;
    [SerializeField] private IKAnimation leanMotionAsset;
    [SerializeField] private IKAnimation crouchMotionAsset;
    [SerializeField] private IKAnimation unCrouchMotionAsset;
    [SerializeField] private IKAnimation onJumpMotionAsset;
    [SerializeField] private IKAnimation onLandedMotionAsset;

    [Header("Movement")]
    [SerializeField] private FPSMovement_Remote movementComponent;

    [SerializeField][HideInInspector] private LookLayer lookLayer;
    [SerializeField][HideInInspector] private AdsLayer adsLayer;
    [SerializeField][HideInInspector] private SwayLayer swayLayer;
    [SerializeField][HideInInspector] private LocomotionLayer locoLayer;
    [SerializeField][HideInInspector] private SlotLayer slotLayer;
    [SerializeField][HideInInspector] private WeaponCollision collisionLayer;

    [SerializeField][Tab("Weapon")] private List<Weapon> weapons;
    public Transform weaponBone;

    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int OverlayType = Animator.StringToHash("OverlayType");
    private static readonly int TurnRight = Animator.StringToHash("TurnRight");
    private static readonly int TurnLeft = Animator.StringToHash("TurnLeft");
    private static readonly int Equip = Animator.StringToHash("Equip");
    private static readonly int UnEquip = Animator.StringToHash("Unequip");

    private void Start()
    {
        movementComponent = GetComponent<FPSMovement_Remote>();

        movementComponent.onCrouch.AddListener(OnCrouch);
        movementComponent.onUncrouch.AddListener(OnUncrouch);

        movementComponent.onJump.AddListener(OnJump);
        movementComponent.onLanded.AddListener(OnLand);

        movementComponent.onSprintStarted.AddListener(OnSprintStarted);
        movementComponent.onSprintEnded.AddListener(OnSprintEnded);

        movementComponent.onSlideStarted.AddListener(OnSlideStarted);
        movementComponent.onSlideEnded.AddListener(OnSlideEnded);

        movementComponent.onProneStarted.AddListener(() => collisionLayer.SetLayerAlpha(0f));
        movementComponent.onProneEnded.AddListener(() => collisionLayer.SetLayerAlpha(1f));

        InitLayers();
        EquipWeapon(0);
    }

    private void InitLayers()
    {
        InitAnimController();

        animator = GetComponentInChildren<Animator>();
        
        lookLayer = GetComponentInChildren<LookLayer>();
        adsLayer = GetComponentInChildren<AdsLayer>();
        locoLayer = GetComponentInChildren<LocomotionLayer>();
        swayLayer = GetComponentInChildren<SwayLayer>();
        slotLayer = GetComponentInChildren<SlotLayer>();
        collisionLayer = GetComponentInChildren<WeaponCollision>();
    }

    private void Update()
    {
        UpdateAnimController();
    }

    #region Aiming

    private bool isAds;
    private Vector2 aim;
    private CoroutineHandle updateAim;

    public void SetAds( bool _isAds )
    {
        if (isAds == _isAds)
            return;

        isAds = _isAds;

        if (isAds)
        {
            adsLayer.SetAds(true);

            swayLayer.SetFreeAimEnable(false);
            swayLayer.SetLayerAlpha(0.3f);

            slotLayer.PlayMotion(aimMotionAsset);
        }
        else if (!isAds)
        {
            DisableAim();
        }

        recoilComponent.isAiming = isAds;
    }

    private void DisableAim()
    {
        adsLayer.SetAds(false);
        adsLayer.SetPointAim(false);

        swayLayer.SetFreeAimEnable(true);
        swayLayer.SetLayerAlpha(1f);

        slotLayer.PlayMotion(aimMotionAsset);
    }

    public void SetAim( Vector2 newAim )
    {
        if (updateAim.IsRunning)
        {
            Timing.KillCoroutines(updateAim);
        }

        updateAim = Timing.RunCoroutine(UpdateAim(newAim));
    }

    private IEnumerator<float> UpdateAim( Vector2 endAim )
    {
        float interval = 0.05f;
        float delTime = 0.0f;

        Vector2 startAim = aim;

        while (delTime < interval)
        {
            delTime += Time.deltaTime;

            Vector2 currentAim = Vector2.Lerp(startAim, endAim, delTime / interval);

            float deltaAim = aim.x - currentAim.x;

            aim = currentAim;
            charAnimData.SetAimInput(aim);
            charAnimData.AddDeltaInput(new Vector2(deltaAim, charAnimData.deltaAimInput.y));

            yield return Timing.WaitForOneFrame;
        }

        aim = endAim;
    }

    public void Turn(bool isRight)
    {
        animator.ResetTrigger(TurnRight);
        animator.ResetTrigger(TurnLeft);

        if (isRight)
        {
            animator.SetTrigger(TurnRight);
        }
        else
        {
            animator.SetTrigger(TurnLeft);
        }
    }

    #endregion

    private Weapon GetGun()
    {
        if (weapons.Count == 0) return null;

        return weapons[weaponIndex];
    }

    #region Weapon

    private int weaponIndex;
    public readonly float equipDelay = 0.9f;
    
    public IEnumerator<float> ChangeWeapon(int newWeaponIndex, float waitTime)
    {
        DisableAim();
        animator.CrossFade(UnEquip, 0.1f);

        yield return Timing.WaitForSeconds(waitTime);

        EquipWeapon(newWeaponIndex);
    }

    private void EquipWeapon( int newWeaponIndex )
    {
        weapons[weaponIndex].gameObject.SetActive(false);

        weaponIndex = newWeaponIndex;

        var gun = weapons[weaponIndex];

        StopAnimation(0.1f);
        InitWeapon(gun);
        gun.gameObject.SetActive(true);

        animator.SetFloat(OverlayType, (float)gun.overlayType);
    }

    #endregion

    #region Reload

    public void Reload()
    {
        var reloadClip = GetGun().reloadClip;

        if (reloadClip == null) 
            return;

        PlayAnimation(reloadClip);
        GetGun().Reload();
    }

    #endregion

    public void Fire()
    {
        GetGun().OnFire();
        PlayAnimation(GetGun().fireClip);

        if (recoilComponent != null)
        {
            recoilComponent.Play();
        }
    }

    #region Animation Event

    private void OnSlideStarted()
    {
        lookLayer.SetLayerAlpha(0.3f);
    }

    private void OnSlideEnded()
    {
        lookLayer.SetLayerAlpha(1f);
    }

    private void OnSprintStarted()
    {
        lookLayer.SetLayerAlpha(0.5f);
        adsLayer.SetLayerAlpha(0f);
        locoLayer.SetReadyWeight(0f);

        if (recoilComponent != null)
        {
            recoilComponent.Stop();
        }
    }

    private void OnSprintEnded()
    {
        lookLayer.SetLayerAlpha(1f);
        adsLayer.SetLayerAlpha(1f);
    }

    private void OnCrouch()
    {
        lookLayer.SetPelvisWeight(0f);
        animator.SetBool(Crouching, true);
        slotLayer.PlayMotion(crouchMotionAsset);
    }

    private void OnUncrouch()
    {
        lookLayer.SetPelvisWeight(1f);
        animator.SetBool(Crouching, false);
        slotLayer.PlayMotion(unCrouchMotionAsset);
    }

    private void OnJump()
    {
        slotLayer.PlayMotion(onJumpMotionAsset);
    }

    private void OnLand()
    {
        slotLayer.PlayMotion(onLandedMotionAsset);
    }

    #endregion
}
