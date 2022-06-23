using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Dungeonator
{
	public class SemioticDungeonGenerator : IDungeonGenerator
	{
		public bool RAPID_DEBUG_ITERATION_MODE;

		public int RAPID_DEBUG_ITERATION_INDEX;

		private SemioticDungeonGenSettings m_patternSettings;

		private int m_numberRoomCells;

		public SemioticDungeonGenerator(Dungeon d, SemioticDungeonGenSettings sdgs)
		{
			m_patternSettings = sdgs;
		}

		public DungeonData GenerateDungeonLayout()
		{
			//Discarded unreachable code: IL_0176
			PrototypeDungeonRoom prototypeDungeonRoom = GetRandomEntranceRoomFromList(m_patternSettings.flows[0].fallbackRoomTable.GetCompiledList());
			if (prototypeDungeonRoom == null)
			{
				prototypeDungeonRoom = m_patternSettings.flows[0].fallbackRoomTable.SelectByWeight().room;
			}
			SemioticLayoutManager semioticLayoutManager = null;
			DungeonFlowBuilder dungeonFlowBuilder = null;
			CellArea cellArea = null;
			RoomHandler roomHandler = null;
			bool flag = true;
			int num = 0;
			int num2 = 10;
			while (true)
			{
				BraveMemory.DoCollect();
				if (num == num2)
				{
					Debug.LogError("DUNGEON GENERATION FAILED.");
					flag = false;
					break;
				}
				num++;
				cellArea = new CellArea(IntVector2.Zero, new IntVector2(prototypeDungeonRoom.Width, prototypeDungeonRoom.Height));
				cellArea.prototypeRoom = prototypeDungeonRoom;
				cellArea.instanceUsedExits = new List<PrototypeRoomExit>();
				roomHandler = new RoomHandler(cellArea);
				roomHandler.distanceFromEntrance = 0;
				semioticLayoutManager = new SemioticLayoutManager();
				semioticLayoutManager.StampCellAreaToLayout(roomHandler);
				dungeonFlowBuilder = new DungeonFlowBuilder(m_patternSettings.flows[0], semioticLayoutManager);
				bool flag2 = dungeonFlowBuilder.Build(roomHandler);
				if (!flag2)
				{
					continue;
				}
				for (int i = 0; i < m_patternSettings.mandatoryExtraRooms.Count; i++)
				{
					flag2 = dungeonFlowBuilder.AttemptAppendExtraRoom(m_patternSettings.mandatoryExtraRooms[i]);
					if (!flag2)
					{
						break;
					}
				}
				if (!flag2)
				{
					continue;
				}
				Debug.Log("DUNGEON GENERATION SUCCEEDED ON ATTEMPT #" + num);
				break;
			}
			if (flag)
			{
				dungeonFlowBuilder.AppendCapChains();
			}
			IntVector2 minimumCellPosition = semioticLayoutManager.GetMinimumCellPosition();
			IntVector2 maximumCellPosition = semioticLayoutManager.GetMaximumCellPosition();
			IntVector2 offset = new IntVector2(-minimumCellPosition.x + 10, -minimumCellPosition.y + 10);
			IntVector2 intVector = maximumCellPosition - minimumCellPosition;
			semioticLayoutManager.HandleOffsetRooms(offset);
			dungeonFlowBuilder.DebugActionLines();
			if (RAPID_DEBUG_ITERATION_MODE)
			{
				Texture2D texture2D = new Texture2D(intVector.x + 20, intVector.y + 20);
				Color[] array = new Color[(intVector.x + 20) * (intVector.y + 20)];
				for (int j = 0; j < intVector.x + 20; j++)
				{
					for (int k = 0; k < intVector.y + 20; k++)
					{
						array[k * (intVector.x + 20) + j] = ((!flag) ? new Color(0.5f, 0f, 0f) : new Color(0f, 0.5f, 0f));
					}
				}
				texture2D.SetPixels(array);
				texture2D.Apply();
				byte[] buffer = texture2D.EncodeToPNG();
				FileStream output = File.Open(Application.dataPath + "/DungeonDebug/debug_" + RAPID_DEBUG_ITERATION_INDEX + ".png", FileMode.Create);
				using (BinaryWriter binaryWriter = new BinaryWriter(output))
				{
					binaryWriter.Write(buffer);
				}
				return null;
			}
			CellData[][] array2 = new CellData[intVector.x + 20][];
			for (int l = 0; l < array2.Length; l++)
			{
				array2[l] = new CellData[intVector.y + 20];
				for (int m = 0; m < array2[l].Length; m++)
				{
					array2[l][m] = new CellData(l, m);
				}
			}
			DungeonData dungeonData = new DungeonData(array2);
			List<RoomHandler> rooms = semioticLayoutManager.Rooms;
			dungeonData.InitializeCoreData(rooms);
			dungeonData.Entrance = GetRoomHandlerByArea(rooms, cellArea);
			dungeonData.Exit = dungeonFlowBuilder.EndRoom;
			return dungeonData;
		}

		private PrototypeDungeonRoom GetRandomEntranceRoomFromList(List<WeightedRoom> source)
		{
			List<PrototypeDungeonRoom> list = new List<PrototypeDungeonRoom>();
			for (int i = 0; i < source.Count; i++)
			{
				if (source[i].room.category == PrototypeDungeonRoom.RoomCategory.ENTRANCE)
				{
					list.Add(source[i].room);
				}
			}
			if (list.Count > 0)
			{
				return list[BraveRandom.GenerationRandomRange(0, list.Count)];
			}
			return null;
		}

		private RoomHandler GetRoomHandlerByArea(List<RoomHandler> rooms, CellArea area)
		{
			for (int i = 0; i < rooms.Count; i++)
			{
				if (rooms[i].area == area)
				{
					return rooms[i];
				}
			}
			return null;
		}

		private void DrawDebugSquare(IntVector2 pos, Color col)
		{
			Debug.DrawLine(pos.ToVector2(), pos.ToVector2() + Vector2.up, col, 1000f);
			Debug.DrawLine(pos.ToVector2(), pos.ToVector2() + Vector2.right, col, 1000f);
			Debug.DrawLine(pos.ToVector2() + Vector2.up, pos.ToVector2() + Vector2.right + Vector2.up, col, 1000f);
			Debug.DrawLine(pos.ToVector2() + Vector2.right, pos.ToVector2() + Vector2.right + Vector2.up, col, 1000f);
		}
	}
}
