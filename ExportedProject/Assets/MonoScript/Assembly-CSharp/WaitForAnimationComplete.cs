public class WaitForAnimationComplete : OverrideBehaviorBase
{
	public string[] TargetAnimations;

	public float ExtraDelay;

	protected float remainingDelay;

	public override void Start()
	{
		base.Start();
		remainingDelay = ExtraDelay;
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		for (int i = 0; i < TargetAnimations.Length; i++)
		{
			if (m_aiAnimator != null)
			{
				if (m_aiAnimator.IsPlaying(TargetAnimations[i]))
				{
					remainingDelay = ExtraDelay;
					return BehaviorResult.SkipAllRemainingBehaviors;
				}
			}
			else if (m_aiActor.spriteAnimator.IsPlaying(TargetAnimations[i]))
			{
				remainingDelay = ExtraDelay;
				return BehaviorResult.SkipAllRemainingBehaviors;
			}
		}
		if (remainingDelay > 0f)
		{
			remainingDelay -= m_deltaTime;
			return BehaviorResult.SkipAllRemainingBehaviors;
		}
		return behaviorResult;
	}
}
