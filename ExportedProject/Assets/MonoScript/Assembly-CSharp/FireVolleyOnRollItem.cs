using UnityEngine;

public class FireVolleyOnRollItem : PassiveItem
{
	public float FireCooldown = 2f;

	public ProjectileVolleyData Volley;

	public string AudioEvent;

	public bool MetroidCrawlUsesCustomExplosion;

	public ExplosionData MetroidCrawlSynergyExplosion;

	private ProjectileVolleyData m_modVolley;

	private float m_cooldown;

	private PlayerController m_player;

	public ProjectileVolleyData ModVolley
	{
		get
		{
			if (m_modVolley == null)
			{
				return Volley;
			}
			return m_modVolley;
		}
		set
		{
			m_modVolley = value;
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnRollStarted += OnRollStarted;
		}
	}

	private void OnRollStarted(PlayerController obj, Vector2 dirVec)
	{
		if (m_cooldown <= 0f && (!GameManager.HasInstance || GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES))
		{
			FireVolley(obj, dirVec);
			m_cooldown = FireCooldown;
			if (!string.IsNullOrEmpty(AudioEvent))
			{
				AkSoundEngine.PostEvent(AudioEvent, base.gameObject);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		m_cooldown -= BraveTime.DeltaTime;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<FireVolleyOnRollItem>().m_pickedUpThisRun = true;
		player.OnRollStarted -= OnRollStarted;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			m_owner.OnRollStarted -= OnRollStarted;
		}
		base.OnDestroy();
	}

	private void FireVolley(PlayerController p, Vector2 dirVec)
	{
		for (int i = 0; i < ModVolley.projectiles.Count; i++)
		{
			ProjectileModule mod = ModVolley.projectiles[i];
			ShootSingleProjectile(p.CenterPosition, BraveMathCollege.Atan2Degrees(dirVec * -1f), mod, 100f);
		}
	}

	private void ShootSingleProjectile(Vector3 offset, float fireAngle, ProjectileModule mod, float chargeTime)
	{
		PlayerController owner = m_owner;
		Projectile projectile = null;
		ProjectileModule.ChargeProjectile chargeProjectile = null;
		if (mod.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			chargeProjectile = mod.GetChargeProjectile(chargeTime);
			if (chargeProjectile != null)
			{
				projectile = chargeProjectile.Projectile;
				projectile.pierceMinorBreakables = true;
			}
		}
		else
		{
			projectile = mod.GetCurrentProjectile();
		}
		if (!projectile)
		{
			if (mod.shootStyle != ProjectileModule.ShootStyle.Charged)
			{
				mod.IncrementShootCount();
			}
			return;
		}
		if (GameManager.Instance.InTutorial && owner != null)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerFiredGun");
		}
		offset = new Vector3(offset.x, offset.y, -1f);
		float angleForShot = mod.GetAngleForShot();
		GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, offset + Quaternion.Euler(0f, 0f, fireAngle) * mod.positionOffset, Quaternion.Euler(0f, 0f, fireAngle + angleForShot));
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = m_owner;
		component.Shooter = m_owner.specRigidbody;
		component.Inverted = mod.inverted;
		if ((bool)m_owner.aiShooter)
		{
			component.collidesWithEnemies = m_owner.aiShooter.CanShootOtherEnemies;
		}
		if ((object)m_owner != null)
		{
			PlayerStats stats = owner.stats;
			component.baseData.damage *= stats.GetStatValue(PlayerStats.StatType.Damage);
			component.baseData.speed *= stats.GetStatValue(PlayerStats.StatType.ProjectileSpeed);
			component.baseData.force *= stats.GetStatValue(PlayerStats.StatType.KnockbackMultiplier);
			component.baseData.range *= stats.GetStatValue(PlayerStats.StatType.RangeMultiplier);
		}
		if ((bool)m_owner && m_owner.HasActiveBonusSynergy(CustomSynergyType.METROID_CAN_CRAWL) && itemName == "Roll Bombs")
		{
			ExplosiveModifier component2 = component.GetComponent<ExplosiveModifier>();
			if ((bool)component2)
			{
				if (MetroidCrawlUsesCustomExplosion)
				{
					component2.explosionData = MetroidCrawlSynergyExplosion;
				}
				else
				{
					component2.explosionData.damage = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData.damage;
					component2.explosionData.damageRadius = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData.damageRadius;
					component2.explosionData.damageToPlayer = 0f;
					component2.explosionData.effect = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData.effect;
				}
			}
		}
		if (Volley != null && Volley.UsesShotgunStyleVelocityRandomizer)
		{
			component.baseData.speed *= Volley.GetVolleySpeedMod();
		}
		component.PlayerProjectileSourceGameTimeslice = Time.time;
		if ((object)m_owner != null)
		{
			owner.DoPostProcessProjectile(component);
		}
		if (mod.mirror)
		{
			gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, offset + Quaternion.Euler(0f, 0f, fireAngle) * mod.InversePositionOffset, Quaternion.Euler(0f, 0f, fireAngle - angleForShot));
			Projectile component3 = gameObject.GetComponent<Projectile>();
			component3.Inverted = true;
			component3.Owner = m_owner;
			component3.Shooter = m_owner.specRigidbody;
			if ((bool)m_owner.aiShooter)
			{
				component3.collidesWithEnemies = m_owner.aiShooter.CanShootOtherEnemies;
			}
			component3.PlayerProjectileSourceGameTimeslice = Time.time;
			if ((object)m_owner != null)
			{
				owner.DoPostProcessProjectile(component3);
			}
			component3.baseData.SetAll(component.baseData);
			component3.IsCritical = component.IsCritical;
		}
	}
}
