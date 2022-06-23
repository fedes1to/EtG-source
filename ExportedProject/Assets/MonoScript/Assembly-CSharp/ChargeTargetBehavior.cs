using System;
using Dungeonator;
using UnityEngine;

public class ChargeTargetBehavior : MovementBehaviorBase
{
	protected enum ChargeState
	{
		Charging,
		Waiting,
		Bumped
	}

	public float ChargeCooldownTime = 1f;

	public float OvershootFactor = 3f;

	public float ChargeSpeed = 8f;

	public float ChargeAcceleration = 4f;

	public float ChargeKnockback = 50f;

	public float ChargeDamage;

	public bool ChargeDoDustUps;

	public float ChargeDustUpInterval;

	public GameObject ChargeHitVFX;

	public float BumpTime = 1f;

	public float PlayMeleeAnimDistance = 2f;

	protected bool m_playedMelee;

	protected bool m_playedBump;

	private ChargeState m_state = ChargeState.Waiting;

	private float m_chargeTargetLength;

	private Vector2 m_chargeDirection;

	private float m_chargeElapsedDistance;

	private float m_deceleration;

	private float m_currentMovementSpeed;

	private float m_cachedKnockback;

	private float m_cachedDamage;

	private bool m_cachedDoDustUps;

	private float m_cachedDustUpInterval;

	private VFXPool m_cachedVfx;

	private float m_repathTimer;

	private float m_phaseTimer;

	protected ChargeState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	public override void Start()
	{
		base.Start();
		m_cachedKnockback = m_aiActor.CollisionKnockbackStrength;
		m_cachedDamage = m_aiActor.CollisionDamage;
		m_cachedDoDustUps = m_aiActor.DoDustUps;
		m_cachedDustUpInterval = m_aiActor.DustUpInterval;
		m_cachedVfx = m_aiActor.CollisionVFX;
		m_deceleration = ChargeSpeed * ChargeSpeed / (-2f * OvershootFactor);
		SpeculativeRigidbody specRigidbody = m_aiAnimator.specRigidbody;
		specRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		DecrementTimer(ref m_phaseTimer);
	}

	public override BehaviorResult Update()
	{
		if (m_aiActor.TargetRigidbody != null)
		{
			switch (State)
			{
			case ChargeState.Charging:
				return HandleChargeState();
			case ChargeState.Waiting:
				return HandleWaitState();
			case ChargeState.Bumped:
				return HandleBumpedState();
			}
		}
		return BehaviorResult.Continue;
	}

	protected void BeginState(ChargeState state)
	{
		switch (state)
		{
		case ChargeState.Charging:
		{
			m_playedMelee = false;
			m_playedBump = false;
			m_chargeElapsedDistance = 0f;
			m_aiActor.CollisionKnockbackStrength = ChargeKnockback;
			m_aiActor.CollisionDamage = ChargeDamage;
			m_aiActor.DoDustUps = ChargeDoDustUps;
			m_aiActor.DustUpInterval = ChargeDustUpInterval;
			if ((bool)ChargeHitVFX)
			{
				VFXObject vFXObject = new VFXObject();
				vFXObject.effect = ChargeHitVFX;
				VFXComplex vFXComplex = new VFXComplex();
				vFXComplex.effects = new VFXObject[1] { vFXObject };
				VFXPool vFXPool = new VFXPool();
				vFXPool.type = VFXPoolType.Single;
				vFXPool.effects = new VFXComplex[1] { vFXComplex };
				m_aiActor.CollisionVFX = vFXPool;
			}
			m_aiActor.ClearPath();
			Vector2 vector = m_aiActor.TargetRigidbody.UnitCenter - m_aiActor.specRigidbody.UnitCenter;
			m_chargeTargetLength = vector.magnitude;
			m_chargeDirection = vector.normalized;
			m_aiActor.BehaviorOverridesVelocity = true;
			break;
		}
		case ChargeState.Waiting:
			m_aiActor.CollisionKnockbackStrength = m_cachedKnockback;
			m_aiActor.CollisionDamage = m_cachedDamage;
			m_aiActor.DoDustUps = m_cachedDoDustUps;
			m_aiActor.DustUpInterval = m_cachedDustUpInterval;
			m_aiActor.CollisionVFX = m_cachedVfx;
			m_aiActor.BehaviorOverridesVelocity = false;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_currentMovementSpeed = m_aiActor.MovementSpeed;
			m_phaseTimer = ChargeCooldownTime;
			break;
		case ChargeState.Bumped:
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_currentMovementSpeed = 0f;
			m_phaseTimer = BumpTime;
			break;
		}
	}

	protected void EndState(ChargeState state)
	{
		switch (state)
		{
		case ChargeState.Charging:
			m_aiAnimator.EndAnimationIf("prebump");
			m_aiAnimator.LockFacingDirection = false;
			break;
		case ChargeState.Bumped:
			m_aiAnimator.EndAnimationIf("bump");
			m_aiAnimator.LockFacingDirection = false;
			break;
		}
	}

	protected BehaviorResult HandleChargeState()
	{
		m_aiActor.BehaviorVelocity = m_chargeDirection * m_currentMovementSpeed;
		m_chargeElapsedDistance += m_currentMovementSpeed * m_deltaTime;
		m_aiActor.ClearPath();
		if (m_chargeElapsedDistance >= m_chargeTargetLength + OvershootFactor || m_currentMovementSpeed == 0f)
		{
			State = ChargeState.Waiting;
		}
		else if (m_chargeElapsedDistance > m_chargeTargetLength)
		{
			m_currentMovementSpeed = Mathf.Max(m_currentMovementSpeed + m_deceleration * m_deltaTime, 0f);
			if (m_playedMelee && !m_playedBump)
			{
				float num = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, m_aiActor.TargetRigidbody.UnitCenter);
				if (num > PlayMeleeAnimDistance)
				{
					m_aiAnimator.EndAnimationIf("prebump");
				}
			}
		}
		else
		{
			m_currentMovementSpeed = Mathf.Min(m_currentMovementSpeed + ChargeAcceleration * m_deltaTime, ChargeSpeed);
			if (!m_playedMelee)
			{
				float num2 = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, m_aiActor.TargetRigidbody.UnitCenter);
				if (num2 < PlayMeleeAnimDistance)
				{
					m_aiAnimator.LockFacingDirection = true;
					m_aiAnimator.FacingDirection = BraveMathCollege.Atan2Degrees(m_chargeDirection);
					m_aiAnimator.PlayUntilCancelled("prebump", true);
					m_playedMelee = true;
				}
			}
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	protected BehaviorResult HandleWaitState()
	{
		bool hasLineOfSightToTarget = m_aiActor.HasLineOfSightToTarget;
		bool flag = false;
		if (hasLineOfSightToTarget)
		{
			flag = GameManager.Instance.Dungeon.data.CheckLineForCellType(m_aiActor.PathTile, m_aiActor.TargetRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor), CellType.PIT);
		}
		if (hasLineOfSightToTarget && !flag && m_phaseTimer == 0f)
		{
			State = ChargeState.Charging;
		}
		return BehaviorResult.Continue;
	}

	protected BehaviorResult HandleBumpedState()
	{
		m_aiActor.CollisionKnockbackStrength = m_cachedKnockback;
		m_aiActor.CollisionDamage = m_cachedDamage;
		m_aiActor.DoDustUps = m_cachedDoDustUps;
		m_aiActor.DustUpInterval = m_cachedDustUpInterval;
		m_aiActor.CollisionVFX = m_cachedVfx;
		m_aiActor.ClearPath();
		if (m_phaseTimer == 0f)
		{
			State = ChargeState.Waiting;
		}
		return BehaviorResult.Continue;
	}

	private void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (State == ChargeState.Charging && m_playedMelee && !m_playedBump && (bool)rigidbodyCollision.OtherRigidbody.GetComponent<PlayerController>())
		{
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.FacingDirection = BraveMathCollege.Atan2Degrees(rigidbodyCollision.OtherRigidbody.UnitCenter - m_aiAnimator.specRigidbody.UnitCenter);
			m_aiAnimator.PlayUntilCancelled("bump", true);
			State = ChargeState.Bumped;
		}
	}
}
