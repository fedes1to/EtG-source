using System;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/TankTreader/ChargeBehavior")]
public class TankTreaderChargeBehavior : BasicAttackBehavior
{
	private enum FireState
	{
		Idle,
		Charging,
		Bouncing
	}

	public float minRange;

	public string chargeAnim;

	public string hitAnim;

	public float chargeSpeed;

	public float maxChargeDistance = -1f;

	public float chargeKnockback = 50f;

	public float chargeDamage = 0.5f;

	public float wallRecoilForce = 10f;

	public GameObject launchVfx;

	public GameObject trailVfx;

	public Transform trailVfxParent;

	public GameObject hitVfx;

	public bool chargeDustUps;

	public float chargeDustUpInterval;

	private FireState m_state;

	private float m_chargeTime;

	private Vector2 m_chargeDir;

	private float m_cachedKnockback;

	private float m_cachedDamage;

	private VFXPool m_cachedVfx;

	private CellTypes m_cachedPathableTiles;

	private bool m_cachedDoDustUps;

	private float m_cachedDustUpInterval;

	private GameObject m_trailVfx;

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
		m_cachedDustUpInterval = m_aiActor.DustUpInterval;
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
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
		float num = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, unitCenter);
		if (num > minRange)
		{
			PixelCollider hitboxPixelCollider = m_aiActor.TargetRigidbody.specRigidbody.HitboxPixelCollider;
			PixelCollider groundPixelCollider = m_aiActor.specRigidbody.GroundPixelCollider;
			bool flag = hitboxPixelCollider.UnitRight < groundPixelCollider.UnitLeft;
			bool flag2 = hitboxPixelCollider.UnitLeft > groundPixelCollider.UnitRight;
			bool flag3 = hitboxPixelCollider.UnitBottom > groundPixelCollider.UnitTop;
			bool flag4 = hitboxPixelCollider.UnitTop < groundPixelCollider.UnitBottom;
			Vector2 vector = Vector2.zero;
			if (flag && !flag4 && !flag3)
			{
				vector = -Vector2.right;
			}
			else if (flag2 && !flag4 && !flag3)
			{
				vector = Vector2.right;
			}
			else if (flag3 && !flag && !flag2)
			{
				vector = Vector2.up;
			}
			else if (flag4 && !flag && !flag2)
			{
				vector = -Vector2.up;
			}
			if (vector != Vector2.zero)
			{
				float num2 = BraveMathCollege.AbsAngleBetween(vector.ToAngle(), m_aiAnimator.FacingDirection);
				if (num2 > 90f)
				{
					num2 = Mathf.Abs(num2 - 180f);
				}
				if (num2 < 20f)
				{
					m_chargeDir = vector;
					State = FireState.Charging;
					m_updateEveryFrame = true;
					return BehaviorResult.RunContinuous;
				}
			}
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (State == FireState.Charging)
		{
			m_aiActor.BehaviorVelocity = m_chargeDir.normalized * chargeSpeed;
			if (maxChargeDistance > 0f)
			{
				m_chargeTime += m_deltaTime;
				if (m_chargeTime * chargeSpeed > maxChargeDistance)
				{
					return ContinuousBehaviorResult.Finished;
				}
			}
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

	private void OnCollision(CollisionData collisionData)
	{
		if (State != FireState.Charging || m_aiActor.healthHaver.IsDead)
		{
			return;
		}
		if ((bool)collisionData.OtherRigidbody)
		{
			Projectile projectile = collisionData.OtherRigidbody.projectile;
			if ((bool)projectile && !(projectile.Owner is PlayerController))
			{
				return;
			}
		}
		State = FireState.Bouncing;
	}

	private void BeginState(FireState state)
	{
		if (state == FireState.Idle || state != FireState.Charging)
		{
			return;
		}
		m_chargeTime = 0f;
		m_aiActor.ClearPath();
		m_aiActor.BehaviorVelocity = m_chargeDir.normalized * chargeSpeed;
		float z = m_aiActor.BehaviorVelocity.ToAngle();
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
		m_aiActor.DoDustUps = chargeDustUps;
		m_aiActor.DustUpInterval = chargeDustUpInterval;
		m_aiAnimator.PlayUntilFinished(chargeAnim, true);
		if ((bool)launchVfx)
		{
			SpawnManager.SpawnVFX(launchVfx, m_aiActor.specRigidbody.UnitCenter, Quaternion.identity);
		}
		if ((bool)trailVfx)
		{
			m_trailVfx = SpawnManager.SpawnParticleSystem(trailVfx, m_aiActor.sprite.WorldCenter, Quaternion.Euler(0f, 0f, z));
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
		m_aiActor.DustUpInterval = m_cachedDustUpInterval;
		m_aiActor.PathableTiles = m_cachedPathableTiles;
		m_aiAnimator.EndAnimationIf(chargeAnim);
	}
}
