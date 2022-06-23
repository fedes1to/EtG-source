using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace Pathfinding
{
	public class Pathfinder : MonoBehaviour
	{
		private struct PathNode : IComparable<PathNode>
		{
			public readonly CellData CellData;

			public IntVector2 Position;

			public int Pass;

			public int ParentId;

			public int Steps;

			public int CombinedWeight;

			public int ActorPathCount;

			public int EstimatedRemainingDist;

			public int FailDist;

			public int SquareClearance;

			public int EstimatedCost
			{
				get
				{
					return CombinedWeight + EstimatedRemainingDist + 2;
				}
			}

			public bool IsOccupied
			{
				get
				{
					return CellData != null && CellData.isOccupied;
				}
			}

			public CellType CellType
			{
				get
				{
					return (CellData == null) ? CellType.WALL : CellData.type;
				}
			}

			public PathNode(CellData cellData, int x, int y)
			{
				CellData = cellData;
				Position = new IntVector2(x, y);
				Pass = 0;
				ParentId = 0;
				Steps = 0;
				CombinedWeight = 0;
				ActorPathCount = 0;
				EstimatedRemainingDist = 0;
				FailDist = 0;
				SquareClearance = 0;
			}

			public int GetWeight(IntVector2 clearance, CellTypes passableCellTypes)
			{
				bool flag = (passableCellTypes & CellTypes.PIT) == CellTypes.PIT;
				bool flag2 = CellData.isOccludedByTopWall;
				bool flag3 = CellData.isNextToWall;
				bool flag4 = !flag && CellData.type == CellType.PIT && !CellData.fallingPrevented;
				if (clearance.x > 1 || clearance.y > 1)
				{
					for (int i = 0; i < clearance.x; i++)
					{
						if (flag2)
						{
							break;
						}
						int num = CellData.position.x + i;
						for (int j = 0; j < clearance.y; j++)
						{
							if (flag2)
							{
								break;
							}
							if (i == 0 && j == 0)
							{
								continue;
							}
							int num2 = CellData.position.y + j;
							if (num >= 0 && num < Instance.m_width && num2 >= 0 && num2 < Instance.m_height)
							{
								CellData cellData = Instance.m_nodes[num + num2 * Instance.m_width].CellData;
								if (cellData.isOccludedByTopWall)
								{
									flag2 = true;
								}
								else if (cellData.isNextToWall)
								{
									flag3 = true;
								}
								if (!flag && cellData.type == CellType.PIT && !cellData.fallingPrevented)
								{
									flag4 = true;
								}
							}
						}
					}
				}
				int num3 = 2 + ActorPathCount;
				if (flag3)
				{
					num3 += 10;
				}
				if (flag2)
				{
					num3 += 2000;
				}
				if (flag4)
				{
					num3 += 10;
				}
				return num3;
			}

			public bool IsPassable(CellTypes passableCellTypes, bool canPassOccupied, CellValidator cellValidator = null)
			{
				if (!canPassOccupied && IsOccupied)
				{
					return false;
				}
				if (((uint)passableCellTypes & (uint)CellType) != (uint)CellType && (CellType != CellType.PIT || !CellData.fallingPrevented || (passableCellTypes & CellTypes.FLOOR) != CellTypes.FLOOR))
				{
					return false;
				}
				if (cellValidator != null && !cellValidator(Position))
				{
					return false;
				}
				return true;
			}

			public bool HasClearance(IntVector2 clearance, CellTypes passableCellTypes, bool canPassOccupied)
			{
				if (clearance.x == clearance.y && passableCellTypes == s_defaultPassableCellTypes && !canPassOccupied)
				{
					return clearance.x <= SquareClearance;
				}
				return Instance.HasRectClearance(CellData.position, clearance, passableCellTypes, canPassOccupied);
			}

			public int CompareTo(PathNode other)
			{
				return EstimatedCost - other.EstimatedCost;
			}
		}

		private struct PathNodeProxy : IComparable<PathNodeProxy>
		{
			public int NodeId;

			public int EstimatedCost;

			public PathNodeProxy(int nodeId, int estimatedCost)
			{
				NodeId = nodeId;
				EstimatedCost = estimatedCost;
			}

			public int CompareTo(PathNodeProxy other)
			{
				return EstimatedCost - other.EstimatedCost;
			}
		}

		[Serializable]
		public class DebugSettings
		{
			public bool DrawGrid;

			public bool DrawImpassable;

			public bool DrawWeights;

			public bool DrawRoomNums;

			public bool DrawPaths;

			public bool TestPath;

			public SpeculativeRigidbody TestPathOrigin;

			public Vector2 TestPathClearance = new Vector2(1f, 1f);
		}

		public static int MaxSteps = 40;

		public DebugSettings Debug;

		private static List<RoomHandler> m_roomHandlers;

		public static readonly CellTypes s_defaultPassableCellTypes = CellTypes.FLOOR;

		public static Pathfinder Instance;

		private const int c_defaultTileWeight = 2;

		private int m_pass;

		private int m_width;

		private int m_height;

		private PathNode[] m_nodes;

		private BinaryHeap<PathNodeProxy> m_openList = new BinaryHeap<PathNodeProxy>();

		private int m_nearestFailDist;

		private int m_nearestFailId;

		private Dictionary<RoomHandler, List<OccupiedCells>> m_registeredObstacles = new Dictionary<RoomHandler, List<OccupiedCells>>();

		private List<RoomHandler> m_dirtyRooms = new List<RoomHandler>();

		public static bool HasInstance
		{
			get
			{
				return Instance != null;
			}
		}

		public static bool CellValidator_NoTopWalls(IntVector2 cellPos)
		{
			CellData cellData = GameManager.Instance.Dungeon.data[cellPos];
			if (cellData != null && cellData.IsTopWall())
			{
				return false;
			}
			return true;
		}

		public void Awake()
		{
			Instance = this;
		}

		public void Update()
		{
			if (GameManager.Instance.PrimaryPlayer != null)
			{
				RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
				if (m_dirtyRooms.Contains(currentRoom))
				{
					RecalculateRoomClearances(currentRoom);
					m_dirtyRooms.Remove(currentRoom);
				}
			}
		}

		public void OnDestroy()
		{
			Instance = null;
		}

		public static void ClearPerLevelData()
		{
			if (m_roomHandlers != null)
			{
				m_roomHandlers = null;
			}
		}

		public void Initialize(DungeonData dungeonData)
		{
			m_width = dungeonData.Width;
			m_height = dungeonData.Height;
			m_nodes = new PathNode[m_width * m_height];
			for (int i = 0; i < m_width; i++)
			{
				for (int j = 0; j < m_height; j++)
				{
					m_nodes[i + j * m_width] = new PathNode(dungeonData.cellData[i][j], i, j);
				}
			}
			RecalculateClearances();
		}

		public void InitializeRegion(DungeonData dungeonData, IntVector2 basePosition, IntVector2 dimensions)
		{
			int width = dungeonData.Width;
			int height = dungeonData.Height;
			PathNode[] array = new PathNode[width * height];
			for (int i = 0; i < m_width; i++)
			{
				for (int j = 0; j < m_height; j++)
				{
					array[i + j * width] = m_nodes[i + j * m_width];
				}
			}
			m_width = width;
			m_height = height;
			m_nodes = array;
			for (int k = basePosition.x - 3; k < basePosition.x + dimensions.x + 4; k++)
			{
				for (int l = basePosition.y - 3; l < basePosition.y + dimensions.y + 4; l++)
				{
					if (k + l * m_width < m_nodes.Length && k < dungeonData.cellData.Length && l < dungeonData.cellData[k].Length)
					{
						m_nodes[k + l * m_width] = new PathNode(dungeonData.cellData[k][l], k, l);
						BraveUtility.DrawDebugSquare(new Vector2(k, l), Color.red, 1000f);
					}
				}
			}
			RecalculateClearances(basePosition.x, basePosition.y, basePosition.x + dimensions.x - 1, basePosition.y + dimensions.y - 1);
		}

		public void RegisterObstacle(OccupiedCells cells, RoomHandler parentRoom)
		{
			if (m_registeredObstacles.ContainsKey(parentRoom))
			{
				m_registeredObstacles[parentRoom].Add(cells);
			}
			else
			{
				List<OccupiedCells> list = new List<OccupiedCells>();
				list.Add(cells);
				m_registeredObstacles.Add(parentRoom, list);
			}
			FlagRoomAsDirty(parentRoom);
		}

		public void DeregisterObstacle(OccupiedCells cells, RoomHandler parentRoom)
		{
			if (m_registeredObstacles.ContainsKey(parentRoom))
			{
				m_registeredObstacles[parentRoom].Remove(cells);
			}
			FlagRoomAsDirty(parentRoom);
		}

		public void FlagRoomAsDirty(RoomHandler room)
		{
			if (!m_dirtyRooms.Contains(room))
			{
				m_dirtyRooms.Add(room);
			}
		}

		public void TryRecalculateRoomClearances(RoomHandler room)
		{
			if (m_dirtyRooms.Contains(room))
			{
				RecalculateRoomClearances(room);
				m_dirtyRooms.Remove(room);
			}
		}

		public void RecalculateRoomClearances(RoomHandler room)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			for (int i = 0; i < room.Cells.Count; i++)
			{
				CellData cellData = data[room.Cells[i]];
				if (cellData != null)
				{
					cellData.isOccupied = false;
				}
			}
			if (m_registeredObstacles.ContainsKey(room))
			{
				List<OccupiedCells> list = m_registeredObstacles[room];
				for (int j = 0; j < list.Count; j++)
				{
					list[j].FlagCells();
				}
			}
			if (m_nodes != null)
			{
				int x = room.area.basePosition.x;
				int y = room.area.basePosition.y;
				int maxX = x + room.area.dimensions.x - 1;
				int maxY = y + room.area.dimensions.y - 1;
				RecalculateClearances(x, y, maxX, maxY);
			}
		}

		public void RecalculateClearances()
		{
			RecalculateClearances(0, 0, m_width - 1, m_height - 1);
		}

		private void RecalculateClearances(int minX, int minY, int maxX, int maxY)
		{
			for (int i = minX; i <= maxX; i++)
			{
				for (int j = minY; j <= maxY; j++)
				{
					int num = i + j * m_width;
					if (!m_nodes[num].IsPassable(s_defaultPassableCellTypes, false))
					{
						m_nodes[num].SquareClearance = 0;
						continue;
					}
					int num2 = Mathf.Max(maxX - i + 1, maxY - j + 1);
					int num3 = 1;
					while (true)
					{
						if (num3 < num2)
						{
							int num4 = 0;
							while (num4 <= num3)
							{
								if (m_nodes[i + num4 + (j + num3) * m_width].IsPassable(s_defaultPassableCellTypes, false))
								{
									num4++;
									continue;
								}
								goto IL_009a;
							}
							int num5 = 0;
							while (num5 < num3)
							{
								if (m_nodes[i + num3 + (j + num5) * m_width].IsPassable(s_defaultPassableCellTypes, false))
								{
									num5++;
									continue;
								}
								goto IL_00f5;
							}
							num3++;
							continue;
						}
						m_nodes[num].SquareClearance = num3;
						break;
						IL_00f5:
						m_nodes[num].SquareClearance = num3;
						break;
						IL_009a:
						m_nodes[num].SquareClearance = num3;
						break;
					}
				}
			}
		}

		public void Smooth(Path path, Vector2 startPos, Vector2 extents, CellTypes passableCellTypes, bool canPassOccupied, IntVector2 clearance)
		{
			if (path.Positions.Count < 2)
			{
				return;
			}
			foreach (IntVector2 position in path.Positions)
			{
				path.PreSmoothedPositions.AddLast(position);
			}
			extents -= Vector2.one * (0.5f / (float)PhysicsEngine.Instance.PixelsPerUnit);
			LinkedListNode<IntVector2> linkedListNode = null;
			LinkedListNode<IntVector2> linkedListNode2 = path.Positions.First;
			int num = 2;
			while (linkedListNode2 != null)
			{
				if (Walkable(startPos, GetClearanceOffset(linkedListNode2.Value, clearance), extents, passableCellTypes, canPassOccupied, clearance, num > 0))
				{
					LinkedListNode<IntVector2> linkedListNode3 = linkedListNode;
					linkedListNode = linkedListNode2;
					linkedListNode2 = linkedListNode2.Next;
					if (linkedListNode3 != null)
					{
						path.Positions.Remove(linkedListNode3);
						path.PreSmoothedPositions.AddLast(linkedListNode3.Value);
					}
					if (!canPassOccupied && m_nodes[GetNodeId(startPos.ToIntVector2(VectorConversions.Floor))].IsOccupied && !m_nodes[GetNodeId(linkedListNode.Value)].IsOccupied)
					{
						linkedListNode2 = linkedListNode;
						break;
					}
					num--;
					continue;
				}
				linkedListNode2 = linkedListNode;
				break;
			}
			if (linkedListNode2 == null && linkedListNode != null)
			{
				return;
			}
			if (linkedListNode == null)
			{
				linkedListNode = path.Positions.First;
			}
			linkedListNode2 = linkedListNode.Next;
			while (linkedListNode2 != null && linkedListNode2.Next != null)
			{
				if (Walkable(GetClearanceOffset(linkedListNode.Value, clearance), GetClearanceOffset(linkedListNode2.Next.Value, clearance), extents, passableCellTypes, canPassOccupied, clearance))
				{
					LinkedListNode<IntVector2> linkedListNode4 = linkedListNode2;
					linkedListNode2 = linkedListNode2.Next;
					path.Positions.Remove(linkedListNode4);
					path.PreSmoothedPositions.AddLast(linkedListNode4.Value);
				}
				else
				{
					linkedListNode = linkedListNode2;
					linkedListNode2 = linkedListNode2.Next;
				}
			}
		}

		public bool GetPath(IntVector2 start, IntVector2 end, out Path path, IntVector2? clearance = null, CellTypes passableCellTypes = CellTypes.FLOOR, CellValidator cellValidator = null, ExtraWeightingFunction extraWeightingFunction = null, bool canPassOccupied = false)
		{
			if (!clearance.HasValue)
			{
				clearance = IntVector2.Zero;
			}
			return GetPath(start, end, passableCellTypes, canPassOccupied, out path, clearance.Value, cellValidator, extraWeightingFunction);
		}

		public void UpdateActorPath(List<IntVector2> path)
		{
			for (int i = 0; i < path.Count; i++)
			{
				m_nodes[path[i].x + path[i].y * m_width].ActorPathCount++;
			}
		}

		public void RemoveActorPath(List<IntVector2> path)
		{
			if (path == null || path.Count == 0)
			{
				return;
			}
			for (int i = 0; i < path.Count; i++)
			{
				m_nodes[path[i].x + path[i].y * m_width].ActorPathCount--;
				if (m_nodes[path[i].x + path[i].y * m_width].ActorPathCount < 0)
				{
					UnityEngine.Debug.LogWarning("Negative ActorPathCount!");
				}
			}
		}

		public bool IsPassable(IntVector2 pos, IntVector2? clearance = null, CellTypes? passableCellTypes = null, bool canPassOccupied = false, CellValidator cellValidator = null)
		{
			if (!clearance.HasValue)
			{
				clearance = IntVector2.One;
			}
			if (!passableCellTypes.HasValue)
			{
				passableCellTypes = (CellTypes)2147483647;
			}
			if (!NodeIsValid(pos.x, pos.y))
			{
				return false;
			}
			return m_nodes[pos.x + pos.y * m_width].HasClearance(clearance.Value, passableCellTypes.Value, canPassOccupied) && m_nodes[pos.x + pos.y * m_width].IsPassable(passableCellTypes.Value, canPassOccupied, cellValidator);
		}

		public bool IsValidPathCell(IntVector2 pos)
		{
			if (!NodeIsValid(pos.x, pos.y))
			{
				return false;
			}
			return m_nodes[pos.x + pos.y * m_width].CellData != null;
		}

		public static Vector2 GetClearanceOffset(IntVector2 pos, IntVector2 clearance)
		{
			return new Vector2((float)pos.x + (float)clearance.x / 2f, (float)pos.y + (float)clearance.y / 2f);
		}

		private bool GetPath(IntVector2 start, IntVector2 goal, CellTypes passableCellTypes, bool canPassOccupied, out Path path, IntVector2 clearance, CellValidator cellValidator = null, ExtraWeightingFunction extraWeightingFunction = null)
		{
			path = null;
			int nodeId = GetNodeId(goal);
			int num = start.x + start.y * m_width;
			if (start == goal)
			{
				path = new Path();
				return true;
			}
			m_pass++;
			m_openList.Clear();
			m_nodes[num].Pass = m_pass;
			m_nodes[num].ParentId = -1;
			m_nodes[num].Steps = 0;
			m_nodes[num].CombinedWeight = m_nodes[num].GetWeight(clearance, passableCellTypes);
			m_nodes[num].EstimatedRemainingDist = IntVector2.ManhattanDistance(m_nodes[num].Position, goal) * 2;
			m_nearestFailDist = m_nodes[num].EstimatedRemainingDist + m_nodes[num].ActorPathCount * 2 * 3;
			m_nearestFailId = num;
			m_openList.Add(new PathNodeProxy(num, m_nodes[num].EstimatedCost));
			while (m_openList.Count > 0)
			{
				int nodeId2 = m_openList.Remove().NodeId;
				if (AtGoal(m_nodes[nodeId2].Position, m_nodes[nodeId].Position, clearance))
				{
					path = RecreatePath(nodeId2, clearance);
					return true;
				}
				if (m_nodes[nodeId2].Steps < MaxSteps)
				{
					IntVector2 position = m_nodes[nodeId2].Position;
					if (position.y < m_height - 1)
					{
						VisitNode(nodeId2, GetNodeId(m_nodes[nodeId2].Position + IntVector2.Up), goal, passableCellTypes, canPassOccupied, clearance, cellValidator, extraWeightingFunction);
					}
					if (position.y > 0)
					{
						VisitNode(nodeId2, GetNodeId(m_nodes[nodeId2].Position + IntVector2.Down), goal, passableCellTypes, canPassOccupied, clearance, cellValidator, extraWeightingFunction);
					}
					if (position.x > 0)
					{
						VisitNode(nodeId2, GetNodeId(m_nodes[nodeId2].Position + IntVector2.Left), goal, passableCellTypes, canPassOccupied, clearance, cellValidator, extraWeightingFunction);
					}
					if (position.x < m_width - 1)
					{
						VisitNode(nodeId2, GetNodeId(m_nodes[nodeId2].Position + IntVector2.Right), goal, passableCellTypes, canPassOccupied, clearance, cellValidator, extraWeightingFunction);
					}
				}
			}
			if (m_nearestFailId != num)
			{
				path = RecreatePath(m_nearestFailId, clearance);
				path.WillReachFinalGoal = false;
				return true;
			}
			return false;
		}

		private int GetNodeId(IntVector2 pos)
		{
			return pos.x + pos.y * m_width;
		}

		private int GetNodeId(int x, int y)
		{
			return x + y * m_width;
		}

		private bool NodeIsValid(int x, int y)
		{
			return x >= 0 && x < m_width && y >= 0 && y < m_height;
		}

		private bool AtGoal(IntVector2 currentPos, IntVector2 goalPos, IntVector2 clearance)
		{
			if (clearance == IntVector2.One)
			{
				return currentPos == goalPos;
			}
			IntVector2 intVector = goalPos - currentPos;
			return intVector.x >= 0 && intVector.y >= 0 && intVector.x < clearance.x && intVector.y < clearance.y;
		}

		private void VisitNode(int prevId, int nodeId, IntVector2 goal, CellTypes passableCellTypes, bool canPassOccupied, IntVector2 clearance, CellValidator cellValidator = null, ExtraWeightingFunction extraWeightingFunction = null)
		{
			if (m_nodes[nodeId].Pass == m_pass || m_nodes[nodeId].CellData == null || !m_nodes[nodeId].IsPassable(passableCellTypes, canPassOccupied, cellValidator) || !m_nodes[nodeId].HasClearance(clearance, passableCellTypes, canPassOccupied))
			{
				return;
			}
			m_nodes[nodeId].Pass = m_pass;
			m_nodes[nodeId].ParentId = prevId;
			m_nodes[nodeId].Steps = m_nodes[prevId].Steps + 1;
			m_nodes[nodeId].CombinedWeight = m_nodes[prevId].CombinedWeight + m_nodes[nodeId].GetWeight(clearance, passableCellTypes);
			m_nodes[nodeId].EstimatedRemainingDist = IntVector2.ManhattanDistance(m_nodes[nodeId].Position, goal) * 2;
			int num = m_nodes[nodeId].EstimatedRemainingDist + m_nodes[nodeId].ActorPathCount * 2 * 3;
			if (extraWeightingFunction != null)
			{
				IntVector2 thisStep = m_nodes[nodeId].Position - m_nodes[prevId].Position;
				IntVector2 prevStep = IntVector2.Zero;
				int parentId = m_nodes[prevId].ParentId;
				if (parentId != -1)
				{
					prevStep = m_nodes[prevId].Position - m_nodes[parentId].Position;
				}
				m_nodes[nodeId].CombinedWeight += extraWeightingFunction(prevStep, thisStep);
			}
			if (num < m_nearestFailDist)
			{
				m_nearestFailId = nodeId;
				m_nearestFailDist = num;
			}
			m_openList.Add(new PathNodeProxy(nodeId, m_nodes[nodeId].EstimatedCost));
		}

		private Path RecreatePath(int destId, IntVector2 clearance)
		{
			LinkedList<IntVector2> linkedList = new LinkedList<IntVector2>();
			for (int num = destId; num >= 0; num = m_nodes[num].ParentId)
			{
				linkedList.AddFirst(m_nodes[num].Position);
			}
			return new Path(linkedList, clearance);
		}

		private bool Walkable(Vector2 start, Vector2 end, Vector2 extents, CellTypes passableCellTypes, bool canPassOccupied, IntVector2 clearance, bool ignoreWeightChecks = false)
		{
			if ((end - start).magnitude < 0.2f)
			{
				return true;
			}
			Vector2 vector = (end - start).normalized / 5f;
			float magnitude = vector.magnitude;
			float num = Vector2.Distance(start, end);
			float num2 = float.MaxValue;
			float num3 = 0f;
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < 4; i++)
			{
				int num4;
				int num5;
				IntVector2 clearance2;
				switch (i)
				{
				case 0:
					num4 = (int)(start.x + extents.x);
					num5 = (int)(start.y + extents.y);
					clearance2 = new IntVector2(1, 1);
					break;
				case 1:
					num4 = (int)(start.x + extents.x);
					num5 = (int)(start.y - extents.y);
					clearance2 = new IntVector2(1, clearance.y);
					break;
				case 2:
					num4 = (int)(start.x - extents.x);
					num5 = (int)(start.y + extents.y);
					clearance2 = new IntVector2(clearance.x, 1);
					break;
				default:
					num4 = (int)(start.x - extents.x);
					num5 = (int)(start.y - extents.y);
					clearance2 = clearance;
					break;
				}
				if (NodeIsValid(num4, num5))
				{
					int num6 = num4 + num5 * m_width;
					num3 = Mathf.Max(num3, m_nodes[num6].GetWeight(clearance2, passableCellTypes));
					if (m_nodes[num6].IsOccupied)
					{
						flag = true;
					}
					if (m_nodes[num6].CellType == CellType.PIT)
					{
						flag2 = true;
					}
				}
			}
			if (num3 > 0f)
			{
				num2 = num3;
			}
			Vector2 vector2 = start;
			while (num >= 0f)
			{
				num3 = 0f;
				bool flag3 = false;
				bool flag4 = false;
				for (int j = 0; j < 4; j++)
				{
					int num4;
					int num5;
					IntVector2 clearance3;
					switch (j)
					{
					case 0:
						num4 = (int)(vector2.x + extents.x);
						num5 = (int)(vector2.y + extents.y);
						clearance3 = new IntVector2(1, 1);
						break;
					case 1:
						num4 = (int)(vector2.x + extents.x);
						num5 = (int)(vector2.y - extents.y);
						clearance3 = new IntVector2(1, clearance.y);
						break;
					case 2:
						num4 = (int)(vector2.x - extents.x);
						num5 = (int)(vector2.y + extents.y);
						clearance3 = new IntVector2(clearance.x, 1);
						break;
					default:
						num4 = (int)(vector2.x - extents.x);
						num5 = (int)(vector2.y - extents.y);
						clearance3 = clearance;
						break;
					}
					int num6 = num4 + num5 * m_width;
					if (!NodeIsValid(num4, num5))
					{
						return false;
					}
					if (!m_nodes[num6].IsPassable((!flag2) ? passableCellTypes : (passableCellTypes | CellTypes.PIT), canPassOccupied || flag))
					{
						return false;
					}
					if (!ignoreWeightChecks && (float)m_nodes[num6].GetWeight(clearance3, passableCellTypes) > num2)
					{
						return false;
					}
					flag3 |= m_nodes[num6].IsOccupied;
					flag4 |= m_nodes[num6].CellType == CellType.PIT;
					num3 = Mathf.Max(num3, m_nodes[num6].GetWeight(clearance, passableCellTypes));
				}
				vector2 += vector;
				num -= magnitude;
				num2 = Mathf.Min(num2, num3);
				flag = flag && flag3;
				flag2 = flag2 && flag4;
			}
			return true;
		}

		private bool HasRectClearance(IntVector2 position, IntVector2 clearance, CellTypes passableCellTypes, bool canPassOccupied)
		{
			for (int i = position.x; i < position.x + clearance.x; i++)
			{
				for (int j = position.y; j < position.y + clearance.y; j++)
				{
					if (i < 0 || i >= m_width || j < 0 || j >= m_height)
					{
						return false;
					}
					if (!m_nodes[i + j * m_width].IsPassable(passableCellTypes, canPassOccupied))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
