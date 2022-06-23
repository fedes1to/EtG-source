using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using tk2dRuntime;
using tk2dRuntime.TileMap;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/TileMap/TileMap")]
public class tk2dTileMap : MonoBehaviour, ISpriteCollectionForceBuild
{
	[Serializable]
	public class TilemapPrefabInstance
	{
		public int x;

		public int y;

		public int layer;

		public GameObject instance;
	}

	[Flags]
	public enum BuildFlags
	{
		Default = 0,
		EditMode = 1,
		ForceBuild = 2
	}

	public string editorDataGUID = string.Empty;

	public tk2dTileMapData data;

	public GameObject renderData;

	[SerializeField]
	private tk2dSpriteCollectionData spriteCollection;

	[SerializeField]
	private int spriteCollectionKey;

	public int width = 128;

	public int height = 128;

	public int partitionSizeX = 32;

	public int partitionSizeY = 32;

	public bool isGungeonTilemap = true;

	[SerializeField]
	private Layer[] layers;

	[SerializeField]
	private ColorChannel colorChannel;

	[SerializeField]
	private GameObject prefabsRoot;

	[SerializeField]
	private List<TilemapPrefabInstance> tilePrefabsList = new List<TilemapPrefabInstance>();

	[SerializeField]
	private bool _inEditMode;

	public string serializedMeshPath;

	public tk2dSpriteCollectionData Editor__SpriteCollection
	{
		get
		{
			return spriteCollection;
		}
		set
		{
			spriteCollection = value;
		}
	}

	public tk2dSpriteCollectionData SpriteCollectionInst
	{
		get
		{
			if (spriteCollection != null)
			{
				return spriteCollection.inst;
			}
			return null;
		}
	}

	public bool AllowEdit
	{
		get
		{
			return _inEditMode;
		}
	}

	public List<TilemapPrefabInstance> TilePrefabsList
	{
		get
		{
			return tilePrefabsList;
		}
	}

	public Layer[] Layers
	{
		get
		{
			return layers;
		}
		set
		{
			layers = value;
		}
	}

	public ColorChannel ColorChannel
	{
		get
		{
			return colorChannel;
		}
		set
		{
			colorChannel = value;
		}
	}

	public GameObject PrefabsRoot
	{
		get
		{
			return prefabsRoot;
		}
		set
		{
			prefabsRoot = value;
		}
	}

	private void Awake()
	{
		bool flag = true;
		if ((bool)SpriteCollectionInst && (SpriteCollectionInst.buildKey != spriteCollectionKey || SpriteCollectionInst.needMaterialInstance))
		{
			flag = false;
		}
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
		{
			if ((Application.isPlaying && _inEditMode) || !flag)
			{
				EndEditMode();
			}
			else if (spriteCollection != null && data != null && renderData == null)
			{
				Build(BuildFlags.ForceBuild);
			}
		}
		else if (_inEditMode)
		{
			Debug.LogError("Tilemap " + base.name + " is still in edit mode. Please fix.Building overhead will be significant.");
			EndEditMode();
		}
		else if (!flag)
		{
			Build(BuildFlags.ForceBuild);
		}
		else if (spriteCollection != null && data != null && renderData == null)
		{
			Build(BuildFlags.ForceBuild);
		}
	}

	private void OnDestroy()
	{
		if (layers != null)
		{
			Layer[] array = layers;
			foreach (Layer layer in array)
			{
				layer.DestroyGameData(this);
			}
		}
		if (renderData != null)
		{
			tk2dUtil.DestroyImmediate(renderData);
		}
	}

	public void Build()
	{
		Build(BuildFlags.Default);
	}

	public void ForceBuild()
	{
		Build(BuildFlags.ForceBuild);
	}

	private void ClearSpawnedInstances()
	{
		if (layers == null)
		{
			return;
		}
		BuilderUtil.HideTileMapPrefabs(this);
		for (int i = 0; i < layers.Length; i++)
		{
			Layer layer = layers[i];
			for (int j = 0; j < layer.spriteChannel.chunks.Length; j++)
			{
				SpriteChunk spriteChunk = layer.spriteChannel.chunks[j];
				if (!(spriteChunk.gameObject == null))
				{
					Transform transform = spriteChunk.gameObject.transform;
					List<Transform> list = new List<Transform>();
					for (int k = 0; k < transform.childCount; k++)
					{
						list.Add(transform.GetChild(k));
					}
					for (int l = 0; l < list.Count; l++)
					{
						tk2dUtil.DestroyImmediate(list[l].gameObject);
					}
				}
			}
		}
	}

	private void SetPrefabsRootActive(bool active)
	{
		if (prefabsRoot != null)
		{
			tk2dUtil.SetActive(prefabsRoot, active);
		}
	}

	public void Build(BuildFlags buildFlags)
	{
		IEnumerator enumerator = DeferredBuild(buildFlags);
		while (enumerator.MoveNext())
		{
		}
	}

	public IEnumerator DeferredBuild(BuildFlags buildFlags)
	{
		if (!(data != null) || !(spriteCollection != null))
		{
			yield break;
		}
		if (data.tilePrefabs == null)
		{
			data.tilePrefabs = new GameObject[SpriteCollectionInst.Count];
		}
		else if (data.tilePrefabs.Length != SpriteCollectionInst.Count)
		{
			Array.Resize(ref data.tilePrefabs, SpriteCollectionInst.Count);
		}
		BuilderUtil.InitDataStore(this);
		if ((bool)SpriteCollectionInst)
		{
			SpriteCollectionInst.InitMaterialIds();
		}
		bool forceBuild = (buildFlags & BuildFlags.ForceBuild) != 0;
		if ((bool)SpriteCollectionInst && SpriteCollectionInst.buildKey != spriteCollectionKey)
		{
			forceBuild = true;
		}
		Dictionary<Layer, bool> layersActive = new Dictionary<Layer, bool>();
		if (layers != null)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				Layer layer = layers[i];
				if (layer != null && layer.gameObject != null)
				{
					layersActive[layer] = layer.gameObject.activeSelf;
				}
			}
		}
		if (forceBuild)
		{
			ClearSpawnedInstances();
		}
		BuilderUtil.CreateRenderData(this, _inEditMode, layersActive);
		SpriteChunk.s_roomChunks = new Dictionary<LayerInfo, List<SpriteChunk>>();
		if (Application.isPlaying && GameManager.Instance.Dungeon != null && GameManager.Instance.Dungeon.data != null && GameManager.Instance.Dungeon.MainTilemap == this)
		{
			List<RoomHandler> rooms = GameManager.Instance.Dungeon.data.rooms;
			if (rooms != null && rooms.Count > 0)
			{
				for (int j = 0; j < data.Layers.Length; j++)
				{
					if (!data.Layers[j].overrideChunkable)
					{
						continue;
					}
					for (int k = 0; k < rooms.Count; k++)
					{
						if (!SpriteChunk.s_roomChunks.ContainsKey(data.Layers[j]))
						{
							SpriteChunk.s_roomChunks.Add(data.Layers[j], new List<SpriteChunk>());
						}
						SpriteChunk spriteChunk = new SpriteChunk(rooms[k].area.basePosition.x + data.Layers[j].overrideChunkXOffset, rooms[k].area.basePosition.y + data.Layers[j].overrideChunkYOffset, rooms[k].area.basePosition.x + rooms[k].area.dimensions.x + data.Layers[j].overrideChunkXOffset, rooms[k].area.basePosition.y + rooms[k].area.dimensions.y + data.Layers[j].overrideChunkYOffset);
						spriteChunk.roomReference = rooms[k];
						string prototypeRoomName = rooms[k].area.PrototypeRoomName;
						Layers[j].CreateOverrideChunk(spriteChunk);
						BuilderUtil.CreateOverrideChunkData(spriteChunk, this, j, prototypeRoomName);
						SpriteChunk.s_roomChunks[data.Layers[j]].Add(spriteChunk);
					}
				}
			}
		}
		IEnumerator BuildTracker = RenderMeshBuilder.Build(this, _inEditMode, forceBuild);
		while (BuildTracker.MoveNext())
		{
			yield return null;
		}
		if (!_inEditMode && isGungeonTilemap)
		{
			BuilderUtil.SpawnAnimatedTiles(this, forceBuild);
		}
		if (!_inEditMode)
		{
			tk2dSpriteDefinition firstValidDefinition = SpriteCollectionInst.FirstValidDefinition;
			if (firstValidDefinition != null && firstValidDefinition.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D)
			{
				ColliderBuilder2D.Build(this, forceBuild);
			}
			else
			{
				ColliderBuilder3D.Build(this, forceBuild);
			}
			BuilderUtil.SpawnPrefabs(this, forceBuild);
		}
		Layer[] array = layers;
		foreach (Layer layer2 in array)
		{
			layer2.ClearDirtyFlag();
		}
		if (colorChannel != null)
		{
			colorChannel.ClearDirtyFlag();
		}
		if ((bool)SpriteCollectionInst)
		{
			spriteCollectionKey = SpriteCollectionInst.buildKey;
		}
	}

	public bool GetTileAtPosition(Vector3 position, out int x, out int y)
	{
		float x2;
		float y2;
		bool tileFracAtPosition = GetTileFracAtPosition(position, out x2, out y2);
		x = (int)x2;
		y = (int)y2;
		return tileFracAtPosition;
	}

	public bool GetTileFracAtPosition(Vector3 position, out float x, out float y)
	{
		switch (data.tileType)
		{
		case tk2dTileMapData.TileType.Rectangular:
		{
			Vector3 vector2 = base.transform.worldToLocalMatrix.MultiplyPoint(position);
			x = (vector2.x - data.tileOrigin.x) / data.tileSize.x;
			y = (vector2.y - data.tileOrigin.y) / data.tileSize.y;
			return x >= 0f && x <= (float)width && y >= 0f && y <= (float)height;
		}
		case tk2dTileMapData.TileType.Isometric:
		{
			if (data.tileSize.x == 0f)
			{
				break;
			}
			float num = Mathf.Atan2(data.tileSize.y, data.tileSize.x / 2f);
			Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint(position);
			x = (vector.x - data.tileOrigin.x) / data.tileSize.x;
			y = (vector.y - data.tileOrigin.y) / data.tileSize.y;
			float num2 = y * 0.5f;
			int num3 = (int)num2;
			float num4 = num2 - (float)num3;
			float num5 = x % 1f;
			x = (int)x;
			y = num3 * 2;
			if (num5 > 0.5f)
			{
				if (num4 > 0.5f && Mathf.Atan2(1f - num4, (num5 - 0.5f) * 2f) < num)
				{
					y += 1f;
				}
				else if (num4 < 0.5f && Mathf.Atan2(num4, (num5 - 0.5f) * 2f) < num)
				{
					y -= 1f;
				}
			}
			else if (num5 < 0.5f)
			{
				if (num4 > 0.5f && Mathf.Atan2(num4 - 0.5f, num5 * 2f) > num)
				{
					y += 1f;
					x -= 1f;
				}
				if (num4 < 0.5f && Mathf.Atan2(num4, (0.5f - num5) * 2f) < num)
				{
					y -= 1f;
					x -= 1f;
				}
			}
			return x >= 0f && x <= (float)width && y >= 0f && y <= (float)height;
		}
		}
		x = 0f;
		y = 0f;
		return false;
	}

	public Vector3 GetTilePosition(int x, int y)
	{
		tk2dTileMapData.TileType tileType = data.tileType;
		if (tileType == tk2dTileMapData.TileType.Rectangular || tileType != tk2dTileMapData.TileType.Isometric)
		{
			Vector3 point = new Vector3((float)x * data.tileSize.x + data.tileOrigin.x, (float)y * data.tileSize.y + data.tileOrigin.y, 0f);
			return base.transform.localToWorldMatrix.MultiplyPoint(point);
		}
		Vector3 point2 = new Vector3(((float)x + ((((uint)y & (true ? 1u : 0u)) != 0) ? 0.5f : 0f)) * data.tileSize.x + data.tileOrigin.x, (float)y * data.tileSize.y + data.tileOrigin.y, 0f);
		return base.transform.localToWorldMatrix.MultiplyPoint(point2);
	}

	public int GetTileIdAtPosition(Vector3 position, int layer)
	{
		if (layer < 0 || layer >= layers.Length)
		{
			return -1;
		}
		int x;
		int y;
		if (!GetTileAtPosition(position, out x, out y))
		{
			return -1;
		}
		return layers[layer].GetTile(x, y);
	}

	public TileInfo GetTileInfoForTileId(int tileId)
	{
		return data.GetTileInfoForSprite(tileId);
	}

	public Color GetInterpolatedColorAtPosition(Vector3 position)
	{
		Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint(position);
		int num = (int)((vector.x - data.tileOrigin.x) / data.tileSize.x);
		int num2 = (int)((vector.y - data.tileOrigin.y) / data.tileSize.y);
		if (colorChannel == null || colorChannel.IsEmpty)
		{
			return Color.white;
		}
		if (num < 0 || num >= width || num2 < 0 || num2 >= height)
		{
			return colorChannel.clearColor;
		}
		int offset;
		ColorChunk colorChunk = colorChannel.FindChunkAndCoordinate(num, num2, out offset);
		if (colorChunk.Empty)
		{
			return colorChannel.clearColor;
		}
		int num3 = partitionSizeX + 1;
		Color a = colorChunk.colors[offset];
		Color b = colorChunk.colors[offset + 1];
		Color a2 = colorChunk.colors[offset + num3];
		Color b2 = colorChunk.colors[offset + num3 + 1];
		float num4 = (float)num * data.tileSize.x + data.tileOrigin.x;
		float num5 = (float)num2 * data.tileSize.y + data.tileOrigin.y;
		float t = (vector.x - num4) / data.tileSize.x;
		float t2 = (vector.y - num5) / data.tileSize.y;
		Color a3 = Color.Lerp(a, b, t);
		Color b3 = Color.Lerp(a2, b2, t);
		return Color.Lerp(a3, b3, t2);
	}

	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return this.spriteCollection != null && (spriteCollection == this.spriteCollection || spriteCollection == this.spriteCollection.inst);
	}

	public void EndEditMode()
	{
		_inEditMode = false;
		SetPrefabsRootActive(true);
		Build(BuildFlags.ForceBuild);
		if (prefabsRoot != null)
		{
			tk2dUtil.DestroyImmediate(prefabsRoot);
			prefabsRoot = null;
		}
	}

	public bool AreSpritesInitialized()
	{
		return layers != null;
	}

	public bool HasColorChannel()
	{
		return colorChannel != null && !colorChannel.IsEmpty;
	}

	public void CreateColorChannel()
	{
		colorChannel = new ColorChannel(width, height, partitionSizeX, partitionSizeY);
		colorChannel.Create();
	}

	public void DeleteColorChannel()
	{
		colorChannel.Delete();
	}

	public void DeleteSprites(int layerId, int x0, int y0, int x1, int y1)
	{
		x0 = Mathf.Clamp(x0, 0, width - 1);
		y0 = Mathf.Clamp(y0, 0, height - 1);
		x1 = Mathf.Clamp(x1, 0, width - 1);
		y1 = Mathf.Clamp(y1, 0, height - 1);
		int num = x1 - x0 + 1;
		int num2 = y1 - y0 + 1;
		Layer layer = layers[layerId];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				layer.SetTile(x0 + j, y0 + i, -1);
			}
		}
		layer.OptimizeIncremental();
	}

	public void TouchMesh(Mesh mesh)
	{
	}

	public void DestroyMesh(Mesh mesh)
	{
		tk2dUtil.DestroyImmediate(mesh);
	}

	public int GetTilePrefabsListCount()
	{
		return tilePrefabsList.Count;
	}

	public void GetTilePrefabsListItem(int index, out int x, out int y, out int layer, out GameObject instance)
	{
		TilemapPrefabInstance tilemapPrefabInstance = tilePrefabsList[index];
		x = tilemapPrefabInstance.x;
		y = tilemapPrefabInstance.y;
		layer = tilemapPrefabInstance.layer;
		instance = tilemapPrefabInstance.instance;
	}

	public void SetTilePrefabsList(List<int> xs, List<int> ys, List<int> layers, List<GameObject> instances)
	{
		int count = instances.Count;
		tilePrefabsList = new List<TilemapPrefabInstance>(count);
		for (int i = 0; i < count; i++)
		{
			TilemapPrefabInstance tilemapPrefabInstance = new TilemapPrefabInstance();
			tilemapPrefabInstance.x = xs[i];
			tilemapPrefabInstance.y = ys[i];
			tilemapPrefabInstance.layer = layers[i];
			tilemapPrefabInstance.instance = instances[i];
			tilePrefabsList.Add(tilemapPrefabInstance);
		}
	}

	public int GetTile(int x, int y, int layer)
	{
		if (layer < 0 || layer >= layers.Length)
		{
			return -1;
		}
		return layers[layer].GetTile(x, y);
	}

	public tk2dTileFlags GetTileFlags(int x, int y, int layer)
	{
		if (layer < 0 || layer >= layers.Length)
		{
			return tk2dTileFlags.None;
		}
		return layers[layer].GetTileFlags(x, y);
	}

	public void SetTile(int x, int y, int layer, int tile)
	{
		if (layer >= 0 && layer < layers.Length)
		{
			layers[layer].SetTile(x, y, tile);
		}
	}

	public void SetTileFlags(int x, int y, int layer, tk2dTileFlags flags)
	{
		if (layer >= 0 && layer < layers.Length)
		{
			layers[layer].SetTileFlags(x, y, flags);
		}
	}

	public void ClearTile(int x, int y, int layer)
	{
		if (layer >= 0 && layer < layers.Length)
		{
			layers[layer].ClearTile(x, y);
		}
	}
}
