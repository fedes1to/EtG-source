using Dungeonator;
using UnityEngine;

public class TrapController : DungeonPlaceableBehaviour
{
	public string TrapSwitchState;

	protected bool m_markCellOccupied = true;

	public virtual void Start()
	{
		if (!string.IsNullOrEmpty(TrapSwitchState))
		{
			AkSoundEngine.SetSwitch("ENV_Trap", TrapSwitchState, base.gameObject);
		}
	}

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
				if (m_markCellOccupied)
				{
					GameManager.Instance.Dungeon.data.cellData[i][j].isOccupied = true;
				}
				GameManager.Instance.Dungeon.data.cellData[i][j].containsTrap = true;
			}
		}
		return base.InstantiateObject(targetRoom, loc, deferConfiguration);
	}
}
