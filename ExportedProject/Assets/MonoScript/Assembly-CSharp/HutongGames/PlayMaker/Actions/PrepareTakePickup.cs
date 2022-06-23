namespace HutongGames.PlayMaker.Actions
{
	public class PrepareTakePickup : FsmStateAction
	{
		public int TargetPickupIndex;

		public override void OnEnter()
		{
			PickupObject byId = PickupObjectDatabase.GetById(TargetPickupIndex);
			FsmString fsmString = base.Fsm.Variables.GetFsmString("npcReplacementString");
			EncounterTrackable component = byId.GetComponent<EncounterTrackable>();
			if (fsmString != null && component != null)
			{
				fsmString.Value = component.journalData.GetPrimaryDisplayName();
			}
			Finish();
		}
	}
}
