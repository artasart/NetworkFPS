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

	private void OnAttacked(Protocol.S_ATTACKED pkt)
	{
		if (pkt.Playerid == objectId)
		{
			if (pkt.Hp <= 0) Debug.Log("죽었습니다.");

			GameManager.UI.FetchPanel<Panel_HUD>().UpdateHealth(pkt.Hp);
		}

		else
		{
			Debug.Log(pkt.Playerid + "번 플레이어가 맞았습니다! 남은 HP : " + pkt.Hp);
		}
	}
}
