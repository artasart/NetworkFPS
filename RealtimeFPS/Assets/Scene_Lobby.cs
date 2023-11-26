using Framework.Network;
using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_Lobby : MonoBehaviour
{
    void Start()
    {
        GameUIManager.Instance.Restart();

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnLoad);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameManager.UI.OpenPanel<Panel_Network>();

        GameManager.Scene.Fade(false);
    }

    private void OnDestroy()
    {
        NetworkManager.Instance.Client.packetHandler.RemoveHandler(OnLoad);
    }

    public void OnLoad( S_FPS_LOAD pkt )
    {
        GameManager.Scene.Fade(true);
        GameManager.Scene.LoadScene(SceneName.Main);
    }
}
