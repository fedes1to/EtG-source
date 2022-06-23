using System;

namespace HutongGames.PlayMaker.Actions
{
	public class PrepareTakeSherpaPickup : FsmStateAction
	{
		public FsmEvent OnOutOfItems;

		[NonSerialized]
		public int CurrentPickupTargetIndex = -1;

		private SherpaDetectItem m_parentAction;

		public static bool IsOnSherpaMoneyStep
		{
			get
			{
				if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_READY_FOR_UNLOCKS))
				{
					if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_COMPLETE))
					{
						return GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_ELEMENT1) && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_ELEMENT2);
					}
					if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK2_COMPLETE))
					{
						return GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK2_ELEMENT1) && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK2_ELEMENT2);
					}
					if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK3_COMPLETE))
					{
						return GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK3_ELEMENT1) && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK3_ELEMENT2);
					}
					if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK4_COMPLETE))
					{
						return GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK4_ELEMENT1) && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK4_ELEMENT2);
					}
				}
				return false;
			}
		}

		public override void OnEnter()
		{
			if (m_parentAction == null)
			{
				for (int i = 0; i < base.Fsm.PreviousActiveState.Actions.Length; i++)
				{
					if (base.Fsm.PreviousActiveState.Actions[i] is SherpaDetectItem)
					{
						m_parentAction = base.Fsm.PreviousActiveState.Actions[i] as SherpaDetectItem;
						break;
					}
				}
			}
			for (int j = 0; j < base.Fsm.ActiveState.Actions.Length; j++)
			{
				if (base.Fsm.ActiveState.Actions[j] is TakeSherpaPickup)
				{
					TakeSherpaPickup takeSherpaPickup = base.Fsm.ActiveState.Actions[j] as TakeSherpaPickup;
					takeSherpaPickup.parentAction = m_parentAction;
					break;
				}
			}
			if (CurrentPickupTargetIndex >= m_parentAction.AllValidTargets.Count)
			{
				CurrentPickupTargetIndex = -1;
			}
			CurrentPickupTargetIndex++;
			if (CurrentPickupTargetIndex >= m_parentAction.AllValidTargets.Count || CurrentPickupTargetIndex < 0)
			{
				base.Fsm.Event(OnOutOfItems);
				Finish();
				return;
			}
			PickupObject pickupObject = m_parentAction.AllValidTargets[CurrentPickupTargetIndex];
			FsmString fsmString = base.Fsm.Variables.GetFsmString("npcReplacementString");
			EncounterTrackable component = pickupObject.GetComponent<EncounterTrackable>();
			if (fsmString != null && component != null)
			{
				fsmString.Value = component.journalData.GetPrimaryDisplayName();
			}
			Finish();
		}
	}
}
