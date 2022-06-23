using System.Collections.Generic;
using Dungeonator;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Unlocks all truth chests in the current room")]
	[ActionCategory(".NPCs")]
	public class OpenTruthChest : FsmStateAction
	{
		[Tooltip("Seconds to wait before opening the chest.")]
		public FsmFloat delay;

		[Tooltip("If true, the chest will open if this action ends early.")]
		public FsmBool openOnEarlyFinish;

		private float m_vanishTimer;

		private bool m_opened;

		public override void Reset()
		{
			delay = 0f;
			openOnEarlyFinish = true;
		}

		public override void OnEnter()
		{
			m_opened = false;
			if (delay.Value <= 0f)
			{
				OpenChest();
				Finish();
			}
			else
			{
				m_vanishTimer = delay.Value;
			}
		}

		public override void OnUpdate()
		{
			m_vanishTimer -= BraveTime.DeltaTime;
			if (m_vanishTimer <= 0f)
			{
				OpenChest();
				Finish();
			}
		}

		public override void OnExit()
		{
			if (openOnEarlyFinish.Value && !m_opened)
			{
				OpenChest();
			}
		}

		private void OpenChest()
		{
			if (m_opened)
			{
				return;
			}
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(base.Owner.transform.position.IntXY(VectorConversions.Floor));
			List<Chest> componentsInRoom = roomFromPosition.GetComponentsInRoom<Chest>();
			for (int i = 0; i < componentsInRoom.Count; i++)
			{
				if (componentsInRoom[i].name.ToLowerInvariant().Contains("truth"))
				{
					componentsInRoom[i].IsLocked = false;
					componentsInRoom[i].IsSealed = false;
					tk2dSpriteAnimator componentInChildren = componentsInRoom[i].transform.Find("lock").GetComponentInChildren<tk2dSpriteAnimator>();
					if (componentInChildren != null)
					{
						componentInChildren.StopAndResetFrame();
						componentInChildren.PlayAndDestroyObject("truth_lock_open");
					}
				}
			}
			m_opened = true;
		}
	}
}
