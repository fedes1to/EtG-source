using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunberMuncherController : BraveBehaviour
{
	public int RequiredNumberOfGuns = 2;

	public GenericLootTable LootTable;

	public List<GunberMuncherRecipe> DefinedRecipes;

	public AnimationCurve QualityDistribution;

	public bool IsProcessing;

	public bool CanBeReused;

	[PickupIdentifier]
	public int evilMuncherReward;

	public float evilMuncherPostRewardChance = 0.07f;

	public GameObject PoopSteamPrefab;

	[NonSerialized]
	private Gun m_first;

	[NonSerialized]
	private Gun m_second;

	[NonSerialized]
	private int m_gunsTossed;

	public bool ShouldGiveReward { get; set; }

	private void Start()
	{
		if (RequiredNumberOfGuns > 2)
		{
			m_gunsTossed = (int)GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.GUNBERS_EVIL_MUNCHED);
		}
		Minimap.Instance.RegisterRoomIcon(base.transform.position.GetAbsoluteRoom(), (GameObject)ResourceCache.Acquire("Global Prefabs/Minimap_Muncher_Icon"));
	}

	public IEnumerator DoReward(PlayerController player)
	{
		yield return new WaitForSeconds(0.6f);
		if ((m_first != null && m_second != null) || m_gunsTossed >= RequiredNumberOfGuns)
		{
			GameObject itemForPlayer = GetItemForPlayer(player);
			tk2dBaseSprite component = itemForPlayer.GetComponent<tk2dBaseSprite>();
			Vector2 vector = Vector2.zero;
			if (component != null)
			{
				vector = -1f * component.GetBounds().center.XY();
			}
			DebrisObject debrisObject = LootEngine.SpawnItem(itemForPlayer, base.sprite.WorldCenter + vector, Vector2.down, 20f);
			debrisObject.bounceCount = 0;
			debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, new Action<DebrisObject>(DoSteamOnGrounded));
		}
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.GUNBERS_MUNCHED, 1f);
		if (RequiredNumberOfGuns > 2)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.MUNCHER_EVIL_COMPLETE, true);
		}
		yield return new WaitForSeconds(2.4f);
		if (!CanBeReused)
		{
			PlayMakerFSM component2 = GetComponent<PlayMakerFSM>();
			component2.FsmVariables.FindFsmBool("canBeUsed").Value = false;
		}
		m_first = null;
		m_second = null;
	}

	private void DoSteamOnGrounded(DebrisObject obj)
	{
		SpawnManager.SpawnVFX(PoopSteamPrefab, obj.sprite.WorldCenter.ToVector3ZUp(obj.sprite.WorldCenter.y - 1f), Quaternion.identity);
	}

	protected GameObject GetRecipeItem()
	{
		PickupObject pickupObject = null;
		for (int i = 0; i < DefinedRecipes.Count; i++)
		{
			if (DefinedRecipes[i].gunIDs_A.Contains(m_first.PickupObjectId) && DefinedRecipes[i].gunIDs_B.Contains(m_second.PickupObjectId))
			{
				pickupObject = PickupObjectDatabase.GetById(DefinedRecipes[i].resultID);
				if (pickupObject != null && DefinedRecipes[i].flagToSet != 0)
				{
					GameStatsManager.Instance.SetFlag(DefinedRecipes[i].flagToSet, true);
				}
				break;
			}
			if (DefinedRecipes[i].gunIDs_A.Contains(m_second.PickupObjectId) && DefinedRecipes[i].gunIDs_B.Contains(m_first.PickupObjectId))
			{
				pickupObject = PickupObjectDatabase.GetById(DefinedRecipes[i].resultID);
				if (pickupObject != null && DefinedRecipes[i].flagToSet != 0)
				{
					GameStatsManager.Instance.SetFlag(DefinedRecipes[i].flagToSet, true);
				}
				break;
			}
		}
		return (!(pickupObject != null)) ? null : pickupObject.gameObject;
	}

	protected GameObject GetItemForPlayer(PlayerController player)
	{
		if (RequiredNumberOfGuns > 2 && !GameStatsManager.Instance.GetFlag(GungeonFlags.MUNCHER_EVIL_COMPLETE))
		{
			return PickupObjectDatabase.GetById(evilMuncherReward).gameObject;
		}
		if (m_first != null && m_second != null)
		{
			GameObject recipeItem = GetRecipeItem();
			if (recipeItem != null)
			{
				return recipeItem;
			}
		}
		PickupObject.ItemQuality b = DetermineQualityToSpawn();
		b = (PickupObject.ItemQuality)Mathf.Min(5, Mathf.Max(0, (int)b));
		bool flag = false;
		while (b >= PickupObject.ItemQuality.COMMON)
		{
			if (b > PickupObject.ItemQuality.COMMON)
			{
				flag = true;
			}
			List<WeightedGameObject> compiledRawItems = LootTable.GetCompiledRawItems();
			List<KeyValuePair<WeightedGameObject, float>> list = new List<KeyValuePair<WeightedGameObject, float>>();
			float num = 0f;
			List<KeyValuePair<WeightedGameObject, float>> list2 = new List<KeyValuePair<WeightedGameObject, float>>();
			float num2 = 0f;
			for (int i = 0; i < compiledRawItems.Count; i++)
			{
				if (!(compiledRawItems[i].gameObject != null))
				{
					continue;
				}
				PickupObject component = compiledRawItems[i].gameObject.GetComponent<PickupObject>();
				bool flag2 = component.quality == b;
				if (!(component != null) || !flag2)
				{
					continue;
				}
				bool flag3 = true;
				float weight = compiledRawItems[i].weight;
				if (!component.PrerequisitesMet())
				{
					flag3 = false;
				}
				if (!component.CanActuallyBeDropped(player))
				{
					flag3 = false;
				}
				EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
				if (component2 != null)
				{
					int num3 = 0;
					if (Application.isPlaying)
					{
						num3 = GameStatsManager.Instance.QueryEncounterableDifferentiator(component2);
					}
					if (num3 > 0 || (Application.isPlaying && GameManager.Instance.ExtantShopTrackableGuids.Contains(component2.EncounterGuid)))
					{
						flag3 = false;
						num2 += weight;
						KeyValuePair<WeightedGameObject, float> item = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], weight);
						list2.Add(item);
					}
				}
				if (flag3)
				{
					num += weight;
					KeyValuePair<WeightedGameObject, float> item2 = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], weight);
					list.Add(item2);
				}
			}
			if (list.Count == 0 && list2.Count > 0)
			{
				list = list2;
				num = num2;
			}
			if (num > 0f && list.Count > 0)
			{
				float num4 = num * UnityEngine.Random.value;
				for (int j = 0; j < list.Count; j++)
				{
					num4 -= list[j].Value;
					if (num4 <= 0f)
					{
						return list[j].Key.gameObject;
					}
				}
				return list[list.Count - 1].Key.gameObject;
			}
			b--;
			if (b < PickupObject.ItemQuality.COMMON && !flag)
			{
				b = PickupObject.ItemQuality.D;
			}
		}
		Debug.LogError("Failed to get any item at all.");
		return null;
	}

	private PickupObject.ItemQuality DetermineQualityToSpawn()
	{
		if (m_first == null && m_gunsTossed >= RequiredNumberOfGuns)
		{
			return (!(UnityEngine.Random.value > 0.25f)) ? PickupObject.ItemQuality.S : PickupObject.ItemQuality.A;
		}
		if (m_first == null || m_second == null)
		{
			Debug.LogError("Problem of type 2 in Gunber Muncher!");
			if (m_first != null)
			{
				return m_first.quality;
			}
			if (m_second != null)
			{
				return m_second.quality;
			}
			return PickupObject.ItemQuality.C;
		}
		int quality = (int)m_first.quality;
		int quality2 = (int)m_second.quality;
		int num = Mathf.Min(quality, quality2);
		int num2 = Mathf.Max(quality, quality2);
		bool flag = num2 < 5;
		int num3 = num2 - num + 1;
		float num4 = 1f / (float)num3;
		float value = UnityEngine.Random.value;
		float num5 = 0f;
		int num6 = -1;
		for (int i = num; i <= num2; i++)
		{
			num5 += num4;
			float num7 = QualityDistribution.Evaluate(num5);
			if (num7 > value)
			{
				num6 = i;
				break;
			}
		}
		if (num6 == -1)
		{
			num6 = num2;
		}
		if (flag && UnityEngine.Random.value > 0.95f)
		{
			num6 = Mathf.Min(num6 + 1, 5);
		}
		return (PickupObject.ItemQuality)num6;
	}

	public void TossPlayerEquippedGun(PlayerController player)
	{
		if (!(player.CurrentGun != null) || !player.CurrentGun.CanActuallyBeDropped(player))
		{
			return;
		}
		Gun currentGun = player.CurrentGun;
		if (RequiredNumberOfGuns == 2)
		{
			if (m_first == null)
			{
				m_first = PickupObjectDatabase.Instance.InternalGetById(currentGun.PickupObjectId) as Gun;
			}
			else if (m_second == null)
			{
				m_second = PickupObjectDatabase.Instance.InternalGetById(currentGun.PickupObjectId) as Gun;
			}
			else
			{
				Debug.LogError("GUNBER MUNCHER FAIL TYPE 1");
			}
		}
		else
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.GUNBERS_EVIL_MUNCHED, 1f);
			m_gunsTossed++;
		}
		TossObjectIntoPot(player, currentGun.GetSprite(), player.CenterPosition);
		player.inventory.DestroyCurrentGun();
	}

	public void TossObjectIntoPot(PlayerController player, tk2dBaseSprite spriteSource, Vector3 startPosition)
	{
		StartCoroutine(HandleObjectPotToss(player, spriteSource, startPosition));
	}

	private IEnumerator HandleObjectPotToss(PlayerController player, tk2dBaseSprite spriteSource, Vector3 startPosition)
	{
		IsProcessing = true;
		ShouldGiveReward = false;
		base.aiAnimator.PlayUntilFinished("activate");
		yield return new WaitForSeconds(0.4f);
		GameObject fakeObject = new GameObject("cauldron temp object");
		tk2dSprite sprite = tk2dBaseSprite.AddComponent<tk2dSprite>(fakeObject, spriteSource.Collection, spriteSource.spriteId);
		sprite.HeightOffGround = 2f;
		sprite.PlaceAtPositionByAnchor(startPosition, tk2dBaseSprite.Anchor.MiddleCenter);
		Vector3 endPosition = base.sprite.WorldCenter.ToVector3ZUp();
		float duration = 0.5f;
		float elapsed = 0f;
		while (elapsed < duration && base.spriteAnimator.CurrentFrame != 8)
		{
			elapsed += BraveTime.DeltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			Vector3 targetPosition = Vector3.Lerp(startPosition, endPosition, t);
			sprite.PlaceAtPositionByAnchor(targetPosition, tk2dBaseSprite.Anchor.MiddleCenter);
			sprite.UpdateZDepth();
			yield return null;
		}
		UnityEngine.Object.Destroy(fakeObject);
		yield return new WaitForSeconds(0.25f);
		if (RequiredNumberOfGuns > 2)
		{
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.MUNCHER_EVIL_COMPLETE))
			{
				float value = UnityEngine.Random.value;
				Debug.Log("evil chance! " + value);
				if (value < evilMuncherPostRewardChance)
				{
					ShouldGiveReward = true;
				}
			}
			else if ((m_first != null && m_second != null) || m_gunsTossed >= RequiredNumberOfGuns)
			{
				ShouldGiveReward = true;
			}
		}
		else if (m_first != null && m_second != null)
		{
			ShouldGiveReward = true;
		}
		if (ShouldGiveReward)
		{
			base.talkDoer.IsInteractable = false;
			yield return new WaitForSeconds(4.75f);
			IsProcessing = false;
			yield return StartCoroutine(DoReward(player));
			base.talkDoer.IsInteractable = true;
		}
		else
		{
			base.aiAnimator.PlayUntilCancelled("idle");
			IsProcessing = false;
		}
	}
}
