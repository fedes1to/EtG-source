using System;
using System.Collections.Generic;

[Serializable]
public class NeighborDependencyData
{
	public List<IndexNeighborDependency> neighborDependencies;

	public NeighborDependencyData(List<IndexNeighborDependency> bcs)
	{
		neighborDependencies = bcs;
	}
}
