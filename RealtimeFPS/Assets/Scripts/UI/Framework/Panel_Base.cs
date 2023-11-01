using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Panel_Base : UI_Base
{
	protected override void Awake()
	{
		base.Awake();
				
		CloseTabAll();
	}

	public void Show(bool _show, float _lerpSpeed = 1f, bool _isInstant = false) 
	{
		if (_isInstant)
		{
			this.GetComponent<CanvasGroup>().alpha = _show ? 1f : 0f;
			this.GetComponent<CanvasGroup>().blocksRaycasts = _show;

			return;
		}

		Timing.RunCoroutine(Co_Show(_show, _lerpSpeed, _isInstant), nameof(Co_Show) + this.GetHashCode());
	}

	private IEnumerator<float> Co_Show(bool _isShow, float _lerpSpeed = 1f, bool _isInstant = false)
	{
		var canvasGroup = this.GetComponent<CanvasGroup>();
		var targetAlpha = _isShow ? 1f : 0f;
		var lerpvalue = 0f;
		var lerpspeed = _lerpSpeed;

		if (!_isShow) canvasGroup.blocksRaycasts = false;

		while (Mathf.Abs(canvasGroup.alpha - targetAlpha) >= 0.001f)
		{
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, lerpvalue += lerpspeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		canvasGroup.alpha = targetAlpha;

		if (_isShow) canvasGroup.blocksRaycasts = true;
	}

    virtual public void OnTop() { }

    virtual public void OnHide() { }
}