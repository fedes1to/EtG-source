using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class Chest : DungeonPlaceableBehaviour, IPlaceConfigurable, IPlayerInteractable
{
	public enum GeneralChestType
	{
		UNSPECIFIED,
		WEAPON,
		ITEM
	}

	public enum SpecialChestIdentifier
	{
		NORMAL,
		RAT,
		SECRET_RAINBOW
	}

	public static bool HasDroppedResourcefulRatNoteThisSession;

	public static bool DoneResourcefulRatMimicThisSession;

	public static bool HasDroppedSerJunkanThisSession;

	protected const float MULTI_ITEM_SPREAD_FACTOR = 1f;

	protected const float ITEM_PRESENTATION_SPEED = 1.5f;

	public SpecialChestIdentifier ChestIdentifier;

	private GeneralChestType m_chestType;

	[NonSerialized]
	public List<PickupObject> contents;

	[PickupIdentifier]
	public List<int> forceContentIds;

	public LootData lootTable;

	public float breakertronNothingChance = 0.1f;

	public LootData breakertronLootTable;

	public bool prefersDungeonProcContents;

	public tk2dSprite ShadowSprite;

	public bool pickedUp;

	[CheckAnimation(null)]
	public string spawnAnimName;

	[CheckAnimation(null)]
	public string openAnimName;

	[CheckAnimation(null)]
	public string breakAnimName;

	[NonSerialized]
	private string overrideSpawnAnimName;

	[NonSerialized]
	private string overrideOpenAnimName;

	[NonSerialized]
	private string overrideBreakAnimName;

	[PickupIdentifier]
	public int overrideJunkId = -1;

	public GameObject VFX_PreSpawn;

	public GameObject VFX_GroundHit;

	public float groundHitDelay = 0.73f;

	public Transform spawnTransform;

	public AnimationCurve spawnCurve;

	public tk2dSpriteAnimator LockAnimator;

	public string LockOpenAnim = "lock_open";

	public string LockBreakAnim = "lock_break";

	public string LockNoKeyAnim = "lock_nokey";

	public tk2dSpriteAnimator[] SubAnimators;

	[NonSerialized]
	private GameObject minimapIconInstance;

	[NonSerialized]
	public bool IsLockBroken;

	public bool IsLocked;

	public bool IsSealed;

	public bool IsOpen;

	public bool IsBroken;

	public bool AlwaysBroadcastsOpenEvent;

	[NonSerialized]
	public float GeneratedMagnificence;

	protected bool m_temporarilyUnopenable;

	public bool IsRainbowChest;

	public bool IsMirrorChest;

	[NonSerialized]
	public bool ForceGlitchChest;

	protected bool m_isKeyOpening;

	private RoomHandler m_room;

	private RoomHandler m_registeredIconRoom;

	private bool m_isMimic;

	private bool m_isMimicBreathing;

	private System.Random m_runtimeRandom;

	[EnemyIdentifier]
	[Header("Mimic")]
	public string MimicGuid;

	public IntVector2 mimicOffset;

	[CheckAnimation(null)]
	public string preMimicIdleAnim;

	public float preMimicIdleAnimDelay = 3f;

	public float overrideMimicChance = -1f;

	private static bool m_IsCoopMode;

	public GameObject MinimapIconPrefab;

	private const float SPAWN_PUSH_RADIUS = 5f;

	private const float SPAWN_PUSH_FORCE = 22f;

	private bool m_hasCheckedBowler;

	private GameObject m_bowlerInstance;

	private Tribool m_bowlerFireStatus;

	private bool m_secretRainbowRevealed;

	private float m_bowlerFireTimer;

	private Color m_cachedOutlineColor;

	[NonSerialized]
	private ChestFuseController extantFuse;

	private const float RESOURCEFULRAT_CHEST_NOTE_CHANCE = 10.025f;

	private bool m_forceDropOkayForRainbowRun;

	private bool m_isGlitchChest;

	private bool m_cachedLockedState;

	private int m_cachedShadowSpriteID = -1;

	[NonSerialized]
	private int m_cachedSpriteForCoop = -1;

	private IntVector2 m_cachedCoopManualOffset;

	private IntVector2 m_cachedCoopManualDimensions;

	private bool m_configured;

	private bool m_hasBeenCheckedForFuses;

	[NonSerialized]
	public bool PreventFuse;

	public GeneralChestType ChestType
	{
		get
		{
			return m_chestType;
		}
		set
		{
			if (m_chestType == GeneralChestType.WEAPON)
			{
				StaticReferenceManager.WeaponChestsSpawnedOnFloor--;
			}
			else if (m_chestType == GeneralChestType.ITEM)
			{
				StaticReferenceManager.ItemChestsSpawnedOnFloor--;
			}
			m_chestType = value;
			if (m_chestType == GeneralChestType.WEAPON)
			{
				StaticReferenceManager.WeaponChestsSpawnedOnFloor++;
			}
			else if (m_chestType == GeneralChestType.ITEM)
			{
				StaticReferenceManager.ItemChestsSpawnedOnFloor++;
			}
		}
	}

	public bool TemporarilyUnopenable
	{
		get
		{
			return m_temporarilyUnopenable;
		}
	}

	public bool IsTruthChest
	{
		get
		{
			return base.name.Contains("TruthChest");
		}
	}

	public bool IsMimic
	{
		get
		{
			return m_isMimic;
		}
	}

	private Color BaseOutlineColor
	{
		get
		{
			if (m_isMimic && !Dungeon.IsGenerating)
			{
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					if ((bool)GameManager.Instance.AllPlayers[i] && GameManager.Instance.AllPlayers[i].CanDetectHiddenEnemies)
					{
						return Color.red;
					}
				}
			}
			return Color.black;
		}
	}

	public bool IsGlitched
	{
		get
		{
			return m_isGlitchChest;
		}
	}

	public static Chest Spawn(Chest chestPrefab, IntVector2 basePosition)
	{
		RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(basePosition);
		return Spawn(chestPrefab, basePosition, roomFromPosition);
	}

	public static Chest Spawn(Chest chestPrefab, IntVector2 basePosition, RoomHandler room, bool ForceNoMimic = false)
	{
		return Spawn(chestPrefab, basePosition.ToVector3(), room, ForceNoMimic);
	}

	public static Chest Spawn(Chest chestPrefab, Vector3 basePosition, RoomHandler room, bool ForceNoMimic = false)
	{
		if (chestPrefab == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(chestPrefab.gameObject, basePosition, Quaternion.identity);
		Chest component = gameObject.GetComponent<Chest>();
		if (ForceNoMimic)
		{
			component.MimicGuid = null;
		}
		component.m_room = room;
		component.HandleSpawnBehavior();
		return component;
	}

	public static void ToggleCoopChests(bool coopDead)
	{
		m_IsCoopMode = coopDead;
		for (int i = 0; i < StaticReferenceManager.AllChests.Count; i++)
		{
			if (coopDead)
			{
				StaticReferenceManager.AllChests[i].BecomeCoopChest();
			}
			else
			{
				StaticReferenceManager.AllChests[i].UnbecomeCoopChest();
			}
		}
	}

	public void RegisterChestOnMinimap(RoomHandler r)
	{
		m_registeredIconRoom = r;
		GameObject iconPrefab = MinimapIconPrefab ?? (BraveResources.Load("Global Prefabs/Minimap_Treasure_Icon") as GameObject);
		minimapIconInstance = Minimap.Instance.RegisterRoomIcon(r, iconPrefab);
	}

	public void DeregisterChestOnMinimap()
	{
		if (m_registeredIconRoom != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_registeredIconRoom, minimapIconInstance);
		}
	}

	private void Awake()
	{
		if (IsTruthChest)
		{
			PreventFuse = true;
		}
		StaticReferenceManager.AllChests.Add(this);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, BaseOutlineColor, 0.1f);
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleCoopChestAnimationEvent));
		MajorBreakable obj2 = base.majorBreakable;
		obj2.OnDamaged = (Action<float>)Delegate.Combine(obj2.OnDamaged, new Action<float>(OnDamaged));
		if (base.majorBreakable.DamageReduction > 1000f)
		{
			base.majorBreakable.ReportZeroDamage = true;
		}
		base.majorBreakable.InvulnerableToEnemyBullets = true;
		m_runtimeRandom = new System.Random();
	}

	private void OnEnable()
	{
		if (m_isMimic && !m_isMimicBreathing)
		{
			StartCoroutine(MimicIdleAnimCR());
		}
	}

	private void HandleCoopChestAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (frame.eventInfo == "coopchestvfx")
		{
			UnityEngine.Object.Instantiate(BraveResources.Load("Global VFX/VFX_ChestKnock_001"), base.sprite.WorldCenter + new Vector2(0f, 0.3125f), Quaternion.identity);
		}
	}

	protected void HandleSpawnBehavior()
	{
		StartCoroutine(SpawnBehavior_CR());
	}

	private IEnumerator SpawnBehavior_CR()
	{
		if ((bool)base.majorBreakable)
		{
			base.majorBreakable.TemporarilyInvulnerable = true;
		}
		Initialize();
		m_cachedSpriteForCoop = base.sprite.spriteId;
		m_temporarilyUnopenable = true;
		if (m_IsCoopMode)
		{
			BecomeCoopChest();
		}
		if (VFX_PreSpawn != null)
		{
			base.renderer.enabled = false;
			if (LockAnimator != null)
			{
				LockAnimator.GetComponent<Renderer>().enabled = false;
			}
			VFX_PreSpawn.SetActive(true);
			yield return new WaitForSeconds(0.1f);
			base.renderer.enabled = true;
			if (LockAnimator != null && IsLocked)
			{
				LockAnimator.GetComponent<Renderer>().enabled = true;
			}
		}
		string targetSpawnAnimName = (string.IsNullOrEmpty(overrideSpawnAnimName) ? spawnAnimName : overrideSpawnAnimName);
		if (!string.IsNullOrEmpty(targetSpawnAnimName))
		{
			tk2dSpriteAnimationClip clip = base.spriteAnimator.GetClipByName(targetSpawnAnimName);
			if (clip != null)
			{
				m_temporarilyUnopenable = true;
				base.specRigidbody.enabled = false;
				float clipTime = (float)clip.frames.Length / clip.fps;
				base.spriteAnimator.Play(targetSpawnAnimName);
				base.sprite.UpdateZDepth();
				float elapsed = 0f;
				bool groundHitTriggered = false;
				while (elapsed < clipTime)
				{
					elapsed += BraveTime.DeltaTime;
					if (elapsed >= groundHitDelay && !groundHitTriggered)
					{
						groundHitTriggered = true;
						Exploder.DoRadialPush(base.sprite.WorldCenter.ToVector3ZUp(base.sprite.WorldCenter.y), 22f, 5f);
						VFX_GroundHit.SetActive(true);
						base.specRigidbody.enabled = true;
						List<CollisionData> list = new List<CollisionData>();
						PhysicsEngine.Instance.OverlapCast(base.specRigidbody, list, false, true, null, null, false, null, null);
						for (int i = 0; i < list.Count; i++)
						{
							CollisionData collisionData = list[i];
							if ((bool)collisionData.OtherRigidbody && (bool)collisionData.OtherRigidbody.minorBreakable)
							{
								collisionData.OtherRigidbody.minorBreakable.Break(collisionData.OtherRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
							}
						}
					}
					yield return null;
				}
			}
		}
		base.sprite.UpdateZDepth();
		m_room.RegisterInteractable(this);
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		m_temporarilyUnopenable = false;
		if ((bool)base.majorBreakable)
		{
			base.majorBreakable.TemporarilyInvulnerable = false;
		}
		if (m_IsCoopMode)
		{
			if (LockAnimator != null)
			{
				LockAnimator.renderer.enabled = false;
			}
			base.spriteAnimator.Play("coop_chest_knock");
		}
		PossiblyCreateBowler(true);
	}

	private void PossiblyCreateBowler(bool mightBeActive)
	{
		if (m_hasCheckedBowler)
		{
			return;
		}
		m_hasCheckedBowler = true;
		if (IsRainbowChest || !GameStatsManager.HasInstance || !GameStatsManager.Instance.IsRainbowRun)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < StaticReferenceManager.AllChests.Count; i++)
		{
			if ((bool)StaticReferenceManager.AllChests[i] && !StaticReferenceManager.AllChests[i].IsRainbowChest && StaticReferenceManager.AllChests[i].GetAbsoluteParentRoom() == GetAbsoluteParentRoom() && StaticReferenceManager.AllChests[i] != this)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		bool flag2 = breakAnimName.Contains("redgold") || breakAnimName.Contains("black");
		m_bowlerInstance = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_BowlerSit"));
		m_bowlerInstance.transform.parent = base.transform;
		tk2dBaseSprite component = m_bowlerInstance.GetComponent<tk2dBaseSprite>();
		if ((bool)component)
		{
			SpriteOutlineManager.AddOutlineToSprite(component, Color.black, 0.05f);
		}
		if (UnityEngine.Random.value < 0.01f)
		{
			m_bowlerInstance.GetComponent<tk2dSpriteAnimator>().Play("salute_right");
			if (flag2)
			{
				m_bowlerInstance.transform.localPosition = new Vector3(0f, 0.625f);
				m_bowlerInstance.GetComponent<tk2dBaseSprite>().HeightOffGround = 0.5f;
			}
			else
			{
				m_bowlerInstance.transform.localPosition = new Vector3(-0.4375f, 0.125f);
				m_bowlerInstance.GetComponent<tk2dBaseSprite>().HeightOffGround = -0.75f;
			}
			m_bowlerFireStatus = Tribool.Ready;
		}
		else
		{
			m_bowlerInstance.GetComponent<tk2dSpriteAnimator>().Play("sit_right");
			if (flag2)
			{
				m_bowlerInstance.transform.localPosition = new Vector3(0f, 0.625f);
				m_bowlerInstance.GetComponent<tk2dBaseSprite>().HeightOffGround = 0.5f;
			}
			else
			{
				m_bowlerInstance.transform.localPosition = new Vector3(-0.25f, -0.3125f);
				m_bowlerInstance.GetComponent<tk2dBaseSprite>().HeightOffGround = -1.5f;
			}
			m_bowlerFireStatus = Tribool.Unready;
		}
		if (mightBeActive)
		{
			LootEngine.DoDefaultPurplePoof(m_bowlerInstance.GetComponent<tk2dBaseSprite>().WorldCenter);
		}
	}

	public void BecomeRainbowChest()
	{
		IsRainbowChest = true;
		lootTable.S_Chance = 0.2f;
		lootTable.A_Chance = 0.7f;
		lootTable.B_Chance = 0.4f;
		lootTable.C_Chance = 0f;
		lootTable.D_Chance = 0f;
		lootTable.Common_Chance = 0f;
		lootTable.canDropMultipleItems = true;
		lootTable.multipleItemDropChances = new WeightedIntCollection();
		lootTable.multipleItemDropChances.elements = new WeightedInt[1];
		lootTable.overrideItemLootTables = new List<GenericLootTable>();
		lootTable.lootTable = GameManager.Instance.RewardManager.GunsLootTable;
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.GunsLootTable);
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.ItemsLootTable);
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.GunsLootTable);
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.ItemsLootTable);
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.GunsLootTable);
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.ItemsLootTable);
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.GunsLootTable);
		lootTable.overrideItemLootTables.Add(GameManager.Instance.RewardManager.ItemsLootTable);
		if (GameStatsManager.Instance.IsRainbowRun)
		{
			lootTable.C_Chance = 0.2f;
			lootTable.D_Chance = 0.2f;
			lootTable.overrideItemQualities = new List<PickupObject.ItemQuality>();
			float value = UnityEngine.Random.value;
			if (value < 0.5f)
			{
				lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.S);
				lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.A);
			}
			else
			{
				lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.A);
				lootTable.overrideItemQualities.Add(PickupObject.ItemQuality.S);
			}
		}
		WeightedInt weightedInt = new WeightedInt();
		weightedInt.value = 8;
		weightedInt.weight = 1f;
		weightedInt.additionalPrerequisites = new DungeonPrerequisite[0];
		lootTable.multipleItemDropChances.elements[0] = weightedInt;
		lootTable.onlyOneGunCanDrop = false;
		if (ChestIdentifier == SpecialChestIdentifier.SECRET_RAINBOW)
		{
			spawnAnimName = "wood_chest_appear";
			openAnimName = "wood_chest_open";
			breakAnimName = "wood_chest_break";
		}
		else
		{
			spawnAnimName = "redgold_chest_appear";
			openAnimName = "redgold_chest_open";
			breakAnimName = "redgold_chest_break";
			base.majorBreakable.spriteNameToUseAtZeroHP = "chest_redgold_break_001";
		}
		base.sprite.usesOverrideMaterial = true;
		tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(spawnAnimName);
		base.sprite.SetSprite(clipByName.frames[clipByName.frames.Length - 1].spriteId);
		if (ChestIdentifier != SpecialChestIdentifier.SECRET_RAINBOW)
		{
			if ((bool)LockAnimator)
			{
				UnityEngine.Object.Destroy(LockAnimator.gameObject);
				LockAnimator = null;
			}
			IsLocked = false;
			base.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/RainbowChestShader");
		}
	}

	public void RevealSecretRainbow()
	{
		if (!m_secretRainbowRevealed)
		{
			m_secretRainbowRevealed = true;
			base.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/RainbowChestShader");
			base.sprite.renderer.material.SetFloat("_HueTestValue", -3.5f);
			if ((bool)LockAnimator)
			{
				UnityEngine.Object.Destroy(LockAnimator.gameObject);
				LockAnimator = null;
			}
			IsLocked = false;
		}
	}

	protected void Initialize()
	{
		if (m_IsCoopMode && GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER)
		{
			m_IsCoopMode = false;
		}
		base.specRigidbody.Initialize();
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
		speculativeRigidbody3.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody3.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		base.specRigidbody.PreventPiercing = true;
		MajorBreakable component = GetComponent<MajorBreakable>();
		if (component != null)
		{
			component.OnBreak = (Action)Delegate.Combine(component.OnBreak, new Action(OnBroken));
		}
		IntVector2 intVector = base.specRigidbody.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = base.specRigidbody.UnitTopRight.ToIntVector2(VectorConversions.Floor);
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				GameManager.Instance.Dungeon.data[new IntVector2(i, j)].isOccupied = true;
			}
		}
		bool flag = UnityEngine.Random.value < 0.000333f;
		if (ChestIdentifier == SpecialChestIdentifier.RAT || (lootTable != null && lootTable.CompletesSynergy))
		{
			flag = false;
		}
		else if (!flag && spawnAnimName.StartsWith("wood_") && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.WOODEN_CHESTS_BROKEN) >= 5f && UnityEngine.Random.value < 0.000333f)
		{
			flag = true;
			ChestIdentifier = SpecialChestIdentifier.SECRET_RAINBOW;
		}
		if (IsMirrorChest)
		{
			base.sprite.renderer.enabled = false;
			if ((bool)LockAnimator)
			{
				LockAnimator.Sprite.renderer.enabled = false;
			}
			if ((bool)ShadowSprite)
			{
				ShadowSprite.renderer.enabled = false;
			}
			base.specRigidbody.enabled = false;
		}
		else if (IsRainbowChest || flag)
		{
			BecomeRainbowChest();
		}
		else if (ForceGlitchChest || ((GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON || GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON || GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON) && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_ATTEMPTS) > 10f && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_BEHOLSTER) && !GameManager.Instance.InTutorial && UnityEngine.Random.value < 0.001f))
		{
			BecomeGlitchChest();
		}
		else if (!m_IsCoopMode)
		{
			MaybeBecomeMimic();
		}
	}

	private void Update()
	{
		if (m_isMimic && !m_temporarilyUnopenable && (bool)base.sprite)
		{
			Color baseOutlineColor = BaseOutlineColor;
			if (baseOutlineColor != m_cachedOutlineColor)
			{
				m_cachedOutlineColor = baseOutlineColor;
				SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
				SpriteOutlineManager.AddOutlineToSprite(base.sprite, baseOutlineColor, 0.1f);
			}
		}
		if (!m_bowlerInstance)
		{
			return;
		}
		if (m_bowlerFireStatus == Tribool.Ready)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				float num = Vector2.Distance(GameManager.Instance.AllPlayers[i].CenterPosition, m_bowlerInstance.transform.position);
				if (num < 5f)
				{
					m_bowlerFireStatus = Tribool.Complete;
					AkSoundEngine.PostEvent("Play_obj_bowler_ignite_01", base.gameObject);
					AkSoundEngine.PostEvent("Play_obj_bowler_burn_01", base.gameObject);
				}
			}
		}
		else
		{
			if (!(m_bowlerFireStatus == Tribool.Complete))
			{
				return;
			}
			m_bowlerFireTimer += BraveTime.DeltaTime * 15f;
			if (m_bowlerFireTimer > 1f)
			{
				GlobalSparksDoer.DoRadialParticleBurst(Mathf.FloorToInt(m_bowlerFireTimer), m_bowlerInstance.GetComponent<tk2dBaseSprite>().WorldBottomLeft + new Vector2(0.125f, 0.1875f), m_bowlerInstance.GetComponent<tk2dBaseSprite>().WorldTopRight - new Vector2(0.125f, 0.125f), 0f, 0f, 0f, null, null, null, GlobalSparksDoer.SparksType.STRAIGHT_UP_FIRE);
				if ((bool)base.sprite)
				{
					GlobalSparksDoer.DoRadialParticleBurst(Mathf.FloorToInt(m_bowlerFireTimer * 3f), base.sprite.WorldBottomLeft + new Vector2(0.125f, 0.1875f), base.sprite.WorldTopRight - new Vector2(0.125f, 0.125f), 0f, 0f, 0f, null, null, null, GlobalSparksDoer.SparksType.STRAIGHT_UP_FIRE);
				}
				m_bowlerFireTimer %= 1f;
			}
		}
	}

	protected void TriggerCountdownTimer()
	{
		if ((bool)this)
		{
			GameObject original = (GameObject)BraveResources.Load("Chest_Fuse");
			GameObject gameObject = UnityEngine.Object.Instantiate(original, base.transform.position + new Vector3(-1.75f, -1.5f, 0f), Quaternion.identity);
			extantFuse = gameObject.GetComponent<ChestFuseController>();
			StartCoroutine(HandleExplosionCountdown(extantFuse));
		}
	}

	public void ForceKillFuse()
	{
		if ((bool)extantFuse)
		{
			AkSoundEngine.PostEvent("stop_obj_fuse_loop_01", base.gameObject);
			extantFuse = null;
		}
	}

	private IEnumerator HandleExplosionCountdown(ChestFuseController fuse)
	{
		float elapsed = 0f;
		float timer = 10f;
		while (elapsed < timer)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / timer;
			if (IsBroken || IsOpen || PreventFuse || !extantFuse)
			{
				yield break;
			}
			Vector2? sparkPos = fuse.SetFuseCompletion(t);
			if (sparkPos.HasValue)
			{
				IntVector2 key = (sparkPos.Value / DeadlyDeadlyGoopManager.GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
				if (DeadlyDeadlyGoopManager.allGoopPositionMap.ContainsKey(key))
				{
					DeadlyDeadlyGoopManager deadlyDeadlyGoopManager = DeadlyDeadlyGoopManager.allGoopPositionMap[key];
					GoopDefinition goopDefinition = deadlyDeadlyGoopManager.goopDefinition;
					if (!goopDefinition.CanBeIgnited)
					{
						AkSoundEngine.PostEvent("stop_obj_fuse_loop_01", base.gameObject);
						yield break;
					}
					deadlyDeadlyGoopManager.IgniteGoopCircle(sparkPos.Value, 0.5f);
				}
			}
			yield return null;
		}
		if (!IsOpen && !IsBroken)
		{
			m_isMimic = false;
			ExplodeInSadness();
		}
	}

	private void ExplodeInSadness()
	{
		MajorBreakable component = GetComponent<MajorBreakable>();
		GetRidOfBowler();
		if (component != null)
		{
			component.OnBreak = (Action)Delegate.Remove(component.OnBreak, new Action(OnBroken));
		}
		base.spriteAnimator.Play(string.IsNullOrEmpty(overrideBreakAnimName) ? breakAnimName : overrideBreakAnimName);
		base.specRigidbody.enabled = false;
		IsBroken = true;
		if (LockAnimator != null && (bool)LockAnimator)
		{
			UnityEngine.Object.Destroy(LockAnimator.gameObject);
		}
		Transform transform = base.transform.Find("Shadow");
		if (transform != null)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		pickedUp = true;
		HandleGeneratedMagnificence();
		if (m_registeredIconRoom != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_registeredIconRoom, minimapIconInstance);
		}
		m_room.DeregisterInteractable(this);
		Exploder.DoDefaultExplosion(base.sprite.WorldCenter, Vector2.zero);
	}

	private void OnBroken()
	{
		GetRidOfBowler();
		if (ChestIdentifier == SpecialChestIdentifier.SECRET_RAINBOW)
		{
			RevealSecretRainbow();
		}
		if (ChestIdentifier == SpecialChestIdentifier.SECRET_RAINBOW || IsRainbowChest || breakAnimName.Contains("redgold") || breakAnimName.Contains("black"))
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_GOLD_JUNK, true);
		}
		base.spriteAnimator.Play(string.IsNullOrEmpty(overrideBreakAnimName) ? breakAnimName : overrideBreakAnimName);
		base.specRigidbody.enabled = false;
		IsBroken = true;
		IntVector2 intVector = base.specRigidbody.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector2 = base.specRigidbody.UnitTopRight.ToIntVector2(VectorConversions.Floor);
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				GameManager.Instance.Dungeon.data[new IntVector2(i, j)].isOccupied = false;
			}
		}
		if (LockAnimator != null && (bool)LockAnimator)
		{
			UnityEngine.Object.Destroy(LockAnimator.gameObject);
		}
		Transform transform = base.transform.Find("Shadow");
		if (transform != null)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		if (pickedUp)
		{
			return;
		}
		pickedUp = true;
		HandleGeneratedMagnificence();
		m_room.DeregisterInteractable(this);
		if (m_registeredIconRoom != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_registeredIconRoom, minimapIconInstance);
		}
		if (spawnAnimName.StartsWith("wood_"))
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.WOODEN_CHESTS_BROKEN, 1f);
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.NumberOfLivingPlayers == 1)
		{
			StartCoroutine(GiveCoopPartnerBack(false));
		}
		else
		{
			bool flag = PassiveItem.IsFlagSetAtAll(typeof(ChestBrokenImprovementItem));
			bool flag2 = GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_GOLD_JUNK);
			float num = GameManager.Instance.RewardManager.ChestDowngradeChance;
			float num2 = GameManager.Instance.RewardManager.ChestHalfHeartChance;
			float num3 = GameManager.Instance.RewardManager.ChestExplosionChance;
			float num4 = GameManager.Instance.RewardManager.ChestJunkChance;
			float num5 = ((!flag2) ? 0f : 0.005f);
			bool flag3 = GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_SER_JUNKAN_UNLOCKED);
			float num6 = ((!flag3 || HasDroppedSerJunkanThisSession) ? 0f : GameManager.Instance.RewardManager.ChestJunkanUnlockedChance);
			if ((bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.carriedConsumables.KeyBullets > 0)
			{
				num4 *= GameManager.Instance.RewardManager.HasKeyJunkMultiplier;
			}
			if (SackKnightController.HasJunkan())
			{
				num4 *= GameManager.Instance.RewardManager.HasJunkanJunkMultiplier;
				num5 *= 3f;
			}
			if (IsTruthChest)
			{
				num = 0f;
				num2 = 0f;
				num3 = 0f;
				num4 = 1f;
			}
			num4 -= num5;
			float num7 = num5 + num + num2 + num3 + num4 + num6;
			float num8 = UnityEngine.Random.value * num7;
			if (flag2 && num8 < num5)
			{
				contents = new List<PickupObject>();
				int goldJunk = GlobalItemIds.GoldJunk;
				contents.Add(PickupObjectDatabase.GetById(goldJunk));
				m_forceDropOkayForRainbowRun = true;
				StartCoroutine(PresentItem());
			}
			else if (num8 < num || flag)
			{
				int tierShift = -4;
				bool flag4 = false;
				if (flag)
				{
					float value = UnityEngine.Random.value;
					if (!(value < ChestBrokenImprovementItem.PickupQualChance))
					{
						tierShift = ((value < ChestBrokenImprovementItem.PickupQualChance + ChestBrokenImprovementItem.MinusOneQualChance) ? (-1) : ((!(value < ChestBrokenImprovementItem.PickupQualChance + ChestBrokenImprovementItem.EqualQualChance + ChestBrokenImprovementItem.MinusOneQualChance)) ? 1 : 0));
					}
					else
					{
						flag4 = true;
						contents = new List<PickupObject>();
						PickupObject pickupObject = null;
						while (pickupObject == null)
						{
							GameObject gameObject = GameManager.Instance.RewardManager.CurrentRewardData.SingleItemRewardTable.SelectByWeight();
							if ((bool)gameObject)
							{
								pickupObject = gameObject.GetComponent<PickupObject>();
							}
						}
						contents.Add(pickupObject);
						StartCoroutine(PresentItem());
					}
				}
				if (!flag4)
				{
					DetermineContents(GameManager.Instance.PrimaryPlayer, tierShift);
					StartCoroutine(PresentItem());
				}
			}
			else if (num8 < num + num2)
			{
				contents = new List<PickupObject>();
				contents.Add(GameManager.Instance.RewardManager.HalfHeartPrefab);
				m_forceDropOkayForRainbowRun = true;
				StartCoroutine(PresentItem());
			}
			else if (num8 < num + num2 + num4)
			{
				bool flag5 = false;
				if (!HasDroppedSerJunkanThisSession && !flag3 && UnityEngine.Random.value < 0.2f)
				{
					HasDroppedSerJunkanThisSession = true;
					flag5 = true;
				}
				contents = new List<PickupObject>();
				int id = ((overrideJunkId < 0) ? GlobalItemIds.Junk : overrideJunkId);
				if (flag5)
				{
					id = GlobalItemIds.SackKnightBoon;
				}
				contents.Add(PickupObjectDatabase.GetById(id));
				m_forceDropOkayForRainbowRun = true;
				StartCoroutine(PresentItem());
			}
			else if (num8 < num + num2 + num4 + num6)
			{
				HasDroppedSerJunkanThisSession = true;
				contents = new List<PickupObject>();
				contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.SackKnightBoon));
				StartCoroutine(PresentItem());
			}
			else
			{
				Exploder.DoDefaultExplosion(base.sprite.WorldCenter, Vector2.zero);
			}
		}
		for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
		{
			if (GameManager.Instance.AllPlayers[k].OnChestBroken != null)
			{
				GameManager.Instance.AllPlayers[k].OnChestBroken(GameManager.Instance.AllPlayers[k], this);
			}
		}
	}

	private IEnumerator GiveCoopPartnerBack(bool doDelay = true)
	{
		if (doDelay)
		{
			yield return new WaitForSeconds(0.7f);
		}
		AkSoundEngine.PostEvent("play_obj_chest_open_01", base.gameObject);
		AkSoundEngine.PostEvent("stop_obj_fuse_loop_01", base.gameObject);
		PlayerController deadPlayer = ((!GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) ? GameManager.Instance.SecondaryPlayer : GameManager.Instance.PrimaryPlayer);
		deadPlayer.specRigidbody.enabled = true;
		deadPlayer.gameObject.SetActive(true);
		deadPlayer.sprite.renderer.enabled = true;
		deadPlayer.ResurrectFromChest(base.sprite.WorldBottomCenter);
	}

	private void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!myPixelCollider.IsTrigger && otherRigidbody.GetComponent<KeyBullet>() != null)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void OnTriggerCollision(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (!m_isKeyOpening)
		{
			KeyBullet component = specRigidbody.GetComponent<KeyBullet>();
			if (component != null)
			{
				HandleKeyEncounter(component);
			}
		}
	}

	public void HandleKeyEncounter(KeyBullet key)
	{
		if (!IsSealed && IsLocked)
		{
			m_isKeyOpening = true;
			Projectile component = key.GetComponent<Projectile>();
			component.specRigidbody.Velocity = Vector2.zero;
			GameObject overrideMidairDeathVFX = component.hitEffects.overrideMidairDeathVFX;
			PlayerController optionalPlayer = component.Owner as PlayerController;
			UnityEngine.Object.Destroy(component.specRigidbody);
			UnityEngine.Object.Destroy(component);
			StartCoroutine(HandleKeyEncounter_CR(key, overrideMidairDeathVFX, optionalPlayer));
		}
	}

	private IEnumerator HandleKeyEncounter_CR(KeyBullet key, GameObject vfxPrefab, PlayerController optionalPlayer)
	{
		tk2dBaseSprite keySprite = key.GetComponentInChildren<tk2dBaseSprite>();
		Vector2 keyPositionOffset = keySprite.GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter);
		Vector2 lockCenter = ((!(LockAnimator != null)) ? base.sprite.WorldCenter : LockAnimator.Sprite.WorldCenter);
		Vector2 lockToKey = (key.transform.position + key.transform.rotation * keyPositionOffset).XY() - lockCenter;
		float distFromLock = lockToKey.magnitude;
		float degreeDiff = BraveMathCollege.ClampAngle180(lockToKey.ToAngle() + 90f);
		while (Mathf.Abs(degreeDiff) > 0f)
		{
			degreeDiff = Mathf.MoveTowards(degreeDiff, 0f, 600f * BraveTime.DeltaTime);
			key.transform.position = (lockCenter + new Vector2(0f, 0f - distFromLock).Rotate(degreeDiff)).ToVector3ZUp(key.transform.position.z) + key.transform.rotation * keyPositionOffset;
			key.transform.rotation = Quaternion.Euler(0f, 0f, degreeDiff + 90f);
			keySprite.UpdateZDepth();
			yield return null;
		}
		while (distFromLock > 1f)
		{
			distFromLock = Mathf.MoveTowards(distFromLock, 1f, BraveTime.DeltaTime * 14f);
			key.transform.position = (lockCenter + new Vector2(0f, 0f - distFromLock)).ToVector3ZUp(key.transform.position.z) + key.transform.rotation * keyPositionOffset;
			yield return null;
		}
		GameObject vfxInstance = UnityEngine.Object.Instantiate(vfxPrefab);
		vfxInstance.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(keySprite.WorldCenter.ToVector3ZUp(), tk2dBaseSprite.Anchor.MiddleCenter);
		UnityEngine.Object.Destroy(key.gameObject);
		if (!IsLocked)
		{
			yield break;
		}
		IsLocked = false;
		if (!pickedUp)
		{
			if (LockAnimator != null)
			{
				LockAnimator.PlayAndDestroyObject(LockOpenAnim);
			}
			Open(optionalPlayer ?? GameManager.Instance.PrimaryPlayer);
		}
	}

	public void ForceUnlock()
	{
		if (IsLocked)
		{
			IsLocked = false;
			if (LockAnimator != null)
			{
				LockAnimator.PlayAndDestroyObject(LockOpenAnim);
			}
		}
	}

	private void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (ChestIdentifier == SpecialChestIdentifier.SECRET_RAINBOW && (bool)rigidbodyCollision.OtherRigidbody && (bool)rigidbodyCollision.OtherRigidbody.projectile && BraveUtility.EnumFlagsContains((uint)rigidbodyCollision.OtherRigidbody.projectile.damageTypes, 32u) > 0)
		{
			RevealSecretRainbow();
		}
	}

	public void ForceOpen(PlayerController player)
	{
		Open(player);
	}

	protected void HandleGeneratedMagnificence()
	{
		if (GeneratedMagnificence > 0f)
		{
			GameManager.Instance.Dungeon.GeneratedMagnificence -= GeneratedMagnificence;
			GeneratedMagnificence = 0f;
		}
	}

	private void GetRidOfBowler()
	{
		if ((bool)m_bowlerInstance)
		{
			LootEngine.DoDefaultPurplePoof(m_bowlerInstance.GetComponent<tk2dBaseSprite>().WorldCenter);
			UnityEngine.Object.Destroy(m_bowlerInstance);
			m_bowlerInstance = null;
			AkSoundEngine.PostEvent("Stop_SND_OBJ", base.gameObject);
		}
	}

	protected void Open(PlayerController player)
	{
		if (!(player != null))
		{
			return;
		}
		GetRidOfBowler();
		if (GameManager.Instance.InTutorial || AlwaysBroadcastsOpenEvent)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerOpenedChest");
		}
		if (m_registeredIconRoom != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_registeredIconRoom, minimapIconInstance);
		}
		if (m_isGlitchChest)
		{
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(player);
				if ((bool)otherPlayer && otherPlayer.IsGhost)
				{
					StartCoroutine(GiveCoopPartnerBack(false));
				}
			}
			GameManager.Instance.InjectedFlowPath = "Core Game Flows/Secret_DoubleBeholster_Flow";
			Pixelator.Instance.FadeToBlack(0.5f);
			GameManager.Instance.DelayedLoadNextLevel(0.5f);
			return;
		}
		if (m_isMimic && !m_IsCoopMode)
		{
			DetermineContents(player);
			DoMimicTransformation(contents);
			return;
		}
		if (ChestIdentifier == SpecialChestIdentifier.SECRET_RAINBOW)
		{
			RevealSecretRainbow();
		}
		pickedUp = true;
		IsOpen = true;
		HandleGeneratedMagnificence();
		m_room.DeregisterInteractable(this);
		MajorBreakable component = GetComponent<MajorBreakable>();
		if (component != null)
		{
			component.usesTemporaryZeroHitPointsState = false;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.NumberOfLivingPlayers == 1 && ChestIdentifier == SpecialChestIdentifier.NORMAL)
		{
			base.spriteAnimator.Play((!string.IsNullOrEmpty(overrideOpenAnimName)) ? overrideOpenAnimName : openAnimName);
			m_isMimic = false;
			StartCoroutine(GiveCoopPartnerBack());
			return;
		}
		if (lootTable.CompletesSynergy)
		{
			StartCoroutine(HandleSynergyGambleChest(player));
			return;
		}
		DetermineContents(player);
		if (base.name.Contains("Chest_Red") && contents != null && contents.Count == 1 && (bool)contents[0] && contents[0].ItemSpansBaseQualityTiers)
		{
			contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.Key));
		}
		base.spriteAnimator.Play((!string.IsNullOrEmpty(overrideOpenAnimName)) ? overrideOpenAnimName : openAnimName);
		AkSoundEngine.PostEvent("play_obj_chest_open_01", base.gameObject);
		AkSoundEngine.PostEvent("stop_obj_fuse_loop_01", base.gameObject);
		if (m_isMimic)
		{
			return;
		}
		if (SubAnimators != null && SubAnimators.Length > 0)
		{
			for (int i = 0; i < SubAnimators.Length; i++)
			{
				SubAnimators[i].Play();
			}
		}
		player.TriggerItemAcquisition();
		StartCoroutine(PresentItem());
	}

	private IEnumerator HandleSynergyGambleChest(PlayerController player)
	{
		base.majorBreakable.TemporarilyInvulnerable = true;
		DetermineContents(player);
		base.spriteAnimator.Play(openAnimName);
		AkSoundEngine.PostEvent("stop_obj_fuse_loop_01", base.gameObject);
		if (SubAnimators.Length > 0 && (bool)SubAnimators[0])
		{
			SubAnimators[0].gameObject.SetActive(true);
			SubAnimators[0].Play("synergy_chest_open_gamble_vfx");
		}
		while (base.spriteAnimator.IsPlaying(openAnimName))
		{
			yield return null;
		}
		bool succeeded = false;
		for (int i = 0; i < contents.Count; i++)
		{
			PickupObject prefab = contents[i];
			bool usesStartingItem = false;
			if (RewardManager.AnyPlayerHasItemInSynergyContainingOtherItem(prefab, ref usesStartingItem))
			{
				succeeded = true;
				break;
			}
		}
		if (succeeded)
		{
			base.spriteAnimator.Play("synergy_chest_open_synergy");
			if (SubAnimators.Length > 0 && (bool)SubAnimators[0])
			{
				SubAnimators[0].PlayAndDisableObject("synergy_chest_open_synergy_vfx");
			}
			yield return new WaitForSeconds(0.725f);
		}
		else
		{
			base.spriteAnimator.Play("synergy_chest_open_fail");
			if (SubAnimators.Length > 0 && (bool)SubAnimators[0])
			{
				SubAnimators[0].PlayAndDisableObject("synergy_chest_open_fail_vfx");
			}
			yield return new WaitForSeconds(0.44f);
		}
		if (!m_isMimic)
		{
			player.TriggerItemAcquisition();
			StartCoroutine(PresentItem());
		}
		base.majorBreakable.TemporarilyInvulnerable = false;
	}

	protected bool HandleQuestContentsModification()
	{
		if (GameManager.Instance.InTutorial)
		{
			return false;
		}
		if (IsRainbowChest)
		{
			return false;
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_01) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_02) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_03) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_04) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_05) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_06))
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.RESOURCEFUL_RAT_COMPLETE, true);
		}
		if (!GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_COMPLETE) && !HasDroppedResourcefulRatNoteThisSession)
		{
			float b = 0.15f;
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_01))
			{
				b = 0.33f;
			}
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_LICH))
			{
				b = 10f;
			}
			float playerStatValue = GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_ATTEMPTS);
			if ((bool)GameManager.Instance.Dungeon && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
			{
				b = 0f;
			}
			b = ((!(playerStatValue < 10f)) ? Mathf.Lerp(0f, b, Mathf.Clamp01(playerStatValue / 20f)) : 0f);
			if (UnityEngine.Random.value < b)
			{
				bool flag = !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_01) && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_CLEARED_GUNGEON) > 0f;
				bool flag2 = !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_02) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_01) && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.ITEMS_TAKEN_BY_RAT) > 0f;
				bool flag3 = !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_03) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_02) && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_SEWERS) > 0f;
				bool flag4 = !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_04) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_03) && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.MASTERY_TOKENS_RECEIVED) > 0f;
				bool flag5 = !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_05) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_04) && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_MINI_FUSELIER);
				bool flag6 = m_isMimic && !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_06) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_05) && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_DRAGUN);
				if (flag)
				{
					contents.Clear();
					contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.RatNote01));
					HasDroppedResourcefulRatNoteThisSession = true;
					return true;
				}
				if (flag2)
				{
					contents.Clear();
					contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.RatNote02));
					HasDroppedResourcefulRatNoteThisSession = true;
					return true;
				}
				if (flag3)
				{
					contents.Clear();
					contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.RatNote03));
					HasDroppedResourcefulRatNoteThisSession = true;
					return true;
				}
				if (flag4)
				{
					contents.Clear();
					contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.RatNote04));
					HasDroppedResourcefulRatNoteThisSession = true;
					return true;
				}
				if (flag5)
				{
					contents.Clear();
					contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.RatNote05));
					HasDroppedResourcefulRatNoteThisSession = true;
					return true;
				}
				if (flag6)
				{
					contents.Clear();
					contents.Add(PickupObjectDatabase.GetById(GlobalItemIds.RatNote06));
					HasDroppedResourcefulRatNoteThisSession = true;
					return true;
				}
			}
		}
		return false;
	}

	public void GenerationDetermineContents(FloorRewardManifest manifest, System.Random safeRandom)
	{
		List<PickupObject> list = GenerateContents(null, 0, safeRandom);
		manifest.RegisterContents(this, list);
	}

	protected List<PickupObject> GenerateContents(PlayerController player, int tierShift, System.Random safeRandom = null)
	{
		List<PickupObject> list = new List<PickupObject>();
		if (lootTable.lootTable == null)
		{
			list.Add(GameManager.Instance.Dungeon.baseChestContents.SelectByWeight().GetComponent<PickupObject>());
		}
		else if (lootTable != null)
		{
			if (tierShift <= -1)
			{
				list = ((!(breakertronLootTable.lootTable != null)) ? lootTable.GetItemsForPlayer(player, tierShift, null, safeRandom) : breakertronLootTable.GetItemsForPlayer(player, -1, null, safeRandom));
			}
			else
			{
				list = lootTable.GetItemsForPlayer(player, tierShift, null, safeRandom);
				if (lootTable.CompletesSynergy)
				{
					float value = Mathf.Clamp01(0.6f - 0.1f * (float)lootTable.LastGenerationNumSynergiesCalculated);
					value = Mathf.Clamp(value, 0.2f, 1f);
					if (value > 0f)
					{
						float num = ((safeRandom == null) ? UnityEngine.Random.value : ((float)safeRandom.NextDouble()));
						if (num < value)
						{
							lootTable.CompletesSynergy = false;
							list = lootTable.GetItemsForPlayer(player, tierShift, null, safeRandom);
							lootTable.CompletesSynergy = true;
						}
					}
				}
			}
		}
		return list;
	}

	public List<PickupObject> PredictContents(PlayerController player)
	{
		DetermineContents(player);
		return contents;
	}

	protected void DetermineContents(PlayerController player, int tierShift = 0)
	{
		if (contents == null)
		{
			contents = new List<PickupObject>();
			if (forceContentIds.Count > 0)
			{
				for (int i = 0; i < forceContentIds.Count; i++)
				{
					contents.Add(PickupObjectDatabase.GetById(forceContentIds[i]));
				}
			}
		}
		bool flag = HandleQuestContentsModification();
		if (contents.Count == 0 && !flag)
		{
			FloorRewardManifest seededManifestForCurrentFloor = GameManager.Instance.RewardManager.GetSeededManifestForCurrentFloor();
			if (seededManifestForCurrentFloor != null && seededManifestForCurrentFloor.PregeneratedChestContents.ContainsKey(this))
			{
				contents = seededManifestForCurrentFloor.PregeneratedChestContents[this];
			}
			else
			{
				contents = GenerateContents(player, tierShift, m_runtimeRandom);
			}
			if (contents.Count == 0)
			{
				Debug.LogError("Emergency Mimic swap... what's going to happen to the loot now?");
				m_isMimic = true;
				DoMimicTransformation(null);
			}
		}
	}

	private void DoMimicTransformation(List<PickupObject> overrideDeathRewards)
	{
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
			aIActor.AdditionalSafeItemDrops.AddRange(overrideDeathRewards);
		}
		aIActor.specRigidbody.Initialize();
		Vector2 unitBottomLeft = aIActor.specRigidbody.UnitBottomLeft;
		Vector2 unitBottomLeft2 = base.specRigidbody.UnitBottomLeft;
		aIActor.transform.position -= (Vector3)(unitBottomLeft - unitBottomLeft2);
		aIActor.transform.position += (Vector3)PhysicsEngine.PixelToUnit(mimicOffset);
		aIActor.specRigidbody.Reinitialize();
		aIActor.HasDonePlayerEnterCheck = true;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected void SpewContentsOntoGround(List<Transform> spawnTransforms)
	{
		List<DebrisObject> list = new List<DebrisObject>();
		bool isRainbowRun = GameStatsManager.Instance.IsRainbowRun;
		if (isRainbowRun && !IsRainbowChest && !m_forceDropOkayForRainbowRun)
		{
			Vector2 vector;
			if (spawnTransform != null)
			{
				vector = spawnTransform.position;
			}
			else
			{
				Bounds bounds = base.sprite.GetBounds();
				vector = base.transform.position + bounds.extents;
			}
			LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteChest, vector + new Vector2(-0.5f, -2.25f), m_room, true);
		}
		else
		{
			for (int i = 0; i < contents.Count; i++)
			{
				List<GameObject> list2 = new List<GameObject>();
				list2.Add(contents[i].gameObject);
				List<DebrisObject> list3 = LootEngine.SpewLoot(list2, spawnTransforms[i].position);
				list.AddRange(list3);
				for (int j = 0; j < list3.Count; j++)
				{
					if ((bool)list3[j])
					{
						list3[j].PreventFallingInPits = true;
					}
					if (!(list3[j].GetComponent<Gun>() != null) && !(list3[j].GetComponent<CurrencyPickup>() != null) && list3[j].specRigidbody != null)
					{
						list3[j].specRigidbody.CollideWithOthers = false;
						DebrisObject debrisObject = list3[j];
						debrisObject.OnTouchedGround = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnTouchedGround, new Action<DebrisObject>(BecomeViableItem));
					}
				}
			}
		}
		if (IsRainbowChest && isRainbowRun && base.transform.position.GetAbsoluteRoom() == GameManager.Instance.Dungeon.data.Entrance)
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleRainbowRunLootProcessing(list));
		}
	}

	private IEnumerator HandleRainbowRunLootProcessing(List<DebrisObject> items)
	{
		if ((bool)base.majorBreakable)
		{
			base.majorBreakable.Break(Vector2.zero);
		}
		while (true)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if ((bool)items[i])
				{
					continue;
				}
				for (int j = 0; j < items.Count; j++)
				{
					if (i != j)
					{
						LootEngine.DoDefaultItemPoof(items[j].transform.position, false, true);
						UnityEngine.Object.Destroy(items[j].gameObject);
					}
				}
				if ((bool)this)
				{
					LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNotePostRainbow, base.transform.position.XY() + new Vector2(1f, 1.5f), base.transform.position.GetAbsoluteRoom(), true);
				}
				yield break;
			}
			yield return null;
		}
	}

	protected void BecomeViableItem(DebrisObject debris)
	{
		debris.OnTouchedGround = (Action<DebrisObject>)Delegate.Remove(debris.OnTouchedGround, new Action<DebrisObject>(BecomeViableItem));
		debris.OnGrounded = (Action<DebrisObject>)Delegate.Remove(debris.OnGrounded, new Action<DebrisObject>(BecomeViableItem));
		debris.specRigidbody.CollideWithOthers = true;
		Vector2 zero = Vector2.zero;
		zero = ((!(spawnTransform != null)) ? (debris.sprite.WorldCenter - base.sprite.WorldCenter) : (debris.sprite.WorldCenter - spawnTransform.position.XY()));
		debris.ClearVelocity();
		debris.ApplyVelocity(zero.normalized * 2f);
	}

	private bool CheckPresentedItemTheoreticalPosition(Vector3 targetPosition, Vector3 objectOffset)
	{
		Vector3 pos = targetPosition - new Vector3(objectOffset.x * 2f, 0f, 0f);
		Vector3 pos2 = targetPosition - new Vector3(0f, objectOffset.y * 2f, 0f);
		Vector3 pos3 = targetPosition - new Vector3(objectOffset.x * 2f, objectOffset.y * 2f, 0f);
		if (!CheckCellValidForItemSpawn(targetPosition) || !CheckCellValidForItemSpawn(pos) || !CheckCellValidForItemSpawn(pos2) || !CheckCellValidForItemSpawn(pos3))
		{
			return true;
		}
		return false;
	}

	private bool CheckCellValidForItemSpawn(Vector3 pos)
	{
		IntVector2 vec = pos.IntXY(VectorConversions.Floor);
		Dungeon dungeon = GameManager.Instance.Dungeon;
		if (!dungeon.data.CheckInBoundsAndValid(vec) || dungeon.CellIsPit(pos) || dungeon.data.isTopWall(vec.x, vec.y))
		{
			return false;
		}
		if (dungeon.data.isWall(vec.x, vec.y) && !dungeon.data.isFaceWallLower(vec.x, vec.y))
		{
			return false;
		}
		return true;
	}

	private IEnumerator PresentItem()
	{
		bool shouldActuallyPresent = !GameStatsManager.Instance.IsRainbowRun || IsRainbowChest || m_forceDropOkayForRainbowRun;
		List<Transform> vfxTransforms = new List<Transform>();
		List<Vector3> vfxObjectOffsets = new List<Vector3>();
		Vector3 attachPoint2 = Vector3.zero;
		if (shouldActuallyPresent)
		{
			if (spawnTransform != null)
			{
				attachPoint2 = spawnTransform.position;
			}
			else
			{
				Bounds bounds = base.sprite.GetBounds();
				attachPoint2 = base.transform.position + bounds.extents;
			}
			for (int i = 0; i < contents.Count; i++)
			{
				PickupObject pickupObject = contents[i];
				tk2dSprite tk2dSprite2 = pickupObject.GetComponent<tk2dSprite>();
				if (tk2dSprite2 == null)
				{
					tk2dSprite2 = pickupObject.GetComponentInChildren<tk2dSprite>();
				}
				GameObject gameObject = new GameObject("VFX_Chest_Item");
				Transform transform = gameObject.transform;
				Vector3 vector = Vector3.zero;
				if (tk2dSprite2 != null)
				{
					tk2dSprite tk2dSprite3 = tk2dSprite.AddComponent(gameObject, tk2dSprite2.Collection, tk2dSprite2.spriteId);
					tk2dSprite3.HeightOffGround = 2f;
					NotePassiveItem component = tk2dSprite2.GetComponent<NotePassiveItem>();
					if (component != null && component.ResourcefulRatNoteIdentifier >= 0)
					{
						tk2dSprite3.SetSprite(component.GetAppropriateSpriteName(false));
					}
					SpriteOutlineManager.AddOutlineToSprite(tk2dSprite3, Color.white, 0.5f);
					vector = -BraveUtility.QuantizeVector(gameObject.GetComponent<tk2dSprite>().GetBounds().extents);
					tk2dSprite3.UpdateZDepth();
				}
				transform.position = attachPoint2 + vector;
				vfxTransforms.Add(transform);
				vfxObjectOffsets.Add(vector);
			}
			float displayTime = 1f;
			float elapsed = 0f;
			while (elapsed < displayTime)
			{
				elapsed += BraveTime.DeltaTime * 1.5f;
				float t = Mathf.Clamp01(elapsed / displayTime);
				float curveValue = spawnCurve.Evaluate(t);
				float modT = Mathf.SmoothStep(0f, 1f, t);
				if (vfxTransforms.Count <= 4)
				{
					for (int j = 0; j < vfxTransforms.Count; j++)
					{
						float num = ((vfxTransforms.Count != 1) ? (-1f + 2f / (float)(vfxTransforms.Count - 1) * (float)j) : 0f);
						num = num * ((float)vfxTransforms.Count / 2f) * 1f;
						Vector3 vector2 = attachPoint2 + vfxObjectOffsets[j] + new Vector3(Mathf.Lerp(0f, num, modT), curveValue, -2.5f);
						if (CheckPresentedItemTheoreticalPosition(vector2, vfxObjectOffsets[j]))
						{
							vector2 = vfxTransforms[j].position;
						}
						vfxTransforms[j].position = vector2;
					}
				}
				else
				{
					for (int k = 0; k < vfxTransforms.Count; k++)
					{
						float num2 = 360f / (float)vfxTransforms.Count;
						Vector3 vector3 = Quaternion.Euler(0f, 0f, num2 * (float)k) * Vector3.right;
						float num3 = 3f;
						Vector2 b = vector3.XY().normalized * num3;
						Vector3 vector4 = attachPoint2 + vfxObjectOffsets[k] + new Vector3(0f, curveValue, -2.5f) + Vector2.Lerp(Vector2.zero, b, modT).ToVector3ZUp();
						if (CheckPresentedItemTheoreticalPosition(vector4, vfxObjectOffsets[k]))
						{
							vector4 = vfxTransforms[k].position;
						}
						vfxTransforms[k].position = vector4;
					}
				}
				yield return null;
			}
		}
		SpewContentsOntoGround(vfxTransforms);
		yield return null;
		for (int l = 0; l < vfxTransforms.Count; l++)
		{
			UnityEngine.Object.Destroy(vfxTransforms[l].gameObject);
		}
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && (!IsLocked || interactor.carriedConsumables.KeyBullets > 0 || interactor.carriedConsumables.InfiniteKeys) && !IsSealed)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, BaseOutlineColor, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (IsMirrorChest)
		{
			return 1000f;
		}
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

	public void BreakLock()
	{
		IsSealed = true;
		IsLocked = false;
		IsLockBroken = true;
		if (!pickedUp && LockAnimator != null)
		{
			AkSoundEngine.PostEvent("Play_OBJ_lock_pick_01", base.gameObject);
			LockAnimator.Play(LockBreakAnim);
		}
	}

	private void Unlock()
	{
		IsLocked = false;
		if (!pickedUp && LockAnimator != null)
		{
			LockAnimator.PlayAndDestroyObject(LockOpenAnim);
		}
	}

	public void Interact(PlayerController player)
	{
		if (IsSealed || IsLockBroken)
		{
			return;
		}
		if (IsLocked)
		{
			if (ChestIdentifier == SpecialChestIdentifier.RAT)
			{
				if (player.carriedConsumables.ResourcefulRatKeys <= 0)
				{
					return;
				}
				player.carriedConsumables.ResourcefulRatKeys--;
				Unlock();
				if (pickedUp)
				{
					return;
				}
				if (forceContentIds != null && forceContentIds.Count == 1)
				{
					for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
					{
						PlayerController playerController = GameManager.Instance.AllPlayers[i];
						if ((bool)playerController && playerController.HasPickupID(forceContentIds[0]))
						{
							forceContentIds.Clear();
							if (UnityEngine.Random.value < 0.5f)
							{
								ChestType = GeneralChestType.WEAPON;
								lootTable.lootTable = GameManager.Instance.RewardManager.GunsLootTable;
							}
							else
							{
								ChestType = GeneralChestType.ITEM;
								lootTable.lootTable = GameManager.Instance.RewardManager.ItemsLootTable;
							}
						}
					}
				}
				Open(player);
			}
			else if (LockAnimator == null || !LockAnimator.renderer.enabled)
			{
				Unlock();
				if (!pickedUp)
				{
					Open(player);
				}
			}
			else if (player.carriedConsumables.KeyBullets <= 0 && !player.carriedConsumables.InfiniteKeys)
			{
				if (LockAnimator != null)
				{
					LockAnimator.Play(LockNoKeyAnim);
				}
			}
			else
			{
				if (!player.carriedConsumables.InfiniteKeys)
				{
					player.carriedConsumables.KeyBullets--;
				}
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.CHESTS_UNLOCKED_WITH_KEY_BULLETS, 1f);
				Unlock();
				if (!pickedUp)
				{
					Open(player);
				}
			}
		}
		else if (!pickedUp)
		{
			Open(player);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public void BecomeGlitchChest()
	{
		AkSoundEngine.PostEvent("Play_OBJ_chestglitch_loop_01", base.gameObject);
		base.sprite.usesOverrideMaterial = true;
		Material material = base.sprite.renderer.material;
		material.shader = ShaderCache.Acquire("Brave/Internal/Glitch");
		material.SetFloat("_GlitchInterval", 0.1f);
		material.SetFloat("_DispProbability", 0.4f);
		material.SetFloat("_DispIntensity", 0.01f);
		material.SetFloat("_ColorProbability", 0.4f);
		material.SetFloat("_ColorIntensity", 0.04f);
		m_isGlitchChest = true;
	}

	private void BecomeCoopChest()
	{
		if (ChestIdentifier != SpecialChestIdentifier.RAT)
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleCoopChestTransform());
		}
	}

	private IEnumerator HandleCoopChestTransform(bool unbecome = false)
	{
		if (IsOpen || IsBroken || ChestIdentifier == SpecialChestIdentifier.RAT)
		{
			yield break;
		}
		while (base.spriteAnimator.IsPlaying(base.spriteAnimator.CurrentClip) && !base.spriteAnimator.IsPlaying("coop_chest_knock"))
		{
			yield return null;
		}
		if (unbecome)
		{
			overrideSpawnAnimName = string.Empty;
			overrideOpenAnimName = string.Empty;
			overrideBreakAnimName = string.Empty;
			if ((bool)base.majorBreakable)
			{
				base.majorBreakable.overrideSpriteNameToUseAtZeroHP = string.Empty;
			}
			base.spriteAnimator.Stop();
			base.spriteAnimator.ForceClearCurrentClip();
			IsLocked = m_cachedLockedState;
			if (LockAnimator != null && IsLocked)
			{
				LockAnimator.renderer.enabled = true;
			}
			if ((bool)ShadowSprite && m_cachedShadowSpriteID > -1)
			{
				ShadowSprite.SetSprite(m_cachedShadowSpriteID);
			}
			if (!string.IsNullOrEmpty(spawnAnimName))
			{
				tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(spawnAnimName);
				base.sprite.SetSprite(clipByName.frames[clipByName.frames.Length - 1].spriteId);
			}
			else
			{
				base.sprite.SetSprite(m_cachedSpriteForCoop);
			}
			if (m_cachedCoopManualOffset != IntVector2.Zero)
			{
				IntVector2 intVector = new IntVector2(25, 14);
				PixelCollider primaryPixelCollider = base.specRigidbody.PrimaryPixelCollider;
				if (primaryPixelCollider.ManualOffsetX != m_cachedCoopManualOffset.x || primaryPixelCollider.ManualOffsetY != m_cachedCoopManualOffset.y || primaryPixelCollider.ManualWidth != m_cachedCoopManualDimensions.x || primaryPixelCollider.ManualHeight != m_cachedCoopManualDimensions.y)
				{
					primaryPixelCollider.ManualOffsetX = m_cachedCoopManualOffset.x;
					primaryPixelCollider.ManualOffsetY = m_cachedCoopManualOffset.y;
					primaryPixelCollider.ManualWidth = m_cachedCoopManualDimensions.x;
					primaryPixelCollider.ManualHeight = m_cachedCoopManualDimensions.y;
					float f = (float)(m_cachedCoopManualDimensions.x - intVector.x) / 2f;
					base.transform.position += new Vector3((float)(Mathf.RoundToInt(f) * -1) * 0.0625f, 0f, 0f);
					base.specRigidbody.Reinitialize();
				}
				m_cachedCoopManualOffset = IntVector2.Zero;
				m_cachedCoopManualDimensions = IntVector2.Zero;
			}
			yield break;
		}
		overrideSpawnAnimName = "coop_chest_appear";
		overrideOpenAnimName = "coop_chest_open";
		overrideBreakAnimName = "coop_chest_break";
		m_cachedLockedState = IsLocked;
		if ((bool)ShadowSprite)
		{
			m_cachedShadowSpriteID = ShadowSprite.spriteId;
			ShadowSprite.SetSprite("low_chest_shadow_001");
		}
		IsLocked = false;
		if (LockAnimator != null)
		{
			LockAnimator.renderer.enabled = false;
		}
		if ((bool)base.majorBreakable)
		{
			base.majorBreakable.overrideSpriteNameToUseAtZeroHP = "coop_chest_break001";
		}
		if (m_cachedSpriteForCoop == -1)
		{
			m_cachedSpriteForCoop = base.sprite.spriteId;
		}
		if (!m_temporarilyUnopenable)
		{
			base.spriteAnimator.Play("coop_chest_knock");
		}
		base.sprite.SetSprite("coop_chest_idle_001");
		IntVector2 intVector2 = new IntVector2(3, 0);
		IntVector2 intVector3 = new IntVector2(25, 14);
		PixelCollider primaryPixelCollider2 = base.specRigidbody.PrimaryPixelCollider;
		if (primaryPixelCollider2.ManualOffsetX != intVector2.x || primaryPixelCollider2.ManualOffsetY != intVector2.y || primaryPixelCollider2.ManualWidth != intVector3.x || primaryPixelCollider2.ManualHeight != intVector3.y)
		{
			m_cachedCoopManualOffset = new IntVector2(primaryPixelCollider2.ManualOffsetX, primaryPixelCollider2.ManualOffsetY);
			m_cachedCoopManualDimensions = new IntVector2(primaryPixelCollider2.ManualWidth, primaryPixelCollider2.ManualHeight);
			primaryPixelCollider2.ManualOffsetX = intVector2.x;
			primaryPixelCollider2.ManualOffsetY = intVector2.y;
			primaryPixelCollider2.ManualWidth = intVector3.x;
			primaryPixelCollider2.ManualHeight = intVector3.y;
			float f2 = (float)(m_cachedCoopManualDimensions.x - primaryPixelCollider2.ManualWidth) / 2f;
			base.transform.position += new Vector3((float)Mathf.RoundToInt(f2) * 0.0625f, 0f, 0f);
			base.specRigidbody.Reinitialize();
		}
	}

	private void UnbecomeCoopChest()
	{
		GameManager.Instance.Dungeon.StartCoroutine(HandleCoopChestTransform(true));
	}

	public void MaybeBecomeMimic()
	{
		if (IsTruthChest || IsRainbowChest || lootTable.CompletesSynergy)
		{
			return;
		}
		m_isMimic = false;
		bool flag = false;
		if (!GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_COMPLETE) && !HasDroppedResourcefulRatNoteThisSession && !DoneResourcefulRatMimicThisSession && !GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_06) && GameStatsManager.Instance.GetFlag(GungeonFlags.RESOURCEFUL_RAT_NOTE_05) && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_LICH) && GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON)
		{
			DoneResourcefulRatMimicThisSession = true;
			flag = true;
		}
		if (string.IsNullOrEmpty(MimicGuid))
		{
			return;
		}
		flag |= GameManager.Instance.Dungeon.sharedSettingsPrefab.RandomShouldBecomeMimic(overrideMimicChance);
		GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
		flag |= lastLoadedLevelDefinition != null && lastLoadedLevelDefinition.lastSelectedFlowEntry != null && lastLoadedLevelDefinition.lastSelectedFlowEntry.levelMode == FlowLevelEntryMode.ALL_MIMICS;
		if (PassiveItem.IsFlagSetAtAll(typeof(MimicToothNecklaceItem)) && ChestIdentifier == SpecialChestIdentifier.RAT)
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		if (PassiveItem.IsFlagSetAtAll(typeof(MimicToothNecklaceItem)))
		{
			Unlock();
		}
		if (!PassiveItem.IsFlagSetAtAll(typeof(MimicRingItem)))
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
		while (m_isMimic)
		{
			yield return new WaitForSeconds(preMimicIdleAnimDelay);
			if (!m_isMimic)
			{
				yield break;
			}
			while (m_IsCoopMode)
			{
				yield return null;
			}
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
		if (m_isMimic && !m_IsCoopMode && !IsMirrorChest)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.PREFIRE_ON_MIMIC);
			DetermineContents(GameManager.Instance.PrimaryPlayer);
			DoMimicTransformation(contents);
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
		if (!m_configured)
		{
			m_room.Entered += RoomEntered;
		}
		Initialize();
		if (!m_configured)
		{
			RegisterChestOnMinimap(room);
		}
		m_configured = true;
		PossiblyCreateBowler(false);
	}

	private void RoomEntered(PlayerController enterer)
	{
		if (m_IsCoopMode && GameManager.HasInstance && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.healthHaver.IsAlive && (bool)GameManager.Instance.SecondaryPlayer && GameManager.Instance.SecondaryPlayer.healthHaver.IsAlive)
		{
			UnbecomeCoopChest();
		}
		if (!m_IsCoopMode && !IsOpen && !IsBroken && !m_hasBeenCheckedForFuses && !PreventFuse && ChestIdentifier != SpecialChestIdentifier.RAT)
		{
			m_hasBeenCheckedForFuses = true;
			float num = 0.02f;
			num += (float)PlayerStats.GetTotalCurse() * 0.05f;
			num += (float)PlayerStats.GetTotalCoolness() * -0.025f;
			num = Mathf.Max(0.01f, Mathf.Clamp01(num));
			if (lootTable != null && lootTable.CompletesSynergy)
			{
				num = 1f;
			}
			if (UnityEngine.Random.value < num)
			{
				TriggerCountdownTimer();
				AkSoundEngine.PostEvent("Play_OBJ_fuse_loop_01", base.gameObject);
			}
		}
	}

	protected override void OnDestroy()
	{
		MajorBreakable obj = base.majorBreakable;
		obj.OnDamaged = (Action<float>)Delegate.Remove(obj.OnDamaged, new Action<float>(OnDamaged));
		StaticReferenceManager.AllChests.Remove(this);
		base.OnDestroy();
		AkSoundEngine.PostEvent("Stop_SND_OBJ", base.gameObject);
		AkSoundEngine.PostEvent("stop_obj_fuse_loop_01", base.gameObject);
	}
}
