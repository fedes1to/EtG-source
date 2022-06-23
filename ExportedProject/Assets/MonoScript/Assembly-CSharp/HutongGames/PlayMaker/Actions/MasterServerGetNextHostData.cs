using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the next host data from the master server. \nEach time this action is called it gets the next connected host.This lets you quickly loop through all the connected hosts to get information on each one.")]
	[ActionCategory(ActionCategory.Network)]
	public class MasterServerGetNextHostData : FsmStateAction
	{
		[Tooltip("Event to send for looping.")]
		[ActionSection("Set up")]
		public FsmEvent loopEvent;

		[Tooltip("Event to send when there are no more hosts.")]
		public FsmEvent finishedEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("The index into the MasterServer Host List")]
		[ActionSection("Result")]
		public FsmInt index;

		[Tooltip("Does this server require NAT punchthrough?")]
		[UIHint(UIHint.Variable)]
		public FsmBool useNat;

		[UIHint(UIHint.Variable)]
		[Tooltip("The type of the game (e.g., 'MyUniqueGameType')")]
		public FsmString gameType;

		[Tooltip("The name of the game (e.g., 'John Does's Game')")]
		[UIHint(UIHint.Variable)]
		public FsmString gameName;

		[UIHint(UIHint.Variable)]
		[Tooltip("Currently connected players")]
		public FsmInt connectedPlayers;

		[UIHint(UIHint.Variable)]
		[Tooltip("Maximum players limit")]
		public FsmInt playerLimit;

		[UIHint(UIHint.Variable)]
		[Tooltip("Server IP address.")]
		public FsmString ipAddress;

		[UIHint(UIHint.Variable)]
		[Tooltip("Server port")]
		public FsmInt port;

		[UIHint(UIHint.Variable)]
		[Tooltip("Does the server require a password?")]
		public FsmBool passwordProtected;

		[UIHint(UIHint.Variable)]
		[Tooltip("A miscellaneous comment (can hold data)")]
		public FsmString comment;

		[UIHint(UIHint.Variable)]
		[Tooltip("The GUID of the host, needed when connecting with NAT punchthrough.")]
		public FsmString guid;

		private int nextItemIndex;

		private bool noMoreItems;

		public override void Reset()
		{
			finishedEvent = null;
			loopEvent = null;
			index = null;
			useNat = null;
			gameType = null;
			gameName = null;
			connectedPlayers = null;
			playerLimit = null;
			ipAddress = null;
			port = null;
			passwordProtected = null;
			comment = null;
			guid = null;
		}

		public override void OnEnter()
		{
			DoGetNextHostData();
			Finish();
		}

		private void DoGetNextHostData()
		{
			if (nextItemIndex >= MasterServer.PollHostList().Length)
			{
				nextItemIndex = 0;
				base.Fsm.Event(finishedEvent);
				return;
			}
			HostData hostData = MasterServer.PollHostList()[nextItemIndex];
			index.Value = nextItemIndex;
			useNat.Value = hostData.useNat;
			gameType.Value = hostData.gameType;
			gameName.Value = hostData.gameName;
			connectedPlayers.Value = hostData.connectedPlayers;
			playerLimit.Value = hostData.playerLimit;
			ipAddress.Value = hostData.ip[0];
			port.Value = hostData.port;
			passwordProtected.Value = hostData.passwordProtected;
			comment.Value = hostData.comment;
			guid.Value = hostData.guid;
			if (nextItemIndex >= MasterServer.PollHostList().Length)
			{
				base.Fsm.Event(finishedEvent);
				nextItemIndex = 0;
				return;
			}
			nextItemIndex++;
			if (loopEvent != null)
			{
				base.Fsm.Event(loopEvent);
			}
		}
	}
}
