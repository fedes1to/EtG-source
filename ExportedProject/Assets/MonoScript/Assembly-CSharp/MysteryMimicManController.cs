using System.Collections;
using Dungeonator;
using UnityEngine;

public class MysteryMimicManController : MonoBehaviour
{
	private IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (!PassiveItem.IsFlagSetAtAll(typeof(MimicToothNecklaceItem)))
		{
			RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
			absoluteRoom.DeregisterInteractable(GetComponent<TalkDoerLite>());
			base.gameObject.SetActive(false);
			Object.Destroy(base.gameObject);
		}
	}
}
