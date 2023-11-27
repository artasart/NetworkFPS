using Framework.Network;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

public class Client : Connection
{
	public string ClientId { get; set; }

	public Client()
	{
        packetHandler.AddHandler(OnEnter);
    }

    ~Client()
    {
        Debug.Log("Client Destructor");

        packetHandler.RemoveHandler(OnEnter);
    }

    public void OnEnter( S_ENTER pkt )
    {
        if (pkt.Result != "SUCCESS")
        {
            Debug.Log(pkt.Result);
            return;
        }
    }

    public void DisplayPing( Protocol.S_PING pkt )
    {
        Panel_NetworkInfo.Instance.SetPing((int)pingAverage);
    }
}