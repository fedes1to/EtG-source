using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Checks if the owning game object has been encountered before.")]
	[ActionCategory(".Brave")]
	public class HasBeenEncountered : FsmStateAction
	{
		[CheckForComponent(typeof(EncounterTrackable))]
		public FsmOwnerDefault GameObject;

		[Tooltip("Event to send when the mouse is released while over the GameObject.")]
		public FsmEvent yes;

		[Tooltip("Event to send when the mouse moves off the GameObject.")]
		public FsmEvent no;

		private EncounterTrackable m_encounterTrackable;

		public override void Reset()
		{
			GameObject = null;
			yes = null;
			no = null;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(GameObject);
			m_encounterTrackable = ownerDefaultTarget.GetComponent<EncounterTrackable>();
			if (m_encounterTrackable != null)
			{
				if (GameStatsManager.Instance.QueryEncounterable(m_encounterTrackable) > 0)
				{
					if (yes != null)
					{
						base.Fsm.Event(yes);
					}
				}
				else if (no != null)
				{
					base.Fsm.Event(no);
				}
			}
			Finish();
		}
	}
}
