using System;
using Dungeonator;

public class EncounterOnRoomClear : BraveBehaviour
{
	public void Start()
	{
		RoomHandler parentRoom = base.aiActor.ParentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom.OnEnemiesCleared, new Action(RoomCleared));
	}

	protected override void OnDestroy()
	{
		if ((bool)base.aiActor && base.aiActor.ParentRoom != null)
		{
			RoomHandler parentRoom = base.aiActor.ParentRoom;
			parentRoom.OnEnemiesCleared = (Action)Delegate.Remove(parentRoom.OnEnemiesCleared, new Action(RoomCleared));
		}
		base.OnDestroy();
	}

	private void RoomCleared()
	{
		if ((bool)base.encounterTrackable)
		{
			GameStatsManager.Instance.HandleEncounteredObject(base.encounterTrackable);
		}
	}
}
