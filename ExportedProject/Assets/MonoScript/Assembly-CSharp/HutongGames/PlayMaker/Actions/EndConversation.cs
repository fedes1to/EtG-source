using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Ends an NPC conversation (makes the NPC interactable).")]
	[ActionCategory(".NPCs")]
	public class EndConversation : FsmStateAction
	{
		[Tooltip("If true, force closes all text boxes, even zombie text boxes.")]
		public FsmBool killZombieTextBoxes;

		public FsmBool doNotLerpCamera;

		public FsmBool suppressReinteractDelay;

		public FsmBool suppressFurtherInteraction;

		public override void Reset()
		{
			killZombieTextBoxes = false;
		}

		public static void ForceEndConversation(TalkDoerLite talkDoer)
		{
			if (talkDoer.TalkingPlayer != null && talkDoer.State == TalkDoerLite.TalkingState.Conversation)
			{
				if (Vector2.Distance(talkDoer.TalkingPlayer.sprite.WorldCenter, talkDoer.sprite.WorldCenter) <= talkDoer.conversationBreakRadius)
				{
					talkDoer.CompletedTalkingPlayer = talkDoer.TalkingPlayer;
				}
				else
				{
					talkDoer.CompletedTalkingPlayer = null;
				}
			}
			if (talkDoer.HasPlayerLocked)
			{
				talkDoer.TalkingPlayer.ClearInputOverride("conversation");
				talkDoer.HasPlayerLocked = false;
				Pixelator.Instance.LerpToLetterbox(0.5f, 0.25f);
				Pixelator.Instance.DoFinalNonFadedLayer = false;
				GameUIRoot.Instance.ToggleLowerPanels(true, false, "conversation");
				GameUIRoot.Instance.ShowCoreUI("conversation");
				if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
				{
					GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().ForceRevealMetaCurrencyPanel();
				}
				Minimap.Instance.TemporarilyPreventMinimap = false;
				GameManager.Instance.MainCameraController.SetManualControl(false);
			}
			if ((bool)talkDoer.TalkingPlayer)
			{
				TextBoxManager.ClearTextBox(talkDoer.TalkingPlayer.transform);
			}
			talkDoer.IsTalking = false;
			talkDoer.TalkingPlayer = null;
			talkDoer.CloseTextBox(true);
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (!component)
			{
				return;
			}
			if (component.TalkingPlayer != null && component.State == TalkDoerLite.TalkingState.Conversation)
			{
				if (Vector2.Distance(component.TalkingPlayer.sprite.WorldCenter, component.sprite.WorldCenter) <= component.conversationBreakRadius)
				{
					component.CompletedTalkingPlayer = component.TalkingPlayer;
				}
				else
				{
					component.CompletedTalkingPlayer = null;
				}
			}
			if (component.HasPlayerLocked)
			{
				component.TalkingPlayer.ClearInputOverride("conversation");
				component.HasPlayerLocked = false;
				Pixelator.Instance.LerpToLetterbox(0.5f, 0.25f);
				Pixelator.Instance.DoFinalNonFadedLayer = false;
				GameUIRoot.Instance.ToggleLowerPanels(true, false, "conversation");
				GameUIRoot.Instance.ShowCoreUI("conversation");
				if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
				{
					GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().ForceRevealMetaCurrencyPanel();
				}
				Minimap.Instance.TemporarilyPreventMinimap = false;
				if (!doNotLerpCamera.Value)
				{
					GameManager.Instance.MainCameraController.SetManualControl(false);
				}
			}
			if (suppressReinteractDelay.Value)
			{
				component.SuppressReinteractDelay = true;
			}
			if ((bool)component.TalkingPlayer)
			{
				TextBoxManager.ClearTextBox(component.TalkingPlayer.transform);
			}
			ClearAlternativeTalkerFromPrevious();
			component.IsTalking = false;
			component.TalkingPlayer = null;
			component.CloseTextBox(killZombieTextBoxes.Value);
			if (suppressReinteractDelay.Value)
			{
				component.SuppressReinteractDelay = false;
			}
			if (suppressFurtherInteraction.Value)
			{
				component.ForceNonInteractable = true;
			}
			Finish();
		}

		private void ClearAlternativeTalkerFromPrevious()
		{
			FsmState previousActiveState = base.Fsm.PreviousActiveState;
			if (previousActiveState == null)
			{
				return;
			}
			for (int i = 0; i < previousActiveState.Actions.Length; i++)
			{
				if (previousActiveState.Actions[i] is DialogueBox)
				{
					DialogueBox dialogueBox = previousActiveState.Actions[i] as DialogueBox;
					if (dialogueBox.AlternativeTalker != null)
					{
						dialogueBox.AlternativeTalker.SuppressClear = false;
						TextBoxManager.ClearTextBox(dialogueBox.AlternativeTalker.speakPoint);
					}
				}
			}
		}
	}
}
