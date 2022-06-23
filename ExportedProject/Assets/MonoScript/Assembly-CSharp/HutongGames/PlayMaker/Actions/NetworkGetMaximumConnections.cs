using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the maximum amount of connections/players allowed.")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetMaximumConnections : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("Get the maximum amount of connections/players allowed.")]
		public FsmInt result;

		public override void Reset()
		{
			result = null;
		}

		public override void OnEnter()
		{
			result.Value = Network.maxConnections;
			Finish();
		}
	}
}
