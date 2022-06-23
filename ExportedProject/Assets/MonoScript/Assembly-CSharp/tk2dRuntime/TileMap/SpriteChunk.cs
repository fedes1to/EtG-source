using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace tk2dRuntime.TileMap
{
	[Serializable]
	public class SpriteChunk
	{
		public static Dictionary<LayerInfo, List<SpriteChunk>> s_roomChunks;

		private bool dirty;

		public int startX;

		public int startY;

		public int endX;

		public int endY;

		public RoomHandler roomReference;

		public int[] spriteIds;

		public bool[] chunkPreprocessFlags;

		public GameObject gameObject;

		public Mesh mesh;

		public MeshCollider meshCollider;

		public Mesh colliderMesh;

		public List<EdgeCollider2D> edgeColliders = new List<EdgeCollider2D>();

		public int Width
		{
			get
			{
				return endX - startX;
			}
		}

		public int Height
		{
			get
			{
				return endY - startY;
			}
		}

		public bool Dirty
		{
			get
			{
				return dirty;
			}
			set
			{
				dirty = value;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return spriteIds.Length == 0;
			}
		}

		public bool IrrelevantToGameplay
		{
			get
			{
				float num = float.MaxValue;
				for (int i = startX; i < endX; i++)
				{
					for (int j = startY; j < endY; j++)
					{
						IntVector2 intVector = new IntVector2(i + RenderMeshBuilder.CurrentCellXOffset, j + RenderMeshBuilder.CurrentCellYOffset);
						if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
						{
							num = Mathf.Min(num, GameManager.Instance.Dungeon.data[intVector].distanceFromNearestRoom);
						}
					}
				}
				if (num > 15f)
				{
					return true;
				}
				return false;
			}
		}

		public bool HasGameData
		{
			get
			{
				return gameObject != null || mesh != null || meshCollider != null || colliderMesh != null || edgeColliders.Count > 0;
			}
		}

		public SpriteChunk(int sX, int sY, int eX, int eY)
		{
			startX = sX;
			startY = sY;
			endX = eX;
			endY = eY;
			spriteIds = new int[0];
		}

		public static void ClearPerLevelData()
		{
			s_roomChunks = null;
		}

		public void DestroyGameData(tk2dTileMap tileMap)
		{
			if (mesh != null)
			{
				tileMap.DestroyMesh(mesh);
			}
			if (gameObject != null)
			{
				tk2dUtil.DestroyImmediate(gameObject);
			}
			gameObject = null;
			mesh = null;
			DestroyColliderData(tileMap);
		}

		public void DestroyColliderData(tk2dTileMap tileMap)
		{
			if (colliderMesh != null)
			{
				tileMap.DestroyMesh(colliderMesh);
			}
			if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh != colliderMesh)
			{
				tileMap.DestroyMesh(meshCollider.sharedMesh);
			}
			if (meshCollider != null)
			{
				tk2dUtil.DestroyImmediate(meshCollider);
			}
			meshCollider = null;
			colliderMesh = null;
			if (edgeColliders.Count > 0)
			{
				for (int i = 0; i < edgeColliders.Count; i++)
				{
					tk2dUtil.DestroyImmediate(edgeColliders[i]);
				}
				edgeColliders.Clear();
			}
		}
	}
}
