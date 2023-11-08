using FrameWork.Network;
using UnityEngine;

public class NetworkFPSPlayer : MonoBehaviour
{
	NetworkObject networkObject;

    void Start()
    {
		networkObject = GetComponent<NetworkObject>();

        networkObject.Client.packetHandler.AddHandler(OnAttacked);
    }

	private void OnAttacked(Protocol.S_ATTACKED pkt)
	{

	}
}
