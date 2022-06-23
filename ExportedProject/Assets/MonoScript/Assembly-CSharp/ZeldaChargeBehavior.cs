using System;
using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class ZeldaChargeBehavior : BasicAttackBehavior
{
	private enum FireState
	{
		Idle,
		Priming,
		Charging,
		Bouncing
	}

	public string primeAnim;

	public string chargeAnim;

	public bool endWhenChargeAnimFinishes;

	public bool switchCollidersOnCharge;

	public string hitAnim;

	public string hitPlayerAnim;

	public float leadAmount;

	public float chargeRange = 15f;

	public float chargeSpeed;

	public float chargeKnockback = 50f;

	public float chargeDamage = 0.5f;

	public bool delayWallRecoil;

	public float wallRecoilForce = 10f;

	public bool stopAtPits = true;

	public GameObject launchVfx;

	public GameObject trailVfx;

	public Transform trailVfxParent;

	public GameObject hitVfx;

	public string trailVfxString;

	public string hitWallVfxString;

	[InspectorHeader("Impact BulletScript")]
	public GameObject shootPoint;

	public BulletScriptSelector impactBulletScript;

	private FireState m_state;

	private float m_primeAnimTime;

	private Vector2? m_chargeDir;

	private Vector2? m_storedCollisionNormal;

	private bool m_hitPlayer;

	private bool m_hitWall;

	private float m_cachedKnockback;

	private float m_cachedDamage;

	private VFXPool m_cachedVfx;

	private CellTypes m_cachedPathableTiles;

	private bool m_cachedDoDustUps;

	private PixelCollider m_enemyCollider;

	private PixelCollider m_enemyHitbox;

	private PixelCollider m_projectileCollider;

	private GameObject m_trailVfx;

	private string m_cachedTrailString;

	private BulletScriptSource m_bulletSource;

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
		if (stopAtPits)
		{
			SpeculativeRigidbody specRigidbody2 = m_aiActor.specRigidbody;
			specRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(specRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PitMovementRestrictor));
		}
		if (!string.IsNullOrEmpty(primeAnim))
		{
			m_primeAnimTime = m_aiAnimator.GetDirectionalAnimationLength(primeAnim);
		}
		m_aiActor.OverrideHitEnemies = true;
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
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && !playerController.healthHaver.IsDead && !playerController.IsFalling && ShouldChargePlayer(GameManager.Instance.AllPlayers[i]))
			{
				State = FireState.Priming;
				m_updateEveryFrame = true;
				return BehaviorResult.RunContinuous;
			}
		}
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
			if (endWhenChargeAnimFinishes && !m_aiAnimator.IsPlaying(chargeAnim))
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (State == FireState.Bouncing && !m_aiAnimator.IsPlaying(hitAnim) && !m_aiAnimator.IsPlaying(hitPlayerAnim))
		{
			if (delayWallRecoil && m_storedCollisionNormal.HasValue)
			{
				m_aiActor.knockbackDoer.ApplyKnockback(m_storedCollisionNormal.Value, wallRecoilForce);
				m_storedCollisionNormal = null;
			}
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
		base.Destroy();
		if (stopAtPits)
		{
			SpeculativeRigidbody specRigidbody = m_aiActor.specRigidbody;
			specRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(specRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PitMovementRestrictor));
		}
	}

	private void OnCollision(CollisionData collisionData)
	{
		if (State != FireState.Charging)
		{
			return;
		}
		if ((bool)collisionData.OtherRigidbody)
		{
			SpeculativeRigidbody otherRigidbody = collisionData.OtherRigidbody;
			if ((bool)otherRigidbody.projectile)
			{
				return;
			}
			if ((bool)otherRigidbody.aiActor)
			{
				if (!otherRigidbody.aiActor.OverrideHitEnemies)
				{
					collisionData.OtherRigidbody.RegisterTemporaryCollisionException(collisionData.MyRigidbody, 0.1f);
					collisionData.MyRigidbody.RegisterTemporaryCollisionException(collisionData.OtherRigidbody, 0.1f);
					return;
				}
				float num = collisionData.MyRigidbody.Velocity.ToAngle();
				float num2 = collisionData.Normal.ToAngle();
				if (Mathf.Abs(BraveMathCollege.ClampAngle180(num - num2)) <= 91f)
				{
					return;
				}
				float magnitude = collisionData.MyRigidbody.Velocity.magnitude;
				float magnitude2 = otherRigidbody.Velocity.magnitude;
				float num3 = otherRigidbody.Velocity.ToAngle();
				if (Mathf.Abs(BraveMathCollege.ClampAngle180(num - num3)) < 45f && magnitude < magnitude2 * 1.25f)
				{
					return;
				}
			}
		}
		m_hitPlayer = (bool)collisionData.OtherRigidbody && (bool)collisionData.OtherRigidbody.GetComponent<PlayerController>();
		m_hitWall = collisionData.collisionType == CollisionData.CollisionType.TileMap;
		State = FireState.Bouncing;
		if (!collisionData.OtherRigidbody || !collisionData.OtherRigidbody.knockbackDoer)
		{
			if (delayWallRecoil)
			{
				m_storedCollisionNormal = collisionData.Normal;
				if (collisionData.Normal == Vector2.zero)
				{
					Vector2? chargeDir = m_chargeDir;
					m_storedCollisionNormal = ((!chargeDir.HasValue) ? null : new Vector2?(-chargeDir.Value));
				}
			}
			else
			{
				m_storedCollisionNormal = null;
				m_aiActor.knockbackDoer.ApplyKnockback(collisionData.Normal, wallRecoilForce);
			}
		}
		else
		{
			m_storedCollisionNormal = null;
		}
		if (!collisionData.OtherRigidbody && !string.IsNullOrEmpty(hitWallVfxString))
		{
			string arg = ((m_storedCollisionNormal.Value.x < -0.75f) ? "right" : ((m_storedCollisionNormal.Value.x > 0.75f) ? "left" : ((!(m_storedCollisionNormal.Value.y < -0.75f)) ? "down" : "up")));
			m_aiAnimator.PlayVfx(string.Format(hitWallVfxString, arg));
		}
	}

	private void PitMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation)
		{
			return;
		}
		Func<IntVector2, bool> func = delegate(IntVector2 pixel)
		{
			Vector2 vector = PhysicsEngine.PixelToUnitMidpoint(pixel);
			if (!GameManager.Instance.Dungeon.CellSupportsFalling(vector))
			{
				return false;
			}
			List<SpeculativeRigidbody> platformsAt = GameManager.Instance.Dungeon.GetPlatformsAt(vector);
			if (platformsAt != null)
			{
				for (int i = 0; i < platformsAt.Count; i++)
				{
					if (platformsAt[i].PrimaryPixelCollider.ContainsPixel(pixel))
					{
						return false;
					}
				}
			}
			return true;
		};
		PixelCollider primaryPixelCollider = specRigidbody.PrimaryPixelCollider;
		if (primaryPixelCollider != null)
		{
			IntVector2 intVector = pixelOffset - prevPixelOffset;
			if (intVector == IntVector2.Down && func(primaryPixelCollider.LowerLeft + pixelOffset) && func(primaryPixelCollider.LowerRight + pixelOffset) && (!func(primaryPixelCollider.UpperRight + prevPixelOffset) || !func(primaryPixelCollider.UpperLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Right && func(primaryPixelCollider.LowerRight + pixelOffset) && func(primaryPixelCollider.UpperRight + pixelOffset) && (!func(primaryPixelCollider.UpperLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Up && func(primaryPixelCollider.UpperRight + pixelOffset) && func(primaryPixelCollider.UpperLeft + pixelOffset) && (!func(primaryPixelCollider.LowerLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerRight + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Left && func(primaryPixelCollider.UpperLeft + pixelOffset) && func(primaryPixelCollider.LowerLeft + pixelOffset) && (!func(primaryPixelCollider.LowerRight + prevPixelOffset) || !func(primaryPixelCollider.UpperRight + prevPixelOffset)))
			{
				validLocation = false;
			}
		}
	}

	private void BeginState(FireState state)
	{
		switch (state)
		{
		case FireState.Idle:
			m_aiActor.BehaviorOverridesVelocity = false;
			m_aiAnimator.LockFacingDirection = false;
			break;
		case FireState.Priming:
			m_aiAnimator.PlayUntilFinished(primeAnim, true);
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			break;
		case FireState.Charging:
		{
			AkSoundEngine.PostEvent("Play_ENM_cube_dash_01", GameManager.Instance.PrimaryPlayer.gameObject);
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = m_chargeDir.Value.normalized * chargeSpeed;
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
			if (switchCollidersOnCharge)
			{
				m_enemyCollider.CollisionLayer = CollisionLayer.TileBlocker;
				m_enemyHitbox.Enabled = false;
				m_projectileCollider.Enabled = true;
			}
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
			if (!string.IsNullOrEmpty(trailVfxString))
			{
				Vector2 normalized = m_aiActor.BehaviorVelocity.normalized;
				m_cachedTrailString = string.Format(arg0: (normalized.x > 0.75f) ? "right" : ((normalized.x < -0.75f) ? "left" : ((!(normalized.y > 0.75f)) ? "down" : "up")), format: trailVfxString);
				AIAnimator aiAnimator = m_aiAnimator;
				string cachedTrailString = m_cachedTrailString;
				Vector2? sourceVelocity = normalized;
				aiAnimator.PlayVfx(cachedTrailString, null, sourceVelocity);
			}
			else
			{
				m_cachedTrailString = null;
			}
			m_aiActor.specRigidbody.ForceRegenerate();
			break;
		}
		case FireState.Bouncing:
			if (!string.IsNullOrEmpty(hitPlayerAnim) && m_hitPlayer)
			{
				m_aiAnimator.PlayUntilFinished(hitPlayerAnim, true);
				if (m_aiAnimator.spriteAnimator.CurrentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Loop)
				{
					m_aiAnimator.PlayForDuration(hitPlayerAnim, 1f, true);
				}
			}
			else
			{
				m_aiAnimator.PlayUntilFinished(hitAnim, true);
			}
			if (impactBulletScript != null && !impactBulletScript.IsNull && m_hitWall)
			{
				if (!m_bulletSource)
				{
					m_bulletSource = shootPoint.GetOrAddComponent<BulletScriptSource>();
				}
				m_bulletSource.BulletManager = m_aiActor.bulletBank;
				m_bulletSource.BulletScript = impactBulletScript;
				m_bulletSource.Initialize();
			}
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
		if (!string.IsNullOrEmpty(m_cachedTrailString))
		{
			m_aiAnimator.StopVfx(m_cachedTrailString);
			m_cachedTrailString = null;
		}
		m_aiActor.DoDustUps = m_cachedDoDustUps;
		m_aiActor.PathableTiles = m_cachedPathableTiles;
		if (switchCollidersOnCharge)
		{
			m_enemyCollider.CollisionLayer = CollisionLayer.EnemyCollider;
			m_enemyHitbox.Enabled = true;
			m_projectileCollider.Enabled = false;
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_aiActor.specRigidbody);
		}
	}

	private bool ShouldChargePlayer(PlayerController player)
	{
		Vector2 vector = player.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		if (leadAmount > 0f)
		{
			Vector2 b = vector + player.specRigidbody.Velocity * m_primeAnimTime;
			vector = Vector2.Lerp(vector, b, leadAmount);
		}
		Vector2 unitBottomLeft = m_aiActor.specRigidbody.UnitBottomLeft;
		Vector2 unitTopRight = m_aiActor.specRigidbody.UnitTopRight;
		m_chargeDir = null;
		if (BraveMathCollege.AABBContains(new Vector2(unitBottomLeft.x - chargeRange, unitBottomLeft.y), unitTopRight, vector))
		{
			m_chargeDir = -Vector2.right;
		}
		else if (BraveMathCollege.AABBContains(unitBottomLeft, new Vector2(unitTopRight.x + chargeRange, unitTopRight.y), vector))
		{
			m_chargeDir = Vector2.right;
		}
		else if (BraveMathCollege.AABBContains(new Vector2(unitBottomLeft.x, unitBottomLeft.y - chargeRange), unitTopRight, vector))
		{
			m_chargeDir = -Vector2.up;
		}
		else if (BraveMathCollege.AABBContains(unitBottomLeft, new Vector2(unitTopRight.x, unitTopRight.y + chargeRange), vector))
		{
			m_chargeDir = Vector2.up;
		}
		if (m_chargeDir.HasValue)
		{
			return true;
		}
		return false;
	}
}
