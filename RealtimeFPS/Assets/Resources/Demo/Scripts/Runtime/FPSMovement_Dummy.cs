using Framework.Network;
using Demo.Scripts.Runtime;
using UnityEngine;
using UnityEngine.Events;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEngine.InputSystem.XR;

public class FPSMovement_Dummy : MonoBehaviour
{
    [SerializeField] public NetworkObject networkObject;

    [SerializeField] private FPSMovementSettings movementSettings;

    #region Events

    [SerializeField] public UnityEvent onSprintStarted;
    [SerializeField] public UnityEvent onSprintEnded;

    [SerializeField] public UnityEvent onCrouch;
    [SerializeField] public UnityEvent onUncrouch;

    [SerializeField] public UnityEvent onProneStarted;
    [SerializeField] public UnityEvent onProneEnded;

    [SerializeField] public UnityEvent onJump;
    [SerializeField] public UnityEvent onLanded;

    [SerializeField] public UnityEvent onSlideStarted;
    [SerializeField] public UnityEvent onSlideEnded;

    #endregion

    public FPSMovementState MovementState { get; private set; }
    public FPSPoseState PoseState { get; private set; }

    private CharacterController _controller;
    private Animator _animator;

    private float _originalHeight;
    private Vector3 _originalCenter;

    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int Sliding = Animator.StringToHash("Sliding");
    private static readonly int Proning = Animator.StringToHash("Proning");

    private void Start()
    {
        MovementState = FPSMovementState.Idle;
        PoseState = FPSPoseState.Standing;

        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<CharacterController>();

        _originalHeight = _controller.height;
        _originalCenter = _controller.center;
    }

    #region Movement

    public void SetMovement( FPSMovementState newMovementState )
    {
        if (MovementState == newMovementState)
            return;

        if (MovementState == FPSMovementState.InAir)
        {
            onLanded.Invoke();
        }
        else if (MovementState == FPSMovementState.Sprinting)
        {
            onSprintEnded.Invoke();
        }
        else if (MovementState == FPSMovementState.Sliding)
        {
            onSlideEnded.Invoke();
            UnCrouch();
        }

        if (newMovementState == FPSMovementState.InAir)
        {
            onJump.Invoke();
        }
        else if (newMovementState == FPSMovementState.Sprinting)
        {
            onSprintStarted.Invoke();
        }
        else if (newMovementState == FPSMovementState.Sliding)
        {
            _animator.CrossFade(Sliding, 0.1f);
            onSlideStarted.Invoke();
            Crouch();
        }

        MovementState = newMovementState;
    }

    #endregion

    #region Pose

    public void SetPose( FPSPoseState newPoseState )
    {
        if (newPoseState == PoseState)
            return;

        if (newPoseState == FPSPoseState.Prone)
        {
            EnableProne();
        }
        else if (newPoseState == FPSPoseState.Crouching)
        {
            Crouch();
        }
        else if (newPoseState == FPSPoseState.Standing)
        {
            if (PoseState == FPSPoseState.Prone)
            {
                CancelProne();
            }
            else if (PoseState == FPSPoseState.Crouching)
            {
                UnCrouch();
            }
        }
    }

    private void EnableProne()
    {
        Crouch();
        PoseState = FPSPoseState.Prone;
        _animator.SetBool(Crouching, false);
        _animator.SetBool(Proning, true);

        onProneStarted?.Invoke();
    }

    private void CancelProne()
    {
        UnCrouch();
        PoseState = FPSPoseState.Standing;
        _animator.SetBool(Proning, false);

        onProneEnded?.Invoke();
    }

    private void Crouch()
    {
        float crouchedHeight = _originalHeight * movementSettings.crouchRatio;
        float heightDifference = _originalHeight - crouchedHeight;

        _controller.height = crouchedHeight;

        Vector3 crouchedCenter = _originalCenter;
        crouchedCenter.y -= heightDifference / 2;
        _controller.center = crouchedCenter;

        PoseState = FPSPoseState.Crouching;

        _animator.SetBool(Crouching, true);
        onCrouch.Invoke();
    }

    private void UnCrouch()
    {
        _controller.height = _originalHeight;
        _controller.center = _originalCenter;

        PoseState = FPSPoseState.Standing;

        _animator.SetBool(Crouching, false);
        onUncrouch.Invoke();
    }

    #endregion
}
