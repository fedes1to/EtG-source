using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animator)]
	[Tooltip("Returns the scale of the current Avatar for a humanoid rig, (1 by default if the rig is generic).\n The scale is relative to Unity's Default Avatar")]
	public class GetAnimatorHumanScale : FsmStateAction
	{
		[Tooltip("The Target. An Animator component is required")]
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("the scale of the current Avatar")]
		[UIHint(UIHint.Variable)]
		[ActionSection("Result")]
		public FsmFloat humanScale;

		private Animator _animator;

		public override void Reset()
		{
			gameObject = null;
			humanScale = null;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (ownerDefaultTarget == null)
			{
				Finish();
				return;
			}
			_animator = ownerDefaultTarget.GetComponent<Animator>();
			if (_animator == null)
			{
				Finish();
				return;
			}
			DoGetHumanScale();
			Finish();
		}

		private void DoGetHumanScale()
		{
			if (!(_animator == null))
			{
				humanScale.Value = _animator.humanScale;
			}
		}
	}
}
