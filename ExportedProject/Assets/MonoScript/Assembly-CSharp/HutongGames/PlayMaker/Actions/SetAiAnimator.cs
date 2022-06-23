using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Handles updating an AIAnimator.")]
	public class SetAiAnimator : FsmStateAction
	{
		public enum Mode
		{
			SetBaseAnim
		}

		public FsmOwnerDefault GameObject;

		public Mode mode;

		[Tooltip("Name of the new default animation state (Directional Animations only).  Leave blank to return to the default (idle/base).")]
		public FsmString baseAnimName;

		public override void Reset()
		{
			GameObject = null;
			mode = Mode.SetBaseAnim;
			baseAnimName = string.Empty;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			GameObject gameObject = ((GameObject.OwnerOption != 0) ? GameObject.GameObject.Value : base.Owner);
			if ((bool)gameObject)
			{
				AIAnimator component = gameObject.GetComponent<AIAnimator>();
				if (!component)
				{
					return "Requires an AI Animator.\n";
				}
				if (mode == Mode.SetBaseAnim && baseAnimName.Value != string.Empty && !component.HasDirectionalAnimation(baseAnimName.Value))
				{
					text = text + "Unknown animation " + baseAnimName.Value + ".\n";
				}
			}
			else if (!GameObject.GameObject.UseVariable)
			{
				return "No object specified";
			}
			return text;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(GameObject);
			AIAnimator component = ownerDefaultTarget.GetComponent<AIAnimator>();
			if (mode == Mode.SetBaseAnim)
			{
				if (baseAnimName.Value == string.Empty)
				{
					component.ClearBaseAnim();
				}
				else
				{
					component.SetBaseAnim(baseAnimName.Value);
				}
			}
			Finish();
		}
	}
}
