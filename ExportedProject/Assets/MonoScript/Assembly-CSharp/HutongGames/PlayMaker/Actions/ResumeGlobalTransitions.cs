namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Allows the FSM to fire global transitions again.")]
	public class ResumeGlobalTransitions : FsmStateAction, INonFinishingState
	{
		public override void OnEnter()
		{
			if (BravePlayMakerUtility.AllOthersAreFinished(this))
			{
				base.Fsm.SuppressGlobalTransitions = false;
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (BravePlayMakerUtility.AllOthersAreFinished(this))
			{
				base.Fsm.SuppressGlobalTransitions = false;
				Finish();
			}
		}
	}
}
