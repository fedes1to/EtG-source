using System;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Check if an Array contains a value. Optionally get its index.")]
	[ActionCategory(ActionCategory.Array)]
	public class ArrayContains : FsmStateAction
	{
		[Tooltip("The Array Variable to use.")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmArray array;

		[Tooltip("The value to check against in the array.")]
		[RequiredField]
		[MatchElementType("array")]
		public FsmVar value;

		[UIHint(UIHint.Variable)]
		[Tooltip("The index of the value in the array.")]
		[ActionSection("Result")]
		public FsmInt index;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store in a bool whether it contains that element or not (described below)")]
		public FsmBool isContained;

		[Tooltip("Event sent if the array contains that element (described below)")]
		public FsmEvent isContainedEvent;

		[Tooltip("Event sent if the array does not contains that element (described below)")]
		public FsmEvent isNotContainedEvent;

		public override void Reset()
		{
			array = null;
			value = null;
			index = null;
			isContained = null;
			isContainedEvent = null;
			isNotContainedEvent = null;
		}

		public override void OnEnter()
		{
			DoCheckContainsValue();
			Finish();
		}

		private void DoCheckContainsValue()
		{
			value.UpdateValue();
			int num = Array.IndexOf(array.Values, value.GetValue());
			bool flag = num != -1;
			isContained.Value = flag;
			index.Value = num;
			if (flag)
			{
				base.Fsm.Event(isContainedEvent);
			}
			else
			{
				base.Fsm.Event(isNotContainedEvent);
			}
		}
	}
}
