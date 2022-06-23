namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Sends Events based on the value of a player save flag.")]
	public class TestCharacterSpecificSaveFlag : FsmStateAction
	{
		public enum SuccessType
		{
			SetMode,
			SendEvent
		}

		public SuccessType successType;

		public CharacterSpecificGungeonFlags[] flagValues;

		public FsmBool[] values;

		[Tooltip("The event to send if the proceeding tests all pass.")]
		public new FsmEvent Event;

		[Tooltip("The name of the mode to set 'currentMode' to if the proceeding tests all pass.")]
		public FsmString mode;

		public FsmBool everyFrame;

		private bool m_success;

		public bool Success
		{
			get
			{
				return m_success;
			}
		}

		public override void Reset()
		{
			successType = SuccessType.SetMode;
			flagValues = new CharacterSpecificGungeonFlags[0];
			values = new FsmBool[0];
			Event = null;
			mode = string.Empty;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			for (int i = 0; i < flagValues.Length; i++)
			{
				if (flagValues[i] == CharacterSpecificGungeonFlags.NONE)
				{
					text += "Flag Value is NONE. This is a mistake.";
				}
			}
			if (successType == SuccessType.SetMode)
			{
				text += BravePlayMakerUtility.CheckCurrentModeVariable(base.Fsm);
				if (!mode.Value.StartsWith("mode"))
				{
					text += "Let's be civil and start all mode names with \"mode\", okay?\n";
				}
				text += BravePlayMakerUtility.CheckEventExists(base.Fsm, mode.Value);
				text += BravePlayMakerUtility.CheckGlobalTransitionExists(base.Fsm, mode.Value);
			}
			return text;
		}

		public override void OnEnter()
		{
			if (ShouldSkip())
			{
				m_success = true;
				Finish();
				return;
			}
			DoCheck();
			if (!everyFrame.Value)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (ShouldSkip())
			{
				m_success = true;
				Finish();
			}
			else
			{
				DoCheck();
			}
		}

		private bool ShouldSkip()
		{
			for (int i = 0; i < base.State.Actions.Length; i++)
			{
				if (base.State.Actions[i] == this)
				{
					return false;
				}
				if (base.State.Actions[i] is TestSaveFlag && (base.State.Actions[i] as TestSaveFlag).Success)
				{
					return true;
				}
				if (base.State.Actions[i] is TestCharacterSpecificSaveFlag && (base.State.Actions[i] as TestCharacterSpecificSaveFlag).Success)
				{
					return true;
				}
			}
			return false;
		}

		private void DoCheck()
		{
			m_success = true;
			for (int i = 0; i < flagValues.Length; i++)
			{
				if (GameStatsManager.Instance.GetCharacterSpecificFlag(flagValues[i]) != values[i].Value)
				{
					m_success = false;
					break;
				}
			}
			if (m_success)
			{
				if (successType == SuccessType.SendEvent)
				{
					base.Fsm.Event(Event);
				}
				else if (successType == SuccessType.SetMode)
				{
					FsmString fsmString = base.Fsm.Variables.GetFsmString("currentMode");
					fsmString.Value = mode.Value;
				}
				Finish();
			}
		}
	}
}
