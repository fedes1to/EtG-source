using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public class BlacksmithDetectItem : FsmStateAction
	{
		public DesiredItem[] desires;

		public FsmEvent NextDesireEvent;

		public FsmEvent OutOfDesiresEvent;

		private int m_currentDesireIndex;

		private List<PickupObject> m_currentTargets;

		private int m_currentTargetIndex;

		private bool m_hasNonItemTarget;

		private PlayerController talkingPlayer;

		private PickupObject m_currentTarget;

		public DesiredItem GetCurrentDesire()
		{
			return desires[m_currentDesireIndex];
		}

		public PickupObject GetTargetPickupObject()
		{
			return m_currentTarget;
		}

		public override void Reset()
		{
		}

		public override string ErrorCheck()
		{
			return string.Empty;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			talkingPlayer = component.TalkingPlayer;
			m_hasNonItemTarget = false;
			DoCheck();
			Finish();
		}

		private void CheckPlayerForDesire(DesiredItem desire)
		{
			m_currentTargets = new List<PickupObject>();
			if (desire.type == DesiredItem.DetectType.SPECIFIC_ITEM)
			{
				PickupObject byId = PickupObjectDatabase.GetById(desire.specificItemId);
				if (byId is Gun)
				{
					for (int i = 0; i < talkingPlayer.inventory.AllGuns.Count; i++)
					{
						if (talkingPlayer.inventory.AllGuns[i].PickupObjectId == byId.PickupObjectId)
						{
							m_currentTargets.Add(byId);
						}
					}
				}
				else if (byId is PlayerItem)
				{
					for (int j = 0; j < talkingPlayer.activeItems.Count; j++)
					{
						if (talkingPlayer.activeItems[j].PickupObjectId == byId.PickupObjectId)
						{
							m_currentTargets.Add(byId);
						}
					}
				}
				else
				{
					if (!(byId is PassiveItem))
					{
						return;
					}
					for (int k = 0; k < GameManager.Instance.PrimaryPlayer.passiveItems.Count; k++)
					{
						if (talkingPlayer.passiveItems[k].PickupObjectId == byId.PickupObjectId)
						{
							m_currentTargets.Add(byId);
						}
					}
				}
			}
			else if (desire.type == DesiredItem.DetectType.CURRENCY)
			{
				if (talkingPlayer.carriedConsumables.Currency >= desire.amount)
				{
					m_hasNonItemTarget = true;
				}
			}
			else if (desire.type == DesiredItem.DetectType.META_CURRENCY)
			{
				int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY));
				if (num >= desire.amount)
				{
					m_hasNonItemTarget = true;
				}
			}
			else if (desire.type == DesiredItem.DetectType.KEYS && talkingPlayer.carriedConsumables.KeyBullets >= desire.amount)
			{
				m_hasNonItemTarget = true;
			}
		}

		private void NextDesire()
		{
			if (!(m_currentTarget != null))
			{
				m_currentTargets = null;
				m_currentTargetIndex = 0;
				m_currentDesireIndex++;
				base.Fsm.Event(NextDesireEvent);
			}
		}

		private void DoCheck()
		{
			m_currentTarget = null;
			m_hasNonItemTarget = false;
			if (m_currentDesireIndex >= desires.Length)
			{
				m_currentDesireIndex = 0;
				m_currentTargets = null;
				m_currentTargetIndex = 0;
				base.Fsm.Event(OutOfDesiresEvent);
				return;
			}
			DesiredItem desiredItem = desires[m_currentDesireIndex];
			if (GameStatsManager.Instance.GetFlag(desiredItem.flagToSet))
			{
				NextDesire();
				return;
			}
			if (m_currentTargets == null)
			{
				m_currentTargetIndex = 0;
				CheckPlayerForDesire(desiredItem);
			}
			if (m_currentTargetIndex >= m_currentTargets.Count && !m_hasNonItemTarget)
			{
				NextDesire();
				return;
			}
			if (m_currentTargets.Count > 0)
			{
				PickupObject pickupObject = (m_currentTarget = m_currentTargets[m_currentTargetIndex]);
				m_currentTargetIndex++;
				FsmString fsmString = base.Fsm.Variables.GetFsmString("npcReplacementString");
				EncounterTrackable component = pickupObject.GetComponent<EncounterTrackable>();
				if (fsmString != null && component != null)
				{
					fsmString.Value = component.GetModifiedDisplayName();
				}
			}
			DialogueBox dialogueBox = null;
			for (int i = 0; i < base.State.Actions.Length; i++)
			{
				if (base.State.Actions[i] is DialogueBox)
				{
					dialogueBox = base.State.Actions[i] as DialogueBox;
				}
			}
			switch (desiredItem.type)
			{
			case DesiredItem.DetectType.SPECIFIC_ITEM:
				dialogueBox.dialogue[0].Value = "#BLACKSMITH_ASK_FOR_SPECIFIC";
				break;
			case DesiredItem.DetectType.CURRENCY:
				dialogueBox.dialogue[0].Value = "#BLACKSMITH_ASK_FOR_AMOUNT_OF_COINS";
				break;
			case DesiredItem.DetectType.META_CURRENCY:
				dialogueBox.dialogue[0].Value = "#BLACKSMITH_ASK_FOR_AMOUNT_OF_META_CURRENCY";
				break;
			case DesiredItem.DetectType.KEYS:
				dialogueBox.dialogue[0].Value = "#BLACKSMITH_ASK_FOR_AMOUNT_OF_KEYS";
				break;
			default:
				dialogueBox.dialogue[0].Value = "#BLACKSMITH_ASK_FOR_SPECIFIC";
				break;
			}
		}
	}
}
