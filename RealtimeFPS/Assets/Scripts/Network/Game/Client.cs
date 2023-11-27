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
        packetHandler.AddHandler(OnDisconnected);
    }

    ~Client()
    {
        Debug.Log("Client Destructor");

        packetHandler.RemoveHandler(OnEnter);
        packetHandler.RemoveHandler(OnDisconnected);
    }

    public void OnEnter( S_ENTER pkt )
    {
        if (pkt.Result != "SUCCESS")
        {
            Debug.Log(pkt.Result);
            return;
        }
    }

    public void OnDisconnected( S_DISCONNECT pkt )
    {
        Debug.Log("Disconnected");
    }

    public void DisplayPing( Protocol.S_PING pkt )
    {
        //Panel_NetworkInfo.Instance.SetPing((int)pingAverage);
    }

    private void OnItemOccupied(Protocol.S_FPS_ITEM_OCCUPIED pkt)
    {
        Debug.Log("item Occupied : " + pkt.Occupier);
    }
}