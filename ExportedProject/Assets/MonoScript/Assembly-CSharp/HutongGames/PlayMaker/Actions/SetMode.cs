namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Sets the variable currentMode to the given string.")]
	public class SetMode : FsmStateAction
	{
		[Tooltip("Mode to set currentMode to.")]
		public FsmString mode;

		[Tooltip("Travel immediately to the new mode.")]
		public FsmBool jumpToMode;

		public override void Reset()
		{
			mode = null;
		}

		public override string ErrorCheck()
		{
			string empty = string.Empty;
			empty += BravePlayMakerUtility.CheckCurrentModeVariable(base.Fsm);
			if (!mode.Value.StartsWith("mode"))
			{
				empty += "Let's be civil and start all mode names with \"mode\", okay?\n";
			}
			empty += BravePlayMakerUtility.CheckEventExists(base.Fsm, mode.Value);
			return empty + BravePlayMakerUtility.CheckGlobalTransitionExists(base.Fsm, mode.Value);
		}

		public override void OnEnter()
		{
			FsmString fsmString = base.Fsm.Variables.GetFsmString("currentMode");
			fsmString.Value = mode.Value;
			if (jumpToMode.Value)
			{
				JumpToState();
			}
			Finish();
		}

		private void JumpToState()
		{
			if (base.Fsm.SuppressGlobalTransitions)
			{
				FsmStateAction[] actions = base.State.Actions;
				foreach (FsmStateAction fsmStateAction in actions)
				{
					if (fsmStateAction is ResumeGlobalTransitions)
					{
						base.Fsm.SuppressGlobalTransitions = false;
						break;
					}
				}
			}
			base.Fsm.Event(mode.Value);
		}
	}
}
