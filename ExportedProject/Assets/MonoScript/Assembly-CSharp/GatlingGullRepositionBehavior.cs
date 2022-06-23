using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/RepositionBehavior")]
public class GatlingGullRepositionBehavior : BasicAttackBehavior
{
	public float LostSightTime = 5f;

	public float MinDistanceToPlayer = 4f;

	public float LeapSpeedMultiplier = 1f;

	private bool m_passthrough;

	private GatlingGullLeapBehavior m_leapBehavior;

	private float m_lostSightTimer;

	private RoomHandler m_room;

	private List<IntVector2> m_leapPositions = new List<IntVector2>();

	public override void Start()
	{
		base.Start();
		AttackBehaviorGroup attackBehaviorGroup = (AttackBehaviorGroup)m_aiActor.behaviorSpeculator.AttackBehaviors.Find((AttackBehaviorBase b) => b is AttackBehaviorGroup);
		m_leapBehavior = (GatlingGullLeapBehavior)attackBehaviorGroup.AttackBehaviors.Find((AttackBehaviorGroup.AttackGroupItem b) => b.Behavior is GatlingGullLeapBehavior).Behavior;
		m_room = GameManager.Instance.Dungeon.GetRoomFromPosition(m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		List<GatlingGullLeapPoint> componentsInRoom = m_room.GetComponentsInRoom<GatlingGullLeapPoint>();
		for (int i = 0; i < componentsInRoom.Count; i++)
		{
			if (componentsInRoom[i].ForReposition)
			{
				m_leapPositions.Add(componentsInRoom[i].PlacedPosition - m_room.area.basePosition);
			}
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (m_passthrough)
		{
			m_leapBehavior.Upkeep();
			return;
		}
		Vector2 vector = BraveUtility.WorldPointToViewport(m_aiActor.specRigidbody.UnitCenter, ViewportType.Gameplay);
		if (!(vector.x >= 0f) || !(vector.x <= 1f) || !(vector.y >= -0.15f) || !(vector.y <= 1f) || !m_aiActor.HasLineOfSightToTarget)
		{
			m_lostSightTimer += m_deltaTime;
		}
		else
		{
			m_lostSightTimer = 0f;
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_leapPositions.Count == 0)
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
		if (m_lostSightTimer >= LostSightTime)
		{
			Vector2 vector = Vector2.zero;
			float num = float.MaxValue;
			for (int i = 0; i < m_leapPositions.Count; i++)
			{
				Vector2 vector2 = (m_room.area.basePosition + m_leapPositions[i]).ToVector2() + new Vector2(1f, 0.5f);
				float num2 = Vector2.Distance(vector2, m_aiActor.TargetRigidbody.UnitCenter);
				if (num2 < num && num2 > MinDistanceToPlayer && m_aiActor.HasLineOfSightToTargetFromPosition(vector2))
				{
					vector = vector2;
					num = num2;
				}
			}
			if (vector != Vector2.zero)
			{
				m_leapBehavior.OverridePosition = vector;
				m_leapBehavior.SpeedMultiplier = LeapSpeedMultiplier;
				BehaviorResult behaviorResult = m_leapBehavior.Update();
				if (behaviorResult == BehaviorResult.RunContinuous)
				{
					m_passthrough = true;
				}
				else
				{
					m_leapBehavior.OverridePosition = null;
					m_leapBehavior.SpeedMultiplier = 1f;
				}
				return behaviorResult;
			}
			Debug.Log("no jumps found!?");
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_passthrough)
		{
			return m_leapBehavior.ContinuousUpdate();
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (m_passthrough)
		{
			m_leapBehavior.EndContinuousUpdate();
		}
		m_passthrough = false;
		m_leapBehavior.OverridePosition = null;
		m_leapBehavior.SpeedMultiplier = 1f;
		UpdateCooldowns();
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
		return !m_passthrough || m_leapBehavior.IsOverridable();
	}
}
