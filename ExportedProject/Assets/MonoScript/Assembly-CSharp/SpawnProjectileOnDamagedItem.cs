using UnityEngine;

public class SpawnProjectileOnDamagedItem : PassiveItem
{
	public float chanceToSpawn = 1f;

	public int minNumToSpawn = 1;

	public int maxNumToSpawn = 1;

	public Projectile projectileToSpawn;

	public bool CanBeModifiedBySynergy;

	public CustomSynergyType SynergyToCheck;

	public Projectile synergyProjectile;

	public bool randomAngle = true;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			player.OnReceivedDamage += PlayerWasDamaged;
			base.Pickup(player);
		}
	}

	private void PlayerWasDamaged(PlayerController obj)
	{
		if (Random.value < chanceToSpawn)
		{
			int num = Random.Range(minNumToSpawn, maxNumToSpawn + 1);
			float num2 = 360f / (float)num;
			float num3 = Random.Range(0f, num2);
			Projectile projectile = projectileToSpawn;
			if (CanBeModifiedBySynergy && (bool)obj && obj.HasActiveBonusSynergy(SynergyToCheck))
			{
				projectile = synergyProjectile;
			}
			for (int i = 0; i < num; i++)
			{
				float z = ((!randomAngle) ? (num3 + num2 * (float)i) : ((float)Random.Range(0, 360)));
				GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, obj.specRigidbody.UnitCenter, Quaternion.Euler(0f, 0f, z));
				Projectile component = gameObject.GetComponent<Projectile>();
				component.Owner = obj;
				component.Shooter = obj.specRigidbody;
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.OnReceivedDamage -= PlayerWasDamaged;
		debrisObject.GetComponent<SpawnProjectileOnDamagedItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
