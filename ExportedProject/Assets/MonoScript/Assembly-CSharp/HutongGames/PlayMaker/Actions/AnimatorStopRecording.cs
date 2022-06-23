using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Animator)]
	[Tooltip("Stops the animator record mode. It will lock the recording buffer's contents in its current state. The data get saved for subsequent playback with StartPlayback.")]
	public class AnimatorStopRecording : FsmStateAction
	{
		[Tooltip("The target. An Animator component and a PlayMakerAnimatorProxy component are required")]
		[CheckForComponent(typeof(Animator))]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("The recorder StartTime")]
		[UIHint(UIHint.Variable)]
		[ActionSection("Results")]
		public FsmFloat recorderStartTime;

		[Tooltip("The recorder StopTime")]
		[UIHint(UIHint.Variable)]
		public FsmFloat recorderStopTime;

		public override void Reset()
		{
			gameObject = null;
			recorderStartTime = null;
			recorderStopTime = null;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (ownerDefaultTarget == null)
			{
				Finish();
				return;
			}
			Animator component = ownerDefaultTarget.GetComponent<Animator>();
			if (component != null)
			{
				component.StopRecording();
				recorderStartTime.Value = component.recorderStartTime;
				recorderStopTime.Value = component.recorderStopTime;
			}
			Finish();
		}
	}
}
