using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Instantly changes a GameObject's scale then returns it to it's starting scale over time.")]
	[ActionCategory("iTween")]
	public class iTweenScaleFrom : iTweenFsmAction
	{
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("iTween ID. If set you can use iTween Stop action to stop it by its id.")]
		public FsmString id;

		[Tooltip("Scale From a transform scale.")]
		public FsmGameObject transformScale;

		[Tooltip("A scale vector the GameObject will animate From.")]
		public FsmVector3 vectorScale;

		[Tooltip("The time in seconds the animation will take to complete.")]
		public FsmFloat time;

		[Tooltip("The time in seconds the animation will wait before beginning.")]
		public FsmFloat delay;

		[Tooltip("Can be used instead of time to allow animation based on speed. When you define speed the time variable is ignored.")]
		public FsmFloat speed;

		[Tooltip("The shape of the easing curve applied to the animation.")]
		public iTween.EaseType easeType = iTween.EaseType.linear;

		[Tooltip("The type of loop to apply once the animation has completed.")]
		public iTween.LoopType loopType;

		public override void Reset()
		{
			base.Reset();
			id = new FsmString
			{
				UseVariable = true
			};
			transformScale = new FsmGameObject
			{
				UseVariable = true
			};
			vectorScale = new FsmVector3
			{
				UseVariable = true
			};
			time = 1f;
			delay = 0f;
			loopType = iTween.LoopType.none;
			speed = new FsmFloat
			{
				UseVariable = true
			};
		}

		public override void OnEnter()
		{
			OnEnteriTween(gameObject);
			if (loopType != 0)
			{
				IsLoop(true);
			}
			DoiTween();
		}

		public override void OnExit()
		{
			OnExitiTween(gameObject);
		}

		private void DoiTween()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (!(ownerDefaultTarget == null))
			{
				Vector3 vector = ((!vectorScale.IsNone) ? vectorScale.Value : Vector3.zero);
				if (!transformScale.IsNone && (bool)transformScale.Value)
				{
					vector = transformScale.Value.transform.localScale + vector;
				}
				itweenType = "scale";
				iTween.ScaleFrom(ownerDefaultTarget, iTween.Hash("scale", vector, "name", (!id.IsNone) ? id.Value : string.Empty, (!speed.IsNone) ? "speed" : "time", (!speed.IsNone) ? speed.Value : ((!time.IsNone) ? time.Value : 1f), "delay", (!delay.IsNone) ? delay.Value : 0f, "easetype", easeType, "looptype", loopType, "oncomplete", "iTweenOnComplete", "oncompleteparams", itweenID, "onstart", "iTweenOnStart", "onstartparams", itweenID, "ignoretimescale", !realTime.IsNone && realTime.Value));
			}
		}
	}
}
