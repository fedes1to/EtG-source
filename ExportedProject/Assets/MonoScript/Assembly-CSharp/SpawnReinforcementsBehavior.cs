using System;
using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class SpawnReinforcementsBehavior : BasicAttackBehavior
{
	public enum IndexType
	{
		Random,
		Ordered
	}

	public enum StaggerMode
	{
		Animation,
		Timer
	}

	private enum State
	{
		Idle,
		Spawning,
		PostSpawnDelay
	}

	public int MaxRoomOccupancy = -1;

	public int OverrideMaxOccupancyToSpawn = -1;

	public List<int> ReinforcementIndices;

	public IndexType indexType;

	public bool StaggerSpawns;

	[InspectorIndent]
	[InspectorShowIf("StaggerSpawns")]
	public StaggerMode staggerMode;

	[InspectorIndent]
	[InspectorShowIf("ShowStaggerDelay")]
	public float staggerDelay = 1f;

	public bool StopDuringAnimation = true;

	public bool DisableDrops = true;

	public float DelayAfterSpawn;

	public int DelayAfterSpawnMinOccupancy;

	[InspectorCategory("Visuals")]
	public string DirectionalAnimation;

	[InspectorCategory("Visuals")]
	public bool HideGun;

	[InspectorCategory("Conditions")]
	public float StaticCooldown;

	private int m_timesReinforced;

	private int m_reinforceIndex;

	private int m_reinforceSubIndex;

	private int m_thingsToSpawn;

	private float m_staggerTimer;

	private float m_timer;

	private static float s_staticCooldown;

	private static int s_lastStaticUpdateFrameNum = -1;

	private State m_state;

	private bool ShowStaggerDelay()
	{
		return StaggerSpawns && staggerMode == StaggerMode.Timer;
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (s_staticCooldown > 0f && s_lastStaticUpdateFrameNum != Time.frameCount)
		{
			s_staticCooldown = Mathf.Max(0f, s_staticCooldown - m_deltaTime);
			s_lastStaticUpdateFrameNum = Time.frameCount;
		}
		DecrementTimer(ref m_staggerTimer);
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_reinforceIndex = ((indexType != IndexType.Ordered) ? BraveUtility.RandomElement(ReinforcementIndices) : ReinforcementIndices[m_timesReinforced]);
		m_thingsToSpawn = m_aiActor.ParentRoom.GetEnemiesInReinforcementLayer(m_reinforceIndex);
		int num = MaxRoomOccupancy;
		if (OverrideMaxOccupancyToSpawn > 0)
		{
			num = OverrideMaxOccupancyToSpawn;
		}
		if (num >= 0)
		{
			int count = m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).Count;
			if (count >= num)
			{
				m_timesReinforced++;
				UpdateCooldowns();
				return BehaviorResult.Continue;
			}
			m_thingsToSpawn = MaxRoomOccupancy - count;
		}
		m_timesReinforced++;
		s_staticCooldown += StaticCooldown;
		if (!string.IsNullOrEmpty(DirectionalAnimation))
		{
			m_aiAnimator.PlayUntilFinished(DirectionalAnimation, true);
		}
		if (HideGun)
		{
			m_aiShooter.ToggleGunAndHandRenderers(false, "SpawnReinforcementBehavior");
		}
		if (StopDuringAnimation)
		{
			m_aiActor.ClearPath();
		}
		if (StaggerSpawns)
		{
			m_reinforceSubIndex = 0;
			if (staggerMode == StaggerMode.Animation)
			{
				tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
				spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
			}
			else if (staggerMode == StaggerMode.Timer)
			{
				m_staggerTimer = staggerDelay;
			}
		}
		else if (m_thingsToSpawn > 0)
		{
			RoomHandler parentRoom = m_aiActor.ParentRoom;
			int reinforceIndex = m_reinforceIndex;
			bool removeLayer = false;
			bool disableDrops = DisableDrops;
			int thingsToSpawn = m_thingsToSpawn;
			parentRoom.TriggerReinforcementLayer(reinforceIndex, removeLayer, disableDrops, -1, thingsToSpawn);
		}
		if (StopDuringAnimation || StaggerSpawns)
		{
			m_updateEveryFrame = true;
			m_state = State.Spawning;
			return BehaviorResult.RunContinuous;
		}
		UpdateCooldowns();
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == State.Spawning)
		{
			bool flag = false;
			if (!StaggerSpawns)
			{
				if (!m_aiAnimator.IsPlaying(DirectionalAnimation))
				{
					flag = true;
				}
			}
			else if (staggerMode == StaggerMode.Timer)
			{
				if (m_staggerTimer <= 0f)
				{
					SpawnOneDude();
					m_staggerTimer = staggerDelay;
					if (m_thingsToSpawn <= 0)
					{
						flag = true;
					}
				}
			}
			else if (staggerMode == StaggerMode.Animation && !m_aiAnimator.IsPlaying(DirectionalAnimation))
			{
				if (m_thingsToSpawn > 0)
				{
					m_aiActor.ParentRoom.TriggerReinforcementLayer(m_reinforceIndex, false, DisableDrops, m_reinforceSubIndex, m_thingsToSpawn);
				}
				flag = true;
			}
			if (flag)
			{
				if (DelayAfterSpawn > 0f)
				{
					m_timer = DelayAfterSpawn;
					m_state = State.PostSpawnDelay;
					return ContinuousBehaviorResult.Continue;
				}
				return ContinuousBehaviorResult.Finished;
			}
		}
		else if (m_state == State.PostSpawnDelay)
		{
			DecrementTimer(ref m_timer);
			if (DelayAfterSpawnMinOccupancy > 0)
			{
				int count = m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).Count;
				if (count < DelayAfterSpawnMinOccupancy)
				{
					return ContinuousBehaviorResult.Finished;
				}
			}
			if (m_timer <= 0f)
			{
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (HideGun)
		{
			m_aiShooter.ToggleGunAndHandRenderers(true, "SpawnReinforcementBehavior");
		}
		if (StaggerSpawns && staggerMode == StaggerMode.Animation)
		{
			tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsReady()
	{
		if (!base.IsReady() || s_staticCooldown > 0f)
		{
			return false;
		}
		return true;
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNum)
	{
		if (clip.GetFrame(frameNum).eventInfo == "spawn" && m_thingsToSpawn > 0)
		{
			SpawnOneDude();
		}
	}

	private void SpawnOneDude()
	{
		m_aiActor.ParentRoom.TriggerReinforcementLayer(m_reinforceIndex, false, DisableDrops, m_reinforceSubIndex, 1);
		m_reinforceSubIndex++;
		m_thingsToSpawn--;
	}
}
