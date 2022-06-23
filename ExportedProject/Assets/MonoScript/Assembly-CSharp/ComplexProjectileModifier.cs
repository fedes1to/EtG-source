using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class ComplexProjectileModifier : PassiveItem
{
	public float ActivationChance = 1f;

	public bool NormalizeAcrossFireRate;

	[ShowInInspectorIf("NormalizeAcrossFireRate", false)]
	public float ActivationsPerSecond = 1f;

	[ShowInInspectorIf("NormalizeAcrossFireRate", false)]
	public float MinActivationChance = 0.05f;

	public bool UsesAlternateActivationChanceInBossRooms;

	[ShowInInspectorIf("UsesAlternateActivationChanceInBossRooms", false)]
	public float BossActivationsPerSecond = 1f;

	public bool AddsChainLightning;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public GameObject ChainLightningVFX;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public CoreDamageTypes ChainLightningDamageTypes;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public float ChainLightningMaxLinkDistance = 15f;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public float ChainLightningDamagePerHit = 6f;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public float ChainLightningDamageCooldown = 1f;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public GameObject ChainLightningDispersalParticles;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public float ChainLightningDispersalDensity;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public float ChainLightningDispersalMinCoherence;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public float ChainLightningDispersalMaxCoherence;

	public bool AddsExplosino;

	[ShowInInspectorIf("AddsExplosino", false)]
	public ExplosionData ExplosionData;

	public bool UsesChanceForAdditionalProjectile;

	[Header("Adds Spawned Projectiles")]
	public bool AddsSpawnProjectileModifier;

	[ShowInInspectorIf("AddsSpawnProjectileModifier", false)]
	public bool SpawnProjectileInheritsApperance;

	[ShowInInspectorIf("AddsSpawnProjectileModifier", false)]
	public float SpawnProjectileScaleModifier = 1f;

	[ShowInInspectorIf("AddsSpawnProjectileModifier", false)]
	public int NumberToSpawnOnCollision = 3;

	[ShowInInspectorIf("AddsSpawnProjectileModifier", false)]
	public Projectile CollisionSpawnProjectile;

	[ShowInInspectorIf("AddsSpawnProjectileModifier", false)]
	public bool ScaleSpawnsByFireRate;

	[ShowInInspectorIf("ScaleSpawnsByFireRate", false)]
	public int MinFlakSpawns = 2;

	[ShowInInspectorIf("ScaleSpawnsByFireRate", false)]
	public int MaxFlakSpawns = 8;

	[ShowInInspectorIf("ScaleSpawnsByFireRate", false)]
	public float MinFlakFireRate = 0.25f;

	[ShowInInspectorIf("ScaleSpawnsByFireRate", false)]
	public float MaxFlakFireRate = 2f;

	[Header("Adds Chance To Blank")]
	public bool AddsChanceToBlank;

	[ShowInInspectorIf("AddsChanceToBlank", false)]
	public float BlankRadius = 5f;

	[Header("Adds Trailed Spawns")]
	public bool AddsTrailedSpawn;

	[ShowInInspectorIf("AddsTrailedSpawn", false)]
	public GameObject TrailedObjectToSpawn;

	[ShowInInspectorIf("AddsTrailedSpawn", false)]
	public float TrailedObjectSpawnDistance = 1f;

	[Header("Critical")]
	public bool AddsCriticalChance;

	[ShowInInspectorIf("AddsCriticalChance", false)]
	public Projectile CriticalProjectile;

	[Header("Devolver")]
	[Space(20f)]
	public bool AddsDevolverModifier;

	[ShowInInspectorIf("AddsDevolverModifier", false)]
	public DevolverModifier DevolverSourceModifier;

	[Header("Hungry Bullets")]
	public bool AddsHungryBullets;

	[ShowInInspectorIf("AddsHungryBullets", false)]
	public float HungryRadius = 1.5f;

	[ShowInInspectorIf("AddsHungryBullets", false)]
	public float DamagePercentGainPerSnack = 0.25f;

	[ShowInInspectorIf("AddsHungryBullets", false)]
	public float HungryMaxMultiplier = 3f;

	[ShowInInspectorIf("AddsHungryBullets", false)]
	public int MaximumBulletsEaten = 10;

	[Header("Katana Bullets")]
	public bool AddsLinearChainExplosionOnKill;

	[ShowInInspectorIf("AddsLinearChainExplosionOnKill", false)]
	public float LCEChainDuration = 1f;

	[ShowInInspectorIf("AddsLinearChainExplosionOnKill", false)]
	public float LCEChainDistance = 10f;

	[ShowInInspectorIf("AddsLinearChainExplosionOnKill", false)]
	public int LCEChainNumExplosions = 5;

	[ShowInInspectorIf("AddsLinearChainExplosionOnKill", false)]
	public GameObject LCEChainTargetSprite;

	[ShowInInspectorIf("AddsLinearChainExplosionOnKill", false)]
	public ExplosionData LinearChainExplosionData;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
			player.PostProcessBeamChanceTick += PostProcessBeamChanceTick;
			if (AddsCriticalChance)
			{
				player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Combine(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModifier));
			}
		}
	}

	private void PostProcessBeam(BeamController obj)
	{
		if (AddsLinearChainExplosionOnKill && (bool)obj && (bool)obj.projectile)
		{
			Projectile obj2 = obj.projectile;
			obj2.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(obj2.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleLinearChainBeamHitEnemy));
		}
	}

	private IEnumerator HandleChainExplosion(SpeculativeRigidbody enemySRB, Vector2 startPosition, Vector2 direction)
	{
		float perExplosionTime = LCEChainDuration / (float)LCEChainNumExplosions;
		float[] explosionTimes = new float[LCEChainNumExplosions];
		explosionTimes[0] = 0f;
		explosionTimes[1] = perExplosionTime;
		for (int i = 2; i < LCEChainNumExplosions; i++)
		{
			explosionTimes[i] = explosionTimes[i - 1] + perExplosionTime;
		}
		Vector2 lastValidPosition = startPosition;
		bool hitWall = false;
		int index = 0;
		float elapsed = 0f;
		lastValidPosition = startPosition;
		hitWall = false;
		Vector2 currentDirection = direction;
		RoomHandler currentRoom = startPosition.GetAbsoluteRoom();
		float enemyDistance = -1f;
		AIActor nearestEnemy = currentRoom.GetNearestEnemyInDirection(startPosition, currentDirection, 35f, out enemyDistance, true, (!enemySRB) ? null : enemySRB.aiActor);
		if ((bool)nearestEnemy && enemyDistance < 20f)
		{
			currentDirection = (nearestEnemy.CenterPosition - startPosition).normalized;
		}
		while (elapsed < LCEChainDuration)
		{
			for (elapsed += BraveTime.DeltaTime; index < LCEChainNumExplosions && elapsed >= explosionTimes[index]; index++)
			{
				Vector2 b = startPosition + currentDirection.normalized * LCEChainDistance;
				Vector2 vector = Vector2.Lerp(startPosition, b, ((float)index + 1f) / (float)LCEChainNumExplosions);
				if (!ValidExplosionPosition(vector))
				{
					hitWall = true;
				}
				if (!hitWall)
				{
					lastValidPosition = vector;
				}
				Exploder.Explode(lastValidPosition, LinearChainExplosionData, currentDirection);
			}
			yield return null;
		}
	}

	private bool ValidExplosionPosition(Vector2 pos)
	{
		IntVector2 intVector = pos.ToIntVector2(VectorConversions.Floor);
		return GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector) && GameManager.Instance.Dungeon.data[intVector].type != CellType.WALL;
	}

	private Projectile HandlePreFireProjectileModifier(Gun sourceGun, Projectile sourceProjectile)
	{
		if (AddsCriticalChance)
		{
			float num = ActivationChance;
			if (NormalizeAcrossFireRate && (bool)sourceGun)
			{
				float num2 = 1f / sourceGun.DefaultModule.cooldownTime;
				if (sourceGun.Volley != null && sourceGun.Volley.UsesShotgunStyleVelocityRandomizer)
				{
					num2 *= (float)Mathf.Max(1, sourceGun.Volley.projectiles.Count);
				}
				num = Mathf.Clamp01(ActivationsPerSecond / num2);
				num = Mathf.Max(MinActivationChance, num);
			}
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.VORPAL_BLADE))
			{
				num *= 0.25f;
			}
			if (UnityEngine.Random.value < num)
			{
				return CriticalProjectile;
			}
		}
		return sourceProjectile;
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		float num = ActivationChance;
		Gun gun = ((!m_player) ? null : m_player.CurrentGun);
		if (NormalizeAcrossFireRate && (bool)gun)
		{
			float num2 = 1f / gun.DefaultModule.cooldownTime;
			if (AddsChanceToBlank && gun.Volley != null && gun.Volley.UsesShotgunStyleVelocityRandomizer)
			{
				num2 *= (float)gun.Volley.projectiles.Count;
			}
			num = Mathf.Clamp01(ActivationsPerSecond / num2);
			if (UsesAlternateActivationChanceInBossRooms && (bool)m_player && m_player.CurrentRoom != null && m_player.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				num = Mathf.Clamp01(BossActivationsPerSecond / num2);
			}
			num = Mathf.Max(MinActivationChance, num);
		}
		if (UsesChanceForAdditionalProjectile && (bool)m_player && m_player.HasActiveBonusSynergy(CustomSynergyType.SHADOW_BACKUP) && Vector2.Dot(obj.transform.right, m_player.unadjustedAimPoint.XY() - m_player.CenterPosition) < -0.75f)
		{
			num = 1f;
		}
		if (!(UnityEngine.Random.value < num))
		{
			return;
		}
		if (AddsChainLightning)
		{
			ChainLightningModifier orAddComponent = obj.gameObject.GetOrAddComponent<ChainLightningModifier>();
			orAddComponent.LinkVFXPrefab = ChainLightningVFX;
			orAddComponent.damageTypes = ChainLightningDamageTypes;
			orAddComponent.maximumLinkDistance = ChainLightningMaxLinkDistance;
			orAddComponent.damagePerHit = ChainLightningDamagePerHit;
			orAddComponent.damageCooldown = ChainLightningDamageCooldown;
			if ((bool)m_player && m_player.HasActiveBonusSynergy(CustomSynergyType.TESLA_UNBOUND))
			{
				orAddComponent.maximumLinkDistance *= 3f;
				orAddComponent.CanChainToAnyProjectile = true;
			}
			if (ChainLightningDispersalParticles != null)
			{
				orAddComponent.UsesDispersalParticles = true;
				orAddComponent.DispersalParticleSystemPrefab = ChainLightningDispersalParticles;
				orAddComponent.DispersalDensity = ChainLightningDispersalDensity;
				orAddComponent.DispersalMinCoherency = ChainLightningDispersalMinCoherence;
				orAddComponent.DispersalMaxCoherency = ChainLightningDispersalMaxCoherence;
			}
			else
			{
				orAddComponent.UsesDispersalParticles = false;
			}
		}
		if (AddsExplosino && !obj.gameObject.GetComponent<ExplosiveModifier>())
		{
			ExplosiveModifier explosiveModifier = obj.gameObject.AddComponent<ExplosiveModifier>();
			explosiveModifier.doExplosion = true;
			explosiveModifier.explosionData = ExplosionData;
		}
		if (UsesChanceForAdditionalProjectile)
		{
			base.Owner.SpawnShadowBullet(obj, true);
		}
		if (AddsSpawnProjectileModifier && !obj.gameObject.GetComponent<SpawnProjModifier>())
		{
			SpawnProjModifier spawnProjModifier = obj.gameObject.AddComponent<SpawnProjModifier>();
			spawnProjModifier.SpawnedProjectilesInheritAppearance = SpawnProjectileInheritsApperance;
			spawnProjModifier.SpawnedProjectileScaleModifier = SpawnProjectileScaleModifier;
			spawnProjModifier.SpawnedProjectilesInheritData = true;
			spawnProjModifier.spawnProjectilesOnCollision = true;
			spawnProjModifier.spawnProjecitlesOnDieInAir = true;
			spawnProjModifier.doOverrideObjectCollisionSpawnStyle = true;
			spawnProjModifier.startAngle = UnityEngine.Random.Range(0, 180);
			int numberToSpawnOnCollison = NumberToSpawnOnCollision;
			if (ScaleSpawnsByFireRate && (bool)gun)
			{
				float value = 1f / gun.DefaultModule.cooldownTime;
				if (gun.Volley.projectiles.Count > 2)
				{
					int num3 = 0;
					for (int i = 0; i < gun.Volley.projectiles.Count; i++)
					{
						num3 = ((gun.Volley.projectiles[i] == null || !gun.Volley.projectiles[i].mirror) ? (num3 + 1) : (num3 + 2));
					}
					value = Mathf.Lerp(MinFlakFireRate, MaxFlakFireRate, (float)num3 / 5f);
				}
				numberToSpawnOnCollison = Mathf.RoundToInt(Mathf.Lerp(MinFlakSpawns, (float)MaxFlakSpawns * 1f, Mathf.InverseLerp(MaxFlakFireRate, MinFlakFireRate, value)));
			}
			if (obj.SpawnedFromOtherPlayerProjectile)
			{
				numberToSpawnOnCollison = 2;
			}
			spawnProjModifier.numberToSpawnOnCollison = numberToSpawnOnCollison;
			spawnProjModifier.projectileToSpawnOnCollision = CollisionSpawnProjectile;
			spawnProjModifier.collisionSpawnStyle = SpawnProjModifier.CollisionSpawnStyle.FLAK_BURST;
		}
		else if (AddsSpawnProjectileModifier)
		{
			SpawnProjModifier component = obj.gameObject.GetComponent<SpawnProjModifier>();
			component.PostprocessSpawnedProjectiles = true;
		}
		if (AddsTrailedSpawn)
		{
			obj.StartCoroutine(HandleTrailedSpawn(obj));
		}
		if (AddsDevolverModifier && !obj.gameObject.GetComponent<DevolverModifier>())
		{
			DevolverModifier devolverModifier = obj.gameObject.AddComponent<DevolverModifier>();
			devolverModifier.chanceToDevolve = DevolverSourceModifier.chanceToDevolve;
			devolverModifier.DevolverHierarchy = DevolverSourceModifier.DevolverHierarchy;
			devolverModifier.EnemyGuidsToIgnore = DevolverSourceModifier.EnemyGuidsToIgnore;
		}
		if (AddsHungryBullets && !obj.gameObject.GetComponent<HungryProjectileModifier>())
		{
			HungryProjectileModifier hungryProjectileModifier = obj.gameObject.AddComponent<HungryProjectileModifier>();
			hungryProjectileModifier.HungryRadius = HungryRadius;
			hungryProjectileModifier.DamagePercentGainPerSnack = DamagePercentGainPerSnack;
			hungryProjectileModifier.MaxMultiplier = HungryMaxMultiplier;
			hungryProjectileModifier.MaximumBulletsEaten = MaximumBulletsEaten;
		}
		if (AddsLinearChainExplosionOnKill)
		{
			obj.OnWillKillEnemy = (Action<Projectile, SpeculativeRigidbody>)Delegate.Combine(obj.OnWillKillEnemy, new Action<Projectile, SpeculativeRigidbody>(HandleWillKillEnemy));
		}
		if ((bool)m_player && AddsChanceToBlank)
		{
			obj.OnDestruction += HandleBlankOnDestruction;
		}
	}

	private void HandleLinearChainBeamHitEnemy(Projectile sourceProjectile, SpeculativeRigidbody enemy, bool fatal)
	{
		if (!AddsLinearChainExplosionOnKill || !enemy || !fatal)
		{
			return;
		}
		Vector2 vector = ((!enemy.aiActor) ? enemy.transform.position.XY() : enemy.aiActor.CenterPosition);
		Debug.LogError(vector);
		Vector2 vector2 = (sourceProjectile ? sourceProjectile.LastVelocity.normalized : ((!enemy.healthHaver) ? BraveMathCollege.DegreesToVector(base.Owner.FacingDirection) : enemy.healthHaver.lastIncurredDamageDirection));
		if ((bool)sourceProjectile)
		{
			BasicBeamController component = sourceProjectile.GetComponent<BasicBeamController>();
			if ((bool)component)
			{
				vector2 = component.Direction.normalized;
			}
		}
		if (vector2.magnitude < 0.05f)
		{
			vector2 = UnityEngine.Random.insideUnitCircle.normalized;
		}
		GameManager.Instance.Dungeon.StartCoroutine(HandleChainExplosion(enemy, vector, vector2.normalized));
	}

	private void HandleWillKillEnemy(Projectile sourceProjectile, SpeculativeRigidbody enemy)
	{
		if (AddsLinearChainExplosionOnKill && (bool)enemy)
		{
			Vector2 vector = ((!enemy.aiActor) ? enemy.transform.position.XY() : enemy.aiActor.CenterPosition);
			Debug.LogError(vector);
			Vector2 vector2 = (sourceProjectile ? sourceProjectile.LastVelocity.normalized : ((!enemy.healthHaver) ? BraveMathCollege.DegreesToVector(base.Owner.FacingDirection) : enemy.healthHaver.lastIncurredDamageDirection));
			if (vector2.magnitude < 0.05f)
			{
				vector2 = UnityEngine.Random.insideUnitCircle.normalized;
			}
			GameManager.Instance.Dungeon.StartCoroutine(HandleChainExplosion(enemy, vector, vector2.normalized));
		}
	}

	private void DoTrailedSpawns(Projectile p, ref Vector2 lastSpawnedPosition, ref float lastElapsedDistance)
	{
		float num = (p.transform.position.XY() - lastSpawnedPosition).magnitude;
		if (num > TrailedObjectSpawnDistance)
		{
			Vector2 vector = p.transform.position.XY() - lastSpawnedPosition;
			while (num > TrailedObjectSpawnDistance)
			{
				num -= TrailedObjectSpawnDistance;
				lastSpawnedPosition += vector.normalized * TrailedObjectSpawnDistance;
				Vector2 vector2 = new Vector2(-0.5f, -1f) + UnityEngine.Random.insideUnitCircle * 0.25f;
				SpawnManager.SpawnVFX(TrailedObjectToSpawn, lastSpawnedPosition + vector2, Quaternion.identity);
				Exploder.DoRadialDamage(5f, lastSpawnedPosition + vector2, 0.5f, false, true);
			}
			lastElapsedDistance = p.GetElapsedDistance();
		}
	}

	private IEnumerator HandleTrailedSpawn(Projectile p)
	{
		Vector2 lastSpawnedPosition = p.transform.position.XY();
		float lastElapsedDistance = p.GetElapsedDistance();
		p.OnDestruction += delegate(Projectile src)
		{
			DoTrailedSpawns(src, ref lastSpawnedPosition, ref lastElapsedDistance);
		};
		while ((bool)p)
		{
			DoTrailedSpawns(p, ref lastSpawnedPosition, ref lastElapsedDistance);
			yield return null;
		}
	}

	private void HandleBlankOnDestruction(Projectile obj)
	{
		if ((bool)m_player && (bool)obj)
		{
			DoMicroBlank((!obj.specRigidbody) ? obj.transform.position.XY() : obj.specRigidbody.UnitCenter);
		}
	}

	private void DoMicroBlank(Vector2 center)
	{
		GameObject silencerVFX = (GameObject)ResourceCache.Acquire("Global VFX/BlankVFX_Ghost");
		AkSoundEngine.PostEvent("Play_OBJ_silenceblank_small_01", base.gameObject);
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		float additionalTimeAtMaxRadius = 0.25f;
		silencerInstance.TriggerSilencer(center, 20f, BlankRadius, silencerVFX, 0f, 3f, 3f, 3f, 30f, 3f, additionalTimeAtMaxRadius, m_player);
	}

	private void PostProcessBeamChanceTick(BeamController beamController)
	{
		if (UnityEngine.Random.value < ActivationChance)
		{
			beamController.ChanceBasedShadowBullet = true;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<ComplexProjectileModifier>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		player.PostProcessBeamChanceTick -= PostProcessBeamChanceTick;
		player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Remove(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModifier));
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
			m_player.PostProcessBeamChanceTick -= PostProcessBeamChanceTick;
			PlayerController player = m_player;
			player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Remove(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModifier));
		}
	}
}
