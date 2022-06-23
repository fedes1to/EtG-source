using System.Linq;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Takes a pickup from the player (gun or item).")]
	[ActionCategory(".NPCs")]
	public class TakePickup : FsmStateAction
	{
		[Tooltip("Item to take.")]
		public FsmInt pickupId;

		[Tooltip("The event to send if the player does not have the pickup.")]
		public FsmEvent failure;

		public override void Reset()
		{
			pickupId = -1;
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
			PlayerController talkingPlayer = component.TalkingPlayer;
			if (talkingPlayer.inventory.AllGuns.Any((Gun g) => g.PickupObjectId == pickupId.Value))
			{
				Gun gun = component.TalkingPlayer.inventory.AllGuns.Find((Gun g) => g.PickupObjectId == pickupId.Value);
				talkingPlayer.inventory.RemoveGunFromInventory(gun);
				Object.Destroy(gun.gameObject);
			}
			else if (talkingPlayer.activeItems.Any((PlayerItem a) => a.PickupObjectId == pickupId.Value))
			{
				talkingPlayer.RemoveActiveItem(pickupId.Value);
			}
			else if (talkingPlayer.passiveItems.Any((PassiveItem p) => p.PickupObjectId == pickupId.Value))
			{
				talkingPlayer.RemovePassiveItem(pickupId.Value);
			}
			else
			{
				base.Fsm.Event(failure);
			}
			Finish();
		}
	}
}
