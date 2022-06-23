using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Network)]
	[Tooltip("Close the connection to another system.\n\nConnection index defines which system to close the connection to (from the Network connections array).\nCan define connection to close via Guid if index is unknown. \nIf we are a client the only possible connection to close is the server connection, if we are a server the target player will be kicked off. \n\nSend Disconnection Notification enables or disables notifications being sent to the other end. If disabled the connection is dropped, if not a disconnect notification is reliably sent to the remote party and there after the connection is dropped.")]
	public class NetworkCloseConnection : FsmStateAction
	{
		[Tooltip("Connection index to close")]
		[UIHint(UIHint.Variable)]
		public FsmInt connectionIndex;

		[UIHint(UIHint.Variable)]
		[Tooltip("Connection GUID to close. Used If Index is not set.")]
		public FsmString connectionGUID;

		[Tooltip("If True, send Disconnection Notification")]
		public bool sendDisconnectionNotification;

		public override void Reset()
		{
			connectionIndex = 0;
			connectionGUID = null;
			sendDisconnectionNotification = true;
		}

		public override void OnEnter()
		{
			int num = 0;
			int guidIndex;
			if (!connectionIndex.IsNone)
			{
				num = connectionIndex.Value;
			}
			else if (!connectionGUID.IsNone && getIndexFromGUID(connectionGUID.Value, out guidIndex))
			{
				num = guidIndex;
			}
			if (num < 0 || num > Network.connections.Length)
			{
				LogError("Connection index out of range: " + num);
			}
			else
			{
				Network.CloseConnection(Network.connections[num], sendDisconnectionNotification);
			}
			Finish();
		}

		private bool getIndexFromGUID(string guid, out int guidIndex)
		{
			for (int i = 0; i < Network.connections.Length; i++)
			{
				if (guid.Equals(Network.connections[i].guid))
				{
					guidIndex = i;
					return true;
				}
			}
			guidIndex = 0;
			return false;
		}
	}
}
