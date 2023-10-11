using Framework.Network;
using Protocol;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Client : Connection
{
	public string ClientId { get; set; }

	private readonly Dictionary<string, GameObject> gameObjects = new();
	private HashSet<int> playerId = new();

	public Client()
	{
        packetHandler.AddHandler(OnEnter);
        packetHandler.AddHandler(OnInstantiateGameObject);
        packetHandler.AddHandler(OnAddGameObject);
        packetHandler.AddHandler(OnRemoveGameObject);
        packetHandler.AddHandler(OnDisconnected);
        packetHandler.AddHandler(DisplayPing);
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
        playerId.Add(pkt.GameObjectId);
    }

    public void OnAddGameObject(S_ADD_FPS_PLAYER _packet )
    {
        foreach (S_ADD_FPS_PLAYER.Types.GameObjectInfo gameObject in _packet.GameObjects)
        {
            UnityEngine.Vector3 position = new(gameObject.Position.X, gameObject.Position.Y, gameObject.Position.Z);

            Quaternion rotation = Quaternion.Euler(gameObject.Rotation.X, gameObject.Rotation.Y, gameObject.Rotation.Z);

            var prefabName = string.Empty;

			if(gameObject.PlayerId == GetPlayerId())
            {
                prefabName = "Prefab/FPSMan";
			}

            else
            {
				prefabName = "Prefab/FPSManOther";
			}

			GameObject prefab = Resources.Load<GameObject>(prefabName);

			GameObject player = UnityEngine.Object.Instantiate(prefab, position, rotation);

            Debug.Log("AddGameObject: " + gameObject.PlayerId + " " + gameObject.OwnerId);

			player.GetComponentInChildren<NetworkObserver>().SetNetworkObject(
				this
				, gameObject.PlayerId
				, gameObject.OwnerId == ClientId
                , true
				, gameObject.OwnerId);

			player.name = gameObject.PlayerId.ToString();

			UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, position, Quaternion.identity);

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

    public int GetPlayerId()
    {
		return ConvertIntArrayToStringAndBack();
	}

	private int ConvertIntArrayToStringAndBack()
	{
		string combinedString = string.Join("", playerId.ToArray().Select(x => x.ToString()));

		if (int.TryParse(combinedString, out int result))
		{
			return result;
		}

		else return 0;
	}
}