using System.Linq;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public class TakeBlacksmithPickup : FsmStateAction
	{
		protected bool TakeAwayPickup(PlayerController player, int pickupId)
		{
			if (player.inventory.AllGuns.Any((Gun g) => g.PickupObjectId == pickupId))
			{
				Gun gun = player.inventory.AllGuns.Find((Gun g) => g.PickupObjectId == pickupId);
				player.inventory.RemoveGunFromInventory(gun);
				Object.Destroy(gun.gameObject);
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
				player.RemovePassiveItem(pickupId);
			}
			return true;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			PlayerController talkingPlayer = component.TalkingPlayer;
			BlacksmithDetectItem blacksmithDetectItem = null;
			for (int i = 0; i < base.Fsm.PreviousActiveState.Actions.Length; i++)
			{
				if (base.Fsm.PreviousActiveState.Actions[i] is BlacksmithDetectItem)
				{
					blacksmithDetectItem = base.Fsm.PreviousActiveState.Actions[i] as BlacksmithDetectItem;
					break;
				}
			}
			PickupObject targetPickupObject = blacksmithDetectItem.GetTargetPickupObject();
			DesiredItem currentDesire = blacksmithDetectItem.GetCurrentDesire();
			bool flag = false;
			if (currentDesire.type == DesiredItem.DetectType.SPECIFIC_ITEM)
			{
				flag = TakeAwayPickup(talkingPlayer, targetPickupObject.PickupObjectId);
			}
			else if (currentDesire.type == DesiredItem.DetectType.CURRENCY)
			{
				talkingPlayer.carriedConsumables.Currency -= currentDesire.amount;
				flag = true;
			}
			else if (currentDesire.type == DesiredItem.DetectType.META_CURRENCY)
			{
				int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY));
				GameStatsManager.Instance.ClearStatValueGlobal(TrackedStats.META_CURRENCY);
				GameStatsManager.Instance.SetStat(TrackedStats.META_CURRENCY, num - currentDesire.amount);
				flag = true;
			}
			else if (currentDesire.type == DesiredItem.DetectType.KEYS)
			{
				talkingPlayer.carriedConsumables.KeyBullets -= currentDesire.amount;
				flag = true;
			}
			if (flag)
			{
				GameStatsManager.Instance.SetFlag(currentDesire.flagToSet, true);
			}
			Finish();
		}
	}
}
