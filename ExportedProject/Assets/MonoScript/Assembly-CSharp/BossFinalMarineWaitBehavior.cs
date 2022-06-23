using Dungeonator;
using FullInspector;

[InspectorDropdownName("Bosses/BossFinalMarine/WaitBehavior")]
public class BossFinalMarineWaitBehavior : AttackBehaviorBase
{
	public float time;

	private float m_waitTimer;

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_waitTimer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		m_waitTimer = time;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_waitTimer <= 0f)
		{
			return ContinuousBehaviorResult.Finished;
		}
		if (m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All).Count <= 1)
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_updateEveryFrame = false;
	}

	public override bool IsReady()
	{
		return true;
	}

	public override float GetMinReadyRange()
	{
		return 0f;
	}

	public override float GetMaxRange()
	{
		return float.MaxValue;
	}
}
