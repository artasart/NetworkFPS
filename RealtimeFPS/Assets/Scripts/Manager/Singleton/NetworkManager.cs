using Cysharp.Threading.Tasks;
using Framework.Network;
using Newtonsoft.Json.Linq;
using Protocol;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : SingletonManager<NetworkManager>
{
    public Client Client { get; private set; }

    private readonly bool isLocal = true;

    private readonly string localAddress = "169.254.22.122";
    private readonly int localPort = 7777;

    private readonly string remoteAddress = "20.200.230.139";
    private readonly int remotePort = 32000;

    private readonly string query = "/Room/FPS";

    private void Awake()
    {
        Client = new();
    }

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
        await webRequest.SendWebRequest();
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
        if (Client.State == ConnectionState.Connected)
            return;

        IPEndPoint endPoint = await GetAddress();
        if (endPoint == null)
        {
            Debug.Log("GetAddress Fail!");
            return;
        }

        bool success = await Connector.Connect(endPoint, Client);
        if (success)
        {
            Client.ClientId = connectionId;

            C_ENTER enter = new()
            {
                ClientId = connectionId
            };

            Client.Send(PacketManager.MakeSendBuffer(enter));
        }
    }

    public void Disconnect()
    {
        if (Client == null || Client.State == ConnectionState.Closed)
            return;

        Client.Send(PacketManager.MakeSendBuffer(new C_LEAVE()));
    }
}
