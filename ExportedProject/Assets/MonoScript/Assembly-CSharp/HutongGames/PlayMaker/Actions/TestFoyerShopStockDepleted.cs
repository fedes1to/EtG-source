namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class TestFoyerShopStockDepleted : FsmStateAction
	{
		public FsmEvent CurrentStockDepleted;

		public FsmEvent AllStockDepleted;

		public FsmEvent NotDepleted;

		private TalkDoerLite m_talkDoer;

		public override void Reset()
		{
			CurrentStockDepleted = null;
			AllStockDepleted = null;
			NotDepleted = null;
		}

		public override string ErrorCheck()
		{
			if (FsmEvent.IsNullOrEmpty(CurrentStockDepleted) && FsmEvent.IsNullOrEmpty(AllStockDepleted) && FsmEvent.IsNullOrEmpty(NotDepleted))
			{
				return "Action sends no events!";
			}
			return string.Empty;
		}

		public override void OnEnter()
		{
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
			DoCompare();
			Finish();
		}

		private void DoCompare()
		{
			if (m_talkDoer.ShopStockStatus == Tribool.Complete)
			{
				base.Fsm.Event(AllStockDepleted);
			}
			else if (m_talkDoer.ShopStockStatus == Tribool.Ready)
			{
				base.Fsm.Event(CurrentStockDepleted);
			}
			else
			{
				base.Fsm.Event(NotDepleted);
			}
		}
	}
}
