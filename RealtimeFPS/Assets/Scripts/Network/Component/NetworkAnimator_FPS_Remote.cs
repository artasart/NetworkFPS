using Demo.Scripts.Runtime;
using Framework.Network;
using Google.Protobuf.WellKnownTypes;
using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAnimator_FPS_Remote : MonoBehaviour
{
    [SerializeField] private NetworkObject networkObject;

    [SerializeField] private FPSController_Dummy FPSController;
    [SerializeField] private FPSMovement_Dummy FPSMovement;
    [SerializeField] private Animator animator;

    private readonly float interval = 0.05f;

    private CoroutineHandle updateAnimation;

    void Start()
    {
        networkObject.Client.packetHandler.AddHandler(OnAnimation);
        networkObject.Client.packetHandler.AddHandler(OnFire);
        networkObject.Client.packetHandler.AddHandler(OnReload);
        networkObject.Client.packetHandler.AddHandler(OnChangeWeapon);
    }

    private void OnDestroy()
    {
        networkObject.Client.packetHandler.RemoveHandler(OnAnimation);
        networkObject.Client.packetHandler.RemoveHandler(OnFire);
        networkObject.Client.packetHandler.RemoveHandler(OnReload);
        networkObject.Client.packetHandler.RemoveHandler(OnChangeWeapon);

        Timing.KillCoroutines(updateAnimation);
    }

    private void OnAnimation( Protocol.S_FPS_ANIMATION pkt )
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        FPSMovement.SetPose((FPSPoseState)pkt.FpsAnimation.PoseState);
        FPSMovement.SetMovement((FPSMovementState)pkt.FpsAnimation.MovementState);

        FPSController.SetAds(pkt.FpsAnimation.Aiming);
        FPSController.SetAimPoint(new Vector2(pkt.FpsAnimation.LookX, pkt.FpsAnimation.LookY));

        if (pkt.FpsAnimation.IsTurning)
        {
            animator.ResetTrigger("TurnRight");
            animator.ResetTrigger("TurnLeft");

            if (pkt.FpsAnimation.TurnRight)
            {
                animator.SetTrigger("TurnRight");
            }
            else
            {
                animator.SetTrigger("TurnLeft");
            }
        }

        if (updateAnimation.IsRunning)
            Timing.KillCoroutines(updateAnimation);

        updateAnimation = Timing.RunCoroutine(UpdateAnimation(pkt.FpsAnimation));
    }

    private IEnumerator<float> UpdateAnimation( Protocol.FPS_Animation pkt )
    {
        float delTime = 0.0f;

        animator.SetBool("Moving", pkt.Moving);
        animator.SetBool("InAir", pkt.InAir);

        float prevMoveX = animator.GetFloat("MoveX");
        float prevMoveY = animator.GetFloat("MoveY");
        float prevVelocity = animator.GetFloat("Velocity");
        float prevSprinting = animator.GetFloat("Sprinting");

        while (delTime < interval)
        {
            delTime += Time.deltaTime;

            animator.SetFloat("MoveX", Mathf.Lerp(prevMoveX, pkt.MoveX, delTime / interval));
            animator.SetFloat("MoveY", Mathf.Lerp(prevMoveY, pkt.MoveY, delTime / interval));
            animator.SetFloat("Velocity", Mathf.Lerp(prevVelocity, pkt.Velocity, delTime / interval));
            animator.SetFloat("Sprinting", Mathf.Lerp(prevSprinting, pkt.Sprinting, delTime / interval));

            yield return Timing.WaitForOneFrame;
        }
    }

    private void OnFire(Protocol.S_SHOOT pkt)
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        FPSController.Fire();
    }

    private void OnReload( Protocol.S_RELOAD pkt )
    {
        if (pkt.PlayerId != networkObject.id)
            return;
    }

    private void OnChangeWeapon( Protocol.S_CHANGE_WEAPON pkt )
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        float waitTime = (pkt.Timestamp + FPSController.equipDelay * 1000 - networkObject.Client.calcuatedServerTime) / 1000f;
        Timing.RunCoroutine(FPSController.ChangeWeapon(pkt.WeaponId, waitTime));
    }
}