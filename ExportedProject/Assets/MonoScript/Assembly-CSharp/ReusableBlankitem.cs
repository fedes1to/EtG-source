using UnityEngine;

public class ReusableBlankitem : PlayerItem
{
	public GameObject GlassGuonStone;

	public int GlassGuonsToGive = 1;

	public int MaxGlassGuons = 4;

	protected override void DoEffect(PlayerController user)
	{
		user.ForceBlank();
		if (user.HasActiveBonusSynergy(CustomSynergyType.BULLET_KILN))
		{
			int glassGuonsToGive = GlassGuonsToGive;
			int num = 0;
			PickupObject component = GlassGuonStone.GetComponent<PickupObject>();
			for (int i = 0; i < user.passiveItems.Count; i++)
			{
				if (user.passiveItems[i].PickupObjectId == component.PickupObjectId)
				{
					num++;
				}
			}
			glassGuonsToGive = Mathf.Min(glassGuonsToGive, MaxGlassGuons - num);
			for (int j = 0; j < glassGuonsToGive; j++)
			{
				EncounterTrackable.SuppressNextNotification = true;
				LootEngine.GivePrefabToPlayer(GlassGuonStone, user);
				EncounterTrackable.SuppressNextNotification = false;
			}
		}
		base.DoEffect(user);
	}
}
