using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    bool isTab = false;

    private void Start()
    {
        GameManager.UI.StackPanel<Panel_Network>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isTab = !isTab;

            if (isTab)
            {
                GameManager.UI.StackPanel<Panel_Network>();
            }
            else
            {
                GameManager.UI.PopPanel();
                GameManager.UI.StackPanel<Panel_HUD>();
            }
        }
    }
}
