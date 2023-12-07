using Framework.Network;
using UnityEngine;

public class MyPlayer : MonoBehaviour
{
    private void Start()
    {
        GameUIManager.Instance.OpenPanel<Panel_HUD>();
    }
}
