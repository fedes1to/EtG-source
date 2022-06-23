using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class CompanionShootsRadialBurstBehavior : AttackBehaviorBase
{
	[CheckAnimation(null)]
	public string BurstAnimation;

	public float AnimationDelay = 0.125f;

	public float DetectRadius = 8f;

	public float WaveRadius = 15f;

	public float Cooldown = 15f;

	public RadialBurstInterface Burst;

	public string BurstAudioEvent;

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
		if (!string.IsNullOrEmpty(BurstAudioEvent))
		{
			AkSoundEngine.PostEvent(BurstAudioEvent, m_aiActor.gameObject);
		}
		if (!string.IsNullOrEmpty(BurstAnimation))
		{
			m_aiAnimator.PlayUntilFinished(BurstAnimation);
		}
		yield return new WaitForSeconds(AnimationDelay);
		Burst.DoBurst(m_aiActor.CompanionOwner, m_aiActor.CenterPosition);
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
