using Cysharp.Threading.Tasks;
using Framework.Network;
using Newtonsoft.Json.Linq;
using Protocol;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GameClientManager : MonoBehaviour
{
    #region Singleton

    public static GameClientManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<GameClientManager>();
            return instance;
        }
    }
    private static GameClientManager instance;

    #endregion

    public Client Client { get; private set; }

    private readonly bool isLocal = false;
    
    private readonly string localAddress = "192.168.0.104";
    private readonly int localPort = 7777;

    private readonly string remoteAddress = "20.200.230.139";
    private readonly int remotePort = 32000;

    private readonly string query = "/Room/FPS";


    private void OnDestroy()
	{
        Disconnect();
	}

    public async Task<IPEndPoint> GetAddress()
    {
        if (isLocal)
        {
            return new(IPAddress.Parse(localAddress), localPort);
        }

        using UnityWebRequest webRequest = UnityWebRequest.Get("http://" + remoteAddress + ":" + remotePort + query);
        _ = await webRequest.SendWebRequest();
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string response = webRequest.downloadHandler.text;
            JObject jsonResponse = JObject.Parse(response);

            string address = jsonResponse["status"]["address"].ToString();

            int defaultPort = 0;
            JArray ports = (JArray)jsonResponse["status"]["ports"];
            foreach (JObject port in ports)
            {
                if (port["name"].ToString() == "default")
                {
                    defaultPort = port["port"].ToObject<int>();
                    break;
                }
            }

            return defaultPort != 0 ? new(IPAddress.Parse(address), defaultPort) : null;
        }
        else
        {
            return null;
        }
    }

    public async void Connect( string connectionId )
    {
        if (Client != null)
        {
            return;
        }

        IPEndPoint endPoint = await GetAddress();
        if (endPoint == null)
        {
            Debug.Log("GetAddress Fail!");
            return;
        }

        Client = (Client)ConnectionManager.GetConnection<Client>();

        bool success = await ConnectionManager.Connect(endPoint, Client);
        if (success)
        {
            Client.ClientId = connectionId;

            C_ENTER enter = new()
            {
                ClientId = connectionId
            };

            Client.Send(PacketManager.MakeSendBuffer(enter));

            GameManager.UI.FetchPanel<Panel_Network>().SetConnetButtonState(false);
            GameManager.UI.FetchPanel<Panel_Network>().SetDisconnectButtonState(true);

            GameManager.UI.ClosePanel<Panel_Network>();
            GameManager.UI.OpenPanel<Panel_HUD>();
        }
    }

    public void Disconnect()
    {
        if (Client == null)
        {
            return;
        }

        Client.Send(PacketManager.MakeSendBuffer(new C_LEAVE()));
        Client = null;

        GameManager.UI.FetchPanel<Panel_Network>().SetDisconnectButtonState(false);
        GameManager.UI.FetchPanel<Panel_Network>().SetConnetButtonState(true);
    }
}
