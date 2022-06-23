namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Switchboard to jump to different NPC modes.")]
	[ActionCategory(".Brave")]
	public class ModeSwitchboard : FsmStateAction
	{
		public override string ErrorCheck()
		{
			string empty = string.Empty;
			empty += BravePlayMakerUtility.CheckCurrentModeVariable(base.Fsm);
			FsmString fsmString = base.Fsm.Variables.GetFsmString("currentMode");
			empty += BravePlayMakerUtility.CheckEventExists(base.Fsm, fsmString.Value);
			return empty + BravePlayMakerUtility.CheckGlobalTransitionExists(base.Fsm, fsmString.Value);
		}

		public override void OnEnter()
		{
			FsmString fsmString = base.Fsm.Variables.GetFsmString("currentMode");
			base.Fsm.Event(fsmString.Value);
			Finish();
		}
	}
}
