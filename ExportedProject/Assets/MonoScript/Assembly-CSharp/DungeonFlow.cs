using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DungeonFlow : ScriptableObject
{
	[SerializeField]
	public GenericRoomTable fallbackRoomTable;

	[SerializeField]
	public GenericRoomTable phantomRoomTable;

	[SerializeField]
	public List<DungeonFlowSubtypeRestriction> subtypeRestrictions;

	[NonSerialized]
	public GenericRoomTable evolvedRoomTable;

	[SerializeField]
	private List<DungeonFlowNode> m_nodes;

	[SerializeField]
	private List<string> m_nodeGuids;

	[SerializeField]
	private string m_firstNodeGuid;

	[SerializeField]
	public List<ProceduralFlowModifierData> flowInjectionData;

	[SerializeField]
	public List<SharedInjectionData> sharedInjectionData;

	public List<DungeonFlowNode> AllNodes
	{
		get
		{
			return m_nodes;
		}
	}

	public DungeonFlowNode FirstNode
	{
		get
		{
			return GetNodeFromGuid(m_firstNodeGuid);
		}
		set
		{
			m_firstNodeGuid = value.guidAsString;
		}
	}

	public int GetAverageNumberRooms()
	{
		int num = 0;
		for (int i = 0; i < m_nodes.Count; i++)
		{
			num += m_nodes[i].GetAverageNumberNodes();
		}
		return num;
	}

	public void Initialize()
	{
		m_nodes = new List<DungeonFlowNode>();
		m_nodeGuids = new List<string>();
	}

	public void GetNodesRecursive(DungeonFlowNode node, List<DungeonFlowNode> all)
	{
		if (node == null)
		{
			return;
		}
		all.Add(node);
		if (node.childNodeGuids == null)
		{
			node.childNodeGuids = new List<string>();
		}
		foreach (string childNodeGuid in node.childNodeGuids)
		{
			DungeonFlowNode nodeFromGuid = GetNodeFromGuid(childNodeGuid);
			GetNodesRecursive(nodeFromGuid, all);
		}
	}

	public void AddNodeToFlow(DungeonFlowNode newNode, DungeonFlowNode parent)
	{
		if (m_nodeGuids == null || m_nodes == null)
		{
			Initialize();
		}
		if (m_nodeGuids.Contains(newNode.guidAsString))
		{
			return;
		}
		m_nodes.Add(newNode);
		m_nodeGuids.Add(newNode.guidAsString);
		if (parent != null)
		{
			if (!parent.childNodeGuids.Contains(newNode.guidAsString))
			{
				parent.childNodeGuids.Add(newNode.guidAsString);
				newNode.parentNodeGuid = parent.guidAsString;
			}
		}
		else
		{
			newNode.parentNodeGuid = string.Empty;
		}
	}

	public void DeleteNode(DungeonFlowNode node, bool deleteChain = false)
	{
		if (deleteChain)
		{
			List<DungeonFlowNode> list = new List<DungeonFlowNode>();
			GetNodesRecursive(node, list);
			{
				foreach (DungeonFlowNode item in list)
				{
					RemoveNodeInternal(item);
				}
				return;
			}
		}
		RemoveNodeInternal(node);
	}

	private void RemoveNodeInternal(DungeonFlowNode node)
	{
		if (!string.IsNullOrEmpty(node.parentNodeGuid))
		{
			DungeonFlowNode nodeFromGuid = GetNodeFromGuid(node.parentNodeGuid);
			nodeFromGuid.childNodeGuids.Remove(node.guidAsString);
		}
		foreach (string childNodeGuid in node.childNodeGuids)
		{
			DungeonFlowNode nodeFromGuid2 = GetNodeFromGuid(childNodeGuid);
			nodeFromGuid2.parentNodeGuid = string.Empty;
		}
		if (!string.IsNullOrEmpty(node.loopTargetNodeGuid))
		{
			DungeonFlowNode nodeFromGuid3 = GetNodeFromGuid(node.loopTargetNodeGuid);
			nodeFromGuid3.loopTargetNodeGuid = string.Empty;
		}
		for (int i = 0; i < m_nodes.Count; i++)
		{
			if (m_nodes[i].loopTargetNodeGuid == node.guidAsString)
			{
				m_nodes[i].loopTargetNodeGuid = string.Empty;
			}
		}
		node.parentNodeGuid = string.Empty;
		node.childNodeGuids.Clear();
		node.loopTargetNodeGuid = string.Empty;
		m_nodes.Remove(node);
		m_nodeGuids.Remove(node.guidAsString);
	}

	public bool IsPartOfSubchain(DungeonFlowNode node)
	{
		DungeonFlowNode dungeonFlowNode = node;
		while (!string.IsNullOrEmpty(dungeonFlowNode.parentNodeGuid))
		{
			dungeonFlowNode = GetNodeFromGuid(dungeonFlowNode.parentNodeGuid);
		}
		if (dungeonFlowNode != FirstNode)
		{
			return true;
		}
		return false;
	}

	private PrototypeDungeonRoom.RoomCategory GetCategoryFromChar(char c)
	{
		switch (c)
		{
		case 'c':
			return PrototypeDungeonRoom.RoomCategory.CONNECTOR;
		case 'n':
			return PrototypeDungeonRoom.RoomCategory.NORMAL;
		case 'b':
			return PrototypeDungeonRoom.RoomCategory.BOSS;
		case 'r':
			return PrototypeDungeonRoom.RoomCategory.REWARD;
		case 's':
			return PrototypeDungeonRoom.RoomCategory.SPECIAL;
		case 'h':
			return PrototypeDungeonRoom.RoomCategory.HUB;
		case 't':
			return PrototypeDungeonRoom.RoomCategory.SECRET;
		case 'e':
			return PrototypeDungeonRoom.RoomCategory.ENTRANCE;
		case 'x':
			return PrototypeDungeonRoom.RoomCategory.EXIT;
		default:
			return PrototypeDungeonRoom.RoomCategory.NORMAL;
		}
	}

	public DungeonFlowNode GetSubchainRootFromNode(DungeonFlowNode source, LoopFlowBuilder builder)
	{
		List<DungeonFlowNode> list = new List<DungeonFlowNode>();
		for (int i = 0; i < m_nodes.Count; i++)
		{
			if (!string.IsNullOrEmpty(m_nodes[i].subchainIdentifier) && source.subchainIdentifiers.Contains(m_nodes[i].subchainIdentifier) && (!m_nodes[i].limitedCopiesOfSubchain || !builder.usedSubchainData.ContainsKey(m_nodes[i]) || builder.usedSubchainData[m_nodes[i]] < m_nodes[i].maxCopiesOfSubchain))
			{
				list.Add(m_nodes[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[BraveRandom.GenerationRandomRange(0, list.Count)];
	}

	public DungeonFlowNode GetSubchainRootFromNode(DungeonFlowNode source, DungeonFlowBuilder builder)
	{
		List<DungeonFlowNode> list = new List<DungeonFlowNode>();
		for (int i = 0; i < m_nodes.Count; i++)
		{
			if (!string.IsNullOrEmpty(m_nodes[i].subchainIdentifier) && source.subchainIdentifiers.Contains(m_nodes[i].subchainIdentifier) && (!m_nodes[i].limitedCopiesOfSubchain || !builder.usedSubchainData.ContainsKey(m_nodes[i]) || builder.usedSubchainData[m_nodes[i]] < m_nodes[i].maxCopiesOfSubchain))
			{
				list.Add(m_nodes[i]);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[BraveRandom.GenerationRandomRange(0, list.Count)];
	}

	public List<BuilderFlowNode> NewGetNodeChildrenToBuild(BuilderFlowNode parentBuilderNode, LoopFlowBuilder builder)
	{
		DungeonFlowNode node = parentBuilderNode.node;
		List<BuilderFlowNode> list = new List<BuilderFlowNode>();
		for (int i = 0; i < node.childNodeGuids.Count; i++)
		{
			DungeonFlowNode nodeFromGuid = GetNodeFromGuid(node.childNodeGuids[i]);
			if (nodeFromGuid.nodeType == DungeonFlowNode.ControlNodeType.SELECTOR)
			{
				List<DungeonFlowNode> selectorNodeChildren = GetSelectorNodeChildren(nodeFromGuid);
				for (int j = 0; j < selectorNodeChildren.Count; j++)
				{
					list.Add(new BuilderFlowNode(selectorNodeChildren[j]));
				}
			}
			else if (nodeFromGuid.nodeType == DungeonFlowNode.ControlNodeType.SUBCHAIN)
			{
				DungeonFlowNode subchainRootFromNode = GetSubchainRootFromNode(nodeFromGuid, builder);
				if (!(subchainRootFromNode == null))
				{
					if (builder.usedSubchainData.ContainsKey(subchainRootFromNode))
					{
						builder.usedSubchainData[subchainRootFromNode] = builder.usedSubchainData[subchainRootFromNode] + 1;
					}
					else
					{
						builder.usedSubchainData.Add(subchainRootFromNode, 1);
					}
					list.Add(new BuilderFlowNode(subchainRootFromNode));
				}
			}
			else if (nodeFromGuid.nodeExpands)
			{
				string text = nodeFromGuid.EvolveChainToCompletion();
				BuilderFlowNode builderFlowNode = null;
				foreach (char c in text)
				{
					PrototypeDungeonRoom.RoomCategory categoryFromChar = GetCategoryFromChar(c);
					BuilderFlowNode builderFlowNode2 = new BuilderFlowNode(nodeFromGuid);
					builderFlowNode2.usesOverrideCategory = true;
					builderFlowNode2.overrideCategory = categoryFromChar;
					if (builderFlowNode == null)
					{
						builderFlowNode2.parentBuilderNode = parentBuilderNode;
						list.Add(builderFlowNode2);
					}
					else
					{
						builderFlowNode2.parentBuilderNode = builderFlowNode;
						builderFlowNode.childBuilderNodes = new List<BuilderFlowNode>();
						builderFlowNode.childBuilderNodes.Add(builderFlowNode2);
					}
					builderFlowNode = builderFlowNode2;
				}
			}
			else if (BraveRandom.GenerationRandomValue() <= nodeFromGuid.percentChance)
			{
				list.Add(new BuilderFlowNode(nodeFromGuid));
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			if (list[l].parentBuilderNode == null)
			{
				list[l].parentBuilderNode = parentBuilderNode;
			}
		}
		return list;
	}

	public List<FlowNodeBuildData> GetNodeChildrenToBuild(DungeonFlowNode source, DungeonFlowBuilder builder)
	{
		List<FlowNodeBuildData> list = new List<FlowNodeBuildData>();
		for (int i = 0; i < source.childNodeGuids.Count; i++)
		{
			DungeonFlowNode nodeFromGuid = GetNodeFromGuid(source.childNodeGuids[i]);
			if (nodeFromGuid.nodeType == DungeonFlowNode.ControlNodeType.SELECTOR)
			{
				List<DungeonFlowNode> selectorNodeChildren = GetSelectorNodeChildren(nodeFromGuid);
				for (int j = 0; j < selectorNodeChildren.Count; j++)
				{
					list.Add(new FlowNodeBuildData(selectorNodeChildren[j]));
				}
			}
			else if (nodeFromGuid.nodeType == DungeonFlowNode.ControlNodeType.SUBCHAIN)
			{
				DungeonFlowNode subchainRootFromNode = GetSubchainRootFromNode(nodeFromGuid, builder);
				if (!(subchainRootFromNode == null))
				{
					if (builder.usedSubchainData.ContainsKey(subchainRootFromNode))
					{
						builder.usedSubchainData[subchainRootFromNode] = builder.usedSubchainData[subchainRootFromNode] + 1;
					}
					else
					{
						builder.usedSubchainData.Add(subchainRootFromNode, 1);
					}
					list.Add(new FlowNodeBuildData(subchainRootFromNode));
				}
			}
			else if (nodeFromGuid.nodeExpands)
			{
				string text = nodeFromGuid.EvolveChainToCompletion();
				FlowNodeBuildData flowNodeBuildData = null;
				foreach (char c in text)
				{
					PrototypeDungeonRoom.RoomCategory categoryFromChar = GetCategoryFromChar(c);
					FlowNodeBuildData flowNodeBuildData2 = new FlowNodeBuildData(nodeFromGuid);
					flowNodeBuildData2.usesOverrideCategory = true;
					flowNodeBuildData2.overrideCategory = categoryFromChar;
					if (flowNodeBuildData == null)
					{
						list.Add(flowNodeBuildData2);
					}
					else
					{
						flowNodeBuildData.childBuildData = new List<FlowNodeBuildData>();
						flowNodeBuildData.childBuildData.Add(flowNodeBuildData2);
					}
					flowNodeBuildData = flowNodeBuildData2;
				}
			}
			else if (BraveRandom.GenerationRandomValue() <= nodeFromGuid.percentChance)
			{
				list.Add(new FlowNodeBuildData(nodeFromGuid));
			}
		}
		int num = -1;
		for (int l = 0; l < list.Count; l++)
		{
			if (SubchainContainsRoomOfType(list[l].node, PrototypeDungeonRoom.RoomCategory.EXIT))
			{
				num = l;
				break;
			}
		}
		if (num != -1 && num != 0)
		{
			FlowNodeBuildData item = list[num];
			list.RemoveAt(num);
			list.Insert(0, item);
		}
		return list;
	}

	public List<DungeonFlowNode> GetCapChainRootNodes(DungeonFlowBuilder builder)
	{
		List<DungeonFlowNode> list = new List<DungeonFlowNode>();
		for (int i = 0; i < m_nodes.Count; i++)
		{
			if (m_nodes[i].capSubchain && (!m_nodes[i].limitedCopiesOfSubchain || !builder.usedSubchainData.ContainsKey(m_nodes[i]) || builder.usedSubchainData[m_nodes[i]] < m_nodes[i].maxCopiesOfSubchain))
			{
				list.Add(m_nodes[i]);
			}
		}
		return list;
	}

	public bool SubchainContainsRoomOfType(DungeonFlowNode baseNode, PrototypeDungeonRoom.RoomCategory type)
	{
		List<DungeonFlowNode> list = new List<DungeonFlowNode>();
		GetNodesRecursive(baseNode, list);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].roomCategory == type)
			{
				return true;
			}
		}
		return false;
	}

	public List<DungeonFlowNode> GetSelectorNodeChildren(DungeonFlowNode source)
	{
		BraveUtility.Assert(source.nodeType != DungeonFlowNode.ControlNodeType.SELECTOR, "Processing selector node on non-selector node.");
		int num = BraveRandom.GenerationRandomRange(source.minChildrenToBuild, source.maxChildrenToBuild + 1);
		List<DungeonFlowNode> list = new List<DungeonFlowNode>();
		float num2 = 0f;
		for (int i = 0; i < source.childNodeGuids.Count; i++)
		{
			DungeonFlowNode nodeFromGuid = GetNodeFromGuid(source.childNodeGuids[i]);
			num2 += nodeFromGuid.percentChance;
		}
		List<string> list2 = new List<string>(source.childNodeGuids);
		for (int j = 0; j < num; j++)
		{
			float num3 = BraveRandom.GenerationRandomValue() * num2;
			float num4 = 0f;
			for (int k = 0; k < list2.Count; k++)
			{
				DungeonFlowNode nodeFromGuid2 = GetNodeFromGuid(list2[k]);
				num4 += nodeFromGuid2.percentChance;
				if (num4 >= num3)
				{
					list.Add(nodeFromGuid2);
					if (!source.canBuildDuplicateChildren)
					{
						num2 -= nodeFromGuid2.percentChance;
						list2.RemoveAt(k);
					}
					break;
				}
			}
		}
		return list;
	}

	public DungeonFlowNode GetNodeFromGuid(string guid)
	{
		int num = m_nodeGuids.IndexOf(guid);
		if (num >= 0)
		{
			return m_nodes[num];
		}
		return null;
	}

	public void ConnectNodes(DungeonFlowNode parent, DungeonFlowNode child)
	{
		if (!string.IsNullOrEmpty(parent.parentNodeGuid))
		{
			string parentNodeGuid = parent.parentNodeGuid;
			while (!string.IsNullOrEmpty(parentNodeGuid))
			{
				if (parentNodeGuid == child.guidAsString)
				{
					return;
				}
				DungeonFlowNode nodeFromGuid = GetNodeFromGuid(parentNodeGuid);
				parentNodeGuid = nodeFromGuid.parentNodeGuid;
			}
		}
		if (!string.IsNullOrEmpty(child.parentNodeGuid))
		{
			DungeonFlowNode nodeFromGuid2 = GetNodeFromGuid(child.parentNodeGuid);
			nodeFromGuid2.childNodeGuids.Remove(child.guidAsString);
		}
		if (parent.loopTargetNodeGuid == child.guidAsString)
		{
			parent.loopTargetNodeGuid = string.Empty;
		}
		if (child.loopTargetNodeGuid == parent.guidAsString)
		{
			child.loopTargetNodeGuid = string.Empty;
		}
		child.parentNodeGuid = parent.guidAsString;
		parent.childNodeGuids.Add(child.guidAsString);
	}

	public void LoopConnectNodes(DungeonFlowNode chainEnd, DungeonFlowNode loopTarget)
	{
		if (chainEnd.childNodeGuids.Contains(loopTarget.guidAsString) || loopTarget.childNodeGuids.Contains(chainEnd.guidAsString))
		{
			DisconnectNodes(chainEnd, loopTarget);
		}
		if (chainEnd.loopTargetNodeGuid == loopTarget.guidAsString)
		{
			if (chainEnd.loopTargetIsOneWay)
			{
				chainEnd.loopTargetIsOneWay = false;
				chainEnd.loopTargetNodeGuid = string.Empty;
			}
			else
			{
				chainEnd.loopTargetIsOneWay = true;
			}
		}
		else
		{
			chainEnd.loopTargetNodeGuid = loopTarget.guidAsString;
		}
	}

	public void DisconnectNodes(DungeonFlowNode node1, DungeonFlowNode node2)
	{
		if (node1.childNodeGuids.Contains(node2.guidAsString))
		{
			node1.childNodeGuids.Remove(node2.guidAsString);
			node2.parentNodeGuid = string.Empty;
		}
		else if (node2.childNodeGuids.Contains(node1.guidAsString))
		{
			node2.childNodeGuids.Remove(node1.guidAsString);
			node1.parentNodeGuid = string.Empty;
		}
		if (node1.loopTargetNodeGuid == node2.guidAsString)
		{
			node1.loopTargetNodeGuid = string.Empty;
		}
		else if (node2.loopTargetNodeGuid == node1.guidAsString)
		{
			node2.loopTargetNodeGuid = string.Empty;
		}
	}

	private DungeonFlowNode GetRootOfNode(DungeonFlowNode node)
	{
		DungeonFlowNode dungeonFlowNode = node;
		while (!string.IsNullOrEmpty(dungeonFlowNode.parentNodeGuid))
		{
			dungeonFlowNode = GetNodeFromGuid(dungeonFlowNode.parentNodeGuid);
		}
		return dungeonFlowNode;
	}
}
