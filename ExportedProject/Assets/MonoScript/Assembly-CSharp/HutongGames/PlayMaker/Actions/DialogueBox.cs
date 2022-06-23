using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Opens a dialogue box and speaks one or more lines of dialogue. Also supports one set of player responses.\nOnly the first valid Dialogue Box action will be run for a given state.")]
	public class DialogueBox : FsmStateAction
	{
		private enum DialogueState
		{
			ShowNextDialogue,
			ShowingDialogue,
			ShowingResponses,
			WaitingForResponse
		}

		public enum DialogueSequence
		{
			Default,
			Sequential,
			SeqThenRepeatLast,
			SeqThenRemoveState,
			Mutliline,
			PersistentSequential
		}

		public enum Condition
		{
			All = 0,
			FirstEncounterThisInstance = 1,
			FirstEverEncounter = 2,
			KeyboardAndMouse = 100,
			Controller = 110
		}

		[Tooltip("Only show this dialogue box if this condition is met")]
		public Condition condition;

		[Tooltip("Handles the dialogue sequence.")]
		[ActionSection("Text")]
		public DialogueSequence sequence;

		[Tooltip("The number of persistent strings to show for each key before progressing to the next one.")]
		public FsmInt persistentStringsToShow = 1;

		[Tooltip("Dialogue strings for the NPC to say.")]
		public FsmString[] dialogue;

		[CompoundArray("Responses", "Text", "Event")]
		public FsmString[] responses;

		public FsmEvent[] events;

		[Tooltip("If true, player distance will not cause the playerWalkedAway event to fire.")]
		[ActionSection("Advanced")]
		public FsmBool skipWalkAwayEvent;

		[Tooltip("If set, after this amount of time (seconds) the dialogue box will force close.")]
		public FsmFloat forceCloseTime;

		[Tooltip("If set, after the dialogue box closes it will remain up for this amount of time (seconds). Set to -1 to leave it up until something else overrides it.")]
		public FsmFloat zombieTime;

		[Tooltip("If true, don't use the default talk and idle animations.")]
		public FsmBool SuppressDefaultAnims;

		[Tooltip("If specified, use this animation instead of the default talk animation.")]
		public FsmString OverrideTalkAnim;

		[Tooltip("If marked, play the textbox over the player. Only for Pasts!")]
		public FsmBool PlayBoxOnInteractingPlayer;

		[Tooltip("Thot box")]
		public FsmBool IsThoughtBubble;

		[Tooltip("If used, play the textbox over this talk doer instead.")]
		public TalkDoerLite AlternativeTalker;

		private TalkDoerLite m_talkDoer;

		private DialogueState m_dialogueState;

		private int m_numDialogues;

		private int m_textIndex;

		private int m_persistentIndex;

		private float m_forceCloseTimer;

		private string[] m_rawResponses;

		private int m_sequentialStringLastIndex = -1;

		private string m_currentDialogueText;

		private string TalkAnimName
		{
			get
			{
				return (!SuppressDefaultAnims.Value && !string.IsNullOrEmpty(OverrideTalkAnim.Value)) ? OverrideTalkAnim.Value : "talk";
			}
		}

		public override void Reset()
		{
			condition = Condition.All;
			sequence = DialogueSequence.Default;
			persistentStringsToShow = 1;
			dialogue = new FsmString[1]
			{
				new FsmString(string.Empty)
			};
			responses = null;
			events = null;
			skipWalkAwayEvent = false;
			forceCloseTime = 0f;
			zombieTime = 0f;
			SuppressDefaultAnims = false;
			OverrideTalkAnim = string.Empty;
			PlayBoxOnInteractingPlayer = false;
			AlternativeTalker = null;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			AIAnimator component = base.Owner.GetComponent<AIAnimator>();
			if (!SuppressDefaultAnims.Value)
			{
				if (!component)
				{
					text += "Owner must have an AIAnimator to manage animations to use default animations.";
				}
				if ((bool)component && !component.HasDefaultAnimation)
				{
					text += "AIAnimator must have a default (base or idle) animation to use default animations.";
				}
				if ((bool)component && !component.HasDirectionalAnimation("talk"))
				{
					text += "AIAnimator must have a talk animation to use default animations.";
				}
			}
			if (sequence == DialogueSequence.Mutliline && dialogue.Length != 1)
			{
				text += "Multiline only supports a single dialogue string.\n";
			}
			if (sequence == DialogueSequence.Sequential && dialogue.Length != 1)
			{
				text += "Sequential only supports a single dialogue string.\n";
			}
			if (sequence == DialogueSequence.SeqThenRepeatLast && dialogue.Length != 1)
			{
				text += "SeqThenRepeatLast only supports a single dialogue string.\n";
			}
			if (sequence == DialogueSequence.SeqThenRemoveState && dialogue.Length != 1)
			{
				text += "SeqThenRemoveState only supports a single dialogue string.\n";
			}
			if (sequence == DialogueSequence.PersistentSequential && dialogue.Length < 2)
			{
				text += "PersistentSequential needs at least one sequential dialogue string and one stopper string.\n";
			}
			if (dialogue != null && dialogue.Length == 0)
			{
				text += "Dialogue strings must contain at least one line of dialogue.\n";
			}
			if (forceCloseTime.Value > 0f && responses != null && responses.Length != 0)
			{
				text += "Force Close Timer will be ignored if there are dialogue responses.\n";
			}
			return text;
		}

		public override void OnEnter()
		{
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
			if (ShouldSkip())
			{
				Finish();
				return;
			}
			m_dialogueState = DialogueState.ShowNextDialogue;
			if (sequence != DialogueSequence.PersistentSequential)
			{
				m_textIndex = 0;
			}
			m_forceCloseTimer = 0f;
			if (skipWalkAwayEvent.Value)
			{
				m_talkDoer.AllowWalkAways = false;
			}
			if (sequence == DialogueSequence.Default)
			{
				m_numDialogues = dialogue.Length;
			}
			else if (sequence == DialogueSequence.Mutliline)
			{
				m_numDialogues = StringTableManager.GetNumStrings(dialogue[0].Value);
			}
			else
			{
				m_numDialogues = 1;
			}
			m_rawResponses = new string[responses.Length];
			for (int i = 0; i < responses.Length; i++)
			{
				m_rawResponses[i] = StringTableManager.GetString(responses[i].Value);
				m_rawResponses[i] = NPCReplacementPostprocessString(m_rawResponses[i]);
			}
		}

		public override void OnUpdate()
		{
			if (m_dialogueState == DialogueState.ShowNextDialogue)
			{
				NextDialogue();
				m_dialogueState = DialogueState.ShowingDialogue;
				if (!SuppressDefaultAnims.Value)
				{
					if (AlternativeTalker != null)
					{
						AlternativeTalker.aiAnimator.PlayUntilFinished(TalkAnimName);
					}
					else
					{
						m_talkDoer.aiAnimator.PlayUntilFinished(TalkAnimName);
					}
				}
			}
			else if (m_dialogueState == DialogueState.ShowingDialogue)
			{
				bool flag = false;
				if (m_talkDoer.State == TalkDoerLite.TalkingState.Conversation)
				{
					BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(m_talkDoer.TalkingPlayer.PlayerIDX);
					bool suppressThisClick;
					flag = instanceForPlayer.WasAdvanceDialoguePressed(out suppressThisClick);
					if (suppressThisClick)
					{
						m_talkDoer.TalkingPlayer.SuppressThisClick = true;
					}
				}
				bool flag2 = false;
				if (m_forceCloseTimer > 0f)
				{
					m_forceCloseTimer -= BraveTime.DeltaTime;
					flag2 = m_forceCloseTimer <= 0f;
				}
				if (flag || flag2)
				{
					if (TextBoxManager.TextBoxCanBeAdvanced(m_talkDoer.speakPoint) && !flag2)
					{
						TextBoxManager.AdvanceTextBox(m_talkDoer.speakPoint);
						return;
					}
					if (m_textIndex < m_numDialogues && sequence != DialogueSequence.PersistentSequential)
					{
						if (m_talkDoer.echo1 != null)
						{
							m_talkDoer.echo1.IsDoingForcedSpeech = false;
						}
						if (m_talkDoer.echo2 != null)
						{
							m_talkDoer.echo2.IsDoingForcedSpeech = false;
						}
						m_dialogueState = DialogueState.ShowNextDialogue;
						return;
					}
					if (responses.Length > 0)
					{
						m_dialogueState = DialogueState.ShowingResponses;
						return;
					}
					if (forceCloseTime.Value != 0f && zombieTime.Value != 0f)
					{
						float a = forceCloseTime.Value + zombieTime.Value;
						float b = 0.5f + TextBoxManager.GetEstimatedReadingTime(m_currentDialogueText) * TextBoxManager.ZombieBoxMultiplier;
						float num = Mathf.Max(a, b);
						m_talkDoer.SetZombieBoxTimer(Mathf.Max(num - forceCloseTime.Value, 0.1f), TalkAnimName);
					}
					else if (!SuppressDefaultAnims.Value)
					{
						if (AlternativeTalker != null)
						{
							AlternativeTalker.aiAnimator.EndAnimationIf(TalkAnimName);
						}
						else
						{
							m_talkDoer.aiAnimator.EndAnimationIf(TalkAnimName);
						}
					}
					Finish();
				}
				else if (responses.Length > 0 && m_textIndex == m_numDialogues && !TextBoxManager.TextBoxCanBeAdvanced(m_talkDoer.speakPoint))
				{
					m_dialogueState = DialogueState.ShowingResponses;
				}
			}
			else if (m_dialogueState == DialogueState.ShowingResponses)
			{
				ShowResponses();
				m_dialogueState = DialogueState.WaitingForResponse;
			}
			else
			{
				int responseIndex;
				if (m_dialogueState != DialogueState.WaitingForResponse || !GameUIRoot.Instance.GetPlayerConversationResponse(out responseIndex))
				{
					return;
				}
				m_talkDoer.TalkingPlayer.ClearInputOverride("dialogueResponse");
				m_talkDoer.CloseTextBox(true);
				Finish();
				if (!SuppressDefaultAnims.Value)
				{
					if (AlternativeTalker != null)
					{
						AlternativeTalker.aiAnimator.EndAnimationIf(TalkAnimName);
					}
					else
					{
						m_talkDoer.aiAnimator.EndAnimationIf(TalkAnimName);
					}
				}
				base.Fsm.Event(events[responseIndex]);
			}
		}

		public override void OnExit()
		{
			if ((bool)m_talkDoer)
			{
				m_talkDoer.CloseTextBox(false);
				if (skipWalkAwayEvent.Value)
				{
					m_talkDoer.AllowWalkAways = true;
				}
			}
		}

		private bool ShouldSkip()
		{
			if (condition == Condition.FirstEncounterThisInstance)
			{
				if (m_talkDoer.NumTimesSpokenTo > 1)
				{
					return true;
				}
			}
			else if (condition == Condition.FirstEverEncounter)
			{
				EncounterTrackable component = base.Owner.GetComponent<EncounterTrackable>();
				if (component == null)
				{
					return true;
				}
				if (GameStatsManager.Instance.QueryEncounterable(component) > 1)
				{
					return true;
				}
			}
			else if (condition == Condition.KeyboardAndMouse)
			{
				if (!BraveInput.GetInstanceForPlayer(m_talkDoer.TalkingPlayer.PlayerIDX).IsKeyboardAndMouse())
				{
					return true;
				}
			}
			else if (condition == Condition.Controller && BraveInput.GetInstanceForPlayer(m_talkDoer.TalkingPlayer.PlayerIDX).IsKeyboardAndMouse())
			{
				return true;
			}
			for (int i = 0; i < base.State.Actions.Length && base.State.Actions[i] != this; i++)
			{
				if (base.State.Actions[i] is DialogueBox && base.State.Actions[i].Active)
				{
					return true;
				}
			}
			return false;
		}

		private string NPCReplacementPostprocessString(string input)
		{
			FsmString fsmString = base.Fsm.Variables.GetFsmString("npcReplacementString");
			if (fsmString != null && !string.IsNullOrEmpty(fsmString.Value))
			{
				input = input.Replace("%NPCREPLACEMENT", fsmString.Value);
			}
			string text = "%NPCNUMBER1";
			int num = 1;
			while (input.Contains(text))
			{
				FsmInt fsmInt = base.Fsm.Variables.GetFsmInt("npcNumber" + num);
				if (fsmInt != null)
				{
					input = input.Replace(text, fsmInt.Value.ToString());
				}
				num++;
				text = "%NPCNUMBER" + num;
			}
			return input;
		}

		private void NextDialogue()
		{
			if (m_textIndex > 0)
			{
			}
			bool flag = m_textIndex == m_numDialogues - 1;
			string text = "ERROR ERROR";
			if (m_textIndex < dialogue.Length && m_textIndex >= 0 && dialogue[m_textIndex].UsesVariable && !dialogue[m_textIndex].Value.StartsWith("#"))
			{
				text = dialogue[m_textIndex].Value;
			}
			else if (sequence == DialogueSequence.Default)
			{
				text = StringTableManager.GetString(dialogue[m_textIndex].Value);
			}
			else if (sequence == DialogueSequence.Mutliline)
			{
				text = StringTableManager.GetExactString(dialogue[0].Value, m_textIndex);
			}
			else if (sequence == DialogueSequence.SeqThenRemoveState)
			{
				bool isLast;
				text = StringTableManager.GetStringSequential(dialogue[0].Value, ref m_sequentialStringLastIndex, out isLast);
				if (isLast)
				{
					BravePlayMakerUtility.DisconnectState(base.State);
				}
			}
			else if (sequence == DialogueSequence.Sequential || sequence == DialogueSequence.SeqThenRepeatLast)
			{
				bool repeatLast = sequence == DialogueSequence.SeqThenRepeatLast;
				text = StringTableManager.GetStringSequential(dialogue[0].Value, ref m_sequentialStringLastIndex, repeatLast);
			}
			else if (sequence == DialogueSequence.PersistentSequential)
			{
				if (m_textIndex < dialogue.Length - 1)
				{
					text = StringTableManager.GetStringPersistentSequential(dialogue[m_textIndex].Value);
				}
				else
				{
					text = StringTableManager.GetString(dialogue[m_textIndex].Value);
					flag = true;
				}
			}
			if (text.Contains("$"))
			{
				string[] array = text.Split('$');
				text = array[0];
				if (array.Length > 1)
				{
					for (int i = 1; i < array.Length && i - 1 < m_rawResponses.Length; i++)
					{
						m_rawResponses[i - 1] = array[i];
					}
				}
			}
			else if (text.Contains("&"))
			{
				string[] array2 = text.Split('&');
				text = array2[0];
				if (m_talkDoer.echo1 != null)
				{
					m_talkDoer.echo1.ForceTimedSpeech(array2[1], 1f, 4f, TextBoxManager.BoxSlideOrientation.FORCE_RIGHT);
				}
				if (m_talkDoer.echo2 != null && array2.Length > 2)
				{
					m_talkDoer.echo2.ForceTimedSpeech(array2[2], 2f, 4f, TextBoxManager.BoxSlideOrientation.FORCE_LEFT);
				}
			}
			text = (m_currentDialogueText = NPCReplacementPostprocessString(text));
			ClearAlternativeTalkerFromPrevious();
			if (AlternativeTalker != null)
			{
				AlternativeTalker.SuppressClear = true;
				TextBoxManager.ClearTextBox(m_talkDoer.speakPoint);
				TextBoxManager.ClearTextBox(m_talkDoer.TalkingPlayer.transform);
				TalkDoerLite talkDoer = m_talkDoer;
				Vector3 worldPosition = AlternativeTalker.speakPoint.position + new Vector3(0f, 0f, -5f);
				Transform speakPoint = AlternativeTalker.speakPoint;
				float duration = -1f;
				string text2 = text;
				bool instant = false;
				bool showContinueText = HasNextDialogue() && m_talkDoer.State == TalkDoerLite.TalkingState.Conversation;
				talkDoer.ShowText(worldPosition, speakPoint, duration, text2, instant, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, showContinueText, IsThoughtBubble.Value, AlternativeTalker.audioCharacterSpeechTag);
			}
			else if (PlayBoxOnInteractingPlayer.Value)
			{
				TextBoxManager.ClearTextBox(m_talkDoer.speakPoint);
				TalkDoerLite talkDoer2 = m_talkDoer;
				Vector3 worldPosition = m_talkDoer.TalkingPlayer.CenterPosition.ToVector3ZUp(m_talkDoer.TalkingPlayer.CenterPosition.y) + new Vector3(0f, 1f, -5f);
				Transform speakPoint = m_talkDoer.TalkingPlayer.transform;
				float duration = -1f;
				string text2 = text;
				bool showContinueText = false;
				bool instant = HasNextDialogue() && m_talkDoer.State == TalkDoerLite.TalkingState.Conversation;
				talkDoer2.ShowText(worldPosition, speakPoint, duration, text2, showContinueText, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, instant, IsThoughtBubble.Value, m_talkDoer.TalkingPlayer.characterAudioSpeechTag);
			}
			else
			{
				if ((bool)m_talkDoer.TalkingPlayer)
				{
					TextBoxManager.ClearTextBox(m_talkDoer.TalkingPlayer.transform);
				}
				TalkDoerLite talkDoer3 = m_talkDoer;
				Vector3 worldPosition = m_talkDoer.speakPoint.position + new Vector3(0f, 0f, -5f);
				Transform speakPoint = m_talkDoer.speakPoint;
				float duration = -1f;
				string text2 = text;
				bool instant = false;
				bool showContinueText = HasNextDialogue() && m_talkDoer.State == TalkDoerLite.TalkingState.Conversation;
				talkDoer3.ShowText(worldPosition, speakPoint, duration, text2, instant, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, showContinueText, IsThoughtBubble.Value);
			}
			if (flag && forceCloseTime.Value > 0f)
			{
				m_forceCloseTimer = forceCloseTime.Value;
			}
			if (sequence == DialogueSequence.PersistentSequential)
			{
				m_persistentIndex++;
				if (m_persistentIndex >= persistentStringsToShow.Value)
				{
					m_persistentIndex = 0;
					m_textIndex = Mathf.Min(m_textIndex + 1, dialogue.Length - 1);
				}
			}
			else
			{
				m_textIndex++;
			}
		}

		private void ClearAlternativeTalkerFromPrevious()
		{
			FsmState previousActiveState = base.Fsm.PreviousActiveState;
			if (previousActiveState == null || previousActiveState == base.State)
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

		private void ShowResponses()
		{
			if (m_talkDoer.echo1 != null)
			{
				m_talkDoer.echo1.IsDoingForcedSpeech = false;
			}
			if (m_talkDoer.echo2 != null)
			{
				m_talkDoer.echo2.IsDoingForcedSpeech = false;
			}
			if (responses.Length > 0)
			{
				m_talkDoer.TalkingPlayer.SetInputOverride("dialogueResponse");
				GameUIRoot.Instance.DisplayPlayerConversationOptions(m_talkDoer.TalkingPlayer, m_rawResponses);
			}
		}

		private bool HasNextDialogue()
		{
			if (sequence == DialogueSequence.PersistentSequential)
			{
				return false;
			}
			if (m_textIndex < m_numDialogues - 1)
			{
				return true;
			}
			for (int i = 0; i < base.State.Transitions.Length; i++)
			{
				if (string.IsNullOrEmpty(base.State.Transitions[i].ToState))
				{
					continue;
				}
				FsmState state = base.Fsm.GetState(base.State.Transitions[i].ToState);
				for (int j = 0; j < state.Actions.Length; j++)
				{
					FsmStateAction fsmStateAction = state.Actions[j];
					if (fsmStateAction is DialogueBox)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
