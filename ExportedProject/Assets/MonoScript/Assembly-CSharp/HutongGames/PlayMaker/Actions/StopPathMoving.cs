using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Stops an NPC's PathMover component.")]
	[ActionCategory(".NPCs")]
	public class StopPathMoving : FsmStateAction
	{
		public FsmOwnerDefault GameObject;

		public FsmBool ReenableCollideWithOthers;

		public override string ErrorCheck()
		{
			string text = string.Empty;
			GameObject gameObject = ((GameObject.OwnerOption != 0) ? GameObject.GameObject.Value : base.Owner);
			if ((bool)gameObject)
			{
				if (!gameObject.GetComponent<PathMover>())
				{
					text += "Must have a PathMover component.\n";
				}
			}
			else if (!GameObject.GameObject.UseVariable)
			{
				return "No object specified";
			}
			return text;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(GameObject);
			if (!ownerDefaultTarget)
			{
				return;
			}
			PathMover component = ownerDefaultTarget.GetComponent<PathMover>();
			if ((bool)component)
			{
				component.Paused = true;
				if (ReenableCollideWithOthers.Value && (bool)component.specRigidbody)
				{
					component.specRigidbody.CollideWithOthers = true;
				}
				Finish();
			}
		}
	}
}
