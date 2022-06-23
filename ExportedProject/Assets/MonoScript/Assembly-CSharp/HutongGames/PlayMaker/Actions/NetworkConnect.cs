using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Connect to a server.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkConnect : FsmStateAction
	{
		[Tooltip("IP address of the host. Either a dotted IP address or a domain name.")]
		[RequiredField]
		public FsmString remoteIP;

		[RequiredField]
		[Tooltip("The port on the remote machine to connect to.")]
		public FsmInt remotePort;

		[Tooltip("Optional password for the server.")]
		public FsmString password;

		[Tooltip("Event to send in case of an error connecting to the server.")]
		[ActionSection("Errors")]
		public FsmEvent errorEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the error string in a variable.")]
		public FsmString errorString;

		public override void Reset()
		{
			remoteIP = "127.0.0.1";
			remotePort = 25001;
			password = string.Empty;
			errorEvent = null;
			errorString = null;
		}

		public override void OnEnter()
		{
			NetworkConnectionError networkConnectionError = Network.Connect(remoteIP.Value, remotePort.Value, password.Value);
			if (networkConnectionError != 0)
			{
				errorString.Value = networkConnectionError.ToString();
				LogError(errorString.Value);
				base.Fsm.Event(errorEvent);
			}
			Finish();
		}
	}
}
