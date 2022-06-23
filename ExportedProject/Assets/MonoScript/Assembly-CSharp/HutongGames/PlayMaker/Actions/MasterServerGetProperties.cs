using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the IP address, port, update rate and dedicated server flag of the master server and store in variables.")]
	[ActionCategory(ActionCategory.Network)]
	public class MasterServerGetProperties : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("The IP address of the master server.")]
		public FsmString ipAddress;

		[UIHint(UIHint.Variable)]
		[Tooltip("The connection port of the master server.")]
		public FsmInt port;

		[Tooltip("The minimum update rate for master server host information update. Default is 60 seconds")]
		[UIHint(UIHint.Variable)]
		public FsmInt updateRate;

		[UIHint(UIHint.Variable)]
		[Tooltip("Flag to report if this machine is a dedicated server.")]
		public FsmBool dedicatedServer;

		[Tooltip("Event sent if this machine is a dedicated server")]
		public FsmEvent isDedicatedServerEvent;

		[Tooltip("Event sent if this machine is not a dedicated server")]
		public FsmEvent isNotDedicatedServerEvent;

		public override void Reset()
		{
			ipAddress = null;
			port = null;
			updateRate = null;
			dedicatedServer = null;
			isDedicatedServerEvent = null;
			isNotDedicatedServerEvent = null;
		}

		public override void OnEnter()
		{
			GetMasterServerProperties();
			Finish();
		}

		private void GetMasterServerProperties()
		{
			ipAddress.Value = MasterServer.ipAddress;
			port.Value = MasterServer.port;
			updateRate.Value = MasterServer.updateRate;
			bool flag = MasterServer.dedicatedServer;
			dedicatedServer.Value = flag;
			if (flag && isDedicatedServerEvent != null)
			{
				base.Fsm.Event(isDedicatedServerEvent);
			}
			if (!flag && isNotDedicatedServerEvent != null)
			{
				base.Fsm.Event(isNotDedicatedServerEvent);
			}
		}
	}
}
