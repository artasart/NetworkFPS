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
        MoveX,
        MoveY,
        Velocity,
        InAir,
        Moving,
        Sprinting,
    }
}