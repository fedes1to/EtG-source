using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Dungeonator
{
	public class SemioticLayoutManager
	{
		public struct BBoxPrepassResults
		{
			public bool overlapping;

			public int numPairs;

			public int numPairsOverlapping;

			public int totalOverlapArea;
		}

		private int MAXIMUM_ROOM_DIMENSION = 50;

		private List<RoomHandler> m_allRooms;

		private HashSet<IntVector2> m_exitTestPoints = new HashSet<IntVector2>();

		private IntVector2 m_currentOffset = IntVector2.Zero;

		private HashSet<IntVector2> m_occupiedCells;

		private HashSet<IntVector2> m_temporaryPathfindingWalls = new HashSet<IntVector2>();

		private List<Tuple<IntVector2, IntVector2>> m_rectangleDecomposition;

		private static List<HashSet<IntVector2>> PooledResizedHashsets = new List<HashSet<IntVector2>>();

		private static IntVector2[] SimpleCardinals = new IntVector2[5]
		{
			IntVector2.Up,
			2 * IntVector2.Up,
			IntVector2.Right,
			IntVector2.Down,
			IntVector2.Left
		};

		private static IntVector2[] LayoutCardinals = new IntVector2[12]
		{
			IntVector2.Up,
			IntVector2.Right,
			IntVector2.Down,
			IntVector2.Left,
			2 * IntVector2.Up,
			3 * IntVector2.Up,
			new IntVector2(1, 1),
			new IntVector2(1, 2),
			new IntVector2(-1, 1),
			new IntVector2(-1, 2),
			new IntVector2(1, -1),
			new IntVector2(-1, -1)
		};

		private static IntVector2[] LayoutPathCardinals = new IntVector2[17]
		{
			IntVector2.Up,
			IntVector2.Right,
			IntVector2.Down,
			IntVector2.Left,
			2 * IntVector2.Up,
			3 * IntVector2.Up,
			2 * IntVector2.Right,
			new IntVector2(2, 1),
			new IntVector2(2, 2),
			new IntVector2(1, 3),
			new IntVector2(2, 3),
			new IntVector2(1, 1),
			new IntVector2(1, 2),
			new IntVector2(-1, 1),
			new IntVector2(-1, 2),
			new IntVector2(1, -1),
			new IntVector2(-1, -1)
		};

		private const int SEARCH_DISTANCE_LAYOUT = 3;

		public bool FindNearestValidLocataionForLayout2Success;

		public IntVector2 FindNearestValidLocationForLayout2Result;

		private const int PER_ROOM_HALLWAY_EXTENSION_MAX = 4;

		private const int PER_LAYOUT_HALLWAY_EXTENSION_MAX = 12;

		private int m_FIRST_FAILS;

		private int m_SECOND_FAILS;

		private int m_THIRD_FAILS;

		private int m_FOURTH_FAILS;

		public bool CanPlaceLayoutAtPointSuccess;

		public List<RoomHandler> Rooms
		{
			get
			{
				return m_allRooms;
			}
		}

		public HashSet<IntVector2> OccupiedCells
		{
			get
			{
				return m_occupiedCells;
			}
		}

		public IntVector2 Dimensions
		{
			get
			{
				Tuple<IntVector2, IntVector2> minAndMaxCellPositions = GetMinAndMaxCellPositions();
				return minAndMaxCellPositions.Second - minAndMaxCellPositions.First;
			}
		}

		public IntVector2 NegativeDimensions
		{
			get
			{
				return IntVector2.Max(IntVector2.Zero, IntVector2.Zero - GetMinimumCellPosition());
			}
		}

		public IntVector2 PositiveDimensions
		{
			get
			{
				return IntVector2.Max(IntVector2.Zero, GetMaximumCellPosition());
			}
		}

		public List<Tuple<IntVector2, IntVector2>> RectangleDecomposition
		{
			get
			{
				return m_rectangleDecomposition;
			}
		}

		public SemioticLayoutManager()
		{
			m_allRooms = new List<RoomHandler>();
			if (PooledResizedHashsets.Count > 0)
			{
				int index = 0;
				for (int i = 0; i < PooledResizedHashsets.Count; i++)
				{
					if (PooledResizedHashsets[index].Count < PooledResizedHashsets[i].Count)
					{
						index = i;
					}
				}
				m_occupiedCells = PooledResizedHashsets[index];
				PooledResizedHashsets.RemoveAt(index);
			}
			else
			{
				m_occupiedCells = new HashSet<IntVector2>();
			}
		}

		public void ComputeRectangleDecomposition()
		{
			if (m_rectangleDecomposition == null)
			{
				m_rectangleDecomposition = new List<Tuple<IntVector2, IntVector2>>();
			}
			else if (m_rectangleDecomposition.Count != 0)
			{
				return;
			}
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				if (hashSet.Contains(occupiedCell))
				{
					continue;
				}
				int num = 1;
				int num2 = 1;
				while (true)
				{
					int y = occupiedCell.y + num2;
					IntVector2 item = new IntVector2(occupiedCell.x, y);
					if (m_occupiedCells.Contains(item))
					{
						num2++;
						continue;
					}
					break;
				}
				while (true)
				{
					int x = occupiedCell.x + num;
					bool flag = true;
					for (int i = occupiedCell.y; i < occupiedCell.y + num2; i++)
					{
						IntVector2 item2 = new IntVector2(x, i);
						if (!m_occupiedCells.Contains(item2))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						num++;
						continue;
					}
					break;
				}
				for (int j = occupiedCell.x; j < occupiedCell.x + num; j++)
				{
					for (int k = occupiedCell.y; k < occupiedCell.y + num2; k++)
					{
						IntVector2 item3 = new IntVector2(j, k);
						hashSet.Add(item3);
					}
				}
				m_rectangleDecomposition.Add(new Tuple<IntVector2, IntVector2>(occupiedCell, new IntVector2(num, num2)));
			}
			m_rectangleDecomposition = m_rectangleDecomposition.OrderByDescending((Tuple<IntVector2, IntVector2> a) => a.Second.x * a.Second.y).ToList();
		}

		public void OnDestroy()
		{
			m_occupiedCells.Clear();
			if (PooledResizedHashsets.Count > 10)
			{
				int index = 0;
				for (int i = 0; i < PooledResizedHashsets.Count; i++)
				{
					if (PooledResizedHashsets[index].Count > PooledResizedHashsets[i].Count)
					{
						index = i;
					}
				}
				PooledResizedHashsets.RemoveAt(index);
			}
			PooledResizedHashsets.Add(m_occupiedCells);
		}

		public void DebugListLengths()
		{
			Debug.Log("SLayoutManager list sizes: " + m_allRooms.Count + "|" + m_occupiedCells.Count);
		}

		public void DebugDrawOccupiedCells(Vector2 positionOffset)
		{
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				BraveUtility.DrawDebugSquare(occupiedCell.ToVector2() + positionOffset, Color.red, 1000f);
			}
		}

		public void DebugDrawBoundingBox(Vector2 positionOffset, Color c)
		{
			Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				vector.x = Mathf.Min(occupiedCell.x, vector.x);
				vector.y = Mathf.Min(occupiedCell.y, vector.x);
				vector2.x = Mathf.Max(occupiedCell.x, vector2.x);
				vector2.y = Mathf.Max(occupiedCell.y, vector2.x);
			}
			BraveUtility.DrawDebugSquare(vector + positionOffset, vector2 + Vector2.one + positionOffset, c, 1000f);
		}

		public void ClearTemporary()
		{
			m_temporaryPathfindingWalls.Clear();
		}

		public void StampComplexExitTemporary(RuntimeRoomExitData exit, CellArea area)
		{
			PrototypeRoomExit referencedExit = exit.referencedExit;
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(referencedExit.exitDirection);
			int num = ((exit.jointedExit && referencedExit.exitDirection != DungeonData.Direction.WEST) ? 1 : 0);
			for (int i = 0; i < referencedExit.containedCells.Count; i++)
			{
				for (int j = 0; j < exit.TotalExitLength + num; j++)
				{
					IntVector2 intVector = referencedExit.containedCells[i].ToIntVector2() - IntVector2.One + area.basePosition + intVector2FromDirection * j;
					m_temporaryPathfindingWalls.Add(intVector);
					for (int k = 0; k < SimpleCardinals.Length; k++)
					{
						m_temporaryPathfindingWalls.Add(intVector + LayoutCardinals[k]);
					}
				}
			}
		}

		public void StampComplexExitToLayout(RuntimeRoomExitData exit, CellArea area, bool unstamp = false)
		{
			PrototypeRoomExit referencedExit = exit.referencedExit;
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(referencedExit.exitDirection);
			int num = ((exit.jointedExit && referencedExit.exitDirection != DungeonData.Direction.WEST) ? 1 : 0);
			for (int i = 0; i < referencedExit.containedCells.Count; i++)
			{
				for (int j = 0; j < exit.TotalExitLength + num; j++)
				{
					IntVector2 intVector = referencedExit.containedCells[i].ToIntVector2() - IntVector2.One + area.basePosition + intVector2FromDirection * j;
					if (unstamp)
					{
						m_occupiedCells.Remove(intVector);
						for (int k = 0; k < LayoutCardinals.Length; k++)
						{
							m_occupiedCells.Remove(intVector + LayoutCardinals[k]);
						}
					}
					else
					{
						m_occupiedCells.Add(intVector);
						for (int l = 0; l < LayoutCardinals.Length; l++)
						{
							m_occupiedCells.Add(intVector + LayoutCardinals[l]);
						}
					}
				}
			}
			IntVector2 item = referencedExit.containedCells[0].ToIntVector2() - IntVector2.One + area.basePosition + intVector2FromDirection * (exit.TotalExitLength + num - 1);
			if (unstamp)
			{
				m_exitTestPoints.Remove(item);
			}
			else
			{
				m_exitTestPoints.Add(item);
			}
			if (m_rectangleDecomposition != null)
			{
				m_rectangleDecomposition.Clear();
			}
		}

		public void DebugDrawComplexit(RuntimeRoomExitData exit, CellArea area, Color c)
		{
			PrototypeRoomExit referencedExit = exit.referencedExit;
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(referencedExit.exitDirection);
			int num = ((exit.jointedExit && referencedExit.exitDirection != DungeonData.Direction.WEST) ? 1 : 0);
			for (int i = 0; i < referencedExit.containedCells.Count; i++)
			{
				for (int j = 0; j < exit.TotalExitLength + num; j++)
				{
					IntVector2 intVector = referencedExit.containedCells[i].ToIntVector2() - IntVector2.One + area.basePosition + intVector2FromDirection * j;
					m_occupiedCells.Add(intVector);
					for (int k = 0; k < LayoutCardinals.Length; k++)
					{
						BraveUtility.DrawDebugSquare((intVector + LayoutCardinals[k]).ToVector2(), c, 1000f);
					}
				}
			}
		}

		public BBoxPrepassResults CheckRoomBoundingBoxCollisions(SemioticLayoutManager otherCanvas, IntVector2 otherCanvasOffset)
		{
			BBoxPrepassResults result = default(BBoxPrepassResults);
			result.overlapping = false;
			for (int i = 0; i < m_allRooms.Count; i++)
			{
				RoomHandler roomHandler = m_allRooms[i];
				for (int j = 0; j < otherCanvas.m_allRooms.Count; j++)
				{
					RoomHandler roomHandler2 = otherCanvas.m_allRooms[j];
					result.numPairs++;
					int cellsOverlapping = 0;
					if (IntVector2.AABBOverlapWithArea(roomHandler.area.basePosition, roomHandler.area.dimensions, roomHandler2.area.basePosition + otherCanvasOffset, roomHandler2.area.dimensions, out cellsOverlapping))
					{
						result.overlapping = true;
						result.numPairsOverlapping++;
						result.totalOverlapArea += cellsOverlapping;
					}
				}
			}
			return result;
		}

		private bool CheckRectangleDecompositionCollisions(SemioticLayoutManager otherCanvas, IntVector2 otherCanvasOffset)
		{
			for (int i = 0; i < otherCanvas.m_rectangleDecomposition.Count; i++)
			{
				Tuple<IntVector2, IntVector2> tuple = otherCanvas.m_rectangleDecomposition[i];
				for (int j = 0; j < m_rectangleDecomposition.Count; j++)
				{
					Tuple<IntVector2, IntVector2> tuple2 = m_rectangleDecomposition[j];
					if (IntVector2.AABBOverlap(tuple.First + otherCanvasOffset, tuple.Second, tuple2.First, tuple2.Second))
					{
						return false;
					}
				}
			}
			return true;
		}

		public IEnumerable FindNearestValidLocationForLayout2(SemioticLayoutManager canvas, RuntimeRoomExitData staticExit, RuntimeRoomExitData newExit, IntVector2 staticAreaBasePosition, IntVector2 newAreaBasePosition)
		{
			FindNearestValidLocataionForLayout2Success = false;
			IntVector2 currentPosition2 = (FindNearestValidLocationForLayout2Result = newAreaBasePosition);
			int ix = 0;
			int iy = 0;
			int dx = 3;
			int dy = 3;
			Queue<IntVector2> spiralPointQueue = new Queue<IntVector2>();
			int iterations = 5000;
			while (iterations > 0)
			{
				iterations--;
				currentPosition2 = newAreaBasePosition + new IntVector2(ix, iy);
				IntVector2 staticExitCanvasPosition = staticAreaBasePosition + staticExit.ExitOrigin - IntVector2.One;
				IntVector2 newExitCanvasPosition = currentPosition2 + newExit.ExitOrigin - IntVector2.One;
				IntVector2 canvasTranslation = staticExitCanvasPosition - newExitCanvasPosition;
				spiralPointQueue.Enqueue(canvasTranslation);
				if (ix == 0 && iy >= 0)
				{
					iy += 3;
				}
				if (ix == 0)
				{
					dy *= -1;
				}
				if (iy == 0)
				{
					dx *= -1;
				}
				ix += dx;
				iy += dy;
				if (iterations % 250 == 0)
				{
					yield return null;
				}
			}
			SpiralPointLayoutHandler.spiralOffsets = spiralPointQueue;
			SpiralPointLayoutHandler.nextElementIndex = 0;
			SpiralPointLayoutHandler.currentResultElementIndex = -1;
			int NUM_THREADS = 6;
			Thread[] spiralThreads = new Thread[NUM_THREADS];
			for (int i = 0; i < NUM_THREADS; i++)
			{
				SpiralPointLayoutHandler @object = new SpiralPointLayoutHandler(this, canvas, i);
				Thread thread = (spiralThreads[i] = new Thread(@object.ThreadRun));
			}
			try
			{
				for (int j = 0; j < NUM_THREADS; j++)
				{
					spiralThreads[j].Start();
				}
				for (int k = 0; k < NUM_THREADS; k++)
				{
					spiralThreads[k].Join();
				}
			}
			catch (ThreadStateException ex)
			{
				Debug.LogError("WELL THIS FUCKING SUCKS : " + ex.Message);
				SpiralPointLayoutHandler.currentResultElementIndex = -1;
			}
			catch (ThreadInterruptedException ex2)
			{
				Debug.LogError("THIS SUCKS MARGINALLY LESS : " + ex2.Message);
				SpiralPointLayoutHandler.currentResultElementIndex = -1;
			}
			if (SpiralPointLayoutHandler.currentResultElementIndex == -1)
			{
				BraveUtility.Log("Failed iterations on find nearest valid location for layout.", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
				FindNearestValidLocationForLayout2Result = IntVector2.Zero;
				FindNearestValidLocataionForLayout2Success = false;
			}
			else
			{
				FindNearestValidLocationForLayout2Result = SpiralPointLayoutHandler.resultOffset;
				FindNearestValidLocataionForLayout2Success = true;
			}
		}

		public bool FindNearestValidLocationForLayout(SemioticLayoutManager canvas, RuntimeRoomExitData staticExit, RuntimeRoomExitData newExit, IntVector2 staticAreaBasePosition, IntVector2 newAreaBasePosition, out IntVector2 idealPosition)
		{
			IntVector2 intVector = (idealPosition = newAreaBasePosition);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = -1;
			int num5 = 50000;
			while (num5 > 0)
			{
				num5--;
				intVector = newAreaBasePosition + new IntVector2(num, num2);
				IntVector2 intVector2 = staticAreaBasePosition + staticExit.ExitOrigin - IntVector2.One;
				IntVector2 intVector3 = intVector + newExit.ExitOrigin - IntVector2.One;
				IntVector2 intVector4 = intVector2 - intVector3;
				bool flag = true;
				foreach (IntVector2 occupiedCell in canvas.m_occupiedCells)
				{
					IntVector2 item = occupiedCell + intVector4;
					if (m_occupiedCells.Contains(item))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					idealPosition = intVector;
					return true;
				}
				if (num == num2 || (num < 0 && num == -num2) || (num > 0 && num == 1 - num2))
				{
					int num6 = num3;
					num3 = -num4;
					num4 = num6;
				}
				num += num3;
				num2 += num4;
			}
			BraveUtility.Log("Failed iterations on find nearest valid location for layout.", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
			idealPosition = IntVector2.Zero;
			return false;
		}

		public bool FindNearestValidLocationForRoom(PrototypeDungeonRoom prototype, IntVector2 startPosition, out IntVector2 idealPosition)
		{
			IntVector2 intVector = startPosition;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = -1;
			int num5 = 10000;
			while (num5 > 0)
			{
				num5--;
				bool flag = true;
				intVector = startPosition + new IntVector2(num, num2);
				for (int i = -1; i < prototype.Width + 1; i++)
				{
					int num6 = -1;
					while (num6 < prototype.Height + 1)
					{
						IntVector2 item = intVector + new IntVector2(i, num6);
						if (!m_occupiedCells.Contains(item))
						{
							num6++;
							continue;
						}
						goto IL_0061;
					}
					continue;
					IL_0061:
					flag = false;
					break;
				}
				if (flag)
				{
					idealPosition = intVector;
					return true;
				}
				if (num == num2 || (num < 0 && num == -num2) || (num > 0 && num == 1 - num2))
				{
					int num7 = num3;
					num3 = -num4;
					num4 = num7;
				}
				num += num3;
				num2 += num4;
			}
			idealPosition = IntVector2.Zero;
			return false;
		}

		public void StampCellAreaToLayout(RoomHandler newRoom, bool unstamp = false)
		{
			CellArea area = newRoom.area;
			if (!unstamp)
			{
				m_allRooms.Add(newRoom);
			}
			else
			{
				m_allRooms.Remove(newRoom);
			}
			if (area.prototypeRoom != null)
			{
				List<IntVector2> cellRepresentationIncFacewalls = area.prototypeRoom.GetCellRepresentationIncFacewalls();
				foreach (IntVector2 item in cellRepresentationIncFacewalls)
				{
					if (unstamp)
					{
						m_occupiedCells.Remove(item + area.basePosition);
					}
					else
					{
						m_occupiedCells.Add(item + area.basePosition);
					}
				}
			}
			else if (area.proceduralCells != null && area.proceduralCells.Count > 0)
			{
				for (int i = 0; i < area.proceduralCells.Count; i++)
				{
					m_occupiedCells.Add(area.proceduralCells[i] + area.basePosition);
					for (int j = 0; j < LayoutCardinals.Length; j++)
					{
						m_occupiedCells.Add(area.proceduralCells[i] + LayoutCardinals[j] + area.basePosition);
					}
				}
			}
			else
			{
				for (int k = 0; k < area.dimensions.x; k++)
				{
					for (int l = 0; l < area.dimensions.y; l++)
					{
						IntVector2 intVector = new IntVector2(area.basePosition.x + k, area.basePosition.y + l);
						if (unstamp)
						{
							m_occupiedCells.Remove(intVector);
							for (int m = 0; m < LayoutCardinals.Length; m++)
							{
								m_occupiedCells.Remove(intVector + LayoutCardinals[m]);
							}
						}
						else
						{
							m_occupiedCells.Add(intVector);
							for (int n = 0; n < LayoutCardinals.Length; n++)
							{
								m_occupiedCells.Add(intVector + LayoutCardinals[n]);
							}
						}
					}
				}
			}
			if (m_rectangleDecomposition != null)
			{
				m_rectangleDecomposition.Clear();
			}
		}

		public void HandleOffsetRooms(IntVector2 offset)
		{
			m_currentOffset += offset;
			for (int i = 0; i < m_allRooms.Count; i++)
			{
				RoomHandler roomHandler = m_allRooms[i];
				roomHandler.area.basePosition += offset;
				m_allRooms[i] = roomHandler;
			}
		}

		public IntVector2 GetSafelyBoundedMinimumCellPosition()
		{
			IntVector2 intVector = GetMinimumCellPosition();
			for (int i = 0; i < m_allRooms.Count; i++)
			{
				intVector = IntVector2.Min(intVector, m_allRooms[i].area.basePosition);
			}
			return intVector;
		}

		public IntVector2 GetSafelyBoundedMaximumCellPosition()
		{
			IntVector2 intVector = GetMaximumCellPosition();
			for (int i = 0; i < m_allRooms.Count; i++)
			{
				intVector = IntVector2.Max(intVector, m_allRooms[i].area.basePosition + m_allRooms[i].area.dimensions);
			}
			return intVector;
		}

		public Tuple<IntVector2, IntVector2> GetMinAndMaxCellPositions()
		{
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			int num3 = int.MinValue;
			int num4 = int.MinValue;
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				num = Math.Min(num, occupiedCell.x);
				num2 = Math.Min(num2, occupiedCell.y);
				num3 = Math.Max(num3, occupiedCell.x);
				num4 = Math.Max(num4, occupiedCell.y);
			}
			return new Tuple<IntVector2, IntVector2>(new IntVector2(num, num2), new IntVector2(num3, num4));
		}

		public IntVector2 GetMinimumCellPosition()
		{
			int num = int.MaxValue;
			int num2 = int.MaxValue;
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				num = Math.Min(num, occupiedCell.x);
				num2 = Math.Min(num2, occupiedCell.y);
			}
			return new IntVector2(num, num2);
		}

		public IntVector2 GetMaximumCellPosition()
		{
			int num = int.MinValue;
			int num2 = int.MinValue;
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				num = Math.Max(num, occupiedCell.x);
				num2 = Math.Max(num2, occupiedCell.y);
			}
			return new IntVector2(num, num2);
		}

		public bool CanPlaceCellBounds(CellArea newArea)
		{
			for (int i = 0; i < m_allRooms.Count; i++)
			{
				if (m_allRooms[i].area.OverlapsWithUnitBorder(newArea))
				{
					return false;
				}
			}
			return true;
		}

		private bool CheckExitsClearForPlacement(PrototypeDungeonRoom newRoom, RuntimeRoomExitData exitToTest, IntVector2 attachPoint)
		{
			IntVector2 areaBasePosition = attachPoint - (exitToTest.ExitOrigin - IntVector2.One);
			return CheckExitClearForPlacement(exitToTest, areaBasePosition);
		}

		private Tuple<IntVector2, IntVector2> GetExitRectCells(RuntimeRoomExitData exit, IntVector2 areaBasePosition)
		{
			PrototypeRoomExit referencedExit = exit.referencedExit;
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(referencedExit.exitDirection);
			int num = ((exit.jointedExit && referencedExit.exitDirection != DungeonData.Direction.WEST) ? 1 : 0);
			if (exit.jointedExit)
			{
				num++;
			}
			int b = exit.TotalExitLength + num;
			b = Mathf.Max(4, b);
			int num2 = int.MaxValue;
			int num3 = int.MaxValue;
			int num4 = int.MinValue;
			int num5 = int.MinValue;
			for (int i = 0; i < referencedExit.containedCells.Count; i++)
			{
				num2 = Mathf.Min((int)referencedExit.containedCells[i].x, num2);
				num3 = Mathf.Min((int)referencedExit.containedCells[i].y, num3);
				num4 = Mathf.Max((int)referencedExit.containedCells[i].x, num4);
				num5 = Mathf.Max((int)referencedExit.containedCells[i].y, num5);
			}
			IntVector2 intVector = new IntVector2(num2, num3) - IntVector2.One;
			IntVector2 intVector2 = new IntVector2(num4, num5) + IntVector2.One;
			IntVector2 intVector3 = intVector + areaBasePosition + intVector2FromDirection * 3;
			IntVector2 intVector4 = intVector2 + areaBasePosition + intVector2FromDirection * b;
			IntVector2 first = IntVector2.Min(intVector3, intVector4);
			IntVector2 second = IntVector2.Max(intVector4, intVector3);
			if (!exit.jointedExit && (referencedExit.exitDirection == DungeonData.Direction.NORTH || referencedExit.exitDirection == DungeonData.Direction.SOUTH))
			{
				second += new IntVector2(1, 0);
				first -= new IntVector2(1, 0);
			}
			else
			{
				second += new IntVector2(2, 3);
				first -= new IntVector2(2, 2);
			}
			return new Tuple<IntVector2, IntVector2>(first, second);
		}

		private bool CheckRectAgainstLayout(Tuple<IntVector2, IntVector2> rectTuple, SemioticLayoutManager layout)
		{
			for (int i = rectTuple.First.x; i < rectTuple.Second.x; i++)
			{
				for (int j = rectTuple.First.y; j < rectTuple.Second.y; j++)
				{
					IntVector2 item = new IntVector2(i, j);
					if (layout.m_occupiedCells.Contains(item))
					{
						return false;
					}
				}
			}
			return true;
		}

		public IEnumerable<ProcessStatus> CheckExitsAgainstDisparateLayouts(SemioticLayoutManager otherLayout, RuntimeRoomExitData staticExit, IntVector2 staticAreaBasePosition, RuntimeRoomExitData newExit, IntVector2 newAreaBasePosition)
		{
			IntVector2 staticExitCanvasPosition = staticAreaBasePosition + staticExit.ExitOrigin - IntVector2.One;
			IntVector2 newExitCanvasPosition = newAreaBasePosition + newExit.ExitOrigin - IntVector2.One;
			IntVector2 canvasTranslation = staticExitCanvasPosition - newExitCanvasPosition;
			Tuple<IntVector2, IntVector2> staticRect = GetExitRectCells(staticExit, staticAreaBasePosition);
			Tuple<IntVector2, IntVector2> newRect = GetExitRectCells(newExit, newAreaBasePosition + canvasTranslation);
			yield return ProcessStatus.Incomplete;
			Tuple<IntVector2, IntVector2> staticRectOther = new Tuple<IntVector2, IntVector2>(staticRect.First - canvasTranslation, staticRect.Second - canvasTranslation);
			Tuple<IntVector2, IntVector2> newRectOther = new Tuple<IntVector2, IntVector2>(newRect.First - canvasTranslation, newRect.Second - canvasTranslation);
			bool firstCheck = CheckRectAgainstLayout(staticRect, this);
			if (!firstCheck)
			{
				m_FIRST_FAILS++;
				yield return ProcessStatus.Fail;
				yield break;
			}
			bool secondCheck = CheckRectAgainstLayout(newRect, this);
			if (!secondCheck)
			{
				m_SECOND_FAILS++;
				yield return ProcessStatus.Fail;
				yield break;
			}
			yield return ProcessStatus.Incomplete;
			bool thirdCheck = CheckRectAgainstLayout(staticRectOther, otherLayout);
			if (!thirdCheck)
			{
				m_THIRD_FAILS++;
				yield return ProcessStatus.Fail;
				yield break;
			}
			bool fourthCheck = CheckRectAgainstLayout(newRectOther, otherLayout);
			if (!fourthCheck)
			{
				m_FOURTH_FAILS++;
				yield return ProcessStatus.Fail;
			}
			else if (firstCheck && secondCheck && thirdCheck && fourthCheck)
			{
				yield return ProcessStatus.Success;
			}
			else
			{
				yield return ProcessStatus.Fail;
			}
		}

		private bool CheckExitClearForPlacement(RuntimeRoomExitData exit, IntVector2 areaBasePosition, bool debugMode = false, SemioticLayoutManager debugManager = null)
		{
			PrototypeRoomExit referencedExit = exit.referencedExit;
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(referencedExit.exitDirection);
			int num = ((exit.jointedExit && referencedExit.exitDirection != DungeonData.Direction.WEST) ? 1 : 0);
			for (int i = 0; i < referencedExit.containedCells.Count; i++)
			{
				int num2 = 3;
				for (int j = num2; j < exit.TotalExitLength + num; j++)
				{
					IntVector2 intVector = referencedExit.containedCells[i].ToIntVector2() - IntVector2.One + areaBasePosition + intVector2FromDirection * j;
					if (m_occupiedCells.Contains(intVector))
					{
						return false;
					}
					for (int k = 0; k < LayoutCardinals.Length; k++)
					{
						if (m_occupiedCells.Contains(intVector + LayoutCardinals[k]))
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private bool CheckExitsClearForPlacement(RuntimeRoomExitData exitToTest, IntVector2 attachPoint)
		{
			IntVector2 areaBasePosition = attachPoint - (exitToTest.ExitOrigin - IntVector2.One);
			return CheckExitClearForPlacement(exitToTest, areaBasePosition);
		}

		private bool CheckExitsClearForPlacement(PrototypeDungeonRoom newRoom, RuntimeRoomExitData exitToTest, IntVector2 basePositionOfPreviousRoom, RuntimeRoomExitData previousExit, IntVector2 attachPoint)
		{
			IntVector2 areaBasePosition = attachPoint - (exitToTest.ExitOrigin - IntVector2.One);
			return CheckExitClearForPlacement(exitToTest, areaBasePosition);
		}

		private bool CheckExitsClearForPlacement2(PrototypeDungeonRoom newRoom, RuntimeRoomExitData exitToTest, IntVector2 basePositionOfPreviousRoom, RuntimeRoomExitData previousExit, IntVector2 attachPoint)
		{
			IntVector2 areaBasePosition = attachPoint - (exitToTest.ExitOrigin - IntVector2.One);
			IntVector2 areaBasePosition2 = attachPoint - (previousExit.ExitOrigin - IntVector2.One);
			Tuple<IntVector2, IntVector2> exitRectCells = GetExitRectCells(exitToTest, areaBasePosition);
			Tuple<IntVector2, IntVector2> exitRectCells2 = GetExitRectCells(previousExit, areaBasePosition2);
			return CheckRectAgainstLayout(exitRectCells, this) && CheckRectAgainstLayout(exitRectCells2, this);
		}

		public IEnumerable CanPlaceLayoutAtPoint(SemioticLayoutManager layout, RuntimeRoomExitData staticExit, RuntimeRoomExitData newExit, IntVector2 staticAreaBasePosition, IntVector2 newAreaBasePosition)
		{
			CanPlaceLayoutAtPointSuccess = false;
			staticExit.additionalExitLength = 0;
			newExit.additionalExitLength = 0;
			Tuple<PrototypeRoomExit, PrototypeRoomExit> exitPair = new Tuple<PrototypeRoomExit, PrototypeRoomExit>(staticExit.referencedExit, newExit.referencedExit);
			bool isInterestingCombo = false;
			if (((exitPair.First.exitDirection == DungeonData.Direction.NORTH || exitPair.First.exitDirection == DungeonData.Direction.SOUTH) && (exitPair.Second.exitDirection == DungeonData.Direction.EAST || exitPair.Second.exitDirection == DungeonData.Direction.WEST)) || ((exitPair.Second.exitDirection == DungeonData.Direction.NORTH || exitPair.Second.exitDirection == DungeonData.Direction.SOUTH) && (exitPair.First.exitDirection == DungeonData.Direction.EAST || exitPair.First.exitDirection == DungeonData.Direction.WEST)))
			{
				isInterestingCombo = true;
			}
			if (isInterestingCombo)
			{
				newExit.additionalExitLength = 3;
			}
			staticExit.jointedExit = isInterestingCombo;
			newExit.jointedExit = isInterestingCombo;
			IntVector2 initialExitOffsets = new IntVector2(staticExit.additionalExitLength, newExit.additionalExitLength);
			staticExit.additionalExitLength = initialExitOffsets.x;
			newExit.additionalExitLength = initialExitOffsets.y;
			IntVector2 staticExitCanvasPosition = staticAreaBasePosition + staticExit.ExitOrigin - IntVector2.One;
			IntVector2 newExitCanvasPosition = newAreaBasePosition + newExit.ExitOrigin - IntVector2.One;
			HashSet<IntVector2> problemChildren = new HashSet<IntVector2>();
			int EXIT_FAILS = 0;
			int CELL_FAILS = 0;
			m_FIRST_FAILS = (m_SECOND_FAILS = (m_THIRD_FAILS = (m_FOURTH_FAILS = 0)));
			int modHallwayExtensionMax = 12;
			for (int diag = 0; diag < modHallwayExtensionMax * 2 - 1; diag++)
			{
				int numCellsInDiag = Mathf.RoundToInt(Mathf.PingPong(diag, modHallwayExtensionMax));
				IntVector2 diagInitialCell = new IntVector2(Mathf.Clamp(diag - modHallwayExtensionMax, 0, modHallwayExtensionMax - 1), Mathf.Clamp(diag, 0, modHallwayExtensionMax - 1));
				for (int diagCoord = 0; diagCoord < numCellsInDiag; diagCoord++)
				{
					IntVector2 currentCell = diagInitialCell + new IntVector2(1, -1) * diagCoord;
					staticExit.additionalExitLength = initialExitOffsets.x + currentCell.x;
					newExit.additionalExitLength = initialExitOffsets.y + currentCell.y;
					staticExitCanvasPosition = staticAreaBasePosition + staticExit.ExitOrigin - IntVector2.One;
					newExitCanvasPosition = newAreaBasePosition + newExit.ExitOrigin - IntVector2.One;
					IntVector2 canvasTranslation = staticExitCanvasPosition - newExitCanvasPosition;
					bool exitCheckSucceeded = false;
					IEnumerator<ProcessStatus> ExitCheckTracker = CheckExitsAgainstDisparateLayouts(layout, staticExit, staticAreaBasePosition, newExit, newAreaBasePosition).GetEnumerator();
					while (ExitCheckTracker.MoveNext())
					{
						if (ExitCheckTracker.Current == ProcessStatus.Success)
						{
							exitCheckSucceeded = true;
							break;
						}
					}
					if (!exitCheckSucceeded)
					{
						EXIT_FAILS++;
						continue;
					}
					bool success = true;
					int iterator = 0;
					foreach (IntVector2 problemChild in problemChildren)
					{
						iterator++;
						IntVector2 readjustedPoint = problemChild + canvasTranslation;
						if (m_occupiedCells.Contains(readjustedPoint))
						{
							success = false;
							CELL_FAILS++;
							break;
						}
						if (iterator % 600 == 0)
						{
							yield return null;
						}
					}
					if (!success)
					{
						continue;
					}
					foreach (IntVector2 cellPosition in layout.m_occupiedCells)
					{
						iterator++;
						IntVector2 adjustedPosition = cellPosition + canvasTranslation;
						if (m_occupiedCells.Contains(adjustedPosition))
						{
							success = false;
							CELL_FAILS++;
							problemChildren.Add(cellPosition);
							break;
						}
						if (iterator % 600 == 0)
						{
							yield return null;
						}
					}
					if (success)
					{
						CanPlaceLayoutAtPointSuccess = true;
						yield break;
					}
					yield return null;
				}
			}
			CanPlaceLayoutAtPointSuccess = false;
		}

		public bool CanPlaceRoomAtAttachPointByExit2(PrototypeDungeonRoom newRoom, RuntimeRoomExitData exitToTest, IntVector2 basePositionOfPreviousRoom, RuntimeRoomExitData previousExit)
		{
			exitToTest.additionalExitLength = 0;
			previousExit.additionalExitLength = 0;
			Tuple<PrototypeRoomExit, PrototypeRoomExit> tuple = new Tuple<PrototypeRoomExit, PrototypeRoomExit>(exitToTest.referencedExit, previousExit.referencedExit);
			bool flag = false;
			if (((tuple.First.exitDirection == DungeonData.Direction.NORTH || tuple.First.exitDirection == DungeonData.Direction.SOUTH) && (tuple.Second.exitDirection == DungeonData.Direction.EAST || tuple.Second.exitDirection == DungeonData.Direction.WEST)) || ((tuple.Second.exitDirection == DungeonData.Direction.NORTH || tuple.Second.exitDirection == DungeonData.Direction.SOUTH) && (tuple.First.exitDirection == DungeonData.Direction.EAST || tuple.First.exitDirection == DungeonData.Direction.WEST)))
			{
				flag = true;
			}
			if (flag)
			{
				exitToTest.additionalExitLength = 3;
			}
			IntVector2 intVector = new IntVector2(exitToTest.additionalExitLength, previousExit.additionalExitLength);
			for (int i = 0; i < 7; i++)
			{
				int num = Mathf.RoundToInt(Mathf.PingPong(i, 4f));
				IntVector2 intVector2 = new IntVector2(Mathf.Clamp(i - 4, 0, 3), Mathf.Clamp(i, 0, 3));
				for (int j = 0; j < num; j++)
				{
					IntVector2 intVector3 = intVector2 + new IntVector2(1, -1) * j;
					exitToTest.additionalExitLength = intVector.x + intVector3.x;
					previousExit.additionalExitLength = intVector.y + intVector3.y;
					IntVector2 intVector4 = basePositionOfPreviousRoom + previousExit.ExitOrigin - IntVector2.One;
					IntVector2 intVector5 = intVector4 - (exitToTest.ExitOrigin - IntVector2.One);
					int num2 = intVector5.x - 1;
					int num3 = intVector5.x + newRoom.Width + 2;
					int num4 = intVector5.y - 1;
					int num5 = intVector5.y + newRoom.Height + 4;
					bool flag2 = false;
					if (!CheckExitsClearForPlacement2(newRoom, exitToTest, basePositionOfPreviousRoom, previousExit, intVector4))
					{
						continue;
					}
					bool flag3 = true;
					for (int k = 0; k < m_allRooms.Count; k++)
					{
						CellArea area = m_allRooms[k].area;
						int num6 = area.basePosition.x - 1;
						int num7 = area.basePosition.x + area.dimensions.x + 2;
						int num8 = area.basePosition.y - 1;
						int num9 = area.basePosition.y + area.dimensions.y + 4;
						if (num2 < num7 && num3 > num6 && num4 < num9 && num5 > num8)
						{
							flag3 = false;
							break;
						}
					}
					if (!flag3 || true)
					{
						List<IntVector2> cellRepresentationIncFacewalls = newRoom.GetCellRepresentationIncFacewalls();
						for (int l = 0; l < cellRepresentationIncFacewalls.Count; l++)
						{
							if (flag2)
							{
								break;
							}
							if (m_occupiedCells.Contains(intVector5 + cellRepresentationIncFacewalls[l]))
							{
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						if (flag)
						{
							exitToTest.jointedExit = true;
							previousExit.jointedExit = true;
						}
						return true;
					}
				}
			}
			return false;
		}

		public bool CanPlaceRawCellPositions(List<IntVector2> positions)
		{
			for (int i = 0; i < positions.Count; i++)
			{
				IntVector2 intVector = positions[i];
				if (m_occupiedCells.Contains(intVector))
				{
					return false;
				}
				for (int j = 0; j < LayoutPathCardinals.Length; j++)
				{
					if (m_occupiedCells.Contains(intVector + LayoutPathCardinals[j]))
					{
						return false;
					}
				}
			}
			return true;
		}

		public IEnumerable<ProcessStatus> CanPlaceRoomAtAttachPointByExit(PrototypeDungeonRoom newRoom, RuntimeRoomExitData exitToTest, IntVector2 basePositionOfPreviousRoom, RuntimeRoomExitData previousExit)
		{
			exitToTest.additionalExitLength = 0;
			previousExit.additionalExitLength = 0;
			Tuple<PrototypeRoomExit, PrototypeRoomExit> exitPair = new Tuple<PrototypeRoomExit, PrototypeRoomExit>(exitToTest.referencedExit, previousExit.referencedExit);
			bool isInterestingCombo = false;
			if (((exitPair.First.exitDirection == DungeonData.Direction.NORTH || exitPair.First.exitDirection == DungeonData.Direction.SOUTH) && (exitPair.Second.exitDirection == DungeonData.Direction.EAST || exitPair.Second.exitDirection == DungeonData.Direction.WEST)) || ((exitPair.Second.exitDirection == DungeonData.Direction.NORTH || exitPair.Second.exitDirection == DungeonData.Direction.SOUTH) && (exitPair.First.exitDirection == DungeonData.Direction.EAST || exitPair.First.exitDirection == DungeonData.Direction.WEST)))
			{
				isInterestingCombo = true;
			}
			if (isInterestingCombo)
			{
				exitToTest.additionalExitLength = 3;
			}
			IntVector2 initialExitOffsets = new IntVector2(exitToTest.additionalExitLength, previousExit.additionalExitLength);
			for (int diag = 0; diag < 7; diag++)
			{
				int numCellsInDiag = Mathf.RoundToInt(Mathf.PingPong(diag, 4f));
				IntVector2 diagInitialCell = new IntVector2(Mathf.Clamp(diag - 4, 0, 3), Mathf.Clamp(diag, 0, 3));
				for (int diagCoord = 0; diagCoord < numCellsInDiag; diagCoord++)
				{
					IntVector2 currentCell = diagInitialCell + new IntVector2(1, -1) * diagCoord;
					exitToTest.additionalExitLength = initialExitOffsets.x + currentCell.x;
					previousExit.additionalExitLength = initialExitOffsets.y + currentCell.y;
					IntVector2 attachPoint = basePositionOfPreviousRoom + previousExit.ExitOrigin - IntVector2.One;
					IntVector2 baseWorldPositionOfNewRoom = attachPoint - (exitToTest.ExitOrigin - IntVector2.One);
					int RectAX1 = baseWorldPositionOfNewRoom.x - 1;
					int RectAX2 = baseWorldPositionOfNewRoom.x + newRoom.Width + 2;
					int RectAY1 = baseWorldPositionOfNewRoom.y - 1;
					int RectAY2 = baseWorldPositionOfNewRoom.y + newRoom.Height + 4;
					bool failedPlacement = false;
					if (!CheckExitsClearForPlacement(newRoom, exitToTest, basePositionOfPreviousRoom, previousExit, attachPoint))
					{
						continue;
					}
					bool broadphaseSuccess = true;
					for (int j = 0; j < m_allRooms.Count; j++)
					{
						CellArea area = m_allRooms[j].area;
						int num = area.basePosition.x - 1;
						int num2 = area.basePosition.x + area.dimensions.x + 2;
						int num3 = area.basePosition.y - 1;
						int num4 = area.basePosition.y + area.dimensions.y + 4;
						if (RectAX1 < num2 && RectAX2 > num && RectAY1 < num4 && RectAY2 > num3)
						{
							broadphaseSuccess = false;
							break;
						}
					}
					if (broadphaseSuccess)
					{
						foreach (IntVector2 exitTestPoint in m_exitTestPoints)
						{
							if (RectAX1 < exitTestPoint.x && RectAX2 > exitTestPoint.x && RectAY1 < exitTestPoint.y && RectAY2 > exitTestPoint.y)
							{
								broadphaseSuccess = false;
								break;
							}
						}
					}
					if (!broadphaseSuccess)
					{
						yield return ProcessStatus.Incomplete;
						List<IntVector2> newRoomPotentialCells = newRoom.GetCellRepresentationIncFacewalls();
						int iterator = 0;
						for (int i = 0; i < newRoomPotentialCells.Count; i++)
						{
							if (failedPlacement)
							{
								break;
							}
							iterator++;
							if (m_occupiedCells.Contains(baseWorldPositionOfNewRoom + newRoomPotentialCells[i]))
							{
								failedPlacement = true;
								break;
							}
							if (iterator % 1000 == 0)
							{
								yield return ProcessStatus.Incomplete;
							}
						}
					}
					if (failedPlacement)
					{
						yield return ProcessStatus.Incomplete;
						continue;
					}
					if (isInterestingCombo)
					{
						exitToTest.jointedExit = true;
						previousExit.jointedExit = true;
					}
					yield return ProcessStatus.Success;
					yield break;
				}
			}
			yield return ProcessStatus.Fail;
		}

		public bool CanPlaceRoomAtAttachPointByExit(PrototypeDungeonRoom newRoom, PrototypeRoomExit exitToTest, IntVector2 attachPoint)
		{
			IntVector2 intVector = attachPoint - (exitToTest.GetExitOrigin(exitToTest.exitLength) - IntVector2.One);
			int num = intVector.x - 1;
			int num2 = intVector.x + newRoom.Width + 2;
			int num3 = intVector.y - 1;
			int num4 = intVector.y + newRoom.Height + 4;
			bool flag = false;
			if (!CheckExitsClearForPlacement(newRoom, new RuntimeRoomExitData(exitToTest), attachPoint))
			{
				return false;
			}
			bool flag2 = true;
			for (int i = 0; i < m_allRooms.Count; i++)
			{
				CellArea area = m_allRooms[i].area;
				int num5 = area.basePosition.x - 1;
				int num6 = area.basePosition.x + area.dimensions.x + 2;
				int num7 = area.basePosition.y - 1;
				int num8 = area.basePosition.y + area.dimensions.y + 4;
				if (num < num6 && num2 > num5 && num3 < num8 && num4 > num7)
				{
					flag2 = false;
					break;
				}
			}
			if (!flag2)
			{
				List<IntVector2> cellRepresentationIncFacewalls = newRoom.GetCellRepresentationIncFacewalls();
				for (int j = 0; j < cellRepresentationIncFacewalls.Count; j++)
				{
					if (flag)
					{
						break;
					}
					if (m_occupiedCells.Contains(intVector + cellRepresentationIncFacewalls[j]))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				return false;
			}
			return true;
		}

		public void ReinitializeFromLayout(SemioticLayoutManager snapshot)
		{
			m_allRooms = new List<RoomHandler>(snapshot.m_allRooms);
			m_occupiedCells = new HashSet<IntVector2>(snapshot.m_occupiedCells);
			m_currentOffset = snapshot.m_currentOffset;
		}

		public void MergeLayout(SemioticLayoutManager other)
		{
			for (int i = 0; i < other.Rooms.Count; i++)
			{
				m_allRooms.Add(other.Rooms[i]);
			}
			foreach (IntVector2 occupiedCell in other.m_occupiedCells)
			{
				m_occupiedCells.Add(other.m_currentOffset + occupiedCell);
			}
			if (m_rectangleDecomposition != null)
			{
				m_rectangleDecomposition.Clear();
			}
		}

		public bool CanPlacePathHallway(List<IntVector2> cellPositions)
		{
			for (int i = 0; i < cellPositions.Count; i++)
			{
				if (m_occupiedCells.Contains(cellPositions[i]))
				{
					return false;
				}
			}
			return true;
		}

		public bool CanPlaceRectangle(IntRect rectangle)
		{
			for (int i = rectangle.Left - 1; i < rectangle.Right + 1; i++)
			{
				for (int j = rectangle.Bottom - 1; j < rectangle.Top + 1; j++)
				{
					IntVector2 item = new IntVector2(i, j);
					if (m_occupiedCells.Contains(item))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool CheckCellAndNeighborsOccupied(IntVector2 position)
		{
			if (m_occupiedCells.Contains(position))
			{
				return true;
			}
			for (int i = 0; i < 4; i++)
			{
				if (m_occupiedCells.Contains(position + IntVector2.Cardinals[i]))
				{
					return true;
				}
			}
			return false;
		}

		public List<IntVector2> PathfindHallway(IntVector2 startPosition, IntVector2 endPosition)
		{
			return PathfindHallwayCompact(startPosition, IntVector2.Zero, endPosition);
		}

		public List<IntVector2> PathfindHallwayCompact(IntVector2 startPosition, IntVector2 startDirection, IntVector2 endPosition)
		{
			IntVector2 intVector = IntVector2.Min(startPosition, endPosition);
			IntVector2 intVector2 = IntVector2.Max(startPosition, endPosition);
			IntVector2 intVector3 = intVector2 - intVector;
			IntVector2 intVector4 = intVector * -1;
			IntVector2 intVector5 = new IntVector2(4, 4);
			int num = intVector3.x + intVector5.x * 2;
			int num2 = intVector3.y + intVector5.y * 2;
			int num3 = Mathf.NextPowerOfTwo(Mathf.Max(num, num2));
			byte[,] array = new byte[num3, num3];
			byte b = 0;
			byte b2 = 1;
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					if (i > num || j > num2)
					{
						array[i, j] = b;
					}
					else
					{
						array[i, j] = b2;
					}
				}
			}
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				int num4 = occupiedCell.x + intVector4.x + intVector5.x;
				int num5 = occupiedCell.y + intVector4.y + intVector5.y;
				if (num4 < 3 || num4 >= num - 3 || num5 < 3 || num5 >= num2 - 3)
				{
					continue;
				}
				for (int k = -3; k < 4; k++)
				{
					for (int l = -3; l < 4; l++)
					{
						array[num4 + k, num5 + l] = b;
					}
				}
			}
			foreach (IntVector2 temporaryPathfindingWall in m_temporaryPathfindingWalls)
			{
				IntVector2 intVector6 = temporaryPathfindingWall + intVector4 + intVector5;
				if (intVector6.x >= 1 && intVector6.x <= array.GetLength(0) - 2 && intVector6.y >= 1 && intVector6.y <= array.GetLength(1) - 4)
				{
					array[intVector6.x, intVector6.y] = b;
				}
			}
			FastDungeonLayoutPathfinder fastDungeonLayoutPathfinder = new FastDungeonLayoutPathfinder(array);
			fastDungeonLayoutPathfinder.Diagonals = false;
			fastDungeonLayoutPathfinder.PunishChangeDirection = true;
			fastDungeonLayoutPathfinder.TieBreaker = true;
			IntVector2 start = startPosition + intVector4 + intVector5;
			IntVector2 end = endPosition + intVector4 + intVector5;
			List<PathFinderNode> list = fastDungeonLayoutPathfinder.FindPath(start, startDirection, end);
			if (list == null || list.Count == 0)
			{
				return null;
			}
			List<IntVector2> list2 = new List<IntVector2>();
			for (int m = 0; m < list.Count; m++)
			{
				IntVector2 item = new IntVector2(list[m].X, list[m].Y) - intVector4 - intVector5;
				list2.Add(item);
			}
			return list2;
		}

		public List<IntVector2> PathfindHallway(IntVector2 startPosition, IntVector2 startDirection, IntVector2 endPosition)
		{
			Tuple<IntVector2, IntVector2> minAndMaxCellPositions = GetMinAndMaxCellPositions();
			IntVector2 intVector = minAndMaxCellPositions.Second - minAndMaxCellPositions.First;
			IntVector2 intVector2 = IntVector2.Max(IntVector2.Zero, IntVector2.Zero - minAndMaxCellPositions.First);
			IntVector2 intVector3 = new IntVector2(8, 8);
			int a = intVector.x + intVector3.x * 2;
			int b = intVector.y + intVector3.y * 2;
			int num = Mathf.NextPowerOfTwo(Mathf.Max(a, b));
			byte[,] array = new byte[num, num];
			byte b2 = 0;
			byte b3 = 1;
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					array[i, j] = b3;
				}
			}
			foreach (IntVector2 occupiedCell in m_occupiedCells)
			{
				int num2 = occupiedCell.x + intVector2.x + intVector3.x;
				int num3 = occupiedCell.y + intVector2.y + intVector3.y;
				for (int k = -3; k < 4; k++)
				{
					for (int l = -3; l < 4; l++)
					{
						array[num2 + k, num3 + l] = b2;
					}
				}
			}
			foreach (IntVector2 temporaryPathfindingWall in m_temporaryPathfindingWalls)
			{
				IntVector2 intVector4 = temporaryPathfindingWall + intVector2 + intVector3;
				if (intVector4.x >= 1 && intVector4.x <= array.GetLength(0) - 2 && intVector4.y >= 1 && intVector4.y <= array.GetLength(1) - 4)
				{
					array[intVector4.x, intVector4.y] = b2;
				}
			}
			FastDungeonLayoutPathfinder fastDungeonLayoutPathfinder = new FastDungeonLayoutPathfinder(array);
			fastDungeonLayoutPathfinder.Diagonals = false;
			fastDungeonLayoutPathfinder.PunishChangeDirection = true;
			fastDungeonLayoutPathfinder.TieBreaker = true;
			IntVector2 start = startPosition + intVector2 + intVector3;
			IntVector2 end = endPosition + intVector2 + intVector3;
			List<PathFinderNode> list = fastDungeonLayoutPathfinder.FindPath(start, startDirection, end);
			if (list == null || list.Count == 0)
			{
				return null;
			}
			List<IntVector2> list2 = new List<IntVector2>();
			for (int m = 0; m < list.Count; m++)
			{
				IntVector2 item = new IntVector2(list[m].X, list[m].Y) - intVector2 - intVector3;
				list2.Add(item);
			}
			return list2;
		}

		public List<IntVector2> TraceHallway(IntVector2 startPosition, IntVector2 endPosition, DungeonData.Direction currentHallwayDirection, DungeonData.Direction endHallwayDirection)
		{
			IntVector2 intVector = startPosition;
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			for (int i = 0; i < 3; i++)
			{
				hashSet.Add(intVector);
				hashSet.Add(intVector + IntVector2.Up);
				hashSet.Add(intVector + IntVector2.Right);
				hashSet.Add(intVector + IntVector2.Up + IntVector2.Right);
				intVector += DungeonData.GetIntVector2FromDirection(currentHallwayDirection);
				hashSet.Add(endPosition);
				hashSet.Add(endPosition + IntVector2.Up);
				hashSet.Add(endPosition + IntVector2.Right);
				hashSet.Add(endPosition + IntVector2.Up + IntVector2.Right);
				endPosition += DungeonData.GetIntVector2FromDirection(endHallwayDirection);
			}
			IntVector2 intVector2 = endPosition - intVector;
			DungeonData.Direction direction = ((intVector2.x <= 0) ? DungeonData.Direction.WEST : DungeonData.Direction.EAST);
			DungeonData.Direction direction2 = ((intVector2.y <= 0) ? DungeonData.Direction.SOUTH : DungeonData.Direction.NORTH);
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(direction);
			IntVector2 intVector2FromDirection2 = DungeonData.GetIntVector2FromDirection(direction2);
			if (currentHallwayDirection != direction && currentHallwayDirection != direction2)
			{
				return null;
			}
			bool flag = true;
			DungeonData.Direction direction3 = currentHallwayDirection;
			int num = 0;
			while (intVector != endPosition && num < 200)
			{
				num++;
				bool flag2 = direction == direction3;
				intVector2 = endPosition - intVector;
				if (flag2)
				{
					bool flag3 = true;
					bool flag4 = true;
					if (intVector2.x == 0 || Mathf.Sign(intVector2.x) != Mathf.Sign(intVector2FromDirection.x))
					{
						flag3 = false;
					}
					else if (CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection) || CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection + IntVector2.Up))
					{
						flag3 = false;
					}
					if (flag3)
					{
						intVector += intVector2FromDirection;
						hashSet.Add(intVector);
						hashSet.Add(intVector + IntVector2.Right);
						hashSet.Add(intVector + IntVector2.Up);
						hashSet.Add(intVector + IntVector2.Right + IntVector2.Up);
						direction3 = direction;
						continue;
					}
					if (intVector2.y == 0 || Mathf.Sign(intVector2.y) != Mathf.Sign(intVector2FromDirection2.y))
					{
						flag4 = false;
					}
					else if (CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection2) || CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection2 + IntVector2.Right))
					{
						flag4 = false;
					}
					if (flag4)
					{
						intVector += intVector2FromDirection2;
						hashSet.Add(intVector);
						hashSet.Add(intVector + IntVector2.Right);
						hashSet.Add(intVector + IntVector2.Up);
						hashSet.Add(intVector + IntVector2.Right + IntVector2.Up);
						direction3 = direction2;
						continue;
					}
				}
				else
				{
					bool flag5 = true;
					bool flag6 = true;
					if (intVector2.y == 0 || Mathf.Sign(intVector2.y) != Mathf.Sign(intVector2FromDirection2.y))
					{
						flag6 = false;
					}
					else if (CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection2) || CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection2 + IntVector2.Right))
					{
						flag6 = false;
					}
					if (flag6)
					{
						intVector += intVector2FromDirection2;
						hashSet.Add(intVector);
						hashSet.Add(intVector + IntVector2.Right);
						hashSet.Add(intVector + IntVector2.Up);
						hashSet.Add(intVector + IntVector2.Right + IntVector2.Up);
						direction3 = direction2;
						continue;
					}
					if (intVector2.x == 0 || Mathf.Sign(intVector2.x) != Mathf.Sign(intVector2FromDirection.x))
					{
						flag5 = false;
					}
					else if (CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection) || CheckCellAndNeighborsOccupied(intVector + intVector2FromDirection + IntVector2.Up))
					{
						flag5 = false;
					}
					if (flag5)
					{
						intVector += intVector2FromDirection;
						hashSet.Add(intVector);
						hashSet.Add(intVector + IntVector2.Right);
						hashSet.Add(intVector + IntVector2.Up);
						hashSet.Add(intVector + IntVector2.Right + IntVector2.Up);
						direction3 = direction;
						continue;
					}
				}
				flag = false;
				break;
			}
			if (num > 10000)
			{
				Debug.LogError("FUCK FUCK FUCK");
			}
			if (flag)
			{
				return hashSet.ToList();
			}
			return null;
		}

		public Dictionary<int, int> DetermineViableExpanse(IntVector2 connectionCenter, DungeonData.Direction direction)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			int num = int.MaxValue;
			for (int i = 1; i <= MAXIMUM_ROOM_DIMENSION; i++)
			{
				int num2 = 0;
				for (int j = 1; j <= MAXIMUM_ROOM_DIMENSION / 2; j++)
				{
					IntVector2 cellToCheck = GetCellToCheck(connectionCenter, i, j, false, direction);
					IntVector2 cellToCheck2 = GetCellToCheck(connectionCenter, i, j, true, direction);
					if (m_occupiedCells.Contains(cellToCheck) || m_occupiedCells.Contains(cellToCheck2))
					{
						if (dictionary.ContainsKey(i - 1))
						{
							dictionary.Remove(i - 1);
						}
						break;
					}
					num2 = j - 1;
				}
				num = Math.Min(num, num2 * 2);
				if (num2 == 0)
				{
					break;
				}
				dictionary.Add(i, num);
			}
			return dictionary;
		}

		private IntVector2 GetCellToCheck(IntVector2 start, int extendMagnitude, int halfWidth, bool invert, DungeonData.Direction dir)
		{
			bool flag = false;
			int x = 0;
			int y = 0;
			switch (dir)
			{
			case DungeonData.Direction.NORTH:
				flag = true;
				y = start.y + extendMagnitude;
				break;
			case DungeonData.Direction.EAST:
				x = start.x + extendMagnitude;
				break;
			case DungeonData.Direction.SOUTH:
				flag = true;
				y = start.y - extendMagnitude;
				break;
			case DungeonData.Direction.WEST:
				x = start.x - extendMagnitude;
				break;
			default:
				Debug.LogError("Switching on invalid direction in SemioticLayoutManager!");
				break;
			}
			if (flag)
			{
				x = ((!invert) ? (start.x - halfWidth) : (start.x + halfWidth));
			}
			else
			{
				y = ((!invert) ? (start.y - halfWidth) : (start.y + halfWidth));
			}
			return new IntVector2(x, y);
		}
	}
}
