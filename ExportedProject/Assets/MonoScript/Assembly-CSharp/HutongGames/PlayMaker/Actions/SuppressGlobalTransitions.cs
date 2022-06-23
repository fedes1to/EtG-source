namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Prevents the FSM from firing global transitions.")]
	public class SuppressGlobalTransitions : FsmStateAction, INonFinishingState
	{
		public override void OnEnter()
		{
			base.Fsm.SuppressGlobalTransitions = true;
			Finish();
		}
	}
}
