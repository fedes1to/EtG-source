using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Performs math operations on 2 Floats: Add, Subtract, Multiply, Divide, Min, Max.")]
	[ActionCategory(ActionCategory.Math)]
	public class FloatOperator : FsmStateAction
	{
		public enum Operation
		{
			Add,
			Subtract,
			Multiply,
			Divide,
			Min,
			Max
		}

		[Tooltip("The first float.")]
		[RequiredField]
		public FsmFloat float1;

		[Tooltip("The second float.")]
		[RequiredField]
		public FsmFloat float2;

		[Tooltip("The math operation to perform on the floats.")]
		public Operation operation;

		[Tooltip("Store the result of the operation in a float variable.")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmFloat storeResult;

		[Tooltip("Repeat every frame. Useful if the variables are changing.")]
		public bool everyFrame;

		public override void Reset()
		{
			float1 = null;
			float2 = null;
			operation = Operation.Add;
			storeResult = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoFloatOperator();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoFloatOperator();
		}

		private void DoFloatOperator()
		{
			float value = float1.Value;
			float value2 = float2.Value;
			switch (operation)
			{
			case Operation.Add:
				storeResult.Value = value + value2;
				break;
			case Operation.Subtract:
				storeResult.Value = value - value2;
				break;
			case Operation.Multiply:
				storeResult.Value = value * value2;
				break;
			case Operation.Divide:
				storeResult.Value = value / value2;
				break;
			case Operation.Min:
				storeResult.Value = Mathf.Min(value, value2);
				break;
			case Operation.Max:
				storeResult.Value = Mathf.Max(value, value2);
				break;
			}
		}
	}
}
