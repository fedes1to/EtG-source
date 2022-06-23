using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Gets the layer's current weight")]
	[ActionCategory(ActionCategory.Animator)]
	public class GetAnimatorLayerWeight : FsmStateActionAnimatorBase
	{
		[Tooltip("The target.")]
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("The layer's index")]
		[RequiredField]
		public FsmInt layerIndex;

		[UIHint(UIHint.Variable)]
		[RequiredField]
		[ActionSection("Results")]
		[Tooltip("The layer's current weight")]
		public FsmFloat layerWeight;

		private Animator _animator;

		public override void Reset()
		{
			base.Reset();
			gameObject = null;
			layerIndex = null;
			layerWeight = null;
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
			GetLayerWeight();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnActionUpdate()
		{
			GetLayerWeight();
		}

		private void GetLayerWeight()
		{
			if (_animator != null)
			{
				layerWeight.Value = _animator.GetLayerWeight(layerIndex.Value);
			}
		}
	}
}
