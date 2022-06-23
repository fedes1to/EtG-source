using Dungeonator;
using UnityEngine;

public class OcclusionLayer
{
	public Color occludedColor;

	public int cachedX;

	public int cachedY;

	private GameManager m_gameManagerCached;

	private PlayerController[] m_allPlayersCached;

	private Pixelator m_pixelatorCached;

	private bool m_playerOneDead;

	private bool m_playerTwoDead;

	protected Texture2D m_occlusionTexture;

	protected int textureMultiplier = 1;

	protected float[] KERNEL = new float[5] { 0.12f, 0.25f, 0.3f, 0.25f, 0.12f };

	protected Color[] m_colorCache;

	public Texture2D SourceOcclusionTexture
	{
		get
		{
			return m_occlusionTexture;
		}
		set
		{
			m_occlusionTexture = value;
		}
	}

	protected float GetCellOcclusion(int x0, int y0, DungeonData d)
	{
		float num = ((d.cellData[x0][y0] != null) ? d.cellData[x0][y0].occlusionData.cellOcclusion : 1f);
		if (!m_pixelatorCached.UseTexturedOcclusion && x0 >= 2 && y0 >= 2 && x0 < d.Width - 2 && y0 < d.Height - 2)
		{
			float num2 = 0f;
			float num3 = 0f;
			for (int i = -2; i <= 2; i++)
			{
				for (int j = -2; j <= 2; j++)
				{
					float num4 = KERNEL[i + 2] * KERNEL[j + 2];
					float num5 = 0f;
					num5 = ((d.cellData[x0 + i][y0 + j] != null) ? (d.cellData[x0 + i][y0 + j].occlusionData.cellOcclusion * num4) : (1f * num4));
					num2 += num5;
					num3 += num4;
				}
			}
			return Mathf.Min(num, num2 / num3);
		}
		return num;
	}

	protected float GetGValueForCell(int x0, int y0, DungeonData d)
	{
		float result = 0f;
		if (x0 < 0 || x0 >= d.Width || y0 < 0 || y0 >= d.Height)
		{
			return result;
		}
		CellData cellData = d[x0, y0];
		if (cellData == null)
		{
			return result;
		}
		bool useTexturedOcclusion = m_pixelatorCached.UseTexturedOcclusion;
		if (cellData.type == CellType.FLOOR || cellData.type == CellType.PIT || cellData.IsLowerFaceWall() || (cellData.IsUpperFacewall() && !useTexturedOcclusion))
		{
			if (cellData.nearestRoom.visibility == RoomHandler.VisibilityStatus.CURRENT)
			{
				result = 1f * (1f - cellData.occlusionData.minCellOccluionHistory);
			}
			else if (cellData.nearestRoom.hasEverBeenVisited && cellData.nearestRoom.visibility != RoomHandler.VisibilityStatus.REOBSCURED)
			{
				result = 1f;
			}
		}
		return result;
	}

	protected float GetRValueForCell(int x0, int y0, DungeonData d)
	{
		float result = 0f;
		if (m_pixelatorCached.UseTexturedOcclusion)
		{
			return result;
		}
		if (x0 < 0 || x0 >= d.Width || y0 < 0 || y0 >= d.Height)
		{
			return result;
		}
		CellData cellData = d[x0, y0];
		if (cellData == null)
		{
			return result;
		}
		if (cellData.isExitCell)
		{
			return result;
		}
		if (cellData.type == CellType.WALL && !cellData.IsAnyFaceWall())
		{
			return result;
		}
		if (y0 - 2 >= 0 && d[x0, y0 - 2] != null && d[x0, y0 - 2].isExitCell)
		{
			return result;
		}
		RoomHandler roomHandler = d[x0, y0].parentRoom ?? d[x0, y0].nearestRoom;
		bool flag = false;
		if (roomHandler != null)
		{
			for (int i = 0; i < m_allPlayersCached.Length; i++)
			{
				if ((i != 0 || !m_playerOneDead) && (i != 1 || !m_playerTwoDead) && m_allPlayersCached[i].CurrentRoom != null && m_allPlayersCached[i].CurrentRoom.connectedRooms != null && m_allPlayersCached[i].CurrentRoom.connectedRooms.Contains(roomHandler))
				{
					flag = true;
				}
			}
		}
		if (x0 < 1 || x0 > d.Width - 2 || y0 < 3 || y0 > d.Height - 2)
		{
			return result;
		}
		if (roomHandler == null || roomHandler.visibility == RoomHandler.VisibilityStatus.OBSCURED || roomHandler.visibility == RoomHandler.VisibilityStatus.REOBSCURED)
		{
			if (flag)
			{
				if (cellData.isExitNonOccluder)
				{
					return result;
				}
				if (cellData.isExitCell)
				{
					return result;
				}
				if (y0 > 1 && d[x0, y0 - 1] != null && d[x0, y0 - 1].isExitCell)
				{
					return result;
				}
				if (y0 > 2 && d[x0, y0 - 2] != null && d[x0, y0 - 2].isExitCell)
				{
					return result;
				}
				if (y0 > 3 && d[x0, y0 - 3] != null && d[x0, y0 - 3].isExitCell)
				{
					return result;
				}
				if (x0 > 1 && d[x0 - 1, y0] != null && d[x0 - 1, y0].isExitCell)
				{
					return result;
				}
				if (x0 < d.Width - 1 && d[x0 + 1, y0] != null && d[x0 + 1, y0].isExitCell)
				{
					return result;
				}
			}
			result = 1f;
		}
		return result;
	}

	protected Color GetInterpolatedValueAtPoint(int baseX, int baseY, float worldX, float worldY, DungeonData d)
	{
		int num = baseX + Mathf.FloorToInt(worldX);
		int num2 = baseY + Mathf.FloorToInt(worldY);
		num = baseX + (int)worldX;
		num2 = baseY + (int)worldY;
		float rValueForCell = GetRValueForCell(num, num2, d);
		float gValueForCell = GetGValueForCell(num, num2, d);
		if (!d.CheckInBounds(num, num2))
		{
			return new Color(rValueForCell, gValueForCell, 0f, 0f);
		}
		float num3 = Mathf.Clamp01(GetCellOcclusion(num, num2, d));
		float a = 1f - num3 * num3;
		return new Color(rValueForCell, gValueForCell, 0f, a);
	}

	public Texture2D GenerateOcclusionTexture(int baseX, int baseY, DungeonData d)
	{
		m_gameManagerCached = GameManager.Instance;
		m_pixelatorCached = Pixelator.Instance;
		m_allPlayersCached = GameManager.Instance.AllPlayers;
		m_playerOneDead = !m_gameManagerCached.PrimaryPlayer || m_gameManagerCached.PrimaryPlayer.healthHaver.IsDead;
		if (m_gameManagerCached.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			m_playerTwoDead = !m_gameManagerCached.SecondaryPlayer || m_gameManagerCached.SecondaryPlayer.healthHaver.IsDead;
		}
		int num = m_pixelatorCached.CurrentMacroResolutionX / 16 + 4;
		int num2 = m_pixelatorCached.CurrentMacroResolutionY / 16 + 4;
		int num3 = num * textureMultiplier;
		int num4 = num2 * textureMultiplier;
		if (m_occlusionTexture == null || m_occlusionTexture.width != num3 || m_occlusionTexture.height != num4)
		{
			if (m_occlusionTexture != null)
			{
				m_occlusionTexture.Resize(num3, num4);
			}
			else
			{
				m_occlusionTexture = new Texture2D(num3, num4, TextureFormat.ARGB32, false);
				m_occlusionTexture.filterMode = FilterMode.Bilinear;
				m_occlusionTexture.wrapMode = TextureWrapMode.Clamp;
			}
		}
		if (m_colorCache == null || m_colorCache.Length != num3 * num4)
		{
			m_colorCache = new Color[num3 * num4];
		}
		cachedX = baseX;
		cachedY = baseY;
		if (!m_gameManagerCached.IsLoadingLevel)
		{
			for (int i = 0; i < num3; i++)
			{
				for (int j = 0; j < num4; j++)
				{
					int num5 = j * num3 + i;
					float worldX = (float)i / (float)textureMultiplier;
					float worldY = (float)j / (float)textureMultiplier;
					m_colorCache[num5] = GetInterpolatedValueAtPoint(baseX, baseY, worldX, worldY, d);
				}
			}
		}
		m_occlusionTexture.SetPixels(m_colorCache);
		m_occlusionTexture.Apply();
		return m_occlusionTexture;
	}
}
