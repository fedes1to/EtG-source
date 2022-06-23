using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Enable or disable the processing of network messages.\n\nIf this is disabled no RPC call execution or network view synchronization takes place.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkSetIsMessageQueueRunning : FsmStateAction
	{
		[Tooltip("Is Message Queue Running. If this is disabled no RPC call execution or network view synchronization takes place")]
		public FsmBool isMessageQueueRunning;

		public override void Reset()
		{
			isMessageQueueRunning = null;
		}

		public override void OnEnter()
		{
			Network.isMessageQueueRunning = isMessageQueueRunning.Value;
			Finish();
		}
	}
}
