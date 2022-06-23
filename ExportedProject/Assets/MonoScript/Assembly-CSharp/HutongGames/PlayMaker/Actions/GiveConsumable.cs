using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Gives a consumable to the player (heart, key, currency, etc.).")]
	public class GiveConsumable : FsmStateAction
	{
		[Tooltip("Type of consumable to give.")]
		public BravePlayMakerUtility.ConsumableType consumableType;

		[Tooltip("Amount of the consumable to give.")]
		public FsmFloat amount;

		public override void Reset()
		{
			consumableType = BravePlayMakerUtility.ConsumableType.Currency;
			amount = 0f;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (!amount.UsesVariable && amount.Value <= 0f)
			{
				text += "Need to give at least some number of consumable.\n";
			}
			return text;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			PlayerController talkingPlayer = component.TalkingPlayer;
			float consumableValue = BravePlayMakerUtility.GetConsumableValue(talkingPlayer, consumableType);
			BravePlayMakerUtility.SetConsumableValue(talkingPlayer, consumableType, consumableValue + amount.Value);
			if (consumableType == BravePlayMakerUtility.ConsumableType.Hearts && amount.Value > 0f)
			{
				GameObject gameObject = BraveResources.Load<GameObject>("Global VFX/VFX_Healing_Sparkles_001");
				if (gameObject != null)
				{
					talkingPlayer.PlayEffectOnActor(gameObject, Vector3.zero);
				}
			}
			Finish();
		}
	}
}
