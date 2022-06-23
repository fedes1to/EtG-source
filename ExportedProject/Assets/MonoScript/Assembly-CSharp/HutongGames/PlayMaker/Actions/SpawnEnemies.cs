namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Spawns enemies in the NPC's current room.")]
	public class SpawnEnemies : FsmStateAction
	{
		public enum Type
		{
			Reinforcement
		}

		[Tooltip("Type of enemy spawn.")]
		public Type type;

		public RoomEventTriggerCondition roomEventTrigger;

		public bool InstantReinforcement;

		public override void Reset()
		{
			type = Type.Reinforcement;
			roomEventTrigger = RoomEventTriggerCondition.NPC_TRIGGER_A;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			if (type == Type.Reinforcement)
			{
				component.ParentRoom.TriggerReinforcementLayersOnEvent(roomEventTrigger, InstantReinforcement);
				component.ParentRoom.SealRoom();
			}
			Finish();
		}
	}
}
