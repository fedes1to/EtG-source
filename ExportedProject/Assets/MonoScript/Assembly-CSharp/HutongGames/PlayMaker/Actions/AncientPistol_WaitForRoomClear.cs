using System.Collections;
using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Paths to near the player's current location.")]
	[ActionCategory(".NPCs")]
	public class AncientPistol_WaitForRoomClear : FsmStateAction
	{
		[Tooltip("Event sent if there are more rooms.")]
		public FsmEvent MoreRooms;

		[Tooltip("Event sent if there aren't.")]
		public FsmEvent NoMoreRooms;

		private AncientPistolController m_pistolDoer;

		private int m_currentTargetRoomIndex;

		private Vector2 m_lastPosition;

		public override void Awake()
		{
			base.Awake();
			m_pistolDoer = base.Owner.GetComponent<AncientPistolController>();
			m_currentTargetRoomIndex = 0;
		}

		public override void OnEnter()
		{
			m_lastPosition = m_pistolDoer.specRigidbody.UnitCenter;
			Vector2 zero = Vector2.zero;
			RoomHandler otherRoom = m_pistolDoer.RoomSequence[m_currentTargetRoomIndex];
			RoomHandler roomHandler = m_pistolDoer.talkDoer.GetAbsoluteParentRoom();
			if (m_currentTargetRoomIndex > 0)
			{
				roomHandler = m_pistolDoer.RoomSequence[m_currentTargetRoomIndex - 1];
			}
			PrototypeRoomExit exitConnectedToRoom = roomHandler.GetExitConnectedToRoom(otherRoom);
			if (exitConnectedToRoom != null)
			{
				zero = (exitConnectedToRoom.GetExitOrigin(0) - IntVector2.One + roomHandler.area.basePosition + -5 * DungeonData.GetIntVector2FromDirection(exitConnectedToRoom.exitDirection)).ToVector2();
				zero = roomHandler.GetBestRewardLocation(IntVector2.One, zero, false).ToVector2();
				m_pistolDoer.StartCoroutine(HandleDelayedPathing(zero));
			}
		}

		private IEnumerator HandleDelayedPathing(Vector2 targetPosition)
		{
			yield return new WaitForSeconds(0.5f);
			m_pistolDoer.talkDoer.PathfindToPosition(targetPosition);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			if (m_pistolDoer.talkDoer.CurrentPath == null)
			{
				return;
			}
			if (!m_pistolDoer.talkDoer.CurrentPath.WillReachFinalGoal)
			{
				RoomHandler otherRoom = m_pistolDoer.RoomSequence[m_currentTargetRoomIndex];
				RoomHandler roomHandler = ((m_currentTargetRoomIndex != 0) ? m_pistolDoer.RoomSequence[m_currentTargetRoomIndex - 1] : m_pistolDoer.talkDoer.GetAbsoluteParentRoom());
				PrototypeRoomExit exitConnectedToRoom = roomHandler.GetExitConnectedToRoom(otherRoom);
				if (exitConnectedToRoom != null)
				{
					IntVector2 intVector = exitConnectedToRoom.GetExitOrigin(0) - IntVector2.One + roomHandler.area.basePosition + -5 * DungeonData.GetIntVector2FromDirection(exitConnectedToRoom.exitDirection);
					m_pistolDoer.transform.position = intVector.ToVector3();
					m_pistolDoer.specRigidbody.Reinitialize();
					PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(m_pistolDoer.specRigidbody, CollisionMask.LayerToMask(CollisionLayer.PlayerCollider));
					m_pistolDoer.talkDoer.CurrentPath = null;
				}
			}
			else
			{
				m_pistolDoer.talkDoer.specRigidbody.Velocity = m_pistolDoer.talkDoer.GetPathVelocityContribution(m_lastPosition, 32);
				m_lastPosition = m_pistolDoer.talkDoer.specRigidbody.UnitCenter;
			}
		}

		public override void OnLateUpdate()
		{
			if (m_pistolDoer.talkDoer.CurrentPath != null)
			{
				return;
			}
			RoomHandler roomHandler = m_pistolDoer.RoomSequence[m_currentTargetRoomIndex];
			bool flag = GameManager.Instance.BestActivePlayer.CurrentRoom != roomHandler && m_currentTargetRoomIndex == m_pistolDoer.RoomSequence.Count - 1;
			if (!roomHandler.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear) && !flag)
			{
				m_currentTargetRoomIndex++;
				if (m_currentTargetRoomIndex >= m_pistolDoer.RoomSequence.Count)
				{
					base.Fsm.Event(NoMoreRooms);
				}
				else
				{
					base.Fsm.Event(MoreRooms);
				}
				Finish();
			}
		}
	}
}
