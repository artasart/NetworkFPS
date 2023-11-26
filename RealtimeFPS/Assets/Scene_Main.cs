using Demo.Scripts.Runtime;
using Framework.Network;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Scene_Main : MonoBehaviour
{
    GameObject item;
    GameObject destination;

    private void Awake()
    {
        GameManager.Scene.FadeInstant(true);
    }

    void Start()
    {
        GameUIManager.Instance.Restart();

        Protocol.C_FPS_LOAD_COMPLETE pkt = new();
        NetworkManager.Instance.Client.Send(PacketManager.MakeSendBuffer(pkt));

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItem);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItemOccupied);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnSpawnDestination);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnDestoryDestination);
    }

    void OnItem(Protocol.S_FPS_SPAWN_ITEM pkt)
    {
        if(destination != null)
        {
            Destroy(destination);
        }

        Vector3 position = NetworkUtils.ConvertVector3(pkt.Position);

        GameObject prefab = Resources.Load<GameObject>("Prefab/Item");

        item = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
    }

    void OnItemOccupied(Protocol.S_FPS_ITEM_OCCUPIED pkt)
    {
        Destroy(item);
    }

    void OnSpawnDestination(Protocol.S_FPS_SPAWN_DESTINATION pkt)
    {
        Vector3 position = NetworkUtils.ConvertVector3(pkt.Position);

        GameObject prefab = Resources.Load<GameObject>("Prefab/Destination");

        destination = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
    }

    void OnDestoryDestination(Protocol.S_FPS_DESTROY_DESTINATION pkt)
    {
        Destroy(destination);
    }
}
