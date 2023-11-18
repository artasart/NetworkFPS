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
        Protocol.FPS_Animation payload = new Protocol.FPS_Animation();
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
}
