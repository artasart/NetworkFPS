using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene_Lobby : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameUIManager.Instance.Restart();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameManager.UI.OpenPanel<Panel_Network>();
        GameManager.Scene.Fade(false);
    }
}
