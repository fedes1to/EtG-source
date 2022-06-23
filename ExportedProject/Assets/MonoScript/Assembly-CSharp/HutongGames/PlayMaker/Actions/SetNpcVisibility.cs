using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Sets the NPC's visibility (renderers and Speculative Rigidbody).")]
	public class SetNpcVisibility : FsmStateAction
	{
		[Tooltip("Set visibility to this.")]
		public FsmBool visible;

		public override void Reset()
		{
			visible = true;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if ((bool)component)
			{
				SetVisible(component, visible.Value);
			}
			Finish();
		}

		public static void SetVisible(TalkDoerLite talkDoer, bool visible)
		{
			talkDoer.renderer.enabled = visible;
			talkDoer.ShowOutlines = visible;
			if ((bool)talkDoer.shadow)
			{
				talkDoer.shadow.GetComponent<Renderer>().enabled = visible;
			}
			if ((bool)talkDoer.specRigidbody)
			{
				talkDoer.specRigidbody.enabled = visible;
			}
			if ((bool)talkDoer.ultraFortunesFavor)
			{
				talkDoer.ultraFortunesFavor.enabled = visible;
			}
		}
	}
}
