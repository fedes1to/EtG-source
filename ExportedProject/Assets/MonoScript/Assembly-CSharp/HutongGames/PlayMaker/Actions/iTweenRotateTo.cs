using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Rotates a GameObject to the supplied Euler angles in degrees over time.")]
	[ActionCategory("iTween")]
	public class iTweenRotateTo : iTweenFsmAction
	{
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("iTween ID. If set you can use iTween Stop action to stop it by its id.")]
		public FsmString id;

		[Tooltip("Rotate to a transform rotation.")]
		public FsmGameObject transformRotation;

		[Tooltip("A rotation the GameObject will animate from.")]
		public FsmVector3 vectorRotation;

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

		[Tooltip("Whether to animate in local or world space.")]
		public Space space;

		public override void Reset()
		{
			base.Reset();
			id = new FsmString
			{
				UseVariable = true
			};
			transformRotation = new FsmGameObject
			{
				UseVariable = true
			};
			vectorRotation = new FsmVector3
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
			space = Space.World;
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
				Vector3 vector = ((!vectorRotation.IsNone) ? vectorRotation.Value : Vector3.zero);
				if (!transformRotation.IsNone && (bool)transformRotation.Value)
				{
					vector = ((space != 0) ? (transformRotation.Value.transform.localEulerAngles + vector) : (transformRotation.Value.transform.eulerAngles + vector));
				}
				itweenType = "rotate";
				iTween.RotateTo(ownerDefaultTarget, iTween.Hash("rotation", vector, "name", (!id.IsNone) ? id.Value : string.Empty, (!speed.IsNone) ? "speed" : "time", (!speed.IsNone) ? speed.Value : ((!time.IsNone) ? time.Value : 1f), "delay", (!delay.IsNone) ? delay.Value : 0f, "easetype", easeType, "looptype", loopType, "oncomplete", "iTweenOnComplete", "oncompleteparams", itweenID, "onstart", "iTweenOnStart", "onstartparams", itweenID, "ignoretimescale", !realTime.IsNone && realTime.Value, "islocal", space == Space.Self));
			}
		}
	}
}
