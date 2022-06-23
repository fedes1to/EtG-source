using System;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BulletBro/RepositionBehavior")]
public class BulletBroRepositionBehavior : BasicAttackBehavior
{
	private enum FireState
	{
		Idle,
		Priming,
		Charging,
		Bouncing
	}

	public float triggerAngle = 30f;

	public string primeAnim;

	public string chargeAnim;

	public string hitAnim;

	public float chargeSpeed;

	public float chargeKnockback = 50f;

	public float chargeDamage = 0.5f;

	public bool HideGun;

	public GameObject launchVfx;

	public GameObject trailVfx;

	public Transform trailVfxParent;

	public GameObject hitVfx;

	[InspectorCategory("Conditions")]
	public float StaticCooldown;

	private FireState m_state;

	private AIActor m_otherBro;

	private Vector2 m_targetCenter;

	private float m_lastAngleToTarget;

	private float m_cachedKnockback;

	private float m_cachedDamage;

	private VFXPool m_cachedVfx;

	private CellTypes m_cachedPathableTiles;

	private bool m_cachedDoDustUps;

	private GameObject m_trailVfx;

	private Vector2 m_cachedTargetCenter;

	private static float s_staticCooldown;

	private static int s_lastStaticUpdateFrameNum = -1;

	private FireState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	public override void Start()
	{
		base.Start();
		SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
		specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		m_cachedKnockback = m_aiActor.CollisionKnockbackStrength;
		m_cachedDamage = m_aiActor.CollisionDamage;
		m_cachedVfx = m_aiActor.CollisionVFX;
		m_cachedPathableTiles = m_aiActor.PathableTiles;
		m_cachedDoDustUps = m_aiActor.DoDustUps;
		m_otherBro = BroController.GetOtherBro(m_aiActor.gameObject).aiActor;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (s_staticCooldown > 0f && s_lastStaticUpdateFrameNum != Time.frameCount)
		{
			s_staticCooldown = Mathf.Max(0f, s_staticCooldown - m_deltaTime);
			s_lastStaticUpdateFrameNum = Time.frameCount;
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!m_otherBro)
		{
			return BehaviorResult.Continue;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		Vector2 unitCenter = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		Vector2 unitCenter2 = m_aiActor.specRigidbody.UnitCenter;
		Vector2 unitCenter3 = m_otherBro.specRigidbody.UnitCenter;
		float a = (unitCenter2 - unitCenter).ToAngle();
		float b = (unitCenter3 - unitCenter).ToAngle();
		if (BraveMathCollege.AbsAngleBetween(a, b) < triggerAngle)
		{
			Vector2 vector = unitCenter - unitCenter3;
			m_targetCenter = unitCenter3 + vector + vector.normalized * 7f;
			m_lastAngleToTarget = (m_targetCenter - unitCenter2).ToAngle();
			State = FireState.Priming;
			s_staticCooldown += StaticCooldown;
			m_updateEveryFrame = true;
			return BehaviorResult.RunContinuous;
		}
		m_cachedTargetCenter = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (State == FireState.Priming)
		{
			if (!m_aiAnimator.IsPlaying(primeAnim))
			{
				if (!m_aiActor.TargetRigidbody)
				{
					return ContinuousBehaviorResult.Finished;
				}
				State = FireState.Charging;
			}
		}
		else if (State == FireState.Charging)
		{
			Vector2 cachedTargetCenter = m_cachedTargetCenter;
			if ((bool)m_aiActor.TargetRigidbody)
			{
				m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
			if ((bool)m_otherBro)
			{
				Vector2 unitCenter2 = m_otherBro.specRigidbody.UnitCenter;
				Vector2 vector = cachedTargetCenter - unitCenter2;
				m_targetCenter = unitCenter2 + vector + vector.normalized * 7f;
			}
			float num = (m_targetCenter - unitCenter).ToAngle();
			if (BraveMathCollege.AbsAngleBetween(num, m_lastAngleToTarget) > 135f)
			{
				return ContinuousBehaviorResult.Finished;
			}
			m_aiActor.BehaviorVelocity = (m_targetCenter - unitCenter).normalized * chargeSpeed;
			m_aiAnimator.FacingDirection = m_aiActor.BehaviorVelocity.ToAngle();
			m_lastAngleToTarget = num;
		}
		else if (State == FireState.Bouncing && !m_aiAnimator.IsPlaying(hitAnim))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_updateEveryFrame = false;
		State = FireState.Idle;
		UpdateCooldowns();
	}

	public override bool IsReady()
	{
		return base.IsReady() && s_staticCooldown <= 0f;
	}

	private void OnCollision(CollisionData collisionData)
	{
		if (State == FireState.Charging && !m_aiActor.healthHaver.IsDead)
		{
			State = FireState.Bouncing;
		}
	}

	private void BeginState(FireState state)
	{
		switch (state)
		{
		case FireState.Idle:
			if (HideGun)
			{
				m_aiShooter.ToggleGunAndHandRenderers(true, "BulletBroRepositionBehavior");
			}
			m_aiActor.BehaviorOverridesVelocity = false;
			m_aiAnimator.LockFacingDirection = false;
			break;
		case FireState.Priming:
			if (HideGun)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "BulletBroRepositionBehavior");
			}
			m_aiAnimator.PlayUntilFinished(primeAnim, true);
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			break;
		case FireState.Charging:
		{
			if (HideGun)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "BulletBroRepositionBehavior");
			}
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = (m_targetCenter - m_aiActor.specRigidbody.UnitCenter).normalized * chargeSpeed;
			float num = m_aiActor.BehaviorVelocity.ToAngle();
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.FacingDirection = num;
			m_aiActor.CollisionKnockbackStrength = chargeKnockback;
			m_aiActor.CollisionDamage = chargeDamage;
			if ((bool)hitVfx)
			{
				VFXObject vFXObject = new VFXObject();
				vFXObject.effect = hitVfx;
				VFXComplex vFXComplex = new VFXComplex();
				vFXComplex.effects = new VFXObject[1] { vFXObject };
				VFXPool vFXPool = new VFXPool();
				vFXPool.type = VFXPoolType.Single;
				vFXPool.effects = new VFXComplex[1] { vFXComplex };
				m_aiActor.CollisionVFX = vFXPool;
			}
			m_aiActor.PathableTiles = CellTypes.FLOOR | CellTypes.PIT;
			m_aiActor.DoDustUps = false;
			m_aiAnimator.PlayUntilFinished(chargeAnim, true);
			if ((bool)launchVfx)
			{
				SpawnManager.SpawnVFX(launchVfx, m_aiActor.specRigidbody.UnitCenter, Quaternion.identity);
			}
			if ((bool)trailVfx)
			{
				m_trailVfx = SpawnManager.SpawnParticleSystem(trailVfx, m_aiActor.sprite.WorldCenter, Quaternion.Euler(0f, 0f, num));
				if ((bool)trailVfxParent)
				{
					m_trailVfx.transform.parent = trailVfxParent;
				}
				else
				{
					m_trailVfx.transform.parent = m_aiActor.transform;
				}
				ParticleKiller component = m_trailVfx.GetComponent<ParticleKiller>();
				if (component != null)
				{
					component.Awake();
				}
			}
			m_aiActor.specRigidbody.ForceRegenerate();
			break;
		}
		}
	}

	private void EndState(FireState state)
	{
		if (state != FireState.Charging)
		{
			return;
		}
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_aiAnimator.PlayUntilFinished(hitAnim, true);
		m_aiActor.CollisionKnockbackStrength = m_cachedKnockback;
		m_aiActor.CollisionDamage = m_cachedDamage;
		m_aiActor.CollisionVFX = m_cachedVfx;
		if ((bool)m_trailVfx)
		{
			ParticleKiller component = m_trailVfx.GetComponent<ParticleKiller>();
			if ((bool)component)
			{
				component.StopEmitting();
			}
			else
			{
				SpawnManager.Despawn(m_trailVfx);
			}
			m_trailVfx = null;
		}
		m_aiActor.DoDustUps = m_cachedDoDustUps;
		m_aiActor.PathableTiles = m_cachedPathableTiles;
		m_aiAnimator.EndAnimationIf(chargeAnim);
	}

	private void TestTargetPosition(IntVector2 testPos, float broAngleToTarget, Vector2 targetCenter, ref IntVector2? targetPos, ref float targetAngleFromBro)
	{
		float num = BraveMathCollege.AbsAngleBetween(broAngleToTarget, (testPos.ToCenterVector2() - targetCenter).ToAngle());
		if (num > targetAngleFromBro)
		{
			targetPos = testPos;
			targetAngleFromBro = num;
		}
	}
}
