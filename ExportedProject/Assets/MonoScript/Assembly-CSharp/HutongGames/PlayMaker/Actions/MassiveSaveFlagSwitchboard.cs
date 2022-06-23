namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Sends Events based on the value of a player save flag.")]
	public class MassiveSaveFlagSwitchboard : FsmStateAction
	{
		public enum SuccessType
		{
			SetMode,
			SendEvent
		}

		public MassiveSaveFlagEntry[] entries;

		public override void Reset()
		{
			entries = new MassiveSaveFlagEntry[0];
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			DoCheck();
			Finish();
		}

		private void DoCheck()
		{
			for (int i = 0; i < entries.Length; i++)
			{
				if (GameStatsManager.Instance.GetFlag(entries[i].RequiredFlag) == entries[i].RequiredFlagState && !GameStatsManager.Instance.GetFlag(entries[i].CompletedFlag) && (entries[i].CompletedFlag != GungeonFlags.CREST_NPC_SGDQ2018 || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH))
				{
					FsmString fsmString = base.Fsm.Variables.GetFsmString("currentMode");
					fsmString.Value = entries[i].mode;
					break;
				}
			}
		}
	}
}
