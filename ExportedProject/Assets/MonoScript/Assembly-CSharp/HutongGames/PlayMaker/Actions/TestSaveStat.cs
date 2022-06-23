namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Sends Events based on the data in a player save.")]
	public class TestSaveStat : FsmStateAction
	{
		public enum SaveType
		{
			Stat,
			EncounteredTrackable,
			EncounteredRoom
		}

		public enum StatGroup
		{
			Global,
			Character,
			Session
		}

		[Tooltip("Type of save data to lookup.")]
		public SaveType saveType;

		[Tooltip("Stat to check")]
		public TrackedStats stat;

		public StatGroup statGroup;

		[Tooltip("Stat must be greather than or equal to this value to pass the test.")]
		public FsmFloat minValue;

		[Tooltip("The ID of the encounterable object.")]
		public FsmString encounterId;

		[Tooltip("The ID of the encounterable object.")]
		public FsmString encounterGuid;

		[Tooltip("The event to send if the test passes.")]
		public new FsmEvent Event;

		public override void Reset()
		{
			saveType = SaveType.Stat;
			stat = TrackedStats.BULLETS_FIRED;
			statGroup = StatGroup.Global;
			minValue = 0f;
			encounterGuid = string.Empty;
			Event = null;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (saveType == SaveType.Stat)
			{
				if (minValue.Value <= 0f)
				{
					text += "Min Value must be greater than 0.\n";
				}
			}
			else if (saveType == SaveType.EncounteredTrackable)
			{
				if (EncounterDatabase.GetEntry(encounterGuid.Value) == null)
				{
					text += "Invalid encounter ID.\n";
				}
			}
			else if (saveType == SaveType.EncounteredRoom && string.IsNullOrEmpty(encounterId.Value))
			{
				text += "Invalid room ID.\n";
			}
			return text;
		}

		public override void OnEnter()
		{
			DoCheck();
			Finish();
		}

		private void DoCheck()
		{
			float num = -1f;
			if (saveType == SaveType.Stat)
			{
				if (statGroup == StatGroup.Global)
				{
					num = GameStatsManager.Instance.GetPlayerStatValue(stat);
				}
				else if (statGroup == StatGroup.Character)
				{
					num = GameStatsManager.Instance.GetCharacterStatValue(stat);
				}
				else if (statGroup == StatGroup.Session)
				{
					num = GameStatsManager.Instance.GetSessionStatValue(stat);
				}
			}
			else if (saveType == SaveType.EncounteredTrackable)
			{
				num = GameStatsManager.Instance.QueryEncounterable(encounterGuid.Value);
			}
			else if (saveType == SaveType.EncounteredRoom)
			{
				num = GameStatsManager.Instance.QueryRoomEncountered(encounterId.Value);
			}
			if (num >= minValue.Value)
			{
				base.Fsm.Event(Event);
			}
		}
	}
}
