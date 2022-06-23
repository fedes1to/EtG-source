using System;
using Dungeonator;

[Serializable]
public class RobotDaveIdea
{
	public DungeonPlaceable[] ValidEasyEnemyPlaceables;

	public DungeonPlaceable[] ValidHardEnemyPlaceables;

	public bool UseWallSawblades;

	public bool UseRollingLogsVertical;

	public bool UseRollingLogsHorizontal;

	public bool UseFloorPitTraps;

	public bool UseFloorFlameTraps;

	public bool UseFloorSpikeTraps;

	public bool UseFloorConveyorBelts;

	public bool UseCaveIns;

	public bool UseAlarmMushrooms;

	public bool UseMineCarts;

	public bool UseChandeliers;

	public bool CanIncludePits = true;

	public RobotDaveIdea()
	{
	}

	public RobotDaveIdea(RobotDaveIdea source)
	{
		if (source != null)
		{
			ValidEasyEnemyPlaceables = new DungeonPlaceable[(source.ValidEasyEnemyPlaceables != null) ? source.ValidEasyEnemyPlaceables.Length : 0];
			for (int i = 0; i < ValidEasyEnemyPlaceables.Length; i++)
			{
				ValidEasyEnemyPlaceables[i] = source.ValidEasyEnemyPlaceables[i];
			}
			ValidHardEnemyPlaceables = new DungeonPlaceable[(source.ValidHardEnemyPlaceables != null) ? source.ValidHardEnemyPlaceables.Length : 0];
			for (int j = 0; j < ValidHardEnemyPlaceables.Length; j++)
			{
				ValidHardEnemyPlaceables[j] = source.ValidHardEnemyPlaceables[j];
			}
			UseWallSawblades = source.UseWallSawblades;
			UseRollingLogsHorizontal = source.UseRollingLogsHorizontal;
			UseRollingLogsVertical = source.UseRollingLogsVertical;
			UseFloorPitTraps = source.UseFloorPitTraps;
			UseFloorFlameTraps = source.UseFloorFlameTraps;
			UseFloorSpikeTraps = source.UseFloorSpikeTraps;
			UseFloorConveyorBelts = source.UseFloorConveyorBelts;
			UseCaveIns = source.UseCaveIns;
			UseAlarmMushrooms = source.UseAlarmMushrooms;
			UseMineCarts = source.UseMineCarts;
			UseChandeliers = source.UseChandeliers;
			CanIncludePits = source.CanIncludePits;
		}
	}
}
