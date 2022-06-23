using Dungeonator;
using UnityEngine;

public class ShieldPlayerBehavior : AttackBehaviorBase
{
	private enum State
	{
		Idle,
		Charging,
		Leaping
	}

	public float BlankRadius = 5f;

	public float Cooldown = 10f;

	public string AnimationName = "block";

	public float AnimationTime = 0.5f;

	private float m_cooldownTimer;

	private GameObject BlankVFXPrefab;

	private SeekTargetBehavior m_seekBehavior;

	private float m_elapsed;

	private State m_state;

	public override void Start()
	{
		base.Start();
		BehaviorSpeculator behaviorSpeculator = m_aiActor.behaviorSpeculator;
		for (int i = 0; i < behaviorSpeculator.MovementBehaviors.Count; i++)
		{
			if (behaviorSpeculator.MovementBehaviors[i] is SeekTargetBehavior)
			{
				m_seekBehavior = behaviorSpeculator.MovementBehaviors[i] as SeekTargetBehavior;
			}
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		base.Update();
		DecrementTimer(ref m_cooldownTimer);
		if (m_seekBehavior != null)
		{
			m_seekBehavior.ExternalCooldownSource = m_cooldownTimer <= 0f;
		}
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_cooldownTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		if (CheckPlayerProjectileRadius())
		{
			m_state = State.Charging;
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_updateEveryFrame = true;
			m_elapsed = 0f;
			return BehaviorResult.RunContinuous;
		}
		return BehaviorResult.Continue;
	}

	private Projectile GetNearestEnemyProjectile(PlayerController player)
	{
		Vector2 centerPosition = m_aiActor.CompanionOwner.CenterPosition;
		float num = BlankRadius * BlankRadius;
		Projectile result = null;
		float num2 = num;
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.collidesWithPlayer && (bool)projectile.specRigidbody && !(projectile.Owner is PlayerController))
			{
				float sqrMagnitude = (centerPosition - projectile.specRigidbody.UnitCenter).sqrMagnitude;
				if (sqrMagnitude < num && sqrMagnitude < num2)
				{
					result = projectile;
					num2 = sqrMagnitude;
				}
			}
		}
		return result;
	}

	private bool CheckPlayerProjectileRadius()
	{
		Vector2 centerPosition = m_aiActor.CompanionOwner.CenterPosition;
		float num = BlankRadius * BlankRadius;
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.collidesWithPlayer && (bool)projectile.specRigidbody && !(projectile.Owner is PlayerController) && (centerPosition - projectile.specRigidbody.UnitCenter).sqrMagnitude < num)
			{
				return true;
			}
		}
		return false;
	}

	private Vector2 GetTargetPoint(SpeculativeRigidbody targetRigidbody, Vector2 myCenter)
	{
		PixelCollider hitboxPixelCollider = targetRigidbody.HitboxPixelCollider;
		return BraveMathCollege.ClosestPointOnRectangle(myCenter, hitboxPixelCollider.UnitBottomLeft, hitboxPixelCollider.UnitDimensions);
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		Projectile projectile = null;
		if (m_state == State.Charging)
		{
			projectile = GetNearestEnemyProjectile(m_aiActor.CompanionOwner);
			m_state = State.Leaping;
			if (!projectile)
			{
				m_state = State.Idle;
				return ContinuousBehaviorResult.Finished;
			}
			Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
			Vector2 centerPosition = m_aiActor.CompanionOwner.CenterPosition;
			float num = Vector2.Distance(unitCenter, centerPosition);
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = (centerPosition - unitCenter).normalized * (num / 0.25f);
			float facingDirection = m_aiActor.BehaviorVelocity.ToAngle();
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.FacingDirection = facingDirection;
			m_aiActor.PathableTiles = CellTypes.FLOOR | CellTypes.PIT;
			m_aiActor.DoDustUps = false;
			if (AnimationTime <= 0f)
			{
				m_aiAnimator.PlayUntilFinished(AnimationName, true);
			}
			else
			{
				m_aiAnimator.PlayForDuration(AnimationName, AnimationTime, true);
			}
		}
		else if (m_state == State.Leaping)
		{
			m_elapsed += m_deltaTime;
			float num2 = 0.25f;
			if (m_elapsed >= num2)
			{
				m_cooldownTimer = Cooldown;
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		DoMicroBlank();
		m_state = State.Idle;
		m_aiActor.PathableTiles = CellTypes.FLOOR;
		m_aiActor.DoDustUps = true;
		m_aiActor.BehaviorOverridesVelocity = false;
		m_aiAnimator.LockFacingDirection = false;
		m_updateEveryFrame = false;
	}

	private void DoMicroBlank()
	{
		if (BlankVFXPrefab == null)
		{
			BlankVFXPrefab = (GameObject)BraveResources.Load("Global VFX/BlankVFX_Ghost");
		}
		AkSoundEngine.PostEvent("Play_OBJ_silenceblank_small_01", m_aiActor.gameObject);
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		float additionalTimeAtMaxRadius = 0.25f;
		silencerInstance.TriggerSilencer(m_aiActor.specRigidbody.UnitCenter, 20f, BlankRadius, BlankVFXPrefab, 0f, 3f, 3f, 3f, 30f, 3f, additionalTimeAtMaxRadius, m_aiActor.CompanionOwner, false);
	}

	public override bool IsReady()
	{
		return true;
	}

	public override float GetMinReadyRange()
	{
		return -1f;
	}

	public override float GetMaxRange()
	{
		return float.MaxValue;
	}
}
