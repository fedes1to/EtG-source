using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Responds to chest events.")]
	[ActionCategory(".NPCs")]
	public class ChestEvent : FsmStateAction
	{
		[Tooltip("Event to play when the chest has been unlocked.")]
		public FsmEvent unlocked;

		[Tooltip("Event to play when the chest has been locked.")]
		public FsmEvent locked;

		[Tooltip("Event to play when the chest has been unsealed.")]
		public FsmEvent unsealed;

		[Tooltip("Event to play when the chest has been sealed.")]
		public FsmEvent Sealed;

		[Tooltip("Event to play when the chest has been opened.")]
		public FsmEvent opened;

		[Tooltip("Event to play when the chest has been destroyed.")]
		public FsmEvent destroyed;

		private Chest m_chest;

		private bool m_wasLocked;

		private bool m_wasSealed;

		private bool m_wasOpen;

		private bool m_wasDestroyed;

		public override void Reset()
		{
			unlocked = null;
			locked = null;
			unsealed = null;
			Sealed = null;
			opened = null;
			destroyed = null;
		}

		public override void OnEnter()
		{
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.Owner.transform.position.IntXY(VectorConversions.Floor));
			List<Chest> componentsInRoom = roomFromPosition.GetComponentsInRoom<Chest>();
			if (componentsInRoom != null && componentsInRoom.Count > 0)
			{
				m_chest = componentsInRoom[0];
				if (componentsInRoom.Count > 1)
				{
					Debug.LogError("Too many chests!");
				}
				m_wasLocked = m_chest.IsLocked;
				m_wasSealed = m_chest.IsSealed;
				m_wasOpen = m_chest.IsOpen;
				m_wasDestroyed = m_chest.IsBroken;
			}
			else
			{
				Debug.LogError("No chests found!");
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (!m_chest)
			{
				Finish();
				return;
			}
			if (((unlocked != null) & m_wasLocked) && !m_chest.IsLocked)
			{
				base.Fsm.Event(unlocked);
			}
			if (((locked != null) & !m_wasLocked) && m_chest.IsLocked)
			{
				base.Fsm.Event(locked);
			}
			if (((unsealed != null) & m_wasSealed) && !m_chest.IsSealed)
			{
				base.Fsm.Event(unsealed);
			}
			if (((Sealed != null) & !m_wasSealed) && m_chest.IsSealed)
			{
				base.Fsm.Event(Sealed);
			}
			if (((opened != null) & !m_wasOpen) && m_chest.IsOpen)
			{
				base.Fsm.Event(opened);
			}
			if (((destroyed != null) & !m_wasDestroyed) && m_chest.IsBroken)
			{
				base.Fsm.Event(destroyed);
			}
			m_wasLocked = m_chest.IsLocked;
			m_wasSealed = m_chest.IsSealed;
			m_wasOpen = m_chest.IsOpen;
			m_wasDestroyed = m_chest.IsBroken;
		}
	}
}
