using Demo.Scripts.Runtime;
using Framework.Network;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;
using MEC;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FPSController_Dummy : FPSAnimController
{
    [SerializeField] public NetworkObject networkObject;

    [Header("General")]
    [Tab("Animation")]
    [SerializeField] private Animator animator;

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
        InitLayers();
        EquipWeapon(0);
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

    private void Update()
    {
        UpdateAnimController();
    }

    #region Aiming

    private bool isAds;
    private Vector2 aim;
    private CoroutineHandle updateAimPoint;

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

    public void SetAimPoint( Vector2 newAim )
    {
        if (updateAimPoint.IsRunning)
        {
            Timing.KillCoroutines(updateAimPoint);
        }

        updateAimPoint = Timing.RunCoroutine(UpdateAimPoint(newAim));
    }

    private IEnumerator<float> UpdateAimPoint( Vector2 endAim )
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
}
