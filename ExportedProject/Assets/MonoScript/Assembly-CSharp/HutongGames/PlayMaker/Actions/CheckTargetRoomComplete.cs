using System;
using System.Collections.Generic;
using Dungeonator;

namespace HutongGames.PlayMaker.Actions
{
	public class CheckTargetRoomComplete : FsmStateAction
	{
		public FsmEvent noEnemies;

		public FsmEvent hasEnemies;

		private GunslingChallengeType ChallengeType;

		private TalkDoerLite m_talkDoer;

		private bool m_challengeInitialized;

		public override void Awake()
		{
			base.Awake();
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
		}

		public override void OnEnter()
		{
			bool flag = CheckRoom(true);
			if (!m_challengeInitialized)
			{
				ChooseRandomChallenge();
				m_challengeInitialized = true;
			}
			if (flag)
			{
				base.Fsm.Event(hasEnemies);
			}
			else
			{
				base.Fsm.Event(noEnemies);
			}
			Finish();
		}

		private bool CheckRoom(bool canFallback)
		{
			RoomHandler absoluteParentRoom = m_talkDoer.GetAbsoluteParentRoom();
			RoomHandler injectionTarget = absoluteParentRoom.injectionTarget;
			if (injectionTarget.visibility != 0 && injectionTarget.visibility != RoomHandler.VisibilityStatus.REOBSCURED && !injectionTarget.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
			{
				if (!canFallback)
				{
					return false;
				}
				if (absoluteParentRoom.distanceFromEntrance > injectionTarget.distanceFromEntrance)
				{
					for (int i = 0; i < absoluteParentRoom.connectedRooms.Count; i++)
					{
						if (absoluteParentRoom.connectedRooms[i] != null && absoluteParentRoom.connectedRooms[i].distanceFromEntrance > absoluteParentRoom.distanceFromEntrance && absoluteParentRoom.connectedRooms[i].EverHadEnemies)
						{
							absoluteParentRoom.injectionTarget = absoluteParentRoom.connectedRooms[i];
							break;
						}
					}
				}
				return CheckRoom(false);
			}
			return true;
		}

		private void ChooseRandomChallenge()
		{
			RoomHandler injectionTarget = m_talkDoer.GetAbsoluteParentRoom().injectionTarget;
			List<GunslingChallengeType> list = new List<GunslingChallengeType>((GunslingChallengeType[])Enum.GetValues(typeof(GunslingChallengeType)));
			if (GameManager.Instance.PrimaryPlayer != null && (GameManager.Instance.PrimaryPlayer.CharacterUsesRandomGuns || GameManager.Instance.PrimaryPlayer.IsGunLocked))
			{
				list.Remove(GunslingChallengeType.SPECIFIC_GUN);
			}
			if (!IsRoomTraversibleWithoutDodgeRolls(injectionTarget))
			{
				list.Remove(GunslingChallengeType.NO_DODGE_ROLL);
			}
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.DAISUKE_ACTIVE_IN_FOYER))
			{
				list.Remove(GunslingChallengeType.DAISUKE_CHALLENGES);
			}
			if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
			{
				list.Remove(GunslingChallengeType.DAISUKE_CHALLENGES);
			}
			ChallengeType = BraveUtility.RandomElement(list);
			base.Fsm.Variables.GetFsmInt("ChallengeType").Value = (int)ChallengeType;
		}

		private bool IsRoomTraversibleWithoutDodgeRolls(RoomHandler room)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			for (int i = 0; i < room.Cells.Count; i++)
			{
				if (data.CheckInBoundsAndValid(room.Cells[i]))
				{
					CellData cellData = data[room.Cells[i]];
					if (cellData.type == CellType.PIT)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
