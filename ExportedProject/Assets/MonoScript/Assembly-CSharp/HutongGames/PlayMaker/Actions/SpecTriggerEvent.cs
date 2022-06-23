using System;
using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Responds to trigger events with Speculative Rigidbodies.")]
	public class SpecTriggerEvent : FsmStateAction
	{
		[CompoundArray("Events", "Trigger Index", "Send Event")]
		[Tooltip("Event to play when the corresponding trigger detects a collision.")]
		public FsmInt[] triggerIndices;

		public FsmEvent[] events;

		private SpeculativeRigidbody m_specRigidbody;

		private List<PixelCollider> m_triggerColliders = new List<PixelCollider>();

		public override void Reset()
		{
			triggerIndices = new FsmInt[0];
			events = new FsmEvent[0];
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			SpeculativeRigidbody component = base.Owner.GetComponent<SpeculativeRigidbody>();
			if (!component)
			{
				text += "Owner does not have a Speculative Rigidbody.\n";
			}
			else
			{
				int num = 0;
				for (int i = 0; i < component.PixelColliders.Count; i++)
				{
					if (component.PixelColliders[i].IsTrigger)
					{
						num++;
					}
				}
				for (int j = 0; j < triggerIndices.Length; j++)
				{
					if (triggerIndices[j].Value >= num)
					{
						text += string.Format("Trigger index {0} is too high for a Speculative Rigidbody with {1} triggers.\n", triggerIndices[j].Value, num);
					}
				}
			}
			return text;
		}

		public override void OnEnter()
		{
			m_specRigidbody = base.Owner.GetComponent<SpeculativeRigidbody>();
			if (!m_specRigidbody)
			{
				return;
			}
			for (int i = 0; i < m_specRigidbody.PixelColliders.Count; i++)
			{
				PixelCollider pixelCollider = m_specRigidbody.PixelColliders[i];
				if (pixelCollider.IsTrigger)
				{
					m_triggerColliders.Add(pixelCollider);
				}
			}
			SpeculativeRigidbody specRigidbody = m_specRigidbody;
			specRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(specRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnEnterTrigger));
		}

		public override void OnExit()
		{
			if ((bool)m_specRigidbody)
			{
				SpeculativeRigidbody specRigidbody = m_specRigidbody;
				specRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Remove(specRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(OnEnterTrigger));
			}
		}

		private void OnEnterTrigger(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
		{
			for (int i = 0; i < triggerIndices.Length; i++)
			{
				if (collisionData.MyPixelCollider == m_triggerColliders[triggerIndices[i].Value])
				{
					base.Fsm.Event(events[i]);
				}
			}
		}
	}
}
