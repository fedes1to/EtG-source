using System;
using System.Collections;
using Dungeonator;
using HutongGames.PlayMaker.Actions;

public class InfiniteRunnerController : BraveBehaviour, IPlaceConfigurable
{
	[NonSerialized]
	public RoomHandler TargetRoom;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		StartCoroutine(HandleDelayedInitialization(room));
	}

	private IEnumerator HandleDelayedInitialization(RoomHandler room)
	{
		yield return null;
		room.TransferInteractableOwnershipToDungeon(base.talkDoer);
		TargetRoom = room.injectionFrameData[room.injectionFrameData.Count - 1];
		PlayMakerFSM dungeonFsm = GetDungeonFSM();
		for (int i = 0; i < dungeonFsm.FsmStates.Length; i++)
		{
			for (int j = 0; j < dungeonFsm.FsmStates[i].Actions.Length; j++)
			{
				if (dungeonFsm.FsmStates[i].Actions[j] is CheckRoomVisited)
				{
					CheckRoomVisited checkRoomVisited = dungeonFsm.FsmStates[i].Actions[j] as CheckRoomVisited;
					checkRoomVisited.targetRoom = TargetRoom;
				}
			}
		}
	}

	public void StartQuest()
	{
		base.talkDoer.PathfindToPosition(TargetRoom.GetCenterCell().ToVector2());
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
