using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Setting : UI_Base
{
	Button btn_Disconnect;

	protected override void Awake()
	{
		base.Awake();

		btn_Disconnect = GetUI_Button(nameof(btn_Disconnect), OnClick_Disconnect);
	}

	private void OnClick_Disconnect()
	{
		GameClientManager.Instance.Disconnect();

		GameManager.UI.PopPanel();
		GameManager.UI.PopPanel();
		GameManager.UI.StackPanel<Panel_Network>();
	}
}
