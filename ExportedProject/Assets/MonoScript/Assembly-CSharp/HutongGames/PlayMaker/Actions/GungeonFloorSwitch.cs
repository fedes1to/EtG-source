namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Sends an Event based on the current floor.")]
	[ActionCategory(ActionCategory.Logic)]
	public class GungeonFloorSwitch : FsmStateAction
	{
		public bool DoSendEvent = true;

		public bool ChangeVariable;

		public GlobalDungeonData.ValidTilesets[] compareTo;

		public FsmEvent[] sendEvent;

		public GlobalDungeonData.ValidTilesets[] varCompareTo;

		public FsmString[] targetStrings;

		public FsmString targetVariable;

		public bool everyFrame;

		public override void Reset()
		{
			compareTo = new GlobalDungeonData.ValidTilesets[1];
			sendEvent = new FsmEvent[1];
			everyFrame = false;
		}

		public override void OnEnter()
		{
			if (DoSendEvent)
			{
				DoFloorSwitch();
			}
			if (ChangeVariable)
			{
				DoVariableSwitch();
			}
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (DoSendEvent)
			{
				DoFloorSwitch();
			}
			if (ChangeVariable)
			{
				DoVariableSwitch();
			}
		}

		private void DoVariableSwitch()
		{
			for (int i = 0; i < varCompareTo.Length; i++)
			{
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == varCompareTo[i])
				{
					targetVariable.Value = targetStrings[i].Value;
					break;
				}
			}
			Finish();
		}

		private void DoFloorSwitch()
		{
			for (int i = 0; i < compareTo.Length; i++)
			{
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == compareTo[i])
				{
					base.Fsm.Event(sendEvent[i]);
					break;
				}
			}
		}
	}
}
