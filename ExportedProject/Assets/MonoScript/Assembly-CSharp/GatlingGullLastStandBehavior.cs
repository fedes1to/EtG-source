using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/LastStandBehavior")]
public class GatlingGullLastStandBehavior : BasicAttackBehavior
{
	public float HealthThreshold = 5f;

	public float AngleVariance = 20f;

	public string OverrideBulletName;

	private bool m_passthrough;

	private GatlingGullLeapBehavior m_leapBehavior;

	private RoomHandler m_room;

	private readonly Vector2 m_leapPosition = new Vector2(19f, 13f);

	private bool m_isInDeathPosition;

	private readonly string[] m_animNames = new string[8] { "fire_up", "fire_north_east", "fire_right", "fire_south_east", "fire_down", "fire_south_west", "fire_left", "fire_north_west" };

	private string m_cachedAnimationName;

	public override void Start()
	{
		base.Start();
		AttackBehaviorGroup attackBehaviorGroup = (AttackBehaviorGroup)m_aiActor.behaviorSpeculator.AttackBehaviors.Find((AttackBehaviorBase b) => b is AttackBehaviorGroup);
		m_leapBehavior = (GatlingGullLeapBehavior)attackBehaviorGroup.AttackBehaviors.Find((AttackBehaviorGroup.AttackGroupItem b) => b.Behavior is GatlingGullLeapBehavior).Behavior;
		m_room = GameManager.Instance.Dungeon.GetRoomFromPosition(m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		m_aiActor.healthHaver.minimumHealth = 1f;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (m_passthrough)
		{
			m_leapBehavior.Upkeep();
		}
	}

	public override bool OverrideOtherBehaviors()
	{
		return m_aiActor.healthHaver.GetCurrentHealth() <= HealthThreshold;
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_aiActor.healthHaver.GetCurrentHealth() <= HealthThreshold)
		{
			m_leapBehavior.OverridePosition = m_room.area.basePosition.ToVector2() + m_leapPosition;
			BehaviorResult behaviorResult = m_leapBehavior.Update();
			if (behaviorResult == BehaviorResult.RunContinuous)
			{
				m_passthrough = true;
			}
			else
			{
				m_leapBehavior.OverridePosition = null;
			}
			return behaviorResult;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_passthrough)
		{
			ContinuousBehaviorResult continuousBehaviorResult = m_leapBehavior.ContinuousUpdate();
			if (continuousBehaviorResult == ContinuousBehaviorResult.Finished)
			{
				m_passthrough = false;
				m_leapBehavior.OverridePosition = null;
				UpdateCooldowns();
				m_aiActor.healthHaver.minimumHealth = 0f;
				m_isInDeathPosition = true;
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					GameManager.Instance.AllPlayers[i].BossKillingMode = true;
				}
			}
		}
		else if (m_isInDeathPosition)
		{
			if (!m_aiActor.TargetRigidbody)
			{
				m_aiShooter.ManualGunAngle = false;
			}
			else
			{
				Vector2 inVec = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.CenterPosition;
				int num = BraveMathCollege.VectorToOctant(inVec);
				m_aiShooter.ManualGunAngle = true;
				m_aiShooter.GunAngle = Mathf.Atan2(inVec.y, inVec.x) * 57.29578f;
				Vector2 direction = Quaternion.Euler(0f, 0f, num * -45) * Vector2.up;
				m_aiShooter.volley.projectiles[0].angleVariance = AngleVariance;
				m_aiShooter.ShootInDirection(direction, OverrideBulletName);
				m_cachedAnimationName = m_animNames[num];
				m_aiAnimator.PlayUntilCancelled(m_cachedAnimationName, true);
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void SetDeltaTime(float deltaTime)
	{
		base.SetDeltaTime(deltaTime);
		if (m_passthrough)
		{
			m_leapBehavior.SetDeltaTime(deltaTime);
		}
	}

	public override bool IsReady()
	{
		if (m_passthrough)
		{
			return m_leapBehavior.IsReady();
		}
		return base.IsReady();
	}

	public override bool UpdateEveryFrame()
	{
		if (m_passthrough)
		{
			return m_leapBehavior.UpdateEveryFrame();
		}
		return base.UpdateEveryFrame();
	}

	public override bool IsOverridable()
	{
		return false;
	}
}
