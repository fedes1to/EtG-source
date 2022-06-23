namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sets the value of a String Variable.")]
	[ActionCategory(ActionCategory.String)]
	public class SetStringValue : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString stringVariable;

		[UIHint(UIHint.TextArea)]
		public FsmString stringValue;

		public bool everyFrame;

		public override void Reset()
		{
			stringVariable = null;
			stringValue = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoSetStringValue();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoSetStringValue();
		}

		private void DoSetStringValue()
		{
			if (stringVariable != null && stringValue != null)
			{
				stringVariable.Value = stringValue.Value;
			}
		}
	}
}
