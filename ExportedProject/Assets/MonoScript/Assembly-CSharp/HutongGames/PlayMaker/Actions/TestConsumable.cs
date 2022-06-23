namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Checks whether or not the player has a certain amount of money.")]
	[ActionCategory(".NPCs")]
	public class TestConsumable : FsmStateAction
	{
		[Tooltip("Type of consumable to check.")]
		public BravePlayMakerUtility.ConsumableType consumableType;

		[Tooltip("Value to check.")]
		public FsmFloat value;

		[Tooltip("Event sent if the amount is greater than <value>.")]
		public FsmEvent greaterThan;

		[Tooltip("Event sent if the amount is greater than or equal to <value>.")]
		public FsmEvent greaterThanOrEqual;

		[Tooltip("Event sent if the amount equals <value>.")]
		public FsmEvent equal;

		[Tooltip("Event sent if the amount is less than or equal to <value>.")]
		public FsmEvent lessThanOrEqual;

		[Tooltip("Event sent if the amount is less than <value>.")]
		public FsmEvent lessThan;

		public bool everyFrame;

		private TalkDoerLite m_talkDoer;

		public override void Reset()
		{
			consumableType = BravePlayMakerUtility.ConsumableType.Currency;
			value = 0f;
			greaterThan = null;
			greaterThanOrEqual = null;
			equal = null;
			lessThanOrEqual = null;
			lessThan = null;
			everyFrame = false;
		}

		public override string ErrorCheck()
		{
			if (FsmEvent.IsNullOrEmpty(greaterThan) && FsmEvent.IsNullOrEmpty(greaterThanOrEqual) && FsmEvent.IsNullOrEmpty(equal) && FsmEvent.IsNullOrEmpty(lessThanOrEqual) && FsmEvent.IsNullOrEmpty(lessThan))
			{
				return "Action sends no events!";
			}
			return string.Empty;
		}

		public override void OnEnter()
		{
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
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
			float consumableValue = BravePlayMakerUtility.GetConsumableValue(m_talkDoer.TalkingPlayer, consumableType);
			if (consumableValue > value.Value)
			{
				base.Fsm.Event(greaterThan);
			}
			if (consumableValue >= value.Value)
			{
				base.Fsm.Event(greaterThanOrEqual);
			}
			if (consumableValue == value.Value)
			{
				base.Fsm.Event(equal);
			}
			if (consumableValue <= value.Value)
			{
				base.Fsm.Event(lessThanOrEqual);
			}
			if (consumableValue < value.Value)
			{
				base.Fsm.Event(lessThan);
			}
		}
	}
}
