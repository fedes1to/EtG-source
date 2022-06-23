namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("When all other actions on this state are finished, send a RESTART event.")]
	[ActionCategory(".Brave")]
	public class RestartWhenFinished : FsmStateAction, INonFinishingState
	{
		public override string ErrorCheck()
		{
			string empty = string.Empty;
			base.Fsm.GetEvent("RESTART");
			return empty + BravePlayMakerUtility.CheckGlobalTransitionExists(base.Fsm, "RESTART");
		}

		public override void OnEnter()
		{
			if (BravePlayMakerUtility.AllOthersAreFinished(this))
			{
				GoToStartState();
			}
		}

		public override void OnUpdate()
		{
			if (BravePlayMakerUtility.AllOthersAreFinished(this))
			{
				GoToStartState();
			}
		}

		private void GoToStartState()
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
			base.Fsm.Event("RESTART");
			Finish();
		}
	}
}
