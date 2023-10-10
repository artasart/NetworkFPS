using MEC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using static EasingFunction;

public static class Util
{
	public static Transform Search(this Transform _target, string _name)
	{
		if (_target.name == _name) return _target;

		for (int i = 0; i < _target.childCount; ++i)
		{
			var result = Search(_target.GetChild(i), _name);

			if (result != null) return result;
		}

		return null;
	}

	public static T GetComponentInSibilings<T>(Transform _transform) where T : Component
	{
		var parent = _transform.parent;

		return parent.GetComponentInChildren<T>();
	}


	public static IEnumerator<float> Co_Flik<T>(T _image, int _interval, float _lerpSpeed = 1f) where T : MaskableGraphic
	{
		float alpha = _image.color.a;

		for (int i = 0; i < _interval; i++)
		{
			CoroutineHandle handle = Timing.RunCoroutine(Co_FadeColor(_image, .25f, _lerpSpeed), Define.FLICK);

			yield return Timing.WaitUntilDone(handle);

			handle = Timing.RunCoroutine(Co_FadeColor(_image, alpha, _lerpSpeed), Define.FLICK);

			yield return Timing.WaitUntilDone(handle);
		}
	}

	public static IEnumerator<float> Co_FadeColor<T>(T _image, float _alpha, float _lerpSpeed = 1f) where T : MaskableGraphic
	{
		var targetColor = new Color(_image.color.r, _image.color.g, _image.color.b, _alpha);
		var lerpvalue = 0f;

		while (GetColorDistance(_image.color, targetColor) >= 0.001f)
		{
			_image.color = Color.Lerp(_image.color, targetColor, lerpvalue += _lerpSpeed * Time.deltaTime);

			yield return Timing.WaitForOneFrame;
		}

		_image.color = targetColor;
	}

	public static float GetColorDistance(Color _colorA, Color _colorB)
	{
		float r = _colorA.r - _colorB.r;
		float g = _colorA.g - _colorB.g;
		float b = _colorA.b - _colorB.b;
		float a = _colorA.a - _colorB.a;

		return Mathf.Sqrt(r * r + g * g + b * b + a * a);
	}

	public static string RGBToHex(int _red, int _green, int _blue)
	{
		string hex = "#" + _red.ToString("X2") + _green.ToString("X2") + _blue.ToString("X2");

		return hex;
	}

	public static Color RGBToColor(int _red, int _green, int _blue, int _alpha = 255)
	{
		return new Color((float)_red / 255, (float)_green / 255, (float)_blue / 255, (float)_alpha / 255);
	}

	public static void EaseScale(Transform _transform, float _scale, float _lerpSpeed = 1f, Ease _easeMode = Ease.EaseOutBack) => Timing.RunCoroutine(Co_SetLocalScale(_transform, _scale, _lerpSpeed, _easeMode), (int)CoroutineLayer.Util);
	
	private static IEnumerator<float> Co_SetLocalScale(Transform _transform, float _size, float _lerpSpeed, Ease _easeMode)
	{
		float lerpvalue = 0f;

		while (lerpvalue <= 1f)
		{
			Function function = GetEasingFunction(_easeMode);

			float x = function(_transform.localScale.x, _size, lerpvalue);
			float y = function(_transform.localScale.y, _size, lerpvalue);
			float z = function(_transform.localScale.z, _size, lerpvalue);

			lerpvalue += _lerpSpeed * Time.deltaTime;

			_transform.localScale = new Vector3(x, y, z);

			yield return Timing.WaitForOneFrame;
		}

		_transform.transform.localScale = Vector3.one * _size;
	}

	public static string ConvertAfterAtToLower(string input)
	{
		string pattern = @"(@.+)";

		Match match = Regex.Match(input, pattern);

		if (match.Success)
		{
			string matchText = match.Groups[1].Value;
			string convertedText = matchText.ToLower();

			return input.Replace(matchText, convertedText);
		}

		return input;
	}

	public static T String2Enum<T>(string _value)
	{
		try
		{
			return (T)Enum.Parse(typeof(T), _value, true);
		}

		catch (Exception)
		{
			return default(T);
		}
	}

	public static string Beautify(string jsonString)
	{
		try
		{
			string beautifiedJson = JValue.Parse(jsonString).ToString(Formatting.Indented);
			return beautifiedJson;
		}
		catch
		{
			return jsonString;
		}
	}

	public static string GenerateRandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		System.Random random = new();
		char[] randomString = new char[length];

		for (int i = 0; i < length; i++)
		{
			randomString[i] = chars[random.Next(chars.Length)];
		}

		return new string(randomString);
	}
}


public static class ButtonExtensions
{
	public static void UseAnimation(this Button button)
	{
		button.gameObject.AddComponent<ButtonAnimation>();
	}
}

public enum CoroutineLayer
{
	None,
	Util,
	Game,
	Login,
}