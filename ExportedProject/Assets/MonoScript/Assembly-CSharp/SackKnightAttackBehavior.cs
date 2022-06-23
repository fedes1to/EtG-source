using Dungeonator;
using UnityEngine;

public class SackKnightAttackBehavior : AttackBehaviorBase
{
	private enum MechaJunkanAttackType
	{
		SWORD,
		GUN,
		ROCKET
	}

	private enum State
	{
		Idle,
		Charging,
		Leaping
	}

	public float maxAttackDistance = 1f;

	public float minAttackDistance = 0.1f;

	public float SquireAttackDamage = 3f;

	public float HedgeKnightAttackDamage = 5f;

	public float KnightAttackDamage = 7f;

	public float KnightLieutenantAttackDamage = 7f;

	public float KnightCommanderAttackDamage = 7f;

	public float HolyKnightAttackDamage = 7f;

	public float MechAttackDamage = 20f;

	public float AngelicKnightAttackDuration = 5f;

	public float AngelicKnightAngleVariance = 30f;

	public float SquireCooldownTime = 3f;

	public float HedgeKnightCooldownTime = 1.75f;

	public float KnightCooldownTime = 0.5f;

	public float KnightLieutenantCooldownTime = 0.5f;

	public float KnightCommanderCooldownTime = 2f;

	public float HolyKnightCooldownTime = 2f;

	public float AngelicKnightCooldownTime = 1f;

	public float AngelicKnightDesiredDistance = 6f;

	public float MechCooldownTime = 2f;

	public float MechGunWeight = 1f;

	public float MechRocketWeight = 1f;

	public float MechSwordWeight = 1f;

	public string SwordHitVFX;

	public GameActorHealthEffect PoisonEffectForTrashSynergy;

	private MechaJunkanAttackType m_mechAttack;

	private float m_angelShootElapsed;

	private float m_angelElapsed;

	private SeekTargetBehavior m_seekBehavior;

	private SackKnightController m_knight;

	private float m_elapsed;

	private int m_attackCounter;

	private float m_cooldownTimer;

	private State m_state;

	private bool m_isTargetPitBoss;

	private float CurrentFormCooldown
	{
		get
		{
			switch (m_knight.CurrentForm)
			{
			case SackKnightController.SackKnightPhase.PEASANT:
			case SackKnightController.SackKnightPhase.SQUIRE:
				return SquireCooldownTime;
			case SackKnightController.SackKnightPhase.HEDGE_KNIGHT:
				return HedgeKnightCooldownTime;
			case SackKnightController.SackKnightPhase.KNIGHT:
				return KnightCooldownTime;
			case SackKnightController.SackKnightPhase.KNIGHT_LIEUTENANT:
				return KnightLieutenantCooldownTime;
			case SackKnightController.SackKnightPhase.KNIGHT_COMMANDER:
				return KnightCommanderCooldownTime;
			case SackKnightController.SackKnightPhase.HOLY_KNIGHT:
				return HolyKnightCooldownTime;
			case SackKnightController.SackKnightPhase.ANGELIC_KNIGHT:
				return AngelicKnightCooldownTime;
			case SackKnightController.SackKnightPhase.MECHAJUNKAN:
				return MechCooldownTime;
			default:
				return SquireCooldownTime;
			}
		}
	}

	public override void Start()
	{
		base.Start();
		m_knight = m_aiActor.GetComponent<SackKnightController>();
		BehaviorSpeculator behaviorSpeculator = m_aiActor.behaviorSpeculator;
		for (int i = 0; i < behaviorSpeculator.MovementBehaviors.Count; i++)
		{
			if (behaviorSpeculator.MovementBehaviors[i] is SeekTargetBehavior)
			{
				m_seekBehavior = behaviorSpeculator.MovementBehaviors[i] as SeekTargetBehavior;
			}
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		if ((bool)targetRigidbody && (bool)targetRigidbody.aiActor && (bool)targetRigidbody.healthHaver && targetRigidbody.healthHaver.IsBoss)
		{
			m_isTargetPitBoss = GameManager.Instance.Dungeon.CellSupportsFalling(targetRigidbody.UnitCenter);
		}
		else
		{
			m_isTargetPitBoss = false;
		}
		if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.MECHAJUNKAN)
		{
			if (m_isTargetPitBoss)
			{
				minAttackDistance = 0.1f;
				maxAttackDistance = ((m_mechAttack != 0) ? 12f : 2.5f);
			}
			else
			{
				minAttackDistance = 0.1f;
				maxAttackDistance = ((m_mechAttack != 0) ? 12f : 1.5f);
			}
		}
		else if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
		{
			minAttackDistance = 0.1f;
			maxAttackDistance = 12f;
		}
		else if (m_isTargetPitBoss)
		{
			minAttackDistance = 0.1f;
			maxAttackDistance = 2f;
		}
		else
		{
			minAttackDistance = 0.1f;
			maxAttackDistance = 1f;
		}
		DecrementTimer(ref m_cooldownTimer);
		if (m_seekBehavior != null)
		{
			if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
			{
				m_seekBehavior.ExternalCooldownSource = false;
				m_seekBehavior.StopWhenInRange = true;
				m_seekBehavior.CustomRange = AngelicKnightDesiredDistance;
			}
			else
			{
				m_seekBehavior.ExternalCooldownSource = m_cooldownTimer > 0f;
				m_seekBehavior.StopWhenInRange = false;
				m_seekBehavior.CustomRange = -1f;
			}
		}
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_knight == null || m_knight.CurrentForm == SackKnightController.SackKnightPhase.PEASANT)
		{
			return BehaviorResult.Continue;
		}
		if (m_cooldownTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 targetPoint = GetTargetPoint(m_aiActor.TargetRigidbody, unitCenter);
		float num = Vector2.Distance(unitCenter, targetPoint);
		bool flag = m_knight.CurrentForm != SackKnightController.SackKnightPhase.ANGELIC_KNIGHT || m_aiActor.HasLineOfSightToTarget;
		if (num < maxAttackDistance && flag)
		{
			m_state = State.Charging;
			if (m_knight.CurrentForm != SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
			{
				m_aiActor.ClearPath();
				m_aiActor.BehaviorOverridesVelocity = true;
				m_aiActor.BehaviorVelocity = Vector2.zero;
			}
			m_updateEveryFrame = true;
			m_elapsed = 0f;
			m_attackCounter = 0;
			return (m_knight.CurrentForm != SackKnightController.SackKnightPhase.ANGELIC_KNIGHT) ? BehaviorResult.RunContinuous : BehaviorResult.RunContinuousInClass;
		}
		return BehaviorResult.Continue;
	}

	private Vector2 GetTargetPoint(SpeculativeRigidbody targetRigidbody, Vector2 myCenter)
	{
		PixelCollider hitboxPixelCollider = targetRigidbody.HitboxPixelCollider;
		return BraveMathCollege.ClosestPointOnRectangle(myCenter, hitboxPixelCollider.UnitBottomLeft, hitboxPixelCollider.UnitDimensions);
	}

	private ContinuousBehaviorResult DoMechBlasters()
	{
		m_angelElapsed += BraveTime.DeltaTime;
		m_angelShootElapsed += BraveTime.DeltaTime;
		if (!m_aiAnimator.IsPlaying("fire"))
		{
			m_aiAnimator.PlayUntilCancelled("fire", true);
		}
		if (m_angelShootElapsed > 0.1f)
		{
			if ((bool)m_aiActor.TargetRigidbody)
			{
				Vector2 unitCenter = m_aiActor.TargetRigidbody.UnitCenter;
				float num = BraveMathCollege.Atan2Degrees(unitCenter - m_aiActor.CenterPosition);
				m_aiAnimator.LockFacingDirection = true;
				m_aiAnimator.FacingDirection = num;
				Vector2 position = m_aiActor.transform.Find("gun").position;
				GameObject gameObject = m_aiActor.bulletBank.CreateProjectileFromBank(position, num, "blaster");
				Vector2 value = ((!(BraveMathCollege.AbsAngleBetween(m_aiAnimator.FacingDirection, 0f) > 90f)) ? new Vector2(1f, 0f) : new Vector2(-1f, 0f));
				m_aiAnimator.PlayVfx("mechGunVFX", value);
				if ((bool)gameObject && (bool)gameObject.GetComponent<Projectile>() && (bool)m_aiShooter && m_aiShooter.PostProcessProjectile != null)
				{
					m_aiShooter.PostProcessProjectile(gameObject.GetComponent<Projectile>());
				}
				AkSoundEngine.SetSwitch("WPN_Guns", "Sack", m_knight.gameObject);
				AkSoundEngine.PostEvent("Play_WPN_gun_shot_01", m_knight.gameObject);
			}
			else
			{
				m_aiAnimator.LockFacingDirection = false;
			}
			m_angelShootElapsed -= 0.1f;
		}
		if (m_angelElapsed >= 2f)
		{
			m_cooldownTimer = CurrentFormCooldown;
			m_state = State.Idle;
			m_aiAnimator.EndAnimationIf("fire");
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	private ContinuousBehaviorResult DoMechRockets()
	{
		if (m_state == State.Charging)
		{
			m_state = State.Leaping;
			if (!m_aiActor.TargetRigidbody || !m_aiActor.TargetRigidbody.enabled)
			{
				m_state = State.Idle;
				m_aiAnimator.LockFacingDirection = false;
				return ContinuousBehaviorResult.Finished;
			}
			Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
			Vector2 targetPoint = GetTargetPoint(m_aiActor.TargetRigidbody, unitCenter);
			float num = Vector2.Distance(unitCenter, targetPoint);
			if (num > maxAttackDistance)
			{
				targetPoint = unitCenter + (targetPoint - unitCenter).normalized * maxAttackDistance;
				num = Vector2.Distance(unitCenter, targetPoint);
			}
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.PlayUntilFinished("rocket", true);
		}
		else if (m_state == State.Leaping)
		{
			m_elapsed += m_deltaTime;
			float num2 = 1f;
			m_angelShootElapsed += BraveTime.DeltaTime;
			if (m_angelShootElapsed > 0.1f)
			{
				if ((bool)m_aiActor.TargetRigidbody)
				{
					Vector2 unitCenter2 = m_aiActor.TargetRigidbody.UnitCenter;
					float num3 = BraveMathCollege.Atan2Degrees(unitCenter2 - m_aiActor.CenterPosition);
					num3 += Random.Range(0f - AngelicKnightAngleVariance, AngelicKnightAngleVariance);
					Vector2 position = m_aiActor.CenterPosition + new Vector2(Random.Range(-0.25f, 0.25f), 0.75f);
					GameObject gameObject = m_aiActor.bulletBank.CreateProjectileFromBank(position, num3, "mechRocket");
					if ((bool)gameObject)
					{
						RobotechProjectile component = gameObject.GetComponent<RobotechProjectile>();
						component.Owner = m_aiActor.CompanionOwner;
						Vector2 dirVec = Quaternion.Euler(0f, 0f, Random.Range(-25, 25)) * Vector2.up;
						component.ForceCurveDirection(dirVec, Random.Range(0.04f, 0.06f));
						component.Ramp(4f, 0.5f);
						if ((bool)m_aiShooter && m_aiShooter.PostProcessProjectile != null)
						{
							m_aiShooter.PostProcessProjectile(gameObject.GetComponent<Projectile>());
						}
					}
					AkSoundEngine.SetSwitch("WPN_Guns", "Sack", m_knight.gameObject);
					AkSoundEngine.PostEvent("Play_WPN_gun_shot_01", m_knight.gameObject);
				}
				m_angelShootElapsed -= 0.1f;
			}
			if (m_elapsed >= num2)
			{
				m_cooldownTimer = CurrentFormCooldown;
				m_aiAnimator.LockFacingDirection = false;
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.MECHAJUNKAN)
		{
			if (m_mechAttack == MechaJunkanAttackType.GUN)
			{
				return DoMechBlasters();
			}
			if (m_mechAttack == MechaJunkanAttackType.ROCKET)
			{
				return DoMechRockets();
			}
		}
		if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
		{
			HandleAngelAttackFrame();
			if (m_angelElapsed >= AngelicKnightCooldownTime)
			{
				m_cooldownTimer = CurrentFormCooldown;
				m_state = State.Idle;
				m_aiAnimator.EndAnimationIf("attack");
				return ContinuousBehaviorResult.Finished;
			}
		}
		else
		{
			if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.MECHAJUNKAN)
			{
				m_aiAnimator.LockFacingDirection = true;
			}
			if (m_state == State.Charging)
			{
				m_state = State.Leaping;
				if (!m_aiActor.TargetRigidbody || !m_aiActor.TargetRigidbody.enabled)
				{
					m_state = State.Idle;
					m_aiAnimator.LockFacingDirection = false;
					return ContinuousBehaviorResult.Finished;
				}
				Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
				Vector2 vector = GetTargetPoint(m_aiActor.TargetRigidbody, unitCenter);
				float num = Vector2.Distance(unitCenter, vector);
				if (num > maxAttackDistance)
				{
					vector = unitCenter + (vector - unitCenter).normalized * maxAttackDistance;
					num = Vector2.Distance(unitCenter, vector);
				}
				m_aiActor.ClearPath();
				m_aiActor.BehaviorOverridesVelocity = true;
				m_aiActor.BehaviorVelocity = (vector - unitCenter).normalized * (num / 0.25f);
				float facingDirection = m_aiActor.BehaviorVelocity.ToAngle();
				m_aiAnimator.LockFacingDirection = true;
				m_aiAnimator.FacingDirection = facingDirection;
				if (m_isTargetPitBoss)
				{
					m_aiActor.BehaviorVelocity = Vector2.zero;
				}
				m_aiActor.PathableTiles = CellTypes.FLOOR | CellTypes.PIT;
				m_aiActor.DoDustUps = false;
				m_aiAnimator.PlayUntilFinished("attack", true);
				if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.MECHAJUNKAN)
				{
					string text = ((!(BraveMathCollege.AbsAngleBetween(m_aiAnimator.FacingDirection, 0f) > 90f)) ? "mechSwordR" : "mechSwordL");
					AIAnimator aiAnimator = m_aiAnimator;
					string name = text;
					Vector2? position = m_knight.transform.position.XY();
					aiAnimator.PlayVfx(name, null, null, position);
				}
			}
			else if (m_state == State.Leaping)
			{
				m_elapsed += m_deltaTime;
				float num2 = 0.25f;
				if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.MECHAJUNKAN)
				{
					num2 = 0.4f;
				}
				if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.KNIGHT_COMMANDER || m_knight.CurrentForm == SackKnightController.SackKnightPhase.HOLY_KNIGHT || m_knight.CurrentForm == SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
				{
					num2 = 1.2f;
					if ((double)m_elapsed >= 0.7 && m_attackCounter < 1)
					{
						m_attackCounter = 1;
						DoAttack();
					}
					if (m_elapsed >= 0.95f && m_attackCounter < 2)
					{
						m_attackCounter = 2;
						DoAttack();
					}
				}
				if (m_elapsed >= num2)
				{
					m_cooldownTimer = CurrentFormCooldown;
					m_aiAnimator.LockFacingDirection = false;
					return ContinuousBehaviorResult.Finished;
				}
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	private void HandleAngelAttackFrame()
	{
		m_angelElapsed += BraveTime.DeltaTime;
		m_angelShootElapsed += BraveTime.DeltaTime;
		if (!m_aiAnimator.IsPlaying("attack"))
		{
			m_aiAnimator.PlayUntilCancelled("attack", true);
		}
		if (!(m_angelShootElapsed > 0.1f))
		{
			return;
		}
		if ((bool)m_aiActor.TargetRigidbody)
		{
			Vector2 unitCenter = m_aiActor.TargetRigidbody.UnitCenter;
			float num = BraveMathCollege.Atan2Degrees(unitCenter - m_aiActor.CenterPosition);
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.FacingDirection = num;
			num += Random.Range(0f - AngelicKnightAngleVariance, AngelicKnightAngleVariance);
			string transformName = ((!(BraveMathCollege.AbsAngleBetween(m_aiAnimator.FacingDirection, 0f) < 90f)) ? "left shoot point" : "right shoot point");
			Vector2 position = m_aiActor.bulletBank.GetTransform(transformName).position + new Vector3(0f, (float)Random.Range(-3, 4) / 16f);
			GameObject gameObject = m_aiActor.bulletBank.CreateProjectileFromBank(position, num, "angel");
			if ((bool)gameObject && (bool)gameObject.GetComponent<Projectile>() && (bool)m_aiShooter && m_aiShooter.PostProcessProjectile != null)
			{
				m_aiShooter.PostProcessProjectile(gameObject.GetComponent<Projectile>());
			}
			AkSoundEngine.SetSwitch("WPN_Guns", "Sack", m_knight.gameObject);
			AkSoundEngine.PostEvent("Play_WPN_gun_shot_01", m_knight.gameObject);
		}
		else
		{
			m_aiAnimator.LockFacingDirection = false;
		}
		m_angelShootElapsed -= 0.1f;
	}

	private void DoAttack()
	{
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		if (!targetRigidbody || !targetRigidbody.enabled || !targetRigidbody.CollideWithOthers || !targetRigidbody.healthHaver || ((bool)targetRigidbody.aiActor && targetRigidbody.aiActor.IsGone))
		{
			return;
		}
		float num = 5f;
		switch (m_knight.CurrentForm)
		{
		case SackKnightController.SackKnightPhase.PEASANT:
		case SackKnightController.SackKnightPhase.SQUIRE:
			num = SquireAttackDamage;
			break;
		case SackKnightController.SackKnightPhase.HEDGE_KNIGHT:
			num = HedgeKnightAttackDamage;
			break;
		case SackKnightController.SackKnightPhase.KNIGHT:
			num = KnightAttackDamage;
			break;
		case SackKnightController.SackKnightPhase.KNIGHT_LIEUTENANT:
			num = KnightLieutenantAttackDamage;
			break;
		case SackKnightController.SackKnightPhase.KNIGHT_COMMANDER:
			num = KnightCommanderAttackDamage / 3f;
			break;
		case SackKnightController.SackKnightPhase.HOLY_KNIGHT:
			num = HolyKnightAttackDamage / 3f;
			break;
		case SackKnightController.SackKnightPhase.MECHAJUNKAN:
			num = MechAttackDamage;
			break;
		default:
			num = SquireAttackDamage;
			break;
		}
		if ((bool)m_aiActor.CompanionOwner && PassiveItem.IsFlagSetForCharacter(m_aiActor.CompanionOwner, typeof(BattleStandardItem)))
		{
			num *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
		}
		if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.KNIGHT_COMMANDER || m_knight.CurrentForm == SackKnightController.SackKnightPhase.HOLY_KNIGHT || m_knight.CurrentForm == SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
		{
			VFXPool hitVFX = null;
			if (!string.IsNullOrEmpty(SwordHitVFX))
			{
				AIAnimator.NamedVFXPool namedVFXPool = m_aiAnimator.OtherVFX.Find((AIAnimator.NamedVFXPool vfx) => vfx.name == SwordHitVFX);
				if (namedVFXPool != null)
				{
					hitVFX = namedVFXPool.vfxPool;
				}
			}
			Exploder.DoRadialDamage(num, m_aiActor.specRigidbody.UnitCenter, 2.5f, false, true, m_knight.CurrentForm == SackKnightController.SackKnightPhase.ANGELIC_KNIGHT, hitVFX);
			return;
		}
		targetRigidbody.healthHaver.ApplyDamage(num, m_aiActor.specRigidbody.Velocity, "Ser Junkan");
		if (m_aiActor.CompanionOwner.HasActiveBonusSynergy(CustomSynergyType.TRASHJUNKAN) && (bool)targetRigidbody.aiActor)
		{
			targetRigidbody.aiActor.ApplyEffect(PoisonEffectForTrashSynergy);
		}
		if (!string.IsNullOrEmpty(SwordHitVFX))
		{
			PixelCollider pixelCollider = targetRigidbody.GetPixelCollider(ColliderType.HitBox);
			Vector2 vector = BraveMathCollege.ClosestPointOnRectangle(m_aiActor.CenterPosition, pixelCollider.UnitBottomLeft, pixelCollider.UnitDimensions);
			Vector2 vector2 = vector - m_aiActor.CenterPosition;
			if (vector2 != Vector2.zero)
			{
				vector += vector2.normalized * 0.1875f;
			}
			AIAnimator aiAnimator = m_aiAnimator;
			string swordHitVFX = SwordHitVFX;
			Vector2? position = vector;
			aiAnimator.PlayVfx(swordHitVFX, null, null, position);
		}
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_angelShootElapsed = 0f;
		m_angelElapsed = 0f;
		if (m_knight.CurrentForm != SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
		{
			if ((m_knight.CurrentForm == SackKnightController.SackKnightPhase.KNIGHT_COMMANDER || m_knight.CurrentForm == SackKnightController.SackKnightPhase.HOLY_KNIGHT) && m_attackCounter < 1)
			{
				DoAttack();
			}
			if ((m_knight.CurrentForm == SackKnightController.SackKnightPhase.KNIGHT_COMMANDER || m_knight.CurrentForm == SackKnightController.SackKnightPhase.HOLY_KNIGHT) && m_attackCounter < 2)
			{
				DoAttack();
			}
			DoAttack();
		}
		else
		{
			m_aiAnimator.EndAnimation();
		}
		if (m_knight.CurrentForm == SackKnightController.SackKnightPhase.MECHAJUNKAN)
		{
			m_mechAttack = SelectNewMechAttack();
		}
		m_state = State.Idle;
		if (!m_aiActor.IsFlying)
		{
			m_aiActor.PathableTiles = CellTypes.FLOOR;
		}
		m_aiActor.DoDustUps = true;
		m_aiActor.BehaviorOverridesVelocity = false;
		m_aiAnimator.LockFacingDirection = false;
		m_updateEveryFrame = false;
	}

	private MechaJunkanAttackType SelectNewMechAttack()
	{
		float num = MechGunWeight + MechRocketWeight + MechSwordWeight;
		float num2 = Random.value * num;
		if (num2 < MechGunWeight)
		{
			return MechaJunkanAttackType.GUN;
		}
		if (num2 < MechGunWeight + MechRocketWeight)
		{
			return MechaJunkanAttackType.ROCKET;
		}
		return MechaJunkanAttackType.SWORD;
	}

	public override bool IsReady()
	{
		return true;
	}

	public override float GetMinReadyRange()
	{
		return maxAttackDistance;
	}

	public override float GetMaxRange()
	{
		return maxAttackDistance;
	}
}
