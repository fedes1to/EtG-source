using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Start a server.")]
	[ActionCategory(ActionCategory.Network)]
	public class StartServer : FsmStateAction
	{
		[Tooltip("The number of allowed incoming connections/number of players allowed in the game.")]
		[RequiredField]
		public FsmInt connections;

		[Tooltip("The port number we want to listen to.")]
		[RequiredField]
		public FsmInt listenPort;

		[Tooltip("Sets the password for the server. This must be matched in the NetworkConnect action.")]
		public FsmString incomingPassword;

		[Tooltip("Sets the NAT punchthrough functionality.")]
		public FsmBool useNAT;

		[Tooltip("Unity handles the network layer by providing secure connections if you wish to use them. \nMost games will want to use secure connections. However, they add up to 15 bytes per packet and take time to compute so you may wish to limit usage to deployed games only.")]
		public FsmBool useSecurityLayer;

		[Tooltip("Run the server in the background, even if it doesn't have focus.")]
		public FsmBool runInBackground;

		[ActionSection("Errors")]
		[Tooltip("Event to send in case of an error creating the server.")]
		public FsmEvent errorEvent;

		[Tooltip("Store the error string in a variable.")]
		[UIHint(UIHint.Variable)]
		public FsmString errorString;

		public override void Reset()
		{
			connections = 32;
			listenPort = 25001;
			incomingPassword = string.Empty;
			errorEvent = null;
			errorString = null;
			useNAT = false;
			useSecurityLayer = false;
			runInBackground = true;
		}

		public override void OnEnter()
		{
			Network.incomingPassword = incomingPassword.Value;
			if (useSecurityLayer.Value)
			{
				Network.InitializeSecurity();
			}
			if (runInBackground.Value)
			{
				Application.runInBackground = true;
			}
			NetworkConnectionError networkConnectionError = Network.InitializeServer(connections.Value, listenPort.Value, useNAT.Value);
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
