using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animation)]
	[Tooltip("Sets the Speed of an Animation. Check Every Frame to update the animation time continuosly, e.g., if you're manipulating a variable that controls animation speed.")]
	public class SetAnimationSpeed : BaseAnimationAction
	{
		[CheckForComponent(typeof(Animation))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Animation)]
		[RequiredField]
		public FsmString animName;

		public FsmFloat speed = 1f;

		public bool everyFrame;

		public override void Reset()
		{
			gameObject = null;
			animName = null;
			speed = 1f;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoSetAnimationSpeed((gameObject.OwnerOption != 0) ? gameObject.GameObject.Value : base.Owner);
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoSetAnimationSpeed((gameObject.OwnerOption != 0) ? gameObject.GameObject.Value : base.Owner);
		}

		private void DoSetAnimationSpeed(GameObject go)
		{
			if (UpdateCache(go))
			{
				AnimationState animationState = base.animation[animName.Value];
				if (animationState == null)
				{
					LogWarning("Missing animation: " + animName.Value);
				}
				else
				{
					animationState.speed = speed.Value;
				}
			}
		}
	}
}
