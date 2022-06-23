using System;
using UnityEngine;

public class BundleOfWandsLightController : MonoBehaviour
{
	public Color baseColor;

	private Light m_light;

	private void Start()
	{
		m_light = GetComponent<Light>();
	}

	private Vector3 shift_col(Vector3 RGB, Vector3 shift)
	{
		Vector3 result = new Vector3(RGB.x, RGB.y, RGB.z);
		float num = shift.z * shift.y * Mathf.Cos(shift.x * (float)Math.PI / 180f);
		float num2 = shift.z * shift.y * Mathf.Sin(shift.x * (float)Math.PI / 180f);
		result.x = (0.299f * shift.z + 0.701f * num + 0.168f * num2) * RGB.x + (0.587f * shift.z - 0.587f * num + 0.33f * num2) * RGB.y + (0.114f * shift.z - 0.114f * num - 0.497f * num2) * RGB.z;
		result.y = (0.299f * shift.z - 0.299f * num - 0.328f * num2) * RGB.x + (0.587f * shift.z + 0.413f * num + 0.035f * num2) * RGB.y + (0.114f * shift.z - 0.114f * num + 0.292f * num2) * RGB.z;
		result.z = (0.299f * shift.z - 0.3f * num + 1.25f * num2) * RGB.x + (0.587f * shift.z - 0.588f * num - 1.05f * num2) * RGB.y + (0.114f * shift.z + 0.886f * num - 0.203f * num2) * RGB.z;
		return result;
	}

	private void Update()
	{
		Vector3 rGB = new Vector3(baseColor.r, baseColor.g, baseColor.b);
		Vector3 vector = shift_col(rGB, new Vector3(1.5f * Time.time * 360f, 1f, 1.5f));
		m_light.color = new Color(vector.x, vector.y, vector.z);
	}
}
