using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Gets the Y Position of the mouse and stores it in a Float Variable.")]
	[ActionCategory(ActionCategory.Input)]
	public class GetMouseY : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[RequiredField]
		public FsmFloat storeResult;

		public bool normalize;

		public override void Reset()
		{
			storeResult = null;
			normalize = true;
		}

		public override void OnEnter()
		{
			DoGetMouseY();
		}

		public override void OnUpdate()
		{
			DoGetMouseY();
		}

		private void DoGetMouseY()
		{
			if (storeResult != null)
			{
				float num = Input.mousePosition.y;
				if (normalize)
				{
					num /= (float)Screen.height;
				}
				storeResult.Value = num;
			}
		}
	}
}
