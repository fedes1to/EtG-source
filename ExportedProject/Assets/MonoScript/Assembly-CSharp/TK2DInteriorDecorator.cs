using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class TK2DInteriorDecorator
{
	protected class ViableStampCategorySet
	{
		public DungeonTileStampData.StampCategory category;

		public DungeonTileStampData.StampPlacementRule placement;

		public DungeonTileStampData.StampSpace space;

		public ViableStampCategorySet(DungeonTileStampData.StampCategory c, DungeonTileStampData.StampPlacementRule p, DungeonTileStampData.StampSpace s)
		{
			category = c;
			placement = p;
			space = s;
		}

		public override int GetHashCode()
		{
			return 1597 * category.GetHashCode() + 5347 * placement.GetHashCode() + 13 * space.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is ViableStampCategorySet)
			{
				ViableStampCategorySet viableStampCategorySet = obj as ViableStampCategorySet;
				return viableStampCategorySet.category == category && viableStampCategorySet.space == space && viableStampCategorySet.placement == placement;
			}
			return false;
		}
	}

	public enum DecorateErrorCode
	{
		ALL_OK,
		FAILED_SPACE,
		FAILED_CHANCE
	}

	public struct WallExpanse
	{
		public IntVector2 basePosition;

		public int width;

		public bool hasMirror;

		public IntVector2 mirroredExpanseBasePosition;

		public int mirroredExpanseWidth;

		public WallExpanse(IntVector2 bp, int w)
		{
			basePosition = bp;
			width = w;
			hasMirror = false;
			mirroredExpanseBasePosition = IntVector2.Zero;
			mirroredExpanseWidth = 0;
		}

		public IntVector2 GetPositionInMirroredExpanse(int basePlacement, int stampWidth)
		{
			IntVector2 intVector = mirroredExpanseBasePosition + mirroredExpanseWidth * IntVector2.Right;
			return intVector + (basePlacement + stampWidth) * IntVector2.Left;
		}
	}

	private TK2DDungeonAssembler m_assembler;

	private Dictionary<DungeonTileStampData.StampPlacementRule, IntVector2> wallPlacementOffsets;

	private List<ViableStampCategorySet> viableCategorySets;

	private List<DungeonTileStampData.StampPlacementRule> validNorthernPlacements;

	private List<DungeonTileStampData.StampPlacementRule> validEasternPlacements;

	private List<DungeonTileStampData.StampPlacementRule> validWesternPlacements;

	private List<DungeonTileStampData.StampPlacementRule> validSouthernPlacements;

	private bool DEBUG_DRAW;

	private List<StampDataBase> roomUsedStamps = new List<StampDataBase>();

	private List<StampDataBase> expanseUsedStamps = new List<StampDataBase>();

	public TK2DInteriorDecorator(TK2DDungeonAssembler assembler)
	{
		m_assembler = assembler;
	}

	private void DecorateRoomExit(RoomHandler r, RuntimeRoomExitData usedExit, Dungeon d, tk2dTileMap map)
	{
		RoomHandler roomHandler = r.connectedRoomsByExit[usedExit.referencedExit];
		if (usedExit.referencedExit.exitDirection != 0)
		{
			return;
		}
		IntVector2 intVector = r.area.basePosition + usedExit.ExitOrigin - IntVector2.One;
		int i;
		for (i = 0; d.data.isFaceWallLower(intVector.x - i - 1, intVector.y); i++)
		{
		}
		int j;
		for (j = 0; d.data.isFaceWallLower(intVector.x + usedExit.referencedExit.ExitCellCount + j, intVector.y); j++)
		{
		}
		int num = Math.Min(i, j);
		int num2 = 0;
		if (num <= 0)
		{
			return;
		}
		for (int k = 0; k < 3; k++)
		{
			StampDataBase stampDataBase = null;
			IntVector2 intVector2 = IntVector2.Zero;
			if (k == 0 || k == 2)
			{
				stampDataBase = d.stampData.GetStampDataComplex(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL, DungeonTileStampData.StampSpace.BOTH_SPACES, DungeonTileStampData.StampCategory.STRUCTURAL, roomHandler.opulence, r.RoomVisualSubtype, num);
			}
			else
			{
				stampDataBase = d.stampData.GetStampDataComplex(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL, DungeonTileStampData.StampSpace.OBJECT_SPACE, DungeonTileStampData.StampCategory.MUNDANE, roomHandler.opulence, r.RoomVisualSubtype, num);
				intVector2 = IntVector2.Up;
			}
			IntVector2 intVector3 = intVector + IntVector2.Down + IntVector2.Left * (stampDataBase.width + num2) + intVector2;
			IntVector2 intVector4 = intVector + IntVector2.Down + IntVector2.Right * (usedExit.referencedExit.ExitCellCount + num2) + intVector2;
			if (stampDataBase is TileStampData)
			{
				m_assembler.ApplyTileStamp(intVector3.x, intVector3.y, stampDataBase as TileStampData, d, map);
				m_assembler.ApplyTileStamp(intVector4.x, intVector4.y, stampDataBase as TileStampData, d, map);
			}
			else if (stampDataBase is SpriteStampData)
			{
				m_assembler.ApplySpriteStamp(intVector3.x, intVector3.y, stampDataBase as SpriteStampData, d, map);
				m_assembler.ApplySpriteStamp(intVector4.x, intVector4.y, stampDataBase as SpriteStampData, d, map);
			}
			else if (stampDataBase is ObjectStampData)
			{
				Debug.Log("object instantiate");
				TK2DDungeonAssembler.ApplyObjectStamp(intVector3.x, intVector3.y, stampDataBase as ObjectStampData, d, map);
				TK2DDungeonAssembler.ApplyObjectStamp(intVector4.x, intVector4.y, stampDataBase as ObjectStampData, d, map);
			}
			num -= stampDataBase.width;
			num2 += stampDataBase.width;
			if (num <= 0)
			{
				break;
			}
		}
	}

	public static void PlaceLightDecorationForCell(Dungeon d, tk2dTileMap map, CellData currentCell, IntVector2 currentPosition)
	{
		if (!currentCell.cellVisualData.containsLight || currentCell.cellVisualData.lightDirection == DungeonData.Direction.SOUTH || currentCell.cellVisualData.lightDirection == (DungeonData.Direction)(-1))
		{
			return;
		}
		DungeonTileStampData.StampPlacementRule stampPlacementRule = DungeonTileStampData.StampPlacementRule.ON_LOWER_FACEWALL;
		bool flipX = false;
		if (currentCell.cellVisualData.lightDirection == DungeonData.Direction.EAST)
		{
			stampPlacementRule = DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS;
			flipX = true;
		}
		else if (currentCell.cellVisualData.lightDirection == DungeonData.Direction.WEST)
		{
			stampPlacementRule = DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS;
		}
		LightStampData lightStampData = ((stampPlacementRule != DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS) ? currentCell.cellVisualData.facewallLightStampData : currentCell.cellVisualData.sidewallLightStampData);
		if (lightStampData == null)
		{
			return;
		}
		GameObject gameObject = TK2DDungeonAssembler.ApplyObjectStamp(currentPosition.x, currentPosition.y, lightStampData, d, map, flipX, true);
		if ((bool)gameObject)
		{
			TorchController component = gameObject.GetComponent<TorchController>();
			if ((bool)component)
			{
				component.Cell = currentCell;
			}
		}
		else
		{
			if (!(currentCell.cellVisualData.lightObject != null))
			{
				return;
			}
			ShadowSystem componentInChildren = currentCell.cellVisualData.lightObject.GetComponentInChildren<ShadowSystem>();
			if ((bool)componentInChildren)
			{
				int num;
				for (num = 0; num < componentInChildren.PersonalCookies.Count; num++)
				{
					componentInChildren.PersonalCookies[num].enabled = false;
					componentInChildren.PersonalCookies.RemoveAt(num);
					num--;
				}
			}
		}
	}

	public void PlaceLightDecoration(Dungeon d, tk2dTileMap map)
	{
		for (int i = 0; i < d.data.Width; i++)
		{
			for (int j = 1; j < d.data.Height; j++)
			{
				IntVector2 intVector = new IntVector2(i, j);
				CellData cellData = d.data[intVector];
				if (cellData != null)
				{
					PlaceLightDecorationForCell(d, map, cellData, intVector);
				}
			}
		}
	}

	protected bool IsValidPondCell(CellData cell, RoomHandler parentRoom, Dungeon d)
	{
		if (cell == null)
		{
			return false;
		}
		if (parentRoom.ContainsPosition(cell.position) && cell.type == CellType.FLOOR && !cell.doesDamage && !cell.HasNonTopWallWallNeighbor() && !cell.HasPitNeighbor(d.data) && !cell.isOccupied && !cell.cellVisualData.floorTileOverridden && cell.cellVisualData.roomVisualTypeIndex == parentRoom.RoomVisualSubtype)
		{
			return true;
		}
		return false;
	}

	protected bool IsValidChannelCell(CellData cell, RoomHandler parentRoom, Dungeon d)
	{
		if (cell == null)
		{
			return false;
		}
		if (parentRoom.ContainsPosition(cell.position) && cell.type == CellType.FLOOR && !cell.doesDamage && !cell.HasPitNeighbor(d.data) && !cell.isOccupied && !cell.cellVisualData.floorTileOverridden && cell.cellVisualData.roomVisualTypeIndex == parentRoom.RoomVisualSubtype)
		{
			return true;
		}
		return false;
	}

	public void DigChannels(RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		if (!d.roomMaterialDefinitions[r.RoomVisualSubtype].supportsChannels || d.roomMaterialDefinitions[r.RoomVisualSubtype].channelGrids.Length == 0)
		{
			return;
		}
		DungeonMaterial dungeonMaterial = d.roomMaterialDefinitions[r.RoomVisualSubtype];
		TileIndexGrid tileIndexGrid = dungeonMaterial.channelGrids[UnityEngine.Random.Range(0, d.roomMaterialDefinitions[r.RoomVisualSubtype].channelGrids.Length)];
		if (tileIndexGrid == null)
		{
			return;
		}
		int num = UnityEngine.Random.Range(dungeonMaterial.minChannelPools, dungeonMaterial.maxChannelPools);
		List<IntVector2> list = new List<IntVector2>();
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		for (int i = 0; i < num; i++)
		{
			HashSet<IntVector2> hashSet2 = new HashSet<IntVector2>();
			int num2 = UnityEngine.Random.Range(2, 5);
			int num3 = UnityEngine.Random.Range(2, 5);
			int num4 = UnityEngine.Random.Range(0, r.area.dimensions.x - num2);
			int num5 = UnityEngine.Random.Range(0, r.area.dimensions.y - num3);
			IntVector2 item = r.area.basePosition + new IntVector2(num4 + num2 / 2, num5 + num3 / 2);
			bool flag = false;
			if (num4 >= 0 && num5 >= 0)
			{
				for (int j = num4; j < num4 + num2; j++)
				{
					int num6 = num5;
					while (num6 < num5 + num3)
					{
						IntVector2 intVector = r.area.basePosition + new IntVector2(j, num6);
						CellData cell = d.data[intVector];
						if (IsValidPondCell(cell, r, d))
						{
							hashSet2.Add(intVector);
							num6++;
							continue;
						}
						goto IL_0177;
					}
					continue;
					IL_0177:
					flag = true;
					break;
				}
			}
			if (!flag && hashSet2.Count > 5)
			{
				list.Add(item);
				foreach (IntVector2 item2 in hashSet2)
				{
					hashSet.Add(item2);
				}
			}
			else if (UnityEngine.Random.value < dungeonMaterial.channelTenacity)
			{
				i--;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			int num7 = UnityEngine.Random.Range(1, 4);
			for (int l = 0; l < num7; l++)
			{
				HashSet<IntVector2> hashSet3 = new HashSet<IntVector2>();
				IntVector2 intVector2 = list[k];
				IntVector2 intVector3 = intVector2;
				bool flag2;
				do
				{
					int num8 = UnityEngine.Random.Range(3, 8);
					List<IntVector2> list2 = new List<IntVector2>(IntVector2.Cardinals);
					IntVector2 intVector4 = list2[UnityEngine.Random.Range(0, 4)];
					list2.Remove(intVector4);
					list2.Remove(intVector4 * -1);
					flag2 = false;
					for (int m = 0; m < num8; m++)
					{
						IntVector2 intVector5 = intVector3 + intVector4;
						CellData cellData = d.data[intVector5];
						if (cellData == null || cellData.type == CellType.WALL)
						{
							flag2 = true;
							break;
						}
						if (IsValidChannelCell(cellData, r, d) && !hashSet3.Contains(intVector5))
						{
							if (list2.Count < 3)
							{
								list2 = new List<IntVector2>(IntVector2.Cardinals);
								list2.Remove(intVector4);
								list2.Remove(intVector4 * -1);
							}
							intVector3 = intVector5;
							hashSet.Add(intVector5);
							hashSet3.Add(intVector5);
						}
						else
						{
							if (list2.Count <= 1)
							{
								flag2 = true;
								break;
							}
							intVector4 = list2[UnityEngine.Random.Range(0, list2.Count)];
							list2.Remove(intVector4);
							list2.Remove(intVector4 * -1);
						}
					}
				}
				while (!flag2);
			}
		}
		IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
		foreach (IntVector2 item3 in hashSet)
		{
			bool[] array = new bool[8];
			int num9 = 0;
			for (int n = 0; n < array.Length; n++)
			{
				array[n] = !hashSet.Contains(item3 + cardinalsAndOrdinals[n]);
				if (array[n])
				{
					num9++;
				}
			}
			if (num9 == 1)
			{
				for (int num10 = 0; num10 < array.Length; num10 += 2)
				{
					if (d.data[item3 + cardinalsAndOrdinals[num10]].type == CellType.WALL)
					{
						array[num10] = true;
						num9++;
					}
				}
			}
			int indexGivenSides = tileIndexGrid.GetIndexGivenSides(array[0], array[1], array[2], array[3], array[4], array[5], array[6], array[7]);
			map.SetTile(item3.x, item3.y, GlobalDungeonData.patternLayerIndex, indexGivenSides);
			d.data[item3].cellVisualData.floorType = CellVisualData.CellFloorType.Water;
			d.data[item3].cellVisualData.IsChannel = true;
		}
	}

	public void ProcessHardcodedUpholstery(RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		DungeonMaterial dungeonMaterial = d.roomMaterialDefinitions[r.RoomVisualSubtype];
		if (dungeonMaterial.carpetGrids.Length == 0)
		{
			return;
		}
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		TileIndexGrid carpetGrid = dungeonMaterial.carpetGrids[UnityEngine.Random.Range(0, dungeonMaterial.carpetGrids.Length)];
		for (int i = 0; i < r.area.dimensions.x; i++)
		{
			for (int j = 0; j < r.area.dimensions.y; j++)
			{
				IntVector2 intVector = r.area.basePosition + new IntVector2(i, j);
				CellData cellData = d.data[intVector];
				if (cellData != null && cellData.type == CellType.FLOOR && cellData.parentRoom == r && cellData.cellVisualData.IsPhantomCarpet && !cellData.HasPitNeighbor(d.data))
				{
					hashSet.Add(intVector);
					BraveUtility.DrawDebugSquare(cellData.position.ToVector2(), cellData.position.ToVector2() + Vector2.one, Color.yellow, 1000f);
				}
			}
		}
		ApplyCarpetedHashset(hashSet, carpetGrid, d, map);
	}

	public void UpholsterRoom(RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		DungeonMaterial dungeonMaterial = d.roomMaterialDefinitions[r.RoomVisualSubtype];
		if (!dungeonMaterial.supportsUpholstery || dungeonMaterial.carpetGrids.Length == 0)
		{
			return;
		}
		TileIndexGrid tileIndexGrid = d.roomMaterialDefinitions[r.RoomVisualSubtype].carpetGrids[UnityEngine.Random.Range(0, d.roomMaterialDefinitions[r.RoomVisualSubtype].carpetGrids.Length)];
		if (tileIndexGrid == null)
		{
			return;
		}
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		if (dungeonMaterial.carpetIsMainFloor)
		{
			for (int i = 0; i < r.area.dimensions.x; i++)
			{
				for (int j = 0; j < r.area.dimensions.y; j++)
				{
					IntVector2 intVector = r.area.basePosition + new IntVector2(i, j);
					CellData cellData = d.data[intVector];
					if (cellData != null && cellData.type == CellType.FLOOR && cellData.parentRoom == r && !cellData.doesDamage && !cellData.cellVisualData.floorTileOverridden && cellData.cellVisualData.roomVisualTypeIndex == r.RoomVisualSubtype)
					{
						bool flag = cellData.HasWallNeighbor(true, false) || cellData.HasPitNeighbor(d.data);
						bool flag2 = cellData.HasPhantomCarpetNeighbor();
						if (!flag && !flag2)
						{
							hashSet.Add(intVector);
						}
					}
				}
			}
			hashSet = Carpetron.PostprocessFullRoom(hashSet);
		}
		else
		{
			bool flag3 = true;
			List<Tuple<IntVector2, IntVector2>> list = new List<Tuple<IntVector2, IntVector2>>();
			Tuple<IntVector2, IntVector2> tuple = null;
			int num = 1;
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
			{
				num = 2;
			}
			while (flag3)
			{
				Tuple<IntVector2, IntVector2> tuple2 = Carpetron.MaxSubmatrix(d.data.cellData, r.area.basePosition, r.area.dimensions, false, false, false, r.RoomVisualSubtype);
				IntVector2 intVector2 = tuple2.Second + IntVector2.One - tuple2.First;
				int num2 = intVector2.x * intVector2.y;
				if (num2 < 15 || intVector2.x < 3 || intVector2.y < 3)
				{
					break;
				}
				if (tuple != null)
				{
					IntVector2 intVector3 = tuple.Second + IntVector2.One - tuple.First;
					if (intVector3 != intVector2)
					{
						num--;
						if (num <= 0)
						{
							break;
						}
					}
				}
				for (int k = tuple2.First.x; k < tuple2.Second.x + 1; k++)
				{
					for (int l = tuple2.First.y; l < tuple2.Second.y + 1; l++)
					{
						IntVector2 key = r.area.basePosition + new IntVector2(k, l);
						d.data[key].cellVisualData.floorTileOverridden = true;
					}
				}
				list.Add(tuple2);
				tuple = tuple2;
			}
			if (list.Count == 1)
			{
				Tuple<IntVector2, IntVector2> bonusRect = null;
				list[0] = Carpetron.PostprocessSubmatrix(list[0], out bonusRect);
				if (bonusRect != null)
				{
					list.Add(bonusRect);
				}
			}
			for (int m = 0; m < list.Count; m++)
			{
				Tuple<IntVector2, IntVector2> tuple3 = list[m];
				for (int n = tuple3.First.x; n < tuple3.Second.x + 1; n++)
				{
					for (int num3 = tuple3.First.y; num3 < tuple3.Second.y + 1; num3++)
					{
						IntVector2 item = r.area.basePosition + new IntVector2(n, num3);
						hashSet.Add(item);
					}
				}
			}
		}
		ApplyCarpetedHashset(hashSet, tileIndexGrid, d, map);
	}

	private void ApplyCarpetedHashset(HashSet<IntVector2> cellsToEncarpet, TileIndexGrid carpetGrid, Dungeon d, tk2dTileMap map)
	{
		IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
		Dictionary<IntVector2, int> dictionary = new Dictionary<IntVector2, int>(new IntVector2EqualityComparer());
		if (carpetGrid.CenterIndicesAreStrata)
		{
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			HashSet<IntVector2> hashSet2 = new HashSet<IntVector2>();
			HashSet<IntVector2> hashSet3 = new HashSet<IntVector2>();
			foreach (IntVector2 item in cellsToEncarpet)
			{
				bool[] array = new bool[8];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = !cellsToEncarpet.Contains(item + cardinalsAndOrdinals[i]);
				}
				if (array[0] || array[1] || array[2] || array[3] || array[4] || array[5] || array[6] || array[7])
				{
					hashSet2.Add(item);
				}
			}
			int num = 0;
			while (hashSet2.Count > 0)
			{
				foreach (IntVector2 item2 in hashSet2)
				{
					hashSet.Add(item2);
					for (int j = 0; j < 8; j++)
					{
						IntVector2 intVector = item2 + cardinalsAndOrdinals[j];
						if (!hashSet.Contains(intVector) && !hashSet2.Contains(intVector) && !hashSet3.Contains(intVector) && cellsToEncarpet.Contains(intVector))
						{
							hashSet3.Add(intVector);
							dictionary.Add(intVector, carpetGrid.centerIndices.indices[Mathf.Clamp(num, 0, carpetGrid.centerIndices.indices.Count - 1)]);
						}
					}
				}
				hashSet2 = hashSet3;
				hashSet3 = new HashSet<IntVector2>();
				num++;
			}
			if (num < 3)
			{
				dictionary.Clear();
			}
		}
		foreach (IntVector2 item3 in cellsToEncarpet)
		{
			bool[] array2 = new bool[8];
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k] = !cellsToEncarpet.Contains(item3 + cardinalsAndOrdinals[k]);
			}
			bool flag = !array2[0] && !array2[1] && !array2[2] && !array2[3] && !array2[4] && !array2[5] && !array2[6] && !array2[7];
			int num2 = -1;
			map.SetTile(tile: (!dictionary.ContainsKey(item3)) ? ((!flag || !carpetGrid.CenterIndicesAreStrata) ? carpetGrid.GetIndexGivenSides(array2[0], array2[1], array2[2], array2[3], array2[4], array2[5], array2[6], array2[7]) : carpetGrid.centerIndices.indices[0]) : dictionary[item3], x: item3.x, y: item3.y, layer: GlobalDungeonData.patternLayerIndex);
			d.data[item3].cellVisualData.floorType = CellVisualData.CellFloorType.Carpet;
			d.data[item3].cellVisualData.floorTileOverridden = true;
		}
	}

	public void HandleRoomDecorationMinimal(RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		roomUsedStamps.Clear();
		if (!(r.area.prototypeRoom == null))
		{
			if (viableCategorySets == null)
			{
				BuildStampLookupTable(d);
				BuildValidPlacementLists();
			}
			ProcessHardcodedUpholstery(r, d, map);
			roomUsedStamps.Clear();
		}
	}

	public void HandleRoomDecoration(RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		roomUsedStamps.Clear();
		ProcessHardcodedUpholstery(r, d, map);
		if (r.area.prototypeRoom == null || !r.area.prototypeRoom.preventAddedDecoLayering)
		{
			UpholsterRoom(r, d, map);
			if (!r.ForcePreventChannels)
			{
				DigChannels(r, d, map);
			}
		}
		if (viableCategorySets == null)
		{
			BuildStampLookupTable(d);
			BuildValidPlacementLists();
		}
		for (int i = 0; i < r.area.instanceUsedExits.Count; i++)
		{
			PrototypeRoomExit key = r.area.instanceUsedExits[i];
			RoomHandler roomHandler = r.connectedRoomsByExit[key];
			if (roomHandler != null && (!(roomHandler.area.prototypeRoom != null) || roomHandler.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.SECRET))
			{
				DecorateRoomExit(r, r.area.exitToLocalDataMap[key], d, map);
			}
		}
		List<WallExpanse> list = r.GatherExpanses(DungeonData.Direction.NORTH, false);
		for (int j = 0; j < list.Count; j++)
		{
			WallExpanse value = list[j];
			WallExpanse? wallExpanse = null;
			int index = -1;
			for (int k = j + 1; k < list.Count; k++)
			{
				WallExpanse value2 = list[k];
				if (value.basePosition.y != value2.basePosition.y || value.width != value2.width)
				{
					continue;
				}
				bool flag = true;
				for (int l = 0; l < value2.width; l++)
				{
					if (d.data[r.area.basePosition + value.basePosition + IntVector2.Up + IntVector2.Right * l].cellVisualData.forcedMatchingStyle != d.data[r.area.basePosition + value2.basePosition + IntVector2.Up + IntVector2.Right * l].cellVisualData.forcedMatchingStyle)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					wallExpanse = value2;
					index = k;
				}
			}
			if (wallExpanse.HasValue)
			{
				value.hasMirror = true;
				value.mirroredExpanseBasePosition = wallExpanse.Value.basePosition;
				value.mirroredExpanseWidth = wallExpanse.Value.width;
				list.RemoveAt(index);
				list[j] = value;
			}
		}
		wallPlacementOffsets = new Dictionary<DungeonTileStampData.StampPlacementRule, IntVector2>();
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL, IntVector2.Zero);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL_LEFT_CORNER, IntVector2.Zero);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL_RIGHT_CORNER, IntVector2.Zero);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.ON_LOWER_FACEWALL, IntVector2.Up);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.ON_UPPER_FACEWALL, IntVector2.Up * 2);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.ON_ANY_FLOOR, IntVector2.Zero);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.ON_ANY_CEILING, IntVector2.Zero);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS, IntVector2.Left);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.ALONG_RIGHT_WALLS, IntVector2.Zero);
		wallPlacementOffsets.Add(DungeonTileStampData.StampPlacementRule.ON_TOPWALL, IntVector2.Zero);
		for (int m = 0; m < list.Count; m++)
		{
			expanseUsedStamps.Clear();
			WallExpanse expanse = list[m];
			if (expanse.hasMirror)
			{
				DecorateExpanseRandom(expanse, r, d, map);
			}
			else if (expanse.width > 2)
			{
				float num = UnityEngine.Random.value;
				for (int n = 0; n < expanse.width; n++)
				{
					if (d.data[r.area.basePosition + expanse.basePosition + IntVector2.Up + IntVector2.Right * n].cellVisualData.forcedMatchingStyle != 0)
					{
						num = 1000f;
					}
				}
				if (num < d.stampData.SymmetricFrameChance)
				{
					DecorateExpanseSymmetricFrame(1, expanse, r, d, map);
				}
				else if (num >= d.stampData.SymmetricFrameChance && num < d.stampData.SymmetricFrameChance + d.stampData.SymmetricCompleteChance)
				{
					DecorateExpanseSymmetric(expanse, r, d, map);
				}
				else
				{
					DecorateExpanseRandom(expanse, r, d, map);
				}
			}
			else
			{
				DecorateExpanseRandom(expanse, r, d, map);
			}
		}
		DecorateCeilingCorners(r, d, map);
		List<WallExpanse> list2 = r.GatherExpanses(DungeonData.Direction.EAST, false);
		List<WallExpanse> list3 = r.GatherExpanses(DungeonData.Direction.WEST, false);
		for (int num2 = 0; num2 < list2.Count; num2++)
		{
			WallExpanse value3 = list2[num2];
			if (value3.width > 1)
			{
				value3.width--;
				list2[num2] = value3;
			}
			else
			{
				list2.RemoveAt(num2);
				num2--;
			}
		}
		for (int num3 = 0; num3 < list3.Count; num3++)
		{
			WallExpanse value4 = list3[num3];
			if (value4.width > 1)
			{
				value4.width--;
				list3[num3] = value4;
			}
			else
			{
				list3.RemoveAt(num3);
				num3--;
			}
		}
		for (int num4 = 0; num4 < list2.Count; num4++)
		{
			expanseUsedStamps.Clear();
			WallExpanse expanse2 = list2[num4];
			WallExpanse? wallExpanse2 = null;
			for (int num5 = 0; num5 < list3.Count; num5++)
			{
				WallExpanse value5 = list3[num5];
				if (value5.basePosition.y == expanse2.basePosition.y && value5.width == expanse2.width)
				{
					wallExpanse2 = value5;
					list3.RemoveAt(num5);
					break;
				}
			}
			int num6 = 1;
			while (true)
			{
				int num7 = expanse2.width - num6;
				if (num7 == 0)
				{
					break;
				}
				IntVector2 basePosition = r.area.basePosition + expanse2.basePosition + num6 * IntVector2.Up;
				StampDataBase placedStamp = null;
				DecorateErrorCode decorateErrorCode = DecorateWallSection(basePosition, num7, r, d, map, validEasternPlacements, expanse2, out placedStamp, Mathf.Max(0.55f, r.RoomMaterial.stampFailChance), true);
				if (decorateErrorCode == DecorateErrorCode.FAILED_SPACE)
				{
					break;
				}
				if (placedStamp == null || decorateErrorCode == DecorateErrorCode.FAILED_CHANCE)
				{
					num6++;
					continue;
				}
				if (wallExpanse2.HasValue)
				{
					IntVector2 basePosition2 = r.area.basePosition + wallExpanse2.Value.basePosition + (expanse2.width - num7) * IntVector2.Up;
					StampDataBase placedStamp2 = null;
					DecorateWallSection(basePosition2, num7, r, d, map, validWesternPlacements, wallExpanse2.Value, out placedStamp2, 0f, true);
				}
				num6 += placedStamp.height;
			}
		}
		for (int num8 = 0; num8 < list3.Count; num8++)
		{
			expanseUsedStamps.Clear();
			WallExpanse expanse3 = list3[num8];
			int num9 = 1;
			while (true)
			{
				int num10 = expanse3.width - num9;
				if (num10 == 0)
				{
					break;
				}
				IntVector2 basePosition3 = r.area.basePosition + expanse3.basePosition + num9 * IntVector2.Up;
				StampDataBase placedStamp3 = null;
				DecorateErrorCode decorateErrorCode2 = DecorateWallSection(basePosition3, num10, r, d, map, validWesternPlacements, expanse3, out placedStamp3, Mathf.Max(0.55f, r.RoomMaterial.stampFailChance), true);
				if (decorateErrorCode2 == DecorateErrorCode.FAILED_SPACE)
				{
					break;
				}
				num9 = ((placedStamp3 != null && decorateErrorCode2 != DecorateErrorCode.FAILED_CHANCE) ? (num9 + placedStamp3.height) : (num9 + 1));
			}
		}
		List<WallExpanse> list4 = r.GatherExpanses(DungeonData.Direction.SOUTH);
		for (int num11 = 0; num11 < list4.Count; num11++)
		{
			expanseUsedStamps.Clear();
			WallExpanse expanse4 = list4[num11];
			int num12 = 1;
			while (true)
			{
				int num13 = Mathf.FloorToInt((float)(expanse4.width - 2 * num12) / 2f);
				if (num13 == 0)
				{
					break;
				}
				IntVector2 basePosition4 = r.area.basePosition + expanse4.basePosition + num12 * IntVector2.Right;
				StampDataBase placedStamp4 = null;
				DecorateErrorCode decorateErrorCode3 = DecorateWallSection(basePosition4, num13, r, d, map, validSouthernPlacements, expanse4, out placedStamp4, 0.5f);
				if (decorateErrorCode3 == DecorateErrorCode.FAILED_SPACE)
				{
					break;
				}
				if (placedStamp4 == null || decorateErrorCode3 == DecorateErrorCode.FAILED_CHANCE)
				{
					num12++;
					continue;
				}
				IntVector2 intVector = r.area.basePosition + expanse4.basePosition + (expanse4.width - num12 - placedStamp4.width) * IntVector2.Right + wallPlacementOffsets[placedStamp4.placementRule];
				m_assembler.ApplyStampGeneric(intVector.x, intVector.y, placedStamp4, d, map, false, GlobalDungeonData.aboveBorderLayerIndex);
				num12 += placedStamp4.width;
				if (placedStamp4.width == 1)
				{
					num12 += 2;
				}
			}
		}
		for (int num14 = 2; num14 < r.area.dimensions.x - 2; num14++)
		{
			for (int num15 = 2; num15 < r.area.dimensions.y - 2; num15++)
			{
				IntVector2 basePosition5 = r.area.basePosition + new IntVector2(num14, num15);
				CellData cellData = d.data.cellData[basePosition5.x][basePosition5.y];
				if (cellData != null && cellData.type == CellType.FLOOR && !cellData.cellVisualData.floorTileOverridden && !cellData.cellVisualData.preventFloorStamping)
				{
					StampDataBase placedStamp5 = null;
					float failChance = 0.8f;
					if (d.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON)
					{
						failChance = 0.99f;
					}
					DecorateFloorSquare(basePosition5, r, d, map, out placedStamp5, failChance);
				}
			}
		}
		roomUsedStamps.Clear();
	}

	private void DecorateCeilingCorners(RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		if (d.tileIndices.edgeDecorationTiles == null || r == d.data.Entrance || r == d.data.Exit)
		{
			return;
		}
		CellArea area = r.area;
		for (int i = 0; i < area.dimensions.x; i++)
		{
			for (int j = 0; j < area.dimensions.y; j++)
			{
				IntVector2 intVector = area.basePosition + new IntVector2(i, j);
				CellData cellData = d.data.cellData[intVector.x][intVector.y];
				if (cellData != null && cellData.type != CellType.WALL && cellData.diagonalWallType == DiagonalWallType.NONE)
				{
					List<CellData> cellNeighbors = d.data.GetCellNeighbors(cellData);
					bool flag = cellNeighbors[0] != null && cellNeighbors[0].type == CellType.WALL && cellNeighbors[0].diagonalWallType == DiagonalWallType.NONE;
					bool isEastBorder = cellNeighbors[1] != null && cellNeighbors[1].type == CellType.WALL && cellNeighbors[1].diagonalWallType == DiagonalWallType.NONE;
					bool isSouthBorder = cellNeighbors[2] != null && cellNeighbors[2].type == CellType.WALL && cellNeighbors[2].diagonalWallType == DiagonalWallType.NONE;
					bool isWestBorder = cellNeighbors[3] != null && cellNeighbors[3].type == CellType.WALL && cellNeighbors[3].diagonalWallType == DiagonalWallType.NONE;
					int indexGivenSides = d.tileIndices.edgeDecorationTiles.GetIndexGivenSides(flag, isEastBorder, isSouthBorder, isWestBorder);
					bool flag2 = UnityEngine.Random.value < 0.25f;
					if (indexGivenSides != -1 && flag2)
					{
						int num = ((!flag) ? 1 : 2);
						map.SetTile(intVector.x, intVector.y + num, GlobalDungeonData.aboveBorderLayerIndex, indexGivenSides);
					}
				}
			}
		}
	}

	private void DecorateExpanseSymmetricFrame(int frameIterations, WallExpanse expanse, RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		int num = 0;
		for (int i = 0; i < frameIterations; i++)
		{
			int num2 = Mathf.FloorToInt((float)(expanse.width - 2 * num) / 2f);
			if (num2 == 0)
			{
				break;
			}
			IntVector2 intVector = r.area.basePosition + expanse.basePosition + num * IntVector2.Right;
			StampDataBase placedStamp = null;
			DecorateErrorCode decorateErrorCode = DecorateWallSection(intVector, num2, r, d, map, validNorthernPlacements, expanse, out placedStamp, r.RoomMaterial.stampFailChance);
			if (decorateErrorCode == DecorateErrorCode.FAILED_SPACE)
			{
				break;
			}
			if (placedStamp == null || decorateErrorCode == DecorateErrorCode.FAILED_CHANCE)
			{
				num++;
				continue;
			}
			if (placedStamp.indexOfSymmetricPartner != -1)
			{
				placedStamp = d.stampData.objectStamps[placedStamp.indexOfSymmetricPartner];
			}
			IntVector2 intVector2 = r.area.basePosition + expanse.basePosition + (expanse.width - num - placedStamp.width) * IntVector2.Right + wallPlacementOffsets[placedStamp.placementRule];
			if (!placedStamp.preventRoomRepeats)
			{
				m_assembler.ApplyStampGeneric(intVector2.x, intVector2.y, placedStamp, d, map);
			}
			else
			{
				StampDataBase stampDataBase = d.stampData.AttemptGetSimilarStampForRoomDuplication(placedStamp, roomUsedStamps, r.RoomVisualSubtype);
				if (stampDataBase != null)
				{
					m_assembler.ApplyStampGeneric(intVector2.x, intVector2.y, stampDataBase, d, map);
					roomUsedStamps.Add(stampDataBase);
				}
			}
			if (DEBUG_DRAW)
			{
				BraveUtility.DrawDebugSquare(intVector.ToVector2(), (intVector + IntVector2.Up + placedStamp.width * IntVector2.Right).ToVector2(), Color.red, 1000f);
				BraveUtility.DrawDebugSquare(intVector2.ToVector2(), (intVector2 + IntVector2.Up + placedStamp.width * IntVector2.Right).ToVector2(), Color.red, 1000f);
			}
			num += placedStamp.width;
		}
		int num3 = expanse.width - 2 * num;
		if (num3 > 0)
		{
			WallExpanse expanse2 = new WallExpanse(expanse.basePosition + num * IntVector2.Right, num3);
			DecorateExpanseRandom(expanse2, r, d, map);
		}
	}

	private void DecorateExpanseSymmetric(WallExpanse expanse, RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		int num = 0;
		while (true)
		{
			int num2 = Mathf.FloorToInt((float)(expanse.width - 2 * num) / 2f);
			if (num2 == 0)
			{
				break;
			}
			IntVector2 intVector = r.area.basePosition + expanse.basePosition + num * IntVector2.Right;
			StampDataBase placedStamp = null;
			DecorateErrorCode decorateErrorCode = DecorateWallSection(intVector, num2, r, d, map, validNorthernPlacements, expanse, out placedStamp, r.RoomMaterial.stampFailChance);
			if (decorateErrorCode == DecorateErrorCode.FAILED_SPACE)
			{
				break;
			}
			if (placedStamp == null || decorateErrorCode == DecorateErrorCode.FAILED_CHANCE)
			{
				num++;
				continue;
			}
			if (placedStamp.indexOfSymmetricPartner != -1)
			{
				placedStamp = d.stampData.objectStamps[placedStamp.indexOfSymmetricPartner];
			}
			IntVector2 intVector2 = r.area.basePosition + expanse.basePosition + (expanse.width - num - placedStamp.width) * IntVector2.Right + wallPlacementOffsets[placedStamp.placementRule];
			if (!placedStamp.preventRoomRepeats)
			{
				m_assembler.ApplyStampGeneric(intVector2.x, intVector2.y, placedStamp, d, map);
			}
			else
			{
				StampDataBase stampDataBase = d.stampData.AttemptGetSimilarStampForRoomDuplication(placedStamp, roomUsedStamps, r.RoomVisualSubtype);
				if (stampDataBase != null)
				{
					m_assembler.ApplyStampGeneric(intVector2.x, intVector2.y, stampDataBase, d, map);
					roomUsedStamps.Add(stampDataBase);
				}
			}
			if (DEBUG_DRAW)
			{
				BraveUtility.DrawDebugSquare(intVector.ToVector2(), (intVector + IntVector2.Up + placedStamp.width * IntVector2.Right).ToVector2(), Color.yellow, 1000f);
				BraveUtility.DrawDebugSquare(intVector2.ToVector2(), (intVector2 + IntVector2.Up + placedStamp.width * IntVector2.Right).ToVector2(), Color.yellow, 1000f);
			}
			num += placedStamp.width;
		}
	}

	private void DecorateExpanseRandom(WallExpanse expanse, RoomHandler r, Dungeon d, tk2dTileMap map)
	{
		int num = 0;
		while (true)
		{
			int num2 = expanse.width - num;
			if (num2 == 0)
			{
				break;
			}
			IntVector2 intVector = r.area.basePosition + expanse.basePosition + num * IntVector2.Right;
			StampDataBase placedStamp = null;
			DecorateErrorCode decorateErrorCode = DecorateWallSection(intVector, num2, r, d, map, validNorthernPlacements, expanse, out placedStamp, r.RoomMaterial.stampFailChance);
			if (decorateErrorCode == DecorateErrorCode.FAILED_SPACE)
			{
				break;
			}
			if (placedStamp == null || decorateErrorCode == DecorateErrorCode.FAILED_CHANCE)
			{
				num++;
				continue;
			}
			if (expanse.hasMirror)
			{
				IntVector2 intVector2 = r.area.basePosition + expanse.GetPositionInMirroredExpanse(num, placedStamp.width);
				Debug.DrawLine(intVector2.ToVector3(), intVector2.ToVector3() + new Vector3(1f, 1f, 0f), Color.cyan, 1000f);
				if (placedStamp.indexOfSymmetricPartner != -1)
				{
					placedStamp = d.stampData.objectStamps[placedStamp.indexOfSymmetricPartner];
				}
				IntVector2 intVector3 = intVector2 + wallPlacementOffsets[placedStamp.placementRule];
				if (!placedStamp.preventRoomRepeats)
				{
					m_assembler.ApplyStampGeneric(intVector3.x, intVector3.y, placedStamp, d, map);
				}
				else
				{
					StampDataBase stampDataBase = d.stampData.AttemptGetSimilarStampForRoomDuplication(placedStamp, roomUsedStamps, r.RoomVisualSubtype);
					if (stampDataBase != null)
					{
						m_assembler.ApplyStampGeneric(intVector3.x, intVector3.y, stampDataBase, d, map);
						roomUsedStamps.Add(stampDataBase);
					}
				}
			}
			if (DEBUG_DRAW)
			{
				BraveUtility.DrawDebugSquare(intVector.ToVector2(), (intVector + IntVector2.Up + placedStamp.width * IntVector2.Right).ToVector2(), Color.magenta, 1000f);
			}
			num += placedStamp.width;
		}
	}

	private DungeonTileStampData.StampSpace GetValidSpaceForSection(IntVector2 basePosition, int viableWidth, Dungeon d)
	{
		List<DungeonTileStampData.StampSpace> list = new List<DungeonTileStampData.StampSpace>();
		list.Add(DungeonTileStampData.StampSpace.OBJECT_SPACE);
		bool flag = true;
		for (int i = 0; i < viableWidth; i++)
		{
			IntVector2 intVector = basePosition + IntVector2.Up + IntVector2.Right * i;
			CellVisualData cellVisualData = d.data.cellData[intVector.x][intVector.y].cellVisualData;
			if (cellVisualData.containsWallSpaceStamp)
			{
				flag = false;
				break;
			}
			intVector += IntVector2.Up;
			cellVisualData = d.data.cellData[intVector.x][intVector.y].cellVisualData;
			if (cellVisualData.containsWallSpaceStamp)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			list.Add(DungeonTileStampData.StampSpace.WALL_SPACE);
			list.Add(DungeonTileStampData.StampSpace.BOTH_SPACES);
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private void BuildValidPlacementLists()
	{
		validNorthernPlacements = new List<DungeonTileStampData.StampPlacementRule>();
		validNorthernPlacements.Add(DungeonTileStampData.StampPlacementRule.ABOVE_UPPER_FACEWALL);
		validNorthernPlacements.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL);
		validNorthernPlacements.Add(DungeonTileStampData.StampPlacementRule.ON_LOWER_FACEWALL);
		validNorthernPlacements.Add(DungeonTileStampData.StampPlacementRule.ON_UPPER_FACEWALL);
		validEasternPlacements = new List<DungeonTileStampData.StampPlacementRule>();
		validEasternPlacements.Add(DungeonTileStampData.StampPlacementRule.ALONG_RIGHT_WALLS);
		validEasternPlacements.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL);
		validWesternPlacements = new List<DungeonTileStampData.StampPlacementRule>();
		validWesternPlacements.Add(DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS);
		validWesternPlacements.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL);
		validSouthernPlacements = new List<DungeonTileStampData.StampPlacementRule>();
		validSouthernPlacements.Add(DungeonTileStampData.StampPlacementRule.ON_TOPWALL);
	}

	private void BuildStampLookupTable(Dungeon d)
	{
		viableCategorySets = new List<ViableStampCategorySet>();
		for (int i = 0; i < d.stampData.stamps.Length; i++)
		{
			StampDataBase stampDataBase = d.stampData.stamps[i];
			ViableStampCategorySet item = new ViableStampCategorySet(stampDataBase.stampCategory, stampDataBase.placementRule, stampDataBase.occupySpace);
			if (!viableCategorySets.Contains(item))
			{
				viableCategorySets.Add(item);
			}
		}
		for (int j = 0; j < d.stampData.spriteStamps.Length; j++)
		{
			StampDataBase stampDataBase2 = d.stampData.spriteStamps[j];
			ViableStampCategorySet item2 = new ViableStampCategorySet(stampDataBase2.stampCategory, stampDataBase2.placementRule, stampDataBase2.occupySpace);
			if (!viableCategorySets.Contains(item2))
			{
				viableCategorySets.Add(item2);
			}
		}
		for (int k = 0; k < d.stampData.objectStamps.Length; k++)
		{
			StampDataBase stampDataBase3 = d.stampData.objectStamps[k];
			ViableStampCategorySet item3 = new ViableStampCategorySet(stampDataBase3.stampCategory, stampDataBase3.placementRule, stampDataBase3.occupySpace);
			if (!viableCategorySets.Contains(item3))
			{
				viableCategorySets.Add(item3);
			}
		}
	}

	private ViableStampCategorySet GetCategorySet(List<DungeonTileStampData.StampPlacementRule> validRules)
	{
		List<ViableStampCategorySet> list = new List<ViableStampCategorySet>();
		for (int i = 0; i < viableCategorySets.Count; i++)
		{
			if (validRules.Contains(viableCategorySets[i].placement))
			{
				list.Add(viableCategorySets[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	private bool CheckExpanseStampValidity(WallExpanse expanse, StampDataBase stamp)
	{
		if (stamp.preventRoomRepeats && roomUsedStamps.Contains(stamp))
		{
			return false;
		}
		int preferredIntermediaryStamps = stamp.preferredIntermediaryStamps;
		for (int i = 0; i < preferredIntermediaryStamps; i++)
		{
			int num = expanseUsedStamps.Count - (1 + i);
			if (num < 0)
			{
				break;
			}
			if (stamp.intermediaryMatchingStyle == DungeonTileStampData.IntermediaryMatchingStyle.ANY)
			{
				if (expanseUsedStamps[num] == stamp)
				{
					return false;
				}
			}
			else if (expanseUsedStamps[num].intermediaryMatchingStyle == stamp.intermediaryMatchingStyle)
			{
				return false;
			}
		}
		return true;
	}

	private bool DecorateFloorSquare(IntVector2 basePosition, RoomHandler r, Dungeon d, tk2dTileMap map, out StampDataBase placedStamp, float failChance = 0.2f)
	{
		if (UnityEngine.Random.value < failChance)
		{
			placedStamp = null;
			return true;
		}
		placedStamp = null;
		StampDataBase stampDataBase = null;
		List<DungeonTileStampData.StampPlacementRule> list = new List<DungeonTileStampData.StampPlacementRule>();
		list.Add(DungeonTileStampData.StampPlacementRule.ON_ANY_FLOOR);
		ViableStampCategorySet categorySet = GetCategorySet(list);
		if (categorySet == null)
		{
			return false;
		}
		stampDataBase = d.stampData.GetStampDataComplex(list, categorySet.space, categorySet.category, r.opulence, r.RoomVisualSubtype, 1);
		if (stampDataBase == null)
		{
			return false;
		}
		IntVector2 intVector = basePosition + wallPlacementOffsets[stampDataBase.placementRule];
		m_assembler.ApplyStampGeneric(intVector.x, intVector.y, stampDataBase, d, map);
		placedStamp = stampDataBase;
		return true;
	}

	private DecorateErrorCode DecorateWallSection(IntVector2 basePosition, int viableWidth, RoomHandler r, Dungeon d, tk2dTileMap map, List<DungeonTileStampData.StampPlacementRule> validRules, WallExpanse expanse, out StampDataBase placedStamp, float failChance = 0.2f, bool excludeWallSpace = false)
	{
		if (GameManager.Options.DebrisQuantity == GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			failChance = Mathf.Min(failChance * 2f, 0.75f);
		}
		if (UnityEngine.Random.value < failChance)
		{
			placedStamp = null;
			return DecorateErrorCode.FAILED_CHANCE;
		}
		StampDataBase stampDataBase = null;
		if (validRules.Contains(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL))
		{
			if (d.data.GetCellTypeSafe(basePosition + IntVector2.Left) == CellType.WALL)
			{
				validRules.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL_LEFT_CORNER);
			}
			if (d.data.GetCellTypeSafe(basePosition + IntVector2.Right) == CellType.WALL)
			{
				validRules.Add(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL_RIGHT_CORNER);
			}
		}
		for (int i = 0; i < 10; i++)
		{
			if (!d.data.CheckInBoundsAndValid(basePosition) || !d.data.CheckInBoundsAndValid(basePosition + IntVector2.Up))
			{
				stampDataBase = null;
				break;
			}
			if (d.data[basePosition + IntVector2.Up].cellVisualData.forcedMatchingStyle == DungeonTileStampData.IntermediaryMatchingStyle.ANY)
			{
				stampDataBase = d.stampData.GetStampDataSimple(validRules, r.opulence, r.RoomVisualSubtype, viableWidth, excludeWallSpace, roomUsedStamps);
				if (stampDataBase != null && stampDataBase.requiresForcedMatchingStyle)
				{
					continue;
				}
			}
			else
			{
				BraveUtility.DrawDebugSquare((basePosition + IntVector2.Up).ToVector2() + new Vector2(0.2f, 0.2f), (basePosition + IntVector2.Up + IntVector2.One).ToVector2() + new Vector2(-0.2f, -0.2f), Color.red, 1000f);
				stampDataBase = d.stampData.GetStampDataSimpleWithForcedRule(validRules, d.data[basePosition + IntVector2.Up].cellVisualData.forcedMatchingStyle, r.opulence, r.RoomVisualSubtype, viableWidth, excludeWallSpace);
				if (stampDataBase != null && stampDataBase.intermediaryMatchingStyle != d.data[basePosition + IntVector2.Up].cellVisualData.forcedMatchingStyle)
				{
					break;
				}
			}
			if (stampDataBase == null)
			{
				break;
			}
			if (!excludeWallSpace || stampDataBase.width <= 1)
			{
				if (CheckExpanseStampValidity(expanse, stampDataBase))
				{
					break;
				}
				stampDataBase = null;
			}
		}
		validRules.Remove(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL_LEFT_CORNER);
		validRules.Remove(DungeonTileStampData.StampPlacementRule.BELOW_LOWER_FACEWALL_RIGHT_CORNER);
		if (stampDataBase == null)
		{
			placedStamp = null;
			return DecorateErrorCode.FAILED_SPACE;
		}
		expanseUsedStamps.Add(stampDataBase);
		roomUsedStamps.Add(stampDataBase);
		IntVector2 intVector = basePosition + wallPlacementOffsets[stampDataBase.placementRule];
		int overrideTileLayerIndex = ((stampDataBase.placementRule != DungeonTileStampData.StampPlacementRule.ALONG_LEFT_WALLS && stampDataBase.placementRule != DungeonTileStampData.StampPlacementRule.ALONG_RIGHT_WALLS && stampDataBase.placementRule != DungeonTileStampData.StampPlacementRule.ON_TOPWALL) ? (-1) : GlobalDungeonData.aboveBorderLayerIndex);
		m_assembler.ApplyStampGeneric(intVector.x, intVector.y, stampDataBase, d, map, false, overrideTileLayerIndex);
		placedStamp = stampDataBase;
		return DecorateErrorCode.ALL_OK;
	}
}
