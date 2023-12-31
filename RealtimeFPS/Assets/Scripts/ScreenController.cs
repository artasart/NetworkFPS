using UnityEngine;

public class ScreenController : MonoBehaviour
{
	private bool isFullscreen = true;

	private void Awake()
	{
		Screen.SetResolution(960, 540, false);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
		{
			ToggleFullscreenMode();
		}
	}

	private void ToggleFullscreenMode()
	{
		isFullscreen = !isFullscreen;

		Screen.fullScreen = isFullscreen;
	}
}