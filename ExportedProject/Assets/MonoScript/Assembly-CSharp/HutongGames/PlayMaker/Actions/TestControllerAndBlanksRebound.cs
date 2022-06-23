using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class TestControllerAndBlanksRebound : FsmStateAction
	{
		[Tooltip("Event to send if the player is in the foyer.")]
		public FsmEvent isTrue;

		[Tooltip("Event to send if the player is not in the foyer.")]
		public FsmEvent isFalse;

		[Tooltip("Event to send if the player is using a Switch")]
		public FsmEvent isSwitch;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Reset()
		{
			isTrue = null;
			isFalse = null;
			isSwitch = null;
			everyFrame = false;
		}

		private void HandleEvents()
		{
			if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.XboxOne)
			{
				if (GameManager.Options.additionalBlankControl != GameOptions.ControllerBlankControl.BOTH_STICKS_DOWN && GameManager.Options.additionalBlankControl != 0)
				{
					base.Fsm.Event(isTrue);
				}
				else
				{
					base.Fsm.Event(isFalse);
				}
			}
			else if (BraveInput.PrimaryPlayerInstance != null && !BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse())
			{
				if (GameManager.Options.additionalBlankControl != GameOptions.ControllerBlankControl.BOTH_STICKS_DOWN)
				{
					base.Fsm.Event(isTrue);
				}
				else
				{
					base.Fsm.Event(isFalse);
				}
			}
			else
			{
				base.Fsm.Event(isFalse);
			}
		}

		public override void OnEnter()
		{
			HandleEvents();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			HandleEvents();
		}
	}
}
