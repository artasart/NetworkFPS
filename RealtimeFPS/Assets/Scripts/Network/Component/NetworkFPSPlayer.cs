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
			if (pkt.Hp <= 0) Debug.Log("�׾����ϴ�.");

			GameManager.UI.FetchPanel<Panel_HUD>().UpdateHealth(pkt.Hp);
		}

		else
		{
			Debug.Log(pkt.Playerid + "�� �÷��̾ �¾ҽ��ϴ�! ���� HP : " + pkt.Hp);
		}
	}
}
