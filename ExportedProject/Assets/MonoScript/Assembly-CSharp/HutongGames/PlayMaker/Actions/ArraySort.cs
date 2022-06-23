using System.Collections.Generic;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sort items in an Array.")]
	[ActionCategory(ActionCategory.Array)]
	public class ArraySort : FsmStateAction
	{
		[Tooltip("The Array to sort.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmArray array;

		public override void Reset()
		{
			array = null;
		}

		public override void OnEnter()
		{
			List<object> list = new List<object>(array.Values);
			list.Sort();
			array.Values = list.ToArray();
			Finish();
		}
	}
}
