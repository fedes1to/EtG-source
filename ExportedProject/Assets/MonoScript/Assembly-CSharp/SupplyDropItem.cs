public class SupplyDropItem : PlayerItem
{
	public GenericLootTable itemTableToUse;

	public CustomSynergyType improvementSynergy;

	public GenericLootTable synergyItemTableToUse01;

	public GenericLootTable synergyItemTableToUse02;

	public bool IsAmmoDrop;

	public override bool CanBeUsed(PlayerController user)
	{
		if (IsAmmoDrop)
		{
			if (user.HasActiveBonusSynergy(improvementSynergy))
			{
				return true;
			}
			if (user.CurrentGun == null || user.CurrentGun.InfiniteAmmo || !user.CurrentGun.CanGainAmmo || user.CurrentGun.CurrentAmmo == user.CurrentGun.AdjustedMaxAmmo)
			{
				return false;
			}
		}
		if (user.CurrentRoom != null)
		{
			if (user.InExitCell)
			{
				return false;
			}
			if (user.CurrentRoom.area.IsProceduralRoom && user.CurrentRoom.area.proceduralCells != null)
			{
				return false;
			}
		}
		return base.CanBeUsed(user);
	}

	protected override void DoEffect(PlayerController user)
	{
		IntVector2 key = user.SpawnEmergencyCrate(itemTableToUse);
		if (user.HasActiveBonusSynergy(improvementSynergy))
		{
			GameManager.Instance.Dungeon.data[key].PreventRewardSpawn = true;
			IntVector2 key2 = user.SpawnEmergencyCrate(synergyItemTableToUse01);
			GameManager.Instance.Dungeon.data[key2].PreventRewardSpawn = true;
			user.SpawnEmergencyCrate(synergyItemTableToUse02);
			GameManager.Instance.Dungeon.data[key].PreventRewardSpawn = false;
			GameManager.Instance.Dungeon.data[key2].PreventRewardSpawn = false;
		}
		AkSoundEngine.PostEvent("Play_OBJ_supplydrop_activate_01", base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
