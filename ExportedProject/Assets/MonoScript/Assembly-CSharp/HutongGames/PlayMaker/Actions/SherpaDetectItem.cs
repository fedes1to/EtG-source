using System;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public class SherpaDetectItem : FsmStateAction
	{
		public enum DetectType
		{
			SPECIFIC_ITEM,
			SOMETHING_EXPLOSIVE,
			SOMETHING_GOOPY,
			SOMETHING_FLYING
		}

		public DetectType detectType;

		[Tooltip("Specific item id to check for.")]
		public FsmInt pickupId;

		public FsmInt numToTake = 1;

		[Tooltip("The event to send if the preceeding tests all pass.")]
		public FsmEvent SuccessEvent;

		[Tooltip("The event to send if the preceeding tests all fail.")]
		public FsmEvent FailEvent;

		[NonSerialized]
		public List<PickupObject> AllValidTargets;

		private PlayerController talkingPlayer;

		public override void Reset()
		{
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			talkingPlayer = component.TalkingPlayer;
			DoCheck();
			Finish();
		}

		private bool CheckPlayerForItem(PickupObject targetItem, List<PickupObject> targets)
		{
			bool result = false;
			if (targetItem is Gun)
			{
				for (int i = 0; i < talkingPlayer.inventory.AllGuns.Count; i++)
				{
					if (talkingPlayer.inventory.AllGuns[i].PickupObjectId == targetItem.PickupObjectId)
					{
						result = true;
						targets.Add(talkingPlayer.inventory.AllGuns[i]);
					}
				}
			}
			else if (targetItem is PlayerItem)
			{
				for (int j = 0; j < talkingPlayer.activeItems.Count; j++)
				{
					if (talkingPlayer.activeItems[j].PickupObjectId == targetItem.PickupObjectId)
					{
						result = true;
						targets.Add(talkingPlayer.activeItems[j]);
					}
				}
			}
			else if (targetItem is PassiveItem)
			{
				for (int k = 0; k < talkingPlayer.passiveItems.Count; k++)
				{
					if (talkingPlayer.passiveItems[k].PickupObjectId == targetItem.PickupObjectId)
					{
						result = true;
						targets.Add(talkingPlayer.passiveItems[k]);
					}
				}
			}
			if (numToTake.Value > 1 && numToTake.Value > targets.Count)
			{
				result = false;
			}
			return result;
		}

		private bool FindFlight(List<PickupObject> fliers)
		{
			for (int i = 0; i < talkingPlayer.activeItems.Count; i++)
			{
				PlayerItem playerItem = talkingPlayer.activeItems[i];
				bool flag = false;
				if (playerItem is JetpackItem)
				{
					flag = true;
				}
				if (flag)
				{
					fliers.Add(playerItem);
				}
			}
			for (int j = 0; j < talkingPlayer.passiveItems.Count; j++)
			{
				PassiveItem passiveItem = talkingPlayer.passiveItems[j];
				bool flag2 = false;
				if (passiveItem is WingsItem)
				{
					flag2 = true;
				}
				if (flag2)
				{
					fliers.Add(passiveItem);
				}
			}
			return fliers.Count > 0;
		}

		private bool FindGoopers(List<PickupObject> goopers)
		{
			for (int i = 0; i < talkingPlayer.inventory.AllGuns.Count; i++)
			{
				Gun gun = talkingPlayer.inventory.AllGuns[i];
				bool flag = false;
				for (int j = 0; j < gun.Volley.projectiles.Count; j++)
				{
					ProjectileModule projectileModule = gun.Volley.projectiles[j];
					for (int k = 0; k < projectileModule.projectiles.Count; k++)
					{
						if (projectileModule.projectiles[k].GetComponent<GoopModifier>() != null)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					goopers.Add(gun);
				}
			}
			for (int l = 0; l < talkingPlayer.activeItems.Count; l++)
			{
				PlayerItem playerItem = talkingPlayer.activeItems[l];
				bool flag2 = false;
				if (playerItem is SpawnObjectPlayerItem)
				{
					GameObject objectToSpawn = (playerItem as SpawnObjectPlayerItem).objectToSpawn;
					if ((bool)objectToSpawn.GetComponent<ThrownGoopItem>())
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					goopers.Add(playerItem);
				}
			}
			for (int m = 0; m < talkingPlayer.passiveItems.Count; m++)
			{
				PassiveItem passiveItem = talkingPlayer.passiveItems[m];
				bool flag3 = false;
				if (passiveItem is PassiveGooperItem)
				{
					flag3 = true;
				}
				if (flag3)
				{
					goopers.Add(passiveItem);
				}
			}
			return goopers.Count > 0;
		}

		private bool FindExplosives(List<PickupObject> explosives)
		{
			for (int i = 0; i < talkingPlayer.inventory.AllGuns.Count; i++)
			{
				Gun gun = talkingPlayer.inventory.AllGuns[i];
				bool flag = false;
				for (int j = 0; j < gun.Volley.projectiles.Count; j++)
				{
					ProjectileModule projectileModule = gun.Volley.projectiles[j];
					for (int k = 0; k < projectileModule.projectiles.Count; k++)
					{
						if (projectileModule.projectiles[k].GetComponent<ExplosiveModifier>() != null)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (flag)
				{
					explosives.Add(gun);
				}
			}
			for (int l = 0; l < talkingPlayer.activeItems.Count; l++)
			{
				PlayerItem playerItem = talkingPlayer.activeItems[l];
				bool flag2 = false;
				if (playerItem is SpawnObjectPlayerItem || playerItem is RemoteMineItem)
				{
					GameObject gameObject = ((!(playerItem is SpawnObjectPlayerItem)) ? (playerItem as RemoteMineItem).objectToSpawn : (playerItem as SpawnObjectPlayerItem).objectToSpawn);
					if ((bool)gameObject.GetComponent<RemoteMineController>() || (bool)gameObject.GetComponent<ProximityMine>())
					{
						flag2 = true;
					}
				}
				if (flag2)
				{
					explosives.Add(playerItem);
				}
			}
			return explosives.Count > 0;
		}

		private void DoCheck()
		{
			bool flag = false;
			AllValidTargets = new List<PickupObject>();
			switch (detectType)
			{
			case DetectType.SPECIFIC_ITEM:
				flag = CheckPlayerForItem(PickupObjectDatabase.Instance.InternalGetById(pickupId.Value), AllValidTargets);
				break;
			case DetectType.SOMETHING_EXPLOSIVE:
				flag = FindExplosives(AllValidTargets);
				break;
			case DetectType.SOMETHING_GOOPY:
				flag = FindGoopers(AllValidTargets);
				break;
			case DetectType.SOMETHING_FLYING:
				flag = FindFlight(AllValidTargets);
				break;
			}
			if (flag)
			{
				base.Fsm.Event(SuccessEvent);
				Finish();
			}
			else
			{
				base.Fsm.Event(FailEvent);
				Finish();
			}
		}
	}
}
