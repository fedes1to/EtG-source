using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animator)]
	[Tooltip("Gets the current State information on a specified layer")]
	public class GetAnimatorCurrentStateInfo : FsmStateActionAnimatorBase
	{
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		[Tooltip("The target.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The layer's index")]
		[RequiredField]
		public FsmInt layerIndex;

		[Tooltip("The layer's name.")]
		[UIHint(UIHint.Variable)]
		[ActionSection("Results")]
		public FsmString name;

		[Tooltip("The layer's name Hash. Obsolete in Unity 5, use fullPathHash or shortPathHash instead, nameHash will be the same as shortNameHash for legacy")]
		[UIHint(UIHint.Variable)]
		public FsmInt nameHash;

		[Tooltip("The layer's tag hash")]
		[UIHint(UIHint.Variable)]
		public FsmInt tagHash;

		[Tooltip("Is the state looping. All animations in the state must be looping")]
		[UIHint(UIHint.Variable)]
		public FsmBool isStateLooping;

		[Tooltip("The Current duration of the state. In seconds, can vary when the State contains a Blend Tree ")]
		[UIHint(UIHint.Variable)]
		public FsmFloat length;

		[Tooltip("The integer part is the number of time a state has been looped. The fractional part is the % (0-1) of progress in the current loop")]
		[UIHint(UIHint.Variable)]
		public FsmFloat normalizedTime;

		[Tooltip("The integer part is the number of time a state has been looped. This is extracted from the normalizedTime")]
		[UIHint(UIHint.Variable)]
		public FsmInt loopCount;

		[Tooltip("The progress in the current loop. This is extracted from the normalizedTime")]
		[UIHint(UIHint.Variable)]
		public FsmFloat currentLoopProgress;

		private Animator _animator;

		public override void Reset()
		{
			base.Reset();
			gameObject = null;
			layerIndex = null;
			name = null;
			nameHash = null;
			tagHash = null;
			length = null;
			normalizedTime = null;
			isStateLooping = null;
			loopCount = null;
			currentLoopProgress = null;
			everyFrame = false;
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
			GetLayerInfo();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnActionUpdate()
		{
			GetLayerInfo();
		}

		private void GetLayerInfo()
		{
			if (_animator != null)
			{
				AnimatorStateInfo currentAnimatorStateInfo = _animator.GetCurrentAnimatorStateInfo(layerIndex.Value);
				if (!nameHash.IsNone)
				{
					nameHash.Value = currentAnimatorStateInfo.nameHash;
				}
				if (!name.IsNone)
				{
					name.Value = _animator.GetLayerName(layerIndex.Value);
				}
				if (!tagHash.IsNone)
				{
					tagHash.Value = currentAnimatorStateInfo.tagHash;
				}
				if (!length.IsNone)
				{
					length.Value = currentAnimatorStateInfo.length;
				}
				if (!isStateLooping.IsNone)
				{
					isStateLooping.Value = currentAnimatorStateInfo.loop;
				}
				if (!normalizedTime.IsNone)
				{
					normalizedTime.Value = currentAnimatorStateInfo.normalizedTime;
				}
				if (!loopCount.IsNone || !currentLoopProgress.IsNone)
				{
					loopCount.Value = (int)Math.Truncate(currentAnimatorStateInfo.normalizedTime);
					currentLoopProgress.Value = currentAnimatorStateInfo.normalizedTime - (float)loopCount.Value;
				}
			}
		}
	}
}
