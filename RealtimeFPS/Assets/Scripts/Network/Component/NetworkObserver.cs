using FrameWork.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkObserver : NetworkComponent
{
    [SerializeField] private List<NetworkComponent> networkComponents = new();

    public void SetNetworkObject( Client client, int id, bool isMine, bool isPlayer, string ownerId )
    {
        this.client = client;

        this.objectId = id;
        this.isMine = isMine;
        this.isPlayer = isPlayer;

        if (isPlayer)
        {
            networkComponents.Add(GetComponent<NetworkTransform_FPS_DeadReckoning>());
            networkComponents.Add(GetComponent<NetworkFPSPlayer>());
        }

        else
        {
            networkComponents.Add(GetComponent<NetworkTransform_Rigidbody>());
        }

        foreach (NetworkComponent item in networkComponents)
        {
            item.client = this.client;
            item.objectId = id;
            item.isMine = isMine;
            item.isPlayer = isPlayer;
        }
    }
}
