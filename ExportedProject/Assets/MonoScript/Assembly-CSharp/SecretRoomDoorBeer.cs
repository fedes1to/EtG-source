using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class SecretRoomDoorBeer : MonoBehaviour
{
	public static List<SecretRoomDoorBeer> AllSecretRoomDoors;

	public DungeonDoorSubsidiaryBlocker subsidiaryBlocker;

	public RuntimeExitDefinition exitDef;

	public RoomHandler linkedRoom;

	public SecretRoomManager manager;

	public SecretRoomExitData collider;

	private MajorBreakable m_breakable;

	private tk2dSprite m_breakVfxSprite;

	public List<BreakableChunk> wallChunks;

	private bool m_hasSnitchedBricked;

	private GameObject m_snitchBrick;

	public BreakFrame[] m_breakFrames;

	private bool m_hasBeenAmygdalaed;

	private float m_amygdalaCheckTimer;

	private GameObject m_amygdala;

	private void Awake()
	{
		if (AllSecretRoomDoors == null)
		{
			AllSecretRoomDoors = new List<SecretRoomDoorBeer>();
		}
		AllSecretRoomDoors.Add(this);
	}

	private void Start()
	{
		if (linkedRoom != null)
		{
			linkedRoom.Entered += HandlePlayerEnteredLinkedRoom;
		}
	}

	private void Update()
	{
		if (m_hasBeenAmygdalaed)
		{
			return;
		}
		m_amygdalaCheckTimer -= BraveTime.DeltaTime;
		if (!(m_amygdalaCheckTimer < 0f))
		{
			return;
		}
		m_amygdalaCheckTimer = 1f;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (GameManager.Instance.AllPlayers[i].HasActiveBonusSynergy(CustomSynergyType.INSIGHT))
			{
				RoomHandler roomHandler = ((exitDef.upstreamRoom != linkedRoom) ? exitDef.upstreamRoom : exitDef.downstreamRoom);
				if (roomHandler == null || !(roomHandler.secretRoomManager != null) || roomHandler.secretRoomManager.revealStyle != SecretRoomManager.SecretRoomRevealStyle.FireplacePuzzle)
				{
					GenerateAmygdala();
					m_hasBeenAmygdalaed = true;
					break;
				}
			}
		}
	}

	private void GenerateAmygdala()
	{
		string resourceName = string.Empty;
		Vector2 vector = Vector2.zero;
		switch (exitDef.GetDirectionFromRoom(linkedRoom))
		{
		case DungeonData.Direction.NORTH:
			resourceName = "Global VFX/Amygdala_South";
			vector = new Vector2(0f, 2f);
			break;
		case DungeonData.Direction.EAST:
			resourceName = "Global VFX/Amygdala_West";
			vector = new Vector2(-0.25f, 2f);
			break;
		case DungeonData.Direction.SOUTH:
			resourceName = "Global VFX/Amygdala_North";
			vector = new Vector2(0f, 1.5f);
			break;
		case DungeonData.Direction.WEST:
			resourceName = "Global VFX/Amygdala_East";
			vector = new Vector2(0f, 2f);
			break;
		}
		m_amygdala = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire(resourceName));
		m_amygdala.transform.position = base.transform.position + vector.ToVector3ZUp();
	}

	private void OnDestroy()
	{
		if (AllSecretRoomDoors != null)
		{
			AllSecretRoomDoors.Remove(this);
		}
	}

	public void SetBreakable()
	{
		if ((bool)m_breakable)
		{
			m_breakable.IsSecretDoor = false;
		}
	}

	private void HandlePlayerEnteredLinkedRoom(PlayerController p)
	{
		if (exitDef != null)
		{
			RoomHandler roomHandler = ((exitDef.upstreamRoom != linkedRoom) ? exitDef.upstreamRoom : exitDef.downstreamRoom);
			if (roomHandler != null && roomHandler.secretRoomManager != null && roomHandler.secretRoomManager.revealStyle == SecretRoomManager.SecretRoomRevealStyle.FireplacePuzzle)
			{
				return;
			}
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (PassiveItem.ActiveFlagItems.ContainsKey(GameManager.Instance.AllPlayers[i]) && PassiveItem.ActiveFlagItems[GameManager.Instance.AllPlayers[i]].ContainsKey(typeof(SnitchBrickItem)) && !m_hasSnitchedBricked)
			{
				DoSnitchBrick();
			}
		}
	}

	private void DoSnitchBrick()
	{
		m_hasSnitchedBricked = true;
		GameObject original = (GameObject)ResourceCache.Acquire("Global VFX/VFX_SnitchBrick");
		Vector3 position = collider.colliderObject.GetComponent<SpeculativeRigidbody>().UnitCenter;
		position += DungeonData.GetIntVector2FromDirection(exitDef.downstreamExit.referencedExit.exitDirection).ToVector3();
		m_snitchBrick = UnityEngine.Object.Instantiate(original, position, Quaternion.identity);
	}

	public void InitializeFireplace()
	{
	}

	public void InitializeShootToBreak()
	{
		SpeculativeRigidbody component = collider.colliderObject.GetComponent<SpeculativeRigidbody>();
		component.PreventPiercing = true;
		m_breakable = collider.colliderObject.AddComponent<MajorBreakable>();
		m_breakable.IsSecretDoor = true;
		m_breakable.spawnShards = false;
		m_breakable.HitPoints = 25f;
		m_breakable.EnemyDamageOverride = 8;
		GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
		if (lastLoadedLevelDefinition != null)
		{
			m_breakable.HitPoints *= lastLoadedLevelDefinition.secretDoorHealthMultiplier;
		}
		MajorBreakable breakable = m_breakable;
		breakable.OnDamaged = (Action<float>)Delegate.Combine(breakable.OnDamaged, new Action<float>(OnDamaged));
		MajorBreakable breakable2 = m_breakable;
		breakable2.OnBreak = (Action)Delegate.Combine(breakable2.OnBreak, new Action(OnBreak));
		GameObject gameObject = UnityEngine.Object.Instantiate(BraveResources.Load<GameObject>("Global VFX/VFX_Secret_Door_Crack_01"));
		m_breakVfxSprite = gameObject.GetComponent<tk2dSprite>();
		m_breakFrames = new BreakFrame[2]
		{
			new BreakFrame
			{
				healthPercentage = 50f,
				sprite = "secret_door_crack_generic{0}_001"
			},
			new BreakFrame
			{
				healthPercentage = 10f,
				sprite = "secret_door_crack_generic{0}_002"
			}
		};
		if (collider.exitDirection == DungeonData.Direction.SOUTH)
		{
			m_breakVfxSprite.IsPerpendicular = true;
			m_breakVfxSprite.transform.position = component.UnitBottomLeft;
			m_breakVfxSprite.HeightOffGround = -1.45f;
			m_breakVfxSprite.UpdateZDepth();
		}
		else
		{
			m_breakVfxSprite.IsPerpendicular = false;
			m_breakVfxSprite.HeightOffGround = 3.2f;
			if (collider.exitDirection == DungeonData.Direction.NORTH)
			{
				m_breakVfxSprite.transform.position = component.UnitBottomLeft;
			}
			else
			{
				m_breakVfxSprite.transform.position = component.UnitBottomLeft + new Vector2(0f, 1f);
			}
			if (collider.exitDirection == DungeonData.Direction.EAST)
			{
				m_breakVfxSprite.transform.position = m_breakVfxSprite.transform.position + new Vector3(-1f, 0f, 0f);
			}
			m_breakVfxSprite.UpdateZDepth();
		}
		m_breakVfxSprite.renderer.enabled = false;
		if (GameManager.Instance.InTutorial)
		{
			m_breakable.MaxHitPoints = m_breakable.HitPoints;
			m_breakable.HitPoints = 2f;
			m_breakable.ApplyDamage(1f, Vector2.zero, false, true, true);
		}
	}

	public void OnDamaged(float damage)
	{
		for (int num = m_breakFrames.Length - 1; num >= 0; num--)
		{
			if ((m_breakable.MinHits <= 0 || num < m_breakable.NumHits) && m_breakable.GetCurrentHealthPercentage() <= m_breakFrames[num].healthPercentage / 100f)
			{
				if ((bool)m_breakVfxSprite)
				{
					m_breakVfxSprite.renderer.enabled = true;
					m_breakVfxSprite.SetSprite(GetFrameName(m_breakFrames[num].sprite, collider.exitDirection));
				}
				return;
			}
		}
		if ((bool)m_breakVfxSprite)
		{
			m_breakVfxSprite.renderer.enabled = false;
		}
	}

	public void OnBreak()
	{
		if (m_snitchBrick != null)
		{
			LootEngine.DoDefaultItemPoof(m_snitchBrick.GetComponentInChildren<tk2dBaseSprite>().WorldCenter);
			UnityEngine.Object.Destroy(m_snitchBrick);
		}
		if ((bool)m_amygdala)
		{
			UnityEngine.Object.Destroy(m_amygdala);
			m_amygdala = null;
		}
		BreakOpen();
	}

	public void BreakOpen()
	{
		if ((bool)m_breakVfxSprite)
		{
			UnityEngine.Object.Destroy(m_breakVfxSprite);
		}
		AkSoundEngine.PostEvent("Play_UI_secret_reveal_01", base.gameObject);
		manager.IsOpen = true;
		manager.HandleDoorBrokenOpen(this);
		collider.colliderObject.GetComponent<SpeculativeRigidbody>().enabled = false;
		if (wallChunks != null)
		{
			for (int i = 0; i < wallChunks.Count; i++)
			{
				wallChunks[i].gameObject.SetActive(true);
				wallChunks[i].Trigger();
			}
		}
	}

	public void GeneratePotentiallyNecessaryShards()
	{
		GameObject secretRoomWallShardCollection = GameManager.Instance.Dungeon.roomMaterialDefinitions[manager.room.RoomVisualSubtype].GetSecretRoomWallShardCollection();
		if (!(secretRoomWallShardCollection != null))
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(secretRoomWallShardCollection);
		gameObject.transform.position = base.transform.position;
		while (gameObject.transform.childCount > 0)
		{
			GameObject gameObject2 = gameObject.transform.GetChild(0).gameObject;
			gameObject2.transform.parent = base.transform;
			if (wallChunks == null)
			{
				wallChunks = new List<BreakableChunk>();
			}
			gameObject2.SetActive(false);
			wallChunks.Add(gameObject2.GetComponent<BreakableChunk>());
		}
	}

	private string GetFrameName(string name, DungeonData.Direction dir)
	{
		if (name.Contains("{0}"))
		{
			string arg;
			switch (dir)
			{
			case DungeonData.Direction.WEST:
				arg = "_left_top";
				break;
			case DungeonData.Direction.NORTH:
				arg = "_top_top";
				break;
			case DungeonData.Direction.EAST:
				arg = "_right_top";
				break;
			default:
				arg = string.Empty;
				break;
			}
			return string.Format(name, arg);
		}
		return name;
	}
}
