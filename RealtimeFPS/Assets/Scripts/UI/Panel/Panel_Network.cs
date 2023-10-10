using Framework.Network;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Network : Panel_Base
{
    private Button btn_Connect;

	protected override void Awake()
    {
        base.Awake();

        btn_Connect = GetUI_Button(nameof(btn_Connect), OnClick_Connect);
    }

	private void OnClick_Connect()
    {
        string clientId = string.Empty;

        if (string.IsNullOrEmpty(clientId))
        {
            clientId = Util.GenerateRandomString(5);
        }

        GameClientManager.Instance.Connect(clientId);
    }
}
