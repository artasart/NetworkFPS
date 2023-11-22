using Framework.Network;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Scene_Main : MonoBehaviour
{
    private void Awake()
    {
        GameManager.Scene.FadeInstant(true);
    }

    void Start()
    {
        GameUIManager.Instance.Restart();

        Protocol.C_FPS_LOAD_COMPLETE pkt = new();
        NetworkManager.Instance.Client.Send(PacketManager.MakeSendBuffer(pkt));
    }
}
