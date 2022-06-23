using System.Collections.Generic;
using Dungeonator;

public class FloodFillUtility
{
	private static List<int> s_openList = new List<int>();

	private static bool[] s_reachable = new bool[0];

	private static IntVector2 m_areaMin;

	private static IntVector2 m_areaDim;

	public static void PreprocessContiguousCells(RoomHandler room, IntVector2 myPos, int bufferCells = 0)
	{
		DungeonData data = GameManager.Instance.Dungeon.data;
		m_areaMin = room.area.basePosition - new IntVector2(bufferCells, bufferCells);
		m_areaDim = room.area.dimensions + new IntVector2(2 * bufferCells, 2 * bufferCells);
		int num = m_areaDim.x * m_areaDim.y;
		if (s_reachable.Length < num)
		{
			s_reachable = new bool[num];
		}
		for (int i = 0; i < num; i++)
		{
			s_reachable[i] = false;
		}
		s_openList.Clear();
		if (data.GetCellTypeSafe(myPos) == CellType.FLOOR)
		{
			int num2 = myPos.x - m_areaMin.x + (myPos.y - m_areaMin.y) * m_areaDim.x;
			s_openList.Add(num2);
			s_reachable[num2] = true;
		}
		int num3 = 0;
		while (s_openList.Count > 0 && num3 < 1000)
		{
			int num4 = s_openList[0];
			int num5 = s_openList[0] % m_areaDim.x;
			int num6 = s_openList[0] / m_areaDim.x;
			int num7 = m_areaMin.x + num5;
			int num8 = m_areaMin.y + num6;
			s_openList.RemoveAt(0);
			int num9 = -1;
			if (num5 > 0 && data.GetCellTypeSafe(num7 - 1, num8) == CellType.FLOOR && !s_reachable[num4 + num9])
			{
				s_reachable[num4 + num9] = true;
				s_openList.Add(num4 + num9);
			}
			num9 = 1;
			if (num5 < m_areaDim.x - 1 && data.GetCellTypeSafe(num7 + 1, num8) == CellType.FLOOR && !s_reachable[num4 + num9])
			{
				s_reachable[num4 + num9] = true;
				s_openList.Add(num4 + num9);
			}
			num9 = -m_areaDim.x;
			if (num6 > 0 && data.GetCellTypeSafe(num7, num8 - 1) == CellType.FLOOR && !s_reachable[num4 + num9])
			{
				s_reachable[num4 + num9] = true;
				s_openList.Add(num4 + num9);
			}
			num9 = m_areaDim.x;
			if (num6 < m_areaDim.y - 1 && data.GetCellTypeSafe(num7, num8 + 1) == CellType.FLOOR && !s_reachable[num4 + num9])
			{
				s_reachable[num4 + num9] = true;
				s_openList.Add(num4 + num9);
			}
			num3++;
		}
	}

	public static bool WasFilled(IntVector2 c)
	{
		return s_reachable[c.x - m_areaMin.x + (c.y - m_areaMin.y) * m_areaDim.x];
	}
}
