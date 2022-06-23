using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatinumBulletsItem : PassiveItem, ILevelLoadedListener
{
	public float ShootSecondsPerDamageDouble = 500f;

	public float ShootSecondsPerRateOfFireDouble = 250f;

	public float MaximumDamageMultiplier = 3f;

	public float MaximumRateOfFireMultiplier = 3f;

	[Header("Per-Floor Starting Values")]
	public float CastleStartingValue;

	public float SewersStartingValue;

	public float GungeonStartingValue;

	public float AbbeyStartingValue;

	public float MinesStartingValue;

	public float RatStartingValue;

	public float HollowStartingValue;

	public float ForgeStartingValue;

	public float HellStartingValue;

	private StatModifier DamageStat;

	private StatModifier RateOfFireStat;

	private float m_totalBulletsFiredNormalizedByFireRate;

	private float m_lastProjectileTimeslice = -1f;

	private Shader m_glintShader;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUpThisRun)
		{
			switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
			{
			case GlobalDungeonData.ValidTilesets.CASTLEGEON:
				m_totalBulletsFiredNormalizedByFireRate = CastleStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.SEWERGEON:
				m_totalBulletsFiredNormalizedByFireRate = SewersStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.GUNGEON:
				m_totalBulletsFiredNormalizedByFireRate = GungeonStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
				m_totalBulletsFiredNormalizedByFireRate = AbbeyStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.MINEGEON:
				m_totalBulletsFiredNormalizedByFireRate = MinesStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.RATGEON:
				m_totalBulletsFiredNormalizedByFireRate = RatStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
				m_totalBulletsFiredNormalizedByFireRate = HollowStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.FORGEGEON:
				m_totalBulletsFiredNormalizedByFireRate = ForgeStartingValue;
				break;
			case GlobalDungeonData.ValidTilesets.HELLGEON:
				m_totalBulletsFiredNormalizedByFireRate = HellStartingValue;
				break;
			}
		}
		base.Pickup(player);
		player.PostProcessProjectile += HandlePostProcessProjectile;
		player.PostProcessBeam += HandlePostProcessBeam;
		player.PostProcessBeamTick += HandlePostProcessBeamTick;
		player.OnKilledEnemyContext += HandleEnemyKilled;
		player.GunChanged += HandleGunChanged;
		m_glintShader = Shader.Find("Brave/ItemSpecific/LootGlintAdditivePass");
		if ((bool)player.CurrentGun)
		{
			ProcessGunShader(player.CurrentGun);
		}
	}

	private void HandleGunChanged(Gun oldGun, Gun newGun, bool arg3)
	{
		RemoveGunShader(oldGun);
		ProcessGunShader(newGun);
	}

	private void RemoveGunShader(Gun g)
	{
		if (!g)
		{
			return;
		}
		MeshRenderer component = g.GetComponent<MeshRenderer>();
		if (!component)
		{
			return;
		}
		Material[] sharedMaterials = component.sharedMaterials;
		List<Material> list = new List<Material>();
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if (sharedMaterials[i].shader != m_glintShader)
			{
				list.Add(sharedMaterials[i]);
			}
		}
		component.sharedMaterials = list.ToArray();
	}

	private void ProcessGunShader(Gun g)
	{
		MeshRenderer component = g.GetComponent<MeshRenderer>();
		if (!component)
		{
			return;
		}
		Material[] array = component.sharedMaterials;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].shader == m_glintShader)
			{
				return;
			}
		}
		Array.Resize(ref array, array.Length + 1);
		Material material = new Material(m_glintShader);
		material.SetTexture("_MainTex", array[0].GetTexture("_MainTex"));
		array[array.Length - 1] = material;
		component.sharedMaterials = array;
	}

	private void HandleEnemyKilled(PlayerController sourcePlayer, HealthHaver enemy)
	{
		if (sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.PLATINUM_AND_GOLD) && (bool)enemy && (bool)enemy.aiActor)
		{
			LootEngine.SpawnCurrency(enemy.aiActor.CenterPosition, UnityEngine.Random.Range(1, 6));
		}
	}

	public void BraveOnLevelWasLoaded()
	{
		m_lastProjectileTimeslice = -1f;
	}

	private void UpdateContributions()
	{
		if ((bool)base.Owner)
		{
			if (DamageStat == null)
			{
				DamageStat = new StatModifier();
				DamageStat.amount = 1f;
				DamageStat.modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE;
				DamageStat.statToBoost = PlayerStats.StatType.Damage;
				base.Owner.ownerlessStatModifiers.Add(DamageStat);
			}
			if (RateOfFireStat == null)
			{
				RateOfFireStat = new StatModifier();
				RateOfFireStat.amount = 1f;
				RateOfFireStat.modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE;
				RateOfFireStat.statToBoost = PlayerStats.StatType.RateOfFire;
				base.Owner.ownerlessStatModifiers.Add(RateOfFireStat);
			}
			DamageStat.amount = Mathf.Min(MaximumDamageMultiplier, 1f + m_totalBulletsFiredNormalizedByFireRate / ShootSecondsPerDamageDouble);
			RateOfFireStat.amount = Mathf.Min(MaximumRateOfFireMultiplier, 1f + m_totalBulletsFiredNormalizedByFireRate / ShootSecondsPerRateOfFireDouble);
		}
	}

	private void HandlePostProcessProjectile(Projectile targetProjectile, float effectChanceScalar)
	{
		targetProjectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(targetProjectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
	}

	private void HandleHitEnemy(Projectile sourceProjectile, SpeculativeRigidbody hitRigidbody, bool fatal)
	{
		if ((bool)sourceProjectile.PossibleSourceGun && sourceProjectile.PlayerProjectileSourceGameTimeslice > m_lastProjectileTimeslice)
		{
			m_lastProjectileTimeslice = sourceProjectile.PlayerProjectileSourceGameTimeslice;
			float num = 1f / sourceProjectile.PossibleSourceGun.DefaultModule.cooldownTime;
			m_totalBulletsFiredNormalizedByFireRate += ((!(num > 0f)) ? 1f : (1f / num));
			UpdateContributions();
		}
	}

	private void HandlePostProcessBeam(BeamController targetBeam)
	{
		UpdateContributions();
	}

	private void HandlePostProcessBeamTick(BeamController arg1, SpeculativeRigidbody arg2, float arg3)
	{
		m_totalBulletsFiredNormalizedByFireRate += BraveTime.DeltaTime;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		if ((bool)player)
		{
			if ((bool)player.CurrentGun)
			{
				RemoveGunShader(player.CurrentGun);
			}
			player.PostProcessProjectile -= HandlePostProcessProjectile;
			player.PostProcessBeam -= HandlePostProcessBeam;
			player.PostProcessBeamTick -= HandlePostProcessBeamTick;
			player.GunChanged -= HandleGunChanged;
			player.ownerlessStatModifiers.Remove(DamageStat);
			player.ownerlessStatModifiers.Remove(RateOfFireStat);
			DamageStat = null;
			RateOfFireStat = null;
		}
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if ((bool)base.Owner)
		{
			if ((bool)base.Owner.CurrentGun)
			{
				RemoveGunShader(base.Owner.CurrentGun);
			}
			base.Owner.PostProcessProjectile -= HandlePostProcessProjectile;
			base.Owner.PostProcessBeam -= HandlePostProcessBeam;
			base.Owner.PostProcessBeamTick -= HandlePostProcessBeamTick;
			base.Owner.GunChanged -= HandleGunChanged;
			base.Owner.ownerlessStatModifiers.Remove(DamageStat);
			base.Owner.ownerlessStatModifiers.Remove(RateOfFireStat);
			DamageStat = null;
			RateOfFireStat = null;
		}
		base.OnDestroy();
	}
}
