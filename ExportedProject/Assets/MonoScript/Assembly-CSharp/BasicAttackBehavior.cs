using Dungeonator;
using FullInspector;
using UnityEngine;

public abstract class BasicAttackBehavior : AttackBehaviorBase
{
	public class ResetCooldownOnDamage
	{
		public bool Cooldown = true;

		public bool AttackCooldown;

		public bool GlobalCooldown;

		public bool GroupCooldown;

		[InspectorTooltip("If set, cooldowns can not be reset again for this amount of time after taking damage.")]
		public float ResetCooldown;
	}

	public static bool DrawDebugFiringArea;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("Time before THIS behavior may be run again.")]
	public float Cooldown = 1f;

	[InspectorTooltip("Time variance added to the base cooldown.")]
	[InspectorCategory("Conditions")]
	public float CooldownVariance;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("Time before ATTACK behaviors may be run again.")]
	public float AttackCooldown;

	[InspectorTooltip("Time before ANY behavior may be run again.")]
	[InspectorCategory("Conditions")]
	public float GlobalCooldown;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("Time after the enemy becomes active before this attack can be used for the first time.")]
	public float InitialCooldown;

	[InspectorTooltip("Time variance added to the initial cooldown.")]
	[InspectorCategory("Conditions")]
	public float InitialCooldownVariance;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("Name of the cooldown group to use; all behaviors on this BehaviorSpeculator with a matching group will use this cooldown value.")]
	[InspectorShowIf("ShowGroupCooldown")]
	public string GroupName;

	[InspectorTooltip("Time before any behaviors with a matching group name may be run again.")]
	[InspectorShowIf("ShowGroupCooldown")]
	[InspectorCategory("Conditions")]
	public float GroupCooldown;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("Minimum range")]
	public float MinRange;

	[InspectorTooltip("Range")]
	[InspectorCategory("Conditions")]
	public float Range;

	[InspectorTooltip("Minimum distance from a wall")]
	[InspectorCategory("Conditions")]
	public float MinWallDistance;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("If the room contains more than this number of enemies, this attack wont be used.")]
	public float MaxEnemiesInRoom;

	[InspectorShowIf("ShowMinHealthThreshold")]
	[InspectorCategory("Conditions")]
	[InspectorTooltip("The minimum amount of health an enemy can have and still use this attack.\n(Raising this means the enemy wont use this attack at low health)")]
	public float MinHealthThreshold;

	[InspectorTooltip("The maximum amount of health an enemy can have and still use this attack.\n(Lowering this means the enemy wont use this attack until they lose health)")]
	[InspectorShowIf("ShowMaxHealthThreshold")]
	[InspectorCategory("Conditions")]
	public float MaxHealthThreshold = 1f;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("The attack can only be used once each time a new health threshold is met")]
	[InspectorShowIf("ShowHealthThresholds")]
	public float[] HealthThresholds = new float[0];

	[InspectorTooltip("If true, the attack can build up multiple uses by passing multiple thresholds in quick succession")]
	[InspectorCategory("Conditions")]
	[InspectorShowIf("ShowHealthThresholds")]
	public bool AccumulateHealthThresholds = true;

	[InspectorCategory("Conditions")]
	[InspectorShowIf("ShowTargetArea")]
	public ShootBehavior.FiringAreaStyle targetAreaStyle;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("The attack can only be used for Black Phantom versions of this enemy")]
	public bool IsBlackPhantom;

	[InspectorShowIf("ShowResetCooldownOnDamage")]
	[InspectorCategory("Conditions")]
	[InspectorTooltip("Resets the appropriate cooldowns when the actor takes damage.")]
	[InspectorNullable]
	public ResetCooldownOnDamage resetCooldownOnDamage;

	[InspectorCategory("Conditions")]
	[InspectorTooltip("Require line of sight to target. Expensive! Use for companions.")]
	public bool RequiresLineOfSight;

	[InspectorTooltip("This attack can only be used this number of times.")]
	[InspectorCategory("Conditions")]
	public int MaxUsages;

	protected float m_cooldownTimer;

	protected float m_resetCooldownOnDamageCooldown;

	protected BehaviorSpeculator m_behaviorSpeculator;

	protected int m_healthThresholdCredits;

	protected float m_lowestRecordedHealthPercentage = float.MaxValue;

	protected int m_numTimesUsed;

	protected static int m_arcCount;

	protected static int m_lastFrame;

	private bool ShowGroupCooldown()
	{
		return GroupCooldown > 0f || !string.IsNullOrEmpty(GroupName);
	}

	private bool ShowMinHealthThreshold()
	{
		return MinHealthThreshold != 0f;
	}

	private bool ShowMaxHealthThreshold()
	{
		return MaxHealthThreshold != 1f;
	}

	private bool ShowHealthThresholds()
	{
		return HealthThresholds.Length > 0;
	}

	private bool ShowTargetArea()
	{
		return targetAreaStyle != null;
	}

	private bool ShowResetCooldownOnDamage()
	{
		return resetCooldownOnDamage != null;
	}

	public override void Start()
	{
		base.Start();
		m_cooldownTimer = InitialCooldown;
		if (InitialCooldownVariance > 0f)
		{
			m_cooldownTimer += Random.Range(0f - InitialCooldownVariance, InitialCooldownVariance);
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_cooldownTimer, true);
		DecrementTimer(ref m_resetCooldownOnDamageCooldown, true);
		if (HealthThresholds.Length > 0)
		{
			float currentHealthPercentage = m_aiActor.healthHaver.GetCurrentHealthPercentage();
			if (currentHealthPercentage < m_lowestRecordedHealthPercentage)
			{
				for (int i = 0; i < HealthThresholds.Length; i++)
				{
					if (HealthThresholds[i] >= currentHealthPercentage && HealthThresholds[i] < m_lowestRecordedHealthPercentage)
					{
						m_healthThresholdCredits++;
					}
				}
				m_lowestRecordedHealthPercentage = currentHealthPercentage;
			}
		}
		if (DrawDebugFiringArea)
		{
			if (Time.frameCount != m_lastFrame)
			{
				m_arcCount = 0;
				m_lastFrame = Time.frameCount;
			}
			if ((bool)m_aiActor.TargetRigidbody && targetAreaStyle != null)
			{
				targetAreaStyle.DrawDebugLines(GetOrigin(targetAreaStyle.targetAreaOrigin), m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox), m_aiActor);
			}
		}
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		return BehaviorResult.Continue;
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		m_behaviorSpeculator = gameObject.GetComponent<BehaviorSpeculator>();
		if (resetCooldownOnDamage != null)
		{
			m_aiActor.healthHaver.OnDamaged += OnDamaged;
		}
	}

	public override bool IsReady()
	{
		if (MinHealthThreshold > 0f && m_aiActor.healthHaver.GetCurrentHealthPercentage() < MinHealthThreshold)
		{
			return false;
		}
		if (MaxHealthThreshold < 1f && m_aiActor.healthHaver.GetCurrentHealthPercentage() > MaxHealthThreshold)
		{
			return false;
		}
		if (HealthThresholds.Length > 0 && m_healthThresholdCredits <= 0)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(GroupName) && m_behaviorSpeculator.GetGroupCooldownTimer(GroupName) > 0f)
		{
			return false;
		}
		if (IsBlackPhantom && !m_aiActor.IsBlackPhantom)
		{
			return false;
		}
		if (MinRange > 0f)
		{
			if (!m_aiActor.TargetRigidbody)
			{
				return false;
			}
			Vector2 unitCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			float num = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, unitCenter);
			if (num < MinRange)
			{
				return false;
			}
		}
		if (Range > 0f)
		{
			if (!m_aiActor.TargetRigidbody)
			{
				return false;
			}
			Vector2 unitCenter2 = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			float num2 = Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, unitCenter2);
			if (num2 > Range)
			{
				return false;
			}
		}
		if (MinWallDistance > 0f)
		{
			PixelCollider hitboxPixelCollider = m_aiActor.specRigidbody.HitboxPixelCollider;
			CellArea area = m_aiActor.ParentRoom.area;
			if (hitboxPixelCollider.UnitLeft - area.UnitLeft < MinWallDistance)
			{
				return false;
			}
			if (area.UnitRight - hitboxPixelCollider.UnitRight < MinWallDistance)
			{
				return false;
			}
			if (hitboxPixelCollider.UnitBottom - area.UnitBottom < MinWallDistance)
			{
				return false;
			}
			if (area.UnitTop - hitboxPixelCollider.UnitTop < MinWallDistance)
			{
				return false;
			}
		}
		if (MaxEnemiesInRoom > 0f && (float)m_aiActor.ParentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) > MaxEnemiesInRoom)
		{
			return false;
		}
		if (!TargetInFiringArea())
		{
			return false;
		}
		if (RequiresLineOfSight && !TargetInLineOfSight())
		{
			return false;
		}
		if (MaxUsages > 0 && m_numTimesUsed >= MaxUsages)
		{
			return false;
		}
		return m_cooldownTimer <= 0f;
	}

	public override float GetMinReadyRange()
	{
		if (Range > 0f)
		{
			return (!IsReady()) ? (-1f) : Range;
		}
		return -1f;
	}

	public override float GetMaxRange()
	{
		if (Range > 0f)
		{
			return Range;
		}
		return -1f;
	}

	public override void OnActorPreDeath()
	{
		if (resetCooldownOnDamage != null)
		{
			m_aiActor.healthHaver.OnDamaged -= OnDamaged;
		}
		base.OnActorPreDeath();
	}

	protected virtual void UpdateCooldowns()
	{
		m_cooldownTimer = Cooldown;
		if (CooldownVariance > 0f)
		{
			m_cooldownTimer += Random.Range(0f - CooldownVariance, CooldownVariance);
		}
		if (AttackCooldown > 0f)
		{
			m_behaviorSpeculator.AttackCooldown = AttackCooldown;
		}
		if (GlobalCooldown > 0f)
		{
			m_behaviorSpeculator.GlobalCooldown = GlobalCooldown;
		}
		if (GroupCooldown > 0f)
		{
			m_behaviorSpeculator.SetGroupCooldown(GroupName, GroupCooldown);
		}
		if (HealthThresholds.Length > 0 && m_healthThresholdCredits > 0)
		{
			m_healthThresholdCredits = (AccumulateHealthThresholds ? (m_healthThresholdCredits - 1) : 0);
		}
		m_numTimesUsed++;
	}

	protected virtual Vector2 GetOrigin(ShootBehavior.TargetAreaOrigin origin)
	{
		if (origin == ShootBehavior.TargetAreaOrigin.ShootPoint)
		{
			Debug.LogWarning("ColliderType.ShootPoint is not supported for base BasicAttackBehaviors!");
		}
		return m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
	}

	protected bool TargetInLineOfSight()
	{
		return m_aiActor.HasLineOfSightToTarget;
	}

	protected bool TargetInFiringArea()
	{
		if (targetAreaStyle == null)
		{
			return true;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return false;
		}
		return targetAreaStyle.TargetInFiringArea(GetOrigin(targetAreaStyle.targetAreaOrigin), m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox));
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (resetCooldownOnDamage != null && m_resetCooldownOnDamageCooldown <= 0f)
		{
			bool flag = false;
			if (resetCooldownOnDamage.Cooldown && m_cooldownTimer > 0f)
			{
				m_cooldownTimer = 0f;
				flag = true;
			}
			if (resetCooldownOnDamage.AttackCooldown && m_behaviorSpeculator.AttackCooldown > 0f)
			{
				m_behaviorSpeculator.AttackCooldown = 0f;
				flag = true;
			}
			if (resetCooldownOnDamage.GlobalCooldown && m_behaviorSpeculator.GlobalCooldown > 0f)
			{
				m_behaviorSpeculator.GlobalCooldown = 0f;
				flag = true;
			}
			if (resetCooldownOnDamage.GroupCooldown && m_behaviorSpeculator.GetGroupCooldownTimer(GroupName) > 0f)
			{
				m_behaviorSpeculator.SetGroupCooldown(GroupName, 0f);
				flag = true;
			}
			if (flag && resetCooldownOnDamage.ResetCooldown > 0f)
			{
				m_resetCooldownOnDamageCooldown = resetCooldownOnDamage.ResetCooldown;
			}
		}
	}
}
