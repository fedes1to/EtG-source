namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Add an item to the end of an Array.")]
	[ActionCategory(ActionCategory.Array)]
	public class ArrayAdd : FsmStateAction
	{
		[Tooltip("The Array Variable to use.")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		public FsmArray array;

		[Tooltip("Item to add.")]
		[RequiredField]
		[MatchElementType("array")]
		public FsmVar value;

		public override void Reset()
		{
			array = null;
			value = null;
		}

		public override void OnEnter()
		{
			DoAddValue();
			Finish();
		}

		private void DoAddValue()
		{
			array.Resize(array.Length + 1);
			value.UpdateValue();
			array.Set(array.Length - 1, value.GetValue());
		}
	}
}
