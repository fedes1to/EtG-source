using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Starts an NPC's PathMover component.")]
	public class StartPathMoving : FsmStateAction
	{
		public FsmOwnerDefault GameObject;

		public FsmBool DisableCollideWithOthers;

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
			PathMover component = ownerDefaultTarget.GetComponent<PathMover>();
			if ((bool)component)
			{
				component.Paused = false;
				if (DisableCollideWithOthers.Value && (bool)component.specRigidbody)
				{
					component.specRigidbody.CollideWithOthers = false;
				}
			}
			Finish();
		}
	}
}
