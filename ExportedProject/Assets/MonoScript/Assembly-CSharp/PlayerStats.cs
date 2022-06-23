using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
	public enum StatType
	{
		MovementSpeed,
		RateOfFire,
		Accuracy,
		Health,
		Coolness,
		Damage,
		ProjectileSpeed,
		AdditionalGunCapacity,
		AdditionalItemCapacity,
		AmmoCapacityMultiplier,
		ReloadSpeed,
		AdditionalShotPiercing,
		KnockbackMultiplier,
		GlobalPriceMultiplier,
		Curse,
		PlayerBulletScale,
		AdditionalClipCapacityMultiplier,
		AdditionalShotBounces,
		AdditionalBlanksPerFloor,
		ShadowBulletChance,
		ThrownGunDamage,
		DodgeRollDamage,
		DamageToBosses,
		EnemyProjectileSpeedMultiplier,
		ExtremeShadowBulletChance,
		ChargeAmountMultiplier,
		RangeMultiplier,
		DodgeRollDistanceMultiplier,
		DodgeRollSpeedMultiplier,
		TarnisherClipCapacityMultiplier,
		MoneyMultiplierFromEnemies
	}

	public int NumBlanksPerFloor = 3;

	public int NumBlanksPerFloorCoop = 2;

	public float rollDamage = 4f;

	[Header("Status Effect Things")]
	public bool UsesFireSourceEffect;

	public GameActorFireEffect OnFireSourceEffect;

	[SerializeField]
	[Header("Base Stat Values")]
	public List<float> BaseStatValues;

	[NonSerialized]
	public List<int> PreviouslyActiveSynergies;

	[NonSerialized]
	public List<CustomSynergyType> ActiveCustomSynergies = new List<CustomSynergyType>();

	protected List<float> StatValues;

	private const bool c_BonusSynergies = true;

	protected float m_magnificence;

	protected float m_floorMagnificence;

	public float MovementSpeed
	{
		get
		{
			return StatValues[0];
		}
	}

	public float Magnificence
	{
		get
		{
			return m_magnificence + m_floorMagnificence;
		}
	}

	public event Action<ProjectileVolleyData> AdditionalVolleyModifiers;

	public static int GetTotalCurse()
	{
		int num = 0;
		if ((bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.stats.StatValues != null)
		{
			num += Mathf.FloorToInt(GameManager.Instance.PrimaryPlayer.stats.GetStatValue(StatType.Curse));
		}
		if ((bool)GameManager.Instance.SecondaryPlayer && GameManager.Instance.SecondaryPlayer.stats.StatValues != null)
		{
			num += Mathf.FloorToInt(GameManager.Instance.SecondaryPlayer.stats.GetStatValue(StatType.Curse));
		}
		GameStatsManager.Instance.UpdateMaximum(TrackedMaximums.HIGHEST_CURSE_LEVEL, num);
		return num;
	}

	public static int GetTotalCoolness()
	{
		int num = 0;
		if ((bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.stats.StatValues != null)
		{
			num += Mathf.FloorToInt(GameManager.Instance.PrimaryPlayer.stats.GetStatValue(StatType.Coolness));
		}
		if ((bool)GameManager.Instance.SecondaryPlayer && GameManager.Instance.SecondaryPlayer.stats.StatValues != null)
		{
			num += Mathf.FloorToInt(GameManager.Instance.SecondaryPlayer.stats.GetStatValue(StatType.Coolness));
		}
		return num;
	}

	public static float GetTotalEnemyProjectileSpeedMultiplier()
	{
		float num = 1f;
		if ((bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.stats.StatValues != null)
		{
			num *= GameManager.Instance.PrimaryPlayer.stats.GetStatValue(StatType.EnemyProjectileSpeedMultiplier);
		}
		if ((bool)GameManager.Instance.SecondaryPlayer && GameManager.Instance.SecondaryPlayer.stats.StatValues != null)
		{
			num *= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(StatType.EnemyProjectileSpeedMultiplier);
		}
		return num;
	}

	public void CopyFrom(PlayerStats prefab)
	{
		NumBlanksPerFloor = prefab.NumBlanksPerFloor;
		NumBlanksPerFloorCoop = prefab.NumBlanksPerFloorCoop;
		rollDamage = prefab.rollDamage;
		UsesFireSourceEffect = prefab.UsesFireSourceEffect;
		OnFireSourceEffect = prefab.OnFireSourceEffect;
		BaseStatValues = new List<float>();
		for (int i = 0; i < prefab.BaseStatValues.Count; i++)
		{
			BaseStatValues.Add(prefab.BaseStatValues[i]);
		}
	}

	public float GetBaseStatValue(StatType stat)
	{
		return BaseStatValues[(int)stat];
	}

	public void SetBaseStatValue(StatType stat, float value, PlayerController owner)
	{
		BaseStatValues[(int)stat] = value;
		RecalculateStats(owner, true);
	}

	public float GetStatValue(StatType stat)
	{
		return StatValues[(int)stat];
	}

	public float GetStatModifier(StatType stat)
	{
		if (!Application.isPlaying)
		{
			return 1f;
		}
		if (stat < StatType.MovementSpeed || (int)stat >= StatValues.Count)
		{
			return 1f;
		}
		return StatValues[(int)stat] / BaseStatValues[(int)stat];
	}

	public void RebuildGunVolleys(PlayerController owner)
	{
		if (owner.inventory == null || owner.inventory.AllGuns == null || owner.inventory.AllGuns.Count == 0)
		{
			return;
		}
		for (int i = 0; i < owner.inventory.AllGuns.Count; i++)
		{
			Gun gun = owner.inventory.AllGuns[i];
			ProjectileVolleyData modifiedVolley = gun.modifiedVolley;
			gun.modifiedVolley = null;
			gun.modifiedFinalVolley = null;
			ProjectileVolleyData projectileVolleyData = ScriptableObject.CreateInstance<ProjectileVolleyData>();
			if (gun.Volley != null)
			{
				projectileVolleyData.InitializeFrom(gun.Volley);
			}
			else
			{
				projectileVolleyData.projectiles = new List<ProjectileModule>();
				projectileVolleyData.projectiles.Add(ProjectileModule.CreateClone(gun.singleModule));
				projectileVolleyData.BeamRotationDegreesPerSecond = float.MaxValue;
			}
			ModVolley(owner, projectileVolleyData);
			for (int j = 0; j < projectileVolleyData.projectiles.Count; j++)
			{
				if (projectileVolleyData.projectiles[j].numberOfShotsInClip > 0)
				{
					projectileVolleyData.projectiles[j].numberOfShotsInClip = Mathf.Max(1, projectileVolleyData.projectiles[j].numberOfShotsInClip + gun.AdditionalClipCapacity);
				}
			}
			if (gun.PostProcessVolley != null)
			{
				gun.PostProcessVolley(projectileVolleyData);
			}
			gun.modifiedVolley = projectileVolleyData;
			if (gun.DefaultModule.HasFinalVolleyOverride())
			{
				ProjectileVolleyData projectileVolleyData2 = ScriptableObject.CreateInstance<ProjectileVolleyData>();
				projectileVolleyData2.InitializeFrom(gun.DefaultModule.finalVolley);
				ModVolley(owner, projectileVolleyData2);
				gun.modifiedFinalVolley = projectileVolleyData2;
			}
			if (gun.rawOptionalReloadVolley != null)
			{
				ProjectileVolleyData projectileVolleyData3 = ScriptableObject.CreateInstance<ProjectileVolleyData>();
				projectileVolleyData3.InitializeFrom(gun.rawOptionalReloadVolley);
				ModVolley(owner, projectileVolleyData3);
				gun.modifiedOptionalReloadVolley = projectileVolleyData3;
			}
			for (int k = 0; k < projectileVolleyData.projectiles.Count; k++)
			{
				if (string.IsNullOrEmpty(projectileVolleyData.projectiles[k].runtimeGuid))
				{
					projectileVolleyData.projectiles[k].runtimeGuid = Guid.NewGuid().ToString();
				}
			}
			gun.ReinitializeModuleData(modifiedVolley);
		}
		for (int l = 0; l < owner.passiveItems.Count; l++)
		{
			if (owner.passiveItems[l] is FireVolleyOnRollItem)
			{
				FireVolleyOnRollItem fireVolleyOnRollItem = owner.passiveItems[l] as FireVolleyOnRollItem;
				fireVolleyOnRollItem.ModVolley = null;
				ProjectileVolleyData projectileVolleyData4 = ScriptableObject.CreateInstance<ProjectileVolleyData>();
				projectileVolleyData4.InitializeFrom(fireVolleyOnRollItem.Volley);
				ModVolley(owner, projectileVolleyData4);
				fireVolleyOnRollItem.ModVolley = projectileVolleyData4;
			}
		}
	}

	private void ModVolley(PlayerController owner, ProjectileVolleyData volley)
	{
		for (int i = 0; i < owner.passiveItems.Count; i++)
		{
			PassiveItem passiveItem = owner.passiveItems[i];
			if (passiveItem is GunVolleyModificationItem)
			{
				GunVolleyModificationItem gunVolleyModificationItem = passiveItem as GunVolleyModificationItem;
				gunVolleyModificationItem.ModifyVolley(volley);
			}
		}
		PlayerItem currentItem = owner.CurrentItem;
		if (currentItem is ActiveGunVolleyModificationItem && currentItem.IsActive)
		{
			ActiveGunVolleyModificationItem activeGunVolleyModificationItem = currentItem as ActiveGunVolleyModificationItem;
			activeGunVolleyModificationItem.ModifyVolley(volley);
		}
		if (this.AdditionalVolleyModifiers != null)
		{
			this.AdditionalVolleyModifiers(volley);
		}
	}

	private void ApplyStatModifier(StatModifier modifier, float[] statModsAdditive, float[] statModsMultiplic)
	{
		int statToBoost = (int)modifier.statToBoost;
		if (modifier.modifyType == StatModifier.ModifyMethod.ADDITIVE)
		{
			statModsAdditive[statToBoost] += modifier.amount;
		}
		else if (modifier.modifyType == StatModifier.ModifyMethod.MULTIPLICATIVE)
		{
			statModsMultiplic[statToBoost] *= modifier.amount;
		}
	}

	public void RecalculateStats(PlayerController owner, bool force = false, bool recursive = false)
	{
		RecalculateStatsInternal(owner);
		if (!recursive && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(owner);
			if ((bool)otherPlayer && otherPlayer.stats != null)
			{
				otherPlayer.stats.RecalculateStats(otherPlayer, force, true);
			}
		}
	}

	private void RecalculateSynergies(PlayerController owner)
	{
		if (PreviouslyActiveSynergies == null)
		{
			PreviouslyActiveSynergies = new List<int>();
		}
		PreviouslyActiveSynergies.Clear();
		PreviouslyActiveSynergies.AddRange(owner.ActiveExtraSynergies);
		if (!GameManager.Instance || !GameManager.Instance.SynergyManager || !owner)
		{
			return;
		}
		GameManager.Instance.SynergyManager.RebuildSynergies(owner, PreviouslyActiveSynergies);
		bool flag = false;
		int num = -1;
		for (int i = 0; i < owner.ActiveExtraSynergies.Count; i++)
		{
			if (!GameManager.Instance.SynergyManager.synergies[owner.ActiveExtraSynergies[i]].SuppressVFX && GameManager.Instance.SynergyManager.synergies[owner.ActiveExtraSynergies[i]].ActivationStatus != SynergyEntry.SynergyActivation.INACTIVE && !PreviouslyActiveSynergies.Contains(owner.ActiveExtraSynergies[i]))
			{
				flag = true;
				num = owner.ActiveExtraSynergies[i];
				GameStatsManager.Instance.HandleEncounteredSynergy(num);
				break;
			}
		}
		if (flag)
		{
			owner.PlayEffectOnActor((GameObject)ResourceCache.Acquire("Global VFX/VFX_Synergy"), new Vector3(0f, 0.5f, 0f));
			AdvancedSynergyEntry advancedSynergyEntry = GameManager.Instance.SynergyManager.synergies[num];
			if (advancedSynergyEntry.ActivationStatus != SynergyEntry.SynergyActivation.INACTIVE && !string.IsNullOrEmpty(advancedSynergyEntry.NameKey))
			{
				GameUIRoot.Instance.notificationController.AttemptSynergyAttachment(advancedSynergyEntry);
			}
		}
		PreviouslyActiveSynergies.Clear();
		PreviouslyActiveSynergies.AddRange(owner.ActiveExtraSynergies);
	}

	public void RecalculateStatsInternal(PlayerController owner)
	{
		owner.DeferredStatRecalculationRequired = false;
		RecalculateSynergies(owner);
		int totalCurse = GetTotalCurse();
		if (StatValues == null)
		{
			StatValues = new List<float>();
		}
		StatValues.Clear();
		for (int i = 0; i < BaseStatValues.Count; i++)
		{
			StatValues.Add(BaseStatValues[i]);
		}
		float[] array = new float[StatValues.Count];
		float[] array2 = new float[StatValues.Count];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = 1f;
		}
		float num = 0f;
		ActiveCustomSynergies.Clear();
		for (int k = 0; k < owner.ActiveExtraSynergies.Count; k++)
		{
			AdvancedSynergyEntry advancedSynergyEntry = GameManager.Instance.SynergyManager.synergies[owner.ActiveExtraSynergies[k]];
			if (!advancedSynergyEntry.SynergyIsActive(GameManager.Instance.PrimaryPlayer, GameManager.Instance.SecondaryPlayer))
			{
				continue;
			}
			for (int l = 0; l < advancedSynergyEntry.statModifiers.Count; l++)
			{
				StatModifier statModifier = advancedSynergyEntry.statModifiers[l];
				int statToBoost = (int)statModifier.statToBoost;
				if (statModifier.modifyType == StatModifier.ModifyMethod.ADDITIVE)
				{
					array[statToBoost] += statModifier.amount;
				}
				else if (statModifier.modifyType == StatModifier.ModifyMethod.MULTIPLICATIVE)
				{
					array2[statToBoost] *= statModifier.amount;
				}
			}
			for (int m = 0; m < advancedSynergyEntry.bonusSynergies.Count; m++)
			{
				ActiveCustomSynergies.Add(advancedSynergyEntry.bonusSynergies[m]);
			}
		}
		for (int n = 0; n < owner.ownerlessStatModifiers.Count; n++)
		{
			StatModifier statModifier2 = owner.ownerlessStatModifiers[n];
			if (!statModifier2.hasBeenOwnerlessProcessed && statModifier2.statToBoost == StatType.Health && statModifier2.amount > 0f)
			{
				num += statModifier2.amount;
			}
			int statToBoost2 = (int)statModifier2.statToBoost;
			if (statModifier2.modifyType == StatModifier.ModifyMethod.ADDITIVE)
			{
				array[statToBoost2] += statModifier2.amount;
			}
			else if (statModifier2.modifyType == StatModifier.ModifyMethod.MULTIPLICATIVE)
			{
				array2[statToBoost2] *= statModifier2.amount;
			}
			statModifier2.hasBeenOwnerlessProcessed = true;
		}
		for (int num2 = 0; num2 < owner.passiveItems.Count; num2++)
		{
			PassiveItem passiveItem = owner.passiveItems[num2];
			if (passiveItem.passiveStatModifiers != null && passiveItem.passiveStatModifiers.Length > 0)
			{
				for (int num3 = 0; num3 < passiveItem.passiveStatModifiers.Length; num3++)
				{
					StatModifier statModifier3 = passiveItem.passiveStatModifiers[num3];
					if (!passiveItem.HasBeenStatProcessed && statModifier3.statToBoost == StatType.Health && statModifier3.amount > 0f)
					{
						num += statModifier3.amount;
					}
					ApplyStatModifier(statModifier3, array, array2);
				}
			}
			if (passiveItem is BasicStatPickup)
			{
				BasicStatPickup basicStatPickup = passiveItem as BasicStatPickup;
				for (int num4 = 0; num4 < basicStatPickup.modifiers.Count; num4++)
				{
					StatModifier statModifier4 = basicStatPickup.modifiers[num4];
					if (!passiveItem.HasBeenStatProcessed && statModifier4.statToBoost == StatType.Health && statModifier4.amount > 0f)
					{
						num += statModifier4.amount;
					}
					ApplyStatModifier(statModifier4, array, array2);
				}
			}
			if (passiveItem is CoopPassiveItem && (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER || ((bool)GameManager.Instance.PrimaryPlayer.healthHaver && GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) || owner.HasActiveBonusSynergy(CustomSynergyType.THE_TRUE_HERO)))
			{
				CoopPassiveItem coopPassiveItem = passiveItem as CoopPassiveItem;
				for (int num5 = 0; num5 < coopPassiveItem.modifiers.Count; num5++)
				{
					StatModifier modifier = coopPassiveItem.modifiers[num5];
					ApplyStatModifier(modifier, array, array2);
				}
			}
			if (passiveItem is MetronomeItem)
			{
				float currentMultiplier = (passiveItem as MetronomeItem).GetCurrentMultiplier();
				array2[5] *= currentMultiplier;
			}
			passiveItem.HasBeenStatProcessed = true;
		}
		if (owner.inventory != null && owner.inventory.AllGuns != null)
		{
			if (owner.inventory.CurrentGun != null && owner.inventory.CurrentGun.currentGunStatModifiers != null && owner.inventory.CurrentGun.currentGunStatModifiers.Length > 0)
			{
				for (int num6 = 0; num6 < owner.inventory.CurrentGun.currentGunStatModifiers.Length; num6++)
				{
					StatModifier modifier2 = owner.inventory.CurrentGun.currentGunStatModifiers[num6];
					ApplyStatModifier(modifier2, array, array2);
				}
			}
			for (int num7 = 0; num7 < owner.inventory.AllGuns.Count; num7++)
			{
				if ((bool)owner.inventory.AllGuns[num7] && owner.inventory.AllGuns[num7].passiveStatModifiers != null && owner.inventory.AllGuns[num7].passiveStatModifiers.Length > 0)
				{
					for (int num8 = 0; num8 < owner.inventory.AllGuns[num7].passiveStatModifiers.Length; num8++)
					{
						StatModifier modifier3 = owner.inventory.AllGuns[num7].passiveStatModifiers[num8];
						ApplyStatModifier(modifier3, array, array2);
					}
				}
			}
		}
		for (int num9 = 0; num9 < owner.activeItems.Count; num9++)
		{
			PlayerItem playerItem = owner.activeItems[num9];
			if (playerItem.passiveStatModifiers != null && playerItem.passiveStatModifiers.Length > 0)
			{
				for (int num10 = 0; num10 < playerItem.passiveStatModifiers.Length; num10++)
				{
					StatModifier statModifier5 = playerItem.passiveStatModifiers[num10];
					if (!playerItem.HasBeenStatProcessed && statModifier5.statToBoost == StatType.Health && statModifier5.amount > 0f)
					{
						num += statModifier5.amount;
					}
					ApplyStatModifier(statModifier5, array, array2);
				}
			}
			StatHolder component = playerItem.GetComponent<StatHolder>();
			if ((bool)component && (!component.RequiresPlayerItemActive || playerItem.IsCurrentlyActive))
			{
				for (int num11 = 0; num11 < component.modifiers.Length; num11++)
				{
					StatModifier statModifier6 = component.modifiers[num11];
					if (!playerItem.HasBeenStatProcessed && statModifier6.statToBoost == StatType.Health && statModifier6.amount > 0f)
					{
						num += statModifier6.amount;
					}
					ApplyStatModifier(statModifier6, array, array2);
				}
			}
			playerItem.HasBeenStatProcessed = true;
		}
		PlayerItem currentItem = owner.CurrentItem;
		if ((bool)currentItem && currentItem is ActiveBasicStatItem && currentItem.IsActive)
		{
			ActiveBasicStatItem activeBasicStatItem = currentItem as ActiveBasicStatItem;
			for (int num12 = 0; num12 < activeBasicStatItem.modifiers.Count; num12++)
			{
				StatModifier modifier4 = activeBasicStatItem.modifiers[num12];
				ApplyStatModifier(modifier4, array, array2);
			}
		}
		for (int num13 = 0; num13 < StatValues.Count; num13++)
		{
			StatValues[num13] = BaseStatValues[num13] * array2[num13] + array[num13];
		}
		float num14 = 0f;
		int num15 = ((!owner.AllowZeroHealthState) ? 1 : 0);
		if (StatValues[3] < (float)num15)
		{
			StatValues[3] = num15;
		}
		if (owner.ForceZeroHealthState)
		{
			StatValues[3] = 0f;
		}
		if (owner.healthHaver.GetMaxHealth() != StatValues[3] + num14)
		{
			owner.healthHaver.SetHealthMaximum(StatValues[3] + num14, num);
		}
		owner.UpdateInventoryMaxGuns();
		owner.UpdateInventoryMaxItems();
		RebuildGunVolleys(owner);
		int totalCurse2 = GetTotalCurse();
		if (totalCurse2 > totalCurse && !MidGameSaveData.IsInitializingPlayerData)
		{
			owner.PlayEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_Curse") as GameObject, Vector3.zero);
		}
		if (totalCurse2 >= 10 && !MidGameSaveData.IsInitializingPlayerData)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.HAVE_MAX_CURSE);
			if (!GameManager.Instance.Dungeon.CurseReaperActive)
			{
				GameManager.Instance.Dungeon.SpawnCurseReaper();
			}
		}
	}

	public void AddFloorMagnificence(float m)
	{
		m_floorMagnificence += m;
	}

	public void ToNextLevel()
	{
		m_magnificence += m_floorMagnificence;
		m_floorMagnificence = 0f;
	}
}
