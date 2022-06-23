public class SpecialKeyItem : PassiveItem
{
	public enum SpecialKeyType
	{
		RESOURCEFUL_RAT_LAIR
	}

	public SpecialKeyType keyType;

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		GameUIRoot.Instance.UpdatePlayerConsumables(player.carriedConsumables);
	}
}
