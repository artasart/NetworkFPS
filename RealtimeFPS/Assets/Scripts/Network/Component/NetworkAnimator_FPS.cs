using Demo.Scripts.Runtime;
using Framework.Network;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAnimator_FPS : MonoBehaviour
{
    [SerializeField] private NetworkObject networkObject;

    [SerializeField] private FPSController FPSController;
    [SerializeField] private FPSMovement FPSMovement;
    [SerializeField] private Animator animator;

    private readonly float interval = 0.05f;

    void Start()
    {
        Timing.RunCoroutine(UpdateAnimation(), nameof(UpdateAnimation) + this.GetHashCode());
    }

    private void OnDestroy()
    {
        Timing.KillCoroutines(nameof(UpdateAnimation) + this.GetHashCode());
    }

    private IEnumerator<float> UpdateAnimation()
    {
        float delTime = 0f;
        Protocol.C_FPS_ANIMATION pkt = new Protocol.C_FPS_ANIMATION();
        Protocol.FPS_Animation payload = new Protocol.FPS_Animation();
        pkt.FpsAnimation = payload;

        while (true)
        {
            delTime += Time.deltaTime;
            if (delTime > interval)
            {
                payload.MoveX = animator.GetFloat("MoveX");
                payload.MoveY = animator.GetFloat("MoveY");
                payload.Velocity = animator.GetFloat("Velocity");
                payload.Moving = animator.GetBool("Moving");
                payload.InAir = animator.GetBool("InAir");
                payload.Sprinting = animator.GetFloat("Sprinting");

                payload.PoseState = (int)FPSMovement.PoseState;
                payload.MovementState = (int)FPSMovement.MovementState;

                payload.IsTurning = FPSController.test_turn;
                FPSController.test_turn = false;

                payload.TurnRight = FPSController.trigger_right;

                payload.LookX = FPSController._playerInput.x;
                payload.LookY = FPSController._playerInput.y;
                payload.Aiming = FPSController._aiming;

                networkObject.Client.Send(PacketManager.MakeSendBuffer(pkt));
            }

            yield return Timing.WaitForOneFrame;
        }
    }
}
