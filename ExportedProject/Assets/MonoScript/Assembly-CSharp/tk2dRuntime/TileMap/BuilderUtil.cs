using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace tk2dRuntime.TileMap
{
	public static class BuilderUtil
	{
		internal class ProcGenMeshData
		{
			public List<Vector3> vertices = new List<Vector3>();

			public List<int> triangles = new List<int>();

			public List<Vector2> uvs = new List<Vector2>();

			public List<Color> colors = new List<Color>();
		}

		private static List<int> TilePrefabsX;

		private static List<int> TilePrefabsY;

		private static List<int> TilePrefabsLayer;

		private static List<GameObject> TilePrefabsInstance;

		private const int tileMask = 16777215;

		public static bool InitDataStore(tk2dTileMap tileMap)
		{
			bool result = false;
			int numLayers = tileMap.data.NumLayers;
			if (tileMap.Layers == null)
			{
				tileMap.Layers = new Layer[numLayers];
				for (int i = 0; i < numLayers; i++)
				{
					tileMap.Layers[i] = new Layer(tileMap.data.Layers[i].hash, tileMap.width, tileMap.height, tileMap.partitionSizeX, tileMap.partitionSizeY);
				}
				result = true;
			}
			else
			{
				Layer[] array = new Layer[numLayers];
				for (int j = 0; j < numLayers; j++)
				{
					LayerInfo layerInfo = tileMap.data.Layers[j];
					bool flag = false;
					for (int k = 0; k < tileMap.Layers.Length; k++)
					{
						if (tileMap.Layers[k].hash == layerInfo.hash)
						{
							array[j] = tileMap.Layers[k];
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						array[j] = new Layer(layerInfo.hash, tileMap.width, tileMap.height, tileMap.partitionSizeX, tileMap.partitionSizeY);
					}
				}
				int num = 0;
				Layer[] array2 = array;
				foreach (Layer layer in array2)
				{
					if (!layer.IsEmpty)
					{
						num++;
					}
				}
				int num2 = 0;
				Layer[] layers = tileMap.Layers;
				foreach (Layer layer2 in layers)
				{
					if (!layer2.IsEmpty)
					{
						num2++;
					}
				}
				if (num != num2)
				{
					result = true;
				}
				tileMap.Layers = array;
			}
			if (tileMap.ColorChannel == null)
			{
				tileMap.ColorChannel = new ColorChannel(tileMap.width, tileMap.height, tileMap.partitionSizeX, tileMap.partitionSizeY);
			}
			return result;
		}

		private static GameObject GetExistingTilePrefabInstance(tk2dTileMap tileMap, int tileX, int tileY, int tileLayer)
		{
			int tilePrefabsListCount = tileMap.GetTilePrefabsListCount();
			for (int i = 0; i < tilePrefabsListCount; i++)
			{
				int x;
				int y;
				int layer;
				GameObject instance;
				tileMap.GetTilePrefabsListItem(i, out x, out y, out layer, out instance);
				if (x == tileX && y == tileY && layer == tileLayer)
				{
					return instance;
				}
			}
			return null;
		}

		public static void SpawnAnimatedTiles(tk2dTileMap tileMap, bool forceBuild)
		{
			int num = tileMap.Layers.Length;
			for (int i = 0; i < num; i++)
			{
				Layer layer = tileMap.Layers[i];
				LayerInfo layerInfo = tileMap.data.Layers[i];
				if (layer.IsEmpty || layerInfo.skipMeshGeneration)
				{
					continue;
				}
				for (int j = 0; j < layer.numRows; j++)
				{
					int baseY = j * layer.divY;
					for (int k = 0; k < layer.numColumns; k++)
					{
						int baseX = k * layer.divX;
						SpriteChunk chunk = layer.GetChunk(k, j);
						if (!chunk.IsEmpty && (forceBuild || chunk.Dirty))
						{
							SpawnAnimatedTilesForChunk(tileMap, chunk, baseX, baseY, i);
						}
					}
				}
			}
		}

		public static void SpawnAnimatedTilesForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY, int layer)
		{
			LayerInfo layerInfo = tileMap.data.Layers[layer];
			if (layerInfo.ForceNonAnimating)
			{
				return;
			}
			int[] spriteIds = chunk.spriteIds;
			float x = 0f;
			float y = 0f;
			tileMap.data.GetTileOffset(out x, out y);
			List<Material> list = new List<Material>();
			for (int i = 0; i < tileMap.partitionSizeY; i++)
			{
				for (int j = 0; j < tileMap.partitionSizeX; j++)
				{
					int tileFromRawTile = GetTileFromRawTile(spriteIds[i * tileMap.partitionSizeX + j]);
					if (tileFromRawTile < 0)
					{
						continue;
					}
					if (tileFromRawTile >= tileMap.SpriteCollectionInst.spriteDefinitions.Length)
					{
						Debug.Log(tileFromRawTile + " tile is oob");
						continue;
					}
					tk2dSpriteDefinition tk2dSpriteDefinition = tileMap.SpriteCollectionInst.spriteDefinitions[tileFromRawTile];
					if (tk2dSpriteDefinition.metadata.usesAnimSequence && !list.Contains(tk2dSpriteDefinition.materialInst))
					{
						list.Add(tk2dSpriteDefinition.materialInst);
					}
				}
			}
			while (list.Count > 0)
			{
				ProcGenMeshData procGenMeshData = null;
				Material material = list[0];
				list.RemoveAt(0);
				List<TilemapAnimatorTileManager> list2 = new List<TilemapAnimatorTileManager>();
				bool flag = false;
				int unityLayer = layerInfo.unityLayer;
				for (int k = 0; k < tileMap.partitionSizeY; k++)
				{
					for (int l = 0; l < tileMap.partitionSizeX; l++)
					{
						IntVector2 intVector = new IntVector2(baseX + l, baseY + k);
						int tileFromRawTile2 = GetTileFromRawTile(spriteIds[k * tileMap.partitionSizeX + l]);
						if (tileFromRawTile2 < 0)
						{
							continue;
						}
						if (tileFromRawTile2 >= tileMap.SpriteCollectionInst.spriteDefinitions.Length)
						{
							Debug.Log(tileFromRawTile2 + " tile is oob");
							continue;
						}
						tk2dSpriteDefinition tk2dSpriteDefinition2 = tileMap.SpriteCollectionInst.spriteDefinitions[tileFromRawTile2];
						if (tk2dSpriteDefinition2.materialInst != material)
						{
							continue;
						}
						if (tk2dSpriteDefinition2.metadata.usesAnimSequence)
						{
							if (procGenMeshData == null)
							{
								procGenMeshData = new ProcGenMeshData();
							}
							TilemapAnimatorTileManager tilemapAnimatorTileManager = new TilemapAnimatorTileManager(tileMap.SpriteCollectionInst, tileFromRawTile2, tk2dSpriteDefinition2.metadata, procGenMeshData.vertices.Count, tk2dSpriteDefinition2.uvs.Length, tileMap);
							tilemapAnimatorTileManager.worldPosition = intVector;
							if (TK2DTilemapChunkAnimator.PositionToAnimatorMap.ContainsKey(intVector))
							{
								TK2DTilemapChunkAnimator.PositionToAnimatorMap[intVector].Add(tilemapAnimatorTileManager);
							}
							else
							{
								List<TilemapAnimatorTileManager> list3 = new List<TilemapAnimatorTileManager>();
								list3.Add(tilemapAnimatorTileManager);
								TK2DTilemapChunkAnimator.PositionToAnimatorMap.Add(intVector, list3);
							}
							bool flag2 = false;
							for (int m = 0; m < list2.Count; m++)
							{
								TilemapAnimatorTileManager tilemapAnimatorTileManager2 = list2[m];
								List<IndexNeighborDependency> dependencies = tilemapAnimatorTileManager2.associatedCollection.GetDependencies(tilemapAnimatorTileManager2.associatedSpriteId);
								if (dependencies == null || dependencies.Count <= 0)
								{
									continue;
								}
								int num = 0;
								while (num < dependencies.Count)
								{
									if (!(tilemapAnimatorTileManager2.worldPosition + DungeonData.GetIntVector2FromDirection(dependencies[num].neighborDirection) == intVector))
									{
										num++;
										continue;
									}
									goto IL_02ea;
								}
								continue;
								IL_02ea:
								flag2 = true;
								tilemapAnimatorTileManager2.linkedManagers.Add(tilemapAnimatorTileManager);
								break;
							}
							if (!flag2)
							{
								list2.Add(tilemapAnimatorTileManager);
							}
							if (AddSquareToAnimChunk(tileMap, chunk, tk2dSpriteDefinition2, baseX, baseY, x, l, k, layer, procGenMeshData))
							{
								flag = true;
							}
						}
						if (tk2dSpriteDefinition2.metadata.usesPerTileVFX && Random.value < tk2dSpriteDefinition2.metadata.tileVFXChance)
						{
							TileVFXManager.Instance.RegisterCellVFX(intVector, tk2dSpriteDefinition2.metadata);
						}
					}
				}
				if (layerInfo.unityLayer == 19 || layerInfo.unityLayer == 20)
				{
					flag = false;
				}
				if (procGenMeshData != null)
				{
					GameObject gameObject = new GameObject("anim chunk data");
					gameObject.transform.parent = tileMap.Layers[layer].gameObject.transform;
					if (flag)
					{
						gameObject.layer = LayerMask.NameToLayer("ShadowCaster");
					}
					else
					{
						gameObject.layer = unityLayer;
					}
					gameObject.transform.localPosition = new Vector3(baseX, baseY, baseY);
					MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
					MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
					Mesh mesh = new Mesh();
					mesh.vertices = procGenMeshData.vertices.ToArray();
					mesh.triangles = procGenMeshData.triangles.ToArray();
					mesh.uv = procGenMeshData.uvs.ToArray();
					mesh.colors = procGenMeshData.colors.ToArray();
					mesh.RecalculateBounds();
					mesh.RecalculateNormals();
					meshFilter.mesh = mesh;
					meshRenderer.material = material;
					for (int n = 0; n < meshRenderer.materials.Length; n++)
					{
						meshRenderer.materials[n].renderQueue += layerInfo.renderQueueOffset;
					}
					TK2DTilemapChunkAnimator tK2DTilemapChunkAnimator = gameObject.AddComponent<TK2DTilemapChunkAnimator>();
					tK2DTilemapChunkAnimator.Initialize(list2, mesh, tileMap);
				}
			}
		}

		private static bool AddSquareToAnimChunk(tk2dTileMap tileMap, SpriteChunk chunk, tk2dSpriteDefinition sprite, int baseX, int baseY, float xOffsetMult, int x, int y, int layer, ProcGenMeshData genMeshData)
		{
			bool result = false;
			LayerInfo layerInfo = tileMap.data.Layers[layer];
			int count = genMeshData.vertices.Count;
			float num = (float)((baseY + y) & 1) * xOffsetMult;
			Vector3[] array = sprite.ConstructExpensivePositions();
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector = new Vector3(tileMap.data.tileSize.x * ((float)x + num), tileMap.data.tileSize.y * (float)y, 0f);
				Vector3 vector2 = ApplySpriteVertexTileFlags(tileMap, sprite, array[i], false, false, false);
				Vector3 item = vector;
				IntVector2 intVector = vector.IntXY() + new IntVector2(baseX, baseY);
				CellData cellData = ((!GameManager.Instance.Dungeon.data.CheckInBounds(intVector, 1)) ? null : GameManager.Instance.Dungeon.data[intVector]);
				if (cellData != null && cellData.IsAnyFaceWall())
				{
					if (GameManager.Instance.Dungeon.data.isFaceWallHigher(cellData.position.x, cellData.position.y))
					{
						if (i > 1)
						{
							genMeshData.colors.Add(new Color(0f, 1f, 1f));
						}
						else
						{
							genMeshData.colors.Add(new Color(0f, 0.5f, 1f));
						}
					}
					else if (i > 1)
					{
						genMeshData.colors.Add(new Color(0f, 0.5f, 1f));
					}
					else
					{
						genMeshData.colors.Add(new Color(0f, 0f, 1f));
					}
					result = true;
					BraveUtility.DrawDebugSquare(intVector.ToVector2(), Color.blue, 1000f);
				}
				else
				{
					genMeshData.colors.Add(Color.black);
				}
				if (tileMap.isGungeonTilemap)
				{
					if (cellData != null && GameManager.Instance.Dungeon.data.isAnyFaceWall(intVector.x, intVector.y))
					{
						Vector3 vector3 = ((!GameManager.Instance.Dungeon.data.isFaceWallHigher(intVector.x, intVector.y)) ? new Vector3(0f, 0f, 1f) : new Vector3(0f, 0f, -1f));
						if (cellData.diagonalWallType == DiagonalWallType.NORTHEAST)
						{
							vector3.z += (1f - vector2.x) * 2f;
						}
						else if (cellData.diagonalWallType == DiagonalWallType.NORTHWEST)
						{
							vector3.z += vector2.x * 2f;
						}
						item += new Vector3(0f, 0f, vector.y - vector2.y) + vector2 + vector3;
					}
					else if (cellData != null && GameManager.Instance.Dungeon.data.isTopDiagonalWall(intVector.x, intVector.y) && layerInfo.name == "Collision Layer")
					{
						Vector3 vector4 = new Vector3(0f, 0f, -3f);
						item += new Vector3(0f, 0f, vector.y + vector2.y) + vector2 + vector4;
					}
					else if (layerInfo.name == "Pit Layer")
					{
						Vector3 vector5 = new Vector3(0f, 0f, 2f);
						if (GameManager.Instance.Dungeon.data.CheckInBounds(intVector.x, intVector.y + 2))
						{
							if (GameManager.Instance.Dungeon.data.cellData[intVector.x][intVector.y + 1].type != CellType.PIT || GameManager.Instance.Dungeon.data.cellData[intVector.x][intVector.y + 2].type != CellType.PIT)
							{
								bool flag = GameManager.Instance.Dungeon.data.cellData[intVector.x][intVector.y + 1].type != CellType.PIT;
								if (GameManager.Instance.Dungeon.debugSettings.WALLS_ARE_PITS && GameManager.Instance.Dungeon.data.cellData[intVector.x][intVector.y + 1].isExitCell)
								{
									flag = false;
								}
								if (flag)
								{
									vector5 = new Vector3(0f, 0f, 0f);
								}
								item += new Vector3(0f, 0f, vector.y - vector2.y) + vector2 + vector5;
							}
							else
							{
								item += new Vector3(0f, 0f, vector.y + vector2.y + 1f) + vector2;
							}
						}
						else
						{
							item += new Vector3(0f, 0f, vector.y + vector2.y + 1f) + vector2;
						}
					}
					else
					{
						item += new Vector3(0f, 0f, vector.y + vector2.y) + vector2;
					}
				}
				else
				{
					item += vector2;
				}
				genMeshData.vertices.Add(item);
				genMeshData.uvs.Add(sprite.uvs[i]);
			}
			bool flag2 = false;
			for (int j = 0; j < sprite.indices.Length; j++)
			{
				int num2 = ((!flag2) ? j : (sprite.indices.Length - 1 - j));
				genMeshData.triangles.Add(count + sprite.indices[num2]);
			}
			return result;
		}

		public static void SpawnPrefabsForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY, int layer, int[] prefabCounts)
		{
			int[] spriteIds = chunk.spriteIds;
			GameObject[] tilePrefabs = tileMap.data.tilePrefabs;
			Vector3 tileSize = tileMap.data.tileSize;
			Transform transform = chunk.gameObject.transform;
			float x = 0f;
			float y = 0f;
			tileMap.data.GetTileOffset(out x, out y);
			for (int i = 0; i < tileMap.partitionSizeY; i++)
			{
				float num = (float)((baseY + i) & 1) * x;
				for (int j = 0; j < tileMap.partitionSizeX; j++)
				{
					int tileFromRawTile = GetTileFromRawTile(spriteIds[i * tileMap.partitionSizeX + j]);
					if (tileFromRawTile < 0 || tileFromRawTile >= tilePrefabs.Length)
					{
						continue;
					}
					Object @object = tilePrefabs[tileFromRawTile];
					if (!(@object != null))
					{
						continue;
					}
					prefabCounts[tileFromRawTile]++;
					GameObject gameObject = GetExistingTilePrefabInstance(tileMap, baseX + j, baseY + i, layer);
					bool flag = gameObject != null;
					if (gameObject == null)
					{
						gameObject = Object.Instantiate(@object, Vector3.zero, Quaternion.identity) as GameObject;
					}
					if (gameObject != null)
					{
						GameObject gameObject2 = @object as GameObject;
						Vector3 localPosition = new Vector3(tileSize.x * ((float)j + num), tileSize.y * (float)i, 0f);
						bool flag2 = false;
						TileInfo tileInfoForSprite = tileMap.data.GetTileInfoForSprite(tileFromRawTile);
						if (tileInfoForSprite != null)
						{
							flag2 = tileInfoForSprite.enablePrefabOffset;
						}
						if (flag2 && gameObject2 != null)
						{
							localPosition += gameObject2.transform.position;
						}
						if (!flag)
						{
							gameObject.name = @object.name + " " + prefabCounts[tileFromRawTile];
						}
						tk2dUtil.SetTransformParent(gameObject.transform, transform);
						gameObject.transform.localPosition = localPosition;
						TilePrefabsX.Add(baseX + j);
						TilePrefabsY.Add(baseY + i);
						TilePrefabsLayer.Add(layer);
						TilePrefabsInstance.Add(gameObject);
					}
				}
			}
		}

		public static void SpawnPrefabs(tk2dTileMap tileMap, bool forceBuild)
		{
			TilePrefabsX = new List<int>();
			TilePrefabsY = new List<int>();
			TilePrefabsLayer = new List<int>();
			TilePrefabsInstance = new List<GameObject>();
			int[] prefabCounts = new int[tileMap.data.tilePrefabs.Length];
			int num = tileMap.Layers.Length;
			for (int i = 0; i < num; i++)
			{
				Layer layer = tileMap.Layers[i];
				LayerInfo layerInfo = tileMap.data.Layers[i];
				if (layer.IsEmpty || layerInfo.skipMeshGeneration)
				{
					continue;
				}
				for (int j = 0; j < layer.numRows; j++)
				{
					int baseY = j * layer.divY;
					for (int k = 0; k < layer.numColumns; k++)
					{
						int baseX = k * layer.divX;
						SpriteChunk chunk = layer.GetChunk(k, j);
						if (!chunk.IsEmpty && (forceBuild || chunk.Dirty))
						{
							SpawnPrefabsForChunk(tileMap, chunk, baseX, baseY, i, prefabCounts);
						}
					}
				}
			}
			tileMap.SetTilePrefabsList(TilePrefabsX, TilePrefabsY, TilePrefabsLayer, TilePrefabsInstance);
		}

		public static void HideTileMapPrefabs(tk2dTileMap tileMap)
		{
			if (tileMap.renderData == null || tileMap.Layers == null)
			{
				return;
			}
			if (tileMap.PrefabsRoot == null)
			{
				GameObject gameObject2 = (tileMap.PrefabsRoot = tk2dUtil.CreateGameObject("Prefabs"));
				GameObject gameObject3 = gameObject2;
				gameObject3.transform.parent = tileMap.renderData.transform;
				gameObject3.transform.localPosition = Vector3.zero;
				gameObject3.transform.localRotation = Quaternion.identity;
				gameObject3.transform.localScale = Vector3.one;
			}
			int tilePrefabsListCount = tileMap.GetTilePrefabsListCount();
			bool[] array = new bool[tilePrefabsListCount];
			for (int i = 0; i < tileMap.Layers.Length; i++)
			{
				Layer layer = tileMap.Layers[i];
				for (int j = 0; j < layer.spriteChannel.chunks.Length; j++)
				{
					SpriteChunk spriteChunk = layer.spriteChannel.chunks[j];
					if (spriteChunk.gameObject == null)
					{
						continue;
					}
					Transform transform = spriteChunk.gameObject.transform;
					int childCount = transform.childCount;
					for (int k = 0; k < childCount; k++)
					{
						GameObject gameObject4 = transform.GetChild(k).gameObject;
						for (int l = 0; l < tilePrefabsListCount; l++)
						{
							int x;
							int y;
							int layer2;
							GameObject instance;
							tileMap.GetTilePrefabsListItem(l, out x, out y, out layer2, out instance);
							if (instance == gameObject4)
							{
								array[l] = true;
								break;
							}
						}
					}
				}
			}
			Object[] tilePrefabs = tileMap.data.tilePrefabs;
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			List<GameObject> list4 = new List<GameObject>();
			for (int m = 0; m < tilePrefabsListCount; m++)
			{
				int x2;
				int y2;
				int layer3;
				GameObject instance2;
				tileMap.GetTilePrefabsListItem(m, out x2, out y2, out layer3, out instance2);
				if (!array[m])
				{
					int num = ((x2 < 0 || x2 >= tileMap.width || y2 < 0 || y2 >= tileMap.height) ? (-1) : tileMap.GetTile(x2, y2, layer3));
					if (num >= 0 && num < tilePrefabs.Length && tilePrefabs[num] != null)
					{
						array[m] = true;
					}
				}
				if (array[m])
				{
					list.Add(x2);
					list2.Add(y2);
					list3.Add(layer3);
					list4.Add(instance2);
					tk2dUtil.SetTransformParent(instance2.transform, tileMap.PrefabsRoot.transform);
				}
			}
			tileMap.SetTilePrefabsList(list, list2, list3, list4);
		}

		private static Vector3 GetTilePosition(tk2dTileMap tileMap, int x, int y)
		{
			return new Vector3(tileMap.data.tileSize.x * (float)x, tileMap.data.tileSize.y * (float)y, 0f);
		}

		public static void CreateOverrideChunkData(SpriteChunk chunk, tk2dTileMap tileMap, int layerId, string overrideChunkName)
		{
			Layer layer = tileMap.Layers[layerId];
			bool flag = layer.IsEmpty || chunk.IsEmpty;
			if (flag && chunk.HasGameData)
			{
				chunk.DestroyGameData(tileMap);
			}
			else if (!flag && chunk.gameObject == null)
			{
				string name = "Chunk_" + overrideChunkName + "_" + tileMap.data.Layers[layerId].name;
				GameObject gameObject = (chunk.gameObject = tk2dUtil.CreateGameObject(name));
				gameObject.transform.parent = layer.gameObject.transform;
				MeshFilter meshFilter = tk2dUtil.AddComponent<MeshFilter>(gameObject);
				tk2dUtil.AddComponent<MeshRenderer>(gameObject);
				chunk.mesh = tk2dUtil.CreateMesh();
				meshFilter.mesh = chunk.mesh;
			}
			if (chunk.gameObject != null)
			{
				Vector3 localPosition = new Vector3(chunk.startX, chunk.startY, 0f);
				chunk.gameObject.transform.localPosition = localPosition;
				chunk.gameObject.transform.localRotation = Quaternion.identity;
				chunk.gameObject.transform.localScale = Vector3.one;
				chunk.gameObject.layer = tileMap.data.Layers[layerId].unityLayer;
			}
			if (chunk.gameObject != null && chunk.roomReference != null)
			{
				chunk.gameObject.transform.parent = chunk.roomReference.hierarchyParent;
			}
		}

		public static void CreateRenderData(tk2dTileMap tileMap, bool editMode, Dictionary<Layer, bool> layersActive)
		{
			if (tileMap.renderData == null)
			{
				GameObject gameObject = GameObject.Find(tileMap.name + " Render Data");
				if (gameObject != null)
				{
					tileMap.renderData = gameObject;
				}
				else
				{
					tileMap.renderData = tk2dUtil.CreateGameObject(tileMap.name + " Render Data");
				}
			}
			tileMap.renderData.transform.position = tileMap.transform.position;
			float num = 0f;
			int num2 = 0;
			Layer[] layers = tileMap.Layers;
			foreach (Layer layer in layers)
			{
				float z = tileMap.data.Layers[num2].z;
				if (num2 != 0)
				{
					num -= z;
				}
				if (layer.IsEmpty && layer.gameObject != null)
				{
					tk2dUtil.DestroyImmediate(layer.gameObject);
					layer.gameObject = null;
				}
				else if (!layer.IsEmpty && layer.gameObject == null)
				{
					Transform transform = tileMap.renderData.transform.Find(tileMap.data.Layers[num2].name);
					if (transform != null)
					{
						layer.gameObject = transform.gameObject;
					}
					else
					{
						(layer.gameObject = tk2dUtil.CreateGameObject(string.Empty)).transform.parent = tileMap.renderData.transform;
					}
				}
				int unityLayer = tileMap.data.Layers[num2].unityLayer;
				if (layer.gameObject != null)
				{
					if (!editMode && layersActive.ContainsKey(layer) && layer.gameObject.activeSelf != layersActive[layer])
					{
						layer.gameObject.SetActive(layersActive[layer]);
					}
					layer.gameObject.name = tileMap.data.Layers[num2].name;
					layer.gameObject.transform.localPosition = new Vector3(0f, 0f, (!tileMap.data.layersFixedZ) ? num : (0f - z));
					layer.gameObject.transform.localRotation = Quaternion.identity;
					layer.gameObject.transform.localScale = Vector3.one;
					layer.gameObject.layer = unityLayer;
				}
				int x;
				int x2;
				int dx;
				int y;
				int y2;
				int dy;
				GetLoopOrder(tileMap.data.sortMethod, layer.numColumns, layer.numRows, out x, out x2, out dx, out y, out y2, out dy);
				float num3 = 0f;
				for (int j = y; j != y2; j += dy)
				{
					for (int k = x; k != x2; k += dx)
					{
						SpriteChunk chunk = layer.GetChunk(k, j);
						bool flag = layer.IsEmpty || chunk.IsEmpty;
						if (editMode)
						{
							flag = false;
						}
						if (flag && chunk.HasGameData)
						{
							chunk.DestroyGameData(tileMap);
						}
						else if (!flag && chunk.gameObject == null)
						{
							string name = "Chunk " + j + " " + k;
							GameObject gameObject2 = (chunk.gameObject = tk2dUtil.CreateGameObject(name));
							gameObject2.transform.parent = layer.gameObject.transform;
							MeshFilter meshFilter = tk2dUtil.AddComponent<MeshFilter>(gameObject2);
							tk2dUtil.AddComponent<MeshRenderer>(gameObject2);
							chunk.mesh = tk2dUtil.CreateMesh();
							meshFilter.mesh = chunk.mesh;
						}
						if (chunk.gameObject != null)
						{
							Vector3 tilePosition = GetTilePosition(tileMap, k * tileMap.partitionSizeX, j * tileMap.partitionSizeY);
							tilePosition.z += num3;
							chunk.gameObject.transform.localPosition = tilePosition;
							chunk.gameObject.transform.localRotation = Quaternion.identity;
							chunk.gameObject.transform.localScale = Vector3.one;
							chunk.gameObject.layer = unityLayer;
							if (editMode)
							{
								chunk.DestroyColliderData(tileMap);
							}
						}
						num3 -= 1E-06f;
					}
				}
				num2++;
			}
		}

		public static void GetLoopOrder(tk2dTileMapData.SortMethod sortMethod, int w, int h, out int x0, out int x1, out int dx, out int y0, out int y1, out int dy)
		{
			switch (sortMethod)
			{
			case tk2dTileMapData.SortMethod.BottomLeft:
				x0 = 0;
				x1 = w;
				dx = 1;
				y0 = 0;
				y1 = h;
				dy = 1;
				break;
			case tk2dTileMapData.SortMethod.BottomRight:
				x0 = w - 1;
				x1 = -1;
				dx = -1;
				y0 = 0;
				y1 = h;
				dy = 1;
				break;
			case tk2dTileMapData.SortMethod.TopLeft:
				x0 = 0;
				x1 = w;
				dx = 1;
				y0 = h - 1;
				y1 = -1;
				dy = -1;
				break;
			case tk2dTileMapData.SortMethod.TopRight:
				x0 = w - 1;
				x1 = -1;
				dx = -1;
				y0 = h - 1;
				y1 = -1;
				dy = -1;
				break;
			default:
				Debug.LogError("Unhandled sort method");
				goto case tk2dTileMapData.SortMethod.BottomLeft;
			}
		}

		public static int GetTileFromRawTile(int rawTile)
		{
			if (rawTile == -1)
			{
				return -1;
			}
			return rawTile & 0xFFFFFF;
		}

		public static bool IsRawTileFlagSet(int rawTile, tk2dTileFlags flag)
		{
			if (rawTile == -1)
			{
				return false;
			}
			return ((uint)rawTile & (uint)flag) != 0;
		}

		public static void SetRawTileFlag(ref int rawTile, tk2dTileFlags flag, bool setValue)
		{
			if (rawTile != -1)
			{
				rawTile = ((!setValue) ? (rawTile & (int)(~flag)) : (rawTile | (int)flag));
			}
		}

		public static void InvertRawTileFlag(ref int rawTile, tk2dTileFlags flag)
		{
			if (rawTile != -1)
			{
				bool flag2 = ((uint)rawTile & (uint)flag) == 0;
				rawTile = ((!flag2) ? (rawTile & (int)(~flag)) : (rawTile | (int)flag));
			}
		}

		public static Vector3 ApplySpriteVertexTileFlags(tk2dTileMap tileMap, tk2dSpriteDefinition spriteDef, Vector3 pos, bool flipH, bool flipV, bool rot90)
		{
			float num = tileMap.data.tileOrigin.x + 0.5f * tileMap.data.tileSize.x;
			float num2 = tileMap.data.tileOrigin.y + 0.5f * tileMap.data.tileSize.y;
			float num3 = pos.x - num;
			float num4 = pos.y - num2;
			if (rot90)
			{
				float num5 = num3;
				num3 = num4;
				num4 = 0f - num5;
			}
			if (flipH)
			{
				num3 *= -1f;
			}
			if (flipV)
			{
				num4 *= -1f;
			}
			pos.x = num + num3;
			pos.y = num2 + num4;
			return pos;
		}

		public static Vector2 ApplySpriteVertexTileFlags(tk2dTileMap tileMap, tk2dSpriteDefinition spriteDef, Vector2 pos, bool flipH, bool flipV, bool rot90)
		{
			float num = tileMap.data.tileOrigin.x + 0.5f * tileMap.data.tileSize.x;
			float num2 = tileMap.data.tileOrigin.y + 0.5f * tileMap.data.tileSize.y;
			float num3 = pos.x - num;
			float num4 = pos.y - num2;
			if (rot90)
			{
				float num5 = num3;
				num3 = num4;
				num4 = 0f - num5;
			}
			if (flipH)
			{
				num3 *= -1f;
			}
			if (flipV)
			{
				num4 *= -1f;
			}
			pos.x = num + num3;
			pos.y = num2 + num4;
			return pos;
		}
	}
}
