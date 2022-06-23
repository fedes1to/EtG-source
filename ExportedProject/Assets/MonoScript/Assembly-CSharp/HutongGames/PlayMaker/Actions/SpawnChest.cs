using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	[Tooltip("Spawns a chest in the NPC's current room.")]
	public class SpawnChest : FsmStateAction
	{
		public enum Type
		{
			RoomReward,
			Custom
		}

		public enum SpawnLocation
		{
			BestRoomLocation,
			OffsetFromTalkDoer
		}

		[Tooltip("Type of chest to spawn.")]
		public Type type;

		[Tooltip("Specific chest to spawn.")]
		public GameObject CustomChest;

		[Tooltip("Where to spawn the item at.")]
		public SpawnLocation spawnLocation;

		[Tooltip("Offset from the TalkDoer to spawn the item at.")]
		public Vector2 spawnOffset;

		public override void Reset()
		{
			type = Type.RoomReward;
			CustomChest = null;
		}

		public override void OnEnter()
		{
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			WeightedGameObjectCollection weightedGameObjectCollection = null;
			if (type == Type.RoomReward)
			{
				weightedGameObjectCollection = null;
			}
			else if (type == Type.Custom)
			{
				WeightedGameObject weightedGameObject = new WeightedGameObject();
				weightedGameObject.SetGameObject(CustomChest);
				weightedGameObjectCollection = new WeightedGameObjectCollection();
				weightedGameObjectCollection.Add(weightedGameObject);
			}
			RoomHandler parentRoom = component.ParentRoom;
			if (spawnLocation == SpawnLocation.BestRoomLocation)
			{
				parentRoom.SpawnRoomRewardChest(weightedGameObjectCollection, component.ParentRoom.GetBestRewardLocation(new IntVector2(2, 1)));
			}
			else if (spawnLocation == SpawnLocation.OffsetFromTalkDoer)
			{
				Vector2 idealPoint = ((!(component.specRigidbody != null)) ? component.sprite.WorldCenter : component.specRigidbody.UnitCenter);
				idealPoint += spawnOffset;
				parentRoom.SpawnRoomRewardChest(weightedGameObjectCollection, component.ParentRoom.GetBestRewardLocation(new IntVector2(2, 1), idealPoint));
			}
			Finish();
		}
	}
}
