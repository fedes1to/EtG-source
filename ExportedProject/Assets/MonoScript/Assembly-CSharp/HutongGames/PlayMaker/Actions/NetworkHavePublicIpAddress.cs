using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Check if this machine has a public IP address.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkHavePublicIpAddress : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("True if this machine has a public IP address")]
		public FsmBool havePublicIpAddress;

		[Tooltip("Event to send if this machine has a public IP address")]
		public FsmEvent publicIpAddressFoundEvent;

		[Tooltip("Event to send if this machine has no public IP address")]
		public FsmEvent publicIpAddressNotFoundEvent;

		public override void Reset()
		{
			havePublicIpAddress = null;
			publicIpAddressFoundEvent = null;
			publicIpAddressNotFoundEvent = null;
		}

		public override void OnEnter()
		{
			bool flag = Network.HavePublicAddress();
			havePublicIpAddress.Value = flag;
			if (flag && publicIpAddressFoundEvent != null)
			{
				base.Fsm.Event(publicIpAddressFoundEvent);
			}
			else if (!flag && publicIpAddressNotFoundEvent != null)
			{
				base.Fsm.Event(publicIpAddressNotFoundEvent);
			}
			Finish();
		}
	}
}
