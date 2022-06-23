using Dungeonator;
using UnityEngine;

public class SecretRoomExitData
{
	public GameObject colliderObject;

	public DungeonData.Direction exitDirection;

	public SecretRoomExitData(GameObject g, DungeonData.Direction d)
	{
		colliderObject = g;
		exitDirection = d;
	}
}
