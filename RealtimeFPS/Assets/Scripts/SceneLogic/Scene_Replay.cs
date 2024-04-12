using Framework.Network;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_Replay : MonoBehaviour
{
    void Start()
    {
        GameManager.Scene.Fade(false);

        Debug.Log("Scene_Replay Start");

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnReplay);

        Protocol.C_FPS_REPLAY req = new Protocol.C_FPS_REPLAY();
        NetworkManager.Instance.Client.Send(PacketManager.MakeSendBuffer(req));
    }

    public void OnReplay( S_FPS_REPLAY pkt)
    {
        Debug.Log("OnReplay");
        Debug.Log(pkt.ReplayData.Count);

        StartCoroutine(Co_Replay(pkt));
    }


    IEnumerator Co_Replay( S_FPS_REPLAY pkt )
    {
        for (int i = 0; i < pkt.ReplayData.Count; i++)
        {
            Debug.Log(pkt.ReplayData[i]);
            yield return new WaitForSeconds(0.05f);
        }

        GameManager.Scene.Fade(true);
        GameManager.Scene.LoadScene(SceneName.Lobby);
    }
}
