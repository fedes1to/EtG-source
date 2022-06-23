namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class PassthroughInteract : FsmStateAction
	{
		public TalkDoerLite TargetTalker;

		private TalkDoerLite m_talkDoer;

		public override void Reset()
		{
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			TargetTalker.Interact(GameManager.Instance.PrimaryPlayer);
			Finish();
		}
	}
}
