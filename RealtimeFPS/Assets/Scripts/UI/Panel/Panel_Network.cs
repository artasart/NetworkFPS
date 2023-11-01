using Framework.Network;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Network : Panel_Base
{
    private Button btn_Connect;
    private Button btn_Disconnect;
    
    private TMP_InputField inputField_ClientId;

	protected override void Awake()
    {
        base.Awake();

        btn_Connect = GetUI_Button(nameof(btn_Connect), OnClick_Connect);
        btn_Connect = GetUI_Button(nameof(btn_Disconnect), OnClick_Disconnect);

        inputField_ClientId = transform.Search(nameof(inputField_ClientId)).GetComponent<TMP_InputField>();
        inputField_ClientId.placeholder.GetComponent<TMP_Text>().text = Util.GenerateRandomString(5);
    }

	private void OnClick_Connect()
    {
        string clientId = inputField_ClientId.text;

        if (string.IsNullOrEmpty(clientId))
        {
            clientId = inputField_ClientId.placeholder.GetComponent<TMP_Text>().text;
        }

        GameClientManager.Instance.Connect(clientId);
    }

    private void OnClick_Disconnect()
    {
        GameClientManager.Instance.Disconnect();
    }

    override public void OnTop()
    {
    }
}
