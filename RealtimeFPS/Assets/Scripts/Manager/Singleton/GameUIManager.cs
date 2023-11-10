using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EasingFunction;

public class GameUIManager : SingletonManager<GameUIManager>
{
	#region Members

	class Panel
	{
		public GameObject GameObject;
		public bool IsOpen;
	}

	[NonReorderable] Dictionary<string, Panel> panels = new Dictionary<string, Panel>();
	[NonReorderable] Dictionary<string, GameObject> popups = new Dictionary<string, GameObject>();

	Stack<string> openPopups = new Stack<string>();

	GameObject group_MasterCanvas;
	GameObject group_Panel;

	public Canvas MasterCanvas { get => group_MasterCanvas.GetComponent<Canvas>(); }

	#endregion

	#region Initialize

	private void Awake()
	{
		group_MasterCanvas = GameObject.Find("go_Canvas");

		group_Panel = GameObject.Find(nameof(group_Panel));

		CacheUI(group_Panel, panels);
	}

    private void OnDestroy()
    {
        panels.Clear();
    }

    private void CacheUI(GameObject _parent, Dictionary<string, Panel> _objects )
	{
		for (int i = 0; i < _parent.transform.childCount; i++)
		{
			var child = _parent.transform.GetChild(i);
			var name = child.name;

			if (_objects.ContainsKey(name))
			{
				DebugManager.Log($"Same Key is registered in {_parent.name}", DebugColor.UI);
				continue;
			}

			child.gameObject.SetActive(true);
			child.gameObject.GetComponent<CanvasGroup>().alpha = 0f;
			child.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
			child.gameObject.SetActive(false);

			_objects[name] = new Panel()
            {
                GameObject = child.gameObject,
                IsOpen = false
            };
		}
	}

	#endregion

	#region Core Methods

	public T FetchPanel<T>() where T : Component
	{
        if (!panels.ContainsKey(typeof(T).ToString())) return null;

        return panels[typeof(T).ToString()].GameObject.GetComponent<T>();
	}

	public void OpenPanel<T>() where T : Component
	{
        string panelName = typeof(T).ToString();

        if (panels.TryGetValue(panelName, out Panel panel))
        {
            if (panel.IsOpen)
                return;

            panel.GameObject.GetComponent<Panel_Base>().OnOpen();
            panel.IsOpen = true;
            ShowPanel(panels[panelName].GameObject, true);
        }
    }

	public void ClosePanel<T>() where T : Component
	{
        string panelName = typeof(T).ToString();

        if (panels.TryGetValue(panelName, out Panel panel))
        {
            if (!panel.IsOpen)
                return;

            panel.GameObject.GetComponent<Panel_Base>().OnClose();
            panel.IsOpen = false;
            ShowPanel(panels[panelName].GameObject, false);
        }
    }

	public void TogglePanel<T>() where T : Component
	{
        string panelName = typeof(T).ToString();

        if (panels.TryGetValue(panelName, out Panel panel))
        {
            panel.IsOpen = !panel.IsOpen;

            if (panel.IsOpen)
                panel.GameObject.GetComponent<Panel_Base>().OnOpen();
            else
                panel.GameObject.GetComponent<Panel_Base>().OnClose();

            ShowPanel(panels[panelName].GameObject, panel.IsOpen);
        }
    }

	public void PopPopup(bool _instant = false)
	{
		if (openPopups.Count <= 0) return;

		var popupName = openPopups.Pop();

		if (_instant)
		{
			popups[popupName].SetActive(false);
			popups[popupName].GetComponent<CanvasGroup>().alpha = 0f;
			popups[popupName].GetComponent<CanvasGroup>().blocksRaycasts = false;
		}

		else ShowPanel(popups[popupName], false);

		DebugManager.Log($"Pop: {popupName}", DebugColor.UI);
	}

	#endregion

	#region Basic Methods

	public void ShowPanel(GameObject _gameObject, bool _isShow)
	{
		Timing.RunCoroutine(Co_Show(_gameObject, _isShow, 1.5f), _gameObject.GetHashCode());
	}

	private IEnumerator<float> Co_Show(GameObject _gameObject, bool _isShow, float _lerpspeed = 1f)
	{
		var canvasGroup = _gameObject.GetComponent<CanvasGroup>();
		var targetAlpha = _isShow ? 1f : 0f;
		var lerpvalue = 0f;
		var lerpspeed = _lerpspeed;

		if (!_isShow) canvasGroup.blocksRaycasts = false;
		else _gameObject.SetActive(true);

		while (Mathf.Abs(canvasGroup.alpha - targetAlpha) >= 0.001f)
		{
			canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, lerpvalue += lerpspeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		canvasGroup.alpha = targetAlpha;

		if (_isShow) canvasGroup.blocksRaycasts = true;
		else _gameObject.SetActive(false);
	}

	public void FadeMaskableGrahpic<T>(T _current, float _target, float _lerpspeed = 1f, float _delay = 0f, Action _start = null, Action _end = null) where T : MaskableGraphic
	{
		Timing.RunCoroutine(Co_FadeMaskableGraphic(_current, _target, _lerpspeed, _start, _end).Delay(_delay), _current.GetHashCode().ToString()); ;
	}

	private IEnumerator<float> Co_FadeMaskableGraphic<T>(T _current, float _target, float _lerpspeed = 1f, Action _start = null, Action _end = null) where T : MaskableGraphic
	{
		float lerpvalue = 0f;

		Color target = new Color(_current.color.r, _current.color.g, _current.color.b, _target);

		_start?.Invoke();

		_current.raycastTarget = true;

		while (Mathf.Abs(_current.color.a - _target) >= 0.001f)
		{
			_current.color = Color.Lerp(_current.color, target, lerpvalue += _lerpspeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		_current.color = target;

		_end?.Invoke();

		yield return Timing.WaitForOneFrame;
	}

	#endregion
}