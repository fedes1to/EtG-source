using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Input)]
	[Tooltip("Gets the value of the specified Input Axis and stores it in a Float Variable. See Unity Input Manager docs.")]
	public class GetAxis : FsmStateAction
	{
		[Tooltip("The name of the axis. Set in the Unity Input Manager.")]
		[RequiredField]
		public FsmString axisName;

		[Tooltip("Axis values are in the range -1 to 1. Use the multiplier to set a larger range.")]
		public FsmFloat multiplier;

		[Tooltip("Store the result in a float variable.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmFloat store;

		[Tooltip("Repeat every frame. Typically this would be set to True.")]
		public bool everyFrame;

		public override void Reset()
		{
			axisName = string.Empty;
			multiplier = 1f;
			store = null;
			everyFrame = true;
		}

		public override void OnEnter()
		{
			DoGetAxis();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetAxis();
		}

		private void DoGetAxis()
		{
			if (!FsmString.IsNullOrEmpty(axisName))
			{
				float num = Input.GetAxis(axisName.Value);
				if (!multiplier.IsNone)
				{
					num *= multiplier.Value;
				}
				store.Value = num;
			}
		}
	}
}
