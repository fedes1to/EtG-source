using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the current network time (seconds).")]
	[ActionCategory(ActionCategory.Network)]
	public class NetworkGetTime : FsmStateAction
	{
		[Tooltip("The network time.")]
		[UIHint(UIHint.Variable)]
		public FsmFloat time;

		public override void Reset()
		{
			time = null;
		}

		public override void OnEnter()
		{
			time.Value = (float)Network.time;
			Finish();
		}
	}
}
