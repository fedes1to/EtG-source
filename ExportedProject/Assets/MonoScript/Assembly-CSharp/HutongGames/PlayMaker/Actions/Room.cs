using Dungeonator;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Respondes to chest events.")]
	[ActionCategory(".NPCs")]
	public class Room : FsmStateAction
	{
		[Tooltip("Seals the room the Owner is in.")]
		public FsmBool seal;

		[Tooltip("Unseals the room the Owner is in.")]
		public FsmBool unseal;

		[Tooltip("Ignores SealPrior in Tutorial.")]
		public FsmBool unsealAllForceTutorial;

		public override void Reset()
		{
			seal = false;
			unseal = false;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			component.specRigidbody.Initialize();
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(component.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
			if (seal.Value)
			{
				roomFromPosition.npcSealState = RoomHandler.NPCSealState.SealAll;
				if (GameManager.Instance.InTutorial && component.name.Contains("NPC_Tutorial_Knight_001_intro"))
				{
					roomFromPosition.npcSealState = RoomHandler.NPCSealState.SealNext;
				}
				roomFromPosition.SealRoom();
			}
			else if (unseal.Value)
			{
				roomFromPosition.npcSealState = RoomHandler.NPCSealState.SealNone;
				if (GameManager.Instance.InTutorial)
				{
					if (component.name.Contains("NPC_Tutorial_Knight_001_intro"))
					{
						roomFromPosition.npcSealState = RoomHandler.NPCSealState.SealNone;
					}
					else
					{
						roomFromPosition.npcSealState = RoomHandler.NPCSealState.SealPrior;
					}
				}
				if (unsealAllForceTutorial.Value)
				{
					roomFromPosition.npcSealState = RoomHandler.NPCSealState.SealNone;
				}
				roomFromPosition.UnsealRoom();
			}
			Finish();
		}
	}
}
