using System;
using Dungeonator;

[Serializable]
public class IndexNeighborDependency
{
	public DungeonData.Direction neighborDirection;

	public int neighborIndex = -1;
}
