using UnityEngine;

public class ClipBulletModifierItem : PassiveItem
{
	public float ActivationChance = 1f;

	public bool FirstShotBoost;

	public float FirstShotMultiplier = 2f;

	public bool LastShotBoost;

	public float LastShotMultiplier = 2f;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
		}
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		float activationChance = ActivationChance;
		if (Random.value < activationChance)
		{
			if (FirstShotBoost && m_player.CurrentGun.LastShotIndex == 0)
			{
				obj.baseData.damage *= FirstShotMultiplier;
			}
			if (LastShotBoost && m_player.CurrentGun.LastShotIndex == m_player.CurrentGun.ClipCapacity - 1)
			{
				obj.baseData.damage *= LastShotMultiplier;
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<ClipBulletModifierItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
		}
	}
}
