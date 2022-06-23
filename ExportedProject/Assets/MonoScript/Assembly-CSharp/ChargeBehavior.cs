using System;
using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class ChargeBehavior : BasicAttackBehavior
{
	private enum FireState
	{
		Idle,
		Priming,
		Charging,
		Bouncing
	}

	[InspectorCategory("Conditions")]
	public float minRange;

	[InspectorHeader("Prime")]
	public float primeTime = -1f;

	public bool stopDuringPrime = true;

	[InspectorHeader("Charge")]
	public float leadAmount;

	public float chargeSpeed;

	public float chargeAcceleration = -1f;

	public float maxChargeDistance = -1f;

	public float chargeKnockback = 50f;

	public float chargeDamage = 0.5f;

	public float wallRecoilForce = 10f;

	public bool stoppedByProjectiles = true;

	public bool endWhenChargeAnimFinishes;

	public bool switchCollidersOnCharge;

	public bool collidesWithDodgeRollingPlayers = true;

	[InspectorCategory("Attack")]
	public GameObject ShootPoint;

	[InspectorCategory("Attack")]
	public BulletScriptSelector bulletScript;

	[InspectorCategory("Visuals")]
	public string primeAnim;

	[InspectorCategory("Visuals")]
	public string chargeAnim;

	[InspectorCategory("Visuals")]
	public string hitAnim;

	[InspectorCategory("Visuals")]
	public bool HideGun;

	[InspectorCategory("Visuals")]
	public GameObject launchVfx;

	[InspectorCategory("Visuals")]
	public GameObject trailVfx;

	[InspectorCategory("Visuals")]
	public Transform trailVfxParent;

	[InspectorCategory("Visuals")]
	public GameObject hitVfx;

	[InspectorCategory("Visuals")]
	public GameObject nonActorHitVfx;

	[InspectorCategory("Visuals")]
	public bool chargeDustUps;

	[InspectorShowIf("chargeDustUps")]
	[InspectorCategory("Visuals")]
	[InspectorIndent]
	public float chargeDustUpInterval;

	private BulletScriptSource m_bulletSource;

	private bool m_initialized;

	private float m_timer;

	private float m_chargeTime;

	private float m_cachedKnockback;

	private float m_cachedDamage;

	private VFXPool m_cachedVfx;

	private VFXPool m_cachedNonActorWallVfx;

	private float m_currentSpeed;

	private float m_chargeDirection;

	private CellTypes m_cachedPathableTiles;

	private bool m_cachedDoDustUps;

	private float m_cachedDustUpInterval;

	private PixelCollider m_enemyCollider;

	private PixelCollider m_enemyHitbox;

	private PixelCollider m_projectileCollider;

	private GameObject m_trailVfx;

	private Vector2 m_collisionNormal;

	private FireState m_state;

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
		m_cachedKnockback = m_aiActor.CollisionKnockbackStrength;
		m_cachedDamage = m_aiActor.CollisionDamage;
		m_cachedVfx = m_aiActor.CollisionVFX;
		m_cachedNonActorWallVfx = m_aiActor.NonActorCollisionVFX;
		m_cachedPathableTiles = m_aiActor.PathableTiles;
		m_cachedDoDustUps = m_aiActor.DoDustUps;
		m_cachedDustUpInterval = m_aiActor.DustUpInterval;
		if (switchCollidersOnCharge)
		{
			for (int i = 0; i < m_aiActor.specRigidbody.PixelColliders.Count; i++)
			{
				PixelCollider pixelCollider = m_aiActor.specRigidbody.PixelColliders[i];
				if (pixelCollider.CollisionLayer == CollisionLayer.EnemyCollider)
				{
					m_enemyCollider = pixelCollider;
				}
				if (pixelCollider.CollisionLayer == CollisionLayer.EnemyHitBox)
				{
					m_enemyHitbox = pixelCollider;
				}
				if (!pixelCollider.Enabled && pixelCollider.CollisionLayer == CollisionLayer.Projectile)
				{
					m_projectileCollider = pixelCollider;
					m_projectileCollider.CollisionLayerCollidableOverride |= CollisionMask.LayerToMask(CollisionLayer.Projectile);
				}
			}
		}
		if (!collidesWithDodgeRollingPlayers)
		{
			SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
			specRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (!m_initialized)
		{
			SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
			specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			m_initialized = true;
		}
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
		Vector2 vector = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		if (leadAmount > 0f)
		{
			Vector2 vector2 = vector + m_aiActor.TargetRigidbody.specRigidbody.Velocity * 0.75f;
			vector2 = BraveMathCollege.GetPredictedPosition(vector, m_aiActor.TargetVelocity, m_aiActor.specRigidbody.UnitCenter, chargeSpeed);
			vector = Vector2.Lerp(vector, vector2, leadAmount);
		}
		float num = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, vector);
		if (num > minRange)
		{
			if (!string.IsNullOrEmpty(primeAnim) || primeTime > 0f)
			{
				State = FireState.Priming;
			}
			else
			{
				State = FireState.Charging;
			}
			m_updateEveryFrame = true;
			return BehaviorResult.RunContinuous;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (State == FireState.Priming)
		{
			if (!m_aiActor.TargetRigidbody)
			{
				return ContinuousBehaviorResult.Finished;
			}
			if (m_timer > 0f)
			{
				float facingDirection = m_aiAnimator.FacingDirection;
				float num = (m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.specRigidbody.UnitCenter).ToAngle();
				float b = BraveMathCollege.ClampAngle180(num - facingDirection);
				float facingDirection2 = facingDirection + Mathf.Lerp(0f, b, m_deltaTime / (m_timer + m_deltaTime));
				m_aiAnimator.FacingDirection = facingDirection2;
			}
			if (!stopDuringPrime)
			{
				float magnitude = m_aiActor.BehaviorVelocity.magnitude;
				float magnitude2 = Mathf.Lerp(magnitude, 0f, m_deltaTime / (m_timer + m_deltaTime));
				m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_aiAnimator.FacingDirection, magnitude2);
			}
			if ((!(primeTime > 0f)) ? (!m_aiAnimator.IsPlaying(primeAnim)) : (m_timer <= 0f))
			{
				State = FireState.Charging;
			}
		}
		else if (State == FireState.Charging)
		{
			if (chargeAcceleration > 0f)
			{
				m_currentSpeed = Mathf.Min(chargeSpeed, m_currentSpeed + chargeAcceleration * m_deltaTime);
				m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_chargeDirection, m_currentSpeed);
			}
			if (endWhenChargeAnimFinishes && !m_aiAnimator.IsPlaying(chargeAnim))
			{
				return ContinuousBehaviorResult.Finished;
			}
			if (maxChargeDistance > 0f)
			{
				m_chargeTime += m_deltaTime;
				if (m_chargeTime * chargeSpeed > maxChargeDistance)
				{
					return ContinuousBehaviorResult.Finished;
				}
			}
		}
		else if (State == FireState.Bouncing)
		{
			if (!m_aiAnimator.IsPlaying(hitAnim))
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (State == FireState.Idle)
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

	public override void Destroy()
	{
		if ((bool)m_aiActor)
		{
			SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
			specRigidbody.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Remove(specRigidbody.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(OnPostRigidbodyMovement));
		}
		base.Destroy();
	}

	private void Fire()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = bulletScript;
		m_bulletSource.Initialize();
	}

	private void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (m_state == FireState.Charging)
		{
			PlayerController playerController = otherRigidbody.gameActor as PlayerController;
			if ((bool)playerController && playerController.spriteAnimator.QueryInvulnerabilityFrame())
			{
				PhysicsEngine.SkipCollision = true;
			}
		}
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
			if ((bool)projectile && (!(projectile.Owner is PlayerController) || !stoppedByProjectiles))
			{
				return;
			}
		}
		if (!string.IsNullOrEmpty(hitAnim))
		{
			State = FireState.Bouncing;
		}
		else
		{
			State = FireState.Idle;
		}
		if (switchCollidersOnCharge)
		{
			PhysicsEngine.CollisionHaltsVelocity = true;
			PhysicsEngine.HaltRemainingMovement = true;
			PhysicsEngine.PostSliceVelocity = Vector2.zero;
			m_collisionNormal = collisionData.Normal;
			SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
			specRigidbody.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(specRigidbody.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(OnPostRigidbodyMovement));
		}
		if (!collisionData.OtherRigidbody || !collisionData.OtherRigidbody.knockbackDoer)
		{
			m_aiActor.knockbackDoer.ApplyKnockback(collisionData.Normal, wallRecoilForce);
		}
	}

	private void OnPostRigidbodyMovement(SpeculativeRigidbody specRigidbody, Vector2 unitDelta, IntVector2 pixelDelta)
	{
		if (!m_behaviorSpeculator)
		{
			return;
		}
		List<CollisionData> list = new List<CollisionData>();
		bool flag = false;
		if (PhysicsEngine.Instance.OverlapCast(m_aiActor.specRigidbody, list, true, true, null, null, false, null, null))
		{
			for (int i = 0; i < list.Count; i++)
			{
				SpeculativeRigidbody otherRigidbody = list[i].OtherRigidbody;
				if ((bool)otherRigidbody && (bool)otherRigidbody.transform.parent && ((bool)otherRigidbody.transform.parent.GetComponent<DungeonDoorSubsidiaryBlocker>() || (bool)otherRigidbody.transform.parent.GetComponent<DungeonDoorController>()))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (m_collisionNormal.y >= 0.5f)
			{
				m_aiActor.transform.position += new Vector3(0f, 0.5f);
			}
			if (m_collisionNormal.x <= -0.5f)
			{
				m_aiActor.transform.position += new Vector3(-0.3125f, 0f);
			}
			if (m_collisionNormal.x >= 0.5f)
			{
				m_aiActor.transform.position += new Vector3(0.3125f, 0f);
			}
			m_aiActor.specRigidbody.Reinitialize();
		}
		else
		{
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_aiActor.specRigidbody);
		}
		SpeculativeRigidbody specRigidbody2 = m_aiActor.specRigidbody;
		specRigidbody2.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Remove(specRigidbody2.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(OnPostRigidbodyMovement));
	}

	private void BeginState(FireState state)
	{
		switch (state)
		{
		case FireState.Idle:
			if (HideGun)
			{
				m_aiShooter.ToggleGunAndHandRenderers(true, "ChargeBehavior");
			}
			m_aiActor.BehaviorOverridesVelocity = false;
			m_aiAnimator.LockFacingDirection = false;
			break;
		case FireState.Priming:
			if (HideGun)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "ChargeBehavior");
			}
			m_aiAnimator.PlayUntilFinished(primeAnim, true);
			if (primeTime > 0f)
			{
				m_timer = primeTime;
			}
			else
			{
				m_timer = m_aiAnimator.CurrentClipLength;
			}
			if (stopDuringPrime)
			{
				m_aiActor.ClearPath();
				m_aiActor.BehaviorOverridesVelocity = true;
				m_aiActor.BehaviorVelocity = Vector2.zero;
			}
			else
			{
				m_aiActor.BehaviorOverridesVelocity = true;
				m_aiActor.BehaviorVelocity = m_aiActor.specRigidbody.Velocity;
			}
			break;
		case FireState.Charging:
		{
			if (HideGun)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "ChargeBehavior");
			}
			m_chargeTime = 0f;
			Vector2 vector = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			if (leadAmount > 0f)
			{
				Vector2 vector2 = vector + m_aiActor.TargetRigidbody.specRigidbody.Velocity * 0.75f;
				vector2 = BraveMathCollege.GetPredictedPosition(vector, m_aiActor.TargetVelocity, m_aiActor.specRigidbody.UnitCenter, chargeSpeed);
				vector = Vector2.Lerp(vector, vector2, leadAmount);
			}
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_currentSpeed = ((!(chargeAcceleration > 0f)) ? chargeSpeed : 0f);
			m_chargeDirection = (vector - m_aiActor.specRigidbody.UnitCenter).ToAngle();
			m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_chargeDirection, m_currentSpeed);
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.FacingDirection = m_chargeDirection;
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
			if ((bool)nonActorHitVfx)
			{
				VFXObject vFXObject2 = new VFXObject();
				vFXObject2.effect = nonActorHitVfx;
				VFXComplex vFXComplex2 = new VFXComplex();
				vFXComplex2.effects = new VFXObject[1] { vFXObject2 };
				VFXPool vFXPool2 = new VFXPool();
				vFXPool2.type = VFXPoolType.Single;
				vFXPool2.effects = new VFXComplex[1] { vFXComplex2 };
				m_aiActor.NonActorCollisionVFX = vFXPool2;
			}
			m_aiActor.PathableTiles = CellTypes.FLOOR | CellTypes.PIT;
			if (switchCollidersOnCharge)
			{
				m_enemyCollider.CollisionLayer = CollisionLayer.TileBlocker;
				m_enemyHitbox.Enabled = false;
				m_projectileCollider.Enabled = true;
			}
			m_aiActor.DoDustUps = chargeDustUps;
			m_aiActor.DustUpInterval = chargeDustUpInterval;
			m_aiAnimator.PlayUntilFinished(chargeAnim, true);
			if ((bool)launchVfx)
			{
				SpawnManager.SpawnVFX(launchVfx, m_aiActor.specRigidbody.UnitCenter, Quaternion.identity);
			}
			if ((bool)trailVfx)
			{
				m_trailVfx = SpawnManager.SpawnParticleSystem(trailVfx, m_aiActor.sprite.WorldCenter, Quaternion.Euler(0f, 0f, m_chargeDirection));
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
			if (bulletScript != null && !bulletScript.IsNull)
			{
				Fire();
			}
			m_aiActor.specRigidbody.ForceRegenerate();
			break;
		}
		case FireState.Bouncing:
			m_aiAnimator.PlayUntilFinished(hitAnim, true);
			break;
		}
	}

	private void EndState(FireState state)
	{
		if (state != FireState.Charging)
		{
			return;
		}
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_aiActor.CollisionKnockbackStrength = m_cachedKnockback;
		m_aiActor.CollisionDamage = m_cachedDamage;
		m_aiActor.CollisionVFX = m_cachedVfx;
		m_aiActor.NonActorCollisionVFX = m_cachedNonActorWallVfx;
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
		if (switchCollidersOnCharge)
		{
			m_enemyCollider.CollisionLayer = CollisionLayer.EnemyCollider;
			m_enemyHitbox.Enabled = true;
			m_projectileCollider.Enabled = false;
		}
		if (m_bulletSource != null)
		{
			m_bulletSource.ForceStop();
		}
		m_aiAnimator.EndAnimationIf(chargeAnim);
	}
}
