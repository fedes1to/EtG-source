using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Tests if an FSM is in the specified State.")]
	[ActionCategory(ActionCategory.Logic)]
	[ActionTarget(typeof(PlayMakerFSM), "gameObject,fsmName", false)]
	public class FsmStateTest : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The GameObject that owns the FSM.")]
		public FsmGameObject gameObject;

		[Tooltip("Optional name of Fsm on Game Object. Useful if there is more than one FSM on the GameObject.")]
		[UIHint(UIHint.FsmName)]
		public FsmString fsmName;

		[Tooltip("Check to see if the FSM is in this state.")]
		[RequiredField]
		public FsmString stateName;

		[Tooltip("Event to send if the FSM is in the specified state.")]
		public FsmEvent trueEvent;

		[Tooltip("Event to send if the FSM is NOT in the specified state.")]
		public FsmEvent falseEvent;

		[Tooltip("Store the result of this test in a bool variable. Useful if other actions depend on this test.")]
		[UIHint(UIHint.Variable)]
		public FsmBool storeResult;

		[Tooltip("Repeat every frame. Useful if you're waiting for a particular state.")]
		public bool everyFrame;

		private GameObject previousGo;

		private PlayMakerFSM fsm;

		public override void Reset()
		{
			gameObject = null;
			fsmName = null;
			stateName = null;
			trueEvent = null;
			falseEvent = null;
			storeResult = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoFsmStateTest();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoFsmStateTest();
		}

		private void DoFsmStateTest()
		{
			GameObject value = gameObject.Value;
			if (value == null)
			{
				return;
			}
			if (value != previousGo)
			{
				fsm = ActionHelpers.GetGameObjectFsm(value, fsmName.Value);
				previousGo = value;
			}
			if (!(fsm == null))
			{
				bool value2 = false;
				if (fsm.ActiveStateName == stateName.Value)
				{
					base.Fsm.Event(trueEvent);
					value2 = true;
				}
				else
				{
					base.Fsm.Event(falseEvent);
				}
				storeResult.Value = value2;
			}
		}
	}
}
