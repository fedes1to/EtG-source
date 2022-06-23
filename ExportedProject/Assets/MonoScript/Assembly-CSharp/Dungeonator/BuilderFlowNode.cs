using System.Collections.Generic;

namespace Dungeonator
{
	public class BuilderFlowNode : ArbitraryFlowBuildData
	{
		public int identifier = -1;

		public DungeonFlowNode node;

		public bool AcquiresRoomAsNecessary;

		public PrototypeDungeonRoom assignedPrototypeRoom;

		public RoomHandler instanceRoom;

		public bool usesOverrideCategory;

		public PrototypeDungeonRoom.RoomCategory overrideCategory;

		public BuilderFlowNode parentBuilderNode;

		public List<BuilderFlowNode> childBuilderNodes;

		public BuilderFlowNode loopConnectedBuilderNode;

		public Dictionary<PrototypeRoomExit, BuilderFlowNode> exitToNodeMap = new Dictionary<PrototypeRoomExit, BuilderFlowNode>();

		public Dictionary<BuilderFlowNode, PrototypeRoomExit> nodeToExitMap = new Dictionary<BuilderFlowNode, PrototypeRoomExit>();

		public BuilderFlowNode InjectionTarget;

		public List<BuilderFlowNode> InjectionFrameSequence;

		public bool IsInjectedNode;

		public int Connectivity
		{
			get
			{
				int num = 0;
				if (parentBuilderNode != null)
				{
					num++;
				}
				if (loopConnectedBuilderNode != null)
				{
					num++;
				}
				return num + childBuilderNodes.Count;
			}
		}

		public PrototypeDungeonRoom.RoomCategory Category
		{
			get
			{
				if (usesOverrideCategory)
				{
					return overrideCategory;
				}
				if (assignedPrototypeRoom != null)
				{
					return assignedPrototypeRoom.category;
				}
				return node.roomCategory;
			}
		}

		public bool IsStandardCategory
		{
			get
			{
				if (Category == PrototypeDungeonRoom.RoomCategory.NORMAL || Category == PrototypeDungeonRoom.RoomCategory.CONNECTOR || Category == PrototypeDungeonRoom.RoomCategory.HUB)
				{
					if (Category == PrototypeDungeonRoom.RoomCategory.NORMAL && assignedPrototypeRoom != null && assignedPrototypeRoom.subCategoryNormal == PrototypeDungeonRoom.RoomNormalSubCategory.TRAP)
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public BuilderFlowNode(DungeonFlowNode n)
		{
			node = n;
		}

		public void ClearData()
		{
			exitToNodeMap.Clear();
			nodeToExitMap.Clear();
			IsInjectedNode = false;
		}

		public bool IsOfDepth(int depth)
		{
			BuilderFlowNode builderFlowNode = this;
			for (int i = 0; i < depth; i++)
			{
				if (builderFlowNode.parentBuilderNode == null)
				{
					return false;
				}
				builderFlowNode = builderFlowNode.parentBuilderNode;
			}
			return true;
		}

		public List<BuilderFlowNode> GetAllConnectedNodes(List<BuilderFlowNode> excluded)
		{
			List<BuilderFlowNode> list = new List<BuilderFlowNode>(childBuilderNodes);
			if (parentBuilderNode != null)
			{
				list.Add(parentBuilderNode);
			}
			if (loopConnectedBuilderNode != null)
			{
				list.Add(loopConnectedBuilderNode);
			}
			for (int i = 0; i < excluded.Count; i++)
			{
				list.Remove(excluded[i]);
			}
			return list;
		}

		public List<BuilderFlowNode> GetAllConnectedNodes(params BuilderFlowNode[] excluded)
		{
			List<BuilderFlowNode> list = new List<BuilderFlowNode>(childBuilderNodes);
			if (parentBuilderNode != null)
			{
				list.Add(parentBuilderNode);
			}
			if (loopConnectedBuilderNode != null)
			{
				list.Add(loopConnectedBuilderNode);
			}
			for (int i = 0; i < excluded.Length; i++)
			{
				list.Remove(excluded[i]);
			}
			return list;
		}

		public void MakeNodeTreeRoot()
		{
			if (parentBuilderNode != null)
			{
				BuilderFlowNode builderFlowNode = null;
				BuilderFlowNode builderFlowNode2 = this;
				BuilderFlowNode builderFlowNode3 = parentBuilderNode;
				while (builderFlowNode3 != null)
				{
					BuilderFlowNode builderFlowNode4 = builderFlowNode3.parentBuilderNode;
					builderFlowNode2.parentBuilderNode = builderFlowNode;
					builderFlowNode2.childBuilderNodes.Add(builderFlowNode3);
					builderFlowNode3.parentBuilderNode = builderFlowNode2;
					builderFlowNode3.childBuilderNodes.Remove(builderFlowNode2);
					builderFlowNode = builderFlowNode2;
					builderFlowNode2 = builderFlowNode3;
					builderFlowNode3 = builderFlowNode4;
				}
			}
		}
	}
}
