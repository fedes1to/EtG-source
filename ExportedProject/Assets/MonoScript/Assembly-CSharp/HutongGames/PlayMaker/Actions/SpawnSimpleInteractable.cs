using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Events)]
	public class SpawnSimpleInteractable : FsmStateAction
	{
		public GameObject ThingToSpawn;

		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			GameObject gObj = Object.Instantiate(ThingToSpawn, base.Owner.transform.position, Quaternion.identity);
			IPlayerInteractable[] interfaces = gObj.GetInterfaces<IPlayerInteractable>();
			IPlaceConfigurable[] interfaces2 = gObj.GetInterfaces<IPlaceConfigurable>();
			RoomHandler absoluteRoom = base.Owner.transform.position.GetAbsoluteRoom();
			for (int i = 0; i < interfaces.Length; i++)
			{
				absoluteRoom.RegisterInteractable(interfaces[i]);
			}
			for (int j = 0; j < interfaces2.Length; j++)
			{
				interfaces2[j].ConfigureOnPlacement(absoluteRoom);
			}
			Finish();
		}
	}
}
