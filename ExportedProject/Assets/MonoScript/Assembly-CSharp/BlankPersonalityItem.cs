using UnityEngine;

public class BlankPersonalityItem : PassiveItem
{
	[Range(0f, 100f)]
	public float ReturnAmmoToAllGunsPercentage = 5f;

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		m_owner = player;
		player.OnReceivedDamage += HandleDamageReceived;
	}

	private void HandleDamageReceived(PlayerController source)
	{
		source.ForceBlank();
		if (!(ReturnAmmoToAllGunsPercentage > 0f) || source.inventory == null || source.inventory.AllGuns == null)
		{
			return;
		}
		for (int i = 0; i < source.inventory.AllGuns.Count; i++)
		{
			Gun gun = source.inventory.AllGuns[i];
			if (!gun.InfiniteAmmo && gun.CanGainAmmo)
			{
				gun.GainAmmo(Mathf.CeilToInt((float)gun.AdjustedMaxAmmo * 0.01f * ReturnAmmoToAllGunsPercentage));
			}
		}
	}

	protected override void DisableEffect(PlayerController disablingPlayer)
	{
		if ((bool)disablingPlayer)
		{
			disablingPlayer.OnReceivedDamage -= HandleDamageReceived;
		}
		base.DisableEffect(disablingPlayer);
	}
}
