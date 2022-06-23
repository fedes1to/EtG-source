using System;
using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/RocketBehavior")]
public class GatlingGullRocketBehavior : BasicAttackBehavior
{
	public Transform RocketOrigin;

	public GameObject Rocket;

	public int MaxRockets = 5;

	public float DamageToHalt = 20f;

	public float PerRocketCooldown = 1f;

	private bool m_passthrough;

	private GatlingGullLeapBehavior m_leapBehavior;

	private float m_fireTimer;

	private float m_healthToHalt;

	private bool m_firedThisCycle;

	private int m_rocketCount;

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
			if (componentsInRoom[i].ForRockets)
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
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_leapPositions.Count == 0)
		{
			return BehaviorResult.Continue;
		}
		int index = UnityEngine.Random.Range(0, m_leapPositions.Count);
		m_leapBehavior.OverridePosition = (m_room.area.basePosition + m_leapPositions[index]).ToVector2() + new Vector2(1f, 0.5f);
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

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_passthrough)
		{
			ContinuousBehaviorResult continuousBehaviorResult = m_leapBehavior.ContinuousUpdate();
			if (continuousBehaviorResult == ContinuousBehaviorResult.Finished)
			{
				m_leapBehavior.EndContinuousUpdate();
				m_aiAnimator.SuppressHitStates = true;
				m_aiAnimator.PlayUntilFinished("rocket", true);
				tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
				spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
				m_rocketCount = 0;
				m_fireTimer = PerRocketCooldown;
				m_firedThisCycle = false;
				m_healthToHalt = m_aiActor.healthHaver.GetCurrentHealth() - DamageToHalt;
				m_aiActor.ClearPath();
				m_passthrough = false;
				m_leapBehavior.OverridePosition = null;
			}
			return ContinuousBehaviorResult.Continue;
		}
		m_fireTimer -= m_deltaTime;
		if (m_fireTimer <= 0f)
		{
			if (!m_firedThisCycle)
			{
				FireRocket();
			}
			m_firedThisCycle = false;
			m_fireTimer += PerRocketCooldown;
			m_aiAnimator.PlayUntilFinished("rocket", true);
		}
		if (m_aiActor.healthHaver.GetCurrentHealth() <= m_healthToHalt || m_rocketCount >= MaxRockets)
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiAnimator.SuppressHitStates = false;
		m_aiAnimator.EndAnimationIf("rocket");
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
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

	private void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (frame.eventInfo == "fire_rocket")
		{
			FireRocket();
		}
	}

	private void FireRocket()
	{
		SkyRocket component = SpawnManager.SpawnProjectile(Rocket, RocketOrigin.position, Quaternion.identity).GetComponent<SkyRocket>();
		component.Target = m_aiActor.TargetRigidbody;
		tk2dSprite componentInChildren = component.GetComponentInChildren<tk2dSprite>();
		component.transform.position = component.transform.position.WithY(component.transform.position.y - componentInChildren.transform.localPosition.y);
		component.ExplosionData.ignoreList.Add(m_aiActor.specRigidbody);
		m_aiActor.sprite.AttachRenderer(component.GetComponentInChildren<tk2dSprite>());
		m_firedThisCycle = true;
		m_rocketCount++;
	}
}
