using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("iTween")]
	[Tooltip("Instantly rotates a GameObject to look at the supplied Vector3 then returns it to it's starting rotation over time.")]
	public class iTweenLookFrom : iTweenFsmAction
	{
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("iTween ID. If set you can use iTween Stop action to stop it by its id.")]
		public FsmString id;

		[Tooltip("Look from a transform position.")]
		public FsmGameObject transformTarget;

		[Tooltip("A target position the GameObject will look at. If Transform Target is defined this is used as a local offset.")]
		public FsmVector3 vectorTarget;

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

		[Tooltip("Restricts rotation to the supplied axis only.")]
		public AxisRestriction axis;

		public override void Reset()
		{
			base.Reset();
			id = new FsmString
			{
				UseVariable = true
			};
			transformTarget = new FsmGameObject
			{
				UseVariable = true
			};
			vectorTarget = new FsmVector3
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
			axis = AxisRestriction.none;
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
				Vector3 vector = ((!vectorTarget.IsNone) ? vectorTarget.Value : Vector3.zero);
				if (!transformTarget.IsNone && (bool)transformTarget.Value)
				{
					vector = transformTarget.Value.transform.position + vector;
				}
				itweenType = "rotate";
				iTween.LookFrom(ownerDefaultTarget, iTween.Hash("looktarget", vector, "name", (!id.IsNone) ? id.Value : string.Empty, (!speed.IsNone) ? "speed" : "time", (!speed.IsNone) ? speed.Value : ((!time.IsNone) ? time.Value : 1f), "delay", (!delay.IsNone) ? delay.Value : 0f, "easetype", easeType, "looptype", loopType, "oncomplete", "iTweenOnComplete", "oncompleteparams", itweenID, "onstart", "iTweenOnStart", "onstartparams", itweenID, "ignoretimescale", !realTime.IsNone && realTime.Value, "axis", (axis != 0) ? Enum.GetName(typeof(AxisRestriction), axis) : string.Empty));
			}
		}
	}
}
