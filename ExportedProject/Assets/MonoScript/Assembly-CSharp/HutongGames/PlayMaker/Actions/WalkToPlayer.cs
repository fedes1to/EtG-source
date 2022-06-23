using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Paths to near the player's current location.")]
	[ActionCategory(".NPCs")]
	public class WalkToPlayer : FsmStateAction
	{
		public enum TargetPathType
		{
			PLAYER,
			PLAYER_ROOM_CENTER
		}

		public TargetPathType pathDestinationType;

		private TalkDoerLite m_owner;

		private Vector2 m_lastPosition;

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			m_owner = base.Owner.GetComponent<TalkDoerLite>();
			m_lastPosition = m_owner.specRigidbody.UnitCenter;
			Vector2 targetPosition = m_lastPosition;
			switch (pathDestinationType)
			{
			case TargetPathType.PLAYER:
				targetPosition = GameManager.Instance.BestActivePlayer.CenterPosition;
				break;
			case TargetPathType.PLAYER_ROOM_CENTER:
				targetPosition = GameManager.Instance.BestActivePlayer.CurrentRoom.GetCenterCell().ToCenterVector3(0f);
				break;
			}
			m_owner.PathfindToPosition(targetPosition);
		}

		public override void OnUpdate()
		{
			base.OnUpdate();
			m_owner.specRigidbody.Velocity = m_owner.GetPathVelocityContribution(m_lastPosition, 32);
			if (m_owner.CurrentPath == null)
			{
				Finish();
			}
			m_lastPosition = m_owner.specRigidbody.UnitCenter;
		}
	}
}
