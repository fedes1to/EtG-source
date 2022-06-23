using System;
using UnityEngine;

public class RandomProjectileReplacementItem : PassiveItem
{
	public float ChancePerSecondToTrigger = 0.01f;

	public Projectile ReplacementProjectile;

	public string ReplacementAudioEvent;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Combine(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModification));
		}
	}

	private Projectile HandlePreFireProjectileModification(Gun sourceGun, Projectile sourceProjectile)
	{
		if (((bool)sourceGun && sourceGun.IsHeroSword) || sourceGun.MovesPlayerForwardOnChargeFire)
		{
			return sourceProjectile;
		}
		float num = 1f / sourceGun.DefaultModule.cooldownTime;
		if (sourceGun.Volley != null)
		{
			float num2 = 0f;
			for (int i = 0; i < sourceGun.Volley.projectiles.Count; i++)
			{
				ProjectileModule projectileModule = sourceGun.Volley.projectiles[i];
				num2 += projectileModule.GetEstimatedShotsPerSecond(sourceGun.reloadTime);
			}
			if (num2 > 0f)
			{
				num = num2;
			}
		}
		float b = Mathf.Clamp01(ChancePerSecondToTrigger / num);
		b = Mathf.Max(0.0001f, b);
		if (UnityEngine.Random.value > b)
		{
			return sourceProjectile;
		}
		if (!string.IsNullOrEmpty(ReplacementAudioEvent))
		{
			AkSoundEngine.PostEvent(ReplacementAudioEvent, base.gameObject);
		}
		return ReplacementProjectile;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<RandomProjectileReplacementItem>().m_pickedUpThisRun = true;
		player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Remove(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModification));
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			PlayerController player = m_player;
			player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Remove(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModification));
		}
	}
}
