using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class BossFinalRobotIntroDoer : SpecificIntroDoer
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void EndIntro()
	{
		base.aiAnimator.StopVfx("torch_intro");
	}
}
