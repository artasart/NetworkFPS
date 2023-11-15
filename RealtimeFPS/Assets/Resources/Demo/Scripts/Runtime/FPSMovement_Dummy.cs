using FrameWork.Network;
using Demo.Scripts.Runtime;
using UnityEngine;
using UnityEngine.Events;
using Kinemation.FPSFramework.Runtime.FPSAnimator;

public class FPSMovement_Dummy : MonoBehaviour
{
    [SerializeField] public NetworkObject networkObject;

    [SerializeField] private FPSMovementSettings movementSettings;
    [SerializeField] public Transform rootBone;

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

    public Vector2 AnimatorVelocity { get; private set; }

    private CharacterController _controller;
    private Animator _animator;
    private Vector2 _inputDirection;

    public Vector3 MoveVector { get; private set; }

    private Vector3 _velocity;

    private float _originalHeight;
    private Vector3 _originalCenter;

    private GaitSettings _desiredGait;
    private float _slideProgress = 0f;

    private static readonly int InAir = Animator.StringToHash("InAir");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int Sliding = Animator.StringToHash("Sliding");
    private static readonly int Sprinting = Animator.StringToHash("Sprinting");
    private static readonly int Proning = Animator.StringToHash("Proning");

    private float _sprintAnimatorInterp = 8f;

    public bool IsInAir()
    {
        return MovementState == FPSMovementState.InAir;
    }

    private void Start()
    {
        MovementState = FPSMovementState.Idle;
        PoseState = FPSPoseState.Standing;

        _animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<CharacterController>();

        _originalHeight = _controller.height;
        _originalCenter = _controller.center;

        networkObject.Client.packetHandler.AddHandler(OnSetAnimation);
    }

    public void OnSetAnimation( Protocol.S_SET_ANIMATION pkt )
    {
        if (pkt.GameObjectId != networkObject.id)
            return;

        var prevState = MovementState;

        MovementState = (FPSMovementState)pkt.Params[(int)AnimatorParamId.MovementState].IntParam;
        
        UpdatePostState(pkt);

        if (prevState != MovementState)
        {
            OnMovementStateChanged(prevState);
        }

        if (MovementState == FPSMovementState.InAir)
        {
            UpdateInAir();
        }
        else if (MovementState == FPSMovementState.Sliding)
        {
            UpdateSliding();
        }
        else
        {
            UpdateGrounded();
        }

        _animator.SetFloat(MoveX, pkt.Params[(int)AnimatorParamId.MoveX].FloatParam);
        _animator.SetFloat(MoveY, pkt.Params[(int)AnimatorParamId.MoveY].FloatParam);
        _animator.SetFloat(Velocity, pkt.Params[(int)AnimatorParamId.Velocity].FloatParam);
        _animator.SetBool(InAir, pkt.Params[(int)AnimatorParamId.InAir].BoolParam);
        _animator.SetBool(Moving, pkt.Params[(int)AnimatorParamId.Moving].BoolParam);
        _animator.SetFloat(Sprinting, pkt.Params[(int)AnimatorParamId.Sprinting].FloatParam);
    }

    private void UpdatePostState( Protocol.S_SET_ANIMATION pkt )
    {
        if (MovementState is FPSMovementState.Sprinting or FPSMovementState.InAir)
        {
            return;
        }

        if (pkt.Params[(int)AnimatorParamId.Proning].BoolParam)
        {
            if (PoseState == FPSPoseState.Prone)
            {
                CancelProne();
            }
            else
            {
                EnableProne();
            }

            return;
        }

        if (!pkt.Params[(int)AnimatorParamId.Crouching].BoolParam)
        {
            return;
        }

        if (PoseState == FPSPoseState.Standing)
        {
            Crouch();

            _desiredGait = movementSettings.crouching;
            return;
        }

        if (!CanUnCrouch()) return;

        UnCrouch();
        _desiredGait = movementSettings.walking;
    }

    private void OnMovementStateChanged( FPSMovementState prevState )
    {
        if (prevState == FPSMovementState.InAir)
        {
            onLanded.Invoke();
        }

        if (prevState == FPSMovementState.Sprinting)
        {
            _sprintAnimatorInterp = 7f;
            onSprintEnded.Invoke();
        }

        if (prevState == FPSMovementState.Sliding)
        {
            _sprintAnimatorInterp = 15f;
            onSlideEnded.Invoke();

            if (CanUnCrouch())
            {
                UnCrouch();
            }
        }

        if (MovementState == FPSMovementState.Idle)
        {
            float prevVelocity = _desiredGait.velocity;
            _desiredGait = movementSettings.idle;
            _desiredGait.velocity = prevVelocity;
            return;
        }

        if (MovementState == FPSMovementState.InAir)
        {
            _velocity.y = movementSettings.jumpHeight;
            onJump.Invoke();
            return;
        }

        if (MovementState == FPSMovementState.Sprinting)
        {
            _desiredGait = movementSettings.sprinting;
            onSprintStarted.Invoke();
            return;
        }

        if (MovementState == FPSMovementState.Sliding)
        {
            _desiredGait.velocitySmoothing = movementSettings.slideDirectionSmoothing;
            _slideProgress = 0f;
            _animator.CrossFade(Sliding, 0.1f);
            onSlideStarted.Invoke();
            Crouch();
            return;
        }

        if (PoseState == FPSPoseState.Crouching)
        {
            _desiredGait = movementSettings.crouching;
            return;
        }

        if (PoseState == FPSPoseState.Prone)
        {
            _desiredGait = movementSettings.prone;
            return;
        }

        // Walking state
        _desiredGait = movementSettings.walking;
    }

    private bool CanUnCrouch()
    {
        float height = _originalHeight - _controller.radius * 2f;
        Vector3 position = rootBone.TransformPoint(_originalCenter + Vector3.up * height / 2f);
        return !Physics.CheckSphere(position, _controller.radius);
    }

    private void EnableProne()
    {
        Crouch();
        PoseState = FPSPoseState.Prone;
        _animator.SetBool(Crouching, false);
        _animator.SetBool(Proning, true);

        onProneStarted?.Invoke();
        _desiredGait = movementSettings.prone;
    }

    private void CancelProne()
    {
        if (!CanUnCrouch()) return;
        UnCrouch();
        PoseState = FPSPoseState.Standing;
        _animator.SetBool(Proning, false);

        onProneEnded?.Invoke();
        _desiredGait = movementSettings.walking;
    }

    private void Crouch()
    {
        print("crouch start");

        float crouchedHeight = _originalHeight * movementSettings.crouchRatio;
        float heightDifference = _originalHeight - crouchedHeight;

        _controller.height = crouchedHeight;

        // Adjust the center position so the bottom of the capsule remains at the same position
        Vector3 crouchedCenter = _originalCenter;
        crouchedCenter.y -= heightDifference / 2;
        _controller.center = crouchedCenter;

        PoseState = FPSPoseState.Crouching;

        _animator.SetBool(Crouching, true);
        onCrouch.Invoke();
    }

    private void UnCrouch()
    {
        print("crouch stop");

        _controller.height = _originalHeight;
        _controller.center = _originalCenter;

        PoseState = FPSPoseState.Standing;

        _animator.SetBool(Crouching, false);
        onUncrouch.Invoke();
    }

    private void UpdateSliding()
    {
        float slideAmount = movementSettings.slideCurve.Evaluate(_slideProgress);

        _velocity *= slideAmount;

        Vector3 desiredVelocity = _velocity;
        desiredVelocity.y = -movementSettings.gravity;
        MoveVector = desiredVelocity;

        _slideProgress = Mathf.Clamp01(_slideProgress + Time.deltaTime * movementSettings.slideSpeed);
    }

    private void UpdateGrounded()
    {
        var normInput = _inputDirection.normalized;
        var desiredVelocity = rootBone.right * normInput.x + rootBone.forward * normInput.y;

        desiredVelocity *= _desiredGait.velocity;

        desiredVelocity = Vector3.Lerp(_velocity, desiredVelocity,
            FPSAnimLib.ExpDecayAlpha(_desiredGait.velocitySmoothing, Time.deltaTime));

        _velocity = desiredVelocity;

        desiredVelocity.y = -movementSettings.gravity;
        MoveVector = desiredVelocity;
    }

    private void UpdateInAir()
    {
        var normInput = _inputDirection.normalized;
        _velocity.y -= movementSettings.gravity * Time.deltaTime;
        _velocity.y = Mathf.Max(-movementSettings.maxFallVelocity, _velocity.y);

        var desiredVelocity = rootBone.right * normInput.x + rootBone.forward * normInput.y;
        desiredVelocity *= _desiredGait.velocity;

        desiredVelocity = Vector3.Lerp(_velocity, desiredVelocity * movementSettings.airFriction,
            FPSAnimLib.ExpDecayAlpha(movementSettings.airVelocity, Time.deltaTime));

        desiredVelocity.y = _velocity.y;
        _velocity = desiredVelocity;

        MoveVector = desiredVelocity;
    }
}
