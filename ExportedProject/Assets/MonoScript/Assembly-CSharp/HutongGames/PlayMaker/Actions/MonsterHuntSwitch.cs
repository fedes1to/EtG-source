namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Checks whether or not the player has a certain amount of money.")]
	[ActionCategory(".NPCs")]
	public class MonsterHuntSwitch : FsmStateAction
	{
		public FsmEvent NeedsNewHunt;

		public FsmEvent HuntIncomplete;

		public FsmEvent HuntComplete;

		public bool everyFrame;

		public override void Reset()
		{
			NeedsNewHunt = null;
			HuntIncomplete = null;
			HuntComplete = null;
			everyFrame = false;
		}

		public override string ErrorCheck()
		{
			if (FsmEvent.IsNullOrEmpty(NeedsNewHunt) && FsmEvent.IsNullOrEmpty(HuntIncomplete) && FsmEvent.IsNullOrEmpty(HuntComplete))
			{
				return "Action sends no events!";
			}
			return string.Empty;
		}

		public override void OnEnter()
		{
			DoCompare();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoCompare();
		}

		private void DoCompare()
		{
			if (GameStatsManager.Instance.huntProgress.CurrentActiveMonsterHuntID <= -1)
			{
				if (GameStatsManager.Instance.GetFlag(GungeonFlags.FRIFLE_CORE_HUNTS_COMPLETE) && !GameStatsManager.Instance.GetFlag(GungeonFlags.FRIFLE_REWARD_GREY_MAUSER))
				{
					base.Fsm.Event(HuntComplete);
				}
				else
				{
					base.Fsm.Event(NeedsNewHunt);
				}
			}
			else if (GameStatsManager.Instance.huntProgress.CurrentActiveMonsterHuntProgress >= GameStatsManager.Instance.huntProgress.ActiveQuest.NumberKillsRequired)
			{
				base.Fsm.Event(HuntComplete);
			}
			else
			{
				base.Fsm.Event(HuntIncomplete);
			}
		}
	}
}
