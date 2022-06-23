namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sends an Event based on the current character.")]
	[ActionCategory(ActionCategory.Logic)]
	public class CharacterClassSwitch : FsmStateAction
	{
		[CompoundArray("Int Switches", "Compare Int", "Send Event")]
		public PlayableCharacters[] compareTo;

		public FsmEvent[] sendEvent;

		public bool everyFrame;

		public override void Reset()
		{
			compareTo = new PlayableCharacters[1];
			sendEvent = new FsmEvent[1];
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoCharSwitch();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoCharSwitch();
		}

		private void DoCharSwitch()
		{
			for (int i = 0; i < compareTo.Length; i++)
			{
				if ((bool)base.Owner && (bool)base.Owner.GetComponent<TalkDoerLite>() && (bool)base.Owner.GetComponent<TalkDoerLite>().TalkingPlayer)
				{
					if (base.Owner.GetComponent<TalkDoerLite>().TalkingPlayer.characterIdentity == compareTo[i])
					{
						base.Fsm.Event(sendEvent[i]);
						break;
					}
				}
				else if (GameManager.Instance.PrimaryPlayer.characterIdentity == compareTo[i])
				{
					base.Fsm.Event(sendEvent[i]);
					break;
				}
			}
		}
	}
}
