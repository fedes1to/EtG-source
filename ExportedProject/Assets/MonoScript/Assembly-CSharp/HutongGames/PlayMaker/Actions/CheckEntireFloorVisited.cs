using System;
using Dungeonator;

namespace HutongGames.PlayMaker.Actions
{
	public class CheckEntireFloorVisited : FsmStateAction
	{
		public static bool IsQuestCallbackActive;

		[Tooltip("Event sent if there are.")]
		public FsmEvent HasVisited;

		[Tooltip("Event sent if there aren't.")]
		public FsmEvent HasNotVisited;

		public FsmBool IncludeSecretRooms = false;

		public FsmBool IncludeWarpRooms = false;

		public FsmBool OnlyIncludeStandardRooms = true;

		public override void Awake()
		{
			base.Awake();
		}

		public override void OnEnter()
		{
			if (TestCompletion())
			{
				if (IsQuestCallbackActive)
				{
					Dungeon dungeon = GameManager.Instance.Dungeon;
					dungeon.OnAnyRoomVisited = (Action)Delegate.Remove(dungeon.OnAnyRoomVisited, new Action(NotifyComplete));
					IsQuestCallbackActive = false;
				}
				base.Fsm.Event(HasVisited);
			}
			else
			{
				if (!IsQuestCallbackActive)
				{
					Dungeon dungeon2 = GameManager.Instance.Dungeon;
					dungeon2.OnAnyRoomVisited = (Action)Delegate.Combine(dungeon2.OnAnyRoomVisited, new Action(NotifyComplete));
					IsQuestCallbackActive = true;
				}
				base.Fsm.Event(HasNotVisited);
			}
			Finish();
		}

		private bool TestCompletion()
		{
			bool result = GameManager.Instance.Dungeon.AllRoomsVisited;
			if (!IncludeSecretRooms.Value || !IncludeWarpRooms.Value || OnlyIncludeStandardRooms.Value)
			{
				result = true;
				for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
				{
					RoomHandler roomHandler = GameManager.Instance.Dungeon.data.rooms[i];
					bool isSecretRoom = roomHandler.IsSecretRoom;
					bool isStartOfWarpWing = roomHandler.IsStartOfWarpWing;
					bool flag = roomHandler.visibility == RoomHandler.VisibilityStatus.OBSCURED;
					bool flag2 = roomHandler.IsStandardRoom || roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SPECIAL || roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD;
					if (!roomHandler.OverrideTilemap && flag && (IncludeSecretRooms.Value || !isSecretRoom) && (IncludeWarpRooms.Value || !isStartOfWarpWing) && (!OnlyIncludeStandardRooms.Value || flag2) && !roomHandler.RevealedOnMap)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		private void NotifyComplete()
		{
			if (TestCompletion())
			{
				IsQuestCallbackActive = false;
				Dungeon dungeon = GameManager.Instance.Dungeon;
				dungeon.OnAnyRoomVisited = (Action)Delegate.Remove(dungeon.OnAnyRoomVisited, new Action(NotifyComplete));
				GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetString("#LOSTADVENTURER_NOTIFICATION_HEADER"), StringTableManager.GetString("#LOSTADVENTURER_NOTIFICATION_BODY"), base.Owner.GetComponent<TalkDoerLite>().OptionalCustomNotificationSprite.Collection, base.Owner.GetComponent<TalkDoerLite>().OptionalCustomNotificationSprite.spriteId, UINotificationController.NotificationColor.GOLD);
			}
		}
	}
}
