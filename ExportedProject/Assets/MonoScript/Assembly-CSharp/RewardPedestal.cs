using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class RewardPedestal : DungeonPlaceableBehaviour, IPlayerInteractable
{
	[NonSerialized]
	public PickupObject contents;

	public bool UsesSpecificItem;

	[PickupIdentifier]
	[ShowInInspectorIf("UsesSpecificItem", true)]
	public int SpecificItemId = -1;

	[HideInInspectorIf("UsesSpecificItem", false)]
	public LootData lootTable;

	public bool UsesDelayedConfiguration;

	public Transform spawnTransform;

	[FormerlySerializedAs("spawnsHearts")]
	public bool SpawnsTertiarySet = true;

	[CheckAnimation(null)]
	public string spawnAnimName;

	public GameObject VFX_PreSpawn;

	public GameObject VFX_GroundHit;

	public float groundHitDelay = 0.73f;

	[NonSerialized]
	public bool pickedUp;

	[NonSerialized]
	public bool ReturnCoopPlayerOnLand;

	[NonSerialized]
	public bool IsBossRewardPedestal;

	private GameObject minimapIconInstance;

	private RoomHandler m_room;

	private RoomHandler m_registeredIconRoom;

	private tk2dBaseSprite m_itemDisplaySprite;

	private bool m_isMimic;

	private bool m_isMimicBreathing;

	[Header("Mimic")]
	[EnemyIdentifier]
	public string MimicGuid;

	public IntVector2 mimicOffset;

	[CheckAnimation(null)]
	public string preMimicIdleAnim;

	public float preMimicIdleAnimDelay = 3f;

	public float overrideMimicChance = -1f;

	private const float SPAWN_PUSH_RADIUS = 5f;

	private const float SPAWN_PUSH_FORCE = 22f;

	public bool OffsetTertiarySet { get; set; }

	public bool IsMimic
	{
		get
		{
			return m_isMimic;
		}
	}

	private void Awake()
	{
		if ((bool)base.majorBreakable)
		{
			base.majorBreakable.TemporarilyInvulnerable = true;
		}
	}

	private void Start()
	{
		if (UsesSpecificItem)
		{
			m_room = GetAbsoluteParentRoom();
			HandleSpawnBehavior();
		}
	}

	private void OnEnable()
	{
		if (m_isMimic && !m_isMimicBreathing)
		{
			StartCoroutine(MimicIdleAnimCR());
		}
	}

	public static RewardPedestal Spawn(RewardPedestal pedestalPrefab, IntVector2 basePosition)
	{
		RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(basePosition);
		return Spawn(pedestalPrefab, basePosition, roomFromPosition);
	}

	public static RewardPedestal Spawn(RewardPedestal pedestalPrefab, IntVector2 basePosition, RoomHandler room)
	{
		if (pedestalPrefab == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(pedestalPrefab.gameObject, basePosition.ToVector3() + new Vector3(0.1875f, 0f, 0f), Quaternion.identity);
		RewardPedestal component = gameObject.GetComponent<RewardPedestal>();
		component.m_room = room;
		component.HandleSpawnBehavior();
		return component;
	}

	public void RegisterChestOnMinimap(RoomHandler r)
	{
		if (!GameStatsManager.HasInstance || !GameStatsManager.Instance.IsRainbowRun)
		{
			m_registeredIconRoom = r;
			GameObject iconPrefab = BraveResources.Load("Global Prefabs/Minimap_Treasure_Icon") as GameObject;
			minimapIconInstance = Minimap.Instance.RegisterRoomIcon(r, iconPrefab);
		}
	}

	public void GiveCoopPartnerBack()
	{
		PlayerController playerController = ((!GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) ? GameManager.Instance.SecondaryPlayer : GameManager.Instance.PrimaryPlayer);
		playerController.specRigidbody.enabled = true;
		playerController.gameObject.SetActive(true);
		playerController.ResurrectFromBossKill();
	}

	private void HandleSpawnBehavior()
	{
		GameManager.Instance.Dungeon.StartCoroutine(SpawnBehavior_CR());
	}

	private IEnumerator SpawnBehavior_CR()
	{
		if (VFX_PreSpawn != null)
		{
			base.renderer.enabled = false;
			VFX_PreSpawn.SetActive(true);
			yield return new WaitForSeconds(0.1f);
			base.renderer.enabled = true;
		}
		if (!string.IsNullOrEmpty(spawnAnimName))
		{
			tk2dSpriteAnimationClip clip = base.spriteAnimator.GetClipByName(spawnAnimName);
			if (clip != null)
			{
				base.specRigidbody.enabled = false;
				float clipTime = (float)clip.frames.Length / clip.fps;
				base.spriteAnimator.Play(clip);
				base.sprite.UpdateZDepth();
				float elapsed = 0f;
				bool groundHitTriggered = false;
				while (elapsed < clipTime)
				{
					elapsed += BraveTime.DeltaTime;
					if (elapsed >= groundHitDelay && !groundHitTriggered)
					{
						Exploder.DoRadialPush(base.sprite.WorldCenter.ToVector3ZUp(base.sprite.WorldCenter.y), 22f, 5f);
						groundHitTriggered = true;
						VFX_GroundHit.SetActive(true);
					}
					yield return null;
				}
			}
		}
		base.sprite.UpdateZDepth();
		m_room.RegisterInteractable(this);
		base.specRigidbody.enabled = true;
		List<CollisionData> overlappingRigidbodies = new List<CollisionData>();
		PhysicsEngine.Instance.OverlapCast(base.specRigidbody, overlappingRigidbodies, false, true, null, null, false, null, null);
		for (int i = 0; i < overlappingRigidbodies.Count; i++)
		{
			SpeculativeRigidbody otherRigidbody = overlappingRigidbodies[i].OtherRigidbody;
			if ((bool)otherRigidbody)
			{
				if ((bool)otherRigidbody.minorBreakable)
				{
					otherRigidbody.minorBreakable.Break(otherRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
				}
				if ((bool)otherRigidbody.majorBreakable)
				{
					otherRigidbody.majorBreakable.Break(otherRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
				}
				MeduziDeathController component = otherRigidbody.GetComponent<MeduziDeathController>();
				if ((bool)component)
				{
					component.Shatter();
				}
			}
		}
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		if (ReturnCoopPlayerOnLand)
		{
			GiveCoopPartnerBack();
		}
		if (SpawnsTertiarySet)
		{
			TertiaryBossRewardSet tertiaryRewardSet = GameManager.Instance.Dungeon.GetTertiaryRewardSet();
			for (int j = 0; j < tertiaryRewardSet.dropIds.Count; j++)
			{
				Vector2 vector = base.sprite.WorldCenter + new Vector2(-3f, 0f);
				if (j == 1)
				{
					vector = base.sprite.WorldCenter + new Vector2(2f, 0f);
				}
				if (j == 2)
				{
					vector = base.sprite.WorldCenter + new Vector2(0f, -3f);
				}
				if (OffsetTertiarySet)
				{
					vector += Vector2.right;
				}
				if (tertiaryRewardSet.dropIds.Count > 3)
				{
					vector = base.sprite.WorldCenter + (Quaternion.Euler(0f, 0f, 360f / (float)tertiaryRewardSet.dropIds.Count * (float)j) * new Vector2(3f, 0f)).XY();
				}
				if (GameManager.Instance.Dungeon.IsGlitchDungeon)
				{
					IntVector2? randomAvailableCell = base.transform.position.GetAbsoluteRoom().GetRandomAvailableCell(new IntVector2(2, 2), CellTypes.FLOOR);
					if (randomAvailableCell.HasValue)
					{
						vector = randomAvailableCell.Value.ToCenterVector2();
					}
				}
				PickupObject byId = PickupObjectDatabase.GetById(tertiaryRewardSet.dropIds[j]);
				LootEngine.SpawnItem(byId.gameObject, vector.ToVector3ZUp(), Vector2.zero, 0f);
			}
		}
		DetermineContents(GameManager.Instance.PrimaryPlayer);
		MaybeBecomeMimic();
		if ((bool)base.majorBreakable)
		{
			base.majorBreakable.TemporarilyInvulnerable = false;
			MajorBreakable obj = base.majorBreakable;
			obj.OnDamaged = (Action<float>)Delegate.Combine(obj.OnDamaged, new Action<float>(OnDamaged));
		}
	}

	protected void DetermineContents(PlayerController player)
	{
		if (contents == null)
		{
			if (IsBossRewardPedestal)
			{
				if (GameStatsManager.Instance.IsRainbowRun)
				{
					LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteBoss, base.sprite.WorldCenter + new Vector2(-0.5f, -3f), GetAbsoluteParentRoom());
					return;
				}
				if (lootTable.lootTable != null)
				{
					contents = lootTable.lootTable.SelectByWeightWithoutDuplicatesFullPrereqs(null).GetComponent<PickupObject>();
				}
				else
				{
					if (GameManager.Instance.IsSeeded)
					{
						FloorRewardManifest seededManifestForCurrentFloor = GameManager.Instance.RewardManager.GetSeededManifestForCurrentFloor();
						if (seededManifestForCurrentFloor != null)
						{
							contents = seededManifestForCurrentFloor.GetNextBossReward(GameManager.Instance.RewardManager.IsBossRewardForcedGun());
						}
					}
					if (contents == null)
					{
						contents = GameManager.Instance.RewardManager.GetRewardObjectBossStyle(player).GetComponent<PickupObject>();
					}
				}
			}
			else if (UsesSpecificItem)
			{
				contents = PickupObjectDatabase.GetById(SpecificItemId);
			}
			else if (lootTable.lootTable == null)
			{
				contents = GameManager.Instance.Dungeon.baseChestContents.SelectByWeight().GetComponent<PickupObject>();
			}
			else if (lootTable != null)
			{
				contents = lootTable.GetSingleItemForPlayer(player);
				if (!(contents == null))
				{
				}
			}
		}
		if (m_itemDisplaySprite == null)
		{
			GameObject gameObject = new GameObject("Display Sprite");
			gameObject.transform.parent = spawnTransform;
			m_itemDisplaySprite = tk2dSprite.AddComponent(gameObject, contents.sprite.Collection, contents.sprite.spriteId);
			SpriteOutlineManager.AddOutlineToSprite(m_itemDisplaySprite, Color.black, 0.1f, 0.05f);
			base.sprite.AttachRenderer(m_itemDisplaySprite);
			m_itemDisplaySprite.HeightOffGround = 0.25f;
			m_itemDisplaySprite.depthUsesTrimmedBounds = true;
			m_itemDisplaySprite.PlaceAtPositionByAnchor(spawnTransform.position, tk2dBaseSprite.Anchor.LowerCenter);
			m_itemDisplaySprite.transform.position = m_itemDisplaySprite.transform.position.Quantize(0.0625f);
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
			tk2dBaseSprite component = gameObject2.GetComponent<tk2dBaseSprite>();
			component.PlaceAtPositionByAnchor(m_itemDisplaySprite.WorldCenter.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
			component.HeightOffGround = 5f;
			component.UpdateZDepth();
			base.sprite.UpdateZDepth();
		}
	}

	private void DoPickup(PlayerController player)
	{
		if (pickedUp)
		{
			return;
		}
		pickedUp = true;
		if (IsMimic && contents != null)
		{
			DoMimicTransformation(contents);
			return;
		}
		if (contents != null)
		{
			LootEngine.GivePrefabToPlayer(contents.gameObject, player);
			if (contents is Gun)
			{
				AkSoundEngine.PostEvent("Play_OBJ_weapon_pickup_01", base.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_OBJ_item_pickup_01", base.gameObject);
			}
			GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Pickup");
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.PlaceAtPositionByAnchor(m_itemDisplaySprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			component.HeightOffGround = 6f;
			component.UpdateZDepth();
			UnityEngine.Object.Destroy(m_itemDisplaySprite);
		}
		if (m_registeredIconRoom != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_registeredIconRoom, minimapIconInstance);
		}
	}

	private void DoMimicTransformation(PickupObject overrideDeathRewards)
	{
		if ((bool)m_itemDisplaySprite)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			component.PlaceAtPositionByAnchor(m_itemDisplaySprite.WorldCenter.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
			component.HeightOffGround = 5f;
			component.UpdateZDepth();
		}
		base.sprite.UpdateZDepth();
		IntVector2 intVector = base.specRigidbody.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = base.specRigidbody.UnitTopRight.ToIntVector2(VectorConversions.Floor);
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				GameManager.Instance.Dungeon.data[new IntVector2(i, j)].isOccupied = false;
			}
		}
		if (!pickedUp)
		{
			pickedUp = true;
			m_room.DeregisterInteractable(this);
		}
		if (m_registeredIconRoom != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_registeredIconRoom, minimapIconInstance);
		}
		AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(MimicGuid);
		AIActor aIActor = AIActor.Spawn(orLoadByGuid, base.transform.position.XY().ToIntVector2(VectorConversions.Floor), GetAbsoluteParentRoom());
		if (overrideDeathRewards != null)
		{
			aIActor.AdditionalSafeItemDrops.Add(overrideDeathRewards);
		}
		PickupObject.ItemQuality itemQuality = (BraveUtility.RandomBool() ? PickupObject.ItemQuality.D : PickupObject.ItemQuality.C);
		GenericLootTable genericLootTable = ((!BraveUtility.RandomBool()) ? GameManager.Instance.RewardManager.GunsLootTable : GameManager.Instance.RewardManager.ItemsLootTable);
		PickupObject itemOfTypeAndQuality = LootEngine.GetItemOfTypeAndQuality<PickupObject>(itemQuality, genericLootTable);
		if ((bool)itemOfTypeAndQuality)
		{
			aIActor.AdditionalSafeItemDrops.Add(itemOfTypeAndQuality);
		}
		aIActor.specRigidbody.Initialize();
		Vector2 unitBottomLeft = aIActor.specRigidbody.UnitBottomLeft;
		Vector2 unitBottomLeft2 = base.specRigidbody.UnitBottomLeft;
		aIActor.transform.position -= (Vector3)(unitBottomLeft - unitBottomLeft2);
		aIActor.transform.position += (Vector3)PhysicsEngine.PixelToUnit(mimicOffset);
		aIActor.specRigidbody.Reinitialize();
		aIActor.HasDonePlayerEnterCheck = true;
		GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_HAS_BEEN_PEDESTAL_MIMICKED, true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void MaybeBecomeMimic()
	{
		m_isMimic = false;
		bool flag = false;
		if (string.IsNullOrEmpty(MimicGuid))
		{
			return;
		}
		flag |= GameManager.Instance.Dungeon.sharedSettingsPrefab.RandomShouldBecomePedestalMimic(overrideMimicChance);
		GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
		flag |= lastLoadedLevelDefinition != null && lastLoadedLevelDefinition.lastSelectedFlowEntry != null && lastLoadedLevelDefinition.lastSelectedFlowEntry.levelMode == FlowLevelEntryMode.ALL_MIMICS;
		if (m_room != null)
		{
			string roomName = m_room.GetRoomName();
			if (roomName.StartsWith("DemonWallRoom"))
			{
				flag = false;
			}
			if (roomName.StartsWith("DoubleBeholsterRoom"))
			{
				flag = BraveUtility.RandomBool();
			}
		}
		if (flag && !PassiveItem.IsFlagSetAtAll(typeof(MimicRingItem)))
		{
			m_isMimic = true;
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(MimicIdleAnimCR());
			}
		}
	}

	private IEnumerator MimicIdleAnimCR()
	{
		m_isMimicBreathing = true;
		tk2dSpriteAnimationClip clip = base.spriteAnimator.GetClipByName(preMimicIdleAnim);
		tk2dSpriteAnimationFrame finalFrame = clip.GetFrame(clip.frames.Length - 1);
		base.spriteAnimator.sprite.SetSprite(finalFrame.spriteCollection, finalFrame.spriteId);
		while (m_isMimic)
		{
			yield return new WaitForSeconds(preMimicIdleAnimDelay);
			if (!m_isMimic)
			{
				yield break;
			}
			base.spriteAnimator.Play(preMimicIdleAnim);
			while (base.spriteAnimator.IsPlaying(preMimicIdleAnim))
			{
				if (!m_isMimic)
				{
					yield break;
				}
				yield return null;
			}
		}
		m_isMimicBreathing = false;
	}

	private void OnDamaged(float damage)
	{
		if (m_isMimic)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.PREFIRE_ON_MIMIC);
			DoMimicTransformation(contents);
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			if (m_itemDisplaySprite != null)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(m_itemDisplaySprite, true);
				SpriteOutlineManager.AddOutlineToSprite(m_itemDisplaySprite, Color.white, 0.1f);
			}
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			if (m_itemDisplaySprite != null)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(m_itemDisplaySprite, true);
				SpriteOutlineManager.AddOutlineToSprite(m_itemDisplaySprite, Color.black, 0.1f, 0.05f);
			}
			base.sprite.UpdateZDepth();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void Interact(PlayerController player)
	{
		if (!pickedUp)
		{
			m_room.DeregisterInteractable(this);
			if (m_itemDisplaySprite != null)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(m_itemDisplaySprite);
			}
			DoPickup(player);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public void ForceConfiguration()
	{
		DetermineContents(GameManager.Instance.PrimaryPlayer);
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		RegisterChestOnMinimap(room);
		if (!UsesDelayedConfiguration)
		{
			ForceConfiguration();
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)base.majorBreakable)
		{
			MajorBreakable obj = base.majorBreakable;
			obj.OnDamaged = (Action<float>)Delegate.Remove(obj.OnDamaged, new Action<float>(OnDamaged));
		}
		base.OnDestroy();
	}
}
