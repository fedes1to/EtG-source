using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ResourcefulRatMinesHiddenTrapdoor : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public PrototypeDungeonRoom TargetMinecartRoom;

	public PrototypeDungeonRoom FirstSecretRoom;

	public PrototypeDungeonRoom SecondSecretRoom;

	public TileIndexGrid OverridePitGrid;

	public Material BlendMaterial;

	public Material LockBlendMaterial;

	public InteractableLock Lock;

	public Texture2D StoneFloorTex;

	public Texture2D DirtFloorTex;

	public float ExplosionReactDistance = 8f;

	public SpeculativeRigidbody FlightCollider;

	[NonSerialized]
	public float RevealPercentage;

	public GameObject MinimapIcon;

	private bool m_hasCreatedRoom;

	private RoomHandler m_createdRoom;

	private Texture2D m_blendTex;

	private Color[] m_blendTexColors;

	private bool m_blendTexDirty;

	private HashSet<IntVector2> m_goopedSpots = new HashSet<IntVector2>();

	private float m_timeHovering;

	private bool m_revealing;

	private IEnumerator Start()
	{
		m_blendTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
		m_blendTexColors = new Color[4096];
		for (int i = 0; i < m_blendTexColors.Length; i++)
		{
			m_blendTexColors[i] = Color.black;
		}
		m_blendTex.SetPixels(m_blendTexColors);
		m_blendTex.Apply();
		BlendMaterial.SetFloat("_BlendMin", RevealPercentage);
		BlendMaterial.SetTexture("_BlendTex", m_blendTex);
		BlendMaterial.SetVector("_BaseWorldPosition", new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z, 0f));
		LockBlendMaterial.SetFloat("_BlendMin", RevealPercentage);
		LockBlendMaterial.SetTexture("_BlendTex", m_blendTex);
		LockBlendMaterial.SetVector("_BaseWorldPosition", new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z, 0f));
		RoomHandler parentRoom = base.transform.position.GetAbsoluteRoom();
		BlendMaterial.SetTexture("_SubTex", (parentRoom.RoomVisualSubtype != 1) ? StoneFloorTex : DirtFloorTex);
		LockBlendMaterial.SetTexture("_SubTex", (parentRoom.RoomVisualSubtype != 1) ? StoneFloorTex : DirtFloorTex);
		if (!StaticReferenceManager.AllRatTrapdoors.Contains(this))
		{
			StaticReferenceManager.AllRatTrapdoors.Add(this);
		}
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if ((bool)FlightCollider)
		{
			SpeculativeRigidbody flightCollider = FlightCollider;
			flightCollider.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(flightCollider.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleFlightCollider));
		}
		if (!string.IsNullOrEmpty(GameManager.Instance.Dungeon.NormalRatGUID))
		{
			for (int j = 0; j < 3; j++)
			{
				parentRoom.AddSpecificEnemyToRoomProcedurally(GameManager.Instance.Dungeon.NormalRatGUID);
			}
		}
	}

	private void HandleFlightCollider(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (GameManager.Instance.IsLoadingLevel || Lock.IsLocked || !m_hasCreatedRoom)
		{
			return;
		}
		PlayerController component = specRigidbody.GetComponent<PlayerController>();
		if ((bool)component && component.IsFlying)
		{
			m_timeHovering += BraveTime.DeltaTime;
			if (m_timeHovering > 1f)
			{
				component.ForceFall();
				m_timeHovering = 0f;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StaticReferenceManager.AllRatTrapdoors.Remove(this);
	}

	public void OnNearbyExplosion(Vector3 center)
	{
		float sqrMagnitude = (base.transform.position.XY() + new Vector2(2f, 2f) - center.XY()).sqrMagnitude;
		if (sqrMagnitude < ExplosionReactDistance * ExplosionReactDistance)
		{
			float revealPercentage = RevealPercentage;
			RevealPercentage = Mathf.Max(revealPercentage, Mathf.Min(0.3f, RevealPercentage + 0.125f));
			UpdatePlayerDustups();
			BlendMaterial.SetFloat("_BlendMin", RevealPercentage);
			LockBlendMaterial.SetFloat("_BlendMin", RevealPercentage);
		}
	}

	public void OnBlank()
	{
		if (GameManager.Instance.BestActivePlayer.CurrentRoom == base.transform.position.GetAbsoluteRoom())
		{
			float revealPercentage = RevealPercentage;
			RevealPercentage = Mathf.Max(revealPercentage, Mathf.Min(0.3f, RevealPercentage + 0.5f));
			UpdatePlayerDustups();
			BlendMaterial.SetFloat("_BlendMin", RevealPercentage);
			LockBlendMaterial.SetFloat("_BlendMin", RevealPercentage);
		}
	}

	private void UpdatePlayerPositions()
	{
		if (RevealPercentage >= 1f)
		{
			return;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			Vector2 vector = playerController.SpriteBottomCenter;
			bool flag = false;
			if (vector.x > base.transform.position.x && vector.y > base.transform.position.y && vector.x < base.transform.position.x + 4f && vector.y < base.transform.position.y + 4f && (playerController.IsGrounded || playerController.IsFlying) && !playerController.IsGhost)
			{
				flag = true;
				playerController.OverrideDustUp = ResourceCache.Acquire("Global VFX/VFX_RatDoor_DustUp") as GameObject;
				if (playerController.Velocity.magnitude > 0f)
				{
					Vector2 vector2 = vector - base.transform.position.XY();
					IntVector2 pxCenter = new IntVector2(Mathf.FloorToInt(vector2.x * 16f), Mathf.FloorToInt(vector2.y * 16f));
					SoftUpdateRadius(pxCenter, 10, 2f * Time.deltaTime);
				}
			}
			if (!flag && (bool)playerController.OverrideDustUp && playerController.OverrideDustUp.name.StartsWith("VFX_RatDoor_DustUp", StringComparison.Ordinal))
			{
				playerController.OverrideDustUp = null;
			}
		}
	}

	private float CalcAvgRevealedness()
	{
		if (RevealPercentage >= 1f)
		{
			return 1f;
		}
		float num = 0f;
		for (int i = 0; i < 64; i++)
		{
			for (int j = 0; j < 64; j++)
			{
				float r = m_blendTexColors[j * 64 + i].r;
				num += Mathf.Max(r, RevealPercentage);
			}
		}
		return num / 4096f;
	}

	private bool SoftUpdateRadius(IntVector2 pxCenter, int radius, float amt)
	{
		bool result = false;
		for (int i = pxCenter.x - radius; i < pxCenter.x + radius; i++)
		{
			for (int j = pxCenter.y - radius; j < pxCenter.y + radius; j++)
			{
				if (i > 0 && j > 0 && i < 64 && j < 64)
				{
					Color color = m_blendTexColors[j * 64 + i];
					float num = Vector2.Distance(pxCenter.ToVector2(), new Vector2(i, j));
					float num2 = Mathf.Clamp01(((float)radius - num) / (float)radius);
					float num3 = Mathf.Clamp01(color.r + amt * num2);
					if (num3 != color.r)
					{
						color.r = num3;
						m_blendTexColors[j * 64 + i] = color;
						result = true;
						m_blendTexDirty = true;
					}
				}
			}
		}
		return result;
	}

	private void UpdateGoopedCells()
	{
		if (RevealPercentage >= 1f)
		{
			return;
		}
		Vector2 vector = base.transform.position.XY();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				Vector2 vector2 = new Vector2((float)i / 4f, (float)j / 4f) + vector;
				IntVector2 intVector = (vector2 / DeadlyDeadlyGoopManager.GOOP_GRID_SIZE).ToIntVector2(VectorConversions.Floor);
				if (!DeadlyDeadlyGoopManager.allGoopPositionMap.ContainsKey(intVector) || m_goopedSpots.Contains(intVector))
				{
					continue;
				}
				m_goopedSpots.Add(intVector);
				IntVector2 intVector2 = new IntVector2(i * 4, j * 4);
				for (int k = intVector2.x; k < intVector2.x + 4; k++)
				{
					for (int l = intVector2.y; l < intVector2.y + 4; l++)
					{
						m_blendTexColors[l * 64 + k] = new Color(1f, 1f, 1f, 1f);
					}
				}
				m_blendTexDirty = true;
			}
		}
	}

	private IEnumerator GraduallyReveal()
	{
		if (!m_revealing)
		{
			m_revealing = true;
			while (RevealPercentage < 1f)
			{
				RevealPercentage = Mathf.Clamp01(RevealPercentage + Time.deltaTime);
				UpdatePlayerDustups();
				BlendMaterial.SetFloat("_BlendMin", RevealPercentage);
				LockBlendMaterial.SetFloat("_BlendMin", RevealPercentage);
				yield return null;
			}
		}
	}

	private void UpdatePlayerDustups()
	{
		if (!(RevealPercentage >= 1f))
		{
			return;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && (bool)playerController.OverrideDustUp && playerController.OverrideDustUp.name.StartsWith("VFX_RatDoor_DustUp", StringComparison.Ordinal))
			{
				playerController.OverrideDustUp = null;
			}
		}
	}

	private void LateUpdate()
	{
		if (RevealPercentage < 1f)
		{
			UpdateGoopedCells();
			UpdatePlayerPositions();
			if (!m_revealing)
			{
				float num = CalcAvgRevealedness();
				if (num > 0.33f)
				{
					StartCoroutine(GraduallyReveal());
				}
			}
			if (m_blendTexDirty)
			{
				m_blendTex.SetPixels(m_blendTexColors);
				m_blendTex.Apply();
			}
		}
		else if (Lock.Suppress)
		{
			Lock.Suppress = false;
			Minimap.Instance.RegisterRoomIcon(base.transform.position.GetAbsoluteRoom(), MinimapIcon);
		}
		else if (!m_hasCreatedRoom && !Lock.IsLocked)
		{
			Open();
		}
	}

	public void Open()
	{
		if (m_hasCreatedRoom)
		{
			return;
		}
		if (!m_hasCreatedRoom)
		{
			m_hasCreatedRoom = true;
			List<PrototypeDungeonRoom> list = new List<PrototypeDungeonRoom>();
			list.Add(TargetMinecartRoom);
			list.Add(FirstSecretRoom);
			list.Add(SecondSecretRoom);
			List<IntVector2> list2 = new List<IntVector2>();
			list2.Add(IntVector2.Zero);
			list2.Add(new IntVector2(73, 17));
			list2.Add(new IntVector2(73, 36));
			List<RoomHandler> list3 = GameManager.Instance.Dungeon.AddRuntimeRoomCluster(list, list2, ActuallyMakeAllTheFacewallsLookTheSameInTheRightSpots, DungeonData.LightGenerationStyle.RAT_HALLWAY);
			m_createdRoom = list3[0];
			for (int i = 0; i < list3.Count; i++)
			{
				list3[i].PreventMinimapUpdates = true;
			}
		}
		if (m_createdRoom != null)
		{
			AssignPitfallRoom(m_createdRoom);
			base.spriteAnimator.Play();
			StartCoroutine(HandleFlaggingCells());
		}
	}

	private IEnumerator HandleFlaggingCells()
	{
		IntVector2 basePosition = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = 1; i < placeableWidth - 1; i++)
		{
			for (int j = 1; j < placeableHeight - 1; j++)
			{
				IntVector2 cellPos = new IntVector2(i, j) + basePosition;
				DeadlyDeadlyGoopManager.ForceClearGoopsInCell(cellPos);
			}
		}
		yield return new WaitForSeconds(0.4f);
		for (int k = 1; k < placeableWidth - 1; k++)
		{
			for (int l = 1; l < placeableHeight - 1; l++)
			{
				IntVector2 key = new IntVector2(k, l) + basePosition;
				CellData cellData = GameManager.Instance.Dungeon.data[key];
				cellData.fallingPrevented = false;
			}
		}
	}

	public void ActuallyMakeAllTheFacewallsLookTheSameInTheRightSpots(RoomHandler target)
	{
		if (target.area.prototypeRoom != TargetMinecartRoom)
		{
			return;
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = 0; i < target.area.dimensions.x; i++)
		{
			for (int j = 0; j < target.area.dimensions.y + 2; j++)
			{
				IntVector2 intVector = target.area.basePosition + new IntVector2(i, j);
				if (!data.CheckInBoundsAndValid(intVector))
				{
					continue;
				}
				CellData cellData = data[intVector];
				if (data.isAnyFaceWall(intVector.x, intVector.y))
				{
					TilesetIndexMetadata.TilesetFlagType key = TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER;
					if (data.isFaceWallLower(intVector.x, intVector.y))
					{
						key = TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER;
					}
					int indexFromTupleArray = SecretRoomUtility.GetIndexFromTupleArray(cellData, SecretRoomUtility.metadataLookupTableRef[key], cellData.cellVisualData.roomVisualTypeIndex, 0f);
					cellData.cellVisualData.faceWallOverrideIndex = indexFromTupleArray;
				}
			}
		}
	}

	private void AssignPitfallRoom(RoomHandler target)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = 0; i < placeableWidth; i++)
		{
			for (int j = -2; j < placeableHeight; j++)
			{
				IntVector2 key = new IntVector2(i, j) + intVector;
				CellData cellData = GameManager.Instance.Dungeon.data[key];
				cellData.targetPitfallRoom = target;
				cellData.forceAllowGoop = false;
			}
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = 1; i < placeableWidth - 1; i++)
		{
			for (int j = 1; j < placeableHeight - 1; j++)
			{
				IntVector2 key = new IntVector2(i, j) + intVector;
				CellData cellData = GameManager.Instance.Dungeon.data[key];
				int num = -1;
				num = ((i != 1) ? ((j != 1) ? OverridePitGrid.topRightIndices.GetIndexByWeight() : OverridePitGrid.bottomRightIndices.GetIndexByWeight()) : ((j != 1) ? OverridePitGrid.topLeftIndices.GetIndexByWeight() : OverridePitGrid.bottomLeftIndices.GetIndexByWeight()));
				cellData.cellVisualData.pitOverrideIndex = num;
				cellData.forceAllowGoop = true;
				cellData.type = CellType.PIT;
				cellData.fallingPrevented = true;
				cellData.cellVisualData.containsObjectSpaceStamp = true;
				cellData.cellVisualData.containsWallSpaceStamp = true;
			}
		}
	}
}
