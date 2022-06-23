namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Logic)]
	public class TestCurrentLanguageEnglish : FsmStateAction
	{
		public FsmEvent EnglishEvent;

		public FsmEvent OtherEvent;

		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			DoIDSwitch();
			Finish();
		}

		public override void OnUpdate()
		{
			DoIDSwitch();
		}

		private void DoIDSwitch()
		{
			if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
			{
				base.Fsm.Event(EnglishEvent);
			}
			else
			{
				base.Fsm.Event(OtherEvent);
			}
		}
	}
}
