using UnityEngine;

public class BarkAtMimicsBehavior : MovementBehaviorBase
{
	public float PathInterval = 0.25f;

	public bool DisableInCombat = true;

	public float IdealRadius = 3f;

	public string BarkAnimation = "bark";

	private float m_repathTimer;

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	public override BehaviorResult Update()
	{
		PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
		if (primaryPlayer == null)
		{
			return BehaviorResult.Continue;
		}
		if (primaryPlayer.CurrentRoom == null)
		{
			return BehaviorResult.Continue;
		}
		if (primaryPlayer.CurrentRoom.IsSealed && DisableInCombat)
		{
			m_aiAnimator.EndAnimationIf(BarkAnimation);
			return BehaviorResult.Continue;
		}
		Chest chest = null;
		for (int i = 0; i < StaticReferenceManager.AllChests.Count; i++)
		{
			if ((bool)StaticReferenceManager.AllChests[i] && !StaticReferenceManager.AllChests[i].IsOpen && StaticReferenceManager.AllChests[i].IsMimic && StaticReferenceManager.AllChests[i].GetAbsoluteParentRoom() == primaryPlayer.CurrentRoom)
			{
				chest = StaticReferenceManager.AllChests[i];
				break;
			}
		}
		if (chest == null || chest.specRigidbody == null)
		{
			m_aiAnimator.EndAnimationIf(BarkAnimation);
			return BehaviorResult.Continue;
		}
		m_aiAnimator.EndAnimationIf("pet");
		float num = Vector2.Distance(chest.specRigidbody.UnitCenter, m_aiActor.CenterPosition);
		if (num <= IdealRadius)
		{
			m_aiActor.ClearPath();
			if (!m_aiAnimator.IsPlaying(BarkAnimation))
			{
				m_aiAnimator.PlayUntilCancelled(BarkAnimation);
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		if (m_repathTimer <= 0f)
		{
			m_repathTimer = PathInterval;
			m_aiActor.PathfindToPosition(chest.specRigidbody.UnitCenter);
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
