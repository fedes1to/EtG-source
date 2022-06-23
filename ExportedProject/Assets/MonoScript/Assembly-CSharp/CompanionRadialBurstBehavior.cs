using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class CompanionRadialBurstBehavior : AttackBehaviorBase
{
	[CheckAnimation(null)]
	public string BurstAnimation;

	public float AnimationDelay = 0.125f;

	public float DetectRadius = 8f;

	public float WaveRadius = 15f;

	public float Cooldown = 15f;

	public bool IgnitesEnemies;

	public GameActorFireEffect IgnitionEffect;

	private float m_cooldownTimer;

	public override BehaviorResult Update()
	{
		base.Update();
		DecrementTimer(ref m_cooldownTimer);
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (m_cooldownTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		RoomHandler currentRoom = m_aiActor.CompanionOwner.CurrentRoom;
		List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear);
		if (activeEnemies == null)
		{
			return BehaviorResult.Continue;
		}
		bool flag = false;
		float num = DetectRadius * DetectRadius;
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if ((activeEnemies[i].CenterPosition - m_aiActor.CenterPosition).sqrMagnitude < num)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			m_aiActor.StartCoroutine(DoRadialBurst());
			m_cooldownTimer = Cooldown;
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		return BehaviorResult.Continue;
	}

	private IEnumerator DoRadialBurst()
	{
		if (!string.IsNullOrEmpty(BurstAnimation))
		{
			m_aiAnimator.PlayUntilFinished(BurstAnimation);
		}
		yield return new WaitForSeconds(AnimationDelay);
		if (IgnitesEnemies)
		{
			Exploder.DoRadialIgnite(IgnitionEffect, m_aiActor.CenterPosition, WaveRadius);
		}
		if ((bool)m_aiActor.CompanionOwner)
		{
			PlayerController companionOwner = m_aiActor.CompanionOwner;
			Vector2? overrideCenter = m_aiActor.CenterPosition;
			companionOwner.ForceBlank(25f, 0.5f, false, true, overrideCenter);
		}
	}

	public override float GetMaxRange()
	{
		return DetectRadius;
	}

	public override float GetMinReadyRange()
	{
		return 0f;
	}

	public override bool IsReady()
	{
		return m_cooldownTimer <= 0f;
	}
}
