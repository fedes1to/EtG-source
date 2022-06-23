using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class SpawnGunslingGun : BraveFsmStateAction
	{
		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			PlayerController player = ((!component.TalkingPlayer) ? GameManager.Instance.PrimaryPlayer : component.TalkingPlayer);
			SelectGunslingGun selectGunslingGun = FindActionOfType<SelectGunslingGun>();
			CheckGunslingChallengeComplete checkGunslingChallengeComplete = FindActionOfType<CheckGunslingChallengeComplete>();
			if (selectGunslingGun != null)
			{
				GameObject selectedObject = selectGunslingGun.SelectedObject;
				Gun gun = LootEngine.TryGiveGunToPlayer(selectedObject, player);
				if ((bool)gun)
				{
					gun.CanBeDropped = false;
					gun.CanBeSold = false;
					gun.IsMinusOneGun = true;
					if (checkGunslingChallengeComplete != null)
					{
						checkGunslingChallengeComplete.GunToUse = gun;
						checkGunslingChallengeComplete.GunToUsePrefab = selectGunslingGun.SelectedObject.GetComponent<Gun>();
					}
				}
			}
			Finish();
		}
	}
}
