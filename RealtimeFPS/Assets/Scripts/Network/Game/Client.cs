using Cysharp.Threading.Tasks;
using Demo.Scripts.Runtime;
using Framework.Network;
using FrameWork.Network;
using MEC;
using Protocol;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Client : Connection
{
	public string ClientId { get; set; }

	private readonly Dictionary<string, GameObject> gameObjects = new();
	private int myPlayerId;

	public Client()
	{
        packetHandler.AddHandler(OnEnter);
        packetHandler.AddHandler(OnInstantiateGameObject);
        packetHandler.AddHandler(OnAddGameObject);
        packetHandler.AddHandler(OnRemoveGameObject);
        packetHandler.AddHandler(OnDisconnected);
        packetHandler.AddHandler(DisplayPing);
        packetHandler.AddHandler(OnAttacked);
    }

    ~Client()
    {
        Debug.Log("Client Destructor");
    }

    public void OnEnter( S_ENTER pkt )
    {
        if (pkt.Result != "SUCCESS")
        {
            Debug.Log(pkt.Result);
            return;
        }

        {
            C_GET_GAME_OBJECT packet = new();

            Send(PacketManager.MakeSendBuffer(packet));
        }

        {
            C_INSTANTIATE_FPS_PLAYER packet = new()
            { 
                Position = NetworkUtils.UnityVector3ToProtocolVector3(UnityEngine.Vector3.zero),
                Rotation = NetworkUtils.UnityVector3ToProtocolVector3(UnityEngine.Vector3.zero),
            };

			Send(PacketManager.MakeSendBuffer(packet));
        }
    }

    public void OnInstantiateGameObject( S_INSTANTIATE_GAME_OBJECT pkt )
    {
        myPlayerId = pkt.GameObjectId;
    }

    public void OnAddGameObject(S_ADD_FPS_PLAYER _packet )
    {
        foreach (S_ADD_FPS_PLAYER.Types.GameObjectInfo gameObject in _packet.GameObjects)
        {
            bool isMine = gameObject.PlayerId == myPlayerId;

            UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);
            Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

            GameObject prefab;

            if(isMine)
                prefab = Resources.Load<GameObject>("Demo/Prefabs/Generic/PlayerCharacter");
                //prefab = Resources.Load<GameObject>("Prefab/FPSMan");
            else
                prefab = Resources.Load<GameObject>("Demo/Prefabs/Generic/PlayerCharacter_Other");
                //prefab = Resources.Load<GameObject>("Prefab/FPSManOther");

            GameObject player = UnityEngine.Object.Instantiate(prefab, position, rotation);

            player.GetComponent<NetworkObject>().Client = this;
            player.GetComponent<NetworkObject>().id = gameObject.PlayerId;
            player.GetComponent<NetworkObject>().isMine = isMine;

            Debug.Log("AddGameObject: " + gameObject.PlayerId + " " + gameObject.OwnerId);

            player.name = gameObject.PlayerId.ToString();

            gameObjects.Add(gameObject.PlayerId.ToString(), player);
        }
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

    public void OnDisconnected( S_DISCONNECT pkt )
    {
        foreach (KeyValuePair<string, GameObject> gameObject in gameObjects)
        {
            UnityEngine.Object.Destroy(gameObject.Value);
        }

        //GameManager.UI.FetchPanel<Panel_HUD>().Clear();
        gameObjects.Clear();
    }

    public void DisplayPing( Protocol.S_PING pkt )
    {
        //Panel_NetworkInfo.Instance.SetPing((int)pingAverage);
    }

    private void OnAttacked( Protocol.S_ATTACKED pkt )
    {
        if (pkt.Playerid == myPlayerId)
            GameManager.UI.FetchPanel<Panel_HUD>().UpdateHealth(pkt.Hp);
    }
}