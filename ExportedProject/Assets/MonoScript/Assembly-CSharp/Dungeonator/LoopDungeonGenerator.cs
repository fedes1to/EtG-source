using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Dungeonator
{
	public class LoopDungeonGenerator : IDungeonGenerator
	{
		public const bool c_ROOM_MIRRORING = false;

		public static int NUM_FAILS_COMPOSITE_REGENERATION;

		public static int NUM_FAILS_COMPOSITE_ATTACHMENT;

		public DungeonData DeferredGeneratedData;

		public bool RAPID_DEBUG_ITERATION_MODE;

		public int RAPID_DEBUG_ITERATION_INDEX;

		private SemioticDungeonGenSettings m_patternSettings;

		private DungeonFlow m_assignedFlow;

		private int m_lastAssignedSeed;

		private bool m_forceAssignedFlow;

		private Stopwatch m_timer;

		public LoopDungeonGenerator(Dungeon d, int dungeonSeed)
		{
			m_patternSettings = d.PatternSettings;
			m_assignedFlow = m_patternSettings.GetRandomFlow();
			m_lastAssignedSeed = dungeonSeed;
			Random.InitState(dungeonSeed);
			BraveRandom.InitializeWithSeed(dungeonSeed);
			GameManager.SEED_LABEL = dungeonSeed.ToString();
		}

		public void AssignFlow(DungeonFlow flow)
		{
			m_forceAssignedFlow = true;
			m_assignedFlow = flow;
		}

		protected void GetNewFlowForIteration()
		{
			if (!m_forceAssignedFlow)
			{
				m_assignedFlow = m_patternSettings.GetRandomFlow();
			}
		}

		protected void RecalculateRoomDistances(RoomHandler entrance)
		{
			Queue<Tuple<RoomHandler, int>> queue = new Queue<Tuple<RoomHandler, int>>();
			List<RoomHandler> list = new List<RoomHandler>();
			queue.Enqueue(new Tuple<RoomHandler, int>(entrance, 0));
			while (queue.Count > 0)
			{
				Tuple<RoomHandler, int> tuple = queue.Dequeue();
				tuple.First.distanceFromEntrance = tuple.Second;
				list.Add(tuple.First);
				for (int i = 0; i < tuple.First.connectedRooms.Count; i++)
				{
					RoomHandler roomHandler = tuple.First.connectedRooms[i];
					if (!list.Contains(roomHandler))
					{
						queue.Enqueue(new Tuple<RoomHandler, int>(roomHandler, tuple.Second + 1));
					}
				}
			}
		}

		public IEnumerable GenerateDungeonLayoutDeferred()
		{
			while (true)
			{
				IEnumerator<ProcessStatus> tracker = GenerateDungeonLayoutDeferred_Internal().GetEnumerator();
				bool didSucceed = false;
				while (tracker.MoveNext())
				{
					if (tracker.Current == ProcessStatus.Incomplete)
					{
						yield return null;
						continue;
					}
					if (tracker.Current == ProcessStatus.Success)
					{
						UnityEngine.Debug.Log("Succeeded generation iteration on: " + m_assignedFlow.name);
						didSucceed = true;
						break;
					}
					if (tracker.Current == ProcessStatus.Fail)
					{
						UnityEngine.Debug.Log("Failed generation iteration on: " + m_assignedFlow.name);
						didSucceed = false;
						break;
					}
				}
				if (didSucceed)
				{
					break;
				}
				GetNewFlowForIteration();
				yield return null;
			}
		}

		public IEnumerable<ProcessStatus> GenerateDungeonLayoutDeferred_Internal()
		{
			BraveMemory.DoCollect();
			if (m_timer == null)
			{
				m_timer = new Stopwatch();
			}
			bool generationSucceeded = false;
			SemioticLayoutManager layout = null;
			int attempts = 0;
			int generationAttempts = 50;
			NUM_FAILS_COMPOSITE_ATTACHMENT = 0;
			NUM_FAILS_COMPOSITE_REGENERATION = 0;
			UnityEngine.Debug.Log("Attempting to generate flow: " + m_assignedFlow.name);
			while (!generationSucceeded && attempts < generationAttempts)
			{
				attempts++;
				LoopFlowBuilder builder = new LoopFlowBuilder(m_assignedFlow, this);
				IEnumerator buildTracker = builder.DeferredBuild().GetEnumerator();
				m_timer.Reset();
				m_timer.Start();
				while (buildTracker.MoveNext())
				{
					if (m_timer.ElapsedMilliseconds > 30)
					{
						m_timer.Reset();
						yield return ProcessStatus.Incomplete;
					}
				}
				layout = builder.DeferredGeneratedLayout;
				generationSucceeded = builder.DeferredGenerationSuccess;
				if (!generationSucceeded && attempts % 3 == 0)
				{
					BraveMemory.DoCollect();
				}
				yield return ProcessStatus.Incomplete;
			}
			if (RAPID_DEBUG_ITERATION_MODE && !generationSucceeded)
			{
				yield return ProcessStatus.Fail;
			}
			if (layout == null)
			{
				yield return ProcessStatus.Fail;
			}
			if (m_assignedFlow != null)
			{
				GameStatsManager.Instance.EncounterFlow(m_assignedFlow.name);
			}
			IntVector2 min = layout.GetSafelyBoundedMinimumCellPosition();
			IntVector2 max = layout.GetSafelyBoundedMaximumCellPosition();
			IntVector2 offsetRequired = new IntVector2(-min.x + 10, -min.y + 10);
			IntVector2 span = max - min;
			layout.HandleOffsetRooms(offsetRequired);
			CellData[][] cells = new CellData[span.x + 20][];
			int cellsCreated2 = 0;
			cellsCreated2 = CreateCellDataIntelligently(cells, layout, span, offsetRequired);
			DungeonData dungeonData = new DungeonData(cells);
			List<RoomHandler> rooms = layout.Rooms;
			for (int i = 0; i < rooms.Count; i++)
			{
				for (int j = 0; j < rooms[i].connectedRooms.Count; j++)
				{
					if (!rooms.Contains(rooms[i].connectedRooms[j]))
					{
						UnityEngine.Debug.LogWarning(rooms[i].connectedRooms[j].GetRoomName() + " is not in the list!!!!!!");
					}
				}
			}
			dungeonData.InitializeCoreData(rooms);
			dungeonData.Entrance = rooms[0];
			dungeonData.Exit = rooms[rooms.Count - 1];
			for (int k = 0; k < rooms.Count; k++)
			{
				if (rooms[k].area.prototypeRoom != null)
				{
					if (rooms[k].area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.ENTRANCE)
					{
						dungeonData.Entrance = rooms[k];
					}
					else if (rooms[k].area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.EXIT)
					{
						dungeonData.Exit = rooms[k];
					}
				}
			}
			RecalculateRoomDistances(dungeonData.Entrance);
			DeferredGeneratedData = dungeonData;
			yield return ProcessStatus.Success;
		}

		private int CreateCellDataIntelligently(CellData[][] cells, SemioticLayoutManager layout, IntVector2 span, IntVector2 offsetRequired)
		{
			int num = 0;
			float[] array = new float[(span.y + 20) * (span.x + 20)];
			for (int i = 0; i < span.x + 20; i++)
			{
				for (int j = 0; j < span.y + 20; j++)
				{
					array[j * (span.x + 20) + i] = 1000000f;
				}
			}
			Queue<IntVector2> queue = new Queue<IntVector2>();
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			foreach (IntVector2 occupiedCell in layout.OccupiedCells)
			{
				IntVector2 item = occupiedCell + offsetRequired;
				array[item.y * (span.x + 20) + item.x] = 0f;
				queue.Enqueue(item);
				hashSet.Add(item);
			}
			IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
			while (queue.Count > 0)
			{
				IntVector2 intVector = queue.Dequeue();
				hashSet.Remove(intVector);
				float num2 = array[intVector.y * (span.x + 20) + intVector.x];
				for (int k = 0; k < cardinalsAndOrdinals.Length; k++)
				{
					IntVector2 item2 = intVector + cardinalsAndOrdinals[k];
					if (item2.x < 0 || item2.y < 0 || item2.x >= span.x + 20 || item2.y >= span.y + 20)
					{
						continue;
					}
					float num3 = array[item2.y * (span.x + 20) + item2.x];
					float num4 = ((k % 2 != 1) ? (num2 + 1f) : (num2 + 1.414f));
					if (num3 > num4)
					{
						array[item2.y * (span.x + 20) + item2.x] = num4;
						if (!hashSet.Contains(item2))
						{
							queue.Enqueue(item2);
							hashSet.Add(item2);
						}
					}
				}
			}
			for (int l = 0; l < cells.Length; l++)
			{
				cells[l] = new CellData[span.y + 20];
			}
			for (int m = 0; m < span.x + 20; m++)
			{
				for (int n = 0; n < span.y + 20; n++)
				{
					float num5 = array[n * (span.x + 20) + m];
					if (num5 <= 7f)
					{
						cells[m][n] = new CellData(m, n);
						num++;
					}
				}
			}
			return num;
		}

		public void GenerateDungeonLayoutThreaded()
		{
			IEnumerable enumerable = GenerateDungeonLayoutDeferred();
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
			}
		}

		public DungeonData GenerateDungeonLayout()
		{
			IEnumerable enumerable = GenerateDungeonLayoutDeferred();
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
			}
			return DeferredGeneratedData;
		}
	}
}
