using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get connected player properties.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetConnectedPlayerProperties : FsmStateAction
	{
		[Tooltip("The player connection index.")]
		[RequiredField]
		public FsmInt index;

		[UIHint(UIHint.Variable)]
		[Tooltip("Get the IP address of this player.")]
		[ActionSection("Result")]
		public FsmString IpAddress;

		[UIHint(UIHint.Variable)]
		[Tooltip("Get the port of this player.")]
		public FsmInt port;

		[UIHint(UIHint.Variable)]
		[Tooltip("Get the GUID for this player, used when connecting with NAT punchthrough.")]
		public FsmString guid;

		[UIHint(UIHint.Variable)]
		[Tooltip("Get the external IP address of the network interface. This will only be populated after some external connection has been made.")]
		public FsmString externalIPAddress;

		[Tooltip("Get the external port of the network interface. This will only be populated after some external connection has been made.")]
		[UIHint(UIHint.Variable)]
		public FsmInt externalPort;

		public override void Reset()
		{
			index = null;
			IpAddress = null;
			port = null;
			guid = null;
			externalIPAddress = null;
			externalPort = null;
		}

		public override void OnEnter()
		{
			getPlayerProperties();
			Finish();
		}

		private void getPlayerProperties()
		{
			int value = index.Value;
			if (value < 0 || value >= Network.connections.Length)
			{
				LogError("Player index out of range");
				return;
			}
			NetworkPlayer networkPlayer = Network.connections[value];
			IpAddress.Value = networkPlayer.ipAddress;
			port.Value = networkPlayer.port;
			guid.Value = networkPlayer.guid;
			externalIPAddress.Value = networkPlayer.externalIP;
			externalPort.Value = networkPlayer.externalPort;
		}
	}
}
