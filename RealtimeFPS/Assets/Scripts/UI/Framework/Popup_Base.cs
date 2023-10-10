using System;
using UnityEngine;

public class Popup_Base : UI_Base
{
	public Action callback_confirm;
	public Action callback_cancel;

	protected override void Awake()
	{
		base.Awake();

		GameObject group_Modal = this.transform.Search(nameof(group_Modal))?.gameObject;

		if (group_Modal != null) group_Modal.GetComponent<RectTransform>().localScale = Vector3.zero;

		CloseTabAll();
	}

	protected virtual void OnClick_Confirm()
	{
		GameManager.UI.PopPopup();

		callback_confirm?.Invoke();
	}

	protected virtual void OnClick_Close()
	{
		GameManager.UI.PopPopup();

		callback_cancel?.Invoke();
	}
}
