using Framework.Network;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

public class Client : Connection
{
	public string ClientId { get; set; }

    public HashSet<string> ClientList { get; set; }

	public Client()
	{
        ClientList = new HashSet<string>();

        packetHandler.AddHandler(OnEnter);
        packetHandler.AddHandler(OnAddClient);
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

    public void OnAddClient( S_ADD_CLIENT pkt )
    {
        for(int i = 0; i < pkt.ClientInfos.Count; i++)
            ClientList.Add(pkt.ClientInfos[i].ClientId);
    }

    public void DisplayPing( Protocol.S_PING pkt )
    {
        Panel_NetworkInfo.Instance.SetPing((int)pingAverage);
    }
}