using Demo.Scripts.Runtime;
using UnityEngine;
using UnityEngine.Events;
using MEC;
using System.Collections.Generic;

public class FPSMovement_Remote : MonoBehaviour
{
    [SerializeField] private FPSMovementSettings movementSettings;

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

    public FPSMovementState MovementState { get; private set; }
    public FPSPoseState PoseState { get; private set; }

    private CharacterController controller;
    private Animator animator;

    private float originalHeight;
    private Vector3 originalCenter;

    private static readonly int InAir = Animator.StringToHash("InAir");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int Sliding = Animator.StringToHash("Sliding");
    private static readonly int Sprinting = Animator.StringToHash("Sprinting");
    private static readonly int Proning = Animator.StringToHash("Proning");

    private void Start()
    {
        MovementState = FPSMovementState.Idle;
        PoseState = FPSPoseState.Standing;

        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();

        originalHeight = controller.height;
        originalCenter = controller.center;
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
            animator.CrossFade(Sliding, 0.1f);
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
        animator.SetBool(Crouching, false);
        animator.SetBool(Proning, true);

        onProneStarted?.Invoke();
    }

    private void CancelProne()
    {
        UnCrouch();
        PoseState = FPSPoseState.Standing;
        animator.SetBool(Proning, false);

        onProneEnded?.Invoke();
    }

    private void Crouch()
    {
        float crouchedHeight = originalHeight * movementSettings.crouchRatio;
        float heightDifference = originalHeight - crouchedHeight;

        controller.height = crouchedHeight;

        Vector3 crouchedCenter = originalCenter;
        crouchedCenter.y -= heightDifference / 2;
        controller.center = crouchedCenter;

        PoseState = FPSPoseState.Crouching;

        animator.SetBool(Crouching, true);
        onCrouch.Invoke();
    }

    private void UnCrouch()
    {
        controller.height = originalHeight;
        controller.center = originalCenter;

        PoseState = FPSPoseState.Standing;

        animator.SetBool(Crouching, false);
        onUncrouch.Invoke();
    }

    #endregion

    public IEnumerator<float> UpdateAnimation( Protocol.FPSAnimation pkt, float interval )
    {
        float delTime = 0.0f;

        animator.SetBool(Moving, pkt.Moving);
        animator.SetBool(InAir, pkt.InAir);

        float prevMoveX = animator.GetFloat(MoveX);
        float prevMoveY = animator.GetFloat(MoveY);
        float prevVelocity = animator.GetFloat(Velocity);
        float prevSprinting = animator.GetFloat(Sprinting);

        while (delTime < interval)
        {
            delTime += Time.deltaTime;

            animator.SetFloat(MoveX, Mathf.Lerp(prevMoveX, pkt.MoveX, delTime / interval));
            animator.SetFloat(MoveY, Mathf.Lerp(prevMoveY, pkt.MoveY, delTime / interval));
            animator.SetFloat(Velocity, Mathf.Lerp(prevVelocity, pkt.Velocity, delTime / interval));
            animator.SetFloat(Sprinting, Mathf.Lerp(prevSprinting, pkt.Sprinting, delTime / interval));

            yield return Timing.WaitForOneFrame;
        }
    }
}
