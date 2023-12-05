namespace Demo.Scripts.Runtime
{
    public enum FPSMovementState
    {
        Idle,
        Walking,
        Sprinting,
        InAir,
        Sliding
    }

    public enum FPSPoseState
    {
        Standing,
        Crouching,
        Prone
    }

    public enum AnimatorParamId
    {
        InAir,
        MoveX,
        MoveY,
        Velocity,
        Moving,
        Crouching,
        Sprinting,
        Proning,

        MovementState,

        PoseState
    }
}