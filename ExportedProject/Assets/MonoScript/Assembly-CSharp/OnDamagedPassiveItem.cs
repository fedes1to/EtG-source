using UnityEngine;

public class OnDamagedPassiveItem : PassiveItem
{
	public int ArmorToGive;

	public int FlatAmmoToGive;

	public float PercentAmmoToGive;

	public bool DoesEffectOnArmorLoss;

	public bool DoesDamageToEnemiesInRoom;

	public float DamageToEnemiesInRoom = 25f;

	public bool HasSynergy;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool SynergyAugmentsNextShot;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.healthHaver.OnDamaged += PlayerTookDamage;
		}
	}

	private void PlayerTookDamage(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (!(resultValue < maxValue) && !DoesEffectOnArmorLoss)
		{
			return;
		}
		if (base.Owner.CurrentGun != null && FlatAmmoToGive > 0)
		{
			base.Owner.CurrentGun.GainAmmo(FlatAmmoToGive);
		}
		if (base.Owner.CurrentGun != null && PercentAmmoToGive > 0f)
		{
			base.Owner.CurrentGun.GainAmmo(Mathf.CeilToInt((float)base.Owner.CurrentGun.AdjustedMaxAmmo * PercentAmmoToGive));
		}
		if (ArmorToGive > 0)
		{
			base.Owner.healthHaver.Armor += ArmorToGive;
		}
		if (DoesDamageToEnemiesInRoom)
		{
			base.Owner.CurrentRoom.ApplyActionToNearbyEnemies(base.Owner.CenterPosition, 100f, delegate(AIActor enemy, float dist)
			{
				if ((bool)enemy && (bool)enemy.healthHaver)
				{
					enemy.healthHaver.ApplyDamage(DamageToEnemiesInRoom, Vector2.zero, string.Empty);
				}
			});
		}
		if (HasSynergy && base.Owner.HasActiveBonusSynergy(RequiredSynergy) && SynergyAugmentsNextShot && base.Owner.CurrentGun.CanCriticalFire)
		{
			base.Owner.CurrentGun.ForceNextShotCritical = true;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		OnDamagedPassiveItem component = debrisObject.GetComponent<OnDamagedPassiveItem>();
		player.healthHaver.OnDamaged -= PlayerTookDamage;
		component.m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			m_owner.healthHaver.OnDamaged -= PlayerTookDamage;
		}
		base.OnDestroy();
	}
}
