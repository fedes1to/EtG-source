using Dungeonator;
using UnityEngine;

public class PitTrapController : BasicTrapController
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 loc, bool deferConfiguration = false)
	{
		IntVector2 intVector = loc + targetRoom.area.basePosition;
		for (int i = intVector.x; i < intVector.x + placeableWidth; i++)
		{
			for (int j = intVector.y; j < intVector.y + placeableHeight; j++)
			{
				CellData cellData = GameManager.Instance.Dungeon.data.cellData[i][j];
				cellData.type = CellType.PIT;
				cellData.fallingPrevented = true;
			}
		}
		return base.InstantiateObject(targetRoom, loc, deferConfiguration);
	}

	protected override void BeginState(State newState)
	{
		switch (newState)
		{
		case State.Active:
		{
			for (int k = m_cachedPosition.x; k < m_cachedPosition.x + placeableWidth; k++)
			{
				for (int l = m_cachedPosition.y; l < m_cachedPosition.y + placeableHeight; l++)
				{
					GameManager.Instance.Dungeon.data.cellData[k][l].fallingPrevented = false;
				}
			}
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.enabled = false;
			}
			break;
		}
		case State.Resetting:
		{
			for (int i = m_cachedPosition.x; i < m_cachedPosition.x + placeableWidth; i++)
			{
				for (int j = m_cachedPosition.y; j < m_cachedPosition.y + placeableHeight; j++)
				{
					GameManager.Instance.Dungeon.data.cellData[i][j].fallingPrevented = true;
				}
			}
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.enabled = true;
			}
			break;
		}
		}
		base.BeginState(newState);
	}
}
