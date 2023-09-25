﻿using Framework.Network;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

public class Client : Connection
{
    public string ClientId { get; set; }

    private readonly Dictionary<string, GameObject> gameObjects = new();
    private HashSet<int> myGameObjectId = new();

    public Client()
    {
        packetHandler.AddHandler(OnEnter);
        packetHandler.AddHandler(OnInstantiateGameObject);
        packetHandler.AddHandler(OnAddGameObject);
        packetHandler.AddHandler(OnRemoveGameObject);
        packetHandler.AddHandler(OnDisconnected);
        packetHandler.AddHandler(DisplayPing);
        packetHandler.AddHandler(OnOwnerChanged);
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
            C_INSTANTIATE_FPS_PLAYER packet = new();

			packet.Transform = new()
			{
				Position = NetworkUtils.UnityVector3ToProtocolVector3(UnityEngine.Vector3.zero),
				Rotation = NetworkUtils.UnityVector3ToProtocolVector3(UnityEngine.Vector3.zero),
				Velocity = NetworkUtils.UnityVector3ToProtocolVector3(UnityEngine.Vector3.zero),
				AngularVelocity = NetworkUtils.UnityVector3ToProtocolVector3(UnityEngine.Vector3.zero),
			};

			Send(PacketManager.MakeSendBuffer(packet));

            DebugManager.Log("Instantiate");
        }
    }

    public void OnInstantiateGameObject( S_INSTANTIATE_GAME_OBJECT pkt )
    {
        myGameObjectId.Add(pkt.GameObjectId);
    }

    public void OnAddGameObject(S_ADD_FPS_PLAYER _packet )
    {
        foreach (S_ADD_FPS_PLAYER.Types.GameObjectInfo gameObject in _packet.GameObjects)
        {
            UnityEngine.Vector3 position = new(gameObject.Transform.Position.X, gameObject.Transform.Position.Y, gameObject.Transform.Position.Z);
            UnityEngine.Quaternion rotation = Quaternion.Euler(gameObject.Transform.Rotation.X, gameObject.Transform.Rotation.Y, gameObject.Transform.Rotation.Z);

            GameObject prefab = Resources.Load<GameObject>("Prefab/FPSMan");

            GameObject player = UnityEngine.Object.Instantiate(prefab, position, rotation);

			player.GetComponent<NetworkObserver>().SetNetworkObject(
				this
				, gameObject.PlayerId
				, gameObject.OwnerId == ClientId
                , true
				, gameObject.OwnerId);

			player.name = gameObject.PlayerId.ToString();

			UnityEngine.Object.FindObjectOfType<EffectPool>().Spawn(EffectType.Effect_Thunder, position, Quaternion.identity);

			gameObjects.Add(gameObject.PlayerId.ToString(), player);

            DebugManager.Log("S_ ADD GAMEOBJECT");
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

            UnityEngine.Object.Destroy(gameObjects[gameObjectId.ToString()]);

            _ = gameObjects.Remove(gameObjectId.ToString());
        }
    }

    public void OnOwnerChanged(S_SET_GAME_OBJECT_OWNER pkt)
    {
        if(pkt.OwnerId == ClientId && !myGameObjectId.Contains(pkt.GameObjectId))
        {
            myGameObjectId.Add(pkt.GameObjectId);
        }
        else if(pkt.OwnerId != ClientId && myGameObjectId.Contains(pkt.GameObjectId))
        {
            myGameObjectId.Remove(pkt.GameObjectId);
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
}