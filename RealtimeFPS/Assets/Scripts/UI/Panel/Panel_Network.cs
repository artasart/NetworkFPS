using Framework.Network;
using Protocol;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Network : Panel_Base
{
    private TMP_InputField inputField_ClientId;

    private Button btn_Connect;
    private Button btn_Disconnect;
    
    private Button btn_Ready;
    private Button btn_UnReady;

	protected override void Awake()
    {
        base.Awake();

        inputField_ClientId = transform.Search(nameof(inputField_ClientId)).GetComponent<TMP_InputField>();
        inputField_ClientId.placeholder.GetComponent<TMP_Text>().text = Util.GenerateRandomString(5);

        btn_Connect = GetUI_Button(nameof(btn_Connect), OnClick_Connect);
        btn_Disconnect = GetUI_Button(nameof(btn_Disconnect), OnClick_Disconnect);

        btn_Ready = GetUI_Button(nameof(btn_Ready), OnClick_Ready);
        btn_UnReady = GetUI_Button(nameof(btn_UnReady), OnClick_UnReady);
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

    private void OnClick_Ready()
    {
        Protocol.C_FPS_READY ready = new Protocol.C_FPS_READY();
        ready.IsReady = true;
        NetworkManager.Instance.Client.Send(PacketManager.MakeSendBuffer(ready));

        SetReadyButtonState(false);
        SetUnReadyButtonState(true);
    }

    private void OnClick_UnReady()
    {
        Protocol.C_FPS_READY ready = new Protocol.C_FPS_READY();
        ready.IsReady = false;
        NetworkManager.Instance.Client.Send(PacketManager.MakeSendBuffer(ready));

        SetUnReadyButtonState(false);
        SetReadyButtonState(true);
    }

    public void SetReadyButtonState(bool state)
    {
        btn_Ready.interactable = state;
    }

    public void SetUnReadyButtonState(bool state)
    {
        btn_UnReady.interactable = state;
    }

    override public void OnOpen()
    {
        var Client = NetworkManager.Instance.Client;

        SetReadyButtonState(false);
        SetUnReadyButtonState(false);

        if(Client == null)
        {
            SetConnetButtonState(true);
            SetDisconnectButtonState(false);
        }
        else
        {
            SetConnetButtonState(false);
            SetDisconnectButtonState(true);

            SetReadyButtonState(true);
        }
    }
}
