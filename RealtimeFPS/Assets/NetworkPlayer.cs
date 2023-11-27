using Demo.Scripts.Runtime;
using Framework.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField] private NetworkObject networkObject;

    public int hp = 100;

    void Start()
    {
        if (networkObject.isMine)
        {
            GameManager.UI.FetchPanel<Panel_HUD>()?.UpdateHealth(hp);
            GameManager.UI.FetchPanel<Panel_HUD>()?.SetController(this.GetComponent<FPSController>());
        }
         
        networkObject.Client.packetHandler.AddHandler(OnAttacked);
    }

    private void OnDestroy()
    {
        networkObject.Client.packetHandler.RemoveHandler(OnAttacked);
    }

    private void OnAttacked( Protocol.S_FPS_ATTACKED pkt )
    {
        if (pkt.Playerid == networkObject.id)
            GameManager.UI.FetchPanel<Panel_HUD>()?.UpdateHealth(pkt.Hp);
    }
}
