using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the number of hosts on the master server.\n\nUse MasterServer Get Host Data to get host data at a specific index.")]
	[ActionCategory(ActionCategory.Network)]
	public class MasterServerGetHostCount : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("The number of hosts on the MasterServer.")]
		[RequiredField]
		public FsmInt count;

		public override void OnEnter()
		{
			count.Value = MasterServer.PollHostList().Length;
			Finish();
		}
	}
}
