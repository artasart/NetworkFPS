using Demo.Scripts.Runtime;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FPSController_Dummy : FPSAnimController
{
    [Header("General")] [Tab("Animation")]
    [SerializeField] private Animator animator;

    // Animation Layers
    [SerializeField][HideInInspector] private LookLayer lookLayer;
    [SerializeField][HideInInspector] private AdsLayer adsLayer;
    [SerializeField][HideInInspector] private SwayLayer swayLayer;
    [SerializeField][HideInInspector] private LocomotionLayer locoLayer;
    [SerializeField][HideInInspector] private SlotLayer slotLayer;
    [SerializeField][HideInInspector] private WeaponCollision collisionLayer;
    // Animation Layers

    [SerializeField][Tab("Weapon")] private List<Weapon> weapons;

    private int _index;
    private int _lastIndex;

    private int _bursts;

    private static readonly int OverlayType = Animator.StringToHash("OverlayType");

    private void Start()
    {
        InitLayers();
        EquipWeapon();
    }

    private void Update()
    {
        UpdateAnimController();
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
