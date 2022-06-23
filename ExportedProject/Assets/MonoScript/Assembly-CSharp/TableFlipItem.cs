using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;
using UnityEngine;

public class TableFlipItem : PassiveItem
{
	public bool TableTriggersBlankEffect;

	public bool TableStunsEnemies;

	[ShowInInspectorIf("TableStunsEnemies", false)]
	public float ChanceToStun = 1f;

	[ShowInInspectorIf("TableStunsEnemies", false)]
	public float StunDuration = 4f;

	[ShowInInspectorIf("TableStunsEnemies", false)]
	public float StunRadius = 10f;

	[ShowInInspectorIf("TableStunsEnemies", false)]
	public bool StunsAllEnemiesInRoom;

	public bool TableGivesCurrency;

	[ShowInInspectorIf("TableGivesCurrency", false)]
	public float ChanceToGiveCurrency = 1f;

	[ShowInInspectorIf("TableGivesCurrency", false)]
	public int CurrencyToGiveMin = 1;

	[ShowInInspectorIf("TableGivesCurrency", false)]
	public int CurrencyToGiveMax = 1;

	public bool TableGivesRage;

	[ShowInInspectorIf("TableGivesRage", false)]
	public float RageDamageMultiplier = 2f;

	[ShowInInspectorIf("TableGivesRage", false)]
	public float RageDuration = 5f;

	[ShowInInspectorIf("TableGivesRage", false)]
	public Color RageFlatColor = new Color(0.5f, 0f, 0f, 0.75f);

	[ShowInInspectorIf("TableGivesRage", false)]
	public GameObject RageOverheadVFX;

	public bool AddsModuleCopies;

	[ShowInInspectorIf("AddsModuleCopies", false)]
	public float ModuleCopyDuration = 5f;

	[ShowInInspectorIf("AddsModuleCopies", false)]
	public int ModuleCopyCount = 1;

	public bool TableBecomesProjectile;

	[ShowInInspectorIf("TableBecomesProjectile", false)]
	public ExplosionData ProjectileExplosionData;

	[ShowInInspectorIf("TableBecomesProjectile", false)]
	public float DirectHitBonusDamage = 10f;

	[ShowInInspectorIf("TableBecomesProjectile", false)]
	public AnimationCurve CustomAccelerationCurve;

	[ShowInInspectorIf("TableBecomesProjectile", false)]
	public float CustomAccelerationCurveDuration;

	public bool TableSlowsTime;

	[ShowInInspectorIf("TableSlowsTime", false)]
	public float SlowTimeAmount = 0.5f;

	[ShowInInspectorIf("TableSlowsTime", false)]
	public float SlowTimeDuration = 3f;

	public bool TableProvidesInvulnerability;

	[ShowInInspectorIf("TableProvidesInvulnerability", false)]
	public float InvulnerableTimeDuration = 3f;

	public bool TableFlocking;

	[Space(10f)]
	public bool TableFiresVolley;

	[ShowInInspectorIf("TableFiresVolley", false)]
	public ProjectileVolleyData Volley;

	[LongNumericEnum]
	public List<CustomSynergyType> VolleyOverrideSynergies;

	public List<ProjectileVolleyData> VolleyOverrides;

	[Space(10f)]
	public bool TableHeat;

	[ShowInInspectorIf("TableHeat", false)]
	public float TableHeatRadius = 5f;

	[ShowInInspectorIf("TableHeat", false)]
	public float TableHeatSynergyRadius = 20f;

	[ShowInInspectorIf("TableHeat", false)]
	public float TableHeatDuration = 5f;

	public GameActorFireEffect TableHeatEffect;

	public GameActorFireEffect TableHeatSynergyEffect;

	[Header("Other Synergies")]
	public bool UsesTableTechBeesSynergy;

	[ShowInInspectorIf("UsesTableTechBeesSynergy", false)]
	public Projectile BeeProjectile;

	public int MinNumberOfBeesPerEnemyBullet = 1;

	public int MaxNumberOfBeesPerEnemyBullet = 1;

	public bool UsesTimeSlowSynergy;

	[LongNumericEnum]
	public CustomSynergyType TimeSlowRequiredSynergy;

	[ShowInInspectorIf("UsesTimeSlowSynergy", false)]
	public RadialSlowInterface RadialSlow;

	private const int c_beeCap = 49;

	private int m_beeCount;

	private static bool TableFlipTimeIsActive;

	private static float AdditionalTableFlipSlowTime;

	private float m_rageElapsed;

	private GameObject rageInstanceVFX;

	private float m_volleyElapsed = -1f;

	private Dictionary<FlippableCover, HeatIndicatorController> m_radialIndicators;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_owner = player;
			base.Pickup(player);
			player.OnTableFlipped = (Action<FlippableCover>)Delegate.Combine(player.OnTableFlipped, new Action<FlippableCover>(DoEffect));
			player.OnTableFlipCompleted = (Action<FlippableCover>)Delegate.Combine(player.OnTableFlipCompleted, new Action<FlippableCover>(DoEffectCompleted));
		}
	}

	private void DoEffect(FlippableCover obj)
	{
		HandleBlankEffect(obj);
		HandleStunEffect();
		HandleRageEffect();
		HandleVolleyEffect();
		StartCoroutine(HandleDelayedEffect(0.25f, HandleTableVolley, obj));
		HandleTemporalEffects();
		HandleHeatEffects(obj);
		if (UsesTimeSlowSynergy && (bool)base.Owner && base.Owner.HasActiveBonusSynergy(TimeSlowRequiredSynergy))
		{
			RadialSlow.DoRadialSlow(base.Owner.CenterPosition, base.Owner.CurrentRoom);
		}
	}

	private void DoEffectCompleted(FlippableCover obj)
	{
		HandleMoneyEffect(obj);
		HandleProjectileEffect(obj);
		HandleTableFlocking(obj);
	}

	private IEnumerator HandleDelayedEffect(float delayTime, Action<FlippableCover> effect, FlippableCover table)
	{
		yield return new WaitForSeconds(delayTime);
		effect(table);
	}

	private void HandleTableVolley(FlippableCover table)
	{
		if (!TableFiresVolley)
		{
			return;
		}
		IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(table.DirectionFlipped);
		ProjectileVolleyData sourceVolley = Volley;
		float num = 1f;
		if (VolleyOverrideSynergies != null)
		{
			for (int i = 0; i < VolleyOverrideSynergies.Count; i++)
			{
				if ((bool)m_owner && m_owner.HasActiveBonusSynergy(VolleyOverrideSynergies[i]))
				{
					sourceVolley = VolleyOverrides[i];
					num = 2f;
				}
			}
		}
		VolleyUtility.FireVolley(sourceVolley, table.sprite.WorldCenter + intVector2FromDirection.ToVector2() * num, intVector2FromDirection.ToVector2(), m_owner);
	}

	private void HandleTableFlocking(FlippableCover table)
	{
		if (!TableFlocking)
		{
			return;
		}
		RoomHandler currentRoom = base.Owner.CurrentRoom;
		ReadOnlyCollection<IPlayerInteractable> roomInteractables = currentRoom.GetRoomInteractables();
		for (int i = 0; i < roomInteractables.Count; i++)
		{
			if (!currentRoom.IsRegistered(roomInteractables[i]))
			{
				continue;
			}
			FlippableCover flippableCover = roomInteractables[i] as FlippableCover;
			if (!(flippableCover != null) || flippableCover.IsFlipped || flippableCover.IsGilded)
			{
				continue;
			}
			if (flippableCover.flipStyle == FlippableCover.FlipStyle.ANY)
			{
				flippableCover.ForceSetFlipper(base.Owner);
				flippableCover.Flip(table.DirectionFlipped);
			}
			else if (flippableCover.flipStyle == FlippableCover.FlipStyle.ONLY_FLIPS_LEFT_RIGHT)
			{
				if (table.DirectionFlipped == DungeonData.Direction.NORTH || table.DirectionFlipped == DungeonData.Direction.SOUTH)
				{
					flippableCover.ForceSetFlipper(base.Owner);
					flippableCover.Flip((!(UnityEngine.Random.value > 0.5f)) ? DungeonData.Direction.WEST : DungeonData.Direction.EAST);
				}
				else
				{
					flippableCover.ForceSetFlipper(base.Owner);
					flippableCover.Flip(table.DirectionFlipped);
				}
			}
			else if (flippableCover.flipStyle == FlippableCover.FlipStyle.ONLY_FLIPS_UP_DOWN)
			{
				if (table.DirectionFlipped == DungeonData.Direction.EAST || table.DirectionFlipped == DungeonData.Direction.WEST)
				{
					flippableCover.ForceSetFlipper(base.Owner);
					flippableCover.Flip((!(UnityEngine.Random.value > 0.5f)) ? DungeonData.Direction.SOUTH : DungeonData.Direction.NORTH);
				}
				else
				{
					flippableCover.ForceSetFlipper(base.Owner);
					flippableCover.Flip(table.DirectionFlipped);
				}
			}
		}
	}

	private void HandleProjectileEffect(FlippableCover table)
	{
		if (!TableBecomesProjectile)
		{
			return;
		}
		GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Table_Exhaust");
		Vector2 vector = DungeonData.GetIntVector2FromDirection(table.DirectionFlipped).ToVector2();
		float z = BraveMathCollege.Atan2Degrees(vector);
		Vector3 vector2 = Vector3.zero;
		switch (table.DirectionFlipped)
		{
		case DungeonData.Direction.NORTH:
			vector2 = Vector3.zero;
			break;
		case DungeonData.Direction.EAST:
			vector2 = new Vector3(-0.5f, 0.25f, 0f);
			break;
		case DungeonData.Direction.SOUTH:
			vector2 = new Vector3(0f, 0.5f, 1f);
			break;
		case DungeonData.Direction.WEST:
			vector2 = new Vector3(0.5f, 0.25f, 0f);
			break;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(original, table.specRigidbody.UnitCenter.ToVector3ZisY() + vector2, Quaternion.Euler(0f, 0f, z));
		gameObject.transform.parent = table.specRigidbody.transform;
		Projectile projectile = table.specRigidbody.gameObject.AddComponent<Projectile>();
		projectile.Shooter = base.Owner.specRigidbody;
		projectile.Owner = base.Owner;
		projectile.baseData.damage = DirectHitBonusDamage;
		projectile.baseData.range = 1000f;
		projectile.baseData.speed = 20f;
		projectile.baseData.force = 50f;
		projectile.baseData.UsesCustomAccelerationCurve = true;
		projectile.baseData.AccelerationCurve = CustomAccelerationCurve;
		projectile.baseData.CustomAccelerationCurveDuration = CustomAccelerationCurveDuration;
		projectile.shouldRotate = false;
		projectile.Start();
		projectile.SendInDirection(vector, true);
		projectile.collidesWithProjectiles = true;
		projectile.projectileHitHealth = 20;
		Action<Projectile> value = delegate
		{
			if ((bool)table && (bool)table.shadowSprite)
			{
				table.shadowSprite.renderer.enabled = false;
			}
		};
		projectile.OnDestruction += value;
		ExplosiveModifier explosiveModifier = projectile.gameObject.AddComponent<ExplosiveModifier>();
		explosiveModifier.explosionData = ProjectileExplosionData;
		table.PreventPitFalls = true;
		if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.ROCKET_POWERED_TABLES))
		{
			HomingModifier homingModifier = projectile.gameObject.AddComponent<HomingModifier>();
			homingModifier.AssignProjectile(projectile);
			homingModifier.HomingRadius = 20f;
			homingModifier.AngularVelocity = 720f;
			BounceProjModifier bounceProjModifier = projectile.gameObject.AddComponent<BounceProjModifier>();
			bounceProjModifier.numberOfBounces = 4;
			bounceProjModifier.onlyBounceOffTiles = true;
		}
	}

	private void HandleBlankEffect(FlippableCover table)
	{
		if (TableTriggersBlankEffect)
		{
			GameManager.Instance.StartCoroutine(DelayedBlankEffect(table));
		}
	}

	private IEnumerator DelayedBlankEffect(FlippableCover table)
	{
		yield return new WaitForSeconds(0.15f);
		if (!base.Owner)
		{
			yield break;
		}
		if (UsesTableTechBeesSynergy && base.Owner.HasActiveBonusSynergy(CustomSynergyType.TABLE_TECH_BEES))
		{
			m_beeCount = 0;
			if ((bool)table && (bool)table.sprite)
			{
				InternalForceBlank(table.sprite.WorldCenter, 25f, 0.5f, false, true, true, -1f, PostProcessTableTechBees);
			}
		}
		else if ((bool)table && (bool)table.sprite)
		{
			InternalForceBlank(table.sprite.WorldCenter);
		}
	}

	private void PostProcessTableTechBees(Projectile target)
	{
		for (int i = 0; i < UnityEngine.Random.Range(MinNumberOfBeesPerEnemyBullet, MaxNumberOfBeesPerEnemyBullet); i++)
		{
			if ((bool)target && (bool)base.Owner && m_beeCount < 49)
			{
				m_beeCount++;
				GameObject gameObject = SpawnManager.SpawnProjectile(BeeProjectile.gameObject, target.transform.position + UnityEngine.Random.insideUnitCircle.ToVector3ZisY(), target.transform.rotation);
				Projectile component = gameObject.GetComponent<Projectile>();
				component.Owner = base.Owner;
				component.Shooter = base.Owner.specRigidbody;
				component.collidesWithPlayer = false;
				component.collidesWithEnemies = true;
				component.collidesWithProjectiles = false;
			}
		}
	}

	private void InternalForceBlank(Vector2 center, float overrideRadius = 25f, float overrideTimeAtMaxRadius = 0.5f, bool silent = false, bool breaksWalls = true, bool breaksObjects = true, float overrideForce = -1f, Action<Projectile> customCallback = null)
	{
		GameObject silencerVFX = ((!silent) ? ((GameObject)BraveResources.Load("Global VFX/BlankVFX")) : null);
		if (!silent)
		{
			AkSoundEngine.PostEvent("Play_OBJ_silenceblank_use_01", base.gameObject);
			AkSoundEngine.PostEvent("Stop_ENM_attack_cancel_01", base.gameObject);
		}
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		if (customCallback != null)
		{
			silencerInstance.UsesCustomProjectileCallback = true;
			silencerInstance.OnCustomBlankedProjectile = customCallback;
		}
		silencerInstance.TriggerSilencer(center, 50f, overrideRadius, silencerVFX, (!silent) ? 0.15f : 0f, (!silent) ? 0.2f : 0f, (!silent) ? 50 : 0, (!silent) ? 10 : 0, silent ? 0f : ((!(overrideForce >= 0f)) ? 140f : overrideForce), breaksObjects ? ((!silent) ? 15 : 5) : 0, overrideTimeAtMaxRadius, base.Owner, breaksWalls);
		if ((bool)base.Owner)
		{
			base.Owner.DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
		}
	}

	private void HandleStunEffect()
	{
		if (!TableStunsEnemies)
		{
			return;
		}
		List<AIActor> activeEnemies = base.Owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if (!(UnityEngine.Random.value < ChanceToStun))
			{
				continue;
			}
			if (StunsAllEnemiesInRoom)
			{
				StunEnemy(activeEnemies[i]);
				continue;
			}
			float num = Vector2.Distance(activeEnemies[i].CenterPosition, base.Owner.CenterPosition);
			if (num < StunRadius)
			{
				StunEnemy(activeEnemies[i]);
			}
		}
	}

	private void StunEnemy(AIActor enemy)
	{
		if (!enemy.healthHaver.IsBoss && (bool)enemy && (bool)enemy.behaviorSpeculator)
		{
			enemy.ClearPath();
			enemy.behaviorSpeculator.Interrupt();
			enemy.behaviorSpeculator.Stun(StunDuration);
		}
	}

	private void HandleMoneyEffect(FlippableCover sourceCover)
	{
		if (TableGivesCurrency)
		{
			float chanceToGiveCurrency = ChanceToGiveCurrency;
			if (UnityEngine.Random.value < chanceToGiveCurrency)
			{
				int amountToDrop = UnityEngine.Random.Range(CurrencyToGiveMin, CurrencyToGiveMax);
				LootEngine.SpawnCurrency(sourceCover.specRigidbody.UnitCenter, amountToDrop);
			}
		}
	}

	private void HandleTemporalEffects()
	{
		if (TableSlowsTime && (!UsesTimeSlowSynergy || !base.Owner || !base.Owner.HasActiveBonusSynergy(TimeSlowRequiredSynergy)))
		{
			base.Owner.StartCoroutine(HandleTimeSlowDuration());
		}
		if (TableProvidesInvulnerability)
		{
			base.Owner.healthHaver.TriggerInvulnerabilityPeriod(InvulnerableTimeDuration);
		}
	}

	private IEnumerator HandleTimeSlowDuration()
	{
		AdditionalTableFlipSlowTime += SlowTimeDuration;
		AdditionalTableFlipSlowTime = Mathf.Min(2f * SlowTimeDuration, AdditionalTableFlipSlowTime);
		if (!TableFlipTimeIsActive)
		{
			TableFlipTimeIsActive = true;
			BraveTime.RegisterTimeScaleMultiplier(SlowTimeAmount, base.gameObject);
			float elapsed = 0f;
			while (elapsed < AdditionalTableFlipSlowTime)
			{
				elapsed += BraveTime.DeltaTime;
				yield return null;
			}
			AdditionalTableFlipSlowTime = 0f;
			TableFlipTimeIsActive = false;
			BraveTime.ClearMultiplier(base.gameObject);
		}
	}

	private void HandleRageEffect()
	{
		if (!TableGivesRage)
		{
			return;
		}
		if (m_rageElapsed > 0f)
		{
			m_rageElapsed = RageDuration;
			if (base.Owner.HasActiveBonusSynergy(CustomSynergyType.ANGRIER_BULLETS))
			{
				m_rageElapsed *= 3f;
			}
			if ((bool)RageOverheadVFX && rageInstanceVFX == null)
			{
				rageInstanceVFX = base.Owner.PlayEffectOnActor(RageOverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
			}
		}
		else
		{
			base.Owner.StartCoroutine(HandleRageCooldown());
		}
	}

	private IEnumerator HandleRageCooldown()
	{
		rageInstanceVFX = null;
		if ((bool)RageOverheadVFX)
		{
			rageInstanceVFX = base.Owner.PlayEffectOnActor(RageOverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
		}
		m_rageElapsed = RageDuration;
		if (base.Owner.HasActiveBonusSynergy(CustomSynergyType.ANGRIER_BULLETS))
		{
			m_rageElapsed *= 3f;
		}
		StatModifier damageStat = new StatModifier
		{
			amount = RageDamageMultiplier,
			modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
			statToBoost = PlayerStats.StatType.Damage
		};
		PlayerController cachedOwner = base.Owner;
		cachedOwner.ownerlessStatModifiers.Add(damageStat);
		cachedOwner.stats.RecalculateStats(cachedOwner);
		Color rageColor = RageFlatColor;
		while (m_rageElapsed > 0f)
		{
			cachedOwner.baseFlatColorOverride = rageColor.WithAlpha(Mathf.Lerp(rageColor.a, 0f, 1f - Mathf.Clamp01(m_rageElapsed)));
			if ((bool)rageInstanceVFX && m_rageElapsed < RageDuration - 1f)
			{
				rageInstanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
				rageInstanceVFX = null;
			}
			yield return null;
			m_rageElapsed -= BraveTime.DeltaTime;
		}
		if ((bool)rageInstanceVFX)
		{
			rageInstanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
		}
		cachedOwner.ownerlessStatModifiers.Remove(damageStat);
		cachedOwner.stats.RecalculateStats(cachedOwner);
	}

	private void HandleVolleyEffect()
	{
		if (AddsModuleCopies)
		{
			if (m_volleyElapsed < 0f)
			{
				base.Owner.StartCoroutine(HandleVolleyCooldown());
			}
			else
			{
				m_volleyElapsed = 0f;
			}
		}
	}

	private IEnumerator HandleVolleyCooldown()
	{
		m_volleyElapsed = 0f;
		PlayerController cachedOwner = base.Owner;
		bool wasFiring = false;
		if (cachedOwner.CurrentGun != null && cachedOwner.CurrentGun.IsFiring)
		{
			cachedOwner.CurrentGun.CeaseAttack();
			wasFiring = true;
		}
		cachedOwner.stats.AdditionalVolleyModifiers += ModifyVolley;
		cachedOwner.stats.RecalculateStats(cachedOwner);
		if (wasFiring)
		{
			cachedOwner.CurrentGun.Attack();
			for (int i = 0; i < cachedOwner.CurrentGun.ActiveBeams.Count; i++)
			{
				if (cachedOwner.CurrentGun.ActiveBeams[i] != null && cachedOwner.CurrentGun.ActiveBeams[i].beam is BasicBeamController)
				{
					(cachedOwner.CurrentGun.ActiveBeams[i].beam as BasicBeamController).ForceChargeTimer(10f);
				}
			}
		}
		while (m_volleyElapsed < ModuleCopyDuration)
		{
			m_volleyElapsed += BraveTime.DeltaTime;
			yield return null;
		}
		bool wasEndFiring = cachedOwner.CurrentGun != null && cachedOwner.CurrentGun.IsFiring;
		if (wasEndFiring)
		{
			cachedOwner.CurrentGun.CeaseAttack();
		}
		cachedOwner.stats.AdditionalVolleyModifiers -= ModifyVolley;
		cachedOwner.stats.RecalculateStats(cachedOwner);
		if (wasEndFiring)
		{
			cachedOwner.CurrentGun.Attack();
			for (int j = 0; j < cachedOwner.CurrentGun.ActiveBeams.Count; j++)
			{
				if (cachedOwner.CurrentGun.ActiveBeams[j] != null && cachedOwner.CurrentGun.ActiveBeams[j].beam is BasicBeamController)
				{
					(cachedOwner.CurrentGun.ActiveBeams[j].beam as BasicBeamController).ForceChargeTimer(10f);
				}
			}
		}
		m_volleyElapsed = -1f;
		yield return null;
	}

	private void HandleHeatEffects(FlippableCover table)
	{
		if (TableHeat && (bool)table)
		{
			table.StartCoroutine(HandleHeatEffectsCR(table));
		}
	}

	private IEnumerator HandleHeatEffectsCR(FlippableCover table)
	{
		HandleRadialIndicator(table);
		float elapsed = 0f;
		int ct = -1;
		bool hasSynergy = PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.HIDDEN_TECH_FLARE, out ct);
		RoomHandler r = table.transform.position.GetAbsoluteRoom();
		Vector3 tableCenter = ((!table.sprite) ? table.transform.position : table.sprite.WorldCenter.ToVector3ZisY());
		Action<AIActor, float> AuraAction = delegate(AIActor actor, float dist)
		{
			actor.ApplyEffect((!hasSynergy) ? TableHeatEffect : TableHeatSynergyEffect);
		};
		float modRadius = ((!hasSynergy) ? TableHeatRadius : TableHeatSynergyRadius);
		while (elapsed < TableHeatDuration)
		{
			elapsed += BraveTime.DeltaTime;
			r.ApplyActionToNearbyEnemies(tableCenter.XY(), modRadius, AuraAction);
			yield return null;
		}
		UnhandleRadialIndicator(table);
	}

	private void HandleRadialIndicator(FlippableCover table)
	{
		if (m_radialIndicators == null)
		{
			m_radialIndicators = new Dictionary<FlippableCover, HeatIndicatorController>();
		}
		if (!m_radialIndicators.ContainsKey(table))
		{
			Vector3 position = ((!table.sprite) ? table.transform.position : table.sprite.WorldCenter.ToVector3ZisY());
			m_radialIndicators.Add(table, ((GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), position, Quaternion.identity, table.transform)).GetComponent<HeatIndicatorController>());
			int count = -1;
			float currentRadius = ((!PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.HIDDEN_TECH_FLARE, out count)) ? TableHeatRadius : TableHeatSynergyRadius);
			m_radialIndicators[table].CurrentRadius = currentRadius;
		}
	}

	private void UnhandleRadialIndicator(FlippableCover table)
	{
		if (m_radialIndicators.ContainsKey(table))
		{
			HeatIndicatorController heatIndicatorController = m_radialIndicators[table];
			heatIndicatorController.EndEffect();
			m_radialIndicators.Remove(table);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<TableFlipItem>().m_pickedUpThisRun = true;
		if ((bool)player)
		{
			player.OnTableFlipped = (Action<FlippableCover>)Delegate.Remove(player.OnTableFlipped, new Action<FlippableCover>(DoEffect));
			player.OnTableFlipCompleted = (Action<FlippableCover>)Delegate.Remove(player.OnTableFlipCompleted, new Action<FlippableCover>(DoEffectCompleted));
		}
		m_owner = null;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		m_radialIndicators = null;
		BraveTime.ClearMultiplier(base.gameObject);
		if ((bool)base.Owner)
		{
			PlayerController owner = base.Owner;
			owner.OnTableFlipped = (Action<FlippableCover>)Delegate.Remove(owner.OnTableFlipped, new Action<FlippableCover>(DoEffect));
			PlayerController owner2 = base.Owner;
			owner2.OnTableFlipCompleted = (Action<FlippableCover>)Delegate.Remove(owner2.OnTableFlipCompleted, new Action<FlippableCover>(DoEffectCompleted));
		}
		base.OnDestroy();
	}

	public void ModifyVolley(ProjectileVolleyData volleyToModify)
	{
		if (ModuleCopyCount <= 0)
		{
			return;
		}
		int count = volleyToModify.projectiles.Count;
		for (int i = 0; i < count; i++)
		{
			ProjectileModule projectileModule = volleyToModify.projectiles[i];
			float num = (float)ModuleCopyCount * 10f * -1f / 2f;
			for (int j = 0; j < ModuleCopyCount; j++)
			{
				int sourceIndex = i;
				if (projectileModule.CloneSourceIndex >= 0)
				{
					sourceIndex = projectileModule.CloneSourceIndex;
				}
				ProjectileModule projectileModule2 = ProjectileModule.CreateClone(projectileModule, false, sourceIndex);
				float num2 = (projectileModule2.angleFromAim = num + 10f * (float)j);
				projectileModule2.ignoredForReloadPurposes = true;
				projectileModule2.ammoCost = 0;
				volleyToModify.projectiles.Add(projectileModule2);
			}
		}
	}
}
