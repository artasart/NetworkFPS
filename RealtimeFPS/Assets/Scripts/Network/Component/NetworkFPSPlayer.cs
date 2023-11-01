using FrameWork.Network;

public class NetworkFPSPlayer : NetworkComponent
{
    void Start()
    {
        client.packetHandler.AddHandler(OnAttacked);
    }

	private void OnAttacked(Protocol.S_ATTACKED pkt)
	{
		if (pkt.Playerid == objectId)
		{
			//피격 애니메이션 재생
		}
	}
}
