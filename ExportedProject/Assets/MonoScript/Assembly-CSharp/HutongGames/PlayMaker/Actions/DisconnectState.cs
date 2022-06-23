namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Removes all transitions to this state.")]
	[ActionCategory(".Brave")]
	public class DisconnectState : FsmStateAction
	{
		public override void OnEnter()
		{
			BravePlayMakerUtility.DisconnectState(base.State);
			Finish();
		}
	}
}
