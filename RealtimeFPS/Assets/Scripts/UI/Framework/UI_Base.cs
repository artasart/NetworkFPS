using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Base : MonoBehaviour
{
	Dictionary<string, GameObject> childUI = new Dictionary<string, GameObject>();
	[HideInInspector] public bool isInstant = false;


	#region Initialize

	protected virtual void Awake()
	{
		FindAllChildUI();
	}

	private void FindAllChildUI() => SearchUI(this.transform);

	private void SearchUI(Transform _parent)
	{
		foreach (Transform child in _parent)
		{
			var tab = child.GetComponent<Tab_Base>();
			var item = child.gameObject.GetComponent<Item_Base>();

			if (tab != null || item != null)
			{
				childUI[child.name] = child.gameObject;
			}

			else
			{
				childUI[child.name] = child.gameObject;

				SearchUI(child);
			}
		}
	}

	#endregion



	#region Basic Methods

	protected TMP_Text GetUI_TMPText(string _hierarchyName, string _message)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var txtmp = childUI[_hierarchyName].GetComponent<TMP_Text>();
			txtmp.text = _message;

			return txtmp;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	protected Button GetUI_Button(string _hierarchyName, Action _action = null, Action _sound = null)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var button = childUI[_hierarchyName].GetComponent<Button>();

			if (_sound == null)
			{
				button.onClick.AddListener(PlaySound);
			}

			else button.onClick.AddListener(() => _sound?.Invoke());

			button.onClick.AddListener(() => _action?.Invoke());

			return button;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	protected Image GetUI_Image(string _hierarchyName, Sprite _sprite)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var image = childUI[_hierarchyName].GetComponent<Image>();
			
			image.sprite = _sprite;

			return image;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	protected Toggle GetUI_Toggle(string _hierarchyName, bool _isToggleOn)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var toggle = childUI[_hierarchyName].GetComponent<Toggle>();
			toggle.isOn = _isToggleOn;

			return toggle;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	protected Slider GetUI_Slider(string _hierarchyName, Action<float> _action = null)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var slider = childUI[_hierarchyName].GetComponent<Slider>();

			slider.onValueChanged.AddListener((value) => _action?.Invoke(value));

			return slider;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	protected TMP_InputField GetUI_TMPInputField(string _hierarchyName, Action<string> _action = null)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var inputField = childUI[_hierarchyName].GetComponent<TMP_InputField>();
			var placeHolder = inputField.placeholder.GetComponent<TextMeshProUGUI>().text;

			inputField.onValueChanged.AddListener((value) => _action?.Invoke(value));

			inputField.onSelect.AddListener((value) =>
			{
				if (placeHolder != string.Empty)
				{
					inputField.placeholder.GetComponent<TextMeshProUGUI>().text = string.Empty;
				}
			});

			inputField.onDeselect.AddListener((value) =>
			{
				if (inputField.text == string.Empty)
				{
					inputField.placeholder.GetComponent<TextMeshProUGUI>().text = placeHolder;
				}
			});

			return inputField;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	protected ScrollRect GetUI_ScrollRect(string _hierarchyName, Action<Vector2> _action = null)
	{
		if (childUI.ContainsKey(_hierarchyName))
		{
			var scrollRect = childUI[_hierarchyName].GetComponent<ScrollRect>();

			scrollRect.onValueChanged.AddListener((position) => _action?.Invoke(position));

			return scrollRect;
		}

		else { Debug.Log($"WARNING : {_hierarchyName} is not in this hierarchy."); return null; }
	}

	#endregion



	#region Utils

	protected void ChangeTab(string _hierarchyName)
	{
		CloseTabAll();

		childUI[_hierarchyName].SetActive(true);
	}

	protected void CloseTab(string _hierarchyName)
	{
		childUI[_hierarchyName].SetActive(false);
	}

	protected void ChangeTab<T>() where T : Component
	{
		CloseTabAll();

		if (childUI.ContainsKey(typeof(T).Name))
		{
			childUI[typeof(T).Name].SetActive(true);
		}

		else Debug.Log($"WARNING: {typeof(T).Name} not found.");
	}

	protected void CloseTab<T>() where T : Component
	{
		if (childUI.ContainsKey(typeof(T).Name))
		{
			childUI[typeof(T).Name].SetActive(false);
		}

		else Debug.Log($"WARNING: {typeof(T).Name} not found.");
	}

	protected void CloseTabAll()
	{
		childUI.Values
			.Where(uiObject => uiObject.GetComponent<Tab_Base>() != null)
			.ToList()
			.ForEach(tabObject => tabObject.SetActive(false));
	}


	protected void PlaySound() => GameManager.Sound.PlaySound("Click_2");

	protected void Display()
	{
		foreach (var item in childUI)
		{
			Debug.Log($"Key: {item.Key}, Value: {item.Value}");
		}
	}

	#endregion
}