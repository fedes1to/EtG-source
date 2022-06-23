using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Removes the NPC with flair.")]
	[ActionCategory(".NPCs")]
	public class Vanish : FsmStateAction
	{
		[Tooltip("Seconds to wait before vanishing (not including the vanish animation).")]
		public FsmFloat delay;

		[Tooltip("Animation to play before vanishing.")]
		public FsmString vanishAnim;

		[Tooltip("Add GameObjects here to leave behind after vanishing.")]
		public FsmGameObject[] itemsToLeaveBehind;

		private float m_vanishTimer;

		public override void Reset()
		{
			delay = 0f;
			vanishAnim = string.Empty;
			itemsToLeaveBehind = new FsmGameObject[0];
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			tk2dSpriteAnimator component = base.Owner.GetComponent<tk2dSpriteAnimator>();
			AIAnimator component2 = base.Owner.GetComponent<AIAnimator>();
			if (!component && !component2)
			{
				return "Requires a 2D Toolkit animator or an AI Animator.\n";
			}
			if ((bool)component2)
			{
				if (!component2.HasDirectionalAnimation(vanishAnim.Value))
				{
					text = text + "Unknown animation " + vanishAnim.Value + ".\n";
				}
			}
			else if ((bool)component && component.GetClipByName(vanishAnim.Value) == null)
			{
				text = text + "Unknown animation " + vanishAnim.Value + ".\n";
			}
			return text;
		}

		public override void OnEnter()
		{
			if (delay.Value <= 0f)
			{
				DoVanish();
				Finish();
			}
			else
			{
				m_vanishTimer = delay.Value;
			}
		}

		public override void OnUpdate()
		{
			m_vanishTimer -= BraveTime.DeltaTime;
			if (m_vanishTimer <= 0f)
			{
				DoVanish();
				Finish();
			}
		}

		private void DoVanish()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(component.transform.position.IntXY(VectorConversions.Floor));
			roomFromPosition.DeregisterInteractable(component);
			component.CloseTextBox(true);
			if (component.specRigidbody != null)
			{
				component.specRigidbody.enabled = false;
			}
			for (int i = 0; i < itemsToLeaveBehind.Length; i++)
			{
				itemsToLeaveBehind[i].Value.transform.parent = component.transform.parent;
			}
			for (int j = 0; j < component.itemsToLeaveBehind.Count; j++)
			{
				component.itemsToLeaveBehind[j].transform.parent = component.transform.parent;
			}
			if ((bool)component.aiAnimator)
			{
				component.aiAnimator.PlayUntilFinished(vanishAnim.Value);
				Object.Destroy(component.gameObject, component.spriteAnimator.CurrentClip.BaseClipLength);
			}
			else if ((bool)component.spriteAnimator && component.spriteAnimator.GetClipByName(vanishAnim.Value) != null)
			{
				component.spriteAnimator.PlayAndDestroyObject(vanishAnim.Value);
			}
			else
			{
				Object.Destroy(component.gameObject);
			}
		}
	}
}
