using System.Collections;
using UnityEngine;

public class SpritePulser : BraveBehaviour
{
	public float duration = 1f;

	public float minDuration = 0.3f;

	public float maxDuration = 2.9f;

	public float metaDuration = 6f;

	public float minAlpha = 0.3f;

	public float minScale = 0.9f;

	public float maxScale = 1.1f;

	private bool m_active;

	private void Start()
	{
		if (base.sprite == null)
		{
			Debug.LogError("No sprite on SpritePulser!", this);
		}
	}

	private void Update()
	{
		if (m_active)
		{
			float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.realtimeSinceStartup, duration) / duration);
			Color color = base.sprite.color;
			color.a = Mathf.Lerp(minAlpha, 1f, t);
			base.sprite.color = color;
		}
	}

	private void OnBecameVisible()
	{
		m_active = true;
	}

	private void OnBecameInvisible()
	{
		m_active = false;
	}

	private IEnumerator Pulse()
	{
		while (true)
		{
			if (m_active)
			{
				float t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.realtimeSinceStartup, duration) / duration);
				Color color = base.sprite.color;
				color.a = Mathf.Lerp(minAlpha, 1f, t);
				base.sprite.color = color;
			}
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
