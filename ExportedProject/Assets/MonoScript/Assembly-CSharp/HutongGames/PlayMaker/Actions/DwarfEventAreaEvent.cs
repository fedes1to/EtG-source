using System;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Responds to trigger events with Speculative Rigidbodies.")]
	[ActionCategory(".Brave")]
	public class DwarfEventAreaEvent : FsmStateAction
	{
		[CompoundArray("Events", "Trigger Index", "Send Event")]
		[Tooltip("Event to play when the corresponding trigger detects a collision.")]
		public FsmInt[] eventIndices;

		public FsmEvent[] events;

		private DwarfEventListener m_eventListener;

		public override void Reset()
		{
			eventIndices = new FsmInt[0];
			events = new FsmEvent[0];
		}

		public override void OnEnter()
		{
			m_eventListener = base.Owner.GetComponent<DwarfEventListener>();
			if ((bool)m_eventListener)
			{
				DwarfEventListener eventListener = m_eventListener;
				eventListener.OnTrigger = (Action<int>)Delegate.Combine(eventListener.OnTrigger, new Action<int>(OnTrigger));
			}
		}

		public override void OnExit()
		{
			if ((bool)m_eventListener)
			{
				DwarfEventListener eventListener = m_eventListener;
				eventListener.OnTrigger = (Action<int>)Delegate.Remove(eventListener.OnTrigger, new Action<int>(OnTrigger));
			}
		}

		private void OnTrigger(int index)
		{
			for (int i = 0; i < eventIndices.Length; i++)
			{
				if (eventIndices[i].Value == index)
				{
					base.Fsm.Event(events[i]);
				}
			}
		}
	}
}
