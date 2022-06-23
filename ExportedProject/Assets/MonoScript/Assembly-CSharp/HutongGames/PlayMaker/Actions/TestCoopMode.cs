namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Only use this in the Foyer!")]
	[ActionCategory(".NPCs")]
	public class TestCoopMode : FsmStateAction
	{
		[Tooltip("Event to send if the player is in the foyer.")]
		public FsmEvent isTrue;

		[Tooltip("Event to send if the player is not in the foyer.")]
		public FsmEvent isFalse;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Reset()
		{
			isTrue = null;
			isFalse = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			base.Fsm.Event((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? isFalse : isTrue);
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			base.Fsm.Event((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? isFalse : isTrue);
		}
	}
}
