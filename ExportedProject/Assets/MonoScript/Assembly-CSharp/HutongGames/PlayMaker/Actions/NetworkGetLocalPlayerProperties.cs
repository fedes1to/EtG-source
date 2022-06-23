using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Get the local network player properties")]
	public class NetworkGetLocalPlayerProperties : FsmStateAction
	{
		[Tooltip("The IP address of this player.")]
		[UIHint(UIHint.Variable)]
		public FsmString IpAddress;

		[UIHint(UIHint.Variable)]
		[Tooltip("The port of this player.")]
		public FsmInt port;

		[UIHint(UIHint.Variable)]
		[Tooltip("The GUID for this player, used when connecting with NAT punchthrough.")]
		public FsmString guid;

		[UIHint(UIHint.Variable)]
		[Tooltip("The external IP address of the network interface. This will only be populated after some external connection has been made.")]
		public FsmString externalIPAddress;

		[Tooltip("Returns the external port of the network interface. This will only be populated after some external connection has been made.")]
		[UIHint(UIHint.Variable)]
		public FsmInt externalPort;

		public override void Reset()
		{
			IpAddress = null;
			port = null;
			guid = null;
			externalIPAddress = null;
			externalPort = null;
		}

		public override void OnEnter()
		{
			IpAddress.Value = Network.player.ipAddress;
			port.Value = Network.player.port;
			guid.Value = Network.player.guid;
			externalIPAddress.Value = Network.player.externalIP;
			externalPort.Value = Network.player.externalPort;
			Finish();
		}
	}
}
