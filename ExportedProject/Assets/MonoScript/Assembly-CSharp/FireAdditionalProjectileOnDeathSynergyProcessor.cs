using UnityEngine;

public class FireAdditionalProjectileOnDeathSynergyProcessor : MonoBehaviour
{
	public enum ProjectileSource
	{
		GUN_THROUGH_CURRENT
	}

	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public Projectile ProjectileToFire;

	public ProjectileSource Source;

	private Projectile m_projectile;

	private void Awake()
	{
		m_projectile = GetComponent<Projectile>();
		m_projectile.OnDestruction += HandleDestruction;
	}

	private void HandleDestruction(Projectile obj)
	{
		if (m_projectile.Owner is PlayerController && (m_projectile.Owner as PlayerController).HasActiveBonusSynergy(SynergyToCheck) && Source == ProjectileSource.GUN_THROUGH_CURRENT && (bool)m_projectile && (bool)m_projectile.specRigidbody && (bool)m_projectile.PossibleSourceGun && m_projectile.PossibleSourceGun.gameObject.activeSelf && m_projectile.specRigidbody.UnitCenter.GetAbsoluteRoom() == (m_projectile.Owner as PlayerController).CurrentRoom)
		{
			float z = (m_projectile.specRigidbody.UnitCenter - m_projectile.PossibleSourceGun.barrelOffset.PositionVector2()).ToAngle();
			GameObject gameObject = SpawnManager.SpawnProjectile(ProjectileToFire.gameObject, m_projectile.PossibleSourceGun.barrelOffset.position, Quaternion.Euler(0f, 0f, z));
			Projectile component = gameObject.GetComponent<Projectile>();
			component.Owner = m_projectile.Owner;
			component.Shooter = m_projectile.Shooter;
			component.collidesWithPlayer = false;
			if ((bool)component)
			{
				component.SpawnedFromOtherPlayerProjectile = true;
			}
		}
	}
}
