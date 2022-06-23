public class DoNothingBehavior : BasicAttackBehavior
{
	public float DoNothingTimer = 2f;

	private float m_doNothingTimer;

	private bool m_hasDoneNothing;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_doNothingTimer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_hasDoneNothing)
		{
			return BehaviorResult.Continue;
		}
		m_doNothingTimer = DoNothingTimer;
		if ((bool)m_aiActor)
		{
			m_aiActor.ClearPath();
		}
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_doNothingTimer > 0f)
		{
			return ContinuousBehaviorResult.Continue;
		}
		m_hasDoneNothing = true;
		return ContinuousBehaviorResult.Finished;
	}
}
