using Framework.Network;
using Protocol;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using System.Collections.Generic;

public class Scene_Main : MonoBehaviour
{
    private readonly Dictionary<string, GameObject> gameObjects = new();

    private GameObject item;
    private GameObject destination;

    private void Awake()
    {
        GameManager.Scene.FadeInstant(true);
    }

    void Start()
    {
        GameUIManager.Instance.Restart();

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnStart);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnFinish);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnInstantiate);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnRemoveGameObject);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnSpawnItem);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItemOccupied);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnSpawnDestination);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnDestoryDestination);

        Protocol.C_FPS_LOAD_COMPLETE pkt = new();
        NetworkManager.Instance.Client.Send(PacketManager.MakeSendBuffer(pkt));
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnStart);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnFinish);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnInstantiate);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnRemoveGameObject);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnSpawnItem);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnItemOccupied);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnSpawnDestination);
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnDestoryDestination);
    }

    public void OnStart( S_FPS_START pkt )
    {
        GameManager.Scene.Fade(false);
    }

    public void OnFinish( S_FPS_FINISH pkt )
    {
        GameManager.Scene.Fade(true);
        GameManager.Scene.LoadScene(SceneName.Lobby);
    }

    public void OnInstantiate( S_FPS_INSTANTIATE pkt )
    {
        bool isMine = pkt.OwnerId == NetworkManager.Instance.Client.ClientId;

        Vector3 position = NetworkUtils.ConvertVector3(pkt.Position);
        Quaternion rotation = NetworkUtils.ConvertQuaternion(pkt.Rotation);

        GameObject prefab;

        if (isMine)
            prefab = Resources.Load<GameObject>("Prefab/Generic/PlayerCharacter");
        else
            prefab = Resources.Load<GameObject>("Prefab/Generic/PlayerCharacter_Remote");

        GameObject player = UnityEngine.Object.Instantiate(prefab, position, rotation);

        player.GetComponent<NetworkObject>().Client = NetworkManager.Instance.Client;
        player.GetComponent<NetworkObject>().id = pkt.PlayerId;
        player.GetComponent<NetworkObject>().isMine = isMine;

        player.GetComponent<NetworkPlayer>().hp = pkt.Hp;

        Debug.Log("AddGameObject: " + pkt.PlayerId + " " + pkt.OwnerId);

        player.name = pkt.PlayerId.ToString();

        gameObjects.Add(pkt.PlayerId.ToString(), player);
    }

    public void OnRemoveGameObject( S_REMOVE_GAME_OBJECT pkt )
    {
        foreach (int gameObjectId in pkt.GameObjects)
        {
            if (!gameObjects.ContainsKey(gameObjectId.ToString()))
            {
                continue;
            }

            UnityEngine.Object.DestroyImmediate(gameObjects[gameObjectId.ToString()]);

            gameObjects.Remove(gameObjectId.ToString());
        }
    }

    void OnSpawnItem(Protocol.S_FPS_SPAWN_ITEM pkt)
    {
        GameUIManager.Instance.OpenPanel<Progressbar>();
        
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
        GameUIManager.Instance.ClosePanel<Progressbar>();

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
