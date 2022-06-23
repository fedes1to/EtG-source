namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Normalizes a Vector2 Variable.")]
	[ActionCategory(ActionCategory.Vector2)]
	public class Vector2Normalize : FsmStateAction
	{
		[Tooltip("The vector to normalize")]
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmVector2 vector2Variable;

		[Tooltip("Repeat every frame")]
		public bool everyFrame;

		public override void Reset()
		{
			vector2Variable = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			vector2Variable.Value = vector2Variable.Value.normalized;
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			vector2Variable.Value = vector2Variable.Value.normalized;
		}
	}
}
