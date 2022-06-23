using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using tk2dRuntime.TileMap;
using UnityEngine;

public class TK2DDungeonAssembler
{
	private TileIndices t;

	private Dictionary<TilesetIndexMetadata.TilesetFlagType, List<Tuple<int, TilesetIndexMetadata>>> m_metadataLookupTable;

	private bool HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType flagType, int roomType)
	{
		if (m_metadataLookupTable[flagType] == null)
		{
			return false;
		}
		List<Tuple<int, TilesetIndexMetadata>> list = m_metadataLookupTable[flagType];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Second.dungeonRoomSubType == roomType || list[i].Second.secondRoomSubType == roomType || list[i].Second.thirdRoomSubType == roomType)
			{
				return true;
			}
		}
		return false;
	}

	public void Initialize(TileIndices indices)
	{
		m_metadataLookupTable = new Dictionary<TilesetIndexMetadata.TilesetFlagType, List<Tuple<int, TilesetIndexMetadata>>>();
		TilesetIndexMetadata.TilesetFlagType[] array = (TilesetIndexMetadata.TilesetFlagType[])Enum.GetValues(typeof(TilesetIndexMetadata.TilesetFlagType));
		for (int i = 0; i < array.Length; i++)
		{
			m_metadataLookupTable.Add(array[i], indices.dungeonCollection.GetIndicesForTileType(array[i]));
		}
		SecretRoomUtility.metadataLookupTableRef = m_metadataLookupTable;
		t = indices;
	}

	public bool BCheck(Dungeon d, int ix, int iy, int thresh)
	{
		if (d.data.CheckInBounds(new IntVector2(ix, iy), 3 + thresh))
		{
			return true;
		}
		return false;
	}

	public bool BCheck(Dungeon d, int ix, int iy)
	{
		return BCheck(d, ix, iy, 0);
	}

	public static void RuntimeResizeTileMap(tk2dTileMap tileMap, int w, int h, int partitionSizeX, int partitionSizeY)
	{
		Layer[] layers = tileMap.Layers;
		foreach (Layer layer in layers)
		{
			layer.DestroyGameData(tileMap);
			if (layer.gameObject != null)
			{
				tk2dUtil.DestroyImmediate(layer.gameObject);
				layer.gameObject = null;
			}
		}
		Layer[] array = new Layer[tileMap.Layers.Length];
		for (int j = 0; j < tileMap.Layers.Length; j++)
		{
			Layer layer2 = tileMap.Layers[j];
			array[j] = new Layer(layer2.hash, w, h, partitionSizeX, partitionSizeY);
			Layer layer3 = array[j];
			if (layer2.IsEmpty)
			{
				continue;
			}
			int num = Mathf.Min(tileMap.height, h);
			int num2 = Mathf.Min(tileMap.width, w);
			for (int k = 0; k < num; k++)
			{
				for (int l = 0; l < num2; l++)
				{
					layer3.SetRawTile(l, k, layer2.GetRawTile(l, k));
				}
			}
			layer3.Optimize();
		}
		bool flag = tileMap.ColorChannel != null && !tileMap.ColorChannel.IsEmpty;
		ColorChannel colorChannel = new ColorChannel(w, h, partitionSizeX, partitionSizeY);
		if (flag)
		{
			int num3 = Mathf.Min(tileMap.height, h) + 1;
			int num4 = Mathf.Min(tileMap.width, w) + 1;
			for (int m = 0; m < num3; m++)
			{
				for (int n = 0; n < num4; n++)
				{
					colorChannel.SetColor(n, m, tileMap.ColorChannel.GetColor(n, m));
				}
			}
			colorChannel.Optimize();
		}
		tileMap.ColorChannel = colorChannel;
		tileMap.Layers = array;
		tileMap.width = w;
		tileMap.height = h;
		tileMap.partitionSizeX = partitionSizeX;
		tileMap.partitionSizeY = partitionSizeY;
		tileMap.ForceBuild();
	}

	private StampIndexVariant GetIndexFromStampArray(CellData current, List<StampIndexVariant> list)
	{
		float num = current.UniqueHash;
		foreach (StampIndexVariant item in list)
		{
			num -= item.likelihood;
			if (num <= 0f)
			{
				return item;
			}
		}
		return list[0];
	}

	private TileIndexVariant GetIndexFromTileArray(CellData current, List<TileIndexVariant> list)
	{
		float uniqueHash = current.UniqueHash;
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			num += list[i].likelihood;
		}
		float num2 = uniqueHash * num;
		for (int j = 0; j < list.Count; j++)
		{
			num2 -= list[j].likelihood;
			if (num2 <= 0f)
			{
				return list[j];
			}
		}
		return list[0];
	}

	private int GetIndexFromTupleArray(CellData current, List<Tuple<int, TilesetIndexMetadata>> list, int roomTypeIndex)
	{
		float uniqueHash = current.UniqueHash;
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Second.dungeonRoomSubType == roomTypeIndex || list[i].Second.secondRoomSubType == roomTypeIndex || list[i].Second.thirdRoomSubType == roomTypeIndex)
			{
				num += list[i].Second.weight;
			}
		}
		float num2 = uniqueHash * num;
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].Second.dungeonRoomSubType == roomTypeIndex || list[j].Second.secondRoomSubType == roomTypeIndex || list[j].Second.thirdRoomSubType == roomTypeIndex)
			{
				num2 -= list[j].Second.weight;
				if (num2 <= 0f)
				{
					return list[j].First;
				}
			}
		}
		return list[0].First;
	}

	private TilesetIndexMetadata GetMetadataFromTupleArray(CellData current, List<Tuple<int, TilesetIndexMetadata>> list, int roomTypeIndex, out int index)
	{
		if (list == null)
		{
			index = -1;
			return null;
		}
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			Tuple<int, TilesetIndexMetadata> tuple = list[i];
			if (tuple.Second.dungeonRoomSubType == -1 || tuple.Second.dungeonRoomSubType == roomTypeIndex || tuple.Second.secondRoomSubType == roomTypeIndex || tuple.Second.thirdRoomSubType == roomTypeIndex)
			{
				num += tuple.Second.weight;
			}
		}
		float num2 = UnityEngine.Random.value * num;
		for (int j = 0; j < list.Count; j++)
		{
			Tuple<int, TilesetIndexMetadata> tuple2 = list[j];
			if (tuple2.Second.dungeonRoomSubType == -1 || tuple2.Second.dungeonRoomSubType == roomTypeIndex || tuple2.Second.secondRoomSubType == roomTypeIndex || tuple2.Second.thirdRoomSubType == roomTypeIndex)
			{
				num2 -= tuple2.Second.weight;
				if (num2 <= 0f)
				{
					index = tuple2.First;
					return tuple2.Second;
				}
			}
		}
		index = list[0].First;
		return list[0].Second;
	}

	public void ClearData(tk2dTileMap map)
	{
		for (int i = 0; i < map.Layers.Length; i++)
		{
			map.DeleteSprites(i, 0, 0, map.width - 1, map.height - 1);
		}
	}

	private void BuildBorderIndicesForCell(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (t.placeBorders && (current.nearestRoom == null || !(current.nearestRoom.area.prototypeRoom != null) || !current.nearestRoom.area.prototypeRoom.preventBorders))
		{
			if (BCheck(d, ix, iy, -2) && (current.type == CellType.WALL || d.data.isTopWall(ix, iy)) && !d.data.isFaceWallHigher(ix, iy) && !d.data.isFaceWallLower(ix, iy))
			{
				BuildBorderIndex(current, d, map, ix, iy);
			}
			if (BCheck(d, ix, iy, -2) && (current.type != CellType.WALL || d.data.isAnyFaceWall(ix, iy)) && !d.data.isTopWall(ix, iy) && d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].outerCeilingBorderGrid != null)
			{
				BuildOuterBorderIndex(current, d, map, ix, iy);
			}
		}
	}

	public void ClearTileIndicesForCell(Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		CellData cellData = ((!d.data.CheckInBoundsAndValid(ix, iy)) ? null : d.data[ix, iy]);
		int x = ((cellData == null) ? ix : cellData.positionInTilemap.x);
		int y = ((cellData == null) ? iy : cellData.positionInTilemap.y);
		for (int i = 0; i < map.Layers.Length; i++)
		{
			map.Layers[i].SetTile(x, y, -1);
		}
		if (!TK2DTilemapChunkAnimator.PositionToAnimatorMap.ContainsKey(cellData.positionInTilemap))
		{
			return;
		}
		for (int j = 0; j < TK2DTilemapChunkAnimator.PositionToAnimatorMap[cellData.positionInTilemap].Count; j++)
		{
			TilemapAnimatorTileManager tilemapAnimatorTileManager = TK2DTilemapChunkAnimator.PositionToAnimatorMap[cellData.positionInTilemap][j];
			if ((bool)tilemapAnimatorTileManager.animator)
			{
				TK2DTilemapChunkAnimator.PositionToAnimatorMap[cellData.positionInTilemap].RemoveAt(j);
				j--;
				UnityEngine.Object.Destroy(tilemapAnimatorTileManager.animator.gameObject);
				tilemapAnimatorTileManager.animator = null;
			}
		}
	}

	public void BuildTileIndicesForCell(Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		CellData cellData = d.data.cellData[ix][iy];
		if (cellData == null)
		{
			return;
		}
		BuildOcclusionPartitionIndex(cellData, d, map, ix, iy);
		cellData.isOccludedByTopWall = d.data.isTopWall(ix, iy);
		if (cellData.cellVisualData.hasAlreadyBeenTilemapped || cellData.cellVisualData.precludeAllTileDrawing)
		{
			return;
		}
		bool flag = BCheck(d, ix, iy, 3) && d.data[ix, iy - 2] != null && d.data[ix, iy - 2].isExitCell;
		if (cellData.nearestRoom != null && cellData.nearestRoom.PrecludeTilemapDrawing && (!cellData.nearestRoom.DrawPrecludedCeilingTiles || (!cellData.isExitCell && !flag)))
		{
			if (cellData.nearestRoom.DrawPrecludedCeilingTiles)
			{
				BuildCollisionIndex(cellData, d, map, ix, iy);
				BuildBorderIndicesForCell(cellData, d, map, ix, iy);
			}
			cellData.cellVisualData.precludeAllTileDrawing = true;
			return;
		}
		if (cellData.parentRoom != null && cellData.parentRoom.PrecludeTilemapDrawing && (!cellData.nearestRoom.DrawPrecludedCeilingTiles || (!cellData.isExitCell && !flag)))
		{
			if (cellData.parentRoom.DrawPrecludedCeilingTiles)
			{
				BuildCollisionIndex(cellData, d, map, ix, iy);
				BuildBorderIndicesForCell(cellData, d, map, ix, iy);
			}
			cellData.cellVisualData.precludeAllTileDrawing = true;
			return;
		}
		DungeonMaterial dungeonMaterial = d.roomMaterialDefinitions[cellData.cellVisualData.roomVisualTypeIndex];
		if (dungeonMaterial.overrideStoneFloorType && cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Stone)
		{
			cellData.cellVisualData.floorType = dungeonMaterial.overrideFloorType;
		}
		bool flag2 = cellData.type == CellType.FLOOR || d.data.isFaceWallLower(ix, iy);
		if (flag2)
		{
			BuildFloorIndex(cellData, d, map, ix, iy);
		}
		BuildDecoIndices(cellData, d, map, ix, iy);
		if (flag2)
		{
			BuildFloorEdgeBorderTiles(cellData, d, map, ix, iy);
		}
		BuildFeatureEdgeBorderTiles(cellData, d, map, ix, iy);
		BuildCollisionIndex(cellData, d, map, ix, iy);
		if (BCheck(d, ix, iy, -2))
		{
			ProcessFacewallIndices(cellData, d, map, ix, iy);
		}
		BuildBorderIndicesForCell(cellData, d, map, ix, iy);
		TileIndexGrid tileIndexGrid = d.roomMaterialDefinitions[cellData.cellVisualData.roomVisualTypeIndex].pitBorderFlatGrid;
		TileIndexGrid additionalPitBorderFlatGrid = dungeonMaterial.additionalPitBorderFlatGrid;
		PrototypeRoomPitEntry.PitBorderType pitBorderType = cellData.GetPitBorderType(d.data);
		switch (pitBorderType)
		{
		case PrototypeRoomPitEntry.PitBorderType.FLAT:
			tileIndexGrid = dungeonMaterial.pitBorderFlatGrid;
			break;
		case PrototypeRoomPitEntry.PitBorderType.RAISED:
			tileIndexGrid = dungeonMaterial.pitBorderRaisedGrid;
			break;
		}
		int num = ((pitBorderType != PrototypeRoomPitEntry.PitBorderType.RAISED) ? GlobalDungeonData.patternLayerIndex : GlobalDungeonData.actorCollisionLayerIndex);
		int num2 = num;
		bool wALLS_ARE_PITS = GameManager.Instance.Dungeon.debugSettings.WALLS_ARE_PITS;
		if (cellData.type == CellType.FLOOR)
		{
			if (d.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.WESTGEON && d.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FINALGEON)
			{
				BuildShadowIndex(cellData, d, map, ix, iy);
			}
			if (tileIndexGrid != null)
			{
				HandlePitBorderTilePlacement(cellData, tileIndexGrid, map.Layers[num], map, d);
			}
			if (additionalPitBorderFlatGrid != null)
			{
				HandlePitBorderTilePlacement(cellData, additionalPitBorderFlatGrid, map.Layers[num2], map, d);
			}
		}
		else if (cellData.type == CellType.PIT && d.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.WESTGEON && d.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FINALGEON)
		{
			BuildPitShadowIndex(cellData, d, map, ix, iy);
		}
		if (cellData.type == CellType.PIT || (wALLS_ARE_PITS && cellData.isExitCell))
		{
			TileIndexGrid pitLayoutGrid = dungeonMaterial.pitLayoutGrid;
			if (pitLayoutGrid == null)
			{
				pitLayoutGrid = d.roomMaterialDefinitions[0].pitLayoutGrid;
			}
			map.data.Layers[GlobalDungeonData.pitLayerIndex].ForceNonAnimating = true;
			HandlePitTilePlacement(cellData, pitLayoutGrid, map.Layers[GlobalDungeonData.pitLayerIndex], d);
			if (tileIndexGrid != null)
			{
				HandlePitBorderTilePlacement(cellData, tileIndexGrid, map.Layers[num], map, d);
			}
			if (additionalPitBorderFlatGrid != null)
			{
				HandlePitBorderTilePlacement(cellData, additionalPitBorderFlatGrid, map.Layers[num2], map, d);
			}
		}
		if (d.data.isTopDiagonalWall(ix, iy))
		{
			if (cellData.diagonalWallType == DiagonalWallType.NORTHEAST)
			{
				AssignSpecificColorsToTile(cellData.positionInTilemap.x, cellData.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 0.5f, 1f), new Color(0f, 1f, 1f), new Color(0f, 1f, 1f), new Color(0f, 1f, 1f), map);
			}
			else if (cellData.diagonalWallType == DiagonalWallType.NORTHWEST)
			{
				AssignSpecificColorsToTile(cellData.positionInTilemap.x, cellData.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 1f, 1f), new Color(0f, 0.5f, 1f), new Color(0f, 1f, 1f), new Color(0f, 1f, 1f), map);
			}
		}
		if (cellData.cellVisualData.pathTilesetGridIndex > -1)
		{
			TileIndexGrid pathGrid = d.pathGridDefinitions[cellData.cellVisualData.pathTilesetGridIndex];
			HandlePathTilePlacement(cellData, d, map, pathGrid);
		}
		if (cellData.cellVisualData.UsesCustomIndexOverride01)
		{
			map.SetTile(cellData.positionInTilemap.x, cellData.positionInTilemap.y, cellData.cellVisualData.CustomIndexOverride01Layer, cellData.cellVisualData.CustomIndexOverride01);
		}
		if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.JUNGLEGEON)
		{
			BuildOcclusionLayerCenterJungle(cellData, d, map, ix, iy);
		}
		if (cellData.distanceFromNearestRoom < 4f && cellData.nearestRoom.area.PrototypeLostWoodsRoom)
		{
			HandleLostWoodsMirroring(cellData, d, map, ix, iy);
		}
		cellData.hasBeenGenerated = true;
	}

	private bool CheckLostWoodsCellValidity(Dungeon d, IntVector2 p1, IntVector2 p2)
	{
		CellData cellData = d.data[p1];
		CellData cellData2 = d.data[p2];
		if (cellData == null || cellData2 == null)
		{
			return false;
		}
		if (cellData2.isExitCell != cellData.isExitCell)
		{
			return false;
		}
		if (cellData2.IsAnyFaceWall() != cellData.IsAnyFaceWall())
		{
			return false;
		}
		if (cellData2.IsTopWall() != cellData.IsTopWall())
		{
			return false;
		}
		if (cellData2.type != cellData.type)
		{
			return false;
		}
		return true;
	}

	private void HandleLostWoodsMirroring(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		RoomHandler nearestRoom = current.nearestRoom;
		IntVector2 intVector = new IntVector2(ix - current.nearestRoom.area.basePosition.x, iy - current.nearestRoom.area.basePosition.y);
		for (int i = 0; i < d.data.rooms.Count; i++)
		{
			RoomHandler roomHandler = d.data.rooms[i];
			if (roomHandler == nearestRoom || !roomHandler.area.PrototypeLostWoodsRoom)
			{
				continue;
			}
			CellData cellData = d.data[intVector + roomHandler.area.basePosition];
			if (cellData != null && current != null && cellData.isExitCell == current.isExitCell && cellData.IsAnyFaceWall() == current.IsAnyFaceWall() && cellData.IsTopWall() == current.IsTopWall() && cellData.type == current.type && CheckLostWoodsCellValidity(d, current.position + new IntVector2(0, 1), cellData.position + new IntVector2(0, 1)) && CheckLostWoodsCellValidity(d, current.position + new IntVector2(0, -1), cellData.position + new IntVector2(0, -1)) && CheckLostWoodsCellValidity(d, current.position + new IntVector2(1, 0), cellData.position + new IntVector2(1, 0)) && CheckLostWoodsCellValidity(d, current.position + new IntVector2(-1, 0), cellData.position + new IntVector2(-1, 0)) && !cellData.cellVisualData.hasAlreadyBeenTilemapped)
			{
				cellData.cellVisualData.hasAlreadyBeenTilemapped = true;
				for (int j = 0; j < map.Layers.Length; j++)
				{
					map.Layers[j].SetTile(cellData.positionInTilemap.x, cellData.positionInTilemap.y, map.Layers[j].GetTile(current.positionInTilemap.x, current.positionInTilemap.y));
				}
			}
		}
	}

	private void HandlePathTilePlacement(CellData current, Dungeon d, tk2dTileMap map, TileIndexGrid pathGrid)
	{
		List<CellData> cellNeighbors = d.data.GetCellNeighbors(current, true);
		bool[] array = new bool[8];
		for (int i = 0; i < array.Length; i++)
		{
			if (current.cellVisualData.pathTilesetGridIndex == cellNeighbors[i].cellVisualData.pathTilesetGridIndex)
			{
				array[i] = true;
			}
		}
		int num = pathGrid.GetIndexGivenSides(!array[0], !array[2], !array[4], !array[6]);
		int num2 = GlobalDungeonData.patternLayerIndex;
		if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON && current.type != CellType.PIT)
		{
			if (array[0] == array[4] && array[0] != array[2] && array[0] != array[6])
			{
				num += (array[0] ? 1 : 2);
			}
		}
		else if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON)
		{
			num2 = GlobalDungeonData.killLayerIndex;
			if (cellNeighbors[4] != null && !array[4] && cellNeighbors[4].type == CellType.PIT)
			{
				int tile = pathGrid.PathPitPosts.indices[cellNeighbors[4].cellVisualData.roomVisualTypeIndex];
				if (array[0] && array[2])
				{
					tile = pathGrid.PathPitPostsBL.indices[cellNeighbors[4].cellVisualData.roomVisualTypeIndex];
				}
				else if (array[0] && array[6])
				{
					tile = pathGrid.PathPitPostsBR.indices[cellNeighbors[4].cellVisualData.roomVisualTypeIndex];
				}
				map.Layers[GlobalDungeonData.killLayerIndex].SetTile(cellNeighbors[4].positionInTilemap.x, cellNeighbors[4].positionInTilemap.y, tile);
			}
		}
		map.Layers[num2].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, num);
	}

	private void BuildFeatureEdgeBorderTiles(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (d.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.WESTGEON)
		{
			return;
		}
		TileIndexGrid exteriorFacadeBorderGrid = d.roomMaterialDefinitions[1].exteriorFacadeBorderGrid;
		List<CellData> cellNeighbors = d.data.GetCellNeighbors(current, true);
		bool[] array = new bool[8];
		for (int i = 0; i < array.Length; i++)
		{
			if (cellNeighbors[i] != null)
			{
				array[i] = cellNeighbors[i].cellVisualData.IsFeatureCell || cellNeighbors[i].cellVisualData.IsFeatureAdditional;
			}
		}
		int indexGivenEightSides = exteriorFacadeBorderGrid.GetIndexGivenEightSides(array);
		if (indexGivenEightSides != -1)
		{
			map.Layers[GlobalDungeonData.decalLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexGivenEightSides);
		}
	}

	private void BuildFloorEdgeBorderTiles(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (current.type != CellType.FLOOR && !d.data.isFaceWallLower(ix, iy))
		{
			return;
		}
		TileIndexGrid tileIndexGrid = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].roomFloorBorderGrid;
		if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON && current.cellVisualData.IsFacewallForInteriorTransition)
		{
			tileIndexGrid = d.roomMaterialDefinitions[current.cellVisualData.InteriorTransitionIndex].exteriorFacadeBorderGrid;
		}
		if (!(tileIndexGrid != null))
		{
			return;
		}
		if (current.diagonalWallType == DiagonalWallType.NONE || !d.data.isFaceWallLower(ix, iy))
		{
			List<CellData> cellNeighbors = d.data.GetCellNeighbors(current, true);
			bool[] array = new bool[8];
			for (int i = 0; i < array.Length; i++)
			{
				if (cellNeighbors[i] != null)
				{
					array[i] = cellNeighbors[i].type == CellType.WALL && !d.data.isTopWall(cellNeighbors[i].position.x, cellNeighbors[i].position.y + 1) && cellNeighbors[i].diagonalWallType == DiagonalWallType.NONE;
					bool flag = cellNeighbors[i].isSecretRoomCell || (d.data[cellNeighbors[i].position + IntVector2.Up].IsTopWall() && d.data[cellNeighbors[i].position + IntVector2.Up].isSecretRoomCell);
					array[i] = array[i] || flag != current.isSecretRoomCell;
				}
			}
			int indexGivenEightSides = tileIndexGrid.GetIndexGivenEightSides(array);
			if (indexGivenEightSides != -1)
			{
				map.Layers[GlobalDungeonData.decalLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexGivenEightSides);
			}
		}
		else
		{
			int indexByWeight = tileIndexGrid.quadNubs.GetIndexByWeight();
			if (indexByWeight != -1)
			{
				map.Layers[GlobalDungeonData.decalLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexByWeight);
			}
		}
	}

	private void AssignSpecificColorsToTile(int ix, int iy, int layer, Color32 bottomLeft, Color32 bottomRight, Color32 topLeft, Color32 topRight, tk2dTileMap map)
	{
		if (!map.HasColorChannel())
		{
			map.CreateColorChannel();
		}
		ColorChannel colorChannel = map.ColorChannel;
		map.data.Layers[layer].useColor = true;
		colorChannel.SetTileColorGradient(ix, iy, bottomLeft, bottomRight, topLeft, topRight);
	}

	private void AssignColorGradientToTile(int ix, int iy, int layer, Color32 bottom, Color32 top, tk2dTileMap map)
	{
		if (!map.HasColorChannel())
		{
			map.CreateColorChannel();
		}
		ColorChannel colorChannel = map.ColorChannel;
		map.data.Layers[layer].useColor = true;
		colorChannel.SetTileColorGradient(ix, iy, bottom, bottom, top, top);
	}

	private void AssignColorOverrideToTile(int ix, int iy, int layer, Color32 color, tk2dTileMap map)
	{
		if (!map.HasColorChannel())
		{
			map.CreateColorChannel();
		}
		ColorChannel colorChannel = map.ColorChannel;
		map.data.Layers[layer].useColor = true;
		colorChannel.SetTileColorOverride(ix, iy, color);
	}

	private void ClearAllIndices(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		for (int i = 0; i < map.Layers.Length; i++)
		{
			map.Layers[i].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, -1);
		}
	}

	private bool CheckHasValidFloorGridForRoomSubType(List<TileIndexGrid> indexGrids, int roomType)
	{
		for (int i = 0; i < indexGrids.Count; i++)
		{
			if (indexGrids[i].roomTypeRestriction == -1 || indexGrids[i].roomTypeRestriction == roomType)
			{
				return true;
			}
		}
		return false;
	}

	private RoomInternalMaterialTransition GetMaterialTransitionFromSubtypes(Dungeon d, int roomType, int cellType)
	{
		if (!d.roomMaterialDefinitions[roomType].usesInternalMaterialTransitions)
		{
			return null;
		}
		if (roomType == cellType)
		{
			return null;
		}
		for (int i = 0; i < d.roomMaterialDefinitions[roomType].internalMaterialTransitions.Length; i++)
		{
			if (d.roomMaterialDefinitions[roomType].internalMaterialTransitions[i].materialTransition == cellType)
			{
				return d.roomMaterialDefinitions[roomType].internalMaterialTransitions[i];
			}
		}
		return null;
	}

	private void BuildFloorIndex(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (current.cellVisualData.inheritedOverrideIndex != -1)
		{
			map.Layers[(!current.cellVisualData.inheritedOverrideIndexIsFloor) ? GlobalDungeonData.patternLayerIndex : GlobalDungeonData.floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, current.cellVisualData.inheritedOverrideIndex);
		}
		if (current.cellVisualData.inheritedOverrideIndex == -1 || !current.cellVisualData.inheritedOverrideIndexIsFloor)
		{
			bool flag = true;
			TileIndexGrid randomGridFromArray = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].GetRandomGridFromArray(d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].floorSquares);
			if (randomGridFromArray == null)
			{
				flag = false;
			}
			if (flag)
			{
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						if (!BCheck(d, ix + i, iy + j))
						{
							flag = false;
						}
						if (d.data.isWall(ix + i, iy + j) || d.data.isAnyFaceWall(ix + i, iy + j))
						{
							flag = false;
						}
						CellData cellData = d.data.cellData[ix + i][iy + j];
						if (cellData.HasWallNeighbor(true, false) || cellData.HasPitNeighbor(d.data))
						{
							flag = false;
						}
						if (cellData.cellVisualData.roomVisualTypeIndex != current.cellVisualData.roomVisualTypeIndex)
						{
							flag = false;
						}
						if (cellData.cellVisualData.inheritedOverrideIndex != -1)
						{
							flag = false;
						}
						if (cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Ice)
						{
							flag = false;
						}
						if (cellData.doesDamage)
						{
							flag = false;
						}
						if (!flag)
						{
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
			}
			if (flag && current.UniqueHash < d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].floorSquareDensity)
			{
				TileIndexGrid tileIndexGrid = randomGridFromArray;
				int num = ((!(current.UniqueHash < 0.025f)) ? 3 : 2);
				if (tileIndexGrid.topIndices.indices[0] == -1)
				{
					num = 2;
				}
				for (int k = 0; k < num; k++)
				{
					for (int l = 0; l < num; l++)
					{
						bool isNorthBorder = l == num - 1;
						bool isSouthBorder = l == 0;
						bool isEastBorder = k == num - 1;
						bool isWestBorder = k == 0;
						int indexGivenSides = tileIndexGrid.GetIndexGivenSides(isNorthBorder, isEastBorder, isSouthBorder, isWestBorder);
						if (BCheck(d, ix + k, iy + l) && !d.data.isFaceWallLower(ix + k, iy + l) && d.data.cellData[ix + k][iy + l].type != CellType.PIT)
						{
							CellData cellData2 = d.data.cellData[ix + k][iy + l];
							cellData2.cellVisualData.inheritedOverrideIndex = indexGivenSides;
							cellData2.cellVisualData.inheritedOverrideIndexIsFloor = true;
							map.Layers[GlobalDungeonData.floorLayerIndex].SetTile(cellData2.positionInTilemap.x, cellData2.positionInTilemap.y, indexGivenSides);
						}
					}
				}
			}
			else if (current.cellVisualData.floorType == CellVisualData.CellFloorType.Ice && d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].supportsIceSquares)
			{
				TileIndexGrid randomGridFromArray2 = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].GetRandomGridFromArray(d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].iceGrids);
				List<CellData> cellNeighbors = d.data.GetCellNeighbors(current, true);
				bool isNorthBorder2 = cellNeighbors[0].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				bool isNortheastBorder = cellNeighbors[1].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				bool isEastBorder2 = cellNeighbors[2].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				bool isSoutheastBorder = cellNeighbors[3].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				bool isSouthBorder2 = cellNeighbors[4].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				bool isSouthwestBorder = cellNeighbors[5].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				bool isWestBorder2 = cellNeighbors[6].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				bool isNorthwestBorder = cellNeighbors[7].cellVisualData.floorType != CellVisualData.CellFloorType.Ice;
				int indexGivenSides2 = randomGridFromArray2.GetIndexGivenSides(isNorthBorder2, isNortheastBorder, isEastBorder2, isSoutheastBorder, isSouthBorder2, isSouthwestBorder, isWestBorder2, isNorthwestBorder);
				map.Layers[GlobalDungeonData.floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexGivenSides2);
				map.Layers[GlobalDungeonData.patternLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexGivenSides2);
			}
			else if (current.doesDamage && d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].supportsLavaOrLavalikeSquares)
			{
				TileIndexGrid randomGridFromArray3 = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].GetRandomGridFromArray(d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].lavaGrids);
				List<CellData> cellNeighbors2 = d.data.GetCellNeighbors(current, true);
				bool isNorthBorder3 = !cellNeighbors2[0].doesDamage;
				bool isNortheastBorder2 = !cellNeighbors2[1].doesDamage;
				bool isEastBorder3 = !cellNeighbors2[2].doesDamage;
				bool isSoutheastBorder2 = !cellNeighbors2[3].doesDamage;
				bool isSouthBorder3 = !cellNeighbors2[4].doesDamage;
				bool isSouthwestBorder2 = !cellNeighbors2[5].doesDamage;
				bool isWestBorder3 = !cellNeighbors2[6].doesDamage;
				bool isNorthwestBorder2 = !cellNeighbors2[7].doesDamage;
				int indexGivenSides3 = randomGridFromArray3.GetIndexGivenSides(isNorthBorder3, isNortheastBorder2, isEastBorder3, isSoutheastBorder2, isSouthBorder3, isSouthwestBorder2, isWestBorder3, isNorthwestBorder2);
				map.Layers[GlobalDungeonData.floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexGivenSides3);
				map.Layers[GlobalDungeonData.patternLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexGivenSides3);
			}
			else
			{
				RoomInternalMaterialTransition roomInternalMaterialTransition = ((current != null && current.parentRoom != null) ? GetMaterialTransitionFromSubtypes(d, current.parentRoom.RoomVisualSubtype, current.cellVisualData.roomVisualTypeIndex) : null);
				if (roomInternalMaterialTransition != null)
				{
					List<CellData> cellNeighbors3 = d.data.GetCellNeighbors(current, true);
					bool flag2 = cellNeighbors3[0].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag3 = cellNeighbors3[1].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag4 = cellNeighbors3[2].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag5 = cellNeighbors3[3].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag6 = cellNeighbors3[4].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag7 = cellNeighbors3[5].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag8 = cellNeighbors3[6].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag9 = cellNeighbors3[7].cellVisualData.roomVisualTypeIndex == current.parentRoom.RoomVisualSubtype;
					bool flag10 = flag2 || flag3 || flag4 || flag5 || flag6 || flag7 || flag8 || flag9;
					int tile = GetIndexFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.FLOOR_TILE], current.cellVisualData.roomVisualTypeIndex);
					if (flag10)
					{
						tile = roomInternalMaterialTransition.transitionGrid.GetIndexGivenSides(flag2, flag3, flag4, flag5, flag6, flag7, flag8, flag9);
					}
					map.Layers[GlobalDungeonData.floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, tile);
				}
				else
				{
					int indexFromTupleArray = GetIndexFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.FLOOR_TILE], current.cellVisualData.roomVisualTypeIndex);
					map.Layers[GlobalDungeonData.floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, indexFromTupleArray);
				}
			}
		}
		if (d.data.HasDoorAtPosition(new IntVector2(ix, iy)) || d.data[ix, iy].cellVisualData.doorFeetOverrideMode != 0)
		{
			DungeonDoorController dungeonDoorController = null;
			IntVector2 key = new IntVector2(ix, iy);
			if (d.data.doors.ContainsKey(key))
			{
				dungeonDoorController = d.data.doors[key];
			}
			if (d.data[ix, iy].cellVisualData.doorFeetOverrideMode == 1 || (dungeonDoorController != null && dungeonDoorController.northSouth))
			{
				int index = -1;
				GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DOOR_FEET_NS], -1, out index);
				map.Layers[GlobalDungeonData.decalLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index);
				map.Layers[GlobalDungeonData.patternLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index);
			}
			else if (d.data[ix, iy].cellVisualData.doorFeetOverrideMode == 2 || (dungeonDoorController != null && !dungeonDoorController.northSouth))
			{
				int index2 = -1;
				GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DOOR_FEET_EW], -1, out index2);
				map.Layers[GlobalDungeonData.patternLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index2);
				map.Layers[GlobalDungeonData.decalLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index2);
			}
		}
	}

	private void BuildDecoIndices(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if ((current.type != CellType.FLOOR && !current.IsLowerFaceWall()) || d.data.isTopWall(ix, iy) || current.cellVisualData.floorTileOverridden || current.cellVisualData.inheritedOverrideIndex != -1)
		{
			return;
		}
		DungeonMaterial dungeonMaterial = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex];
		if (current.HasPitNeighbor(d.data))
		{
			return;
		}
		if (current.cellVisualData.isPattern)
		{
			List<CellData> cellNeighbors = d.data.GetCellNeighbors(current, true);
			bool[] array = new bool[8];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = !cellNeighbors[i].cellVisualData.isPattern && cellNeighbors[i].type != CellType.WALL;
			}
			TileIndexGrid tileIndexGrid = ((!dungeonMaterial.usesPatternLayer) ? t.patternIndexGrid : dungeonMaterial.patternIndexGrid);
			current.cellVisualData.preventFloorStamping = true;
			if (tileIndexGrid != null)
			{
				map.Layers[GlobalDungeonData.patternLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, tileIndexGrid.GetIndexGivenSides(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7]));
			}
		}
		if (current.cellVisualData.isDecal)
		{
			List<CellData> cellNeighbors2 = d.data.GetCellNeighbors(current, true);
			bool[] array2 = new bool[8];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = !cellNeighbors2[j].cellVisualData.isDecal && cellNeighbors2[j].type != CellType.WALL;
			}
			TileIndexGrid tileIndexGrid2 = ((!dungeonMaterial.usesDecalLayer) ? t.decalIndexGrid : dungeonMaterial.decalIndexGrid);
			current.cellVisualData.preventFloorStamping = true;
			if (tileIndexGrid2 != null)
			{
				map.Layers[GlobalDungeonData.decalLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, tileIndexGrid2.GetIndexGivenSides(array2[0], array2[1], array2[2], array2[3], array2[4], array2[5], array2[6], array2[7]));
			}
		}
	}

	private bool IsValidJungleBorderCell(CellData current, Dungeon d, int ix, int iy)
	{
		return !current.cellVisualData.ceilingHasBeenProcessed && !IsCardinalBorder(current, d, ix, iy) && current.type == CellType.WALL && (iy < 2 || !d.data.isFaceWallLower(ix, iy)) && !d.data.isTopDiagonalWall(ix, iy);
	}

	private bool IsValidJungleOcclusionCell(CellData current, Dungeon d, int ix, int iy)
	{
		if (!BCheck(d, ix, iy, 1))
		{
			return false;
		}
		return !current.cellVisualData.ceilingHasBeenProcessed && !current.cellVisualData.occlusionHasBeenProcessed && (current.type != CellType.WALL || IsCardinalBorder(current, d, ix, iy) || (iy > 2 && (d.data.isFaceWallLower(ix, iy) || d.data.isFaceWallHigher(ix, iy))));
	}

	private void BuildOcclusionLayerCenterJungle(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (!IsValidJungleOcclusionCell(current, d, ix, iy))
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		if (!BCheck(d, ix, iy))
		{
			flag = false;
			flag2 = false;
		}
		if (current.UniqueHash < 0.05f)
		{
			flag = false;
			flag2 = false;
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (!IsValidJungleOcclusionCell(d.data[ix + i, iy + j], d, ix + i, iy + j))
				{
					flag2 = false;
					if (i < 2 || j < 2)
					{
						flag = false;
					}
				}
				if (!flag2 && !flag)
				{
					break;
				}
			}
			if (!flag2 && !flag)
			{
				break;
			}
		}
		if (flag2 && current.UniqueHash < 0.75f)
		{
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 352);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y, 353);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y, 354);
			d.data[ix + 1, iy].cellVisualData.occlusionHasBeenProcessed = true;
			d.data[ix + 2, iy].cellVisualData.occlusionHasBeenProcessed = true;
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, 330);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 1, 331);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y + 1, 332);
			d.data[ix, iy + 1].cellVisualData.occlusionHasBeenProcessed = true;
			d.data[ix + 1, iy + 1].cellVisualData.occlusionHasBeenProcessed = true;
			d.data[ix + 2, iy + 1].cellVisualData.occlusionHasBeenProcessed = true;
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 2, 308);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 2, 309);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y + 2, 310);
			d.data[ix, iy + 2].cellVisualData.occlusionHasBeenProcessed = true;
			d.data[ix + 1, iy + 2].cellVisualData.occlusionHasBeenProcessed = true;
			d.data[ix + 2, iy + 2].cellVisualData.occlusionHasBeenProcessed = true;
		}
		else if (flag)
		{
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 418);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y, 419);
			d.data[ix + 1, iy].cellVisualData.occlusionHasBeenProcessed = true;
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, 396);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 1, 397);
			d.data[ix, iy + 1].cellVisualData.occlusionHasBeenProcessed = true;
			d.data[ix + 1, iy + 1].cellVisualData.occlusionHasBeenProcessed = true;
		}
		else
		{
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 374);
		}
		d.data[ix, iy].cellVisualData.occlusionHasBeenProcessed = true;
	}

	private void BuildBorderLayerCenterJungle(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (!IsValidJungleBorderCell(current, d, ix, iy))
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		if (!BCheck(d, ix, iy))
		{
			flag = false;
			flag2 = false;
		}
		if (current.UniqueHash < 0.05f)
		{
			flag = false;
			flag2 = false;
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (!IsValidJungleBorderCell(d.data[ix + i, iy + j], d, ix + i, iy + j))
				{
					flag2 = false;
					if (i < 2 || j < 2)
					{
						flag = false;
					}
				}
				if (!flag2 && !flag)
				{
					break;
				}
			}
			if (!flag2 && !flag)
			{
				break;
			}
		}
		if (flag2 && current.UniqueHash < 0.75f)
		{
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 352);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 352);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y, 353);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y, 353);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y, 354);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y, 354);
			d.data[ix + 1, iy].cellVisualData.ceilingHasBeenProcessed = true;
			d.data[ix + 2, iy].cellVisualData.ceilingHasBeenProcessed = true;
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, 330);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, 330);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 1, 331);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 1, 331);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y + 1, 332);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y + 1, 332);
			d.data[ix, iy + 1].cellVisualData.ceilingHasBeenProcessed = true;
			d.data[ix + 1, iy + 1].cellVisualData.ceilingHasBeenProcessed = true;
			d.data[ix + 2, iy + 1].cellVisualData.ceilingHasBeenProcessed = true;
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 2, 308);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 2, 308);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 2, 309);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 2, 309);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y + 2, 310);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 2, current.positionInTilemap.y + 2, 310);
			d.data[ix, iy + 2].cellVisualData.ceilingHasBeenProcessed = true;
			d.data[ix + 1, iy + 2].cellVisualData.ceilingHasBeenProcessed = true;
			d.data[ix + 2, iy + 2].cellVisualData.ceilingHasBeenProcessed = true;
		}
		else if (flag)
		{
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 418);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 418);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y, 419);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y, 419);
			d.data[ix + 1, iy].cellVisualData.ceilingHasBeenProcessed = true;
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, 396);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, 396);
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 1, 397);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x + 1, current.positionInTilemap.y + 1, 397);
			d.data[ix, iy + 1].cellVisualData.ceilingHasBeenProcessed = true;
			d.data[ix + 1, iy + 1].cellVisualData.ceilingHasBeenProcessed = true;
		}
		else
		{
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 374);
			map.Layers[GlobalDungeonData.occlusionPartitionIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, 374);
		}
		d.data[ix, iy].cellVisualData.ceilingHasBeenProcessed = true;
	}

	private void BuildCollisionIndex(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (current.type == CellType.WALL && (iy < 2 || !d.data.isFaceWallLower(ix, iy)) && !d.data.isTopDiagonalWall(ix, iy))
		{
			TileIndexGrid roomCeilingBorderGrid = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].roomCeilingBorderGrid;
			if ((d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON || d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON) && current.nearestRoom != null)
			{
				roomCeilingBorderGrid = d.roomMaterialDefinitions[current.nearestRoom.RoomVisualSubtype].roomCeilingBorderGrid;
			}
			if (roomCeilingBorderGrid == null)
			{
				roomCeilingBorderGrid = d.roomMaterialDefinitions[0].roomCeilingBorderGrid;
			}
			map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, roomCeilingBorderGrid.centerIndices.indices[0]);
		}
	}

	private void BuildPitShadowIndex(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (!d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].doPitAO || (current != null && current.cellVisualData.hasStampedPath))
		{
			return;
		}
		int floorLayerIndex = GlobalDungeonData.floorLayerIndex;
		if (!BCheck(d, ix, iy, -2))
		{
			return;
		}
		CellData cellData = d.data.cellData[ix - 1][iy];
		CellData cellData2 = d.data.cellData[ix + 1][iy];
		CellData cellData3 = d.data.cellData[ix][iy + 1];
		CellData cellData4 = d.data.cellData[ix][iy + 2];
		CellData cellData5 = d.data.cellData[ix + 1][iy + 2];
		CellData cellData6 = d.data.cellData[ix + 1][iy + 1];
		CellData cellData7 = d.data.cellData[ix - 1][iy + 2];
		CellData cellData8 = d.data.cellData[ix - 1][iy + 1];
		bool flag = false;
		DungeonMaterial dungeonMaterial = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex];
		bool flag2;
		bool flag3;
		bool flag4;
		bool flag5;
		if (dungeonMaterial.pitsAreOneDeep)
		{
			flag2 = cellData.type != CellType.PIT;
			flag3 = cellData2.type != CellType.PIT;
			flag4 = cellData3.type != CellType.PIT;
			flag5 = cellData6.type != CellType.PIT;
			flag = cellData8.type != CellType.PIT;
		}
		else
		{
			flag2 = cellData3.type == CellType.PIT && cellData8.type != CellType.PIT;
			flag3 = cellData3.type == CellType.PIT && cellData6.type != CellType.PIT;
			flag4 = cellData4.type != CellType.PIT && cellData3.type == CellType.PIT;
			flag5 = cellData5.type != CellType.PIT && cellData6.type == CellType.PIT;
			flag = cellData7.type != CellType.PIT && cellData8.type == CellType.PIT;
		}
		if (dungeonMaterial.pitfallVFXPrefab != null && dungeonMaterial.pitfallVFXPrefab.name.ToLowerInvariant().Contains("splash"))
		{
			if (flag4 && flag2 && !flag3)
			{
				map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallUpAndLeft);
			}
			else if (flag4 && flag3 && !flag2)
			{
				map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallUpAndRight);
			}
			else if (flag4 && flag2 && flag3)
			{
				map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallUpAndBoth);
			}
			else if (flag4 && !flag2 && !flag3)
			{
				map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorTileIndex);
			}
		}
		else if (flag4 && flag2 && !flag3)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOBottomWallTileLeftIndex);
		}
		else if (flag4 && flag3 && !flag2)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOBottomWallTileRightIndex);
		}
		else if (flag4 && flag2 && flag3)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOBottomWallTileBothIndex);
		}
		else if (flag4 && !flag2 && !flag3)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOBottomWallBaseTileIndex);
		}
		if (!flag4)
		{
			bool flag6 = flag2 && !d.data.isTopWall(current.positionInTilemap.x - 1, current.positionInTilemap.y + 1);
			bool flag7 = flag3 && !d.data.isTopWall(current.positionInTilemap.x + 1, current.positionInTilemap.y + 1);
			if (flag6 && flag7)
			{
				map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallBoth);
			}
			else if (flag6)
			{
				map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallLeft);
			}
			else if (flag7)
			{
				map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallRight);
			}
		}
		if (!flag4 && flag && !flag2 && !flag3 && !flag5)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceLeft);
		}
		else if (!flag4 && !flag && !flag2 && !flag3 && flag5)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceRight);
		}
		else if (!flag4 && flag && !flag3 && !flag2 && flag5)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceBoth);
		}
		else if (!flag4 && flag && !flag2 && flag3)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceLeftWallRight);
		}
		else if (!flag4 && flag2 && !flag3 && flag5)
		{
			map.Layers[floorLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceRightWallLeft);
		}
	}

	private void BuildShadowIndex(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (!BCheck(d, ix, iy, -2))
		{
			return;
		}
		CellData cellData = d.data.cellData[ix - 1][iy];
		CellData cellData2 = d.data.cellData[ix + 1][iy];
		CellData cellData3 = d.data.cellData[ix][iy + 1];
		CellData cellData4 = d.data.cellData[ix + 1][iy + 1];
		CellData cellData5 = d.data.cellData[ix - 1][iy + 1];
		bool flag = cellData.type == CellType.WALL && cellData.diagonalWallType == DiagonalWallType.NONE;
		bool flag2 = cellData2.type == CellType.WALL && cellData2.diagonalWallType == DiagonalWallType.NONE;
		bool flag3 = cellData3.type == CellType.WALL;
		bool flag4 = cellData4.type == CellType.WALL && cellData4.diagonalWallType == DiagonalWallType.NONE;
		bool flag5 = cellData5.type == CellType.WALL && cellData5.diagonalWallType == DiagonalWallType.NONE;
		if (current.parentRoom != null && current.parentRoom.area.prototypeRoom != null && current.parentRoom.area.prototypeRoom.preventFacewallAO)
		{
			flag3 = false;
			flag4 = false;
			flag5 = false;
		}
		bool flag6 = cellData3.isSecretRoomCell != current.isSecretRoomCell;
		if (cellData3.diagonalWallType == DiagonalWallType.NONE)
		{
			if (flag3 && flag && !flag2)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallUpAndLeft);
			}
			else if (flag3 && flag2 && !flag)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallUpAndRight);
			}
			else if (flag3 && flag && flag2)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallUpAndBoth);
			}
			else if (flag3 && !flag && !flag2)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorTileIndex);
			}
		}
		else if (cellData3.diagonalWallType == DiagonalWallType.NORTHEAST && cellData3.type == CellType.WALL)
		{
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, t.aoTileIndices.AOFloorDiagonalWallNortheast);
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, (!flag2) ? t.aoTileIndices.AOFloorDiagonalWallNortheastLower : t.aoTileIndices.AOFloorDiagonalWallNortheastLowerJoint);
		}
		else if (cellData3.diagonalWallType == DiagonalWallType.NORTHWEST && cellData3.type == CellType.WALL)
		{
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, t.aoTileIndices.AOFloorDiagonalWallNorthwest);
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, (!flag) ? t.aoTileIndices.AOFloorDiagonalWallNorthwestLower : t.aoTileIndices.AOFloorDiagonalWallNorthwestLowerJoint);
		}
		if (!flag3)
		{
			bool flag7 = flag && !d.data.isTopWall(ix - 1, iy + 1);
			bool flag8 = flag2 && !d.data.isTopWall(ix + 1, iy + 1);
			if (flag7 && flag8)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallBoth);
			}
			else if (flag7)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallLeft);
			}
			else if (flag8)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorWallRight);
			}
		}
		if (!flag3 && flag5 && !flag6 && !flag && !flag2 && !flag4)
		{
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceLeft);
		}
		else if (!flag3 && !flag5 && !flag && !flag2 && flag4 && !flag6)
		{
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceRight);
		}
		else if (!flag3 && flag5 && !flag6 && !flag2 && !flag && flag4 && !flag6)
		{
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceBoth);
		}
		else if (!flag3 && flag5 && !flag6 && !flag && flag2)
		{
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceLeftWallRight);
		}
		else if (!flag3 && flag && !flag2 && flag4 && !flag6)
		{
			map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOFloorPizzaSliceRightWallLeft);
		}
	}

	public void ApplyTileStamp(int ix, int iy, TileStampData tsd, Dungeon d, tk2dTileMap map, int overrideTileLayerIndex = -1)
	{
		DungeonTileStampData.StampSpace occupySpace = tsd.occupySpace;
		for (int i = 0; i < tsd.width; i++)
		{
			for (int j = 0; j < tsd.height; j++)
			{
				CellVisualData cellVisualData = d.data.cellData[ix + i][iy + j].cellVisualData;
				switch (occupySpace)
				{
				case DungeonTileStampData.StampSpace.BOTH_SPACES:
					if (cellVisualData.containsObjectSpaceStamp || cellVisualData.containsWallSpaceStamp || cellVisualData.containsLight)
					{
						return;
					}
					break;
				case DungeonTileStampData.StampSpace.OBJECT_SPACE:
					if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
					{
						if (cellVisualData.containsObjectSpaceStamp || cellVisualData.containsLight)
						{
							return;
						}
					}
					else if (cellVisualData.containsObjectSpaceStamp)
					{
						return;
					}
					break;
				case DungeonTileStampData.StampSpace.WALL_SPACE:
					if (cellVisualData.containsWallSpaceStamp || cellVisualData.containsLight)
					{
						return;
					}
					break;
				}
			}
		}
		for (int k = 0; k < tsd.width; k++)
		{
			for (int l = 0; l < tsd.height; l++)
			{
				CellData cellData = d.data.cellData[ix + k][iy + l];
				CellVisualData cellVisualData2 = cellData.cellVisualData;
				int num = ((occupySpace != 0) ? GlobalDungeonData.wallStampLayerIndex : GlobalDungeonData.objectStampLayerIndex);
				if (d.data.isFaceWallHigher(ix + k, iy + l - 1))
				{
					num = GlobalDungeonData.aboveBorderLayerIndex;
				}
				if (!d.data.isAnyFaceWall(ix + k, iy + l) && d.data.cellData[ix + k][iy + l].type == CellType.WALL)
				{
					num = GlobalDungeonData.aboveBorderLayerIndex;
				}
				if (overrideTileLayerIndex != -1)
				{
					num = overrideTileLayerIndex;
				}
				map.Layers[num].SetTile(cellData.positionInTilemap.x, cellData.positionInTilemap.y, tsd.stampTileIndices[(tsd.height - 1 - l) * tsd.width + k]);
				if (occupySpace == DungeonTileStampData.StampSpace.OBJECT_SPACE)
				{
					cellVisualData2.containsObjectSpaceStamp = true;
				}
				if (occupySpace == DungeonTileStampData.StampSpace.WALL_SPACE)
				{
					cellVisualData2.containsWallSpaceStamp = true;
				}
				if (occupySpace == DungeonTileStampData.StampSpace.BOTH_SPACES)
				{
					cellVisualData2.containsObjectSpaceStamp = true;
					cellVisualData2.containsWallSpaceStamp = true;
				}
			}
		}
	}

	public void ApplyStampGeneric(int ix, int iy, StampDataBase sd, Dungeon d, tk2dTileMap map, bool flipX = false, int overrideTileLayerIndex = -1)
	{
		if (sd is TileStampData)
		{
			ApplyTileStamp(ix, iy, sd as TileStampData, d, map, overrideTileLayerIndex);
		}
		else if (sd is SpriteStampData)
		{
			ApplySpriteStamp(ix, iy, sd as SpriteStampData, d, map);
		}
		else if (sd is ObjectStampData)
		{
			ApplyObjectStamp(ix, iy, sd as ObjectStampData, d, map, flipX);
		}
	}

	public static GameObject ApplyObjectStamp(int ix, int iy, ObjectStampData osd, Dungeon d, tk2dTileMap map, bool flipX = false, bool isLightStamp = false)
	{
		DungeonTileStampData.StampSpace occupySpace = osd.occupySpace;
		for (int i = 0; i < osd.width; i++)
		{
			for (int j = 0; j < osd.height; j++)
			{
				CellData cellData = d.data.cellData[ix + i][iy + j];
				CellVisualData cellVisualData = cellData.cellVisualData;
				if (cellVisualData.forcedMatchingStyle != 0 && cellVisualData.forcedMatchingStyle != osd.intermediaryMatchingStyle)
				{
					return null;
				}
				if (osd.placementRule == DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS && isLightStamp)
				{
					continue;
				}
				bool flag = cellVisualData.containsWallSpaceStamp;
				if (cellVisualData.facewallGridPreventsWallSpaceStamp && isLightStamp)
				{
					flag = false;
				}
				switch (occupySpace)
				{
				case DungeonTileStampData.StampSpace.BOTH_SPACES:
					if (cellVisualData.containsObjectSpaceStamp || flag || (!isLightStamp && cellVisualData.containsLight))
					{
						return null;
					}
					if (cellData.type == CellType.PIT)
					{
						return null;
					}
					break;
				case DungeonTileStampData.StampSpace.OBJECT_SPACE:
					if (cellVisualData.containsObjectSpaceStamp)
					{
						return null;
					}
					if (cellData.type == CellType.PIT)
					{
						return null;
					}
					break;
				case DungeonTileStampData.StampSpace.WALL_SPACE:
					if (flag || (!isLightStamp && cellVisualData.containsLight))
					{
						return null;
					}
					break;
				}
			}
		}
		int num = ((occupySpace != 0) ? GlobalDungeonData.wallStampLayerIndex : GlobalDungeonData.objectStampLayerIndex);
		float z = map.data.Layers[num].z;
		Vector3 vector = osd.objectReference.transform.position;
		ObjectStampOptions component = osd.objectReference.GetComponent<ObjectStampOptions>();
		if (component != null)
		{
			vector = component.GetPositionOffset();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(osd.objectReference);
		gameObject.transform.position = new Vector3(ix, iy, z) + vector;
		if (!isLightStamp && osd.placementRule == DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS)
		{
			gameObject.transform.position = new Vector3(ix + 1, iy, z) + vector.WithX(0f - vector.x);
		}
		tk2dSprite component2 = gameObject.GetComponent<tk2dSprite>();
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(new IntVector2(ix, iy));
		MinorBreakable componentInChildren = gameObject.GetComponentInChildren<MinorBreakable>();
		if ((bool)componentInChildren)
		{
			if (osd.placementRule == DungeonTileStampData.StampPlacementRule.ON_ANY_FLOOR)
			{
				componentInChildren.IgnoredForPotShotsModifier = true;
			}
			componentInChildren.IsDecorativeOnly = true;
		}
		IPlaceConfigurable @interface = gameObject.GetInterface<IPlaceConfigurable>();
		if (@interface != null)
		{
			@interface.ConfigureOnPlacement(absoluteRoomFromPosition);
		}
		SurfaceDecorator component3 = gameObject.GetComponent<SurfaceDecorator>();
		if (component3 != null)
		{
			component3.Decorate(absoluteRoomFromPosition);
		}
		if (flipX)
		{
			if (component2 != null)
			{
				component2.FlipX = true;
				float x = Mathf.Ceil(component2.GetBounds().size.x);
				gameObject.transform.position = gameObject.transform.position + new Vector3(x, 0f, 0f);
			}
			else
			{
				gameObject.transform.localScale = Vector3.Scale(gameObject.transform.localScale, new Vector3(-1f, 1f, 1f));
			}
		}
		gameObject.transform.parent = ((absoluteRoomFromPosition == null) ? null : absoluteRoomFromPosition.hierarchyParent);
		DepthLookupManager.ProcessRenderer(gameObject.GetComponentInChildren<Renderer>());
		if (component2 != null)
		{
			component2.UpdateZDepth();
		}
		for (int k = 0; k < osd.width; k++)
		{
			for (int l = 0; l < osd.height; l++)
			{
				CellVisualData cellVisualData2 = d.data.cellData[ix + k][iy + l].cellVisualData;
				if (occupySpace == DungeonTileStampData.StampSpace.OBJECT_SPACE)
				{
					cellVisualData2.containsObjectSpaceStamp = true;
				}
				if (occupySpace == DungeonTileStampData.StampSpace.WALL_SPACE)
				{
					cellVisualData2.containsWallSpaceStamp = true;
				}
				if (occupySpace == DungeonTileStampData.StampSpace.BOTH_SPACES)
				{
					cellVisualData2.containsObjectSpaceStamp = true;
					cellVisualData2.containsWallSpaceStamp = true;
				}
			}
		}
		return gameObject;
	}

	public void ApplySpriteStamp(int ix, int iy, SpriteStampData ssd, Dungeon d, tk2dTileMap map)
	{
		DungeonTileStampData.StampSpace occupySpace = ssd.occupySpace;
		for (int i = 0; i < ssd.width; i++)
		{
			for (int j = 0; j < ssd.height; j++)
			{
				CellVisualData cellVisualData = d.data.cellData[ix + i][iy + j].cellVisualData;
				switch (occupySpace)
				{
				case DungeonTileStampData.StampSpace.BOTH_SPACES:
					if (cellVisualData.containsObjectSpaceStamp || cellVisualData.containsWallSpaceStamp)
					{
						return;
					}
					break;
				case DungeonTileStampData.StampSpace.OBJECT_SPACE:
					if (cellVisualData.containsObjectSpaceStamp)
					{
						return;
					}
					break;
				case DungeonTileStampData.StampSpace.WALL_SPACE:
					if (cellVisualData.containsWallSpaceStamp)
					{
						return;
					}
					break;
				}
			}
		}
		int num = ((occupySpace != 0) ? GlobalDungeonData.wallStampLayerIndex : GlobalDungeonData.objectStampLayerIndex);
		float z = map.data.Layers[num].z;
		GameObject gameObject = new GameObject(ssd.spriteReference.name);
		gameObject.transform.position = new Vector3(ix, iy, z);
		SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = ssd.spriteReference;
		DepthLookupManager.ProcessRenderer(spriteRenderer);
		for (int k = 0; k < ssd.width; k++)
		{
			for (int l = 0; l < ssd.height; l++)
			{
				CellVisualData cellVisualData2 = d.data.cellData[ix + k][iy + l].cellVisualData;
				if (occupySpace == DungeonTileStampData.StampSpace.OBJECT_SPACE)
				{
					cellVisualData2.containsObjectSpaceStamp = true;
				}
				if (occupySpace == DungeonTileStampData.StampSpace.WALL_SPACE)
				{
					cellVisualData2.containsWallSpaceStamp = true;
				}
				if (occupySpace == DungeonTileStampData.StampSpace.BOTH_SPACES)
				{
					cellVisualData2.containsObjectSpaceStamp = true;
					cellVisualData2.containsWallSpaceStamp = true;
				}
			}
		}
	}

	private TileIndexGrid GetCeilingBorderIndexGrid(CellData current, Dungeon d)
	{
		TileIndexGrid roomCeilingBorderGrid = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].roomCeilingBorderGrid;
		if (roomCeilingBorderGrid == null)
		{
			roomCeilingBorderGrid = d.roomMaterialDefinitions[0].roomCeilingBorderGrid;
		}
		return roomCeilingBorderGrid;
	}

	private int GetCeilingCenterIndex(CellData current, TileIndexGrid gridToUse)
	{
		if (gridToUse.CeilingBorderUsesDistancedCenters)
		{
			int count = gridToUse.centerIndices.indices.Count;
			int index = Mathf.Max(0, Mathf.Min(Mathf.FloorToInt(current.distanceFromNearestRoom) - 1, count - 1));
			return gridToUse.centerIndices.indices[index];
		}
		return gridToUse.centerIndices.GetIndexByWeight();
	}

	private void BuildOuterBorderIndex(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		bool isNorthBorder = (d.data.isWall(ix, iy + 1) || d.data.isTopWall(ix, iy + 1)) && !d.data.isAnyFaceWall(ix, iy + 1);
		bool isNortheastBorder = (d.data.isWall(ix + 1, iy + 1) || d.data.isTopWall(ix + 1, iy + 1)) && !d.data.isAnyFaceWall(ix + 1, iy + 1);
		bool isEastBorder = (d.data.isWall(ix + 1, iy) || d.data.isTopWall(ix + 1, iy)) && !d.data.isAnyFaceWall(ix + 1, iy);
		bool isSoutheastBorder = (d.data.isWall(ix + 1, iy - 1) || d.data.isTopWall(ix + 1, iy - 1)) && !d.data.isAnyFaceWall(ix + 1, iy - 1);
		bool isSouthBorder = (d.data.isWall(ix, iy - 1) || d.data.isTopWall(ix, iy - 1)) && !d.data.isAnyFaceWall(ix, iy - 1);
		bool isSouthwestBorder = (d.data.isWall(ix - 1, iy - 1) || d.data.isTopWall(ix - 1, iy - 1)) && !d.data.isAnyFaceWall(ix - 1, iy - 1);
		bool isWestBorder = (d.data.isWall(ix - 1, iy) || d.data.isTopWall(ix - 1, iy)) && !d.data.isAnyFaceWall(ix - 1, iy);
		bool isNorthwestBorder = (d.data.isWall(ix - 1, iy + 1) || d.data.isTopWall(ix - 1, iy + 1)) && !d.data.isAnyFaceWall(ix - 1, iy + 1);
		int num = -1;
		TileIndexGrid outerCeilingBorderGrid = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].outerCeilingBorderGrid;
		num = outerCeilingBorderGrid.GetIndexGivenSides(isNorthBorder, isNortheastBorder, isEastBorder, isSoutheastBorder, isSouthBorder, isSouthwestBorder, isWestBorder, isNorthwestBorder);
		if (num != -1 && !current.cellVisualData.shouldIgnoreWallDrawing)
		{
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, num);
		}
	}

	private bool IsCardinalBorder(CellData current, Dungeon d, int ix, int iy)
	{
		bool flag = d.data.isTopWall(ix, iy) && !d.data[ix, iy + 1].cellVisualData.shouldIgnoreBorders;
		bool flag2 = ((!d.data.isWallRight(ix, iy) && !d.data.isRightTopWall(ix, iy)) || d.data.isFaceWallHigher(ix + 1, iy) || d.data.isFaceWallLower(ix + 1, iy)) && !d.data[ix + 1, iy].cellVisualData.shouldIgnoreBorders;
		bool flag3 = iy > 3 && d.data.isFaceWallHigher(ix, iy - 1) && !d.data[ix, iy - 1].cellVisualData.shouldIgnoreBorders;
		bool flag4 = ((!d.data.isWallLeft(ix, iy) && !d.data.isLeftTopWall(ix, iy)) || d.data.isFaceWallHigher(ix - 1, iy) || d.data.isFaceWallLower(ix - 1, iy)) && !d.data[ix - 1, iy].cellVisualData.shouldIgnoreBorders;
		return flag || flag2 || flag3 || flag4;
	}

	private TileIndexGrid GetTypeBorderGridForBorderIndex(CellData current, Dungeon d, out int usedVisualType)
	{
		TileIndexGrid roomCeilingBorderGrid = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex].roomCeilingBorderGrid;
		usedVisualType = current.cellVisualData.roomVisualTypeIndex;
		if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON)
		{
			if (current.nearestRoom != null && current.distanceFromNearestRoom < 4f)
			{
				if (current.cellVisualData.IsFacewallForInteriorTransition)
				{
					roomCeilingBorderGrid = d.roomMaterialDefinitions[current.cellVisualData.InteriorTransitionIndex].roomCeilingBorderGrid;
					usedVisualType = current.cellVisualData.InteriorTransitionIndex;
				}
				else if (!current.cellVisualData.IsFeatureCell)
				{
					roomCeilingBorderGrid = d.roomMaterialDefinitions[current.nearestRoom.RoomVisualSubtype].roomCeilingBorderGrid;
					usedVisualType = current.nearestRoom.RoomVisualSubtype;
				}
			}
		}
		else if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON)
		{
			roomCeilingBorderGrid = d.roomMaterialDefinitions[current.nearestRoom.RoomVisualSubtype].roomCeilingBorderGrid;
			usedVisualType = current.nearestRoom.RoomVisualSubtype;
		}
		if (roomCeilingBorderGrid == null)
		{
			roomCeilingBorderGrid = d.roomMaterialDefinitions[0].roomCeilingBorderGrid;
			usedVisualType = 0;
		}
		return roomCeilingBorderGrid;
	}

	private void BuildOcclusionPartitionIndex(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (current == null || current.cellVisualData.ceilingHasBeenProcessed || current.cellVisualData.occlusionHasBeenProcessed)
		{
			return;
		}
		int usedVisualType = -1;
		TileIndexGrid typeBorderGridForBorderIndex = GetTypeBorderGridForBorderIndex(current, d, out usedVisualType);
		if (!(typeBorderGridForBorderIndex != null))
		{
			return;
		}
		List<CellData> cellNeighbors = d.data.GetCellNeighbors(current, true);
		bool[] array = new bool[8];
		int usedVisualType2 = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (cellNeighbors[i] != null)
			{
				GetTypeBorderGridForBorderIndex(cellNeighbors[i], d, out usedVisualType2);
				if (d.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.WESTGEON || usedVisualType2 == 0 || usedVisualType == 0)
				{
					array[i] = usedVisualType != usedVisualType2;
				}
			}
		}
		int num = typeBorderGridForBorderIndex.GetIndexGivenEightSides(array);
		if (num == -1)
		{
			num = typeBorderGridForBorderIndex.centerIndices.GetIndexByWeight();
		}
		map.SetTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.occlusionPartitionIndex, num);
	}

	private bool IsBorderCell(Dungeon d, int ix, int iy)
	{
		bool flag = d.data[ix, iy + 1].diagonalWallType != 0 && (d.data[ix, iy + 1].IsTopWall() || d.data[ix, iy + 1].type == CellType.WALL);
		bool flag2 = d.data[ix + 1, iy].diagonalWallType != 0 && (d.data[ix + 1, iy].IsTopWall() || d.data[ix + 1, iy].type == CellType.WALL);
		bool flag3 = d.data[ix, iy - 1].diagonalWallType != 0 && (d.data[ix, iy - 1].IsTopWall() || d.data[ix, iy - 1].type == CellType.WALL);
		bool flag4 = d.data[ix - 1, iy].diagonalWallType != 0 && (d.data[ix - 1, iy].IsTopWall() || d.data[ix - 1, iy].type == CellType.WALL);
		bool flag5 = d.data.isTopWall(ix, iy) && !d.data[ix, iy + 1].cellVisualData.shouldIgnoreBorders;
		bool flag6 = ((!d.data.isWallRight(ix, iy) && !d.data.isRightTopWall(ix, iy)) || d.data.isFaceWallHigher(ix + 1, iy) || d.data.isFaceWallLower(ix + 1, iy)) && !d.data[ix + 1, iy].cellVisualData.shouldIgnoreBorders;
		bool flag7 = iy > 3 && d.data.isFaceWallHigher(ix, iy - 1) && !d.data[ix, iy - 1].cellVisualData.shouldIgnoreBorders;
		bool flag8 = ((!d.data.isWallLeft(ix, iy) && !d.data.isLeftTopWall(ix, iy)) || d.data.isFaceWallHigher(ix - 1, iy) || d.data.isFaceWallLower(ix - 1, iy)) && !d.data[ix - 1, iy].cellVisualData.shouldIgnoreBorders;
		bool flag9 = (!flag || !flag2) && d.data.isTopWall(ix + 1, iy) && !d.data.isTopWall(ix, iy) && (d.data.isWall(ix, iy + 1) || d.data.isTopWall(ix, iy + 1)) && !d.data[ix + 1, iy + 1].cellVisualData.shouldIgnoreBorders;
		bool flag10 = (!flag || !flag4) && d.data.isTopWall(ix - 1, iy) && !d.data.isTopWall(ix, iy) && (d.data.isWall(ix, iy + 1) || d.data.isTopWall(ix, iy + 1)) && !d.data[ix - 1, iy + 1].cellVisualData.shouldIgnoreBorders;
		bool flag11 = (!flag3 || !flag2) && iy > 3 && d.data.isFaceWallHigher(ix + 1, iy - 1) && !d.data.isFaceWallHigher(ix, iy - 1) && !d.data[ix + 1, iy - 1].cellVisualData.shouldIgnoreBorders;
		bool flag12 = (!flag3 || !flag4) && iy > 3 && d.data.isFaceWallHigher(ix - 1, iy - 1) && !d.data.isFaceWallHigher(ix, iy - 1) && !d.data[ix - 1, iy - 1].cellVisualData.shouldIgnoreBorders;
		return flag5 || flag6 || flag8 || flag7 || flag9 || flag10 || flag11 || flag12;
	}

	private void HandleRatChunkOverhangs(Dungeon d, int ix, int iy, tk2dTileMap map)
	{
		bool flag = IsBorderCell(d, ix, iy + 1);
		bool flag2 = IsBorderCell(d, ix + 1, iy);
		bool flag3 = IsBorderCell(d, ix, iy - 1);
		bool flag4 = IsBorderCell(d, ix - 1, iy);
		if ((flag && flag2) || (flag2 && flag3) || (flag3 && flag4) || (flag4 && flag))
		{
			if (!flag)
			{
				map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(d.data[ix, iy + 1].positionInTilemap.x, d.data[ix, iy + 1].positionInTilemap.y, 312);
				d.data[ix, iy + 1].cellVisualData.ceilingHasBeenProcessed = true;
			}
			if (!flag2)
			{
				map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(d.data[ix + 1, iy].positionInTilemap.x, d.data[ix + 1, iy].positionInTilemap.y, 315);
				d.data[ix + 1, iy].cellVisualData.ceilingHasBeenProcessed = true;
			}
			if (!flag3)
			{
				map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(d.data[ix, iy - 1].positionInTilemap.x, d.data[ix, iy - 1].positionInTilemap.y, 313);
			}
			if (!flag4)
			{
				map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(d.data[ix - 1, iy].positionInTilemap.x, d.data[ix - 1, iy].positionInTilemap.y, 314);
			}
		}
	}

	private void BuildBorderIndex(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (current.cellVisualData.ceilingHasBeenProcessed)
		{
			return;
		}
		bool flag = d.data[ix, iy + 1] != null && d.data[ix, iy + 1].diagonalWallType != 0 && (d.data[ix, iy + 1].IsTopWall() || d.data[ix, iy + 1].type == CellType.WALL);
		bool flag2 = d.data[ix + 1, iy] != null && d.data[ix + 1, iy].diagonalWallType != 0 && (d.data[ix + 1, iy].IsTopWall() || d.data[ix + 1, iy].type == CellType.WALL);
		bool flag3 = d.data[ix, iy - 1] != null && d.data[ix, iy - 1].diagonalWallType != 0 && (d.data[ix, iy - 1].IsTopWall() || d.data[ix, iy - 1].type == CellType.WALL);
		bool flag4 = d.data[ix - 1, iy] != null && d.data[ix - 1, iy].diagonalWallType != 0 && (d.data[ix - 1, iy].IsTopWall() || d.data[ix - 1, iy].type == CellType.WALL);
		bool flag5 = d.data.isTopWall(ix, iy) && d.data[ix, iy + 1] != null && !d.data[ix, iy + 1].cellVisualData.shouldIgnoreBorders;
		bool flag6 = ((!d.data.isWallRight(ix, iy) && !d.data.isRightTopWall(ix, iy)) || d.data.isFaceWallHigher(ix + 1, iy) || d.data.isFaceWallLower(ix + 1, iy)) && d.data[ix + 1, iy] != null && !d.data[ix + 1, iy].cellVisualData.shouldIgnoreBorders;
		bool flag7 = iy > 3 && d.data.isFaceWallHigher(ix, iy - 1) && d.data[ix, iy - 1] != null && !d.data[ix, iy - 1].cellVisualData.shouldIgnoreBorders;
		bool flag8 = ((!d.data.isWallLeft(ix, iy) && !d.data.isLeftTopWall(ix, iy)) || d.data.isFaceWallHigher(ix - 1, iy) || d.data.isFaceWallLower(ix - 1, iy)) && d.data[ix - 1, iy] != null && !d.data[ix - 1, iy].cellVisualData.shouldIgnoreBorders;
		bool flag9 = (!flag || !flag2) && d.data.isTopWall(ix + 1, iy) && !d.data.isTopWall(ix, iy) && (d.data.isWall(ix, iy + 1) || d.data.isTopWall(ix, iy + 1)) && d.data[ix + 1, iy + 1] != null && !d.data[ix + 1, iy + 1].cellVisualData.shouldIgnoreBorders;
		bool flag10 = (!flag || !flag4) && d.data.isTopWall(ix - 1, iy) && !d.data.isTopWall(ix, iy) && (d.data.isWall(ix, iy + 1) || d.data.isTopWall(ix, iy + 1)) && d.data[ix - 1, iy + 1] != null && !d.data[ix - 1, iy + 1].cellVisualData.shouldIgnoreBorders;
		bool flag11 = (!flag3 || !flag2) && iy > 3 && d.data.isFaceWallHigher(ix + 1, iy - 1) && !d.data.isFaceWallHigher(ix, iy - 1) && d.data[ix + 1, iy - 1] != null && !d.data[ix + 1, iy - 1].cellVisualData.shouldIgnoreBorders;
		bool flag12 = (!flag3 || !flag4) && iy > 3 && d.data.isFaceWallHigher(ix - 1, iy - 1) && !d.data.isFaceWallHigher(ix, iy - 1) && d.data[ix - 1, iy - 1] != null && !d.data[ix - 1, iy - 1].cellVisualData.shouldIgnoreBorders;
		int num = -1;
		int usedVisualType = -1;
		TileIndexGrid typeBorderGridForBorderIndex = GetTypeBorderGridForBorderIndex(current, d, out usedVisualType);
		if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON)
		{
			int usedVisualType2 = -1;
			if (!flag5)
			{
				flag5 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.North], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
			if (!flag9)
			{
				flag9 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.NorthEast], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
			if (!flag6)
			{
				flag6 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.East], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
			if (!flag11)
			{
				flag11 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.SouthEast], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
			if (!flag7)
			{
				flag7 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.South], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
			if (!flag12)
			{
				flag12 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.SouthWest], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
			if (!flag8)
			{
				flag8 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.West], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
			if (!flag10)
			{
				flag10 = typeBorderGridForBorderIndex != GetTypeBorderGridForBorderIndex(d.data[current.position + IntVector2.NorthWest], d, out usedVisualType2) && (usedVisualType2 == 0 || usedVisualType == 0);
			}
		}
		if (current.diagonalWallType == DiagonalWallType.NONE)
		{
			if (!flag5 && !flag9 && !flag6 && !flag11 && !flag7 && !flag12 && !flag8 && !flag10)
			{
				if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.JUNGLEGEON)
				{
					BuildBorderLayerCenterJungle(current, d, map, ix, iy);
					num = -1;
				}
				else if (typeBorderGridForBorderIndex.CeilingBorderUsesDistancedCenters)
				{
					int count = typeBorderGridForBorderIndex.centerIndices.indices.Count;
					int index = Mathf.Max(0, Mathf.Min(Mathf.FloorToInt(current.distanceFromNearestRoom) - 1, count - 1));
					num = typeBorderGridForBorderIndex.centerIndices.indices[index];
				}
				else
				{
					num = typeBorderGridForBorderIndex.centerIndices.GetIndexByWeight();
					if (d.tileIndices.globalSecondBorderTiles.Count > 0 && current.distanceFromNearestRoom < 3f && UnityEngine.Random.value > 0.5f)
					{
						num = d.tileIndices.globalSecondBorderTiles[UnityEngine.Random.Range(0, d.tileIndices.globalSecondBorderTiles.Count)];
					}
				}
			}
			else if (typeBorderGridForBorderIndex.UsesRatChunkBorders)
			{
				bool flag13 = iy > 3;
				if (flag13)
				{
					flag13 = !d.data[ix, iy - 1].HasFloorNeighbor(d.data, false, true);
				}
				TileIndexGrid.RatChunkResult result = TileIndexGrid.RatChunkResult.NONE;
				num = -1;
				num = ((!d.data[ix, iy].nearestRoom.area.PrototypeLostWoodsRoom) ? typeBorderGridForBorderIndex.GetRatChunkIndexGivenSides(flag5, flag9, flag6, flag11, flag7, flag12, flag8, flag10, flag13, out result) : typeBorderGridForBorderIndex.GetRatChunkIndexGivenSidesStatic(flag5, flag9, flag6, flag11, flag7, flag12, flag8, flag10, flag13, out result));
				if (result == TileIndexGrid.RatChunkResult.CORNER)
				{
					HandleRatChunkOverhangs(d, ix, iy, map);
				}
			}
			else
			{
				num = typeBorderGridForBorderIndex.GetIndexGivenSides(flag5, flag9, flag6, flag11, flag7, flag12, flag8, flag10);
			}
		}
		else
		{
			switch (current.diagonalWallType)
			{
			case DiagonalWallType.NORTHEAST:
				if (flag7 && flag8)
				{
					num = typeBorderGridForBorderIndex.diagonalBorderNE.GetIndexByWeight();
				}
				break;
			case DiagonalWallType.SOUTHEAST:
				num = ((!flag5 || !flag8) ? typeBorderGridForBorderIndex.GetIndexGivenSides(flag5, flag9, flag6, flag11, flag7, flag12, flag8, flag10) : typeBorderGridForBorderIndex.diagonalBorderSE.GetIndexByWeight());
				break;
			case DiagonalWallType.SOUTHWEST:
				num = ((!flag5 || !flag6) ? typeBorderGridForBorderIndex.GetIndexGivenSides(flag5, flag9, flag6, flag11, flag7, flag12, flag8, flag10) : typeBorderGridForBorderIndex.diagonalBorderSW.GetIndexByWeight());
				break;
			case DiagonalWallType.NORTHWEST:
				if (flag7 && flag6)
				{
					num = typeBorderGridForBorderIndex.diagonalBorderNW.GetIndexByWeight();
				}
				break;
			}
		}
		TileIndexGrid typeBorderGridForBorderIndex2 = GetTypeBorderGridForBorderIndex(current, d, out usedVisualType);
		if (num == -1)
		{
			return;
		}
		if (!current.cellVisualData.shouldIgnoreWallDrawing)
		{
			map.Layers[GlobalDungeonData.borderLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, num);
		}
		if (current.cellVisualData.shouldIgnoreWallDrawing)
		{
			BraveUtility.DrawDebugSquare(current.position.ToVector2(), Color.blue, 1000f);
		}
		if (flag5 && current.diagonalWallType != 0)
		{
			int num2 = -1;
			switch (current.diagonalWallType)
			{
			case DiagonalWallType.SOUTHEAST:
				num2 = typeBorderGridForBorderIndex2.diagonalCeilingSE.GetIndexByWeight();
				break;
			case DiagonalWallType.SOUTHWEST:
				num2 = typeBorderGridForBorderIndex2.diagonalCeilingSW.GetIndexByWeight();
				break;
			}
			if (num2 != -1)
			{
				map.Layers[GlobalDungeonData.ceilingLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, num2);
				AssignColorOverrideToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.ceilingLayerIndex, new Color(1f, 1f, 1f, 0f), map);
			}
			num2 = GetCeilingCenterIndex(current, typeBorderGridForBorderIndex2);
			map.Layers[GlobalDungeonData.ceilingLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y - 1, num2);
			AssignColorOverrideToTile(current.positionInTilemap.x, current.positionInTilemap.y - 1, GlobalDungeonData.ceilingLayerIndex, new Color(1f, 1f, 1f, 0f), map);
		}
		else if (flag5)
		{
			int ceilingCenterIndex = GetCeilingCenterIndex(current, typeBorderGridForBorderIndex2);
			map.Layers[GlobalDungeonData.ceilingLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, ceilingCenterIndex);
			AssignColorOverrideToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.ceilingLayerIndex, new Color(1f, 1f, 1f, 0f), map);
		}
		else if (flag7 && current.diagonalWallType != 0)
		{
			int num3 = -1;
			switch (current.diagonalWallType)
			{
			case DiagonalWallType.NORTHEAST:
				num3 = typeBorderGridForBorderIndex2.diagonalCeilingNE.GetIndexByWeight();
				break;
			case DiagonalWallType.NORTHWEST:
				num3 = typeBorderGridForBorderIndex2.diagonalCeilingNW.GetIndexByWeight();
				break;
			}
			if (num3 != -1)
			{
				map.Layers[GlobalDungeonData.ceilingLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, num3);
				AssignColorOverrideToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.ceilingLayerIndex, new Color(1f, 1f, 1f, 0f), map);
			}
		}
		else if (flag6 || flag8 || flag9 || flag10 || flag7 || flag11 || flag12)
		{
			int ceilingCenterIndex2 = GetCeilingCenterIndex(current, typeBorderGridForBorderIndex2);
			map.Layers[GlobalDungeonData.ceilingLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, ceilingCenterIndex2);
			AssignColorOverrideToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.ceilingLayerIndex, new Color(1f, 1f, 1f, 0f), map);
		}
		if (flag5 || (d.data[current.position + IntVector2.Up] != null && d.data[current.position + IntVector2.Up].IsTopWall()))
		{
			AssignColorOverrideToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.borderLayerIndex, new Color(1f, 1f, 1f, 0f), map);
		}
		else
		{
			AssignColorOverrideToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.borderLayerIndex, new Color(0f, 0f, 0f), map);
		}
	}

	private bool ProcessFacewallNeighborMetadata(int ix, int iy, Dungeon d, List<IndexNeighborDependency> neighborDependencies, bool preventWallStamping = false)
	{
		bool flag = d.data.isFaceWallLower(ix, iy);
		d.data[ix, iy].cellVisualData.containsWallSpaceStamp |= preventWallStamping;
		bool flag2 = true;
		List<CellData> list = new List<CellData>();
		for (int i = 0; i < neighborDependencies.Count; i++)
		{
			CellData cellData = d.data[new IntVector2(ix, iy) + DungeonData.GetIntVector2FromDirection(neighborDependencies[i].neighborDirection)];
			if (cellData.cellVisualData.faceWallOverrideIndex != -1 || !cellData.IsAnyFaceWall())
			{
				flag2 = false;
				break;
			}
			if (cellData.cellVisualData.roomVisualTypeIndex != d.data.cellData[ix][iy].cellVisualData.roomVisualTypeIndex)
			{
				flag2 = false;
				break;
			}
			if (cellData.position.y == iy && d.data.isFaceWallLower(cellData.position.x, cellData.position.y) != flag)
			{
				flag2 = false;
				break;
			}
			list.Add(cellData);
			cellData.cellVisualData.containsWallSpaceStamp |= preventWallStamping;
			cellData.cellVisualData.faceWallOverrideIndex = neighborDependencies[i].neighborIndex;
		}
		if (!flag2)
		{
			for (int j = 0; j < list.Count; j++)
			{
				CellData cellData2 = list[j];
				cellData2.cellVisualData.faceWallOverrideIndex = -1;
			}
		}
		return flag2;
	}

	private bool FaceWallTypesMatch(CellData c1, CellData c2)
	{
		if (c1.IsLowerFaceWall() && c2.IsLowerFaceWall())
		{
			return true;
		}
		if (c1.IsUpperFacewall() && c2.IsUpperFacewall())
		{
			return true;
		}
		return false;
	}

	private bool IsNorthernmostColumnarFacewall(CellData current, Dungeon d, int ix, int iy)
	{
		for (CellData cellData = d.data[ix, iy + 1]; cellData != null; cellData = d.data[cellData.position.x, cellData.position.y + 1])
		{
			if (cellData.nearestRoom != current.nearestRoom)
			{
				return true;
			}
			if (cellData.type == CellType.FLOOR)
			{
				return false;
			}
			if (!d.data.CheckInBounds(cellData.position.x, cellData.position.y + 1))
			{
				return true;
			}
		}
		return true;
	}

	private void ProcessFacewallType(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy, TilesetIndexMetadata.TilesetFlagType wallType, TilesetIndexMetadata.TilesetFlagType tileOverrideType)
	{
		int num = current.cellVisualData.roomVisualTypeIndex;
		if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON && num == 0)
		{
			bool flag = false;
			int num2 = -1;
			for (int i = 0; i < current.nearestRoom.connectedRooms.Count; i++)
			{
				if (current.nearestRoom.GetDirectionToConnectedRoom(current.nearestRoom.connectedRooms[i]) == DungeonData.Direction.NORTH && current.nearestRoom.connectedRooms[i].RoomVisualSubtype != 0)
				{
					flag = true;
					num2 = current.nearestRoom.connectedRooms[i].RoomVisualSubtype;
					break;
				}
			}
			if (flag && current.cellVisualData.IsFacewallForInteriorTransition)
			{
				num = num2;
			}
		}
		CellData cellData = d.data.cellData[ix + 1][iy];
		CellData cellData2 = d.data.cellData[ix - 1][iy];
		if (current.cellVisualData.faceWallOverrideIndex != -1)
		{
			List<IndexNeighborDependency> dependencies = d.tileIndices.dungeonCollection.GetDependencies(current.cellVisualData.faceWallOverrideIndex);
			if (dependencies != null && dependencies.Count > 0 && current.IsUpperFacewall())
			{
				for (int j = 0; j < dependencies.Count; j++)
				{
					if (dependencies[j].neighborDirection == DungeonData.Direction.NORTH)
					{
						d.data.cellData[ix][iy + 1].cellVisualData.UsesCustomIndexOverride01 = true;
						d.data.cellData[ix][iy + 1].cellVisualData.CustomIndexOverride01 = dependencies[j].neighborIndex;
						d.data.cellData[ix][iy + 1].cellVisualData.CustomIndexOverride01Layer = GlobalDungeonData.borderLayerIndex;
					}
				}
			}
			map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, current.cellVisualData.faceWallOverrideIndex);
		}
		else
		{
			if (current.diagonalWallType != 0)
			{
				int index = -1;
				if (current.diagonalWallType == DiagonalWallType.NORTHEAST)
				{
					switch (wallType)
					{
					case TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER:
						GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DIAGONAL_FACEWALL_LOWER_NE], num, out index);
						map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index);
						break;
					case TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER:
						GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DIAGONAL_FACEWALL_UPPER_NE], num, out index);
						map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index);
						GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DIAGONAL_FACEWALL_TOP_NE], num, out index);
						map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, index);
						break;
					}
				}
				else if (current.diagonalWallType == DiagonalWallType.NORTHWEST)
				{
					switch (wallType)
					{
					case TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER:
						GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DIAGONAL_FACEWALL_LOWER_NW], num, out index);
						map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index);
						break;
					case TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER:
						GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DIAGONAL_FACEWALL_UPPER_NW], num, out index);
						map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index);
						GetMetadataFromTupleArray(current, m_metadataLookupTable[TilesetIndexMetadata.TilesetFlagType.DIAGONAL_FACEWALL_TOP_NW], num, out index);
						map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y + 1, index);
						break;
					}
				}
				else
				{
					Debug.LogError("Attempting to stamp a facewall tile on a non-facewall diagonal type.");
				}
				switch (wallType)
				{
				case TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER:
					if (current.diagonalWallType == DiagonalWallType.NORTHEAST)
					{
						AssignSpecificColorsToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 0f, 1f), new Color(0f, 0f, 1f), new Color(0f, 0f, 1f), new Color(0f, 0.5f, 1f), map);
					}
					else if (current.diagonalWallType == DiagonalWallType.NORTHWEST)
					{
						AssignSpecificColorsToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 0f, 1f), new Color(0f, 0f, 1f), new Color(0f, 0.5f, 1f), new Color(0f, 0f, 1f), map);
					}
					break;
				case TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER:
					if (current.diagonalWallType == DiagonalWallType.NORTHEAST)
					{
						AssignSpecificColorsToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 0f, 1f), new Color(0f, 0.5f, 1f), new Color(0f, 0.5f, 1f), new Color(0f, 1f, 1f), map);
					}
					else if (current.diagonalWallType == DiagonalWallType.NORTHWEST)
					{
						AssignSpecificColorsToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 0.5f, 1f), new Color(0f, 0f, 1f), new Color(0f, 1f, 1f), new Color(0f, 0.5f, 1f), map);
					}
					break;
				}
				return;
			}
			int index2 = -1;
			bool flag2 = false;
			int num3 = 0;
			while (!flag2 && num3 < 1000)
			{
				num3++;
				flag2 = true;
				TilesetIndexMetadata metadataFromTupleArray = GetMetadataFromTupleArray(current, m_metadataLookupTable[tileOverrideType], num, out index2);
				List<IndexNeighborDependency> dependencies2 = d.tileIndices.dungeonCollection.GetDependencies(index2);
				if (metadataFromTupleArray != null && dependencies2 != null && dependencies2.Count > 0)
				{
					flag2 = ProcessFacewallNeighborMetadata(ix, iy, d, dependencies2, metadataFromTupleArray.preventWallStamping);
				}
			}
			if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON && (tileOverrideType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_RIGHTCORNER || tileOverrideType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_LEFTCORNER || tileOverrideType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_RIGHTCORNER || tileOverrideType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_LEFTCORNER))
			{
				current.cellVisualData.containsWallSpaceStamp = true;
			}
			BraveUtility.Assert(index2 == -1, "FACEWALL INDEX -1, there are no facewalls defined");
			map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, index2);
		}
		if (current.parentRoom == null || current.parentRoom.area.prototypeRoom == null || !current.parentRoom.area.prototypeRoom.preventFacewallAO)
		{
			if (wallType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, t.aoTileIndices.AOBottomWallBaseTileIndex);
			}
			bool flag3 = cellData.type == CellType.WALL && cellData.diagonalWallType == DiagonalWallType.NONE && (!d.data.isFaceWallRight(ix, iy) || (wallType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER && cellData.IsUpperFacewall()));
			bool flag4 = cellData2.type == CellType.WALL && cellData2.diagonalWallType == DiagonalWallType.NONE && (!d.data.isFaceWallLeft(ix, iy) || (wallType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER && cellData2.IsUpperFacewall()));
			if (flag4 && flag3)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, (wallType != TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER) ? t.aoTileIndices.AOTopFacewallBothIndex : t.aoTileIndices.AOBottomWallTileBothIndex);
			}
			else if (flag4)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, (wallType != TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER) ? t.aoTileIndices.AOTopFacewallLeftIndex : t.aoTileIndices.AOBottomWallTileLeftIndex);
			}
			else if (flag3)
			{
				map.Layers[GlobalDungeonData.shadowLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, (wallType != TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER) ? t.aoTileIndices.AOTopFacewallRightIndex : t.aoTileIndices.AOBottomWallTileRightIndex);
			}
		}
		if (wallType == TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER)
		{
			AssignColorGradientToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 0f, 1f), new Color(0f, 0.5f, 1f), map);
		}
		else
		{
			AssignColorGradientToTile(current.positionInTilemap.x, current.positionInTilemap.y, GlobalDungeonData.collisionLayerIndex, new Color(0f, 0.5f, 1f), new Color(0f, 1f, 1f), map);
		}
	}

	private int FindValidFacewallExpanse(int ix, int iy, Dungeon d, FacewallIndexGridDefinition gridDefinition)
	{
		int num = 0;
		int roomVisualTypeIndex = d.data[ix, iy].cellVisualData.roomVisualTypeIndex;
		while (d.data.isFaceWallLower(ix, iy) && d.data[ix, iy].cellVisualData.faceWallOverrideIndex == -1 && d.data[ix, iy].cellVisualData.roomVisualTypeIndex == roomVisualTypeIndex)
		{
			bool flag = !d.data.isFaceWallLeft(ix, iy) || !d.data.isFaceWallRight(ix, iy);
			if ((!gridDefinition.canExistInCorners && flag) || (d.data[ix, iy - 2].isExitCell && !gridDefinition.canBePlacedInExits))
			{
				break;
			}
			ix++;
			num++;
			if (num >= gridDefinition.maxWidth || (num > gridDefinition.minWidth && UnityEngine.Random.value < gridDefinition.perTileFailureRate))
			{
				break;
			}
		}
		return num;
	}

	private bool AssignFacewallGrid(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy, FacewallIndexGridDefinition gridDefinition)
	{
		int num = FindValidFacewallExpanse(ix, iy, d, gridDefinition);
		if (num < gridDefinition.minWidth)
		{
			return false;
		}
		TileIndexGrid grid = gridDefinition.grid;
		int num2 = 0;
		int num3 = num;
		int num4 = num3;
		int num5 = 0;
		if (gridDefinition.hasIntermediaries)
		{
			num4 = UnityEngine.Random.Range(gridDefinition.minIntermediaryBuffer, gridDefinition.maxIntermediaryBuffer + 1);
		}
		bool flag = true;
		int num6 = 0;
		while (num3 > 0)
		{
			CellData cellData = d.data[ix + num2, iy];
			CellData cellData2 = d.data[ix + num2, iy + 1];
			if (num5 > 0)
			{
				num5--;
				cellData.cellVisualData.faceWallOverrideIndex = grid.doubleNubsBottom.GetIndexByWeight();
				cellData2.cellVisualData.faceWallOverrideIndex = grid.doubleNubsTop.GetIndexByWeight();
				if (num5 == 0)
				{
					flag = true;
					num4 = UnityEngine.Random.Range(gridDefinition.minIntermediaryBuffer, gridDefinition.maxIntermediaryBuffer + 1);
				}
			}
			else
			{
				bool flag2 = false;
				BraveUtility.DrawDebugSquare(cellData.position.ToVector2(), Color.blue, 1000f);
				num4--;
				if (num4 <= 0)
				{
					if (gridDefinition.hasIntermediaries)
					{
						num5 = UnityEngine.Random.Range(gridDefinition.minIntermediaryLength, gridDefinition.maxIntermediaryLength + 1);
					}
					flag2 = true;
				}
				if (flag)
				{
					cellData.cellVisualData.faceWallOverrideIndex = grid.bottomLeftIndices.GetIndexByWeight();
					cellData2.cellVisualData.faceWallOverrideIndex = grid.topLeftIndices.GetIndexByWeight();
					cellData.cellVisualData.containsWallSpaceStamp = true;
					cellData2.cellVisualData.containsWallSpaceStamp = true;
				}
				else if (flag2 || num3 == 1)
				{
					cellData.cellVisualData.faceWallOverrideIndex = grid.bottomRightIndices.GetIndexByWeight();
					cellData2.cellVisualData.faceWallOverrideIndex = grid.topRightIndices.GetIndexByWeight();
					cellData.cellVisualData.containsWallSpaceStamp = true;
					cellData2.cellVisualData.containsWallSpaceStamp = true;
					if (flag2 && num5 == 0)
					{
						flag = true;
						num4 = UnityEngine.Random.Range(gridDefinition.minIntermediaryBuffer, gridDefinition.maxIntermediaryBuffer + 1);
					}
				}
				else
				{
					cellData.cellVisualData.faceWallOverrideIndex = ((!gridDefinition.middleSectionSequential) ? grid.bottomIndices.GetIndexByWeight() : grid.bottomIndices.indices[num6]);
					if (gridDefinition.topsMatchBottoms)
					{
						cellData2.cellVisualData.faceWallOverrideIndex = grid.topIndices.indices[grid.bottomIndices.GetIndexOfIndex(cellData.cellVisualData.faceWallOverrideIndex)];
					}
					else
					{
						cellData2.cellVisualData.faceWallOverrideIndex = ((!gridDefinition.middleSectionSequential) ? grid.topIndices.GetIndexByWeight() : grid.topIndices.indices[num6]);
					}
					num6 = (num6 + 1) % grid.bottomIndices.indices.Count;
					cellData.cellVisualData.forcedMatchingStyle = gridDefinition.forcedStampMatchingStyle;
					cellData2.cellVisualData.forcedMatchingStyle = gridDefinition.forcedStampMatchingStyle;
				}
				flag = false;
				cellData.cellVisualData.containsObjectSpaceStamp = cellData.cellVisualData.containsObjectSpaceStamp || !gridDefinition.canAcceptFloorDecoration;
				cellData2.cellVisualData.containsObjectSpaceStamp = cellData2.cellVisualData.containsObjectSpaceStamp || !gridDefinition.canAcceptFloorDecoration;
				cellData.cellVisualData.facewallGridPreventsWallSpaceStamp = !gridDefinition.canAcceptWallDecoration;
				cellData2.cellVisualData.facewallGridPreventsWallSpaceStamp = !gridDefinition.canAcceptWallDecoration;
				cellData.cellVisualData.containsWallSpaceStamp = cellData.cellVisualData.containsWallSpaceStamp || !gridDefinition.canAcceptWallDecoration;
				cellData2.cellVisualData.containsWallSpaceStamp = cellData2.cellVisualData.containsWallSpaceStamp || !gridDefinition.canAcceptWallDecoration;
			}
			num2++;
			num3--;
		}
		return true;
	}

	private void ProcessFacewallIndices(CellData current, Dungeon d, tk2dTileMap map, int ix, int iy)
	{
		if (current.cellVisualData.shouldIgnoreWallDrawing)
		{
			return;
		}
		DungeonMaterial dungeonMaterial = d.roomMaterialDefinitions[current.cellVisualData.roomVisualTypeIndex];
		if (current.cellVisualData.IsFacewallForInteriorTransition)
		{
			dungeonMaterial = d.roomMaterialDefinitions[current.cellVisualData.InteriorTransitionIndex];
		}
		if (d.data.isSingleCellWall(ix, iy))
		{
			map.Layers[GlobalDungeonData.collisionLayerIndex].SetTile(current.positionInTilemap.x, current.positionInTilemap.y, GetIndexFromTileArray(current, t.chestHighWallIndices).index);
		}
		else if (d.data.isFaceWallLower(ix, iy))
		{
			if (dungeonMaterial != null && dungeonMaterial.usesFacewallGrids)
			{
				FacewallIndexGridDefinition facewallIndexGridDefinition = dungeonMaterial.facewallGrids[UnityEngine.Random.Range(0, dungeonMaterial.facewallGrids.Length)];
				if (current.cellVisualData.faceWallOverrideIndex == -1 && UnityEngine.Random.value < facewallIndexGridDefinition.chanceToPlaceIfPossible)
				{
					AssignFacewallGrid(current, d, map, ix, iy, facewallIndexGridDefinition);
				}
			}
			bool flag = d.data.isWallLeft(ix, iy) && !d.data.isFaceWallLeft(ix, iy);
			bool flag2 = d.data.isWallRight(ix, iy) && !d.data.isFaceWallRight(ix, iy);
			bool flag3 = !d.data.isWallLeft(ix, iy);
			bool flag4 = !d.data.isWallRight(ix, iy);
			if (flag3 && dungeonMaterial.forceEdgesDiagonal)
			{
				current.diagonalWallType = DiagonalWallType.NORTHEAST;
			}
			if (flag4 && dungeonMaterial.forceEdgesDiagonal)
			{
				current.diagonalWallType = DiagonalWallType.NORTHWEST;
			}
			if (flag3 && !flag4 && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_LEFTEDGE, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_LEFTEDGE);
			}
			else if (flag4 && !flag3 && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_RIGHTEDGE, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_RIGHTEDGE);
			}
			else if (flag && !flag2 && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_LEFTCORNER, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_LEFTCORNER);
			}
			else if (flag2 && !flag && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_RIGHTCORNER, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER_RIGHTCORNER);
			}
			else
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER);
			}
		}
		else if (d.data.isFaceWallHigher(ix, iy))
		{
			bool flag5 = d.data.isWallLeft(ix, iy) && !d.data.isFaceWallLeft(ix, iy);
			bool flag6 = d.data.isWallRight(ix, iy) && !d.data.isFaceWallRight(ix, iy);
			bool flag7 = !d.data.isWallLeft(ix, iy) || (d.data.isFaceWallLeft(ix, iy) && !d.data[ix - 1, iy].IsUpperFacewall());
			bool flag8 = !d.data.isWallRight(ix, iy) || (d.data.isFaceWallRight(ix, iy) && !d.data[ix + 1, iy].IsUpperFacewall());
			if (flag7 && !flag8 && dungeonMaterial.forceEdgesDiagonal)
			{
				current.diagonalWallType = DiagonalWallType.NORTHEAST;
			}
			if (flag8 && !flag7 && dungeonMaterial.forceEdgesDiagonal)
			{
				current.diagonalWallType = DiagonalWallType.NORTHWEST;
			}
			if (flag7 && !flag8 && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_LEFTEDGE, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_LEFTEDGE);
			}
			else if (flag8 && !flag7 && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_RIGHTEDGE, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_RIGHTEDGE);
			}
			else if (flag5 && !flag6 && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_LEFTCORNER, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_LEFTCORNER);
			}
			else if (flag6 && !flag5 && HasMetadataForRoomType(TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_RIGHTCORNER, current.cellVisualData.roomVisualTypeIndex))
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER_RIGHTCORNER);
			}
			else
			{
				ProcessFacewallType(current, d, map, ix, iy, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER, TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER);
			}
		}
	}

	public IEnumerator ConstructTK2DDungeon(Dungeon d, tk2dTileMap map)
	{
		for (int j = 0; j < d.data.Width; j++)
		{
			for (int k = 0; k < d.data.Height; k++)
			{
				BuildTileIndicesForCell(d, map, j, k);
			}
		}
		yield return null;
		if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON || d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FINALGEON)
		{
			for (int l = 0; l < d.data.Width; l++)
			{
				for (int m = 0; m < d.data.Height; m++)
				{
					CellData cellData = d.data.cellData[l][m];
					if (cellData != null)
					{
						if (cellData.type == CellType.FLOOR)
						{
							BuildShadowIndex(cellData, d, map, l, m);
						}
						else if (cellData.type == CellType.PIT)
						{
							BuildPitShadowIndex(cellData, d, map, l, m);
						}
					}
				}
			}
		}
		TK2DInteriorDecorator decorator = new TK2DInteriorDecorator(this);
		decorator.PlaceLightDecoration(d, map);
		for (int i = 0; i < d.data.rooms.Count; i++)
		{
			if (d.data.rooms[i].area.prototypeRoom == null || d.data.rooms[i].area.prototypeRoom.usesProceduralDecoration)
			{
				decorator.HandleRoomDecoration(d.data.rooms[i], d, map);
			}
			else
			{
				decorator.HandleRoomDecorationMinimal(d.data.rooms[i], d, map);
			}
			if (i % 5 == 0)
			{
				yield return null;
			}
		}
		if ((d.data.rooms.Count - 1) % 5 != 0)
		{
			yield return null;
		}
		map.Editor__SpriteCollection = t.dungeonCollection;
		if (d.ActuallyGenerateTilemap)
		{
			IEnumerator BuildTracker = map.DeferredBuild(tk2dTileMap.BuildFlags.Default);
			while (BuildTracker.MoveNext())
			{
				yield return null;
			}
		}
	}

	private void HandlePitBorderTilePlacement(CellData cell, TileIndexGrid borderGrid, Layer tileMapLayer, tk2dTileMap tileMap, Dungeon d)
	{
		if (borderGrid.PitBorderIsInternal)
		{
			if (cell.type == CellType.PIT)
			{
				List<CellData> cellNeighbors = d.data.GetCellNeighbors(cell, true);
				bool flag = cellNeighbors[0] != null && cellNeighbors[0].type == CellType.PIT;
				bool flag2 = cellNeighbors[1] != null && cellNeighbors[1].type == CellType.PIT;
				bool flag3 = cellNeighbors[2] != null && cellNeighbors[2].type == CellType.PIT;
				bool flag4 = cellNeighbors[3] != null && cellNeighbors[3].type == CellType.PIT;
				bool flag5 = cellNeighbors[4] != null && cellNeighbors[4].type == CellType.PIT;
				bool flag6 = cellNeighbors[5] != null && cellNeighbors[5].type == CellType.PIT;
				bool flag7 = cellNeighbors[6] != null && cellNeighbors[6].type == CellType.PIT;
				bool flag8 = cellNeighbors[7] != null && cellNeighbors[7].type == CellType.PIT;
				int indexGivenSides = borderGrid.GetIndexGivenSides(!flag, !flag2, !flag3, !flag4, !flag5, !flag6, !flag7, !flag8);
				tileMapLayer.SetTile(cell.positionInTilemap.x, cell.positionInTilemap.y, indexGivenSides);
			}
			return;
		}
		if (cell.type == CellType.PIT)
		{
			List<CellData> cellNeighbors2 = d.data.GetCellNeighbors(cell);
			bool isNorthBorder = cellNeighbors2[0] != null && cellNeighbors2[0].type == CellType.PIT;
			bool isEastBorder = cellNeighbors2[1] != null && cellNeighbors2[1].type == CellType.PIT;
			bool isSouthBorder = cellNeighbors2[2] != null && cellNeighbors2[2].type == CellType.PIT;
			bool isWestBorder = cellNeighbors2[3] != null && cellNeighbors2[3].type == CellType.PIT;
			int internalIndexGivenSides = borderGrid.GetInternalIndexGivenSides(isNorthBorder, isEastBorder, isSouthBorder, isWestBorder);
			if (internalIndexGivenSides != -1)
			{
				tileMapLayer.SetTile(cell.positionInTilemap.x, cell.positionInTilemap.y, internalIndexGivenSides);
			}
			return;
		}
		List<CellData> cellNeighbors3 = d.data.GetCellNeighbors(cell, true);
		bool flag9 = cellNeighbors3[0] != null && (cellNeighbors3[0].type == CellType.PIT || cellNeighbors3[0].cellVisualData.RequiresPitBordering);
		bool flag10 = cellNeighbors3[1] != null && (cellNeighbors3[1].type == CellType.PIT || cellNeighbors3[1].cellVisualData.RequiresPitBordering);
		bool flag11 = cellNeighbors3[2] != null && (cellNeighbors3[2].type == CellType.PIT || cellNeighbors3[2].cellVisualData.RequiresPitBordering);
		bool flag12 = cellNeighbors3[3] != null && (cellNeighbors3[3].type == CellType.PIT || cellNeighbors3[3].cellVisualData.RequiresPitBordering);
		bool flag13 = cellNeighbors3[4] != null && (cellNeighbors3[4].type == CellType.PIT || cellNeighbors3[4].cellVisualData.RequiresPitBordering);
		bool flag14 = cellNeighbors3[5] != null && (cellNeighbors3[5].type == CellType.PIT || cellNeighbors3[5].cellVisualData.RequiresPitBordering);
		bool flag15 = cellNeighbors3[6] != null && (cellNeighbors3[6].type == CellType.PIT || cellNeighbors3[6].cellVisualData.RequiresPitBordering);
		bool flag16 = cellNeighbors3[7] != null && (cellNeighbors3[7].type == CellType.PIT || cellNeighbors3[7].cellVisualData.RequiresPitBordering);
		if (!flag9 && !flag10 && !flag11 && !flag12 && !flag13 && !flag14 && !flag15 && !flag16)
		{
			return;
		}
		int indexGivenSides2 = borderGrid.GetIndexGivenSides(flag9, flag10, flag11, flag12, flag13, flag14, flag15, flag16);
		if (borderGrid.PitBorderOverridesFloorTile)
		{
			tileMap.SetTile(cell.positionInTilemap.x, cell.positionInTilemap.y, GlobalDungeonData.floorLayerIndex, indexGivenSides2);
		}
		else
		{
			tileMapLayer.SetTile(cell.positionInTilemap.x, cell.positionInTilemap.y, indexGivenSides2);
		}
		if (borderGrid.PitBorderOverridesFloorTile)
		{
			TileIndexGrid pitLayoutGrid = d.roomMaterialDefinitions[cell.cellVisualData.roomVisualTypeIndex].pitLayoutGrid;
			if (pitLayoutGrid == null)
			{
				pitLayoutGrid = d.roomMaterialDefinitions[0].pitLayoutGrid;
			}
			tileMap.Layers[GlobalDungeonData.pitLayerIndex].SetTile(cell.positionInTilemap.x, cell.positionInTilemap.y, pitLayoutGrid.centerIndices.GetIndexByWeight());
		}
	}

	private void HandlePitTilePlacement(CellData cell, TileIndexGrid pitGrid, Layer tileMapLayer, Dungeon d)
	{
		if (pitGrid == null)
		{
			return;
		}
		List<CellData> cellNeighbors = d.data.GetCellNeighbors(cell);
		bool flag = cellNeighbors[0] != null && cellNeighbors[0].type == CellType.PIT;
		bool flag2 = cellNeighbors[1] != null && cellNeighbors[1].type == CellType.PIT;
		bool flag3 = cellNeighbors[2] != null && cellNeighbors[2].type == CellType.PIT;
		bool flag4 = cellNeighbors[3] != null && cellNeighbors[3].type == CellType.PIT;
		bool flag5 = BCheck(d, cell.position.x, cell.position.y + 2) && d.data.cellData[cell.position.x][cell.position.y + 2].type == CellType.PIT;
		bool flag6 = BCheck(d, cell.position.x, cell.position.y + 3) && d.data.cellData[cell.position.x][cell.position.y + 3].type == CellType.PIT;
		if (cell.cellVisualData.pitOverrideIndex > -1)
		{
			tileMapLayer.SetTile(cell.positionInTilemap.x, cell.positionInTilemap.y, cell.cellVisualData.pitOverrideIndex);
		}
		else
		{
			if (GameManager.Instance.Dungeon.debugSettings.WALLS_ARE_PITS)
			{
				if (cellNeighbors[2] != null && cellNeighbors[2].isExitCell)
				{
					flag3 = true;
				}
				if (cellNeighbors[0] != null && cellNeighbors[0].isExitCell)
				{
					flag = true;
				}
				if (BCheck(d, cell.position.x, cell.position.y + 2) && d.data.cellData[cell.position.x][cell.position.y + 2].isExitCell)
				{
					flag5 = true;
				}
				if (BCheck(d, cell.position.x, cell.position.y + 3) && d.data.cellData[cell.position.x][cell.position.y + 3].isExitCell)
				{
					flag6 = true;
				}
			}
			int tile = pitGrid.GetIndexGivenSides(!flag, !flag5, !flag6, !flag2, !flag3, !flag4);
			if (pitGrid.PitInternalSquareGrids.Count > 0 && UnityEngine.Random.value < pitGrid.PitInternalSquareOptions.PitSquareChance && (pitGrid.PitInternalSquareOptions.CanBeFlushLeft || flag4) && (pitGrid.PitInternalSquareOptions.CanBeFlushBottom || flag3) && flag2 && flag && flag5 && flag6)
			{
				bool flag7 = BCheck(d, cell.position.x + 2, cell.position.y) && d.data.cellData[cell.position.x + 2][cell.position.y].type == CellType.PIT;
				bool flag8 = BCheck(d, cell.position.x + 1, cell.position.y + 1) && d.data.cellData[cell.position.x + 1][cell.position.y + 1].type == CellType.PIT;
				bool flag9 = BCheck(d, cell.position.x + 1, cell.position.y + 2) && d.data.cellData[cell.position.x + 1][cell.position.y + 2].type == CellType.PIT;
				bool flag10 = BCheck(d, cell.position.x + 1, cell.position.y + 3) && d.data.cellData[cell.position.x + 1][cell.position.y + 3].type == CellType.PIT;
				if ((pitGrid.PitInternalSquareOptions.CanBeFlushRight || flag7) && flag8 && flag10 && flag9)
				{
					TileIndexGrid tileIndexGrid = pitGrid.PitInternalSquareGrids[UnityEngine.Random.Range(0, pitGrid.PitInternalSquareGrids.Count)];
					tile = tileIndexGrid.bottomLeftIndices.GetIndexByWeight();
					d.data.cellData[cell.position.x + 1][cell.position.y].cellVisualData.pitOverrideIndex = tileIndexGrid.bottomRightIndices.GetIndexByWeight();
					d.data.cellData[cell.position.x][cell.position.y + 1].cellVisualData.pitOverrideIndex = tileIndexGrid.topLeftIndices.GetIndexByWeight();
					d.data.cellData[cell.position.x + 1][cell.position.y + 1].cellVisualData.pitOverrideIndex = tileIndexGrid.topRightIndices.GetIndexByWeight();
				}
			}
			tileMapLayer.SetTile(cell.positionInTilemap.x, cell.positionInTilemap.y, tile);
		}
		if (flag && !flag5)
		{
			AssignColorGradientToTile(cell.positionInTilemap.x, cell.positionInTilemap.y, GlobalDungeonData.pitLayerIndex, new Color(1f, 1f, 1f, 1f), new Color(0f, 0f, 0f, 1f), GameManager.Instance.Dungeon.MainTilemap);
		}
	}
}
