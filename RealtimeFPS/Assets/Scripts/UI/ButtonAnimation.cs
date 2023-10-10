using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using MEC;
using static EasingFunction;

public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Ease easeFunction = Ease.EaseOutBack;
    public float targetScaleY = .95f;

    private Button button;

    CoroutineHandle handle_Button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

	public void OnPointerDown(PointerEventData eventData)
	{
        Timing.KillCoroutines(handle_Button);

		handle_Button = Timing.RunCoroutine(Co_SizeTransform(targetScaleY));
	}

	public void OnPointerUp(PointerEventData eventData)
	{
        Timing.KillCoroutines(handle_Button);

        handle_Button = Timing.RunCoroutine(Co_SizeTransform(1f));
	}

	private IEnumerator<float> Co_SizeTransform(float _size)
    {
        Vector3 current = button.GetComponent<RectTransform>().localScale;

        float lerpvalue = 0f;

        while (lerpvalue <= 1f)
        {
            Function function = GetEasingFunction(easeFunction);

            float x = function(current.x, _size, lerpvalue);
            float y = function(current.y, _size, lerpvalue);
            float z = function(current.z, _size, lerpvalue);

            lerpvalue += 3f * Time.deltaTime;

            button.GetComponent<RectTransform>().localScale = new Vector3(x, y, z);

            yield return Timing.WaitForOneFrame;
        }

        button.GetComponent<RectTransform>().localScale = Vector3.one * _size;
    }
}