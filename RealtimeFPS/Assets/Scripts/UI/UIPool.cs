using MEC;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static EasingFunction;

public class UIPool : ObjectPool
{
	public int poolSize = 50;

	List<GameObject> pool = new List<GameObject>();
	private Dictionary<string, List<GameObject>> activeObject = new Dictionary<string, List<GameObject>>();

	void Awake()
    {
		//CreatePool(poolSize);
	}

	private void CreatePool(int _poolSize)
	{
		//var parent = this.transform.Search("group_Damage");
		//var prefab = Resources.Load<GameObject>(Define.PATH_UI + Define.DAMAGEUI);

		//for (int i = 0; i < _poolSize; i++)
		//{
		//	pool.Add(CreateUIObject(prefab, parent));
		//}
	}

	public GameObject GetPool()
	{
		foreach (var item in pool)
		{
			if (!item.activeInHierarchy)
			{
				return item;
			}
		}

		var parent = this.transform.Search("group_Damage");
		var prefab = Resources.Load<GameObject>(Define.PATH_UI + Define.DAMAGEUI);

		GameObject effect = CreateUIObject(prefab, parent);

		return effect;
	}

	private GameObject CreateUIObject(GameObject _prefab, Transform _parent)
	{
		GameObject item = Instantiate(_prefab, _parent);

		item.GetComponent<TMP_Text>().color = new Color(
			item.GetComponent<TMP_Text>().color.r,
			item.GetComponent<TMP_Text>().color.g,
			item.GetComponent<TMP_Text>().color.b,
			0f);

		pool.Add(item);

		return item;
	}

	public void ShowUI(Transform _target, string _text, Color _color)
	{
		GameObject go_Damage = GetPool();

		go_Damage.GetComponent<TMP_Text>().text = _text;
		go_Damage.GetComponent<TMP_Text>().color = _color;
		go_Damage.SetActive(true);

		go_Damage.transform.position = Camera.main.WorldToScreenPoint(_target.position + Vector3.up * 2.25f);

		Timing.RunCoroutine(Co_FadeOut(go_Damage, _target), (int)CoroutineLayer.Game);
	}

	public void ShowUI(Vector3 _target, string _text, Color _color)
	{
		GameObject go_Damage = GetPool();

		go_Damage.GetComponent<TMP_Text>().text = _text;
		go_Damage.GetComponent<TMP_Text>().color = _color;
		go_Damage.SetActive(true);

		go_Damage.transform.position = Camera.main.WorldToScreenPoint(_target + Vector3.up * 2.25f);

		Timing.RunCoroutine(Co_ShowUI(go_Damage));
	}

	IEnumerator<float> Co_ShowUI(GameObject _damage)
	{
		GameManager.UI.FadeMaskableGrahpic(_damage.GetComponent<TMP_Text>(), 1f, 1.25f);

		yield return Timing.WaitUntilTrue(() => _damage.GetComponentInChildren<TMP_Text>().alpha >= 0);
		yield return Timing.WaitForSeconds(.1f);

		GameManager.UI.FadeMaskableGrahpic(_damage.GetComponentInChildren<TMP_Text>(), 0f, 1f);

		yield return Timing.WaitUntilTrue(() => _damage.GetComponentInChildren<TMP_Text>().alpha <= 0);

		_damage.GetComponent<TMP_Text>().text = string.Empty;
		_damage.GetComponent<TMP_Text>().color = Color.white;
		_damage.GetComponent<RectTransform>().position = Vector3.zero;
	}



	public void ShowUI(Transform _target, int _amount)
	{
		//GameObject go_Damage = GetPool();

		//var position = Camera.main.WorldToScreenPoint(_target.position + Vector3.up * 2.25f);

		//if (go_Damage != null)
		//{
		//	go_Damage.transform.position = position + Vector3.right * Random.Range(-10, 10);

		//	go_Damage.GetComponent<TMP_Text>().text = _amount.ToString();

		//	go_Damage.SetActive(true);

  //          Timing.RunCoroutine(Co_FadeOut(go_Damage, _target));

		//	Timing.RunCoroutine(Co_TrackObject(go_Damage.transform, _target), Define.DAMAGEUI + _target.GetHashCode());

		//	if(activeObject.ContainsKey(Define.DAMAGEUI + _target.GetHashCode()))
		//	{
		//		activeObject[Define.DAMAGEUI + _target.GetHashCode()].Add(go_Damage);
		//	}

		//	else
		//	{
		//		List<GameObject> list = new List<GameObject> { go_Damage };

		//		activeObject.Add(Define.DAMAGEUI + _target.GetHashCode(), list);
		//	}
		//}
	}

	private IEnumerator<float> Co_FadeOut(GameObject _damage, Transform _target = null)
	{
		float start = _damage.GetComponent<RectTransform>().position.y;
		float target = _damage.GetComponent<RectTransform>().position.y + 30;
		float elapsedTime = 0f;
		bool isFadeOut = false;

		_damage.transform.SetAsLastSibling();

		GameManager.UI.FadeMaskableGrahpic(_damage.GetComponent<TMP_Text>(), 1f, 1.25f);

		while (elapsedTime <= 1f)
		{
			Function function = GetEasingFunction(Ease.EaseOutQuad);

			float y = function(start, target, elapsedTime += 1f * Time.deltaTime);

			_damage.GetComponent<RectTransform>().position = new Vector3(_damage.GetComponent<RectTransform>().position.x, y, _damage.GetComponent<RectTransform>().position.z);

			if(elapsedTime >= .75f && !isFadeOut)
			{
				GameManager.UI.FadeMaskableGrahpic(_damage.GetComponentInChildren<TMP_Text>(), 0f, 1f);

				isFadeOut = true;
			}

			yield return Timing.WaitForOneFrame;
		}

		_damage.transform.position = new Vector3(_damage.GetComponent<RectTransform>().position.x, target, _damage.GetComponent<RectTransform>().position.z);



		yield return Timing.WaitUntilTrue(() => _damage.GetComponentInChildren<TMP_Text>().alpha <= 0);

		_damage.GetComponent<TMP_Text>().text = string.Empty;
		_damage.GetComponent<TMP_Text>().color = Color.white;
		_damage.GetComponent<RectTransform>().position = Vector3.zero;

		_damage.SetActive(false);

		if (_target == null) yield break;

		Timing.KillCoroutines(Define.DAMAGEUI + _target.GetHashCode());

		activeObject.Remove(Define.DAMAGEUI + _target.GetHashCode());
	}

	private IEnumerator<float> Co_TrackObject(Transform _mine, Transform _target)
	{
		while (true)
		{
			Vector3 position = Camera.main.WorldToScreenPoint(_target.position + Vector3.up * 2.25f);

			position.y = _mine.position.y;

			_mine.position = position;

			yield return Timing.WaitForSeconds(.02f);
		}
	}
	
	public void KillUI(Transform _target)
	{
		if (!activeObject.ContainsKey(Define.DAMAGEUI + _target.GetHashCode())) return;

		Timing.KillCoroutines(Define.DAMAGEUI + _target.GetHashCode());

		foreach(var item in activeObject[Define.DAMAGEUI + _target.GetHashCode()])
		{
			item.SetActive(false);
			item.transform.position = Vector3.zero;
		}

		activeObject.Remove(Define.DAMAGEUI + _target.GetHashCode());
	}
}
