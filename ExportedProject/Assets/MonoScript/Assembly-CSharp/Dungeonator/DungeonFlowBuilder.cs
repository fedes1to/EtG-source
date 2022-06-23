using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeonator
{
	public class DungeonFlowBuilder
	{
		private struct FlowRoomAttachData
		{
			public WeightedRoom weightedRoom;

			public PrototypeRoomExit exitOfNewRoom;

			public PrototypeRoomExit exitToUse;

			public FlowRoomAttachData(WeightedRoom w, PrototypeRoomExit exitOfNew, PrototypeRoomExit exitOfOld)
			{
				weightedRoom = w;
				exitOfNewRoom = exitOfNew;
				exitToUse = exitOfOld;
			}
		}

		internal class LoopPathData
		{
			public List<IntVector2> path;

			public PrototypeRoomExit initialExit;

			public PrototypeRoomExit finalExit;

			public LoopPathData(List<IntVector2> path, PrototypeRoomExit initialExit, PrototypeRoomExit finalExit)
			{
				this.path = path;
				this.initialExit = initialExit;
				this.finalExit = finalExit;
			}
		}

		public List<RoomHandler> coreAreas;

		public List<RoomHandler> additionalAreas;

		public Dictionary<DungeonFlowNode, int> usedSubchainData = new Dictionary<DungeonFlowNode, int>();

		private Dictionary<RoomHandler, FlowNodeBuildData> roomToUndoDataMap = new Dictionary<RoomHandler, FlowNodeBuildData>();

		private Dictionary<FlowNodeBuildData, RoomHandler> dataToRoomMap = new Dictionary<FlowNodeBuildData, RoomHandler>();

		private List<DungeonChainStructure> m_cachedComposedChains;

		private SemioticLayoutManager m_layoutRef;

		private DungeonFlow m_flow;

		private List<FlowActionLine> m_actionLines;

		private ChainSetupData.ExitPreferenceMetric exitMetric = ChainSetupData.ExitPreferenceMetric.FARTHEST;

		private FlowBuilderDebugger m_debugger;

		public SemioticLayoutManager Layout
		{
			get
			{
				return m_layoutRef;
			}
		}

		public RoomHandler StartRoom
		{
			get
			{
				return coreAreas[0];
			}
		}

		public RoomHandler EndRoom
		{
			get
			{
				return coreAreas[coreAreas.Count - 1];
			}
		}

		public DungeonFlowBuilder(DungeonFlow flow, SemioticLayoutManager layout)
		{
			coreAreas = new List<RoomHandler>();
			additionalAreas = new List<RoomHandler>();
			m_flow = flow;
			m_layoutRef = layout;
		}

		private int ContainsPrototypeRoom(PrototypeDungeonRoom r)
		{
			int num = 0;
			for (int i = 0; i < coreAreas.Count; i++)
			{
				if (coreAreas[i].area.prototypeRoom == r)
				{
					num++;
				}
			}
			for (int j = 0; j < additionalAreas.Count; j++)
			{
				if (additionalAreas[j].area.prototypeRoom == r)
				{
					num++;
				}
			}
			return num;
		}

		private PrototypeRoomExit RoomIsViableAtPosition(PrototypeDungeonRoom room, IntVector2 attachPoint, DungeonData.Direction newRoomExitDirection)
		{
			if (!room.CheckPrerequisites())
			{
				return null;
			}
			List<PrototypeRoomExit> unusedExitsOnSide = room.exitData.GetUnusedExitsOnSide(newRoomExitDirection);
			PrototypeRoomExit prototypeRoomExit = null;
			for (int i = 0; i < unusedExitsOnSide.Count; i++)
			{
				if (unusedExitsOnSide[i].exitType == PrototypeRoomExit.ExitType.EXIT_ONLY)
				{
					return null;
				}
				if (m_layoutRef.CanPlaceRoomAtAttachPointByExit(room, unusedExitsOnSide[i], attachPoint))
				{
					prototypeRoomExit = unusedExitsOnSide[i];
					break;
				}
			}
			if (prototypeRoomExit == null)
			{
				return null;
			}
			return prototypeRoomExit;
		}

		private Dictionary<WeightedRoom, PrototypeRoomExit> GetViableRoomsFromList(List<WeightedRoom> source, PrototypeDungeonRoom.RoomCategory category, IntVector2 attachPoint, DungeonData.Direction newRoomExitDirection)
		{
			Dictionary<WeightedRoom, PrototypeRoomExit> dictionary = new Dictionary<WeightedRoom, PrototypeRoomExit>();
			List<int> list = Enumerable.Range(0, source.Count).ToList().GenerationShuffle();
			for (int i = 0; i < source.Count; i++)
			{
				int index = list[i];
				WeightedRoom weightedRoom = source[index];
				PrototypeDungeonRoom room = weightedRoom.room;
				if ((Enum.IsDefined(typeof(PrototypeDungeonRoom.RoomCategory), category) && room.category != category) || !weightedRoom.CheckPrerequisites() || !room.CheckPrerequisites() || (weightedRoom.limitedCopies && ContainsPrototypeRoom(room) >= weightedRoom.maxCopies))
				{
					continue;
				}
				List<PrototypeRoomExit> unusedExitsOnSide = room.exitData.GetUnusedExitsOnSide(newRoomExitDirection);
				PrototypeRoomExit prototypeRoomExit = null;
				for (int j = 0; j < unusedExitsOnSide.Count; j++)
				{
					if (unusedExitsOnSide[j].exitType != PrototypeRoomExit.ExitType.EXIT_ONLY && m_layoutRef.CanPlaceRoomAtAttachPointByExit(room, unusedExitsOnSide[j], attachPoint))
					{
						prototypeRoomExit = unusedExitsOnSide[j];
						break;
					}
				}
				if (prototypeRoomExit != null)
				{
					dictionary.Add(weightedRoom, prototypeRoomExit);
				}
			}
			return dictionary;
		}

		private void AddActionLine(FlowActionLine line)
		{
			if (m_actionLines == null)
			{
				m_actionLines = new List<FlowActionLine>();
			}
			m_actionLines.Add(line);
		}

		private bool CheckActionLineCrossings(Vector2 p1, Vector2 p2)
		{
			FlowActionLine other = new FlowActionLine(p1, p2);
			for (int i = 0; i < m_actionLines.Count; i++)
			{
				if (m_actionLines[i].Crosses(other))
				{
					return true;
				}
			}
			return false;
		}

		private DungeonFlowNode SelectNodeByWeightingWithoutDuplicates(List<DungeonFlowNode> nodes, HashSet<DungeonFlowNode> duplicates)
		{
			float num = 0f;
			for (int i = 0; i < nodes.Count; i++)
			{
				if (!duplicates.Contains(nodes[i]))
				{
					num += nodes[i].percentChance;
				}
			}
			float num2 = BraveRandom.GenerationRandomValue() * num;
			float num3 = 0f;
			for (int j = 0; j < nodes.Count; j++)
			{
				if (!duplicates.Contains(nodes[j]))
				{
					num3 += nodes[j].percentChance;
					if (num3 > num2)
					{
						return nodes[j];
					}
				}
			}
			return nodes[nodes.Count - 1];
		}

		private int SelectIndexByWeightingWithoutDuplicates(List<FlowRoomAttachData> chainRooms, HashSet<int> duplicates)
		{
			float num = 0f;
			for (int i = 0; i < chainRooms.Count; i++)
			{
				if (!duplicates.Contains(i))
				{
					num += chainRooms[i].weightedRoom.weight;
				}
			}
			float num2 = BraveRandom.GenerationRandomValue() * num;
			float num3 = 0f;
			for (int j = 0; j < chainRooms.Count; j++)
			{
				if (!duplicates.Contains(j))
				{
					num3 += chainRooms[j].weightedRoom.weight;
					if (num3 > num2)
					{
						return j;
					}
				}
			}
			return chainRooms.Count - 1;
		}

		private int SelectIndexByWeighting(List<WeightedRoom> chainRooms)
		{
			float num = 0f;
			for (int i = 0; i < chainRooms.Count; i++)
			{
				num += chainRooms[i].weight;
			}
			float num2 = BraveRandom.GenerationRandomValue() * num;
			float num3 = 0f;
			for (int j = 0; j < chainRooms.Count; j++)
			{
				num3 += chainRooms[j].weight;
				if (num3 > num2)
				{
					return j;
				}
			}
			return chainRooms.Count - 1;
		}

		private WeightedRoom GetViableRoomPrototype(PrototypeDungeonRoom.RoomCategory category, IntVector2 attachPoint, DungeonData.Direction extendDirection, ref PrototypeRoomExit exitRef, List<WeightedRoom> roomTable)
		{
			DungeonData.Direction newRoomExitDirection = (DungeonData.Direction)((int)(extendDirection + 4) % 8);
			Dictionary<WeightedRoom, PrototypeRoomExit> viableRoomsFromList = GetViableRoomsFromList(roomTable, category, attachPoint, newRoomExitDirection);
			if (viableRoomsFromList.Keys.Count > 0)
			{
				WeightedRoom weightedRoom = viableRoomsFromList.Keys.First();
				exitRef = viableRoomsFromList[weightedRoom];
				return weightedRoom;
			}
			return null;
		}

		public void DebugActionLines()
		{
			for (int i = 0; i < m_actionLines.Count; i++)
			{
				Debug.DrawLine(m_actionLines[i].point1, m_actionLines[i].point2, Color.yellow, 1000f);
			}
		}

		private void RecomposeNodeStructure(FlowNodeBuildData currentNodeBuildData, DungeonChainStructure extantStructure, List<DungeonChainStructure> runningList)
		{
			DungeonFlowNode node = currentNodeBuildData.node;
			extantStructure.containedNodes.Add(currentNodeBuildData);
			if (!string.IsNullOrEmpty(node.loopTargetNodeGuid) || node.childNodeGuids.Count == 0)
			{
				runningList.Add(extantStructure);
				extantStructure = null;
			}
			if (currentNodeBuildData.childBuildData == null)
			{
				currentNodeBuildData.childBuildData = m_flow.GetNodeChildrenToBuild(node, this);
			}
			for (int i = 0; i < currentNodeBuildData.childBuildData.Count; i++)
			{
				FlowNodeBuildData currentNodeBuildData2 = currentNodeBuildData.childBuildData[i];
				DungeonChainStructure dungeonChainStructure = ((i != 0) ? null : extantStructure);
				if (dungeonChainStructure == null)
				{
					dungeonChainStructure = new DungeonChainStructure();
					dungeonChainStructure.parentNode = currentNodeBuildData;
				}
				RecomposeNodeStructure(currentNodeBuildData2, dungeonChainStructure, runningList);
			}
		}

		private void DecomposeLoopSubchains(List<DungeonChainStructure> subchains)
		{
			for (int i = 0; i < subchains.Count; i++)
			{
				DungeonChainStructure dungeonChainStructure = subchains[i];
				if (dungeonChainStructure.optionalRequiredNode != null && dungeonChainStructure.containedNodes.Count > 1)
				{
					int count = dungeonChainStructure.containedNodes.Count;
					int num = BraveRandom.GenerationRandomRange(1, count - 1);
					List<FlowNodeBuildData> list = new List<FlowNodeBuildData>();
					for (int num2 = count - 1; num2 >= num; num2--)
					{
						list.Add(dungeonChainStructure.containedNodes[num2]);
						dungeonChainStructure.containedNodes.RemoveAt(num2);
					}
					DungeonChainStructure dungeonChainStructure2 = new DungeonChainStructure();
					dungeonChainStructure.optionalRequiredNode.childBuildData.Add(list[0]);
					dungeonChainStructure2.parentNode = dungeonChainStructure.optionalRequiredNode;
					dungeonChainStructure2.containedNodes = list;
					dungeonChainStructure2.optionalRequiredNode = dungeonChainStructure.containedNodes[dungeonChainStructure.containedNodes.Count - 1];
					dungeonChainStructure.optionalRequiredNode = dungeonChainStructure2.containedNodes[dungeonChainStructure2.containedNodes.Count - 1];
					if (dungeonChainStructure2.containedNodes.Count >= dungeonChainStructure.containedNodes.Count)
					{
						subchains.Insert(i + 1, dungeonChainStructure2);
					}
					else
					{
						subchains.Insert(i, dungeonChainStructure2);
					}
					i++;
				}
			}
		}

		protected List<DungeonChainStructure> ComposeBuildOrderSimple()
		{
			List<DungeonChainStructure> list = new List<DungeonChainStructure>();
			List<FlowNodeBuildData> list2 = new List<FlowNodeBuildData>();
			Stack<FlowNodeBuildData> stack = new Stack<FlowNodeBuildData>();
			stack.Push(new FlowNodeBuildData(m_flow.FirstNode));
			while (stack.Count > 0)
			{
				FlowNodeBuildData flowNodeBuildData = stack.Pop();
				list2.Add(flowNodeBuildData);
				if (flowNodeBuildData.childBuildData == null)
				{
					flowNodeBuildData.childBuildData = m_flow.GetNodeChildrenToBuild(flowNodeBuildData.node, this);
				}
				for (int i = 0; i < flowNodeBuildData.childBuildData.Count; i++)
				{
					if (!stack.Contains(flowNodeBuildData.childBuildData[i]))
					{
						stack.Push(flowNodeBuildData.childBuildData[i]);
					}
				}
			}
			DungeonChainStructure dungeonChainStructure = new DungeonChainStructure();
			dungeonChainStructure.containedNodes = list2;
			list.Add(dungeonChainStructure);
			return list;
		}

		public List<DungeonChainStructure> ComposeBuildOrder()
		{
			if (m_cachedComposedChains != null)
			{
				return m_cachedComposedChains;
			}
			m_cachedComposedChains = null;
			return ComposeBuildOrderSimple();
		}

		public bool Build(RoomHandler startRoom)
		{
			m_debugger = new FlowBuilderDebugger();
			coreAreas.Add(startRoom);
			List<DungeonChainStructure> list = ComposeBuildOrder();
			bool flag = true;
			for (int i = 0; i < list.Count; i++)
			{
				DungeonChainStructure dungeonChainStructure = list[i];
				RoomHandler roomToExtendFrom = startRoom;
				if (i > 0)
				{
					roomToExtendFrom = dungeonChainStructure.parentNode.room;
				}
				flag = BuildNode(dungeonChainStructure.containedNodes[0], roomToExtendFrom, null, true);
				if (!flag)
				{
					break;
				}
			}
			if (!flag)
			{
				coreAreas.RemoveAt(0);
			}
			m_debugger.FinalizeLog();
			return flag;
		}

		protected void ShuffleExitsByMetric(ref List<PrototypeRoomExit> unusedExits, PrototypeRoomExit previouslyUsedExit)
		{
			switch (exitMetric)
			{
			case ChainSetupData.ExitPreferenceMetric.RANDOM:
				unusedExits = unusedExits.GenerationShuffle();
				break;
			case ChainSetupData.ExitPreferenceMetric.HORIZONTAL:
				break;
			case ChainSetupData.ExitPreferenceMetric.VERTICAL:
				Debug.LogError("Vertical not yet implemented");
				break;
			case ChainSetupData.ExitPreferenceMetric.FARTHEST:
				if (previouslyUsedExit == null)
				{
					unusedExits = unusedExits.GenerationShuffle();
					break;
				}
				unusedExits = unusedExits.OrderBy((PrototypeRoomExit a) => IntVector2.ManhattanDistance(a.GetExitOrigin(a.exitLength), previouslyUsedExit.GetExitOrigin(previouslyUsedExit.exitLength))).ToList();
				break;
			case ChainSetupData.ExitPreferenceMetric.NEAREST:
				if (previouslyUsedExit == null)
				{
					unusedExits = unusedExits.GenerationShuffle();
					break;
				}
				unusedExits = unusedExits.OrderByDescending((PrototypeRoomExit a) => Vector2.Distance(a.GetExitOrigin(a.exitLength).ToVector2(), previouslyUsedExit.GetExitOrigin(previouslyUsedExit.exitLength).ToVector2())).ToList();
				break;
			default:
				unusedExits = unusedExits.GenerationShuffle();
				break;
			}
		}

		private List<FlowRoomAttachData> GetViableRoomsForExits(CellArea areaToExtendFrom, PrototypeDungeonRoom.RoomCategory nextRoomCategory, List<PrototypeRoomExit> unusedExits, List<WeightedRoom> roomTable)
		{
			List<FlowRoomAttachData> list = new List<FlowRoomAttachData>();
			for (int i = 0; i < unusedExits.Count; i++)
			{
				PrototypeRoomExit prototypeRoomExit = unusedExits[i];
				IntVector2 attachPoint = areaToExtendFrom.basePosition + prototypeRoomExit.GetExitOrigin(prototypeRoomExit.exitLength) - IntVector2.One;
				DungeonData.Direction exitDirection = prototypeRoomExit.exitDirection;
				DungeonData.Direction newRoomExitDirection = (DungeonData.Direction)((int)(exitDirection + 4) % 8);
				Dictionary<WeightedRoom, PrototypeRoomExit> viableRoomsFromList = GetViableRoomsFromList(roomTable, nextRoomCategory, attachPoint, newRoomExitDirection);
				foreach (WeightedRoom key in viableRoomsFromList.Keys)
				{
					list.Add(new FlowRoomAttachData(key, viableRoomsFromList[key], prototypeRoomExit));
				}
			}
			return list;
		}

		private void RecursivelyUnstampChildren(FlowNodeBuildData buildData)
		{
			for (int i = 0; i < buildData.childBuildData.Count; i++)
			{
				RecursivelyUnstampChildren(buildData.childBuildData[i]);
			}
			if (buildData.room != null)
			{
				m_layoutRef.StampCellAreaToLayout(buildData.room, true);
				if (buildData.room.flowActionLine != null)
				{
					m_actionLines.Remove(buildData.room.flowActionLine);
					buildData.room.flowActionLine = null;
				}
				if (coreAreas.Contains(buildData.room))
				{
					coreAreas.Remove(buildData.room);
				}
				roomToUndoDataMap.Remove(buildData.room);
				buildData.UnmarkExits();
			}
			buildData.unbuilt = true;
			dataToRoomMap.Remove(buildData);
			buildData.room = null;
		}

		private bool HandleNodeChildren(FlowNodeBuildData originalNodeBuildData, DungeonChainStructure chain)
		{
			originalNodeBuildData.MarkExits();
			originalNodeBuildData.unbuilt = false;
			FlowActionLine flowActionLine = new FlowActionLine(originalNodeBuildData.room.GetCenterCell().ToCenterVector2(), originalNodeBuildData.sourceRoom.GetCenterCell().ToCenterVector2());
			AddActionLine(flowActionLine);
			originalNodeBuildData.room.flowActionLine = flowActionLine;
			bool flag = true;
			if (chain != null)
			{
				int num = chain.containedNodes.IndexOf(originalNodeBuildData) + 1;
				if (num >= chain.containedNodes.Count)
				{
					if (chain.optionalRequiredNode != null && chain.optionalRequiredNode.room != null && !GameManager.Instance.Dungeon.debugSettings.DISABLE_LOOPS)
					{
						flag = BuildLoopNode(originalNodeBuildData, chain.optionalRequiredNode, chain);
					}
				}
				else
				{
					flag = BuildNode(chain.containedNodes[num], originalNodeBuildData.room, chain);
					if (chain.containedNodes[num].node.priority == DungeonFlowNode.NodePriority.OPTIONAL)
					{
						flag = true;
					}
				}
			}
			else
			{
				for (int i = 0; i < originalNodeBuildData.childBuildData.Count; i++)
				{
					flag = BuildNode(originalNodeBuildData.childBuildData[i], originalNodeBuildData.room, null);
					if (originalNodeBuildData.childBuildData[i].node.priority == DungeonFlowNode.NodePriority.OPTIONAL)
					{
						flag = true;
					}
					if (!flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				return true;
			}
			return false;
		}

		private IntRect GetSpanningRectangle(IntVector2 p1, IntVector2 p2, out bool valid)
		{
			int num = Math.Min(p1.x, p2.x);
			int num2 = Math.Min(p1.y, p2.y);
			int num3 = Math.Max(p1.x, p2.x);
			int num4 = Math.Max(p1.y, p2.y);
			IntRect intRect = new IntRect(num, num2, num3 - num, num4 - num2);
			valid = m_layoutRef.CanPlaceRectangle(intRect);
			return intRect;
		}

		private bool BuildLoopNode(FlowNodeBuildData chainEndData, FlowNodeBuildData loopTargetData, DungeonChainStructure chain)
		{
			RoomHandler room = chainEndData.room;
			RoomHandler room2 = loopTargetData.room;
			CellArea area = room.area;
			CellArea area2 = room2.area;
			if (area2.prototypeRoom != null && area.prototypeRoom != null)
			{
				List<PrototypeRoomExit> unusedExitsFromInstance = area.prototypeRoom.exitData.GetUnusedExitsFromInstance(area);
				List<PrototypeRoomExit> unusedExitsFromInstance2 = area2.prototypeRoom.exitData.GetUnusedExitsFromInstance(area2);
				List<LoopPathData> list = new List<LoopPathData>();
				for (int i = 0; i < unusedExitsFromInstance.Count; i++)
				{
					for (int j = 0; j < unusedExitsFromInstance2.Count; j++)
					{
						PrototypeRoomExit prototypeRoomExit = unusedExitsFromInstance[i];
						PrototypeRoomExit prototypeRoomExit2 = unusedExitsFromInstance2[j];
						IntVector2 startPosition = area.basePosition + prototypeRoomExit.GetExitOrigin(prototypeRoomExit.exitLength) - IntVector2.One + DungeonData.GetIntVector2FromDirection(prototypeRoomExit.exitDirection);
						IntVector2 endPosition = area2.basePosition + prototypeRoomExit2.GetExitOrigin(prototypeRoomExit2.exitLength) - IntVector2.One + DungeonData.GetIntVector2FromDirection(prototypeRoomExit2.exitDirection);
						List<IntVector2> list2 = m_layoutRef.TraceHallway(startPosition, endPosition, prototypeRoomExit.exitDirection, prototypeRoomExit2.exitDirection);
						if (list2 != null)
						{
							list.Add(new LoopPathData(list2, prototypeRoomExit, prototypeRoomExit2));
						}
					}
				}
				if (list.Count > 0)
				{
					LoopPathData loopPathData = list[0];
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].path.Count < loopPathData.path.Count)
						{
							loopPathData = list[k];
						}
					}
					IntVector2 intVector = new IntVector2(int.MaxValue, int.MaxValue);
					IntVector2 d = new IntVector2(int.MinValue, int.MinValue);
					for (int l = 0; l < loopPathData.path.Count; l++)
					{
						intVector.x = Math.Min(intVector.x, loopPathData.path[l].x);
						intVector.y = Math.Min(intVector.y, loopPathData.path[l].y);
						d.x = Math.Max(d.x, loopPathData.path[l].x);
						d.y = Math.Max(d.y, loopPathData.path[l].y);
					}
					for (int m = 0; m < loopPathData.path.Count; m++)
					{
						loopPathData.path[m] -= intVector;
					}
					CellArea cellArea = new CellArea(intVector, d);
					cellArea.proceduralCells = loopPathData.path;
					RoomHandler roomHandler = new RoomHandler(cellArea);
					roomHandler.distanceFromEntrance = room2.distanceFromEntrance + 1;
					roomHandler.CalculateOpulence();
					coreAreas.Add(roomHandler);
					m_layoutRef.StampCellAreaToLayout(roomHandler);
					room.area.instanceUsedExits.Add(loopPathData.initialExit);
					room2.area.instanceUsedExits.Add(loopPathData.finalExit);
					roomHandler.parentRoom = room;
					roomHandler.childRooms.Add(room2);
					room.childRooms.Add(roomHandler);
					room.connectedRooms.Add(roomHandler);
					room.connectedRoomsByExit.Add(loopPathData.initialExit, roomHandler);
					room2.childRooms.Add(roomHandler);
					room2.connectedRooms.Add(roomHandler);
					room2.connectedRoomsByExit.Add(loopPathData.finalExit, roomHandler);
					return true;
				}
				if (unusedExitsFromInstance2.Count == 0 || unusedExitsFromInstance.Count == 0)
				{
					BraveUtility.Log("No free exits to generate loop. No loop generated.", Color.cyan, BraveUtility.LogVerbosity.CHATTY);
				}
				else
				{
					BraveUtility.Log("All loops failed. No loop generated.", Color.cyan, BraveUtility.LogVerbosity.CHATTY);
				}
			}
			else
			{
				Debug.LogError("Procedural rooms not implemented in loop generation yet!");
			}
			return false;
		}

		private bool BuildNode(FlowNodeBuildData nodeBuildData, RoomHandler roomToExtendFrom, DungeonChainStructure chain, bool initial = false)
		{
			DungeonFlowNode node = nodeBuildData.node;
			if (node == null)
			{
				return true;
			}
			if (dataToRoomMap.ContainsKey(nodeBuildData))
			{
				Debug.LogError("FAILURE");
				RecursivelyUnstampChildren(nodeBuildData);
			}
			if (node.nodeType != 0)
			{
				switch (node.nodeType)
				{
				case DungeonFlowNode.ControlNodeType.SUBCHAIN:
					Debug.Break();
					break;
				case DungeonFlowNode.ControlNodeType.SELECTOR:
					Debug.Break();
					break;
				}
			}
			else
			{
				PrototypeDungeonRoom.RoomCategory nextRoomCategory = ((!nodeBuildData.usesOverrideCategory) ? node.roomCategory : nodeBuildData.overrideCategory);
				CellArea area = roomToExtendFrom.area;
				List<FlowRoomAttachData> list = null;
				if (!(area.prototypeRoom != null))
				{
					Debug.LogError("Procedural room handling not yet implemented!");
					return false;
				}
				List<PrototypeRoomExit> unusedExits = area.prototypeRoom.exitData.GetUnusedExitsFromInstance(area);
				PrototypeRoomExit previouslyUsedExit = null;
				if (area.instanceUsedExits.Count != 0)
				{
					previouslyUsedExit = area.instanceUsedExits[BraveRandom.GenerationRandomRange(0, area.instanceUsedExits.Count)];
				}
				if (chain != null && chain.optionalRequiredNode != null && chain.optionalRequiredNode.room != null)
				{
					for (int i = 0; i < unusedExits.Count; i++)
					{
						Vector2 vector = (area.basePosition + unusedExits[i].GetExitOrigin(unusedExits[i].exitLength) - IntVector2.One).ToCenterVector2();
						Vector2 vector2 = chain.optionalRequiredNode.room.GetCenterCell().ToCenterVector2();
						vector2 += (vector - vector2).normalized;
						if (CheckActionLineCrossings(vector, vector2))
						{
							unusedExits.RemoveAt(i);
							i--;
						}
					}
				}
				ShuffleExitsByMetric(ref unusedExits, previouslyUsedExit);
				List<WeightedRoom> list2 = null;
				if (node.UsesGlobalBossData)
				{
					list2 = GameManager.Instance.BossManager.SelectBossTable().GetCompiledList();
				}
				else if (node.overrideExactRoom != null)
				{
					WeightedRoom weightedRoom = new WeightedRoom();
					weightedRoom.room = node.overrideExactRoom;
					weightedRoom.weight = 1f;
					list2 = new List<WeightedRoom>();
					list2.Add(weightedRoom);
					nextRoomCategory = node.overrideExactRoom.category;
				}
				else
				{
					GenericRoomTable genericRoomTable = m_flow.fallbackRoomTable;
					if (node.overrideRoomTable != null)
					{
						genericRoomTable = node.overrideRoomTable;
					}
					list2 = genericRoomTable.GetCompiledList();
				}
				list = GetViableRoomsForExits(area, nextRoomCategory, unusedExits, list2);
				int num = list.Count;
				for (int j = 0; j < num; j++)
				{
					FlowRoomAttachData item = list[j];
					if (ContainsPrototypeRoom(item.weightedRoom.room) > 0)
					{
						list.RemoveAt(j);
						list.Add(item);
						j--;
						num--;
					}
				}
				if (list == null || list.Count == 0)
				{
					return false;
				}
				if (nodeBuildData.childBuildData == null && m_flow.IsPartOfSubchain(nodeBuildData.node))
				{
					nodeBuildData.childBuildData = m_flow.GetNodeChildrenToBuild(nodeBuildData.node, this);
				}
				List<FlowNodeBuildData> childBuildData = nodeBuildData.childBuildData;
				int num2 = 0;
				for (int k = 0; k < childBuildData.Count; k++)
				{
					num2 += ((childBuildData[k].node.priority == DungeonFlowNode.NodePriority.MANDATORY) ? 1 : 0);
				}
				HashSet<int> hashSet = new HashSet<int>();
				for (int l = 0; l < list.Count; l++)
				{
					int num3 = SelectIndexByWeightingWithoutDuplicates(list, hashSet);
					hashSet.Add(num3);
					PrototypeDungeonRoom room = list[num3].weightedRoom.room;
					if (room.exitData.exits.Count < num2 + 1)
					{
						continue;
					}
					PrototypeRoomExit exitOfNewRoom = list[num3].exitOfNewRoom;
					PrototypeRoomExit exitToUse = list[num3].exitToUse;
					IntVector2 intVector = area.basePosition + exitToUse.GetExitOrigin(exitToUse.exitLength) - IntVector2.One;
					IntVector2 intVector2 = intVector - (exitOfNewRoom.GetExitOrigin(exitOfNewRoom.exitLength) - IntVector2.One);
					if (chain != null && chain.optionalRequiredNode != null && chain.optionalRequiredNode.room != null)
					{
						if (chain.previousLoopDistanceMetric == int.MaxValue)
						{
							chain.previousLoopDistanceMetric = IntVector2.ManhattanDistance(chain.optionalRequiredNode.room.GetCenterCell(), intVector);
						}
						int num4 = int.MaxValue;
						for (int m = 0; m < room.exitData.exits.Count; m++)
						{
							if (room.exitData.exits[m] != exitOfNewRoom)
							{
								num4 = Math.Min(num4, IntVector2.ManhattanDistance(chain.optionalRequiredNode.room.GetCenterCell(), intVector2 + room.exitData.exits[m].GetExitOrigin(room.exitData.exits[m].exitLength) - IntVector2.One));
							}
						}
						if (num4 > chain.previousLoopDistanceMetric)
						{
							continue;
						}
						chain.previousLoopDistanceMetric = num4;
					}
					CellArea cellArea = new CellArea(intVector2, new IntVector2(room.Width, room.Height));
					cellArea.prototypeRoom = room;
					cellArea.instanceUsedExits = new List<PrototypeRoomExit>();
					if (nodeBuildData.usesOverrideCategory)
					{
						cellArea.PrototypeRoomCategory = nodeBuildData.overrideCategory;
					}
					RoomHandler roomHandler = new RoomHandler(cellArea);
					roomHandler.distanceFromEntrance = roomToExtendFrom.distanceFromEntrance + 1;
					roomHandler.CalculateOpulence();
					roomHandler.CanReceiveCaps = node.receivesCaps;
					coreAreas.Add(roomHandler);
					m_layoutRef.StampCellAreaToLayout(roomHandler);
					nodeBuildData.room = roomHandler;
					nodeBuildData.roomEntrance = exitOfNewRoom;
					nodeBuildData.sourceExit = exitToUse;
					nodeBuildData.sourceRoom = roomToExtendFrom;
					roomToUndoDataMap.Add(roomHandler, nodeBuildData);
					dataToRoomMap.Add(nodeBuildData, roomHandler);
					m_debugger.Log(roomToExtendFrom, roomHandler);
					m_debugger.LogMonoHeapStatus();
					if (HandleNodeChildren(nodeBuildData, chain))
					{
						return true;
					}
					m_debugger.Log(roomToExtendFrom.area.prototypeRoom.name + " is falling back...");
					RecursivelyUnstampChildren(nodeBuildData);
				}
			}
			m_debugger.Log(roomToExtendFrom.area.prototypeRoom.name + " completely failed.");
			return false;
		}

		public void AppendCapChains()
		{
			List<RoomHandler> roomsWithViableExits = new List<RoomHandler>();
			List<PrototypeRoomExit> viableExitsToCap = new List<PrototypeRoomExit>();
			for (int i = 0; i < coreAreas.Count; i++)
			{
				PrototypeDungeonRoom prototypeRoom = coreAreas[i].area.prototypeRoom;
				if (prototypeRoom == null || !coreAreas[i].CanReceiveCaps)
				{
					continue;
				}
				for (int j = 0; j < prototypeRoom.exitData.exits.Count; j++)
				{
					if (!coreAreas[i].area.instanceUsedExits.Contains(prototypeRoom.exitData.exits[j]) && prototypeRoom.exitData.exits[j].exitType != PrototypeRoomExit.ExitType.ENTRANCE_ONLY)
					{
						roomsWithViableExits.Add(coreAreas[i]);
						viableExitsToCap.Add(prototypeRoom.exitData.exits[j]);
					}
				}
			}
			List<int> input = Enumerable.Range(0, roomsWithViableExits.Count).ToList();
			input = input.GenerationShuffle();
			roomsWithViableExits = input.Select((int index) => roomsWithViableExits[index]).ToList();
			viableExitsToCap = input.Select((int index) => viableExitsToCap[index]).ToList();
			for (int k = 0; k < viableExitsToCap.Count; k++)
			{
				List<DungeonFlowNode> capChainRootNodes = m_flow.GetCapChainRootNodes(this);
				if (capChainRootNodes == null || capChainRootNodes.Count == 0)
				{
					break;
				}
				HashSet<DungeonFlowNode> hashSet = new HashSet<DungeonFlowNode>();
				bool flag = false;
				DungeonFlowNode dungeonFlowNode = SelectNodeByWeightingWithoutDuplicates(capChainRootNodes, hashSet);
				hashSet.Add(dungeonFlowNode);
				FlowNodeBuildData flowNodeBuildData = new FlowNodeBuildData(dungeonFlowNode);
				flowNodeBuildData.childBuildData = m_flow.GetNodeChildrenToBuild(dungeonFlowNode, this);
				if (BuildNode(flowNodeBuildData, roomsWithViableExits[k], null))
				{
					if (usedSubchainData.ContainsKey(dungeonFlowNode))
					{
						usedSubchainData[dungeonFlowNode] += 1;
					}
					else
					{
						usedSubchainData.Add(dungeonFlowNode, 1);
					}
				}
			}
		}

		public bool AttemptAppendExtraRoom(ExtraIncludedRoomData extraRoomData)
		{
			List<RoomHandler> roomsWithViableExits = new List<RoomHandler>();
			List<PrototypeRoomExit> viableExitsToCap = new List<PrototypeRoomExit>();
			for (int i = 0; i < coreAreas.Count; i++)
			{
				PrototypeDungeonRoom prototypeRoom = coreAreas[i].area.prototypeRoom;
				if (prototypeRoom == null)
				{
					continue;
				}
				for (int j = 0; j < prototypeRoom.exitData.exits.Count; j++)
				{
					if (!coreAreas[i].area.instanceUsedExits.Contains(prototypeRoom.exitData.exits[j]) && prototypeRoom.exitData.exits[j].exitType != PrototypeRoomExit.ExitType.ENTRANCE_ONLY)
					{
						roomsWithViableExits.Add(coreAreas[i]);
						viableExitsToCap.Add(prototypeRoom.exitData.exits[j]);
					}
				}
			}
			List<int> input = Enumerable.Range(0, roomsWithViableExits.Count).ToList();
			input = input.GenerationShuffle();
			roomsWithViableExits = input.Select((int index) => roomsWithViableExits[index]).ToList();
			viableExitsToCap = input.Select((int index) => viableExitsToCap[index]).ToList();
			for (int k = 0; k < viableExitsToCap.Count; k++)
			{
				PrototypeRoomExit prototypeRoomExit = viableExitsToCap[k];
				IntVector2 intVector = roomsWithViableExits[k].area.basePosition + prototypeRoomExit.GetExitOrigin(prototypeRoomExit.exitLength) - IntVector2.One;
				DungeonData.Direction newRoomExitDirection = (DungeonData.Direction)((int)(prototypeRoomExit.exitDirection + 4) % 8);
				PrototypeRoomExit prototypeRoomExit2 = RoomIsViableAtPosition(extraRoomData.room, intVector, newRoomExitDirection);
				if (prototypeRoomExit2 != null)
				{
					IntVector2 p = intVector - (prototypeRoomExit2.GetExitOrigin(prototypeRoomExit2.exitLength) - IntVector2.One);
					CellArea cellArea = new CellArea(p, new IntVector2(extraRoomData.room.Width, extraRoomData.room.Height));
					cellArea.prototypeRoom = extraRoomData.room;
					cellArea.instanceUsedExits = new List<PrototypeRoomExit>();
					RoomHandler roomHandler = new RoomHandler(cellArea);
					roomHandler.distanceFromEntrance = roomsWithViableExits[k].distanceFromEntrance + 1;
					roomHandler.CalculateOpulence();
					additionalAreas.Add(roomHandler);
					m_layoutRef.StampCellAreaToLayout(roomHandler);
					cellArea.instanceUsedExits.Add(prototypeRoomExit2);
					roomsWithViableExits[k].area.instanceUsedExits.Add(prototypeRoomExit);
					roomHandler.parentRoom = roomsWithViableExits[k];
					roomHandler.connectedRooms.Add(roomsWithViableExits[k]);
					roomHandler.connectedRoomsByExit.Add(prototypeRoomExit2, roomsWithViableExits[k]);
					roomsWithViableExits[k].childRooms.Add(roomHandler);
					roomsWithViableExits[k].connectedRooms.Add(roomHandler);
					roomsWithViableExits[k].connectedRoomsByExit.Add(prototypeRoomExit, roomHandler);
					return true;
				}
			}
			return false;
		}
	}
}
