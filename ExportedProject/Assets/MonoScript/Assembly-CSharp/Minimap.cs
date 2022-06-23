using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using tk2dRuntime.TileMap;
using UnityEngine;

public class Minimap : MonoBehaviour
{
	public enum MinimapDisplayMode
	{
		NEVER,
		ALWAYS,
		FADE_ON_ROOM_SEAL
	}

	[NonSerialized]
	public bool PreventAllTeleports;

	public tk2dTileMap tilemap;

	public TileIndexGrid indexGrid;

	public TileIndexGrid darkIndexGrid;

	public TileIndexGrid redIndexGrid;

	public TileIndexGrid CurrentRoomBorderIndexGrid;

	public Camera cameraRef;

	public MinimapUIController UIMinimap;

	public float targetSaturation = 0.3f;

	[NonSerialized]
	public float currentXRectFactor = 1f;

	[NonSerialized]
	public float currentYRectFactor = 1f;

	private bool[,] m_simplifiedData;

	private List<Tuple<Transform, Renderer>> m_playerMarkers = new List<Tuple<Transform, Renderer>>();

	private static float SCALE_FACTOR = 15f;

	[SerializeField]
	private Material m_mapMaskMaterial;

	private Texture m_itemsMaskTex;

	private Texture2D m_whiteTex;

	private Dictionary<RoomHandler, List<GameObject>> roomToIconsMap = new Dictionary<RoomHandler, List<GameObject>>();

	private Dictionary<RoomHandler, bool> roomHasMovedTeleportIcon = new Dictionary<RoomHandler, bool>();

	private Dictionary<RoomHandler, GameObject> roomToTeleportIconMap = new Dictionary<RoomHandler, GameObject>();

	private Dictionary<RoomHandler, Vector3> roomToInitialTeleportIconPositionMap = new Dictionary<RoomHandler, Vector3>();

	public List<RoomHandler> roomsContainingTeleporters = new List<RoomHandler>();

	private Dictionary<RoomHandler, GameObject> roomToUnknownIconMap = new Dictionary<RoomHandler, GameObject>();

	private Vector3 m_cameraBasePosition;

	private Vector3 m_cameraPanOffset;

	private float m_cameraOrthoBase;

	private static float m_cameraOrthoOffset;

	private float m_currentMinimapZoom;

	private bool m_isFullscreen;

	private static Minimap m_instance;

	private bool m_isAutoPanning;

	public bool TemporarilyPreventMinimap;

	private bool[,] m_revealProcessedMap;

	protected bool m_shouldBuildTilemap;

	protected bool m_isInitialized;

	private float m_cachedFadeValue = 1f;

	private bool m_isFaded;

	private bool m_isTransitioning;

	public MinimapDisplayMode MinimapMode
	{
		get
		{
			if (PreventMinimap)
			{
				return MinimapDisplayMode.NEVER;
			}
			if (GameManager.Instance.IsFoyer)
			{
				return MinimapDisplayMode.NEVER;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.RESOURCEFUL_RAT)
			{
				return MinimapDisplayMode.NEVER;
			}
			if ((bool)GameManager.Instance.BestActivePlayer && GameManager.Instance.BestActivePlayer.CurrentRoom != null && GameManager.Instance.BestActivePlayer.CurrentRoom.PreventMinimapUpdates)
			{
				return MinimapDisplayMode.NEVER;
			}
			return GameManager.Options.MinimapDisplayMode;
		}
	}

	public static bool DoMinimap
	{
		get
		{
			if (GameManager.Instance.IsLoadingLevel)
			{
				return false;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				return false;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
			{
				return false;
			}
			if (TextBoxManager.ExtantTextBoxVisible)
			{
				return false;
			}
			if (TimeTubeCreditsController.IsTimeTubing)
			{
				return false;
			}
			return true;
		}
	}

	public Dictionary<RoomHandler, GameObject> RoomToTeleportMap
	{
		get
		{
			return roomToTeleportIconMap;
		}
	}

	public bool IsFullscreen
	{
		get
		{
			return m_isFullscreen;
		}
	}

	public static Minimap Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = UnityEngine.Object.FindObjectOfType<Minimap>();
			}
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance != null;
		}
	}

	public bool HeldOpen { get; set; }

	public bool this[int x, int y]
	{
		get
		{
			if (m_simplifiedData != null && x >= 0 && y >= 0 && x < m_simplifiedData.GetLength(0) && y < m_simplifiedData.GetLength(1))
			{
				return m_simplifiedData[x, y];
			}
			return false;
		}
	}

	public bool IsPanning
	{
		get
		{
			return m_isAutoPanning;
		}
	}

	private bool PreventMinimap
	{
		get
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.RESOURCEFUL_RAT)
			{
				return true;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				return true;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				return true;
			}
			if ((bool)GameUIRoot.Instance && GameUIRoot.Instance.DisplayingConversationBar)
			{
				return true;
			}
			return TemporarilyPreventMinimap;
		}
	}

	private void AssignColorToTile(int ix, int iy, int layer, Color32 color, tk2dTileMap map)
	{
		if (!map.HasColorChannel())
		{
			map.CreateColorChannel();
		}
		ColorChannel colorChannel = map.ColorChannel;
		map.data.Layers[layer].useColor = true;
		colorChannel.SetColor(ix, iy, color);
	}

	private void ToggleMinimapRat(bool fullscreen, bool holdOpen = false)
	{
		cameraRef.cullingMask = 0;
		GameUIRoot.Instance.notificationController.ForceHide();
		GameUIRoot.Instance.ToggleAllDefaultLabels(!fullscreen, "minimap");
		m_isFullscreen = fullscreen;
		HeldOpen = holdOpen;
		if (fullscreen)
		{
			m_cachedFadeValue = ((!m_isFaded) ? 1 : 0);
			m_mapMaskMaterial.SetFloat("_Fade", 1f);
			currentXRectFactor = 1f;
			currentYRectFactor = 1f;
		}
		else
		{
			m_mapMaskMaterial.SetFloat("_Fade", m_cachedFadeValue);
			currentXRectFactor = 0.25f;
			currentYRectFactor = 0.25f;
			BraveInput.ConsumeAllAcrossInstances(GungeonActions.GungeonActionType.Shoot);
		}
		Shader.SetGlobalFloat("_FullMapActive", fullscreen ? 1 : 0);
		UpdateScale();
		if (fullscreen)
		{
			AkSoundEngine.PostEvent("Play_UI_map_open_01", base.gameObject);
		}
		m_cameraPanOffset = Vector3.zero;
		if (fullscreen)
		{
			Pixelator.Instance.FadeColor = Color.black;
			Pixelator.Instance.fade = 0.3f;
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			GameUIRoot.Instance.UnfoldGunventory(GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameUIRoot.Instance.ToggleItemPanels(false);
			}
			UIMinimap.ToggleState(true);
		}
		else
		{
			Pixelator.Instance.FadeColor = Color.black;
			Pixelator.Instance.fade = 1f;
			GameUIRoot.Instance.ShowCoreUI(string.Empty);
			GameUIRoot.Instance.RefoldGunventory();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameUIRoot.Instance.ToggleItemPanels(true);
			}
			UIMinimap.ToggleState(false);
		}
		if (m_isFullscreen)
		{
			m_cameraBasePosition = GetCameraBasePosition();
			cameraRef.transform.position = m_cameraBasePosition + m_cameraPanOffset;
		}
	}

	public void ToggleMinimap(bool fullscreen, bool holdOpen = false)
	{
		if (!fullscreen)
		{
			HeldOpen = false;
		}
		bool flag = GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.RESOURCEFUL_RAT;
		if (PreventMinimap && !flag)
		{
			return;
		}
		if (flag)
		{
			cameraRef.cullingMask = 0;
		}
		GameUIRoot.Instance.notificationController.ForceHide();
		GameUIRoot.Instance.ToggleAllDefaultLabels(!fullscreen, "minimap");
		m_isFullscreen = fullscreen;
		HeldOpen = holdOpen;
		if (fullscreen)
		{
			m_cachedFadeValue = ((!m_isFaded) ? 1 : 0);
			m_mapMaskMaterial.SetFloat("_Fade", 1f);
			currentXRectFactor = 1f;
			currentYRectFactor = 1f;
		}
		else
		{
			m_mapMaskMaterial.SetFloat("_Fade", m_cachedFadeValue);
			currentXRectFactor = 0.25f;
			currentYRectFactor = 0.25f;
			BraveInput.ConsumeAllAcrossInstances(GungeonActions.GungeonActionType.Shoot);
		}
		Shader.SetGlobalFloat("_FullMapActive", fullscreen ? 1 : 0);
		UpdateScale();
		if (fullscreen)
		{
			AkSoundEngine.PostEvent("Play_UI_map_open_01", base.gameObject);
		}
		m_cameraPanOffset = Vector3.zero;
		if (fullscreen)
		{
			Pixelator.Instance.FadeColor = Color.black;
			Pixelator.Instance.fade = 0.3f;
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			GameUIRoot.Instance.UnfoldGunventory(GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameUIRoot.Instance.ToggleItemPanels(false);
			}
			UIMinimap.ToggleState(true);
		}
		else
		{
			Pixelator.Instance.FadeColor = Color.black;
			Pixelator.Instance.fade = 1f;
			GameUIRoot.Instance.ShowCoreUI(string.Empty);
			GameUIRoot.Instance.RefoldGunventory();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameUIRoot.Instance.ToggleItemPanels(true);
			}
			UIMinimap.ToggleState(false);
		}
		if (m_isFullscreen)
		{
			m_cameraBasePosition = GetCameraBasePosition();
			cameraRef.transform.position = m_cameraBasePosition + m_cameraPanOffset;
		}
	}

	private Vector3 GetCameraBasePosition()
	{
		if (m_playerMarkers == null || m_playerMarkers.Count == 0)
		{
			return Vector3.zero;
		}
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = 0; i < m_playerMarkers.Count; i++)
		{
			if ((i != 0 || !(GameManager.Instance.PrimaryPlayer != null) || !GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) && (i != 1 || !(GameManager.Instance.SecondaryPlayer != null) || !GameManager.Instance.SecondaryPlayer.healthHaver.IsDead))
			{
				num++;
				zero += m_playerMarkers[i].First.position;
			}
		}
		zero /= (float)num;
		return zero.WithZ(-5f);
	}

	public void AttemptPanCamera(Vector3 delta)
	{
		m_cameraPanOffset += delta * cameraRef.orthographicSize;
	}

	public void PanToPosition(Vector3 position)
	{
		StartCoroutine(HandleAutoPan((position - m_cameraBasePosition).WithZ(0f)));
	}

	private IEnumerator HandleAutoPan(Vector3 targetPan)
	{
		while (m_isAutoPanning)
		{
			yield return null;
		}
		m_isAutoPanning = true;
		float elapsed = 0f;
		float duration = 0.2f;
		Vector3 startPan = m_cameraPanOffset;
		while (elapsed < duration)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			m_cameraPanOffset = Vector3.Lerp(startPan, targetPan, t);
			yield return null;
		}
		m_isAutoPanning = false;
	}

	public void TogglePresetZoomValue()
	{
		if (m_cameraOrthoOffset == 0f)
		{
			m_cameraOrthoOffset = 4.25f;
		}
		else if (m_cameraOrthoOffset == 4.25f)
		{
			m_cameraOrthoOffset = 8.5f;
		}
		else if (m_cameraOrthoOffset == 8.5f)
		{
			m_cameraOrthoOffset = 0f;
		}
		else
		{
			m_cameraOrthoOffset = 0f;
		}
		GameManager.Options.PreferredMapZoom = m_cameraOrthoOffset;
	}

	public void AttemptZoomCamera(float zoom)
	{
		m_cameraOrthoOffset = Mathf.Clamp(m_cameraOrthoOffset + zoom * 2f, -2f, 9f);
		GameManager.Options.PreferredMapZoom = m_cameraOrthoOffset;
	}

	public void AttemptZoomMinimap(float zoom)
	{
		m_currentMinimapZoom = Mathf.Clamp(m_currentMinimapZoom + zoom * 2f, -1f, 4f);
		GameManager.Options.PreferredMinimapZoom = m_currentMinimapZoom;
	}

	public void InitializeMinimap(DungeonData data)
	{
		if (PreventMinimap)
		{
			return;
		}
		TK2DDungeonAssembler.RuntimeResizeTileMap(tilemap, data.Width, data.Height, tilemap.partitionSizeX, tilemap.partitionSizeY);
		for (int i = 0; i < data.Width; i++)
		{
			for (int j = 0; j < data.Height; j++)
			{
				Color color = new Color(1f, 1f, 1f, 0.75f);
				AssignColorToTile(i, j, 0, color, tilemap);
			}
		}
		tilemap.ForceBuild();
		float x = (float)data.Width / 2f * 0.125f;
		float y = (float)data.Height / 2f * 0.125f;
		m_cameraBasePosition = tilemap.transform.position + new Vector3(x, y, -5f);
		cameraRef.transform.position = m_cameraBasePosition;
	}

	public void UpdatePlayerPositionExact(Vector3 worldPosition, PlayerController player, bool isDying = false)
	{
		if (PreventMinimap)
		{
			return;
		}
		if (m_playerMarkers == null)
		{
			m_playerMarkers = new List<Tuple<Transform, Renderer>>();
		}
		if (m_playerMarkers.Count == 0)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(GameManager.Instance.AllPlayers[i].minimapIconPrefab, base.transform.position, Quaternion.identity);
				gameObject.transform.parent = base.transform;
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				Tuple<Transform, Renderer> item = new Tuple<Transform, Renderer>(gameObject.transform, component.renderer);
				m_playerMarkers.Add(item);
				if (component != null)
				{
					component.renderer.sortingLayerName = "Foreground";
				}
			}
		}
		Vector3 vector = base.transform.position + new Vector3(worldPosition.x * 0.125f, worldPosition.y * 0.125f, -1f);
		int num = ((!player.IsPrimaryPlayer) ? 1 : 0);
		if ((bool)player && player.CurrentRoom != null && player.CurrentRoom.PreventMinimapUpdates)
		{
			if (num < m_playerMarkers.Count)
			{
				m_playerMarkers[num].Second.enabled = false;
			}
			if (!m_isFullscreen)
			{
				m_cameraBasePosition = GetCameraBasePosition().Quantize(0.0625f) + CameraController.PLATFORM_CAMERA_OFFSET;
			}
			cameraRef.transform.position = m_cameraBasePosition + m_cameraPanOffset;
			return;
		}
		if (num < m_playerMarkers.Count)
		{
			m_playerMarkers[num].First.position = vector.Quantize(0.0625f);
			if (isDying || player.healthHaver.IsDead)
			{
				m_playerMarkers[num].Second.enabled = false;
			}
			else
			{
				m_playerMarkers[num].Second.enabled = true;
			}
		}
		if (!m_isFullscreen)
		{
			m_cameraBasePosition = GetCameraBasePosition().Quantize(0.0625f) + CameraController.PLATFORM_CAMERA_OFFSET;
		}
		cameraRef.transform.position = m_cameraBasePosition + m_cameraPanOffset;
	}

	private void PixelQuantizeCameraPosition()
	{
		Vector3 position = cameraRef.transform.position;
		float multiplesOf = 1f / (cameraRef.orthographicSize * 2f * 16f);
		float multiplesOf2 = 16f / (cameraRef.orthographicSize * 2f * 16f * 9f);
		cameraRef.transform.position = position.WithX(BraveMathCollege.QuantizeFloat(position.x, multiplesOf2)).WithY(BraveMathCollege.QuantizeFloat(position.y, multiplesOf));
	}

	public void RevealAllRooms(bool revealSecretRooms)
	{
		if (PreventMinimap)
		{
			return;
		}
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			RoomHandler roomHandler = GameManager.Instance.Dungeon.data.rooms[i];
			if (roomHandler.connectedRooms.Count != 0 && (revealSecretRooms || roomHandler.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.SECRET))
			{
				roomHandler.RevealedOnMap = true;
				RevealMinimapRoom(roomHandler, true, false, roomHandler == GameManager.Instance.PrimaryPlayer.CurrentRoom);
			}
		}
		for (int j = 0; j < GameManager.Instance.Dungeon.data.rooms.Count; j++)
		{
			RoomHandler roomHandler2 = GameManager.Instance.Dungeon.data.rooms[j];
			if (roomHandler2.connectedRooms.Count != 0 && roomToUnknownIconMap.ContainsKey(roomHandler2))
			{
				UnityEngine.Object.Destroy(roomToUnknownIconMap[roomHandler2]);
			}
		}
		roomToUnknownIconMap.Clear();
		StartCoroutine(DelayedMarkDirty());
	}

	private IEnumerator DelayedMarkDirty()
	{
		yield return null;
		m_shouldBuildTilemap = true;
	}

	public void DeflagPreviousRoom(RoomHandler previousRoom)
	{
		if (!PreventMinimap)
		{
			RevealMinimapRoom(previousRoom, true, true, false);
		}
	}

	private void DrawUnknownRoomSquare(RoomHandler current, RoomHandler adjacent, bool doBuild = true, int overrideCellIndex = -1, bool isLockedDoor = false)
	{
		if (PreventMinimap || adjacent.IsSecretRoom || adjacent.RevealedOnMap)
		{
			return;
		}
		int tile = ((overrideCellIndex == -1) ? 49 : overrideCellIndex);
		RuntimeExitDefinition exitDefinitionForConnectedRoom = adjacent.GetExitDefinitionForConnectedRoom(current);
		IntVector2 cellAdjacentToExit = adjacent.GetCellAdjacentToExit(exitDefinitionForConnectedRoom);
		IntVector2 intVector = IntVector2.Zero;
		RuntimeRoomExitData runtimeRoomExitData = ((exitDefinitionForConnectedRoom.upstreamRoom != adjacent) ? exitDefinitionForConnectedRoom.downstreamExit : exitDefinitionForConnectedRoom.upstreamExit);
		if (runtimeRoomExitData != null && runtimeRoomExitData.referencedExit != null)
		{
			intVector = DungeonData.GetIntVector2FromDirection(runtimeRoomExitData.referencedExit.exitDirection);
		}
		if (cellAdjacentToExit == IntVector2.Zero)
		{
			return;
		}
		for (int i = -1; i < 3; i++)
		{
			for (int j = -1; j < 3; j++)
			{
				tilemap.SetTile(cellAdjacentToExit.x + i, cellAdjacentToExit.y + j, 0, tile);
			}
		}
		IntVector2 intVector2 = cellAdjacentToExit + IntVector2.Left;
		IntVector2 intVector3 = cellAdjacentToExit + IntVector2.Left;
		GameObject gameObject = null;
		GameObject gameObject2;
		if (!adjacent.area.IsProceduralRoom && adjacent.area.runtimePrototypeData.associatedMinimapIcon != null)
		{
			gameObject2 = UnityEngine.Object.Instantiate(adjacent.area.runtimePrototypeData.associatedMinimapIcon);
			if (isLockedDoor)
			{
				gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Minimap_Locked_Icon"));
				intVector3 = intVector3 + IntVector2.Right + IntVector2.Down + intVector * 6;
			}
		}
		else if (!isLockedDoor)
		{
			gameObject2 = ((overrideCellIndex == -1) ? ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Minimap_Unknown_Icon"))) : ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Minimap_Blocked_Icon"))));
		}
		else
		{
			gameObject2 = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Minimap_Locked_Icon"));
			intVector2 = intVector2 + IntVector2.Right + IntVector2.Down;
		}
		gameObject2.transform.parent = base.transform;
		gameObject2.transform.position = base.transform.position + intVector2.ToVector3() * 0.125f;
		if (roomToUnknownIconMap.ContainsKey(adjacent))
		{
			gameObject2.transform.parent = roomToUnknownIconMap[adjacent].transform;
		}
		else
		{
			roomToUnknownIconMap.Add(adjacent, gameObject2);
		}
		if (gameObject != null)
		{
			gameObject.transform.parent = roomToUnknownIconMap[adjacent].transform;
			gameObject.transform.position = base.transform.position + intVector3.ToVector3() * 0.125f;
		}
	}

	private void UpdateTeleporterIconForRoom(RoomHandler targetRoom)
	{
		if (!PreventMinimap && roomToTeleportIconMap.ContainsKey(targetRoom) && targetRoom.TeleportersActive)
		{
			GameObject gameObject = roomToTeleportIconMap[targetRoom];
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			if (component.GetCurrentSpriteDef().name == "teleport_001")
			{
				component.SetSprite("teleport_active_001");
			}
		}
	}

	public void RevealMinimapRoom(RoomHandler revealedRoom, bool force = false, bool doBuild = true, bool isCurrentRoom = true)
	{
		if (!revealedRoom.OverrideTilemap)
		{
			StartCoroutine(RevealMinimapRoomInternal(revealedRoom, force, doBuild, isCurrentRoom));
		}
	}

	public IEnumerator RevealMinimapRoomInternal(RoomHandler revealedRoom, bool force = false, bool doBuild = true, bool isCurrentRoom = true)
	{
		revealedRoom.RevealedOnMap = true;
		yield return null;
		if (!m_isInitialized)
		{
			HandleInitialization();
		}
		if (PreventMinimap)
		{
			yield break;
		}
		if (m_revealProcessedMap == null)
		{
			m_revealProcessedMap = new bool[GameManager.Instance.Dungeon.data.Width, GameManager.Instance.Dungeon.data.Height];
		}
		else
		{
			Array.Clear(m_revealProcessedMap, 0, m_revealProcessedMap.GetLength(0) * m_revealProcessedMap.GetLength(1));
		}
		int assignedDefaultIndex = ((!isCurrentRoom) ? 49 : 50);
		if (revealedRoom.visibility != RoomHandler.VisibilityStatus.CURRENT && !force)
		{
			yield break;
		}
		if (revealedRoom.visibility == RoomHandler.VisibilityStatus.OBSCURED && revealedRoom.RevealedOnMap)
		{
			assignedDefaultIndex = -1;
		}
		IntVector2[] cardinals = IntVector2.CardinalsAndOrdinals;
		DungeonData data = GameManager.Instance.Dungeon.data;
		HashSet<IntVector2> AllCells = revealedRoom.GetCellsAndAllConnectedExitsSlow();
		DungeonData dungeonData = GameManager.Instance.Dungeon.data;
		foreach (IntVector2 item in AllCells)
		{
			int tile = assignedDefaultIndex;
			int layer = 0;
			if (data[item] == null || data[item].isSecretRoomCell || data[item].isWallMimicHideout)
			{
				continue;
			}
			if (data[item].isExitCell)
			{
				TileIndexGrid tileIndexGrid = darkIndexGrid;
				if (data[item].exitDoor != null && data[item].exitDoor.isLocked)
				{
					tileIndexGrid = redIndexGrid;
				}
				if (data[item].exitDoor != null && data[item].exitDoor.OneWayDoor && data[item].exitDoor.IsSealed)
				{
					tileIndexGrid = redIndexGrid;
				}
				IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
				tile = tileIndexGrid.GetIndexGivenSides(dungeonData[item + cardinalsAndOrdinals[0]].type == CellType.WALL, dungeonData[item + cardinalsAndOrdinals[1]].type == CellType.WALL, dungeonData[item + cardinalsAndOrdinals[2]].type == CellType.WALL, dungeonData[item + cardinalsAndOrdinals[3]].type == CellType.WALL, dungeonData[item + cardinalsAndOrdinals[4]].type == CellType.WALL, dungeonData[item + cardinalsAndOrdinals[5]].type == CellType.WALL, dungeonData[item + cardinalsAndOrdinals[6]].type == CellType.WALL, dungeonData[item + cardinalsAndOrdinals[7]].type == CellType.WALL);
				layer = 1;
			}
			else if (item.x >= 0 && item.y >= 0 && item.x < m_revealProcessedMap.GetLength(0) && item.y < m_revealProcessedMap.GetLength(1))
			{
				m_revealProcessedMap[item.x, item.y] = true;
			}
			tilemap.SetTile(item.x, item.y, layer, tile);
		}
		foreach (IntVector2 item2 in AllCells)
		{
			for (int i = 0; i < cardinals.Length; i++)
			{
				IntVector2 key = item2 + cardinals[i];
				if (key.x < 0 || key.x >= m_revealProcessedMap.GetLength(0) || key.y < 0 || key.y >= m_revealProcessedMap.GetLength(1) || m_revealProcessedMap[key.x, key.y])
				{
					continue;
				}
				m_revealProcessedMap[key.x, key.y] = true;
				CellData cellData = data[key];
				if (cellData.isWallMimicHideout || cellData.type == CellType.WALL || cellData.isExitCell || cellData.isSecretRoomCell)
				{
					List<CellData> cellNeighbors = data.GetCellNeighbors(cellData, true);
					TileIndexGrid tileIndexGrid2 = ((revealedRoom.visibility != 0) ? indexGrid : CurrentRoomBorderIndexGrid);
					int indexGivenSides = tileIndexGrid2.GetIndexGivenSides(cellNeighbors[0] != null && cellNeighbors[0].type != CellType.WALL && !cellNeighbors[0].isExitCell && !cellNeighbors[0].isWallMimicHideout, cellNeighbors[1] != null && cellNeighbors[1].type != CellType.WALL && !cellNeighbors[1].isExitCell && !cellNeighbors[1].isWallMimicHideout, cellNeighbors[2] != null && cellNeighbors[2].type != CellType.WALL && !cellNeighbors[2].isExitCell && !cellNeighbors[2].isWallMimicHideout, cellNeighbors[3] != null && cellNeighbors[3].type != CellType.WALL && !cellNeighbors[3].isExitCell && !cellNeighbors[3].isWallMimicHideout, cellNeighbors[4] != null && cellNeighbors[4].type != CellType.WALL && !cellNeighbors[4].isExitCell && !cellNeighbors[4].isWallMimicHideout, cellNeighbors[5] != null && cellNeighbors[5].type != CellType.WALL && !cellNeighbors[5].isExitCell && !cellNeighbors[5].isWallMimicHideout, cellNeighbors[6] != null && cellNeighbors[6].type != CellType.WALL && !cellNeighbors[6].isExitCell && !cellNeighbors[6].isWallMimicHideout, cellNeighbors[7] != null && cellNeighbors[7].type != CellType.WALL && !cellNeighbors[7].isExitCell && !cellNeighbors[7].isWallMimicHideout);
					if ((cellNeighbors[0] == null || cellNeighbors[0].type != CellType.FLOOR || cellNeighbors[0].parentRoom == revealedRoom || cellNeighbors[0].isExitCell) && (cellNeighbors[1] == null || cellNeighbors[1].type != CellType.FLOOR || cellNeighbors[1].parentRoom == revealedRoom || cellNeighbors[1].isExitCell) && (cellNeighbors[2] == null || cellNeighbors[2].type != CellType.FLOOR || cellNeighbors[2].parentRoom == revealedRoom || cellNeighbors[2].isExitCell) && (cellNeighbors[3] == null || cellNeighbors[3].type != CellType.FLOOR || cellNeighbors[3].parentRoom == revealedRoom || cellNeighbors[3].isExitCell) && (cellNeighbors[4] == null || cellNeighbors[4].type != CellType.FLOOR || cellNeighbors[4].parentRoom == revealedRoom || cellNeighbors[4].isExitCell) && (cellNeighbors[5] == null || cellNeighbors[5].type != CellType.FLOOR || cellNeighbors[5].parentRoom == revealedRoom || cellNeighbors[5].isExitCell) && (cellNeighbors[6] == null || cellNeighbors[6].type != CellType.FLOOR || cellNeighbors[6].parentRoom == revealedRoom || cellNeighbors[6].isExitCell) && (cellNeighbors[7] == null || cellNeighbors[7].type != CellType.FLOOR || cellNeighbors[7].parentRoom == revealedRoom || cellNeighbors[7].isExitCell))
					{
						tilemap.SetTile(key.x, key.y, 0, indexGivenSides);
					}
				}
			}
		}
		for (int j = 0; j < revealedRoom.connectedRooms.Count; j++)
		{
			if (revealedRoom.connectedRooms[j].visibility == RoomHandler.VisibilityStatus.OBSCURED && !force)
			{
				int overrideCellIndex = -1;
				RuntimeExitDefinition exitDefinitionForConnectedRoom = revealedRoom.GetExitDefinitionForConnectedRoom(revealedRoom.connectedRooms[j]);
				if (exitDefinitionForConnectedRoom.linkedDoor != null && exitDefinitionForConnectedRoom.linkedDoor.OneWayDoor && exitDefinitionForConnectedRoom.linkedDoor.IsSealed)
				{
					overrideCellIndex = redIndexGrid.centerIndices.GetIndexByWeight();
				}
				if (exitDefinitionForConnectedRoom.linkedDoor != null && exitDefinitionForConnectedRoom.linkedDoor.isLocked)
				{
					overrideCellIndex = redIndexGrid.centerIndices.GetIndexByWeight();
				}
				DrawUnknownRoomSquare(revealedRoom, revealedRoom.connectedRooms[j], true, overrideCellIndex, exitDefinitionForConnectedRoom.linkedDoor != null && exitDefinitionForConnectedRoom.linkedDoor.isLocked);
			}
		}
		if (roomToUnknownIconMap.ContainsKey(revealedRoom))
		{
			UnityEngine.Object.Destroy(roomToUnknownIconMap[revealedRoom]);
			roomToUnknownIconMap.Remove(revealedRoom);
		}
		if (roomToIconsMap.ContainsKey(revealedRoom))
		{
			for (int k = 0; k < roomToIconsMap[revealedRoom].Count; k++)
			{
				roomToIconsMap[revealedRoom][k].SetActive(true);
			}
		}
		if (roomToTeleportIconMap.ContainsKey(revealedRoom))
		{
			roomToTeleportIconMap[revealedRoom].SetActive(true);
		}
		UpdateTeleporterIconForRoom(revealedRoom);
		yield return null;
		if (doBuild)
		{
			m_shouldBuildTilemap = true;
		}
	}

	private void Start()
	{
		m_cameraOrthoOffset = GameManager.Options.PreferredMapZoom;
		m_currentMinimapZoom = GameManager.Options.PreferredMinimapZoom;
		HandleInitialization();
	}

	private void HandleInitialization()
	{
		if (!m_isInitialized)
		{
			m_isInitialized = true;
			cameraRef.enabled = true;
			m_mapMaskMaterial = cameraRef.GetComponent<MinimapRenderer>().QuadTransform.GetComponent<MeshRenderer>().sharedMaterial;
			m_whiteTex = new Texture2D(1, 1);
			m_whiteTex.SetPixel(0, 0, Color.white);
			m_whiteTex.Apply();
			if (GameManager.Instance.IsFoyer || MinimapMode == MinimapDisplayMode.NEVER)
			{
				m_isFaded = true;
				m_cachedFadeValue = 0f;
				m_mapMaskMaterial.SetFloat("_Fade", 0f);
			}
			ToggleMinimap(false);
		}
	}

	private void UpdateScale()
	{
		if (m_isFullscreen)
		{
			if (m_cameraOrthoBase != GameManager.Instance.MainCameraController.GetComponent<Camera>().orthographicSize)
			{
				m_cameraOrthoBase = GameManager.Instance.MainCameraController.GetComponent<Camera>().orthographicSize;
			}
			if (cameraRef.orthographicSize != m_cameraOrthoBase + m_cameraOrthoOffset)
			{
				cameraRef.orthographicSize = m_cameraOrthoBase + m_cameraOrthoOffset;
			}
			cameraRef.orthographicSize = BraveMathCollege.QuantizeFloat(cameraRef.orthographicSize, 0.5f);
		}
		else
		{
			cameraRef.orthographicSize = 2.109375f + m_currentMinimapZoom;
		}
	}

	private void LateUpdate()
	{
		UpdateScale();
		if (MinimapMode == MinimapDisplayMode.NEVER || !DoMinimap || TemporarilyPreventMinimap || GameManager.Instance.IsPaused)
		{
			if (!m_isFaded)
			{
				StartCoroutine(TransitionToNewFadeState(true));
			}
		}
		else if (MinimapMode == MinimapDisplayMode.FADE_ON_ROOM_SEAL)
		{
			CheckRoomSealState();
		}
		else if (MinimapMode == MinimapDisplayMode.ALWAYS && m_isFaded)
		{
			StartCoroutine(TransitionToNewFadeState(false));
		}
		bool flag = GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.RESOURCEFUL_RAT;
		if (m_shouldBuildTilemap && !flag)
		{
			m_shouldBuildTilemap = false;
			tilemap.Build(tk2dTileMap.BuildFlags.Default);
		}
	}

	private void CheckRoomSealState()
	{
		if (GameManager.Instance.PrimaryPlayer == null || GameManager.Instance.IsFoyer)
		{
			return;
		}
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		if (currentRoom != null)
		{
			if (currentRoom.IsSealed && !m_isFaded)
			{
				StartCoroutine(TransitionToNewFadeState(true));
			}
			else if (!currentRoom.IsSealed && m_isFaded)
			{
				StartCoroutine(TransitionToNewFadeState(false));
			}
			else if (currentRoom.IsSealed && m_isFaded && !m_isTransitioning)
			{
				m_mapMaskMaterial.SetFloat("_Fade", m_isFullscreen ? 1 : 0);
			}
			else if (!currentRoom.IsSealed && !m_isFaded && !m_isTransitioning)
			{
				m_mapMaskMaterial.SetFloat("_Fade", 1f);
			}
		}
	}

	private IEnumerator TransitionToNewFadeState(bool newFadeState)
	{
		m_isTransitioning = true;
		m_isFaded = newFadeState;
		float elapsed = 0f;
		float duration = 0.5f;
		while (elapsed < duration && m_isFaded == newFadeState && !m_isFullscreen)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / duration;
			if (m_isFaded)
			{
				t = 1f - t;
			}
			if ((bool)m_mapMaskMaterial)
			{
				m_mapMaskMaterial.SetFloat("_Fade", Mathf.Clamp01(t));
			}
			yield return null;
		}
		m_cachedFadeValue = ((!newFadeState) ? 1 : 0);
		m_isTransitioning = false;
	}

	public RoomHandler CheckIconsNearCursor(Vector3 screenPosition, out GameObject icon)
	{
		Vector2 vector = BraveUtility.GetMinimapViewportPosition(screenPosition);
		vector.x = (vector.x - 0.5f) * (BraveCameraUtility.ASPECT / 1.77777779f) + 0.5f;
		Vector2 vector2 = (cameraRef.ViewportPointToRay(vector).origin.XY() - base.transform.position.XY()) * 8f;
		IntVector2 intVector = vector2.ToIntVector2(VectorConversions.Floor);
		if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
		{
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			RoomHandler parentRoom = cellData.parentRoom;
			if (parentRoom != null && roomToTeleportIconMap.ContainsKey(parentRoom))
			{
				icon = roomToTeleportIconMap[parentRoom];
				return parentRoom;
			}
		}
		icon = null;
		return null;
	}

	public RoomHandler GetNearestVisibleRoom(Vector2 screenPosition, out float dist)
	{
		float num = screenPosition.x / (float)Screen.width;
		float num2 = screenPosition.y / (float)Screen.height;
		num = (num - 0.5f) / BraveCameraUtility.GetRect().width + 0.5f;
		num2 = (num2 - 0.5f) / BraveCameraUtility.GetRect().height + 0.5f;
		Vector2 vector = cameraRef.ViewportPointToRay(new Vector3(num, num2, 0f)).origin.XY();
		dist = float.MaxValue;
		RoomHandler result = null;
		foreach (RoomHandler key in roomToTeleportIconMap.Keys)
		{
			if (key.TeleportersActive)
			{
				GameObject gameObject = roomToTeleportIconMap[key];
				Debug.DrawLine(vector, gameObject.GetComponent<tk2dBaseSprite>().WorldCenter, Color.red, 5f);
				float num3 = Vector2.Distance(vector, gameObject.GetComponent<tk2dBaseSprite>().WorldCenter);
				if (num3 < dist)
				{
					dist = num3;
					result = key;
				}
			}
		}
		return result;
	}

	private void OrganizeExtantIcons(RoomHandler targetRoom, bool includeTeleIcon = false)
	{
		if (!roomToIconsMap.ContainsKey(targetRoom) && !roomToTeleportIconMap.ContainsKey(targetRoom))
		{
			Debug.LogError("ORGANIZING ROOM: " + targetRoom.GetRoomName() + " IN MINIMAP WITH NO ICONS, TELL BR.NET");
			return;
		}
		List<GameObject> list = ((!roomToIconsMap.ContainsKey(targetRoom)) ? null : roomToIconsMap[targetRoom]);
		if (roomHasMovedTeleportIcon.ContainsKey(targetRoom))
		{
			includeTeleIcon = true;
		}
		bool flag = roomToTeleportIconMap.ContainsKey(targetRoom) && includeTeleIcon;
		int num = ((list != null) ? list.Count : 0) + (flag ? 1 : 0);
		float num2 = 6f;
		float num3 = (float)(num - 1) * num2;
		float num4 = num3 / 2f;
		IntVector2 centerCell = targetRoom.GetCenterCell();
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = ((!flag || i != num - 1) ? list[i] : roomToTeleportIconMap[targetRoom]);
			if ((bool)gameObject)
			{
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				if ((bool)component)
				{
					Vector3 vector = new Vector3(num2 * (float)i - num4, 0f, 0f);
					Vector3 position = base.transform.position + (centerCell.ToVector3() + vector) * 0.125f + new Vector3(0f, 0f, 1f);
					component.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.MiddleCenter);
				}
			}
		}
		for (int j = 0; j < num; j++)
		{
			GameObject gameObject2 = ((!flag || j != num - 1) ? list[j] : roomToTeleportIconMap[targetRoom]);
			if ((bool)gameObject2)
			{
				gameObject2.transform.position = gameObject2.transform.position.Quantize(1f / 64f);
			}
		}
		if (!includeTeleIcon && roomToTeleportIconMap.ContainsKey(targetRoom) && num > 0)
		{
			tk2dBaseSprite component2 = roomToTeleportIconMap[targetRoom].GetComponent<tk2dBaseSprite>();
			float num5 = float.MaxValue;
			for (int k = 0; k < num; k++)
			{
				tk2dBaseSprite component3 = list[k].GetComponent<tk2dBaseSprite>();
				num5 = Mathf.Min(num5, Vector2.Distance(component3.WorldCenter, component2.WorldCenter));
			}
			if (num5 <= 0.375f)
			{
				roomHasMovedTeleportIcon.Add(targetRoom, true);
				OrganizeExtantIcons(targetRoom, true);
			}
		}
		else if (roomToTeleportIconMap.ContainsKey(targetRoom) && num == 0)
		{
			roomToTeleportIconMap[targetRoom].transform.position = roomToInitialTeleportIconPositionMap[targetRoom];
		}
	}

	private void AddIconToRoomList(RoomHandler room, GameObject instanceIcon)
	{
		if (roomToIconsMap.ContainsKey(room))
		{
			roomToIconsMap[room].Add(instanceIcon);
		}
		else
		{
			List<GameObject> list = new List<GameObject>();
			list.Add(instanceIcon);
			roomToIconsMap.Add(room, list);
		}
		OrganizeExtantIcons(room);
	}

	private void RemoveIconFromRoomList(RoomHandler room, GameObject instanceIcon)
	{
		if (roomToIconsMap.ContainsKey(room) && roomToIconsMap[room].Remove(instanceIcon))
		{
			UnityEngine.Object.Destroy(instanceIcon);
			OrganizeExtantIcons(room);
		}
	}

	public bool HasTeleporterIcon(RoomHandler room)
	{
		return roomToTeleportIconMap.ContainsKey(room);
	}

	private void ClampIconToRoomBounds(RoomHandler room, GameObject instanceIcon, Vector2 placedPosition)
	{
		Vector2 min = base.transform.position.XY() + room.area.basePosition.ToVector2() * 0.125f;
		Vector2 max = base.transform.position.XY() + (room.area.basePosition.ToVector2() + room.area.dimensions.ToVector2()) * 0.125f;
		min += Vector2.one * 0.5f;
		max -= Vector2.one * 0.5f;
		placedPosition = BraveMathCollege.ClampToBounds(placedPosition, min, max);
		instanceIcon.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(placedPosition, tk2dBaseSprite.Anchor.MiddleCenter);
	}

	public void RegisterTeleportIcon(RoomHandler room, GameObject iconPrefab, Vector2 position)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(iconPrefab);
		Vector2 vector = base.transform.position + position.ToVector3ZUp() * 0.125f + new Vector3(0f, 0f, 1f);
		gameObject.GetComponent<tk2dBaseSprite>().PlaceAtPositionByAnchor(vector, tk2dBaseSprite.Anchor.MiddleCenter);
		ClampIconToRoomBounds(room, gameObject, vector);
		gameObject.transform.position = gameObject.transform.position.WithZ(-1f);
		gameObject.transform.parent = base.transform;
		gameObject.SetActive((room.visibility != 0) ? true : false);
		roomsContainingTeleporters.Add(room);
		roomToTeleportIconMap.Add(room, gameObject);
		roomToInitialTeleportIconPositionMap.Add(room, gameObject.transform.position);
		roomsContainingTeleporters.Sort(delegate(RoomHandler a, RoomHandler b)
		{
			Vector2 vector2 = roomToInitialTeleportIconPositionMap[a];
			Vector2 vector3 = roomToInitialTeleportIconPositionMap[b];
			return (vector2.y == vector3.y) ? vector2.x.CompareTo(vector3.x) : vector2.y.CompareTo(vector3.y);
		});
		OrganizeExtantIcons(room);
	}

	public GameObject RegisterRoomIcon(RoomHandler room, GameObject iconPrefab, bool forceActive = false)
	{
		if (iconPrefab == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(iconPrefab);
		gameObject.transform.position = gameObject.transform.position.WithZ(-1f);
		gameObject.transform.parent = base.transform;
		if (forceActive)
		{
			gameObject.SetActive(true);
		}
		else
		{
			gameObject.SetActive((room.visibility != 0) ? true : false);
		}
		AddIconToRoomList(room, gameObject);
		return gameObject;
	}

	public void DeregisterRoomIcon(RoomHandler room, GameObject instanceIcon)
	{
		RemoveIconFromRoomList(room, instanceIcon);
	}

	public void OnDestroy()
	{
		m_instance = null;
	}

	public RoomHandler NextSelectedTeleporter(ref int selectedIndex, int dir)
	{
		selectedIndex = Mathf.Clamp(selectedIndex, 0, RoomToTeleportMap.Count - 1);
		int num = selectedIndex;
		do
		{
			num = (num + dir + RoomToTeleportMap.Count) % RoomToTeleportMap.Count;
			RoomHandler roomHandler = roomsContainingTeleporters[num];
			if (roomHandler.TeleportersActive)
			{
				selectedIndex = num;
				return roomHandler;
			}
		}
		while (num != selectedIndex);
		return null;
	}
}
