namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Checks what controller is being used.")]
	public class ControlMethod : FsmStateAction
	{
		[Tooltip("Event to send if the keyboard and mouse are being used.")]
		public FsmEvent keyboardAndMouse;

		[Tooltip("Event to send when a controller is being used.")]
		public FsmEvent controller;

		public override void Reset()
		{
			keyboardAndMouse = null;
			controller = null;
		}

		public override void OnEnter()
		{
			if (BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse())
			{
				base.Fsm.Event(keyboardAndMouse);
			}
			else
			{
				base.Fsm.Event(controller);
			}
			Finish();
		}
	}
}
