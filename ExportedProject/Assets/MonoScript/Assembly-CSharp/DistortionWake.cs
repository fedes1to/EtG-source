using System;
using System.Collections.Generic;
using UnityEngine;

public class DistortionWake : BraveBehaviour
{
	public float maxLength = 3f;

	public float initialIntensity;

	public float maxIntensity = 1f;

	public float initialRadius;

	public float maxRadius = 0.5f;

	public float initialOffset;

	public float offsetVariance;

	public float offsetVarianceSpeed = 1f;

	[NonSerialized]
	private Material m_material;

	[NonSerialized]
	private List<Vector2> m_positions = new List<Vector2>();

	private void Start()
	{
		m_material = new Material(ShaderCache.Acquire("Brave/Internal/DistortionLine"));
		m_material.SetVector("_WavePoint1", CalculateSettings(base.specRigidbody.UnitCenter, 0f));
		m_material.SetVector("_WavePoint2", CalculateSettings(base.specRigidbody.UnitCenter, 0f));
		m_material.SetFloat("_DistortProgress", initialOffset);
		Pixelator.Instance.RegisterAdditionalRenderPass(m_material);
	}

	private Vector4 CalculateSettings(Vector2 worldPoint, float t)
	{
		Vector3 vector = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(worldPoint.ToVector3ZUp());
		return new Vector4(vector.x, vector.y, Mathf.Lerp(initialRadius, maxRadius, t), Mathf.Lerp(initialIntensity, maxIntensity, t));
	}

	private void LateUpdate()
	{
		m_positions.Add(base.specRigidbody.UnitCenter);
		if (m_positions.Count == 1)
		{
			return;
		}
		if (m_positions.Count != 2)
		{
			Vector2 a = m_positions[m_positions.Count - 1];
			while (Vector2.Distance(a, m_positions[1]) > maxLength)
			{
				m_positions.RemoveAt(0);
			}
		}
		m_material.SetVector("_WavePoint1", CalculateSettings(m_positions[m_positions.Count - 1], 0f));
		float num = Vector2.Distance(m_positions[m_positions.Count - 1], m_positions[0]);
		m_material.SetVector("_WavePoint2", CalculateSettings(m_positions[0], Mathf.Clamp01(num / maxLength)));
		float num2 = initialOffset;
		if (offsetVariance > 0f)
		{
			num2 += Mathf.Sin(Time.realtimeSinceStartup * offsetVarianceSpeed) * offsetVariance;
		}
		m_material.SetFloat("_DistortProgress", num2);
	}

	protected override void OnDestroy()
	{
		if (Pixelator.HasInstance && (bool)m_material)
		{
			Pixelator.Instance.DeregisterAdditionalRenderPass(m_material);
		}
		if ((bool)m_material)
		{
			UnityEngine.Object.Destroy(m_material);
		}
	}
}
