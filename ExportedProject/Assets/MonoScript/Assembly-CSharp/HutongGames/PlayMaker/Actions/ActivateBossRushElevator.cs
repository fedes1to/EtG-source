using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Events)]
	public class ActivateBossRushElevator : FsmStateAction
	{
		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			ShortcutElevatorController shortcutElevatorController = Object.FindObjectOfType<ShortcutElevatorController>();
			shortcutElevatorController.SetBossRushPaymentValid();
			Finish();
		}
	}
}
