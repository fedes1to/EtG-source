using System;
using UnityEngine;

public class WaftingDebrisObject : DebrisObject
{
	[Header("Waft Properties")]
	public string waftAnimationName;

	public Vector2 initialBurstDuration = new Vector2(0.3f, 0.45f);

	public Vector2 waftDuration = new Vector2(2f, 4.5f);

	public Vector2 waftDistance = new Vector2(1.5f, 3.5f);

	private bool m_initialized;

	private Vector3 m_cachedInitialVelocity;

	private float m_peakElapsed;

	private float m_peakDuration;

	private bool m_hasHitPeak;

	private float m_waftElapsed;

	private float m_waftPeriod;

	private float m_waftDistance;

	private int m_coplanarSign;

	protected override void UpdateVelocity(float adjustedDeltaTime)
	{
		if (!m_initialized)
		{
			m_initialized = true;
			m_cachedInitialVelocity = m_velocity;
			m_peakDuration = Mathf.Lerp(initialBurstDuration.x, initialBurstDuration.y, UnityEngine.Random.value);
		}
		if (!(m_currentPosition.z > 0f))
		{
			return;
		}
		if (!m_hasHitPeak)
		{
			m_peakElapsed += adjustedDeltaTime;
			float t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, m_peakElapsed / m_peakDuration);
			m_velocity = Vector3.Lerp(Vector3.Scale(m_cachedInitialVelocity, new Vector3(2.5f, 2.5f, 4f)), new Vector3(m_cachedInitialVelocity.x * 0.5f, m_cachedInitialVelocity.y * 0.5f, 0f), t);
			if (m_velocity.z <= 0f)
			{
				m_hasHitPeak = true;
				m_waftPeriod = Mathf.Lerp(waftDuration.x, waftDuration.y, UnityEngine.Random.value);
				m_waftDistance = Mathf.Lerp(waftDistance.x, waftDistance.y, UnityEngine.Random.value);
				m_coplanarSign = ((UnityEngine.Random.value > 0.5f) ? 1 : (-1));
				if (UnityEngine.Random.value < 0.5f)
				{
					m_waftElapsed = m_waftPeriod / 2f;
				}
				else
				{
					m_waftElapsed = 0f;
				}
			}
			return;
		}
		m_waftElapsed += adjustedDeltaTime;
		float num = m_waftElapsed % m_waftPeriod;
		float num2 = Mathf.Cos(num / m_waftPeriod * 2f * (float)Math.PI);
		float num3 = Mathf.Sin(num / m_waftPeriod * 2f * (float)Math.PI);
		float num4 = Mathf.Lerp(m_velocity.z, m_velocity.z + 4f * adjustedDeltaTime, Mathf.Abs(num2));
		num4 += -3f * adjustedDeltaTime;
		m_velocity = new Vector3(m_waftDistance * num2, m_waftDistance / 5f * num3 * (float)m_coplanarSign, num4);
		if (!string.IsNullOrEmpty(waftAnimationName))
		{
			tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(waftAnimationName);
			if (clipByName != base.spriteAnimator.CurrentClip)
			{
				base.spriteAnimator.Play(waftAnimationName);
				base.spriteAnimator.Stop();
			}
			float num5 = (m_waftElapsed + 0.5f * m_waftPeriod) % m_waftPeriod;
			float num6 = Mathf.PingPong(num5 / m_waftPeriod * 2f, 1f);
			int frame = Mathf.Clamp(Mathf.FloorToInt((float)clipByName.frames.Length * num6), 0, clipByName.frames.Length - 1);
			base.spriteAnimator.SetFrame(frame);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
