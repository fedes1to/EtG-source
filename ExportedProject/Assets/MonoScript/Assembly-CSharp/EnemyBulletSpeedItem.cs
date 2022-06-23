using UnityEngine;

public class EnemyBulletSpeedItem : PassiveItem
{
	public float percentageSpeedReduction = 0.25f;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			Projectile.BaseEnemyBulletSpeedMultiplier *= Mathf.Clamp01(1f - percentageSpeedReduction);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<EnemyBulletSpeedItem>().m_pickedUpThisRun = true;
		Projectile.BaseEnemyBulletSpeedMultiplier /= Mathf.Clamp01(1f - percentageSpeedReduction);
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
