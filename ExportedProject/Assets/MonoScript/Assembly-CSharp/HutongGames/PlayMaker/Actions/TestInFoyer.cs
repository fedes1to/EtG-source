namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Sends Events based on whether or not the player is in the foyer.")]
	public class TestInFoyer : FsmStateAction
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
			base.Fsm.Event((!GameManager.Instance.IsFoyer) ? isFalse : isTrue);
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			base.Fsm.Event((!GameManager.Instance.IsFoyer) ? isFalse : isTrue);
		}
	}
}
