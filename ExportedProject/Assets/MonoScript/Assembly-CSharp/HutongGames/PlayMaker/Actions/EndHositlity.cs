using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Makes this NPC become an enemy.")]
	public class EndHositlity : FsmStateAction
	{
		public FsmBool DontMoveNPC = false;

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (!DontMoveNPC.Value)
			{
				component.transform.position += (Vector3)(component.HostileObject.specRigidbody.UnitBottomLeft - component.specRigidbody.UnitBottomLeft);
				component.specRigidbody.Reinitialize();
			}
			SetNpcVisibility.SetVisible(component, true);
			component.aiAnimator.FacingDirection = component.HostileObject.aiAnimator.FacingDirection;
			component.aiAnimator.LockFacingDirection = true;
			Finish();
		}
	}
}
