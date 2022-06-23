using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeonator
{
	public class LoopBuilderComposite : ArbitraryFlowBuildData
	{
		public enum CompositeStyle
		{
			NON_LOOP,
			LOOP
		}

		protected class CompositeNodeBuildData
		{
			public BuilderFlowNode node;

			public BuilderFlowNode parentNode;

			public RoomHandler parentRoom;

			public IntVector2 parentBasePosition;

			public Tuple<RuntimeRoomExitData, RuntimeRoomExitData> connectionTuple;

			public CompositeNodeBuildData(BuilderFlowNode n, BuilderFlowNode parent, RoomHandler pRoom, IntVector2 pbp)
			{
				node = n;
				parentNode = parent;
				parentRoom = pRoom;
				parentBasePosition = pbp;
			}
		}

		protected const int MIN_PATH_THRESHOLD = 4;

		protected const int MIN_PATH_PHANTOM_THRESHOLD = 10;

		protected const int MAX_LOOP_DISTANCE_THRESHOLD = 30;

		protected const int MAX_PROC_RECTANGLE_AREA = 350;

		protected const int MAX_LOOP_DISTANCE_THRESHOLD_MINES = 50;

		public bool RequiresRegeneration;

		public CompositeStyle loopStyle;

		protected IntVector2 m_dimensions;

		protected List<BuilderFlowNode> m_containedNodes;

		protected Dictionary<BuilderFlowNode, BuilderFlowNode> m_externalToInternalNodeMap;

		protected List<BuilderFlowNode> m_externalConnectedNodes;

		protected LoopFlowBuilder m_owner;

		protected DungeonFlow m_flow;

		protected bool LoopCompositeBuildSuccess;

		private const bool DO_PHANTOM_CORRIDORS = false;

		protected bool LinearCompositeBuildSuccess;

		public SemioticLayoutManager CompletedCanvas;

		public IntVector2 Dimensions
		{
			get
			{
				return m_dimensions;
			}
		}

		public List<BuilderFlowNode> Nodes
		{
			get
			{
				return m_containedNodes;
			}
		}

		public List<BuilderFlowNode> ExternalConnectedNodes
		{
			get
			{
				return m_externalConnectedNodes;
			}
		}

		public LoopBuilderComposite(List<BuilderFlowNode> containedNodes, DungeonFlow flow, LoopFlowBuilder owner, CompositeStyle loop = CompositeStyle.NON_LOOP)
		{
			loopStyle = loop;
			m_owner = owner;
			m_flow = flow;
			m_containedNodes = containedNodes;
			m_externalConnectedNodes = new List<BuilderFlowNode>();
			m_externalToInternalNodeMap = new Dictionary<BuilderFlowNode, BuilderFlowNode>();
			BuilderFlowNode[] excluded = m_containedNodes.ToArray();
			for (int i = 0; i < m_containedNodes.Count; i++)
			{
				BuilderFlowNode builderFlowNode = m_containedNodes[i];
				List<BuilderFlowNode> allConnectedNodes = builderFlowNode.GetAllConnectedNodes(excluded);
				for (int j = 0; j < allConnectedNodes.Count; j++)
				{
					if (!m_externalConnectedNodes.Contains(allConnectedNodes[j]))
					{
						m_externalToInternalNodeMap.Add(allConnectedNodes[j], builderFlowNode);
						m_externalConnectedNodes.Add(allConnectedNodes[j]);
					}
				}
			}
		}

		protected static int GetMaxLoopDistanceThreshold()
		{
			return (GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.MINEGEON && GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.OFFICEGEON) ? 30 : 50;
		}

		public BuilderFlowNode GetConnectedInternalNode(BuilderFlowNode external)
		{
			if (m_externalToInternalNodeMap.ContainsKey(external))
			{
				return m_externalToInternalNodeMap[external];
			}
			return null;
		}

		protected static RoomHandler PlacePhantomRoom(PrototypeDungeonRoom room, SemioticLayoutManager layout, IntVector2 newRoomPosition)
		{
			IntVector2 d = new IntVector2(room.Width, room.Height);
			CellArea cellArea = new CellArea(newRoomPosition, d);
			cellArea.prototypeRoom = room;
			cellArea.instanceUsedExits = new List<PrototypeRoomExit>();
			RoomHandler roomHandler = new RoomHandler(cellArea);
			roomHandler.distanceFromEntrance = 0;
			roomHandler.CalculateOpulence();
			roomHandler.CanReceiveCaps = false;
			layout.StampCellAreaToLayout(roomHandler);
			return roomHandler;
		}

		public static RoomHandler PlaceRoom(BuilderFlowNode current, SemioticLayoutManager layout, IntVector2 newRoomPosition)
		{
			IntVector2 d = new IntVector2(current.assignedPrototypeRoom.Width, current.assignedPrototypeRoom.Height);
			CellArea cellArea = new CellArea(newRoomPosition, d);
			cellArea.prototypeRoom = current.assignedPrototypeRoom;
			cellArea.instanceUsedExits = new List<PrototypeRoomExit>();
			if (current.usesOverrideCategory)
			{
				cellArea.PrototypeRoomCategory = current.overrideCategory;
			}
			RoomHandler roomHandler = new RoomHandler(cellArea);
			roomHandler.distanceFromEntrance = 0;
			roomHandler.CalculateOpulence();
			roomHandler.CanReceiveCaps = current.node.receivesCaps;
			current.instanceRoom = roomHandler;
			if (roomHandler.area.prototypeRoom != null && current.Category == PrototypeDungeonRoom.RoomCategory.SECRET && current.parentBuilderNode != null && current.parentBuilderNode.instanceRoom != null)
			{
				roomHandler.AssignRoomVisualType(current.parentBuilderNode.instanceRoom.RoomVisualSubtype);
			}
			layout.StampCellAreaToLayout(roomHandler);
			return roomHandler;
		}

		public static void RemoveRoom(BuilderFlowNode current, SemioticLayoutManager layout)
		{
			if (current.instanceRoom == null)
			{
				return;
			}
			for (int i = 0; i < layout.Rooms.Count; i++)
			{
				if (layout.Rooms[i].connectedRooms.Contains(current.instanceRoom))
				{
					layout.Rooms[i].DeregisterConnectedRoom(current.instanceRoom, layout.Rooms[i].area.exitToLocalDataMap[layout.Rooms[i].GetExitConnectedToRoom(current.instanceRoom)]);
				}
			}
			layout.StampCellAreaToLayout(current.instanceRoom, true);
			current.instanceRoom = null;
		}

		protected static void CleanupProceduralRoomConnectivity(RoomHandler room, SemioticLayoutManager layout)
		{
			for (int i = 0; i < room.connectedRooms.Count; i++)
			{
				RoomHandler roomHandler = room.connectedRooms[i];
				if (layout.Rooms.Contains(roomHandler))
				{
					continue;
				}
				PrototypeRoomExit prototypeRoomExit = null;
				foreach (PrototypeRoomExit key in room.connectedRoomsByExit.Keys)
				{
					if (room.connectedRoomsByExit[key] == roomHandler)
					{
						prototypeRoomExit = key;
						break;
					}
				}
				if (prototypeRoomExit != null)
				{
					room.area.exitToLocalDataMap.Remove(prototypeRoomExit);
					room.area.instanceUsedExits.Remove(prototypeRoomExit);
					room.connectedRoomsByExit.Remove(prototypeRoomExit);
				}
				room.childRooms.Remove(roomHandler);
				room.connectedRooms.RemoveAt(i);
				i--;
			}
		}

		protected static void FinalizeProceduralRoomConnectivity(RuntimeRoomExitData exitLData, RuntimeRoomExitData exitRData, RoomHandler initialRoom, RoomHandler finalRoom, RoomHandler newProceduralRoom)
		{
			PrototypeRoomExit referencedExit = exitLData.referencedExit;
			PrototypeRoomExit referencedExit2 = exitRData.referencedExit;
			initialRoom.area.instanceUsedExits.Add(referencedExit);
			finalRoom.area.instanceUsedExits.Add(referencedExit2);
			initialRoom.area.exitToLocalDataMap.Add(referencedExit, exitLData);
			finalRoom.area.exitToLocalDataMap.Add(referencedExit2, exitRData);
			newProceduralRoom.parentRoom = initialRoom;
			newProceduralRoom.childRooms.Add(finalRoom);
			newProceduralRoom.connectedRooms.Add(initialRoom);
			newProceduralRoom.connectedRooms.Add(finalRoom);
			initialRoom.childRooms.Add(newProceduralRoom);
			initialRoom.connectedRooms.Add(newProceduralRoom);
			initialRoom.connectedRoomsByExit.Add(referencedExit, newProceduralRoom);
			finalRoom.childRooms.Add(newProceduralRoom);
			finalRoom.connectedRooms.Add(newProceduralRoom);
			finalRoom.connectedRoomsByExit.Add(referencedExit2, newProceduralRoom);
		}

		public static RoomHandler PlaceProceduralPathRoom(IntRect rect, RuntimeRoomExitData exitL, RuntimeRoomExitData exitR, RoomHandler initialRoom, RoomHandler finalRoom, SemioticLayoutManager layout)
		{
			CellArea a = new CellArea(rect.Min, rect.Dimensions);
			RoomHandler roomHandler = new RoomHandler(a);
			roomHandler.distanceFromEntrance = finalRoom.distanceFromEntrance + 1;
			roomHandler.CalculateOpulence();
			layout.StampCellAreaToLayout(roomHandler);
			layout.StampComplexExitToLayout(exitL, initialRoom.area);
			layout.StampComplexExitToLayout(exitR, finalRoom.area);
			FinalizeProceduralRoomConnectivity(exitL, exitR, initialRoom, finalRoom, roomHandler);
			return roomHandler;
		}

		protected static List<IntVector2> ComposeRoomFromPath(List<IntVector2> path, PrototypeRoomExit exitL, PrototypeRoomExit exitR)
		{
			if (path.Count < 2)
			{
				return new List<IntVector2>(path);
			}
			List<List<IntVector2>> list = new List<List<IntVector2>>();
			List<IntVector2> list2 = new List<IntVector2>();
			IntVector2 intVector = path[1] - path[0];
			list2.Add(path[0]);
			for (int i = 1; i < path.Count; i++)
			{
				IntVector2 intVector2 = path[i] - path[i - 1];
				if (intVector2 != intVector)
				{
					intVector = intVector2;
					list.Add(list2);
					list2 = new List<IntVector2>();
					list2.Add(path[i - 1]);
				}
				list2.Add(path[i]);
			}
			list.Add(list2);
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			for (int j = 0; j < list.Count; j++)
			{
				IntVector2 intVector3 = (((list[j][1] - list[j][0]).x == 0) ? IntVector2.Right : IntVector2.Up);
				for (int k = 0; k < list[j].Count; k++)
				{
					if (k == 0 || k == list[j].Count - 1)
					{
						hashSet.Add(list[j][k]);
						hashSet.Add(list[j][k] + IntVector2.Right);
						hashSet.Add(list[j][k] + IntVector2.Up);
						hashSet.Add(list[j][k] + IntVector2.One);
					}
					else
					{
						hashSet.Add(list[j][k]);
						hashSet.Add(list[j][k] + intVector3);
					}
				}
			}
			return hashSet.ToList();
		}

		protected static void ConnectPathToExits(List<IntVector2> inputPath, RuntimeRoomExitData exitL, RuntimeRoomExitData exitR, RoomHandler initialRoom, RoomHandler finalRoom)
		{
			IntVector2 intVector = initialRoom.area.basePosition + exitL.ExitOrigin - IntVector2.One;
			IntVector2 intVector2 = finalRoom.area.basePosition + exitR.ExitOrigin - IntVector2.One;
			if (intVector.x == inputPath[inputPath.Count - 1].x || intVector.y == inputPath[inputPath.Count - 1].y)
			{
				IntVector2 majorAxis = (intVector - inputPath[inputPath.Count - 1]).MajorAxis;
				while (intVector != inputPath[inputPath.Count - 1])
				{
					inputPath.Add(inputPath[inputPath.Count - 1] + majorAxis);
				}
			}
			if (intVector2.x == inputPath[0].x || intVector2.y == inputPath[0].y)
			{
				IntVector2 majorAxis2 = (intVector2 - inputPath[0]).MajorAxis;
				while (intVector2 != inputPath[0])
				{
					inputPath.Insert(0, inputPath[0] + majorAxis2);
				}
			}
		}

		public static RoomHandler PlaceProceduralPathRoom(List<IntVector2> inputPath, RuntimeRoomExitData exitL, RuntimeRoomExitData exitR, RoomHandler initialRoom, RoomHandler finalRoom, SemioticLayoutManager layout)
		{
			IntVector2 intVector = new IntVector2(int.MaxValue, int.MaxValue);
			IntVector2 intVector2 = new IntVector2(int.MinValue, int.MinValue);
			ConnectPathToExits(inputPath, exitL, exitR, initialRoom, finalRoom);
			List<IntVector2> list = ComposeRoomFromPath(inputPath, exitL.referencedExit, exitR.referencedExit);
			for (int i = 0; i < list.Count; i++)
			{
				intVector.x = Math.Min(intVector.x, list[i].x);
				intVector.y = Math.Min(intVector.y, list[i].y);
				intVector2.x = Math.Max(intVector2.x, list[i].x);
				intVector2.y = Math.Max(intVector2.y, list[i].y);
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j] -= intVector;
			}
			CellArea cellArea = new CellArea(intVector, intVector2 - intVector);
			cellArea.proceduralCells = list;
			RoomHandler roomHandler = new RoomHandler(cellArea);
			roomHandler.distanceFromEntrance = finalRoom.distanceFromEntrance + 1;
			roomHandler.CalculateOpulence();
			layout.StampCellAreaToLayout(roomHandler);
			FinalizeProceduralRoomConnectivity(exitL, exitR, initialRoom, finalRoom, roomHandler);
			return roomHandler;
		}

		protected List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> GetNumberOfIdealExitPairs(BuilderFlowNode parentNode, BuilderFlowNode currentNode, IntVector2 previousNodeBasePosition, int numExits)
		{
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairsSimple = GetExitPairsSimple(parentNode, currentNode, previousNodeBasePosition);
			List<PrototypeRoomExit> list = new List<PrototypeRoomExit>();
			for (int i = 0; i < numExits; i++)
			{
				float num = float.MinValue;
				PrototypeRoomExit prototypeRoomExit = null;
				for (int j = 0; j < parentNode.assignedPrototypeRoom.exitData.exits.Count; j++)
				{
					PrototypeRoomExit prototypeRoomExit2 = parentNode.assignedPrototypeRoom.exitData.exits[j];
					if (!parentNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit2) && !list.Contains(prototypeRoomExit2))
					{
						int num2 = 0;
						for (int k = 0; k < parentNode.instanceRoom.area.instanceUsedExits.Count; k++)
						{
							num2 += IntVector2.ManhattanDistance(parentNode.instanceRoom.area.instanceUsedExits[k].GetExitOrigin(0), prototypeRoomExit2.GetExitOrigin(0));
						}
						for (int l = 0; l < list.Count; l++)
						{
							num2 += IntVector2.ManhattanDistance(list[l].GetExitOrigin(0), prototypeRoomExit2.GetExitOrigin(0));
						}
						float num3 = (float)num2 / (float)parentNode.instanceRoom.area.instanceUsedExits.Count;
						if (num3 > num)
						{
							num = num3;
							prototypeRoomExit = prototypeRoomExit2;
						}
					}
				}
				if (prototypeRoomExit != null)
				{
					list.Add(prototypeRoomExit);
					continue;
				}
				break;
			}
			for (int m = 0; m < exitPairsSimple.Count; m++)
			{
				if (!list.Contains(exitPairsSimple[m].First.referencedExit))
				{
					exitPairsSimple.RemoveAt(m);
					m--;
				}
			}
			return exitPairsSimple;
		}

		protected List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> GetExitPairsPreferDistance(BuilderFlowNode parentNode, BuilderFlowNode currentNode, IntVector2 previousNodeBasePosition)
		{
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairsSimple = GetExitPairsSimple(parentNode, currentNode, previousNodeBasePosition);
			if (parentNode.instanceRoom.area.instanceUsedExits.Count < 1)
			{
				return exitPairsSimple;
			}
			Dictionary<PrototypeRoomExit, float> exitsToAverageDistanceMap = new Dictionary<PrototypeRoomExit, float>();
			for (int i = 0; i < parentNode.assignedPrototypeRoom.exitData.exits.Count; i++)
			{
				PrototypeRoomExit prototypeRoomExit = parentNode.assignedPrototypeRoom.exitData.exits[i];
				if (!parentNode.instanceRoom.area.instanceUsedExits.Contains(prototypeRoomExit))
				{
					int num = 0;
					for (int j = 0; j < parentNode.instanceRoom.area.instanceUsedExits.Count; j++)
					{
						num += IntVector2.ManhattanDistance(parentNode.instanceRoom.area.instanceUsedExits[j].GetExitOrigin(0), prototypeRoomExit.GetExitOrigin(0));
					}
					float value = (float)num / (float)parentNode.instanceRoom.area.instanceUsedExits.Count;
					exitsToAverageDistanceMap.Add(prototypeRoomExit, value);
				}
			}
			return exitPairsSimple.OrderByDescending((Tuple<RuntimeRoomExitData, RuntimeRoomExitData> tuple) => exitsToAverageDistanceMap[tuple.First.referencedExit]).ToList();
		}

		protected List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> GetExitPairsSimple(BuilderFlowNode parentNode, BuilderFlowNode currentNode, IntVector2 previousNodeBasePosition)
		{
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> list = new List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>();
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> list2 = new List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>();
			bool flag = true;
			if (parentNode.Category == PrototypeDungeonRoom.RoomCategory.SECRET || currentNode.Category == PrototypeDungeonRoom.RoomCategory.SECRET)
			{
				flag = false;
			}
			if (parentNode.Category == PrototypeDungeonRoom.RoomCategory.BOSS || currentNode.Category == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				flag = false;
			}
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
			{
				flag = true;
			}
			List<PrototypeRoomExit.ExitGroup> definedExitGroups = parentNode.assignedPrototypeRoom.exitData.GetDefinedExitGroups();
			bool flag2 = definedExitGroups.Count > 1;
			for (int i = 0; i < parentNode.instanceRoom.area.instanceUsedExits.Count; i++)
			{
				definedExitGroups.Remove(parentNode.instanceRoom.area.instanceUsedExits[i].exitGroup);
			}
			if (definedExitGroups.Count == 0)
			{
				flag2 = false;
			}
			for (int j = 0; j < parentNode.assignedPrototypeRoom.exitData.exits.Count; j++)
			{
				RuntimeRoomExitData runtimeRoomExitData = new RuntimeRoomExitData(parentNode.assignedPrototypeRoom.exitData.exits[j]);
				if (flag2)
				{
					bool flag3 = false;
					for (int k = 0; k < parentNode.instanceRoom.area.instanceUsedExits.Count; k++)
					{
						if (parentNode.instanceRoom.area.instanceUsedExits[k].exitGroup == runtimeRoomExitData.referencedExit.exitGroup)
						{
							flag3 = true;
							break;
						}
					}
					if (flag3)
					{
						continue;
					}
				}
				for (int l = 0; l < currentNode.assignedPrototypeRoom.exitData.exits.Count; l++)
				{
					RuntimeRoomExitData runtimeRoomExitData2 = new RuntimeRoomExitData(currentNode.assignedPrototypeRoom.exitData.exits[l]);
					if (parentNode.exitToNodeMap.ContainsKey(runtimeRoomExitData.referencedExit) || currentNode.exitToNodeMap.ContainsKey(runtimeRoomExitData2.referencedExit))
					{
						continue;
					}
					if (parentNode.node.childNodeGuids.Contains(currentNode.node.guidAsString))
					{
						if (runtimeRoomExitData.referencedExit.exitType == PrototypeRoomExit.ExitType.ENTRANCE_ONLY || runtimeRoomExitData2.referencedExit.exitType == PrototypeRoomExit.ExitType.EXIT_ONLY)
						{
							continue;
						}
					}
					else if (currentNode.node.childNodeGuids.Contains(parentNode.node.guidAsString) && (runtimeRoomExitData2.referencedExit.exitType == PrototypeRoomExit.ExitType.ENTRANCE_ONLY || runtimeRoomExitData.referencedExit.exitType == PrototypeRoomExit.ExitType.EXIT_ONLY))
					{
						continue;
					}
					if ((runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.EAST && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.WEST) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.WEST && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.EAST) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.NORTH && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.SOUTH) || (runtimeRoomExitData.referencedExit.exitDirection == DungeonData.Direction.SOUTH && runtimeRoomExitData2.referencedExit.exitDirection == DungeonData.Direction.NORTH))
					{
						Tuple<RuntimeRoomExitData, RuntimeRoomExitData> item = new Tuple<RuntimeRoomExitData, RuntimeRoomExitData>(runtimeRoomExitData, runtimeRoomExitData2);
						list.Add(item);
					}
					else if (runtimeRoomExitData.referencedExit.exitDirection != runtimeRoomExitData2.referencedExit.exitDirection)
					{
						Tuple<RuntimeRoomExitData, RuntimeRoomExitData> item2 = new Tuple<RuntimeRoomExitData, RuntimeRoomExitData>(runtimeRoomExitData, runtimeRoomExitData2);
						list2.Add(item2);
					}
				}
			}
			list = list.GenerationShuffle();
			if (flag)
			{
				list.AddRange(list2.GenerationShuffle());
			}
			return list;
		}

		protected List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> GetExitPairsForNode(BuilderFlowNode placedNode, BuilderFlowNode nextNode, IntVector2 previousRoomBasePosition, BuilderFlowNode currentLoopTargetNode, IntVector2 currentLoopTargetBasePosition, RoomHandler currentLoopTargetRoom, List<FlowActionLine> actionLines, bool minimizeLoopDistance)
		{
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairsSimple = GetExitPairsSimple(placedNode, nextNode, previousRoomBasePosition);
			int[] array = new int[exitPairsSimple.Count];
			for (int i = 0; i < exitPairsSimple.Count; i++)
			{
				Tuple<RuntimeRoomExitData, RuntimeRoomExitData> tuple = exitPairsSimple[i];
				IntVector2 intVector = previousRoomBasePosition + tuple.First.ExitOrigin - IntVector2.One;
				IntVector2 intVector2 = intVector - (tuple.Second.ExitOrigin - IntVector2.One);
				int num = int.MaxValue;
				for (int j = 0; j < nextNode.assignedPrototypeRoom.exitData.exits.Count; j++)
				{
					for (int k = 0; k < currentLoopTargetNode.assignedPrototypeRoom.exitData.exits.Count; k++)
					{
						int a2 = 0;
						PrototypeRoomExit prototypeRoomExit = nextNode.assignedPrototypeRoom.exitData.exits[j];
						PrototypeRoomExit prototypeRoomExit2 = currentLoopTargetNode.assignedPrototypeRoom.exitData.exits[k];
						if (prototypeRoomExit != tuple.Second.referencedExit && !nextNode.exitToNodeMap.ContainsKey(prototypeRoomExit) && !currentLoopTargetNode.exitToNodeMap.ContainsKey(prototypeRoomExit2))
						{
							if (minimizeLoopDistance)
							{
								IntVector2 a3 = currentLoopTargetBasePosition + prototypeRoomExit2.GetExitOrigin(prototypeRoomExit2.exitLength) - IntVector2.One;
								IntVector2 b2 = intVector2 + prototypeRoomExit.GetExitOrigin(prototypeRoomExit.exitLength) - IntVector2.One;
								a2 = IntVector2.ManhattanDistance(a3, b2);
							}
							num = Mathf.Min(a2, num);
						}
					}
				}
				array[i] = num;
			}
			if (minimizeLoopDistance)
			{
				List<Tuple<int, Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>> list = new List<Tuple<int, Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>>();
				for (int l = 0; l < exitPairsSimple.Count; l++)
				{
					list.Add(new Tuple<int, Tuple<RuntimeRoomExitData, RuntimeRoomExitData>>(array[l], exitPairsSimple[l]));
				}
				list.Sort((Tuple<int, Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> a, Tuple<int, Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> b) => a.First.CompareTo(b.First));
				for (int m = 0; m < exitPairsSimple.Count; m++)
				{
					exitPairsSimple[m] = list[m].Second;
				}
			}
			return exitPairsSimple;
		}

		protected IEnumerable BuildLoopComposite(SemioticLayoutManager layout, IntVector2 startPosition)
		{
			int numNodes = m_containedNodes.Count;
			BuilderFlowNode previousNodeL = m_containedNodes[0];
			BuilderFlowNode previousNodeR = m_containedNodes[0];
			AcquireRoomIfNecessary(m_containedNodes[0]);
			RoomHandler previousRoomL = PlaceRoom(m_containedNodes[0], layout, startPosition);
			RoomHandler previousRoomR = previousRoomL;
			Guid loopGuid = Guid.NewGuid();
			previousRoomL.IsLoopMember = true;
			previousRoomL.LoopGuid = loopGuid;
			IntVector2 previousRoomLBasePosition = startPosition;
			IntVector2 previousRoomRBasePosition = startPosition;
			List<FlowActionLine> actionLines = new List<FlowActionLine>();
			int roomIndexL = 1;
			int roomIndexR = numNodes - 1;
			while (roomIndexL <= roomIndexR)
			{
				bool shouldMinimizeLoopDistance = ((numNodes <= 3 || (float)roomIndexL > (float)numNodes / 4f) ? true : false);
				BuilderFlowNode nextNodeL = m_containedNodes[roomIndexL];
				BuilderFlowNode nextNodeR = m_containedNodes[roomIndexR];
				if (!AcquireRoomDirectionalIfNecessary(nextNodeL, previousNodeL))
				{
					LoopCompositeBuildSuccess = false;
					yield break;
				}
				if (!AcquireRoomDirectionalIfNecessary(nextNodeR, previousNodeR))
				{
					LoopCompositeBuildSuccess = false;
					yield break;
				}
				List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairsL = GetExitPairsForNode(previousNodeL, nextNodeL, previousRoomLBasePosition, previousNodeR, previousRoomRBasePosition, previousRoomR, actionLines, shouldMinimizeLoopDistance);
				bool success = false;
				for (int i = 0; i < exitPairsL.Count; i++)
				{
					Tuple<RuntimeRoomExitData, RuntimeRoomExitData> tuple = exitPairsL[i];
					if (layout.CanPlaceRoomAtAttachPointByExit2(nextNodeL.assignedPrototypeRoom, tuple.Second, previousRoomLBasePosition, tuple.First))
					{
						IntVector2 intVector = previousRoomLBasePosition + tuple.First.ExitOrigin - IntVector2.One;
						IntVector2 intVector2 = intVector - (tuple.Second.ExitOrigin - IntVector2.One);
						RoomHandler roomHandler = PlaceRoom(nextNodeL, layout, intVector2);
						if (roomHandler != null)
						{
							roomHandler.IsLoopMember = true;
							roomHandler.LoopGuid = loopGuid;
						}
						if ((nextNodeL.loopConnectedBuilderNode == previousNodeL || previousNodeL.loopConnectedBuilderNode == nextNodeL) && (nextNodeL.loopConnectedBuilderNode.node.loopTargetIsOneWay || nextNodeL.node.loopTargetIsOneWay))
						{
							tuple.First.oneWayDoor = true;
							tuple.Second.oneWayDoor = true;
						}
						HandleAdditionalRoomPlacementData(tuple, nextNodeL, previousNodeL, layout);
						FlowActionLine item = new FlowActionLine(roomHandler.GetCenterCell().ToCenterVector2(), previousRoomL.GetCenterCell().ToCenterVector2());
						actionLines.Add(item);
						previousNodeL = nextNodeL;
						previousRoomL = roomHandler;
						previousRoomLBasePosition = intVector2;
						success = true;
						break;
					}
				}
				if (!success)
				{
					BraveUtility.Log("No fitting placements L.", Color.white);
					LoopCompositeBuildSuccess = false;
					yield break;
				}
				yield return null;
				if (roomIndexL != roomIndexR)
				{
					List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairsForNode = GetExitPairsForNode(previousNodeR, nextNodeR, previousRoomRBasePosition, previousNodeL, previousRoomLBasePosition, previousRoomL, actionLines, shouldMinimizeLoopDistance);
					bool flag = false;
					for (int j = 0; j < exitPairsForNode.Count; j++)
					{
						Tuple<RuntimeRoomExitData, RuntimeRoomExitData> tuple2 = exitPairsForNode[j];
						if (layout.CanPlaceRoomAtAttachPointByExit2(nextNodeR.assignedPrototypeRoom, tuple2.Second, previousRoomRBasePosition, tuple2.First))
						{
							IntVector2 intVector3 = previousRoomRBasePosition + tuple2.First.ExitOrigin - IntVector2.One;
							IntVector2 intVector4 = intVector3 - (tuple2.Second.ExitOrigin - IntVector2.One);
							RoomHandler roomHandler2 = PlaceRoom(nextNodeR, layout, intVector4);
							if (roomHandler2 != null)
							{
								roomHandler2.IsLoopMember = true;
								roomHandler2.LoopGuid = loopGuid;
							}
							if ((nextNodeR.loopConnectedBuilderNode == previousNodeR || previousNodeR.loopConnectedBuilderNode == nextNodeR) && (nextNodeR.loopConnectedBuilderNode.node.loopTargetIsOneWay || nextNodeR.node.loopTargetIsOneWay))
							{
								tuple2.First.oneWayDoor = true;
								tuple2.Second.oneWayDoor = true;
							}
							HandleAdditionalRoomPlacementData(tuple2, nextNodeR, previousNodeR, layout);
							FlowActionLine item2 = new FlowActionLine(roomHandler2.GetCenterCell().ToCenterVector2(), previousRoomR.GetCenterCell().ToCenterVector2());
							actionLines.Add(item2);
							previousNodeR = nextNodeR;
							previousRoomR = roomHandler2;
							previousRoomRBasePosition = intVector4;
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						BraveUtility.Log("No fitting placements R.", Color.white);
						LoopCompositeBuildSuccess = false;
						yield break;
					}
				}
				yield return null;
				roomIndexL++;
				roomIndexR--;
			}
			RoomHandler loopRoom = AttemptLoopClosure(layout, previousRoomL, previousRoomR, previousRoomLBasePosition, previousRoomRBasePosition, 0, m_flow);
			if (loopRoom != null)
			{
				loopRoom.IsLoopMember = true;
				loopRoom.LoopGuid = loopGuid;
			}
			if (loopRoom != null)
			{
				LoopCompositeBuildSuccess = true;
			}
			else
			{
				LoopCompositeBuildSuccess = false;
			}
		}

		protected static IntRect GetExitRect(PrototypeRoomExit closestExitL, PrototypeRoomExit closestExitR, IntVector2 closestExitPositionL, IntVector2 closestExitPositionR)
		{
			IntVector2 intVector = IntVector2.Min(closestExitPositionL, closestExitPositionR);
			IntVector2 intVector2 = IntVector2.Max(closestExitPositionL, closestExitPositionR);
			if (closestExitPositionL.x < closestExitPositionR.x)
			{
				if (closestExitL.exitDirection == DungeonData.Direction.EAST)
				{
					intVector += IntVector2.Right;
				}
				if (closestExitR.exitDirection == DungeonData.Direction.NORTH || closestExitR.exitDirection == DungeonData.Direction.SOUTH)
				{
					intVector2 += IntVector2.Right * 2;
				}
			}
			else
			{
				if (closestExitR.exitDirection == DungeonData.Direction.EAST)
				{
					intVector += IntVector2.Right;
				}
				if (closestExitL.exitDirection == DungeonData.Direction.NORTH || closestExitL.exitDirection == DungeonData.Direction.SOUTH)
				{
					intVector2 += IntVector2.Right * 2;
				}
			}
			if (closestExitPositionL.y < closestExitPositionR.y)
			{
				if (closestExitR.exitDirection == DungeonData.Direction.EAST || closestExitR.exitDirection == DungeonData.Direction.WEST)
				{
					intVector2 += IntVector2.Up * 2;
				}
				else if (closestExitR.exitDirection == DungeonData.Direction.SOUTH)
				{
					intVector2 += IntVector2.Up;
				}
			}
			else if (closestExitL.exitDirection == DungeonData.Direction.EAST || closestExitL.exitDirection == DungeonData.Direction.WEST)
			{
				intVector2 += IntVector2.Up * 2;
			}
			else if (closestExitL.exitDirection == DungeonData.Direction.SOUTH)
			{
				intVector2 += IntVector2.Up;
			}
			return new IntRect(intVector.x, intVector.y, intVector2.x - intVector.x, intVector2.y - intVector.y);
		}

		protected static RoomHandler AttemptLoopClosure(SemioticLayoutManager layout, RoomHandler previousRoomL, RoomHandler previousRoomR, IntVector2 previousRoomLBasePosition, IntVector2 previousRoomRBasePosition, int depth, DungeonFlow flow)
		{
			List<Tuple<PrototypeRoomExit, PrototypeRoomExit>> list = new List<Tuple<PrototypeRoomExit, PrototypeRoomExit>>();
			List<int> list2 = new List<int>();
			List<PrototypeRoomExit.ExitGroup> definedExitGroups = previousRoomL.area.prototypeRoom.exitData.GetDefinedExitGroups();
			bool flag = definedExitGroups.Count > 1;
			for (int i = 0; i < previousRoomL.area.instanceUsedExits.Count; i++)
			{
				definedExitGroups.Remove(previousRoomL.area.instanceUsedExits[i].exitGroup);
			}
			if (definedExitGroups.Count == 0)
			{
				flag = false;
			}
			List<PrototypeRoomExit.ExitGroup> definedExitGroups2 = previousRoomR.area.prototypeRoom.exitData.GetDefinedExitGroups();
			bool flag2 = definedExitGroups2.Count > 1;
			for (int j = 0; j < previousRoomR.area.instanceUsedExits.Count; j++)
			{
				definedExitGroups2.Remove(previousRoomR.area.instanceUsedExits[j].exitGroup);
			}
			if (definedExitGroups2.Count == 0)
			{
				flag2 = false;
			}
			for (int k = 0; k < previousRoomL.area.prototypeRoom.exitData.exits.Count; k++)
			{
				PrototypeRoomExit prototypeRoomExit = previousRoomL.area.prototypeRoom.exitData.exits[k];
				if (flag)
				{
					bool flag3 = false;
					for (int l = 0; l < previousRoomL.area.instanceUsedExits.Count; l++)
					{
						if (previousRoomL.area.instanceUsedExits[l].exitGroup == prototypeRoomExit.exitGroup)
						{
							flag3 = true;
							break;
						}
					}
					if (flag3)
					{
						continue;
					}
				}
				for (int m = 0; m < previousRoomR.area.prototypeRoom.exitData.exits.Count; m++)
				{
					PrototypeRoomExit prototypeRoomExit2 = previousRoomR.area.prototypeRoom.exitData.exits[m];
					if (flag2)
					{
						bool flag4 = false;
						for (int n = 0; n < previousRoomR.area.instanceUsedExits.Count; n++)
						{
							if (previousRoomR.area.instanceUsedExits[n].exitGroup == prototypeRoomExit2.exitGroup)
							{
								flag4 = true;
								break;
							}
						}
						if (flag4)
						{
							continue;
						}
					}
					if (!previousRoomL.area.instanceUsedExits.Contains(prototypeRoomExit) && !previousRoomR.area.instanceUsedExits.Contains(prototypeRoomExit2))
					{
						IntVector2 a = previousRoomLBasePosition + prototypeRoomExit.GetExitOrigin(prototypeRoomExit.exitLength + 3) - IntVector2.One;
						IntVector2 b = previousRoomRBasePosition + prototypeRoomExit2.GetExitOrigin(prototypeRoomExit2.exitLength + 3) - IntVector2.One;
						int item = IntVector2.ManhattanDistance(a, b);
						list.Add(new Tuple<PrototypeRoomExit, PrototypeRoomExit>(prototypeRoomExit, prototypeRoomExit2));
						list2.Add(item);
					}
				}
			}
			List<Tuple<PrototypeRoomExit, PrototypeRoomExit>> list3 = (from v in list2.Zip(list, (int d, Tuple<PrototypeRoomExit, PrototypeRoomExit> p) => new
				{
					Dist = d,
					Pair = p
				})
				orderby v.Dist
				select v.Pair).ToList();
			RuntimeRoomExitData closestExitL = null;
			RuntimeRoomExitData closestExitR = null;
			List<IntVector2> list4 = null;
			int num = int.MaxValue;
			for (int num2 = 0; num2 < list3.Count; num2++)
			{
				PrototypeRoomExit first = list3[num2].First;
				PrototypeRoomExit second = list3[num2].Second;
				IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(first.exitDirection);
				IntVector2 intVector2FromDirection2 = DungeonData.GetIntVector2FromDirection(second.exitDirection);
				int num3 = ((first.exitDirection != DungeonData.Direction.SOUTH && first.exitDirection != DungeonData.Direction.WEST) ? 3 : 2);
				int num4 = ((second.exitDirection != DungeonData.Direction.SOUTH && second.exitDirection != DungeonData.Direction.WEST) ? 3 : 2);
				IntVector2 intVector = previousRoomLBasePosition + first.GetExitOrigin(first.exitLength + num3) - IntVector2.One;
				IntVector2 intVector2 = previousRoomRBasePosition + second.GetExitOrigin(second.exitLength + num4) - IntVector2.One;
				if (IntVector2.ManhattanDistance(intVector, intVector2) >= GetMaxLoopDistanceThreshold())
				{
					continue;
				}
				RuntimeRoomExitData runtimeRoomExitData = new RuntimeRoomExitData(first);
				runtimeRoomExitData.additionalExitLength = 3;
				RuntimeRoomExitData runtimeRoomExitData2 = new RuntimeRoomExitData(second);
				runtimeRoomExitData2.additionalExitLength = 3;
				IntRect exitRect = GetExitRect(first, second, intVector, intVector2);
				bool flag5 = exitRect.Width > 6 && exitRect.Height > 6 && (exitRect.Width > 12 || exitRect.Height > 12) && exitRect.Area < 350 && exitRect.Aspect < 5f && exitRect.Aspect > 0.2f;
				if (intVector2FromDirection == intVector2FromDirection2)
				{
					flag5 = false;
				}
				RuntimeRoomExitData runtimeRoomExitData3 = new RuntimeRoomExitData(first);
				runtimeRoomExitData3.additionalExitLength = 1;
				RuntimeRoomExitData runtimeRoomExitData4 = new RuntimeRoomExitData(second);
				runtimeRoomExitData4.additionalExitLength = 1;
				layout.StampComplexExitTemporary(runtimeRoomExitData3, previousRoomL.area);
				layout.StampComplexExitTemporary(runtimeRoomExitData4, previousRoomR.area);
				if (flag5 && layout.CanPlaceRectangle(exitRect))
				{
					IntVector2 intVector3 = intVector2FromDirection + intVector2FromDirection2;
					IntRect rect = exitRect;
					for (int num5 = 0; num5 < 5; num5++)
					{
						int num6 = ((intVector3.x < 0) ? num5 : 0);
						int num7 = ((intVector3.x > 0) ? num5 : 0);
						int num8 = ((intVector3.y < 0) ? num5 : 0);
						int num9 = ((intVector3.y > 0) ? num5 : 0);
						if (intVector3 == IntVector2.Zero)
						{
							if (intVector2FromDirection.y == 0 && intVector2FromDirection2.y == 0)
							{
								num8 = num5;
								num9 = num5;
							}
							else
							{
								num6 = num5;
								num7 = num5;
							}
						}
						IntRect intRect = new IntRect(exitRect.Left - num6, exitRect.Bottom - num8, exitRect.Width + num6 + num7, exitRect.Height + num8 + num9);
						if (intRect.Area < 350 && intRect.Aspect < 5f && intRect.Aspect > 0.2f && layout.CanPlaceRectangle(intRect))
						{
							rect = intRect;
						}
					}
					layout.ClearTemporary();
					return PlaceProceduralPathRoom(rect, runtimeRoomExitData, runtimeRoomExitData2, previousRoomL, previousRoomR, layout);
				}
				layout.ClearTemporary();
				List<IntVector2> list5 = layout.PathfindHallwayCompact(intVector, DungeonData.GetIntVector2FromDirection(first.exitDirection), intVector2);
				layout.ClearTemporary();
				if (list5 != null && !layout.CanPlaceRawCellPositions(list5))
				{
					list5 = null;
				}
				if (list5 != null && list5.Count > 0 && list5.Count < num)
				{
					runtimeRoomExitData.additionalExitLength = 0;
					runtimeRoomExitData2.additionalExitLength = 0;
					closestExitL = runtimeRoomExitData;
					closestExitR = runtimeRoomExitData2;
					list4 = list5;
					num = list4.Count;
				}
			}
			if (num > GetMaxLoopDistanceThreshold())
			{
				return null;
			}
			if (list4 != null && list4.Count > 0)
			{
				return ConstructPhantomCorridor(list4, closestExitL, closestExitR, previousRoomL, previousRoomR, layout, depth, flow);
			}
			return null;
		}

		protected static RoomHandler ConstructPhantomCorridor(List<IntVector2> path, RuntimeRoomExitData closestExitL, RuntimeRoomExitData closestExitR, RoomHandler previousRoomL, RoomHandler previousRoomR, SemioticLayoutManager layout, int depth, DungeonFlow flow)
		{
			if (path.Count < 4)
			{
				return null;
			}
			return PlaceProceduralPathRoom(path, closestExitL, closestExitR, previousRoomL, previousRoomR, layout);
		}

		protected IEnumerable BuildCompositeDepthFirst(SemioticLayoutManager layout, IntVector2 startPosition)
		{
			BuilderFlowNode currentNode = m_containedNodes[0];
			AcquireRoomIfNecessary(currentNode);
			RoomHandler room = PlaceRoom(currentNode, layout, startPosition);
			bool success = true;
			for (int i = 0; i < currentNode.childBuilderNodes.Count; i++)
			{
				BuilderFlowNode childNode = currentNode.childBuilderNodes[i];
				if (m_containedNodes.Contains(childNode))
				{
					CompositeNodeBuildData currentBuildData = new CompositeNodeBuildData(childNode, currentNode, room, startPosition);
					IEnumerator<ProcessStatus> enumerator = BuildCompositeNodeDepthFirst(layout, currentBuildData).GetEnumerator();
					success = false;
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == ProcessStatus.Success)
						{
							success = true;
							break;
						}
						if (enumerator.Current == ProcessStatus.Fail)
						{
							success = false;
							break;
						}
					}
				}
				if (!success)
				{
					break;
				}
				yield return null;
			}
			if (!success)
			{
				RemoveRoom(currentNode, layout);
			}
			LinearCompositeBuildSuccess = success;
		}

		protected bool BuildComposite(SemioticLayoutManager layout, IntVector2 startPosition)
		{
			BuilderFlowNode builderFlowNode = m_containedNodes[0];
			AcquireRoomIfNecessary(builderFlowNode);
			RoomHandler pRoom = PlaceRoom(builderFlowNode, layout, startPosition);
			Queue<CompositeNodeBuildData> queue = new Queue<CompositeNodeBuildData>();
			for (int i = 0; i < builderFlowNode.childBuilderNodes.Count; i++)
			{
				BuilderFlowNode builderFlowNode2 = builderFlowNode.childBuilderNodes[i];
				if (m_containedNodes.Contains(builderFlowNode2))
				{
					queue.Enqueue(new CompositeNodeBuildData(builderFlowNode2, builderFlowNode, pRoom, startPosition));
				}
			}
			bool flag = true;
			while (queue.Count > 0)
			{
				CompositeNodeBuildData currentBuildData = queue.Dequeue();
				flag = BuildCompositeNode(layout, currentBuildData, queue);
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}

		protected bool AcquireRoomDirectionalIfNecessary(BuilderFlowNode buildNode, BuilderFlowNode parentNode)
		{
			if (buildNode.AcquiresRoomAsNecessary)
			{
				PrototypeDungeonRoom.RoomCategory category = ((!buildNode.usesOverrideCategory) ? buildNode.node.roomCategory : buildNode.overrideCategory);
				GenericRoomTable genericRoomTable = m_flow.fallbackRoomTable;
				if (buildNode.node.overrideRoomTable != null)
				{
					genericRoomTable = buildNode.node.overrideRoomTable;
				}
				m_owner.ClearPlacedRoomData(buildNode);
				PrototypeDungeonRoom assignedPrototypeRoom = parentNode.assignedPrototypeRoom;
				List<DungeonData.Direction> list = new List<DungeonData.Direction>();
				for (int i = 0; i < assignedPrototypeRoom.exitData.exits.Count; i++)
				{
					if (!parentNode.exitToNodeMap.ContainsKey(assignedPrototypeRoom.exitData.exits[i]))
					{
						DungeonData.Direction item = (DungeonData.Direction)((int)(assignedPrototypeRoom.exitData.exits[i].exitDirection + 4) % 8);
						list.Add(item);
					}
				}
				PrototypeDungeonRoom availableRoomByExitDirection = m_owner.GetAvailableRoomByExitDirection(category, buildNode.Connectivity, list, genericRoomTable.GetCompiledList());
				if (availableRoomByExitDirection != null)
				{
					buildNode.assignedPrototypeRoom = availableRoomByExitDirection;
					m_owner.NotifyPlacedRoomData(availableRoomByExitDirection);
					return true;
				}
				Debug.LogError("Failed to acquire a prototype room. This means the list is too sparse for the relevant category (" + category.ToString() + ") or something has gone terribly wrong. We should be falling back gracefully, though.");
				return false;
			}
			return true;
		}

		protected void AcquireRoomIfNecessary(BuilderFlowNode buildNode)
		{
			if (buildNode.AcquiresRoomAsNecessary)
			{
				PrototypeDungeonRoom.RoomCategory category = ((!buildNode.usesOverrideCategory) ? buildNode.node.roomCategory : buildNode.overrideCategory);
				GenericRoomTable genericRoomTable = m_flow.fallbackRoomTable;
				if (buildNode.node.overrideRoomTable != null)
				{
					genericRoomTable = buildNode.node.overrideRoomTable;
				}
				m_owner.ClearPlacedRoomData(buildNode);
				PrototypeDungeonRoom availableRoom = m_owner.GetAvailableRoom(category, buildNode.Connectivity, genericRoomTable.GetCompiledList());
				if (availableRoom != null)
				{
					buildNode.assignedPrototypeRoom = availableRoom;
					m_owner.NotifyPlacedRoomData(availableRoom);
				}
				else
				{
					Debug.LogError("Failed to acquire a prototype room. This means the list is too sparse for the relevant category (" + category.ToString() + ") or something has gone terribly wrong.");
				}
			}
		}

		protected static void HandleAdditionalRoomPlacementData(Tuple<RuntimeRoomExitData, RuntimeRoomExitData> exitPair, BuilderFlowNode nextNode, BuilderFlowNode previousNode, SemioticLayoutManager layout)
		{
			if (previousNode.nodeToExitMap.ContainsKey(nextNode))
			{
				previousNode.nodeToExitMap.Remove(nextNode);
			}
			if (nextNode.nodeToExitMap.ContainsKey(previousNode))
			{
				nextNode.nodeToExitMap.Remove(previousNode);
			}
			previousNode.exitToNodeMap.Add(exitPair.First.referencedExit, nextNode);
			previousNode.nodeToExitMap.Add(nextNode, exitPair.First.referencedExit);
			nextNode.exitToNodeMap.Add(exitPair.Second.referencedExit, previousNode);
			nextNode.nodeToExitMap.Add(previousNode, exitPair.Second.referencedExit);
			layout.StampComplexExitToLayout(exitPair.Second, nextNode.instanceRoom.area);
			layout.StampComplexExitToLayout(exitPair.First, previousNode.instanceRoom.area);
			exitPair.First.linkedExit = exitPair.Second;
			exitPair.Second.linkedExit = exitPair.First;
			if ((previousNode.parentBuilderNode == nextNode && previousNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.ONE_WAY) || (nextNode.parentBuilderNode == previousNode && nextNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.ONE_WAY))
			{
				exitPair.First.oneWayDoor = true;
				exitPair.Second.oneWayDoor = true;
			}
			if ((previousNode.parentBuilderNode == nextNode && previousNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.LOCKED) || (nextNode.parentBuilderNode == previousNode && nextNode.node.forcedDoorType == DungeonFlowNode.ForcedDoorType.LOCKED))
			{
				exitPair.First.isLockedDoor = true;
				exitPair.Second.isLockedDoor = true;
			}
			previousNode.instanceRoom.RegisterConnectedRoom(nextNode.instanceRoom, exitPair.First);
			nextNode.instanceRoom.RegisterConnectedRoom(previousNode.instanceRoom, exitPair.Second);
		}

		protected static void UnhandleAdditionalRoomPlacementData(Tuple<RuntimeRoomExitData, RuntimeRoomExitData> exitPair, BuilderFlowNode nextNode, BuilderFlowNode previousNode, SemioticLayoutManager layout)
		{
			previousNode.exitToNodeMap.Remove(exitPair.First.referencedExit);
			previousNode.nodeToExitMap.Remove(nextNode);
			nextNode.exitToNodeMap.Remove(exitPair.Second.referencedExit);
			nextNode.nodeToExitMap.Remove(previousNode);
			layout.StampComplexExitToLayout(exitPair.Second, nextNode.instanceRoom.area, true);
			layout.StampComplexExitToLayout(exitPair.First, previousNode.instanceRoom.area, true);
			exitPair.First.linkedExit = null;
			exitPair.Second.linkedExit = null;
			exitPair.First.oneWayDoor = false;
			exitPair.Second.oneWayDoor = false;
			exitPair.First.isLockedDoor = false;
			exitPair.Second.isLockedDoor = false;
			previousNode.instanceRoom.DeregisterConnectedRoom(nextNode.instanceRoom, exitPair.First);
			nextNode.instanceRoom.DeregisterConnectedRoom(previousNode.instanceRoom, exitPair.Second);
		}

		protected IEnumerable<ProcessStatus> BuildCompositeNodeDepthFirst(SemioticLayoutManager layout, CompositeNodeBuildData currentBuildData)
		{
			if (!AcquireRoomDirectionalIfNecessary(currentBuildData.node, currentBuildData.parentNode))
			{
				yield return ProcessStatus.Fail;
				yield break;
			}
			if (currentBuildData.node.assignedPrototypeRoom == null && currentBuildData.node.node.priority == DungeonFlowNode.NodePriority.OPTIONAL)
			{
				yield return ProcessStatus.Success;
				yield break;
			}
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairs = GetExitPairsPreferDistance(currentBuildData.parentNode, currentBuildData.node, currentBuildData.parentBasePosition);
			BuilderFlowNode nextNode = currentBuildData.node;
			BuilderFlowNode previousNode = currentBuildData.parentNode;
			int numberChildFailures = 0;
			bool success = false;
			for (int i = 0; i < exitPairs.Count; i++)
			{
				Tuple<RuntimeRoomExitData, RuntimeRoomExitData> exitPair = exitPairs[i];
				IEnumerator<ProcessStatus> AttachTracker = layout.CanPlaceRoomAtAttachPointByExit(nextNode.assignedPrototypeRoom, exitPair.Second, currentBuildData.parentBasePosition, exitPair.First).GetEnumerator();
				bool attachSuccess = false;
				while (AttachTracker.MoveNext())
				{
					switch (AttachTracker.Current)
					{
					case ProcessStatus.Success:
						attachSuccess = true;
						break;
					case ProcessStatus.Fail:
						attachSuccess = false;
						break;
					default:
						yield return ProcessStatus.Incomplete;
						break;
					}
				}
				if (!attachSuccess)
				{
					continue;
				}
				success = true;
				IntVector2 attachPoint = currentBuildData.parentBasePosition + exitPair.First.ExitOrigin - IntVector2.One;
				IntVector2 baseWorldPositionOfNewRoom = attachPoint - (exitPair.Second.ExitOrigin - IntVector2.One);
				RoomHandler newRoom = PlaceRoom(nextNode, layout, baseWorldPositionOfNewRoom);
				if (newRoom.IsSecretRoom)
				{
					newRoom.AssignRoomVisualType(previousNode.instanceRoom.RoomVisualSubtype);
				}
				HandleAdditionalRoomPlacementData(exitPair, nextNode, previousNode, layout);
				currentBuildData.connectionTuple = exitPair;
				List<CompositeNodeBuildData> successfulChildren = new List<CompositeNodeBuildData>();
				for (int k = 0; k < nextNode.childBuilderNodes.Count; k++)
				{
					BuilderFlowNode childNode = nextNode.childBuilderNodes[k];
					if (!m_containedNodes.Contains(childNode))
					{
						continue;
					}
					CompositeNodeBuildData childBuildData2 = new CompositeNodeBuildData(childNode, nextNode, newRoom, baseWorldPositionOfNewRoom);
					IEnumerator<ProcessStatus> RecursiveTracker = BuildCompositeNodeDepthFirst(layout, childBuildData2).GetEnumerator();
					while (RecursiveTracker.MoveNext())
					{
						switch (RecursiveTracker.Current)
						{
						case ProcessStatus.Fail:
							success = false;
							numberChildFailures++;
							break;
						case ProcessStatus.Success:
							success = true;
							break;
						default:
							yield return ProcessStatus.Incomplete;
							continue;
						}
						break;
					}
					yield return ProcessStatus.Incomplete;
					if (!success)
					{
						break;
					}
					successfulChildren.Add(childBuildData2);
				}
				if (success)
				{
					break;
				}
				numberChildFailures++;
				for (int j = 0; j < successfulChildren.Count; j++)
				{
					CompositeNodeBuildData childBuildData = successfulChildren[j];
					if (!(childBuildData.node.assignedPrototypeRoom == null) || childBuildData.node.node.priority != DungeonFlowNode.NodePriority.OPTIONAL)
					{
						UnhandleAdditionalRoomPlacementData(childBuildData.connectionTuple, childBuildData.node, childBuildData.parentNode, layout);
						RemoveRoom(childBuildData.node, layout);
						yield return ProcessStatus.Incomplete;
					}
				}
				UnhandleAdditionalRoomPlacementData(exitPair, nextNode, previousNode, layout);
				RemoveRoom(currentBuildData.node, layout);
				if (numberChildFailures > 3)
				{
					yield return ProcessStatus.Fail;
					break;
				}
			}
			if (success)
			{
				yield return ProcessStatus.Success;
			}
			else
			{
				yield return ProcessStatus.Fail;
			}
		}

		protected bool BuildCompositeNode(SemioticLayoutManager layout, CompositeNodeBuildData currentBuildData, Queue<CompositeNodeBuildData> buildQueue)
		{
			if (!AcquireRoomDirectionalIfNecessary(currentBuildData.node, currentBuildData.parentNode))
			{
				return false;
			}
			List<Tuple<RuntimeRoomExitData, RuntimeRoomExitData>> exitPairsSimple = GetExitPairsSimple(currentBuildData.parentNode, currentBuildData.node, currentBuildData.parentBasePosition);
			BuilderFlowNode node = currentBuildData.node;
			BuilderFlowNode parentNode = currentBuildData.parentNode;
			for (int i = 0; i < exitPairsSimple.Count; i++)
			{
				Tuple<RuntimeRoomExitData, RuntimeRoomExitData> tuple = exitPairsSimple[i];
				IEnumerator<ProcessStatus> enumerator = layout.CanPlaceRoomAtAttachPointByExit(node.assignedPrototypeRoom, tuple.Second, currentBuildData.parentBasePosition, tuple.First).GetEnumerator();
				bool flag = false;
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current)
					{
					case ProcessStatus.Success:
						flag = true;
						break;
					case ProcessStatus.Fail:
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				IntVector2 intVector = currentBuildData.parentBasePosition + tuple.First.ExitOrigin - IntVector2.One;
				IntVector2 intVector2 = intVector - (tuple.Second.ExitOrigin - IntVector2.One);
				RoomHandler pRoom = PlaceRoom(node, layout, intVector2);
				HandleAdditionalRoomPlacementData(tuple, node, parentNode, layout);
				for (int j = 0; j < node.childBuilderNodes.Count; j++)
				{
					BuilderFlowNode builderFlowNode = node.childBuilderNodes[j];
					if (m_containedNodes.Contains(builderFlowNode))
					{
						CompositeNodeBuildData item = new CompositeNodeBuildData(builderFlowNode, node, pRoom, intVector2);
						buildQueue.Enqueue(item);
					}
				}
				return true;
			}
			return false;
		}

		protected void PostprocessLoopDirectionality()
		{
			bool loopIsUnidirectional = false;
			for (int i = 0; i < m_containedNodes.Count; i++)
			{
				if (m_containedNodes[i].loopConnectedBuilderNode != null && m_containedNodes.Contains(m_containedNodes[i].loopConnectedBuilderNode) && m_containedNodes[i].node.loopTargetIsOneWay)
				{
					loopIsUnidirectional = true;
				}
			}
			for (int j = 0; j < m_containedNodes.Count; j++)
			{
				if (m_containedNodes[j].instanceRoom != null)
				{
					m_containedNodes[j].instanceRoom.LoopIsUnidirectional = loopIsUnidirectional;
				}
			}
		}

		public IEnumerable Build(IntVector2 startPosition)
		{
			if (CompletedCanvas != null)
			{
				CompletedCanvas.OnDestroy();
			}
			CompletedCanvas = null;
			SemioticLayoutManager canvas = new SemioticLayoutManager();
			if (loopStyle == CompositeStyle.LOOP)
			{
				LoopCompositeBuildSuccess = false;
				IEnumerator buildTracker2 = BuildLoopComposite(canvas, startPosition).GetEnumerator();
				while (buildTracker2.MoveNext())
				{
					yield return null;
				}
				if (LoopCompositeBuildSuccess)
				{
					PostprocessLoopDirectionality();
				}
				RequiresRegeneration = !LoopCompositeBuildSuccess;
			}
			else if (loopStyle == CompositeStyle.NON_LOOP)
			{
				LinearCompositeBuildSuccess = false;
				IEnumerator buildTracker = BuildCompositeDepthFirst(canvas, startPosition).GetEnumerator();
				while (buildTracker.MoveNext())
				{
					yield return null;
				}
				RequiresRegeneration = !LinearCompositeBuildSuccess;
			}
			CompletedCanvas = canvas;
		}
	}
}
