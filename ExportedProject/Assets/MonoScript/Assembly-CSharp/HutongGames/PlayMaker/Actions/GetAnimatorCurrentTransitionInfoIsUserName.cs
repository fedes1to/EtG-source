using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Check the active Transition user-specified name on a specified layer.")]
	[ActionCategory(ActionCategory.Animator)]
	public class GetAnimatorCurrentTransitionInfoIsUserName : FsmStateActionAnimatorBase
	{
		[Tooltip("The target. An Animator component is required")]
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("The layer's index")]
		[RequiredField]
		public FsmInt layerIndex;

		[Tooltip("The user-specified name to check the transition against.")]
		public FsmString userName;

		[Tooltip("True if name matches")]
		[UIHint(UIHint.Variable)]
		[ActionSection("Results")]
		public FsmBool nameMatch;

		[Tooltip("Event send if name matches")]
		public FsmEvent nameMatchEvent;

		[Tooltip("Event send if name doesn't match")]
		public FsmEvent nameDoNotMatchEvent;

		private Animator _animator;

		public override void Reset()
		{
			base.Reset();
			gameObject = null;
			layerIndex = null;
			userName = null;
			nameMatch = null;
			nameMatchEvent = null;
			nameDoNotMatchEvent = null;
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
			IsName();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnActionUpdate()
		{
			IsName();
		}

		private void IsName()
		{
			if (_animator != null)
			{
				bool flag = _animator.GetAnimatorTransitionInfo(layerIndex.Value).IsUserName(userName.Value);
				if (!nameMatch.IsNone)
				{
					nameMatch.Value = flag;
				}
				if (flag)
				{
					base.Fsm.Event(nameMatchEvent);
				}
				else
				{
					base.Fsm.Event(nameDoNotMatchEvent);
				}
			}
		}
	}
}
