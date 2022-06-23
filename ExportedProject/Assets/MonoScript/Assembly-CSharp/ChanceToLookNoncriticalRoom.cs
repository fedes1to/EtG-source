using Dungeonator;
using UnityEngine;

public class ChanceToLookNoncriticalRoom : MonoBehaviour, IPlaceConfigurable
{
	public float chanceMin = 0.3f;

	public float chanceMax = 0.5f;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		if (room.connectedRooms.Count == 1)
		{
			room.ShouldAttemptProceduralLock = true;
			room.AttemptProceduralLockChance = Random.Range(chanceMin, chanceMax);
		}
	}
}
