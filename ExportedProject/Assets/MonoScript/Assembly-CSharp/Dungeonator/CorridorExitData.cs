using System.Collections.Generic;

namespace Dungeonator
{
	public class CorridorExitData
	{
		public List<CellData> cells;

		public RoomHandler linkedRoom;

		public CorridorExitData(List<CellData> c, RoomHandler rh)
		{
			cells = c;
			linkedRoom = rh;
		}
	}
}
