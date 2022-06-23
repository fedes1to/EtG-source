using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Triggers phantom door events.")]
	[ActionCategory(".NPCs")]
	public class PhantomDoor : FsmStateAction
	{
		[Tooltip("Seals the room the Owner is in.")]
		public FsmBool seal;

		public override void Reset()
		{
			seal = false;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			component.specRigidbody.Initialize();
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(component.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
			if (seal.Value)
			{
				DungeonDoorSubsidiaryBlocker closestToPosition = BraveUtility.GetClosestToPosition(new List<DungeonDoorSubsidiaryBlocker>(Object.FindObjectsOfType<DungeonDoorSubsidiaryBlocker>()), roomFromPosition.area.Center);
				closestToPosition.Seal();
			}
			Finish();
		}
	}
}
