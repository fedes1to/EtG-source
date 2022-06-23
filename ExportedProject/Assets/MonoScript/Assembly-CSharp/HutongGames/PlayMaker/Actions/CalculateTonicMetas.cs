using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class CalculateTonicMetas : FsmStateAction
	{
		public override void OnEnter()
		{
			int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY));
			FsmInt fsmInt = base.Fsm.Variables.FindFsmInt("npcNumber1");
			FsmFloat fsmFloat = base.Fsm.Variables.FindFsmFloat("costFloat");
			fsmInt.Value = Mathf.RoundToInt(((float)num * 0.9f).Quantize(50f));
			fsmFloat.Value = fsmInt.Value;
			Finish();
		}
	}
}
