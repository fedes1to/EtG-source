using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Completes a synergy. Requires SynergraceTestCompletionPossible.")]
	[ActionCategory(".Brave")]
	public class SynergraceGiveSelectedPickup : BraveFsmStateAction
	{
		public override void OnEnter()
		{
			base.OnEnter();
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			SynergraceTestCompletionPossible synergraceTestCompletionPossible = FindActionOfType<SynergraceTestCompletionPossible>();
			if ((bool)component && (bool)component.TalkingPlayer && synergraceTestCompletionPossible != null && (bool)synergraceTestCompletionPossible.SelectedPickupGameObject)
			{
				Chest chest = Chest.Spawn(GameManager.Instance.RewardManager.Synergy_Chest, component.transform.position.IntXY(VectorConversions.Floor) + new IntVector2(1, -5));
				if ((bool)chest)
				{
					chest.IsLocked = false;
					PickupObject component2 = synergraceTestCompletionPossible.SelectedPickupGameObject.GetComponent<PickupObject>();
					if ((bool)component2)
					{
						chest.forceContentIds = new List<int>();
						chest.forceContentIds.Add(component2.PickupObjectId);
					}
				}
				else
				{
					LootEngine.TryGivePrefabToPlayer(synergraceTestCompletionPossible.SelectedPickupGameObject, component.TalkingPlayer);
				}
				synergraceTestCompletionPossible.SelectedPickupGameObject = null;
				component.TalkingPlayer.HandleItemPurchased(null);
			}
			Finish();
		}
	}
}
