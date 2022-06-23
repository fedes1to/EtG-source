using System.Linq;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Checks whether or not the player has a specific pickup (gun or item).")]
	[ActionCategory(".NPCs")]
	public class TestPickup : FsmStateAction
	{
		[Tooltip("Item to check.")]
		public FsmInt pickupId;

		[Tooltip("The event to send if the player has the pickup.")]
		public FsmEvent success;

		[Tooltip("The event to send if the player does not have the pickup.")]
		public FsmEvent failure;

		public override void Reset()
		{
			pickupId = -1;
			success = null;
			failure = null;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			if (PickupObjectDatabase.GetById(pickupId.Value) == null)
			{
				text += "Invalid item ID.\n";
			}
			return text;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (component.TalkingPlayer.inventory.AllGuns.Any((Gun g) => g.PickupObjectId == pickupId.Value))
			{
				base.Fsm.Event(success);
			}
			else if (component.TalkingPlayer.activeItems.Any((PlayerItem a) => a.PickupObjectId == pickupId.Value))
			{
				base.Fsm.Event(success);
			}
			else if (component.TalkingPlayer.passiveItems.Any((PassiveItem p) => p.PickupObjectId == pickupId.Value))
			{
				base.Fsm.Event(success);
			}
			else if (component.TalkingPlayer.additionalItems.Any((PickupObject p) => p.PickupObjectId == pickupId.Value))
			{
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
