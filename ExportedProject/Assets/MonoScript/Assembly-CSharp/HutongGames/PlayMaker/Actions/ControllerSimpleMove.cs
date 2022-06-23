using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Moves a Game Object with a Character Controller. Velocity along the y-axis is ignored. Speed is in meters/s. Gravity is automatically applied.")]
	[ActionCategory(ActionCategory.Character)]
	public class ControllerSimpleMove : FsmStateAction
	{
		[CheckForComponent(typeof(CharacterController))]
		[RequiredField]
		[Tooltip("The GameObject to move.")]
		public FsmOwnerDefault gameObject;

		[Tooltip("The movement vector.")]
		[RequiredField]
		public FsmVector3 moveVector;

		[Tooltip("Multiply the movement vector by a speed factor.")]
		public FsmFloat speed;

		[Tooltip("Move in local or word space.")]
		public Space space;

		private GameObject previousGo;

		private CharacterController controller;

		public override void Reset()
		{
			gameObject = null;
			moveVector = new FsmVector3
			{
				UseVariable = true
			};
			speed = 1f;
			space = Space.World;
		}

		public override void OnUpdate()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (!(ownerDefaultTarget == null))
			{
				if (ownerDefaultTarget != previousGo)
				{
					controller = ownerDefaultTarget.GetComponent<CharacterController>();
					previousGo = ownerDefaultTarget;
				}
				if (controller != null)
				{
					Vector3 vector = ((space != 0) ? ownerDefaultTarget.transform.TransformDirection(moveVector.Value) : moveVector.Value);
					controller.SimpleMove(vector * speed.Value);
				}
			}
		}
	}
}
