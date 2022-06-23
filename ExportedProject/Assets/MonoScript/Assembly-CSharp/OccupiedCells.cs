using Dungeonator;
using Pathfinding;
using UnityEngine;

public class OccupiedCells
{
	protected RoomHandler m_cachedRoom;

	protected SpeculativeRigidbody m_specRigidbody;

	protected PixelCollider m_pixelCollider;

	protected bool m_usesCustom;

	protected IntVector2 m_customBasePosition;

	protected IntVector2 m_customDimensions;

	public OccupiedCells(SpeculativeRigidbody specRigidbody, RoomHandler room)
		: this(specRigidbody, specRigidbody.PrimaryPixelCollider, room)
	{
	}

	public OccupiedCells(SpeculativeRigidbody specRigidbody, PixelCollider pixelCollider, RoomHandler room)
	{
		m_specRigidbody = specRigidbody;
		m_pixelCollider = pixelCollider;
		m_cachedRoom = room;
		if (m_cachedRoom == null)
		{
			Debug.LogError("error on: " + m_specRigidbody.name + m_specRigidbody.transform.position);
		}
		Pathfinder.Instance.RegisterObstacle(this, m_cachedRoom);
	}

	public OccupiedCells(IntVector2 basePosition, IntVector2 dimensions, RoomHandler room)
	{
		m_usesCustom = true;
		m_customBasePosition = basePosition;
		m_customDimensions = dimensions;
		m_cachedRoom = room;
		if (m_cachedRoom == null)
		{
			Debug.LogError("error on: " + m_specRigidbody.name + m_specRigidbody.transform.position);
		}
		Pathfinder.Instance.RegisterObstacle(this, m_cachedRoom);
	}

	public void Clear()
	{
		if (GameManager.HasInstance && !GameManager.Instance.IsLoadingLevel && PhysicsEngine.HasInstance && (bool)m_specRigidbody && (bool)GameManager.Instance.Dungeon)
		{
			if (m_usesCustom)
			{
				RoomHandler absoluteRoom = m_customBasePosition.ToVector3().GetAbsoluteRoom();
				if (absoluteRoom != null)
				{
					m_cachedRoom = absoluteRoom;
				}
			}
			else
			{
				RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(m_specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
				if (absoluteRoomFromPosition != null)
				{
					m_cachedRoom = absoluteRoomFromPosition;
				}
			}
		}
		if (Pathfinder.HasInstance && m_cachedRoom != null)
		{
			Pathfinder.Instance.DeregisterObstacle(this, m_cachedRoom);
		}
	}

	public void FlagCells()
	{
		if (!GameManager.HasInstance || GameManager.Instance.Dungeon == null || GameManager.Instance.Dungeon.data == null)
		{
			return;
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		if (m_usesCustom)
		{
			IntVector2 customBasePosition = m_customBasePosition;
			IntVector2 intVector = m_customBasePosition + m_customDimensions;
			for (int i = customBasePosition.x; i < intVector.x; i++)
			{
				for (int j = customBasePosition.y; j < intVector.y; j++)
				{
					CellData cellData = data.cellData[i][j];
					if (cellData != null)
					{
						cellData.isOccupied = true;
					}
				}
			}
			return;
		}
		IntVector2 intVector2 = m_pixelCollider.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
		IntVector2 intVector3 = m_pixelCollider.UnitTopRight.ToIntVector2(VectorConversions.Ceil);
		for (int k = intVector2.x; k < intVector3.x; k++)
		{
			for (int l = intVector2.y; l < intVector3.y; l++)
			{
				CellData cellData2 = data.cellData[k][l];
				if (cellData2 != null)
				{
					cellData2.isOccupied = true;
				}
			}
		}
	}

	public void UpdateCells()
	{
		Pathfinder.Instance.FlagRoomAsDirty(m_cachedRoom);
	}
}
