using Cinemachine;
using MEC;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineSwitcher
{
	private static CinemachineVirtualCamera current = null;
	private static CoroutineHandle handle_match;

	private static bool isSwitched = false;

	public static CinemachineVirtualCamera GetCurrentCamera()
    {
		return current;
	}
	
	public static void SwitchCamera(CinemachineVirtualCamera _camera, CinemachineBlendDefinition.Style _blendStyle = CinemachineBlendDefinition.Style.EaseInOut, float _speed = 2f)
	{
		SetWatchSpeed(_blendStyle, _speed);

		if (current == null)
		{
			current = _camera;
			current.Priority = 1;
		}

		else
		{
			_camera.Priority = 1;

			if(_camera != current)
			{
				current.Priority = 0;
				current = _camera;
			}
		}

		Timing.KillCoroutines(handle_match);

		handle_match = Timing.RunCoroutine(Co_MatchCamera(_camera), handle_match.GetHashCode());
	}

	public static void SwitchMainCamera(CinemachineBlendDefinition.Style _blendStyle = CinemachineBlendDefinition.Style.Cut, float _speed = 2f)
	{
		//SetWatchSpeed(_blendStyle, _speed);

		//if (current != null)
		//{
		//	current.Priority = 0;
		//	current = null;
		//}

		//var virtualCam = CrystalController.Instance.virtualCamera;
		//virtualCam.Priority = 1;

		//Timing.KillCoroutines(handle_match);

		//handle_match = Timing.RunCoroutine(Co_MatchCamera(virtualCam), handle_match.GetHashCode());
	}

	public static void SetWatchSpeed(CinemachineBlendDefinition.Style _blendStyle, float _speed)
	{
		//CrystalController.Instance.cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(_blendStyle, _speed);
	}

	private static IEnumerator<float> Co_MatchCamera(CinemachineVirtualCamera _current)
	{
		isSwitched = false;

		yield return Timing.WaitUntilTrue(() => Equals(Camera.main.transform.position, _current.transform.position));

		isSwitched = true;

		SetWatchSpeed(CinemachineBlendDefinition.Style.EaseInOut, 2f);
	}

	public static void FadeWatchSpeed(CinemachineBlendDefinition.Style _blendStyle, float _speed, float _lerpSpeed = 1f)
	{
		Timing.RunCoroutine(Co_FadeWatchSpeed(_blendStyle, _speed));
	}

	private static IEnumerator<float> Co_FadeWatchSpeed(CinemachineBlendDefinition.Style _blendStyle, float _speed, float _lerpSpeed = 1f)
	{
		float lerpValue = 0f;

		float current = _speed;
		float target = _speed * 2f;

		while (!Equals(current, target))
		{
			current = Mathf.Lerp(current, target, lerpValue += _lerpSpeed * Time.deltaTime);

			//CrystalController.Instance.cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(_blendStyle, current);

			yield return Timing.WaitForOneFrame;
		}

		//CrystalController.Instance.cinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(_blendStyle, target);
	}



	public static bool IsSwitched()
	{
		return isSwitched;
	}
}
