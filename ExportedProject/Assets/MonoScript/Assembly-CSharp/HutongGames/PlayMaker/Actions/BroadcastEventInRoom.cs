namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sends Events.")]
	[ActionCategory(".Brave")]
	public class BroadcastEventInRoom : BraveFsmStateAction
	{
		public FsmString eventString;

		public override void OnEnter()
		{
			base.OnEnter();
			GameManager.BroadcastRoomFsmEvent(eventString.Value, base.Owner.transform.position.GetAbsoluteRoom());
			Finish();
		}
	}
}
