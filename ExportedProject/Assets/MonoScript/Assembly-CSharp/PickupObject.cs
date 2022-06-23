using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class PickupObject : BraveBehaviour
{
	public enum ItemQuality
	{
		EXCLUDED = -100,
		SPECIAL = -50,
		COMMON = 0,
		D = 1,
		C = 2,
		B = 3,
		A = 4,
		S = 5
	}

	public static bool RatBeatenAtPunchout;

	[DisableInInspector]
	public int PickupObjectId = -1;

	public ItemQuality quality;

	public float additionalMagnificenceModifier;

	public bool ItemSpansBaseQualityTiers;

	[HideInInspectorIf("ItemSpansBaseQualityTiers", false)]
	public bool ItemRespectsHeartMagnificence;

	public LootModData[] associatedItemChanceMods;

	public ContentSource contentSource;

	public bool ShouldBeExcludedFromShops;

	public bool CanBeDropped = true;

	public bool PreventStartingOwnerFromDropping;

	public bool PersistsOnDeath;

	public bool RespawnsIfPitfall;

	public bool PreventSaveSerialization;

	public bool IgnoredByRat;

	[NonSerialized]
	public bool ClearIgnoredByRatFlagOnPickup;

	[NonSerialized]
	public bool IsBeingSold;

	[LongEnum]
	public GungeonFlags SaveFlagToSetOnAcquisition;

	[SerializeField]
	protected string itemName;

	protected static int s_lastRainbowPickupFrame = -1;

	[NonSerialized]
	public bool HasBeenStatProcessed;

	[HideInInspector]
	public int ForcedPositionInAmmonomicon = -1;

	public bool UsesCustomCost;

	[FormerlySerializedAs("costInStore")]
	public int CustomCost;

	public bool PersistsOnPurchase;

	public bool CanBeSold = true;

	[NonSerialized]
	public bool HasProcessedStatMods;

	protected Color m_alienPickupColor = new Color(1f, 1f, 0f, 1f);

	public static bool ItemIsBeingTakenByRat;

	protected bool m_isBeingEyedByRat;

	protected int m_numberTimesRatTheftAttempted;

	public virtual string DisplayName
	{
		get
		{
			return itemName;
		}
	}

	public string EncounterNameOrDisplayName
	{
		get
		{
			EncounterTrackable component = GetComponent<EncounterTrackable>();
			if ((bool)component)
			{
				return component.GetModifiedDisplayName();
			}
			return itemName;
		}
	}

	public int PurchasePrice
	{
		get
		{
			return (!UsesCustomCost) ? GlobalDungeonData.GetBasePrice(quality) : CustomCost;
		}
	}

	public bool IsBeingEyedByRat
	{
		get
		{
			return m_isBeingEyedByRat;
		}
	}

	public bool CanActuallyBeDropped(PlayerController owner)
	{
		if (!CanBeDropped)
		{
			return false;
		}
		if (this is Gun && owner.CurrentGun == this && owner.inventory.GunLocked.Value)
		{
			return false;
		}
		if ((bool)owner)
		{
			for (int i = 0; i < owner.startingGunIds.Count; i++)
			{
				if (owner.startingGunIds[i] == PickupObjectId)
				{
					return !PreventStartingOwnerFromDropping;
				}
			}
			for (int j = 0; j < owner.startingAlternateGunIds.Count; j++)
			{
				if (owner.startingAlternateGunIds[j] == PickupObjectId)
				{
					return !PreventStartingOwnerFromDropping;
				}
			}
			for (int k = 0; k < owner.startingPassiveItemIds.Count; k++)
			{
				if (owner.startingPassiveItemIds[k] == PickupObjectId)
				{
					return !PreventStartingOwnerFromDropping;
				}
			}
			for (int l = 0; l < owner.startingActiveItemIds.Count; l++)
			{
				if (owner.startingActiveItemIds[l] == PickupObjectId)
				{
					return !PreventStartingOwnerFromDropping;
				}
			}
		}
		return true;
	}

	public bool PrerequisitesMet()
	{
		if (quality == ItemQuality.EXCLUDED)
		{
			return false;
		}
		EncounterTrackable component = GetComponent<EncounterTrackable>();
		if (component == null)
		{
			return true;
		}
		return component.PrerequisitesMet();
	}

	public virtual bool ShouldBeDestroyedOnExistence(bool isForEnemyInventory = false)
	{
		return false;
	}

	protected void HandleEncounterable(PlayerController player)
	{
		EncounterTrackable component = GetComponent<EncounterTrackable>();
		if (component != null)
		{
			component.HandleEncounter();
			if ((bool)this && PickupObjectId == GlobalItemIds.FinishedGun)
			{
				GameStatsManager.Instance.SingleIncrementDifferentiator(PickupObjectDatabase.GetById(GlobalItemIds.UnfinishedGun).encounterTrackable);
			}
		}
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			HandleMagnficence();
		}
		if (SaveFlagToSetOnAcquisition != 0)
		{
			GameStatsManager.Instance.SetFlag(SaveFlagToSetOnAcquisition, true);
		}
	}

	protected void HandleMagnficence()
	{
		GameManager.Instance.PrimaryPlayer.stats.AddFloorMagnificence(additionalMagnificenceModifier);
		if (!ItemRespectsHeartMagnificence)
		{
			switch (quality)
			{
			case ItemQuality.COMMON:
				GameManager.Instance.PrimaryPlayer.stats.AddFloorMagnificence(0f);
				break;
			case ItemQuality.D:
			case ItemQuality.C:
			case ItemQuality.B:
				break;
			case ItemQuality.A:
				GameManager.Instance.PrimaryPlayer.stats.AddFloorMagnificence(1f);
				break;
			case ItemQuality.S:
				GameManager.Instance.PrimaryPlayer.stats.AddFloorMagnificence(1f);
				break;
			}
		}
	}

	protected void HandleLootMods(PlayerController player)
	{
		if (associatedItemChanceMods != null)
		{
			for (int i = 0; i < associatedItemChanceMods.Length; i++)
			{
				player.lootModData.Add(associatedItemChanceMods[i]);
			}
		}
	}

	public abstract void Pickup(PlayerController player);

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected bool ShouldBeTakenByRat(Vector2 point)
	{
		if (GameManager.Instance.IsLoadingLevel || Dungeon.IsGenerating)
		{
			return false;
		}
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		if (IgnoredByRat)
		{
			return false;
		}
		if (this is NotePassiveItem)
		{
			return false;
		}
		if (this is AmmoPickup && base.transform.position.GetAbsoluteRoom().IsSecretRoom)
		{
			return false;
		}
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.RATGEON)
		{
			return false;
		}
		if (RatBeatenAtPunchout && !PassiveItem.IsFlagSetAtAll(typeof(RingOfResourcefulRatItem)))
		{
			return false;
		}
		if (base.transform.position == Vector3.zero)
		{
			return false;
		}
		if (ItemIsBeingTakenByRat)
		{
			return false;
		}
		if (GameManager.Instance.AllPlayers != null)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].PlayerIsRatTransformed)
				{
					return false;
				}
				if (Vector2.Distance(point, GameManager.Instance.AllPlayers[i].CenterPosition) < 10f)
				{
					return false;
				}
			}
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
		{
			return false;
		}
		if ((GameManager.Instance.PrimaryPlayer == null || GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) && (GameManager.Instance.SecondaryPlayer == null || GameManager.Instance.SecondaryPlayer.healthHaver.IsDead))
		{
			return false;
		}
		if ((bool)base.encounterTrackable && base.encounterTrackable.UsesPurpleNotifications)
		{
			return false;
		}
		if (this is SilencerItem)
		{
			return false;
		}
		if (this is RobotUnlockTelevisionItem || m_numberTimesRatTheftAttempted == 0)
		{
			RoomHandler currentRoom = GameManager.Instance.GetPlayerClosestToPoint(point).CurrentRoom;
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
			return currentRoom != absoluteRoomFromPosition;
		}
		RoomHandler currentRoom2 = GameManager.Instance.GetPlayerClosestToPoint(point).CurrentRoom;
		RoomHandler absoluteRoomFromPosition2 = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		if (currentRoom2 == absoluteRoomFromPosition2)
		{
			return false;
		}
		if (currentRoom2.connectedRooms.Contains(absoluteRoomFromPosition2))
		{
			return false;
		}
		return true;
	}

	protected IEnumerator HandleRatTheft()
	{
		Debug.Log("starting grabby..." + base.name);
		m_isBeingEyedByRat = true;
		ItemIsBeingTakenByRat = true;
		m_numberTimesRatTheftAttempted++;
		float elapsed = 0f;
		float duration = 2f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if (!this || !m_isBeingEyedByRat)
		{
			ItemIsBeingTakenByRat = false;
			yield break;
		}
		Debug.Log("doing grabby...");
		GameObject ratInstance = UnityEngine.Object.Instantiate(position: (base.sprite.WorldCenter + Vector2.right / -2f).ToVector3ZUp(), original: PrefabDatabase.Instance.ResourcefulRatThief, rotation: Quaternion.identity);
		ThievingRatGrabby grabby = null;
		PlayMakerFSM fsm = ratInstance.GetComponent<PlayMakerFSM>();
		for (int i = 0; i < fsm.FsmStates.Length; i++)
		{
			for (int j = 0; j < fsm.FsmStates[i].Actions.Length; j++)
			{
				if (fsm.FsmStates[i].Actions[j] is ThievingRatGrabby)
				{
					grabby = fsm.FsmStates[i].Actions[j] as ThievingRatGrabby;
				}
			}
		}
		if (grabby != null)
		{
			grabby.TargetObject = this;
		}
		while ((bool)ratInstance)
		{
			yield return null;
		}
		ItemIsBeingTakenByRat = false;
		m_isBeingEyedByRat = false;
	}

	public static void HandlePickupCurseParticles(tk2dBaseSprite targetSprite, float zOffset = 0f)
	{
		if ((bool)targetSprite)
		{
			Vector3 vector = targetSprite.WorldBottomLeft.ToVector3ZisY(zOffset);
			Vector3 vector2 = targetSprite.WorldTopRight.ToVector3ZisY(zOffset);
			float num = (vector2.y - vector.y) * (vector2.x - vector.x);
			float num2 = 25f * num;
			int num3 = Mathf.CeilToInt(Mathf.Max(1f, num2 * BraveTime.DeltaTime));
			int num4 = num3;
			Vector3 minPosition = vector;
			Vector3 maxPosition = vector2;
			Vector3 direction = Vector3.up / 2f;
			float angleVariance = 120f;
			float magnitudeVariance = 0.2f;
			float? startLifetime = UnityEngine.Random.Range(0.8f, 1.25f);
			GlobalSparksDoer.DoRandomParticleBurst(num4, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
		}
	}

	protected void HandlePickupCurseParticles()
	{
		if (!this || !base.sprite)
		{
			return;
		}
		bool flag = false;
		if (this is Gun)
		{
			Gun gun = this as Gun;
			for (int i = 0; i < gun.passiveStatModifiers.Length; i++)
			{
				if (gun.passiveStatModifiers[i].statToBoost == PlayerStats.StatType.Curse && gun.passiveStatModifiers[i].amount > 0f)
				{
					flag = true;
					break;
				}
			}
		}
		else if (this is PlayerItem)
		{
			PlayerItem playerItem = this as PlayerItem;
			for (int j = 0; j < playerItem.passiveStatModifiers.Length; j++)
			{
				if (playerItem.passiveStatModifiers[j].statToBoost == PlayerStats.StatType.Curse && playerItem.passiveStatModifiers[j].amount > 0f)
				{
					flag = true;
					break;
				}
			}
		}
		else if (this is PassiveItem)
		{
			PassiveItem passiveItem = this as PassiveItem;
			for (int k = 0; k < passiveItem.passiveStatModifiers.Length; k++)
			{
				if (passiveItem.passiveStatModifiers[k].statToBoost == PlayerStats.StatType.Curse && passiveItem.passiveStatModifiers[k].amount > 0f)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			HandlePickupCurseParticles(base.sprite);
		}
	}

	protected void OnSharedPickup()
	{
		if (IgnoredByRat && ClearIgnoredByRatFlagOnPickup)
		{
			IgnoredByRat = false;
		}
	}

	public virtual void MidGameSerialize(List<object> data)
	{
	}

	public virtual void MidGameDeserialize(List<object> data)
	{
	}
}
