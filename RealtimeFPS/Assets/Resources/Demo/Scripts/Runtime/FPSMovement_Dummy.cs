using FrameWork.Network;
using Demo.Scripts.Runtime;
using UnityEngine;
using UnityEngine.Events;
using Microsoft.Cci;

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

    private Animator _animator;

    private static readonly int InAir = Animator.StringToHash("InAir");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int Moving = Animator.StringToHash("Moving");
    private static readonly int Crouching = Animator.StringToHash("Crouching");
    private static readonly int Sliding = Animator.StringToHash("Sliding");
    private static readonly int Sprinting = Animator.StringToHash("Sprinting");
    private static readonly int Proning = Animator.StringToHash("Proning");

    public FPSMovementState MovementState { get; private set; }
    public FPSPoseState PoseState { get; private set; }

    public Vector3 MoveVector { get; private set; }

    private void Start()
    {
        MovementState = FPSMovementState.Idle;
        PoseState = FPSPoseState.Standing;

        _animator = GetComponentInChildren<Animator>();

        networkObject.Client.packetHandler.AddHandler(OnSetAnimation);
    }

    public void OnSetAnimation(Protocol.S_SET_ANIMATION pkt)
    {
        if (pkt.GameObjectId != networkObject.id)
            return;

        _animator.SetFloat(MoveX, pkt.Params[(int)AnimatorParamId.MoveX].FloatParam);
        _animator.SetFloat(MoveY, pkt.Params[(int)AnimatorParamId.MoveY].FloatParam);
        _animator.SetFloat(Velocity, pkt.Params[(int)AnimatorParamId.Velocity].FloatParam);
        _animator.SetBool(InAir, pkt.Params[(int)AnimatorParamId.InAir].BoolParam);
        _animator.SetBool(Moving, pkt.Params[(int)AnimatorParamId.Moving].BoolParam);
        _animator.SetFloat(Sprinting, pkt.Params[(int)AnimatorParamId.Sprinting].FloatParam);
    }

}
