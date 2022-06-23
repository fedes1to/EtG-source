using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Device)]
	[Tooltip("Sends an Event when the mobile device is shaken.")]
	public class DeviceShakeEvent : FsmStateAction
	{
		[Tooltip("Amount of acceleration required to trigger the event. Higher numbers require a harder shake.")]
		[RequiredField]
		public FsmFloat shakeThreshold;

		[Tooltip("Event to send when Shake Threshold is exceded.")]
		[RequiredField]
		public FsmEvent sendEvent;

		public override void Reset()
		{
			shakeThreshold = 3f;
			sendEvent = null;
		}

		public override void OnUpdate()
		{
			if (Input.acceleration.sqrMagnitude > shakeThreshold.Value * shakeThreshold.Value)
			{
				base.Fsm.Event(sendEvent);
			}
		}
	}
}
