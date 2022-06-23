using System.Collections;
using UnityEngine;

public class ProjectileTrailRendererController : BraveBehaviour
{
	public TrailRenderer trailRenderer;

	public CustomTrailRenderer customTrailRenderer;

	public float desiredLength;

	private float? m_previousTrailSpeed;

	private bool isStopping;

	public void Awake()
	{
		m_previousTrailSpeed = null;
		base.projectile.TrailRendererController = this;
	}

	public void Start()
	{
		TryUpdateTrailLength();
	}

	public void LateUpdate()
	{
		TryUpdateTrailLength();
	}

	public void OnSpawned()
	{
		if ((bool)customTrailRenderer)
		{
			customTrailRenderer.Reenable();
		}
		TryUpdateTrailLength();
	}

	public void OnDespawned()
	{
		m_previousTrailSpeed = null;
		if ((bool)customTrailRenderer)
		{
			customTrailRenderer.Clear();
			isStopping = false;
			StopAllCoroutines();
		}
	}

	public void Stop()
	{
		if ((bool)customTrailRenderer)
		{
			StartCoroutine(StopGracefully());
		}
	}

	private IEnumerator StopGracefully()
	{
		isStopping = true;
		float startLifetime = customTrailRenderer.lifeTime;
		float endLifetime = 0f;
		float timer = 0f;
		for (float duration = 1f; timer < duration; timer += BraveTime.DeltaTime)
		{
			customTrailRenderer.lifeTime = Mathf.Lerp(startLifetime, endLifetime, timer / duration);
			yield return null;
		}
		customTrailRenderer.lifeTime = endLifetime;
		customTrailRenderer.emit = false;
	}

	private void TryUpdateTrailLength()
	{
		if (isStopping)
		{
			return;
		}
		float? num = null;
		if (!num.HasValue && (bool)base.projectile.braveBulletScript && base.projectile.braveBulletScript.bullet != null && !base.projectile.braveBulletScript.bullet.ManualControl)
		{
			num = base.projectile.braveBulletScript.bullet.Speed;
		}
		if (!num.HasValue && (bool)base.specRigidbody)
		{
			num = base.specRigidbody.Velocity.magnitude;
		}
		if (!num.HasValue)
		{
			return;
		}
		float? previousTrailSpeed = m_previousTrailSpeed;
		if (!previousTrailSpeed.HasValue || num.GetValueOrDefault() != m_previousTrailSpeed.Value || !num.HasValue)
		{
			m_previousTrailSpeed = num;
			if ((bool)trailRenderer)
			{
				trailRenderer.time = ((num != 0f) ? (desiredLength / num.Value) : desiredLength);
			}
			if ((bool)customTrailRenderer)
			{
				customTrailRenderer.lifeTime = ((num != 0f) ? (desiredLength / num.Value) : desiredLength);
			}
		}
	}
}
