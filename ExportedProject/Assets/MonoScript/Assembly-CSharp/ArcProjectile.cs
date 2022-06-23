using System;
using UnityEngine;

public class ArcProjectile : Projectile
{
	[Header("Arc Projectile")]
	public float startingHeight = 1f;

	public float startingZSpeed;

	public float gravity = -10f;

	public bool destroyOnGroundContact = true;

	public string groundAudioEvent = string.Empty;

	public GameObject LandingTargetSprite;

	private float m_currentHeight;

	private Vector3 m_current3DVelocity;

	private tk2dBaseSprite m_landingTarget;

	private Vector3 m_targetLandPosition;

	public event Action OnGrounded;

	public override void Start()
	{
		base.Start();
		m_currentHeight = startingHeight;
		m_current3DVelocity = (m_currentDirection * m_currentSpeed).ToVector3ZUp(startingZSpeed);
		if ((bool)LandingTargetSprite && !m_landingTarget)
		{
			float timeInFlight = GetTimeInFlight();
			m_targetLandPosition = base.transform.position.XY() + m_currentDirection * m_currentSpeed * timeInFlight + new Vector2(0f, 0f - m_currentHeight);
			m_landingTarget = SpawnManager.SpawnVFX(LandingTargetSprite, m_targetLandPosition, Quaternion.identity).GetComponent<tk2dBaseSprite>();
			m_landingTarget.UpdateZDepth();
			tk2dSpriteAnimator componentInChildren = m_landingTarget.GetComponentInChildren<tk2dSpriteAnimator>();
			componentInChildren.Play(componentInChildren.DefaultClip, 0f, (float)componentInChildren.DefaultClip.frames.Length / timeInFlight);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void Move()
	{
		if (angularVelocity != 0f)
		{
			base.transform.RotateAround(base.sprite.WorldCenter, Vector3.forward, angularVelocity * base.LocalDeltaTime);
		}
		m_current3DVelocity.x = m_currentDirection.x;
		m_current3DVelocity.y = m_currentDirection.y;
		m_current3DVelocity.z += base.LocalDeltaTime * gravity;
		float num = m_currentHeight + m_current3DVelocity.z * base.LocalDeltaTime;
		if (num < 0f)
		{
			if (this.OnGrounded != null)
			{
				this.OnGrounded();
			}
			if (!string.IsNullOrEmpty(groundAudioEvent))
			{
				AkSoundEngine.PostEvent(groundAudioEvent, base.gameObject);
			}
			if (destroyOnGroundContact)
			{
				DieInAir();
			}
			else
			{
				m_current3DVelocity.z = 0f - m_current3DVelocity.z;
				num = m_currentHeight + m_current3DVelocity.z * base.LocalDeltaTime;
			}
		}
		m_currentHeight = num;
		m_currentDirection = m_current3DVelocity.XY();
		Vector2 vector = m_current3DVelocity.XY().normalized * m_currentSpeed;
		base.specRigidbody.Velocity = new Vector2(vector.x, vector.y + m_current3DVelocity.z);
		base.LastVelocity = m_current3DVelocity.XY();
		UpdateTargetPosition(false);
	}

	public float GetTimeInFlight()
	{
		float num = 0f - startingHeight;
		float num2 = startingZSpeed;
		float num3 = gravity;
		float num4 = (Mathf.Sqrt(2f * num3 * num + num2 * num2) + num2) / (0f - num3);
		if (num4 < 0f)
		{
			num4 = (Mathf.Sqrt(2f * num3 * num + num2 * num2) - num2) / num3;
		}
		return num4;
	}

	public float GetRemainingTimeInFlight()
	{
		float num = 0f - m_currentHeight;
		float z = m_current3DVelocity.z;
		float num2 = gravity;
		float num3 = (Mathf.Sqrt(2f * num2 * num + z * z) + z) / (0f - num2);
		if (num3 < 0f)
		{
			num3 = (Mathf.Sqrt(2f * num2 * num + z * z) - z) / num2;
		}
		return num3;
	}

	public override float EstimatedTimeToTarget(Vector2 targetPoint, Vector2? overridePos = null)
	{
		return GetTimeInFlight();
	}

	public override Vector2 GetPredictedTargetPosition(Vector2 targetCenter, Vector2 targetVelocity, Vector2? overridePos = null, float? overrideProjectileSpeed = null)
	{
		return BraveMathCollege.GetPredictedPosition(targetCenter, targetVelocity, EstimatedTimeToTarget(targetCenter, overridePos));
	}

	public void AdjustSpeedToHit(Vector2 target)
	{
		Vector2 vector = target - base.transform.position.XY();
		baseData.speed = vector.magnitude / GetTimeInFlight();
		UpdateSpeed();
		UpdateTargetPosition(true);
	}

	protected override void HandleDestruction(CollisionData lcr, bool allowActorSpawns = true, bool allowProjectileSpawns = true)
	{
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget.gameObject);
			m_landingTarget = null;
		}
		base.HandleDestruction(lcr, allowActorSpawns, allowProjectileSpawns);
	}

	public override void OnDespawned()
	{
		if ((bool)m_landingTarget)
		{
			SpawnManager.Despawn(m_landingTarget.gameObject);
			m_landingTarget = null;
		}
		base.OnDespawned();
	}

	private void UpdateTargetPosition(bool useStartPosition)
	{
		if ((bool)LandingTargetSprite && (bool)m_landingTarget)
		{
			float num = ((!useStartPosition) ? GetRemainingTimeInFlight() : GetTimeInFlight());
			Vector2 a = m_targetLandPosition;
			Vector2 b = base.transform.position.XY() + m_currentDirection * m_currentSpeed * num + new Vector2(0f, 0f - m_currentHeight);
			m_targetLandPosition = Vector2.Lerp(a, b, (!useStartPosition) ? Mathf.Clamp01(BraveTime.DeltaTime * 4f) : 1f);
			m_landingTarget.transform.position = m_targetLandPosition;
			m_landingTarget.UpdateZDepth();
		}
	}
}
