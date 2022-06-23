using System;

namespace Dungeonator
{
	[Flags]
	public enum CellTypes
	{
		WALL = 1,
		FLOOR = 2,
		PIT = 4
	}
}
