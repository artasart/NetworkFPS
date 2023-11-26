using Demo.Scripts.Runtime;
using Framework.Network;
using Protocol;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class Client : Connection
{
	public string ClientId { get; set; }

	private readonly Dictionary<string, GameObject> gameObjects = new();
	private int myPlayerId;

	public Client()
	{
        packetHandler.AddHandler(OnEnter);
        packetHandler.AddHandler(OnLoad);
        packetHandler.AddHandler(OnStart);
        packetHandler.AddHandler(OnFinish);

        packetHandler.AddHandler(OnInstantiateGameObject);
        packetHandler.AddHandler(OnAddGameObject);
        packetHandler.AddHandler(OnRemoveGameObject);
        packetHandler.AddHandler(OnDisconnected);
        packetHandler.AddHandler(DisplayPing);
        packetHandler.AddHandler(OnAttacked);
        packetHandler.AddHandler(OnItemOccupied);
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
    }

    public void OnLoad(S_FPS_LOAD pkt)
    {
        GameManager.Scene.LoadScene(SceneName.Main);
    }

    public void OnStart(S_FPS_START pkt)
    {
        GameManager.Scene.Fade(false);
    }

    public void OnFinish(S_FPS_FINISH pkt)
    {
        GameManager.Scene.Fade(true);
        GameManager.Scene.LoadScene(SceneName.Lobby);
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

            Vector3 position = NetworkUtils.ConvertVector3(gameObject.Position);
            Quaternion rotation = NetworkUtils.ConvertQuaternion(gameObject.Rotation);

            GameObject prefab;

            if(isMine)
                prefab = Resources.Load<GameObject>("Demo/Prefabs/Generic/PlayerCharacter");
            else
                prefab = Resources.Load<GameObject>("Demo/Prefabs/Generic/PlayerCharacter_Other");

            GameObject player = UnityEngine.Object.Instantiate(prefab, position, rotation);

            if (isMine)
            {
                GameUIManager.Instance.FetchPanel<Panel_HUD>().SetController(player.GetComponent<FPSController>());
                GameManager.UI.FetchPanel<Panel_HUD>().UpdateHealth(100);
            }

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

    private void OnItemOccupied(Protocol.S_FPS_ITEM_OCCUPIED pkt)
    {
        Debug.Log("item Occupied : " + pkt.Occupier);
    }
}