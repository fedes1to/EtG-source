namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Performs boolean operations on 2 Bool Variables.")]
	[ActionCategory(ActionCategory.Math)]
	public class BoolOperator : FsmStateAction
	{
		public enum Operation
		{
			AND,
			NAND,
			OR,
			XOR
		}

		[Tooltip("The first Bool variable.")]
		[RequiredField]
		public FsmBool bool1;

		[Tooltip("The second Bool variable.")]
		[RequiredField]
		public FsmBool bool2;

		[Tooltip("Boolean Operation.")]
		public Operation operation;

		[Tooltip("Store the result in a Bool Variable.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmBool storeResult;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

		public override void Reset()
		{
			bool1 = false;
			bool2 = false;
			operation = Operation.AND;
			storeResult = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoBoolOperator();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoBoolOperator();
		}

		private void DoBoolOperator()
		{
			bool value = bool1.Value;
			bool value2 = bool2.Value;
			switch (operation)
			{
			case Operation.AND:
				storeResult.Value = value && value2;
				break;
			case Operation.NAND:
				storeResult.Value = !value || !value2;
				break;
			case Operation.OR:
				storeResult.Value = value || value2;
				break;
			case Operation.XOR:
				storeResult.Value = value ^ value2;
				break;
			}
		}
	}
}
