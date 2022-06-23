using System.Collections.Generic;

namespace Dungeonator
{
	public class FlowCompositeMetastructure
	{
		public List<List<BuilderFlowNode>> loopLists = new List<List<BuilderFlowNode>>();

		public List<List<BuilderFlowNode>> compositeLists = new List<List<BuilderFlowNode>>();

		public List<BuilderFlowNode> usedList = new List<BuilderFlowNode>();

		public bool ContainedInBidirectionalLoop(BuilderFlowNode node)
		{
			for (int i = 0; i < loopLists.Count; i++)
			{
				if (!loopLists[i].Contains(node))
				{
					continue;
				}
				bool result = true;
				for (int j = 0; j < loopLists[i].Count; j++)
				{
					if (loopLists[i][j].loopConnectedBuilderNode != null && loopLists[i][j].node.loopTargetIsOneWay)
					{
						result = false;
					}
				}
				return result;
			}
			return false;
		}
	}
}
