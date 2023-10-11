using FrameWork.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkFPSPlayer : NetworkComponent
{
    void Start()
    {
        client.packetHandler.AddHandler(OnAttacked);
    }

    private void OnAttacked( Protocol.S_ATTACKED pkt )
    {
        if (pkt.Playerid == objectId && pkt.Hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
