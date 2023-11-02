using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    bool isTab = false;

    private void Start()
    {
        GameManager.UI.OpenPanel<Panel_Network>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameManager.UI.TogglePanel<Panel_Network>();
            GameManager.UI.TogglePanel<Panel_HUD>();
        }
    }
}
