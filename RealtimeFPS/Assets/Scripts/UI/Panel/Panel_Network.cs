using Framework.Network;
using Protocol;
using System;
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
        btn_Disconnect = GetUI_Button(nameof(btn_Disconnect), OnClick_Disconnect);

        SetDisconnectButtonState(false);

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

        NetworkManager.Instance.Connect(clientId);
    }

    public void SetConnetButtonState(bool state)
    {
        btn_Connect.interactable = state;
    }

    private void OnClick_Disconnect()
    {
        NetworkManager.Instance.Disconnect();
    }

    public void SetDisconnectButtonState(bool state)
    {
        btn_Disconnect.interactable = state;
    }

    override public void OnOpen()
    {
    }
}
