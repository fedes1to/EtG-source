using Dungeonator;
using UnityEngine;

public static class DungeonPlaceableUtility
{
	public static GameObject InstantiateDungeonPlaceable(GameObject objectToInstantiate, RoomHandler targetRoom, IntVector2 location, bool deferConfiguration, AIActor.AwakenAnimationType awakenAnimType = AIActor.AwakenAnimationType.Default, bool autoEngage = false)
	{
		if (objectToInstantiate != null)
		{
			Vector3 position = location.ToVector3(0f) + targetRoom.area.basePosition.ToVector3();
			position.z = position.y + position.z;
			AIActor component = objectToInstantiate.GetComponent<AIActor>();
			if (component is AIActorDummy)
			{
				objectToInstantiate = (component as AIActorDummy).realPrefab;
				component = objectToInstantiate.GetComponent<AIActor>();
			}
			SpeculativeRigidbody component2 = objectToInstantiate.GetComponent<SpeculativeRigidbody>();
			if ((bool)component && (bool)component2)
			{
				PixelCollider pixelCollider = component2.GetPixelCollider(ColliderType.Ground);
				if (pixelCollider.ColliderGenerationMode != 0)
				{
					Debug.LogErrorFormat("Trying to spawn an AIActor who doesn't have a manual ground collider... do we still do this? Name: {0}", objectToInstantiate.name);
				}
				Vector2 vector = PhysicsEngine.PixelToUnit(new IntVector2(pixelCollider.ManualOffsetX, pixelCollider.ManualOffsetY));
				Vector2 vector2 = PhysicsEngine.PixelToUnit(new IntVector2(pixelCollider.ManualWidth, pixelCollider.ManualHeight));
				Vector2 vector3 = new Vector2((new Vector2(Mathf.CeilToInt(vector2.x), Mathf.CeilToInt(vector2.y)).x - vector2.x) / 2f, 0f).Quantize(0.0625f);
				position -= (Vector3)(vector - vector3);
			}
			if ((bool)component)
			{
				component.AwakenAnimType = awakenAnimType;
			}
			GameObject gameObject = Object.Instantiate(objectToInstantiate, position, Quaternion.identity);
			if (!deferConfiguration)
			{
				Component[] componentsInChildren = gameObject.GetComponentsInChildren(typeof(IPlaceConfigurable));
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					IPlaceConfigurable placeConfigurable = componentsInChildren[i] as IPlaceConfigurable;
					if (placeConfigurable != null)
					{
						placeConfigurable.ConfigureOnPlacement(targetRoom);
					}
				}
			}
			ObjectVisibilityManager component3 = gameObject.GetComponent<ObjectVisibilityManager>();
			if (component3 != null)
			{
				component3.Initialize(targetRoom, autoEngage);
			}
			MinorBreakable componentInChildren = gameObject.GetComponentInChildren<MinorBreakable>();
			if (componentInChildren != null)
			{
				IntVector2 key = location + targetRoom.area.basePosition;
				CellData cellData = GameManager.Instance.Dungeon.data[key];
				if (cellData != null)
				{
					cellData.cellVisualData.containsObjectSpaceStamp = true;
				}
			}
			PlayerItem component4 = gameObject.GetComponent<PlayerItem>();
			if (component4 != null)
			{
				component4.ForceAsExtant = true;
			}
			return gameObject;
		}
		return null;
	}

	public static GameObject InstantiateDungeonPlaceableOnlyActors(GameObject objectToInstantiate, RoomHandler targetRoom, IntVector2 location, bool deferConfiguration)
	{
		bool flag = objectToInstantiate.GetComponent<AIActor>();
		bool flag2 = GameManager.Instance.InTutorial && (bool)objectToInstantiate.GetComponent<TalkDoerLite>();
		bool flag3 = objectToInstantiate.GetComponent<GenericIntroDoer>();
		if (!flag && !flag2 && !flag3)
		{
			return null;
		}
		GameObject gameObject = InstantiateDungeonPlaceable(objectToInstantiate, targetRoom, location, deferConfiguration);
		AIActor component = gameObject.GetComponent<AIActor>();
		if ((bool)component)
		{
			component.CanDropCurrency = false;
			component.CanDropItems = false;
		}
		return gameObject;
	}
}
