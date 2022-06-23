using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class TileIndices
	{
		public GlobalDungeonData.ValidTilesets tilesetId;

		public tk2dSpriteCollectionData dungeonCollection;

		public bool dungeonCollectionSupportsDiagonalWalls;

		public AOTileIndices aoTileIndices;

		public bool placeBorders = true;

		public bool placePits = true;

		public List<TileIndexVariant> chestHighWallIndices;

		public TileIndexGrid decalIndexGrid;

		public TileIndexGrid patternIndexGrid;

		public List<int> globalSecondBorderTiles;

		public TileIndexGrid edgeDecorationTiles;

		public bool PitAtPositionIsWater(Vector2 point)
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				return false;
			}
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(point.ToIntVector2());
			if (absoluteRoomFromPosition.RoomFallValidForMaintenance())
			{
				return false;
			}
			DungeonMaterial dungeonMaterial = GameManager.Instance.Dungeon.roomMaterialDefinitions[absoluteRoomFromPosition.RoomVisualSubtype];
			if (dungeonMaterial == null || dungeonMaterial.pitfallVFXPrefab == null)
			{
				return false;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.PVP_ENABLED)
			{
				return false;
			}
			if (dungeonMaterial.pitfallVFXPrefab.name.Contains("Splash"))
			{
				return true;
			}
			return false;
		}

		public GameObject DoSplashAtPosition(Vector2 point)
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				return null;
			}
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(point.ToIntVector2());
			if (absoluteRoomFromPosition.RoomFallValidForMaintenance())
			{
				return null;
			}
			DungeonMaterial dungeonMaterial = GameManager.Instance.Dungeon.roomMaterialDefinitions[absoluteRoomFromPosition.RoomVisualSubtype];
			if (dungeonMaterial == null || dungeonMaterial.pitfallVFXPrefab == null)
			{
				return null;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.PVP_ENABLED)
			{
				return null;
			}
			IntVector2 key = point.ToIntVector2(VectorConversions.Floor);
			CellData cellData = GameManager.Instance.Dungeon.data[key];
			if (cellData == null)
			{
				return null;
			}
			if (Time.realtimeSinceStartup - cellData.lastSplashTime < 0.25f)
			{
				return null;
			}
			cellData.lastSplashTime = Time.realtimeSinceStartup;
			GameObject pitfallVFXPrefab = dungeonMaterial.pitfallVFXPrefab;
			GameObject gameObject = SpawnManager.SpawnVFX(pitfallVFXPrefab, point.ToVector3ZUp(), Quaternion.identity);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.HeightOffGround = -4.0625f;
			component.PlaceAtPositionByAnchor(point, tk2dBaseSprite.Anchor.MiddleCenter);
			component.transform.position = component.transform.position.Quantize(1f / (float)PhysicsEngine.Instance.PixelsPerUnit);
			component.UpdateZDepth();
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON && dungeonMaterial.usesFacewallGrids && cellData.type != CellType.FLOOR)
			{
				GlobalSparksDoer.DoRandomParticleBurst(30, component.transform.position + new Vector3(-0.75f, -0.75f, 0f), component.transform.position + new Vector3(0.75f, 0.75f, 0f), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
			}
			return gameObject;
		}
	}
}
