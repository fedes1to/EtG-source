using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class TriggerNextMonsterHunt : FsmStateAction
	{
		public FsmBool OnlySetText = false;

		private TalkDoerLite m_talkDoer;

		public override void Reset()
		{
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			m_talkDoer = base.Owner.GetComponent<TalkDoerLite>();
			if (!OnlySetText.Value)
			{
				int num = GameStatsManager.Instance.huntProgress.TriggerNextQuest();
				if (num > 0)
				{
					LootEngine.SpawnCurrency(m_talkDoer.sprite.WorldCenter, num, true, Vector2.down * 1.75f, 45f);
				}
			}
			FsmString fsmString = base.Fsm.Variables.GetFsmString("QuestIntroString");
			if (fsmString != null && GameStatsManager.Instance.huntProgress.ActiveQuest != null)
			{
				fsmString.Value = GameStatsManager.Instance.huntProgress.ActiveQuest.QuestIntroString;
				DialogueBox.DialogueSequence dialogueSequence = DialogueBox.DialogueSequence.Mutliline;
				if (fsmString.Value.Contains("_GENERIC"))
				{
					dialogueSequence = DialogueBox.DialogueSequence.Default;
				}
				if (base.State.Transitions.Length > 0)
				{
					FsmState state = base.Fsm.GetState(base.State.Transitions[0].ToState);
					for (int i = 0; i < state.Actions.Length; i++)
					{
						if (state.Actions[i] is DialogueBox)
						{
							(state.Actions[i] as DialogueBox).sequence = dialogueSequence;
						}
						FsmState state2 = base.Fsm.GetState(state.Transitions[0].ToState);
						if (state2.Actions[0] is DialogueBox)
						{
							if (dialogueSequence == DialogueBox.DialogueSequence.Default)
							{
								state2.Actions[0].Enabled = true;
							}
							else
							{
								state2.Actions[0].Enabled = false;
							}
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(GameStatsManager.Instance.huntProgress.ActiveQuest.TargetStringKey))
			{
				Debug.Log("doing 1");
				FsmString fsmString2 = base.Fsm.Variables.GetFsmString("npcReplacementString");
				if (fsmString2 != null)
				{
					Debug.Log("doing 2: " + GameStatsManager.Instance.huntProgress.GetReplacementString());
					fsmString2.Value = GameStatsManager.Instance.huntProgress.GetReplacementString();
				}
			}
			FsmInt fsmInt = base.Fsm.Variables.GetFsmInt("npcNumber1");
			if (fsmInt != null)
			{
				fsmInt.Value = GameStatsManager.Instance.huntProgress.ActiveQuest.NumberKillsRequired - GameStatsManager.Instance.huntProgress.CurrentActiveMonsterHuntProgress;
			}
			Finish();
		}
	}
}
