using System;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	public static class BravePlayMakerUtility
	{
		public enum ConsumableType
		{
			Currency,
			Keys,
			Hearts,
			HeartContainers,
			MetaCurrency,
			Blanks,
			Armor
		}

		public static FsmTransition[] CachedGlobalTransitions { get; set; }

		public static string CheckCurrentModeVariable(Fsm fsm)
		{
			FsmString fsmString = fsm.Variables.FindFsmString("currentMode");
			if (fsmString == null)
			{
				List<FsmString> list = new List<FsmString>(fsm.Variables.StringVariables);
				FsmString fsmString2 = new FsmString("currentMode");
				fsmString2.Value = "modeBegin";
				list.Add(fsmString2);
				fsm.Variables.StringVariables = list.ToArray();
			}
			return string.Empty;
		}

		public static string CheckEventExists(Fsm fsm, string eventName)
		{
			if (fsm != null && !Array.Exists(fsm.Events, (FsmEvent e) => e.Name == eventName))
			{
				return string.Format("No event with name \"{0}\" exists.\n", eventName);
			}
			return string.Empty;
		}

		public static string CheckGlobalTransitionExists(Fsm fsm, string eventName)
		{
			if (fsm != null && !Array.Exists(fsm.GlobalTransitions, (FsmTransition t) => t.EventName == eventName))
			{
				return string.Format("No global transition exists for the event \"{0}\".\n", eventName);
			}
			return string.Empty;
		}

		public static bool ModeIsSetSomewhere(Fsm fsm, string eventName)
		{
			FsmState[] states = fsm.States;
			foreach (FsmState fsmState in states)
			{
				FsmStateAction[] actions = fsmState.Actions;
				foreach (FsmStateAction fsmStateAction in actions)
				{
					if (fsmStateAction is SetMode && (fsmStateAction as SetMode).mode.Value == eventName)
					{
						return true;
					}
					if (fsmStateAction is TestSaveFlag && (fsmStateAction as TestSaveFlag).successType == TestSaveFlag.SuccessType.SetMode && (fsmStateAction as TestSaveFlag).mode.Value == eventName)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool AllOthersAreFinished(FsmStateAction action)
		{
			for (int i = 0; i < action.State.Actions.Length; i++)
			{
				FsmStateAction fsmStateAction = action.State.Actions[i];
				if (fsmStateAction != action && !(fsmStateAction is INonFinishingState) && !fsmStateAction.Finished)
				{
					return false;
				}
			}
			return true;
		}

		public static void DisconnectState(FsmState fsmState)
		{
			Fsm fsm = fsmState.Fsm;
			for (int i = 0; i < fsm.GlobalTransitions.Length; i++)
			{
				FsmTransition fsmTransition = fsm.GlobalTransitions[i];
				if (fsmTransition.ToState == fsmState.Name)
				{
					fsmTransition.FsmEvent = null;
					fsmTransition.ToState = string.Empty;
				}
			}
			for (int j = 0; j < fsm.States.Length; j++)
			{
				FsmState fsmState2 = fsm.States[j];
				for (int k = 0; k < fsmState2.Transitions.Length; k++)
				{
					FsmTransition fsmTransition2 = fsmState2.Transitions[k];
					if (fsmTransition2.ToState == fsmState.Name)
					{
						fsmTransition2.FsmEvent = null;
						fsmTransition2.ToState = string.Empty;
					}
				}
			}
		}

		public static float GetConsumableValue(PlayerController player, ConsumableType consumableType)
		{
			switch (consumableType)
			{
			case ConsumableType.Currency:
				return player.carriedConsumables.Currency;
			case ConsumableType.Keys:
				return player.carriedConsumables.KeyBullets;
			case ConsumableType.Hearts:
				return player.healthHaver.GetCurrentHealth();
			case ConsumableType.HeartContainers:
				return player.healthHaver.GetMaxHealth();
			case ConsumableType.Blanks:
				return player.Blanks;
			case ConsumableType.MetaCurrency:
				return GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY);
			case ConsumableType.Armor:
				return player.healthHaver.Armor;
			default:
				Debug.LogError("Unknown consumable type: " + consumableType);
				return 0f;
			}
		}

		public static void SetConsumableValue(PlayerController player, ConsumableType consumableType, float value)
		{
			switch (consumableType)
			{
			case ConsumableType.Currency:
				player.carriedConsumables.Currency = Mathf.RoundToInt(value);
				break;
			case ConsumableType.Keys:
				player.carriedConsumables.KeyBullets = Mathf.RoundToInt(value);
				break;
			case ConsumableType.Hearts:
				player.healthHaver.ForceSetCurrentHealth(BraveMathCollege.QuantizeFloat(value, 0.5f));
				break;
			case ConsumableType.HeartContainers:
				player.healthHaver.SetHealthMaximum(BraveMathCollege.QuantizeFloat(value, 0.5f));
				break;
			case ConsumableType.Blanks:
				player.Blanks = Mathf.FloorToInt(value);
				break;
			case ConsumableType.MetaCurrency:
				GameStatsManager.Instance.ClearStatValueGlobal(TrackedStats.META_CURRENCY);
				GameStatsManager.Instance.SetStat(TrackedStats.META_CURRENCY, value);
				break;
			case ConsumableType.Armor:
				if (player.ForceZeroHealthState && value == 0f)
				{
					value = 1f;
				}
				player.healthHaver.Armor = Mathf.RoundToInt(value);
				break;
			default:
				Debug.LogError("Unknown consumable type: " + consumableType);
				break;
			}
		}
	}
}
