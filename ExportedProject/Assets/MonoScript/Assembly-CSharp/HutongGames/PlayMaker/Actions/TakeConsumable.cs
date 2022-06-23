namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Takes a consumable from the player (heart, key, currency, etc.).")]
	public class TakeConsumable : FsmStateAction
	{
		[Tooltip("Type of consumable to take.")]
		public BravePlayMakerUtility.ConsumableType consumableType;

		[Tooltip("Amount of the consumable to take.")]
		public FsmFloat amount;

		[Tooltip("The event to send if the player pays.")]
		public FsmEvent success;

		[Tooltip("The event to send if the player does not have enough of the consumable.")]
		public FsmEvent failure;

		public override void Reset()
		{
			consumableType = BravePlayMakerUtility.ConsumableType.Currency;
			amount = 0f;
			failure = null;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (!amount.UsesVariable && amount.Value <= 0f)
			{
				text += "Need to take at least some number of consumable.\n";
			}
			return text;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			PlayerController talkingPlayer = component.TalkingPlayer;
			float consumableValue = BravePlayMakerUtility.GetConsumableValue(talkingPlayer, consumableType);
			if (consumableValue >= amount.Value)
			{
				BravePlayMakerUtility.SetConsumableValue(talkingPlayer, consumableType, consumableValue - amount.Value);
				base.Fsm.Event(success);
			}
			else
			{
				base.Fsm.Event(failure);
			}
			Finish();
		}
	}
}
