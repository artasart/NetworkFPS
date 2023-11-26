using Demo.Scripts.Runtime;
using Framework.Network;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class NetworkAnimator_FPS : MonoBehaviour
{
    [SerializeField] private NetworkObject networkObject;

    [SerializeField] private FPSController controllerComponent;
    [SerializeField] private FPSMovement movementCompnent;

    private readonly float interval = 0.05f;

    CoroutineHandle updateAnimation;

    void Start()
    {
        controllerComponent.OnFire += OnFire;
        controllerComponent.OnReload += OnReload;
        controllerComponent.OnChangeWeapon += OnChangeWeapon;

        updateAnimation = Timing.RunCoroutine(UpdateAnimation());
    }

    private void OnDestroy()
    {
        Timing.KillCoroutines(updateAnimation);
    }

    private IEnumerator<float> UpdateAnimation()
    {
        float delTime = 0f;
        Protocol.C_FPS_ANIMATION pkt = new Protocol.C_FPS_ANIMATION();
        Protocol.FPSAnimation payload = new Protocol.FPSAnimation();
        pkt.FpsAnimation = payload;

        while (true)
        {
            delTime += Time.deltaTime;
            if (delTime > interval)
            {
                movementCompnent.MakePacket(ref payload);
                controllerComponent.MakePacket(ref payload);

                networkObject.Client.Send(PacketManager.MakeSendBuffer(pkt));
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    private void OnFire()
    {
        Protocol.C_FPS_SHOOT pkt = new Protocol.C_FPS_SHOOT();

        Transform aimPoint = controllerComponent.GetGun().GetAimPoint();

        pkt.Position = NetworkUtils.ConvertVector3(aimPoint.position);
        pkt.Direction = NetworkUtils.ConvertVector3(aimPoint.forward);

        networkObject.Client.Send(PacketManager.MakeSendBuffer(pkt));
    }

    private void OnReload()
    {
        Protocol.C_FPS_RELOAD pkt = new Protocol.C_FPS_RELOAD();

        networkObject.Client.Send(PacketManager.MakeSendBuffer(pkt));
    }

    private void OnChangeWeapon(int weaponId)
    {
        Protocol.C_FPS_CHANGE_WEAPON pkt = new Protocol.C_FPS_CHANGE_WEAPON();
        pkt.WeaponId = weaponId;
        pkt.Timestamp = networkObject.Client.calcuatedServerTime;
        networkObject.Client.Send(PacketManager.MakeSendBuffer(pkt));
    }
}
