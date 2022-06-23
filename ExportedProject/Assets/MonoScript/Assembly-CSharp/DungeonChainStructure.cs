using System.Collections.Generic;
using Dungeonator;

public class DungeonChainStructure
{
	public FlowNodeBuildData parentNode;

	public FlowNodeBuildData optionalRequiredNode;

	public List<FlowNodeBuildData> containedNodes = new List<FlowNodeBuildData>();

	public int previousLoopDistanceMetric = int.MaxValue;
}
