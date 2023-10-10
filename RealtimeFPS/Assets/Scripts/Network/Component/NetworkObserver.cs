using FrameWork.Network;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkObserver : NetworkComponent
{
    [SerializeField] private List<NetworkComponent> networkComponents = new();

    public void SetNetworkObject( Client client, int id, bool isMine, bool isPlayer, string ownerId )
    {
        base.client = client;

        this.objectId = id;
        this.isMine = isMine;
        this.isPlayer = isPlayer;

        if (isPlayer)
        {
            networkComponents.Add(GetComponent<NetworkTransform_FPS_DeadReckoning>());

            //transform.GetComponentInChildren<TMP_Text>().text = ownerId;

            if (!isMine)
            {

            }
        }

        else
        {
            networkComponents.Add(GetComponent<NetworkTransform_Rigidbody>());
        }

        foreach (NetworkComponent item in networkComponents)
        {
            item.client = base.client;
            item.objectId = id;
            item.isMine = isMine;
            item.isPlayer = isPlayer;
        }
    }
}
