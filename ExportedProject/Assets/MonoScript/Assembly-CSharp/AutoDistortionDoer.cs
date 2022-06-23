using System.Collections;
using UnityEngine;

public class AutoDistortionDoer : BraveBehaviour
{
	public float Intensity = 0.25f;

	public float Width = 0.125f;

	public float Radius = 5f;

	public float Duration = 1f;

	public float DelayTime = 0.25f;

	private bool m_triggered;

	private void Start()
	{
		OnSpawned();
	}

	private void OnSpawned()
	{
		if (!m_triggered)
		{
			Vector2 centerPoint = ((!base.sprite) ? base.transform.position.XY() : base.sprite.WorldCenter);
			StartCoroutine(Distort(centerPoint));
			m_triggered = true;
		}
	}

	private IEnumerator Distort(Vector2 centerPoint)
	{
		yield return new WaitForSeconds(DelayTime);
		Exploder.DoDistortionWave(centerPoint, Intensity, Width, Radius, Duration);
	}

	private void OnDespawned()
	{
		m_triggered = false;
	}
}
