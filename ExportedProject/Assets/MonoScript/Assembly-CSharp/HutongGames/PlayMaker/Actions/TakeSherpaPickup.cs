using System;
using System.Linq;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public class TakeSherpaPickup : FsmStateAction
	{
		[Tooltip("The event to send if the player does not have the pickup.")]
		public FsmEvent failure;

		public FsmInt numToTake = 1;

		[NonSerialized]
		public SherpaDetectItem parentAction;

		protected bool TakeAwayPickup(PlayerController player, int pickupId)
		{
			if (player.inventory.AllGuns.Any((Gun g) => g.PickupObjectId == pickupId))
			{
				Gun gun = player.inventory.AllGuns.Find((Gun g) => g.PickupObjectId == pickupId);
				player.inventory.RemoveGunFromInventory(gun);
				UnityEngine.Object.Destroy(gun.gameObject);
			}
			else if (player.activeItems.Any((PlayerItem a) => a.PickupObjectId == pickupId))
			{
				player.RemoveActiveItem(pickupId);
			}
			else
			{
				if (!player.passiveItems.Any((PassiveItem p) => p.PickupObjectId == pickupId))
				{
					return false;
				}
				if (numToTake.Value > 1)
				{
					for (int i = 0; i < numToTake.Value; i++)
					{
						player.RemovePassiveItem(pickupId);
					}
				}
				else
				{
					player.RemovePassiveItem(pickupId);
				}
			}
			return true;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			PlayerController talkingPlayer = component.TalkingPlayer;
			if (parentAction == null)
			{
				for (int i = 0; i < base.Fsm.PreviousActiveState.Actions.Length; i++)
				{
					if (base.Fsm.PreviousActiveState.Actions[i] is SherpaDetectItem)
					{
						parentAction = base.Fsm.PreviousActiveState.Actions[i] as SherpaDetectItem;
						break;
					}
				}
			}
			PrepareTakeSherpaPickup prepareTakeSherpaPickup = null;
			for (int j = 0; j < base.Fsm.ActiveState.Actions.Length; j++)
			{
				if (base.Fsm.ActiveState.Actions[j] is PrepareTakeSherpaPickup)
				{
					prepareTakeSherpaPickup = base.Fsm.ActiveState.Actions[j] as PrepareTakeSherpaPickup;
					break;
				}
			}
			PickupObject pickupObject = parentAction.AllValidTargets[prepareTakeSherpaPickup.CurrentPickupTargetIndex];
			if (!TakeAwayPickup(talkingPlayer, pickupObject.PickupObjectId))
			{
				base.Fsm.Event(failure);
			}
			else
			{
				parentAction = null;
			}
			Finish();
		}
	}
}
