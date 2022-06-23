namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Convert)]
	[Tooltip("Converts an Enum value to a String value.")]
	public class ConvertEnumToString : FsmStateAction
	{
		[Tooltip("The Enum variable to convert.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmEnum enumVariable;

		[Tooltip("The String variable to store the converted value.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmString stringVariable;

		[Tooltip("Repeat every frame. Useful if the Enum variable is changing.")]
		public bool everyFrame;

		public override void Reset()
		{
			enumVariable = null;
			stringVariable = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoConvertEnumToString();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoConvertEnumToString();
		}

		private void DoConvertEnumToString()
		{
			stringVariable.Value = ((enumVariable.Value == null) ? string.Empty : enumVariable.Value.ToString());
		}
	}
}
