using System;
using FullInspector;
using UnityEngine;

[Serializable]
public class ScreenShakeSettings : fiInspectorOnly
{
	public enum VibrationType
	{
		None = 0,
		Auto = 10,
		Simple = 20
	}

	public static float GLOBAL_SHAKE_MULTIPLIER = 1f;

	public float magnitude;

	public float speed;

	public float time;

	public float falloff;

	public Vector2 direction;

	public VibrationType vibrationType = VibrationType.Auto;

	[InspectorShowIf("ShowSimpleVibrationParams")]
	[InspectorIndent]
	public Vibration.Time simpleVibrationTime = Vibration.Time.Normal;

	[InspectorIndent]
	[InspectorShowIf("ShowSimpleVibrationParams")]
	public Vibration.Strength simpleVibrationStrength = Vibration.Strength.Medium;

	public ScreenShakeSettings()
	{
		magnitude = 0.35f;
		speed = 6f;
		time = 0.06f;
		falloff = 0f;
	}

	public ScreenShakeSettings(float mag, float spd, float tim, float foff)
	{
		magnitude = mag;
		speed = spd;
		time = tim;
		falloff = foff;
		direction = Vector2.zero;
	}

	public ScreenShakeSettings(float mag, float spd, float tim, float foff, Vector2 dir)
	{
		magnitude = mag;
		speed = spd;
		time = tim;
		falloff = foff;
		direction = dir;
	}

	public bool ShowSimpleVibrationParams()
	{
		return vibrationType == VibrationType.Simple;
	}
}
