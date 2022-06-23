using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class AdvancedShrineController : DungeonPlaceableBehaviour, IPlayerInteractable, IPlaceConfigurable
{
	public string displayTextKey;

	public string acceptOptionKey;

	public string declineOptionKey;

	public string spentOptionKey = "#SHRINE_GENERIC_SPENT";

	public bool IsBlankShrine;

	public bool IsRNGShrine;

	public bool IsHealthArmorSwapShrine;

	public bool IsJunkShrine;

	public bool IsBloodShrine;

	public bool IsGlassShrine;

	public List<ShrineCost> Costs;

	public List<ShrineBenefit> Benefits;

	public bool CanBeReused;

	public bool IsCleanseShrine;

	public bool IsLegendaryHeroShrine;

	public bool IncrementMoneyCostEachUse;

	public int IncrementMoneyCostAmount = 10;

	public bool ShattersOnUse;

	public GameObject ShatterSystem;

	public tk2dSprite ShatterSpriteDisable;

	public tk2dBaseSprite AlternativeOutlineTarget;

	public Transform talkPoint;

	public GameObject onPlayerVFX;

	public Vector3 playerVFXOffset;

	public tk2dBaseSprite EncounterNotificationSprite;

	private RoomHandler m_parentRoom;

	private GameObject m_instanceMinimapIcon;

	private int m_useCount;

	private int m_totalUseCount;

	private const float ChanceToGoApeshit = 0.001f;

	private float m_curChanceToBlankChestIntoExistence = 0.9f;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_parentRoom = room;
		if (!IsLegendaryHeroShrine)
		{
			room.OptionalDoorTopDecorable = ResourceCache.Acquire("Global Prefabs/Shrine_Lantern") as GameObject;
			if (!room.IsOnCriticalPath && room.connectedRooms.Count == 1)
			{
				room.ShouldAttemptProceduralLock = true;
				room.AttemptProceduralLockChance = Mathf.Max(room.AttemptProceduralLockChance, Random.Range(0.3f, 0.5f));
			}
		}
		RegisterMinimapIcon();
	}

	public void Start()
	{
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.PreventPiercing = true;
		}
		if (!StaticReferenceManager.AllAdvancedShrineControllers.Contains(this))
		{
			StaticReferenceManager.AllAdvancedShrineControllers.Add(this);
		}
	}

	public void RegisterMinimapIcon()
	{
		m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_parentRoom, (GameObject)BraveResources.Load("Global Prefabs/Minimap_Shrine_Icon"));
	}

	public void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_parentRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	private bool CheckCosts(PlayerController interactor)
	{
		bool result = true;
		for (int i = 0; i < Costs.Count; i++)
		{
			if (!Costs[i].CheckCost(interactor))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private bool CheckAndApplyCosts(PlayerController interactor)
	{
		if (CheckCosts(interactor))
		{
			for (int i = 0; i < Costs.Count; i++)
			{
				Costs[i].ApplyCost(interactor);
			}
			return true;
		}
		return false;
	}

	private void ResetForReuse()
	{
		m_useCount--;
	}

	private ShrineCost GetRandomCost()
	{
		float num = 0f;
		for (int i = 0; i < Costs.Count; i++)
		{
			num += Costs[i].rngWeight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		for (int j = 0; j < Costs.Count; j++)
		{
			num3 += Costs[j].rngWeight;
			if (num3 >= num2)
			{
				return Costs[j];
			}
		}
		return Costs[Costs.Count - 1];
	}

	private ShrineBenefit GetRandomBenefit()
	{
		float num = 0f;
		for (int i = 0; i < Benefits.Count; i++)
		{
			num += Benefits[i].rngWeight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		for (int j = 0; j < Benefits.Count; j++)
		{
			num3 += Benefits[j].rngWeight;
			if (num3 >= num2)
			{
				return Benefits[j];
			}
		}
		return Benefits[Benefits.Count - 1];
	}

	private void DoShrineEffect(PlayerController player)
	{
		if (IsJunkShrine)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_SER_JUNKAN_UNLOCKED, true);
		}
		if (IsGlassShrine)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_GLASS_SHRINE, true);
			AkSoundEngine.PostEvent("Play_OBJ_mirror_shatter_01", base.gameObject);
			AkSoundEngine.PostEvent("Play_OBJ_crystal_shatter_01", base.gameObject);
		}
		if (IsBloodShrine)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_BLOOD_SHRINED, 1f);
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_BLOOD_SHRINED) >= 2f)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_LIFE_ORB, true);
			}
		}
		if (IsHealthArmorSwapShrine)
		{
			AkSoundEngine.PostEvent("Play_OBJ_shrine_accept_01", base.gameObject);
			player.HealthAndArmorSwapped = !player.HealthAndArmorSwapped;
			if (onPlayerVFX != null)
			{
				player.PlayEffectOnActor(onPlayerVFX, playerVFXOffset);
			}
			if (base.transform.parent != null)
			{
				EncounterTrackable component = base.transform.parent.gameObject.GetComponent<EncounterTrackable>();
				if (component != null)
				{
					if (m_instanceMinimapIcon == null && EncounterNotificationSprite == null)
					{
						RegisterMinimapIcon();
					}
					component.ForceDoNotification(EncounterNotificationSprite ?? m_instanceMinimapIcon.GetComponent<tk2dBaseSprite>());
				}
			}
		}
		else if (IsRNGShrine)
		{
			if (Random.value < 0.001f)
			{
				player.healthHaver.TriggerInvulnerabilityPeriod();
				player.knockbackDoer.ApplyKnockback(player.CenterPosition - base.specRigidbody.UnitCenter, 150f);
				Exploder.DoDefaultExplosion(base.specRigidbody.UnitCenter, Vector2.zero);
				StatModifier statModifier = new StatModifier();
				statModifier.statToBoost = PlayerStats.StatType.Health;
				statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				statModifier.amount = Mathf.Min(0f, -1f * (Mathf.Ceil(player.healthHaver.GetMaxHealth()) - 1f));
				StatModifier statModifier2 = new StatModifier();
				statModifier2.statToBoost = PlayerStats.StatType.Damage;
				statModifier2.modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE;
				statModifier2.amount = 4f;
				StatModifier statModifier3 = new StatModifier();
				statModifier3.statToBoost = PlayerStats.StatType.Curse;
				statModifier3.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				statModifier3.amount = 10f;
				player.ownerlessStatModifiers.Add(statModifier);
				player.ownerlessStatModifiers.Add(statModifier2);
				player.stats.RecalculateStats(player);
				Object.Destroy(base.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_OBJ_shrine_accept_01", base.gameObject);
				ShrineCost randomCost = GetRandomCost();
				ShrineBenefit randomBenefit = GetRandomBenefit();
				if (randomCost.costType == ShrineCost.CostType.HEALTH)
				{
					randomCost.cost = Random.Range(1, 3);
					if ((float)randomCost.cost >= player.healthHaver.GetCurrentHealth())
					{
						randomCost.cost = 1;
					}
				}
				if (randomCost.costType == ShrineCost.CostType.STATS && player.healthHaver.GetMaxHealth() > 2f)
				{
					randomCost.cost = Random.Range(1, 3);
				}
				if (randomCost.costType == ShrineCost.CostType.BLANK)
				{
					randomCost.cost = Random.Range(1, player.Blanks + 1);
				}
				if (randomCost.costType == ShrineCost.CostType.MONEY)
				{
					randomCost.cost = Mathf.FloorToInt((float)player.carriedConsumables.Currency * Random.Range(0.25f, 1f));
				}
				if (randomBenefit.benefitType == ShrineBenefit.BenefitType.MONEY)
				{
					randomBenefit.amount = Random.Range(20, 100);
				}
				if (randomBenefit.benefitType == ShrineBenefit.BenefitType.HEALTH)
				{
					randomBenefit.amount = Mathf.RoundToInt(Random.Range(1f, player.healthHaver.GetMaxHealth()));
				}
				if (randomBenefit.benefitType == ShrineBenefit.BenefitType.STATS)
				{
					if (randomBenefit.statMods[0].statToBoost == PlayerStats.StatType.Health)
					{
						randomBenefit.statMods[0].amount = Random.Range(1, 3);
					}
					else if (randomBenefit.statMods[0].statToBoost == PlayerStats.StatType.MovementSpeed)
					{
						randomBenefit.statMods[0].amount = Random.Range(1.5f, 4f);
					}
					else if (randomBenefit.statMods[0].statToBoost == PlayerStats.StatType.Damage)
					{
						randomBenefit.statMods[0].amount = Random.Range(1.2f, 1.5f);
					}
				}
				if (randomBenefit.benefitType == ShrineBenefit.BenefitType.BLANK)
				{
					randomBenefit.amount = Random.Range(1, 11);
				}
				if (randomBenefit.benefitType == ShrineBenefit.BenefitType.ARMOR)
				{
					randomBenefit.amount = Random.Range(1, 4);
				}
				if (randomBenefit.benefitType == ShrineBenefit.BenefitType.SPAWN_CHEST)
				{
					randomBenefit.IsRNGChest = true;
				}
				string empty = string.Empty;
				if (randomCost.CheckCost(player))
				{
					randomCost.ApplyCost(player);
					empty += StringTableManager.GetItemsString(randomCost.rngString);
				}
				else
				{
					empty += StringTableManager.GetItemsString("#SHRINE_DICE_BAD_FAIL");
				}
				empty += " + ";
				randomBenefit.ApplyBenefit(player);
				empty += StringTableManager.GetItemsString(randomBenefit.rngString);
				if (m_instanceMinimapIcon == null)
				{
					RegisterMinimapIcon();
				}
				if (EncounterNotificationSprite != null)
				{
					GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetItemsString("#SHRINE_DICE_ENCNAME"), empty, EncounterNotificationSprite.Collection, EncounterNotificationSprite.spriteId);
				}
				else
				{
					GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetItemsString("#SHRINE_DICE_ENCNAME"), empty, m_instanceMinimapIcon.GetComponent<tk2dBaseSprite>().Collection, m_instanceMinimapIcon.GetComponent<tk2dBaseSprite>().spriteId);
				}
			}
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.DICE_SHRINES_USED, 1f);
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.DICE_SHRINES_USED) >= 2f && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_FORGE) >= 1f)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.DAISUKE_IS_UNLOCKABLE, true);
			}
		}
		else if (IsLegendaryHeroShrine)
		{
			int totalCurse = PlayerStats.GetTotalCurse();
			int num = ((m_useCount <= 0) ? 5 : 9);
			if (totalCurse >= 5)
			{
				num = 9;
			}
			Debug.LogError("total curse: " + totalCurse + "|" + num + "|" + m_useCount);
			if (totalCurse < num)
			{
				StatModifier statModifier4 = new StatModifier();
				statModifier4.statToBoost = PlayerStats.StatType.Curse;
				statModifier4.amount = num - totalCurse;
				statModifier4.modifyType = StatModifier.ModifyMethod.ADDITIVE;
				player.ownerlessStatModifiers.Add(statModifier4);
				player.stats.RecalculateStats(player);
			}
		}
		else
		{
			if (!CheckAndApplyCosts(player))
			{
				ResetForReuse();
				return;
			}
			AkSoundEngine.PostEvent("Play_OBJ_shrine_accept_01", base.gameObject);
			for (int i = 0; i < Benefits.Count; i++)
			{
				Benefits[i].ApplyBenefit(player);
			}
			if (IncrementMoneyCostEachUse)
			{
				for (int j = 0; j < Costs.Count; j++)
				{
					if (Costs[j].costType == ShrineCost.CostType.MONEY)
					{
						Costs[j].cost = Costs[j].cost + IncrementMoneyCostAmount;
					}
				}
			}
			if (onPlayerVFX != null)
			{
				player.PlayEffectOnActor(onPlayerVFX, playerVFXOffset);
			}
			if (base.transform.parent != null)
			{
				EncounterTrackable component2 = base.transform.parent.gameObject.GetComponent<EncounterTrackable>();
				if (component2 != null)
				{
					if (m_instanceMinimapIcon == null && EncounterNotificationSprite == null)
					{
						RegisterMinimapIcon();
					}
					component2.ForceDoNotification(EncounterNotificationSprite ?? m_instanceMinimapIcon.GetComponent<tk2dBaseSprite>());
				}
			}
		}
		if (!CanBeReused)
		{
			GetRidOfMinimapIcon();
		}
		if (ShattersOnUse)
		{
			ShatterSpriteDisable.renderer.enabled = false;
			ShatterSystem.SetActive(true);
			ShatterSystem.GetComponent<ParticleSystem>().Play();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (base.sprite == null)
		{
			return 100f;
		}
		Vector3 vector = BraveMathCollege.ClosestPointOnRectangle(point, base.specRigidbody.UnitBottomLeft, base.specRigidbody.UnitDimensions);
		if (IsLegendaryHeroShrine && point.y > vector.y + 0.5f)
		{
			return 1000f;
		}
		return Vector2.Distance(point, vector) / 1.5f;
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (AlternativeOutlineTarget != null)
		{
			SpriteOutlineManager.AddOutlineToSprite(AlternativeOutlineTarget, Color.white);
		}
		else
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if (AlternativeOutlineTarget != null)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(AlternativeOutlineTarget);
		}
		else
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		}
	}

	private IEnumerator HandleShrineConversation(PlayerController interactor)
	{
		string targetDisplayKey = displayTextKey;
		if (IsCleanseShrine)
		{
			int totalCurse = PlayerStats.GetTotalCurse();
			targetDisplayKey = ((totalCurse < 3) ? "#SHRINE_CLEANSE_DISPLAY_01" : ((totalCurse < 5) ? "#SHRINE_CLEANSE_DISPLAY_02" : ((totalCurse < 7) ? "#SHRINE_CLEANSE_DISPLAY_03" : ((totalCurse >= 10) ? "#SHRINE_CLEANSE_DISPLAY_05" : "#SHRINE_CLEANSE_DISPLAY_04"))));
		}
		TextBoxManager.ShowStoneTablet(talkPoint.position, talkPoint, -1f, StringTableManager.GetLongString(targetDisplayKey));
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		yield return null;
		bool canUse = (IsHealthArmorSwapShrine && m_totalUseCount == 0) || IsRNGShrine || CheckCosts(interactor);
		if (Costs.Count == 1 && Costs[0].costType == ShrineCost.CostType.CURRENT_GUN && Benefits.Count == 1 && Benefits[0].benefitType == ShrineBenefit.BenefitType.HEALTH && (bool)interactor.healthHaver && interactor.healthHaver.GetCurrentHealthPercentage() == 1f)
		{
			canUse = false;
		}
		if (IsCleanseShrine && PlayerStats.GetTotalCurse() == 0)
		{
			canUse = false;
		}
		if (IsLegendaryHeroShrine && PlayerStats.GetTotalCurse() >= 9)
		{
			canUse = false;
		}
		if (canUse)
		{
			string text = StringTableManager.GetString(acceptOptionKey);
			if (IsCleanseShrine)
			{
				string text2 = text;
				text = text2 + " (" + Costs[0].cost * PlayerStats.GetTotalCurse() + " " + StringTableManager.GetString("#COINS") + ")";
			}
			else if (IncrementMoneyCostEachUse)
			{
				string text2 = text;
				text = text2 + " (" + Costs[0].cost + " " + StringTableManager.GetString("#COINS") + ")";
			}
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, text, StringTableManager.GetString(declineOptionKey));
		}
		else
		{
			GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(declineOptionKey), string.Empty);
		}
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
		if (canUse && selectedResponse == 0)
		{
			DoShrineEffect(interactor);
			m_totalUseCount++;
			if (IsLegendaryHeroShrine && m_totalUseCount >= 2)
			{
				CanBeReused = false;
			}
			if (CanBeReused)
			{
				ResetForReuse();
			}
		}
		else
		{
			ResetForReuse();
		}
	}

	private IEnumerator HandleSpentText(PlayerController interactor)
	{
		TextBoxManager.ShowStoneTablet(talkPoint.position, talkPoint, -1f, StringTableManager.GetLongString(spentOptionKey));
		int selectedResponse = -1;
		interactor.SetInputOverride("shrineConversation");
		GameUIRoot.Instance.DisplayPlayerConversationOptions(interactor, null, StringTableManager.GetString(declineOptionKey), string.Empty);
		while (!GameUIRoot.Instance.GetPlayerConversationResponse(out selectedResponse))
		{
			yield return null;
		}
		interactor.ClearInputOverride("shrineConversation");
		TextBoxManager.ClearTextBox(talkPoint);
	}

	public void Interact(PlayerController interactor)
	{
		if (TextBoxManager.HasTextBox(talkPoint))
		{
			return;
		}
		if (m_useCount > 0 || IsBlankShrine)
		{
			if (!string.IsNullOrEmpty(spentOptionKey))
			{
				StartCoroutine(HandleSpentText(interactor));
			}
		}
		else
		{
			m_useCount++;
			StartCoroutine(HandleShrineConversation(interactor));
		}
	}

	public void OnBlank()
	{
		if (IsBlankShrine && Random.value < m_curChanceToBlankChestIntoExistence)
		{
			m_useCount++;
			m_curChanceToBlankChestIntoExistence = Mathf.Max(0.25f, m_curChanceToBlankChestIntoExistence - 0.45f);
			RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
			IntVector2? randomAvailableCell = absoluteRoom.GetRandomAvailableCell(IntVector2.One * 4, CellTypes.FLOOR);
			IntVector2? intVector = ((!randomAvailableCell.HasValue) ? null : new IntVector2?(randomAvailableCell.GetValueOrDefault() + IntVector2.One));
			if (intVector.HasValue)
			{
				GameManager.Instance.RewardManager.SpawnRoomClearChestAt(intVector.Value);
			}
			else
			{
				GameManager.Instance.RewardManager.SpawnRoomClearChestAt(absoluteRoom.GetBestRewardLocation(new IntVector2(3, 3), RoomHandler.RewardLocationStyle.Original) + IntVector2.Up);
			}
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllAdvancedShrineControllers.Remove(this);
		base.OnDestroy();
	}
}
