using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sends Events based on synergy completion possibility.")]
	[ActionCategory(".Brave")]
	public class SynergraceTestCompletionPossible : BraveFsmStateAction
	{
		public enum SuccessType
		{
			SetMode,
			SendEvent
		}

		public SuccessType successType;

		[Tooltip("The event to send if the proceeding tests all pass.")]
		public new FsmEvent Event;

		[Tooltip("The name of the mode to set 'currentMode' to if the proceeding tests all pass.")]
		public FsmString mode;

		public FsmBool everyFrame;

		[NonSerialized]
		public GameObject SelectedPickupGameObject;

		private bool m_success;

		public bool Success
		{
			get
			{
				return m_success;
			}
		}

		public override void Reset()
		{
			successType = SuccessType.SetMode;
			Event = null;
			mode = string.Empty;
		}

		public override void OnEnter()
		{
			DoCheck();
			if (!everyFrame.Value)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoCheck();
		}

		private void DoCheck()
		{
			m_success = false;
			GenericLootTable genericLootTable = ((!(UnityEngine.Random.value < 0.5f)) ? GameManager.Instance.RewardManager.GunsLootTable : GameManager.Instance.RewardManager.ItemsLootTable);
			GenericLootTable tableToUse = ((!(genericLootTable == GameManager.Instance.RewardManager.GunsLootTable)) ? GameManager.Instance.RewardManager.GunsLootTable : GameManager.Instance.RewardManager.ItemsLootTable);
			SynercacheManager.UseCachedSynergyIDs = true;
			GameObject gameObject = GameManager.Instance.RewardManager.GetItemForPlayer(GameManager.Instance.BestActivePlayer, genericLootTable, PickupObject.ItemQuality.A, null, false, null, false, null, true);
			if ((bool)gameObject)
			{
				PickupObject component = gameObject.GetComponent<PickupObject>();
				bool usesStartingItem = false;
				if (!component || !RewardManager.AnyPlayerHasItemInSynergyContainingOtherItem(component, ref usesStartingItem))
				{
					gameObject = null;
				}
			}
			if (!gameObject)
			{
				gameObject = GameManager.Instance.RewardManager.GetItemForPlayer(GameManager.Instance.BestActivePlayer, tableToUse, PickupObject.ItemQuality.A, null, false, null, false, null, true);
			}
			if ((bool)gameObject)
			{
				PickupObject component2 = gameObject.GetComponent<PickupObject>();
				bool usesStartingItem2 = false;
				if ((bool)component2 && RewardManager.AnyPlayerHasItemInSynergyContainingOtherItem(component2, ref usesStartingItem2))
				{
					m_success = true;
					SelectedPickupGameObject = gameObject;
				}
			}
			SynercacheManager.UseCachedSynergyIDs = false;
			if (m_success)
			{
				if (successType == SuccessType.SendEvent)
				{
					base.Fsm.Event(Event);
				}
				else if (successType == SuccessType.SetMode)
				{
					FsmString fsmString = base.Fsm.Variables.GetFsmString("currentMode");
					fsmString.Value = mode.Value;
				}
				Finish();
			}
		}
	}
}
