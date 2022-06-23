using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Returns the Animator controller layer count")]
	[ActionCategory(ActionCategory.Animator)]
	public class GetAnimatorLayerCount : FsmStateAction
	{
		[Tooltip("The Target. An Animator component is required")]
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[ActionSection("Results")]
		[Tooltip("The Animator controller layer count")]
		[UIHint(UIHint.Variable)]
		public FsmInt layerCount;

		private Animator _animator;

		public override void Reset()
		{
			gameObject = null;
			layerCount = null;
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
			DoGetLayerCount();
			Finish();
		}

		private void DoGetLayerCount()
		{
			if (!(_animator == null))
			{
				layerCount.Value = _animator.layerCount;
			}
		}
	}
}
