public class GunClassPassiveItem : PassiveItem
{
	public GunClass[] classesToModify;

	public float[] damageModifiers;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
		}
	}

	private void PostProcessBeam(BeamController obj)
	{
		if (!m_player || !m_player.CurrentGun || damageModifiers == null || !obj || !obj.projectile)
		{
			return;
		}
		for (int i = 0; i < classesToModify.Length; i++)
		{
			if (m_player.CurrentGun.gunClass == classesToModify[i])
			{
				obj.projectile.baseData.damage *= damageModifiers[i];
			}
		}
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		for (int i = 0; i < classesToModify.Length; i++)
		{
			if (m_player.CurrentGun != null && m_player.CurrentGun.gunClass == classesToModify[i])
			{
				obj.baseData.damage *= damageModifiers[i];
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<GunClassPassiveItem>().m_pickedUpThisRun = true;
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
