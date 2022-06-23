using Dungeonator;
using UnityEngine;

public class DungeonPlaceableContainer : MonoBehaviour
{
	public DungeonPlaceable placeable;

	private void Awake()
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(intVector);
		GameObject gameObject = placeable.InstantiateObject(absoluteRoomFromPosition, intVector - absoluteRoomFromPosition.area.basePosition);
		if (gameObject != null)
		{
			IPlayerInteractable[] interfacesInChildren = gameObject.GetInterfacesInChildren<IPlayerInteractable>();
			for (int i = 0; i < interfacesInChildren.Length; i++)
			{
				absoluteRoomFromPosition.RegisterInteractable(interfacesInChildren[i]);
			}
			SurfaceDecorator component = gameObject.GetComponent<SurfaceDecorator>();
			if (component != null)
			{
				component.Decorate(absoluteRoomFromPosition);
			}
		}
	}
}
