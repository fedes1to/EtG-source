using System;
using UnityEngine;

public class OnKilledEnemySynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public bool DoesRadialBurst;

	public RadialBurstInterface RadialBurst;

	public bool DoesRadialSlow;

	public RadialSlowInterface RadialSlow;

	public bool UsesCooldown;

	public float Cooldown;

	public bool AddsDroppedCurrency;

	public int MinCurrency;

	public int MaxCurrency = 5;

	public bool TriggersEvenOnJustDamagedEnemy;

	public bool SpawnsObject;

	public GameObject ObjectToSpawn;

	private static float m_lastActiveTime;

	private Projectile m_projectile;

	private void Awake()
	{
		m_projectile = GetComponent<Projectile>();
		Projectile projectile = m_projectile;
		projectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(projectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
	}

	private void HandleHitEnemy(Projectile sourceProjectile, SpeculativeRigidbody hitRigidbody, bool killed)
	{
		if ((!UsesCooldown || Time.time - m_lastActiveTime > Cooldown) && (killed || TriggersEvenOnJustDamagedEnemy) && (bool)hitRigidbody && (bool)m_projectile.PossibleSourceGun && m_projectile.PossibleSourceGun.OwnerHasSynergy(SynergyToCheck))
		{
			if (UsesCooldown)
			{
				m_lastActiveTime = Time.time;
			}
			if (DoesRadialBurst)
			{
				RadialBurst.DoBurst(m_projectile.PossibleSourceGun.CurrentOwner as PlayerController, hitRigidbody.UnitCenter);
			}
			if (DoesRadialSlow)
			{
				RadialSlow.DoRadialSlow(hitRigidbody.UnitCenter, hitRigidbody.UnitCenter.GetAbsoluteRoom());
			}
			if (AddsDroppedCurrency)
			{
				LootEngine.SpawnCurrency(hitRigidbody.UnitCenter, UnityEngine.Random.Range(MinCurrency, MaxCurrency + 1));
			}
			if (SpawnsObject)
			{
				UnityEngine.Object.Instantiate(ObjectToSpawn, hitRigidbody.UnitCenter, Quaternion.identity);
			}
		}
	}
}
