using Protocol;
using UnityEngine;

public class Scene_Lobby : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameUIManager.Instance.Restart();
        GameManager.UI.OpenPanel<Panel_Network>();
        GameManager.Scene.Fade(false);

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnLoad);
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
