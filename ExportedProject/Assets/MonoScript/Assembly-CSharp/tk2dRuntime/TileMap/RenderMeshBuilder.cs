using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace tk2dRuntime.TileMap
{
	public static class RenderMeshBuilder
	{
		public static int CurrentCellXOffset;

		public static int CurrentCellYOffset;

		public static void BuildForChunk(tk2dTileMap tileMap, SpriteChunk chunk, bool useColor, bool skipPrefabs, int baseX, int baseY, LayerInfo layerData)
		{
			GameManager instance = GameManager.Instance;
			Dungeon dungeon = instance.Dungeon;
			List<Vector3> list = new List<Vector3>();
			List<Color> list2 = new List<Color>();
			List<Vector2> list3 = new List<Vector2>();
			if (layerData.preprocessedFlags == null || layerData.preprocessedFlags.Length == 0)
			{
				layerData.preprocessedFlags = new bool[tileMap.width * tileMap.height];
			}
			int[] spriteIds = chunk.spriteIds;
			Vector3 tileSize = tileMap.data.tileSize;
			int num = tileMap.SpriteCollectionInst.spriteDefinitions.Length;
			Object[] tilePrefabs = tileMap.data.tilePrefabs;
			tk2dSpriteDefinition firstValidDefinition = tileMap.SpriteCollectionInst.FirstValidDefinition;
			bool flag = firstValidDefinition != null && firstValidDefinition.normals != null && firstValidDefinition.normals.Length > 0;
			Color32 color = ((!useColor || tileMap.ColorChannel == null) ? Color.white : tileMap.ColorChannel.clearColor);
			int x;
			int x2;
			int dx;
			int y;
			int y2;
			int dy;
			BuilderUtil.GetLoopOrder(tileMap.data.sortMethod, chunk.Width, chunk.Height, out x, out x2, out dx, out y, out y2, out dy);
			float x3 = 0f;
			float y3 = 0f;
			tileMap.data.GetTileOffset(out x3, out y3);
			List<int>[] array = new List<int>[tileMap.SpriteCollectionInst.materials.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new List<int>();
			}
			IntVector2 intVector = new IntVector2(layerData.overrideChunkXOffset, layerData.overrideChunkYOffset);
			int num2 = tileMap.partitionSizeX + 1;
			for (int j = y; j != y2; j += dy)
			{
				float num3 = (float)((baseY + j) & 1) * x3;
				for (int k = x; k != x2; k += dx)
				{
					Vector3 vector = new Vector3(tileSize.x * ((float)k + num3), tileSize.y * (float)j, 0f);
					IntVector2 intVector2 = IntVector2.Zero;
					if (tileMap.isGungeonTilemap)
					{
						intVector2 = vector.IntXY() + new IntVector2(baseX, baseY);
						if ((chunk.roomReference != null && !chunk.roomReference.ContainsPosition(intVector2 - intVector)) || intVector2.y * tileMap.width + intVector2.x >= layerData.preprocessedFlags.Length || layerData.preprocessedFlags[intVector2.y * tileMap.width + intVector2.x])
						{
							continue;
						}
						layerData.preprocessedFlags[intVector2.y * tileMap.width + intVector2.x] = true;
					}
					int rawTile = spriteIds[j * chunk.Width + k];
					int tileFromRawTile = BuilderUtil.GetTileFromRawTile(rawTile);
					bool flag2 = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.FlipX);
					bool flag3 = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.FlipY);
					bool rot = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.Rot90);
					ColorChunk colorChunk = null;
					colorChunk = ((!tileMap.isGungeonTilemap) ? tileMap.ColorChannel.GetChunk(Mathf.FloorToInt((float)baseX / (float)tileMap.partitionSizeX), Mathf.FloorToInt((float)baseY / (float)tileMap.partitionSizeY)) : tileMap.ColorChannel.GetChunk(Mathf.FloorToInt((float)intVector2.x / (float)tileMap.partitionSizeX), Mathf.FloorToInt((float)intVector2.y / (float)tileMap.partitionSizeY)));
					bool flag4 = useColor;
					if (colorChunk == null || (colorChunk.colors.Length == 0 && colorChunk.colorOverrides.GetLength(0) == 0))
					{
						flag4 = false;
					}
					if (tileFromRawTile < 0 || tileFromRawTile >= num || (skipPrefabs && (bool)tilePrefabs[tileFromRawTile]))
					{
						continue;
					}
					tk2dSpriteDefinition tk2dSpriteDefinition = tileMap.SpriteCollectionInst.spriteDefinitions[tileFromRawTile];
					if (!layerData.ForceNonAnimating && tk2dSpriteDefinition.metadata.usesAnimSequence)
					{
						continue;
					}
					int count = list.Count;
					Vector3[] array2 = tk2dSpriteDefinition.ConstructExpensivePositions();
					for (int l = 0; l < array2.Length; l++)
					{
						Vector3 vector2 = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, tk2dSpriteDefinition, array2[l], flag2, flag3, rot);
						if (flag4)
						{
							IntVector2 intVector3 = new IntVector2(k, j);
							if (tileMap.isGungeonTilemap)
							{
								intVector3 = new IntVector2(intVector2.x % tileMap.partitionSizeX, intVector2.y % tileMap.partitionSizeY);
							}
							int num4 = l % 4;
							Color32 color2 = colorChunk.colorOverrides[intVector3.y * num2 + intVector3.x, num4];
							if (tileMap.isGungeonTilemap && (color2.r != color.r || color2.g != color.g || color2.b != color.b || color2.a != color.a))
							{
								Color item = color2;
								list2.Add(item);
							}
							else
							{
								Color a = colorChunk.colors[intVector3.y * num2 + intVector3.x];
								Color b = colorChunk.colors[intVector3.y * num2 + intVector3.x + 1];
								Color a2 = colorChunk.colors[(intVector3.y + 1) * num2 + intVector3.x];
								Color b2 = colorChunk.colors[(intVector3.y + 1) * num2 + (intVector3.x + 1)];
								Vector3 vector3 = vector2 - tk2dSpriteDefinition.untrimmedBoundsDataCenter;
								Vector3 vector4 = vector3 + tileMap.data.tileSize * 0.5f;
								float t = Mathf.Clamp01(vector4.x / tileMap.data.tileSize.x);
								float t2 = Mathf.Clamp01(vector4.y / tileMap.data.tileSize.y);
								Color item2 = Color.Lerp(Color.Lerp(a, b, t), Color.Lerp(a2, b2, t), t2);
								list2.Add(item2);
							}
						}
						else
						{
							list2.Add(Color.black);
						}
						Vector3 item3 = vector;
						if (tileMap.isGungeonTilemap)
						{
							IntVector2 intVector4 = vector.IntXY() + new IntVector2(baseX + CurrentCellXOffset, baseY + CurrentCellYOffset);
							if (dungeon.data.CheckInBounds(intVector4, 1) && dungeon.data.isAnyFaceWall(intVector4.x, intVector4.y))
							{
								Vector3 vector5 = ((!dungeon.data.isFaceWallHigher(intVector4.x, intVector4.y)) ? new Vector3(0f, 0f, 1f) : new Vector3(0f, 0f, -1f));
								CellData cellData = dungeon.data[intVector4];
								if (cellData.diagonalWallType == DiagonalWallType.NORTHEAST)
								{
									vector5.z += (1f - vector2.x) * 2f;
								}
								else if (cellData.diagonalWallType == DiagonalWallType.NORTHWEST)
								{
									vector5.z += vector2.x * 2f;
								}
								item3 += new Vector3(0f, 0f, vector.y - vector2.y) + vector2 + vector5;
							}
							else if (dungeon.data.CheckInBounds(intVector4, 1) && dungeon.data.isTopDiagonalWall(intVector4.x, intVector4.y) && layerData.name == "Collision Layer")
							{
								Vector3 vector6 = new Vector3(0f, 0f, -3f);
								item3 += new Vector3(0f, 0f, vector.y + vector2.y) + vector2 + vector6;
							}
							else if (layerData.name == "AOandShadows")
							{
								if (dungeon.data.CheckInBounds(intVector4, 1) && dungeon.data[intVector4] != null && dungeon.data[intVector4].type == CellType.PIT)
								{
									Vector3 vector7 = new Vector3(0f, 0f, 2.5f);
									item3 += new Vector3(0f, 0f, vector.y + vector2.y) + vector2 + vector7;
								}
								else
								{
									Vector3 vector8 = new Vector3(0f, 0f, 1f);
									item3 += new Vector3(0f, 0f, vector.y + vector2.y) + vector2 + vector8;
								}
							}
							else if (layerData.name == "Pit Layer")
							{
								Vector3 vector9 = new Vector3(0f, 0f, 2f);
								if (dungeon.data.CheckInBounds(intVector4.x, intVector4.y + 2))
								{
									if (dungeon.data.cellData[intVector4.x][intVector4.y + 1].type != CellType.PIT || dungeon.data.cellData[intVector4.x][intVector4.y + 2].type != CellType.PIT)
									{
										bool flag5 = dungeon.data.cellData[intVector4.x][intVector4.y + 1].type != CellType.PIT;
										if (dungeon.debugSettings.WALLS_ARE_PITS && dungeon.data.cellData[intVector4.x][intVector4.y + 1].isExitCell)
										{
											flag5 = false;
										}
										if (flag5)
										{
											vector9 = new Vector3(0f, 0f, 0f);
										}
										item3 += new Vector3(0f, 0f, vector.y - vector2.y) + vector2 + vector9;
									}
									else
									{
										item3 += new Vector3(0f, 0f, vector.y + vector2.y + 1f) + vector2;
									}
								}
								else
								{
									item3 += new Vector3(0f, 0f, vector.y + vector2.y + 1f) + vector2;
								}
							}
							else
							{
								item3 += new Vector3(0f, 0f, vector.y + vector2.y) + vector2;
							}
						}
						else
						{
							item3 += vector2;
						}
						list.Add(item3);
						list3.Add(tk2dSpriteDefinition.uvs[l]);
					}
					bool flag6 = false;
					if (flag2)
					{
						flag6 = !flag6;
					}
					if (flag3)
					{
						flag6 = !flag6;
					}
					List<int> list4 = array[tk2dSpriteDefinition.materialId];
					for (int m = 0; m < tk2dSpriteDefinition.indices.Length; m++)
					{
						int num5 = ((!flag6) ? m : (tk2dSpriteDefinition.indices.Length - 1 - m));
						list4.Add(count + tk2dSpriteDefinition.indices[num5]);
					}
				}
			}
			if (chunk.mesh == null)
			{
				chunk.mesh = tk2dUtil.CreateMesh();
			}
			chunk.mesh.vertices = list.ToArray();
			chunk.mesh.uv = list3.ToArray();
			chunk.mesh.colors = list2.ToArray();
			List<Material> list5 = new List<Material>();
			int num6 = 0;
			int num7 = 0;
			List<int>[] array3 = array;
			foreach (List<int> list6 in array3)
			{
				if (list6.Count > 0)
				{
					list5.Add(tileMap.SpriteCollectionInst.materialInsts[num6]);
					num7++;
				}
				num6++;
			}
			if (num7 > 0)
			{
				chunk.mesh.subMeshCount = num7;
				chunk.gameObject.GetComponent<Renderer>().materials = list5.ToArray();
				int num8 = 0;
				List<int>[] array4 = array;
				foreach (List<int> list7 in array4)
				{
					if (list7.Count > 0)
					{
						chunk.mesh.SetTriangles(list7.ToArray(), num8);
						num8++;
					}
				}
			}
			chunk.mesh.RecalculateBounds();
			if (flag)
			{
				chunk.mesh.RecalculateNormals();
			}
			if (tileMap.isGungeonTilemap)
			{
				chunk.gameObject.transform.position = chunk.gameObject.transform.position.WithZ((float)baseY + chunk.gameObject.transform.position.z);
			}
			MeshFilter component = chunk.gameObject.GetComponent<MeshFilter>();
			component.sharedMesh = chunk.mesh;
		}

		public static IEnumerator Build(tk2dTileMap tileMap, bool editMode, bool forceBuild)
		{
			bool skipPrefabs = ((!editMode) ? true : false);
			bool incremental = !forceBuild;
			int numLayers = tileMap.data.NumLayers;
			for (int layerId = 0; layerId < numLayers; layerId++)
			{
				Layer layer = tileMap.Layers[layerId];
				if (layer.IsEmpty)
				{
					continue;
				}
				LayerInfo layerData = tileMap.data.Layers[layerId];
				bool useColor = !tileMap.ColorChannel.IsEmpty && tileMap.data.Layers[layerId].useColor;
				bool useSortingLayer = tileMap.data.useSortingLayers;
				if (tileMap.isGungeonTilemap && SpriteChunk.s_roomChunks != null && SpriteChunk.s_roomChunks.ContainsKey(tileMap.data.Layers[layerId]))
				{
					for (int i = 0; i < SpriteChunk.s_roomChunks[tileMap.data.Layers[layerId]].Count; i++)
					{
						SpriteChunk spriteChunk = SpriteChunk.s_roomChunks[tileMap.data.Layers[layerId]][i];
						if (!spriteChunk.IsEmpty)
						{
							BuildForChunk(tileMap, spriteChunk, useColor, skipPrefabs, spriteChunk.startX, spriteChunk.startY, tileMap.data.Layers[layerId]);
						}
					}
				}
				for (int j = 0; j < layer.numRows; j++)
				{
					int baseY = j * layer.divY;
					for (int k = 0; k < layer.numColumns; k++)
					{
						int baseX = k * layer.divX;
						SpriteChunk chunk = layer.GetChunk(k, j);
						ColorChunk chunk2 = tileMap.ColorChannel.GetChunk(k, j);
						bool flag = chunk2 != null && chunk2.Dirty;
						if (incremental && !flag && !chunk.Dirty)
						{
							continue;
						}
						if (chunk.mesh != null)
						{
							chunk.mesh.Clear();
						}
						if (chunk.IsEmpty || chunk.IrrelevantToGameplay)
						{
							continue;
						}
						if (editMode || (!editMode && !layerData.skipMeshGeneration))
						{
							BuildForChunk(tileMap, chunk, useColor, skipPrefabs, baseX, baseY, tileMap.data.Layers[layerId]);
							if (chunk.gameObject != null && useSortingLayer)
							{
								Renderer component = chunk.gameObject.GetComponent<Renderer>();
								if (component != null)
								{
									component.sortingLayerName = layerData.sortingLayerName;
									component.sortingOrder = layerData.sortingOrder;
								}
							}
						}
						if (chunk.mesh != null)
						{
							tileMap.TouchMesh(chunk.mesh);
						}
					}
				}
				yield return null;
			}
			for (int l = 0; l < numLayers; l++)
			{
				Layer layer2 = tileMap.Layers[l];
				if (!layer2.IsEmpty)
				{
					LayerInfo layerInfo = tileMap.data.Layers[l];
					layerInfo.preprocessedFlags = null;
				}
			}
		}
	}
}
