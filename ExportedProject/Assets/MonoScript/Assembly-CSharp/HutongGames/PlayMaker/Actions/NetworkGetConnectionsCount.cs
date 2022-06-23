using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the number of connected players.\n\nOn a client this returns 1 (the server).")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetConnectionsCount : FsmStateAction
	{
		[Tooltip("Number of connected players.")]
		[UIHint(UIHint.Variable)]
		public FsmInt connectionsCount;

		[Tooltip("Repeat every frame.")]
		public bool everyFrame;

		public override void Reset()
		{
			connectionsCount = null;
			everyFrame = true;
		}

		public override void OnEnter()
		{
			connectionsCount.Value = Network.connections.Length;
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			connectionsCount.Value = Network.connections.Length;
		}
	}
}
