namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sends Events based on whether or not the player is in rainbow mode.")]
	[ActionCategory(".Brave")]
	public class TestRainbowRun : FsmStateAction
	{
		[Tooltip("Event to send if rainbow mode is active.")]
		public FsmEvent isTrue;

		[Tooltip("Event to send if rainbow mode is inactive.")]
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
			base.Fsm.Event((!GameStatsManager.Instance.rainbowRunToggled) ? isFalse : isTrue);
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			base.Fsm.Event((!GameStatsManager.Instance.rainbowRunToggled) ? isFalse : isTrue);
		}
	}
}
