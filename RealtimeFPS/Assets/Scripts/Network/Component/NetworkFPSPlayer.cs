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
			//�ǰ� �ִϸ��̼� ���
		}
	}
}
