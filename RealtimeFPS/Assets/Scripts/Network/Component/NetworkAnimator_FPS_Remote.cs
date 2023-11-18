using Demo.Scripts.Runtime;
using Framework.Network;
using MEC;
using UnityEngine;

public class NetworkAnimator_FPS_Remote : MonoBehaviour
{
    [SerializeField] private NetworkObject networkObject;

    [SerializeField] private FPSController_Remote controllerComponent;
    [SerializeField] private FPSMovement_Remote movementComponent;

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

        movementComponent.SetPose((FPSPoseState)pkt.FpsAnimation.PoseState);
        movementComponent.SetMovement((FPSMovementState)pkt.FpsAnimation.MovementState);

        if (pkt.FpsAnimation.IsTurning)
            controllerComponent.Turn(pkt.FpsAnimation.TurnRight);

        controllerComponent.SetAds(pkt.FpsAnimation.Aiming);
        controllerComponent.SetAim(new Vector2(pkt.FpsAnimation.LookX, pkt.FpsAnimation.LookY));

        if (updateAnimation.IsRunning)
            Timing.KillCoroutines(updateAnimation);

        updateAnimation = Timing.RunCoroutine(movementComponent.UpdateAnimation(pkt.FpsAnimation, interval));
    }

    private void OnFire(Protocol.S_SHOOT pkt)
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        controllerComponent.Fire();
    }

    private void OnReload( Protocol.S_RELOAD pkt )
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        controllerComponent.Reload();
    }

    private void OnChangeWeapon( Protocol.S_CHANGE_WEAPON pkt )
    {
        if (pkt.PlayerId != networkObject.id)
            return;

        float waitTime = (pkt.Timestamp + controllerComponent.equipDelay * 1000 - networkObject.Client.calcuatedServerTime) / 1000f;
        Timing.RunCoroutine(controllerComponent.ChangeWeapon(pkt.WeaponId, waitTime));
    }
}