using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ShopSubsidiaryZone : MonoBehaviour
{
	public GenericLootTable shopItems;

	public Transform[] spawnPositions;

	public GameObject shopItemShadowPrefab;

	public bool IsShopRoundTable;

	public bool PrecludeAllDiscounts;

	public void HandleSetup(ShopController controller, RoomHandler room, List<GameObject> shopItemObjects, List<ShopItemController> shopItemControllers)
	{
		int count = shopItemObjects.Count;
		for (int i = 0; i < spawnPositions.Length; i++)
		{
			if (IsShopRoundTable && i == 0 && (GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON || GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON))
			{
				shopItemObjects.Add(shopItems.defaultItemDrops.elements[0].gameObject);
				continue;
			}
			GameObject item = shopItems.SelectByWeightWithoutDuplicatesFullPrereqs(shopItemObjects);
			shopItemObjects.Add(item);
		}
		bool flag = false;
		for (int j = 0; j < spawnPositions.Length; j++)
		{
			if (shopItemObjects[count + j] == null)
			{
				continue;
			}
			flag = true;
			Transform transform = spawnPositions[j];
			PickupObject component = shopItemObjects[count + j].GetComponent<PickupObject>();
			if (!(component == null))
			{
				GameObject gameObject = new GameObject("Shop item " + j);
				Transform transform2 = gameObject.transform;
				transform2.parent = transform;
				transform2.localPosition = Vector3.zero;
				EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
				if (component2 != null)
				{
					GameManager.Instance.ExtantShopTrackableGuids.Add(component2.EncounterGuid);
				}
				ShopItemController shopItemController = gameObject.AddComponent<ShopItemController>();
				shopItemController.PrecludeAllDiscounts = PrecludeAllDiscounts;
				if (transform.name.Contains("SIDE") || transform.name.Contains("EAST"))
				{
					shopItemController.itemFacing = DungeonData.Direction.EAST;
				}
				else if (transform.name.Contains("WEST"))
				{
					shopItemController.itemFacing = DungeonData.Direction.WEST;
				}
				else if (transform.name.Contains("NORTH"))
				{
					shopItemController.itemFacing = DungeonData.Direction.NORTH;
				}
				if (!room.IsRegistered(shopItemController))
				{
					room.RegisterInteractable(shopItemController);
				}
				shopItemController.Initialize(component, controller);
				shopItemControllers.Add(shopItemController);
			}
		}
		if (!flag)
		{
			SpeculativeRigidbody[] componentsInChildren = GetComponentsInChildren<SpeculativeRigidbody>();
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				componentsInChildren[k].enabled = false;
			}
			base.gameObject.SetActive(false);
		}
	}

	public void HandleSetup(BaseShopController controller, RoomHandler room, List<GameObject> shopItemObjects, List<ShopItemController> shopItemControllers)
	{
		int count = shopItemObjects.Count;
		for (int i = 0; i < spawnPositions.Length; i++)
		{
			GameObject item = shopItems.SelectByWeightWithoutDuplicatesFullPrereqs(shopItemObjects);
			shopItemObjects.Add(item);
		}
		bool flag = false;
		for (int j = 0; j < spawnPositions.Length; j++)
		{
			if (shopItemObjects[count + j] == null)
			{
				continue;
			}
			flag = true;
			Transform transform = spawnPositions[j];
			PickupObject component = shopItemObjects[count + j].GetComponent<PickupObject>();
			if (!(component == null))
			{
				GameObject gameObject = new GameObject("Shop item " + j);
				Transform transform2 = gameObject.transform;
				transform2.parent = transform;
				transform2.localPosition = Vector3.zero;
				EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
				if (component2 != null)
				{
					GameManager.Instance.ExtantShopTrackableGuids.Add(component2.EncounterGuid);
				}
				ShopItemController shopItemController = gameObject.AddComponent<ShopItemController>();
				shopItemController.PrecludeAllDiscounts = PrecludeAllDiscounts;
				if (transform.name.Contains("SIDE") || transform.name.Contains("EAST"))
				{
					shopItemController.itemFacing = DungeonData.Direction.EAST;
				}
				else if (transform.name.Contains("WEST"))
				{
					shopItemController.itemFacing = DungeonData.Direction.WEST;
				}
				else if (transform.name.Contains("NORTH"))
				{
					shopItemController.itemFacing = DungeonData.Direction.NORTH;
				}
				if (!room.IsRegistered(shopItemController))
				{
					room.RegisterInteractable(shopItemController);
				}
				shopItemController.Initialize(component, controller);
				shopItemControllers.Add(shopItemController);
			}
		}
		if (!flag)
		{
			SpeculativeRigidbody[] componentsInChildren = GetComponentsInChildren<SpeculativeRigidbody>();
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				componentsInChildren[k].enabled = false;
			}
			base.gameObject.SetActive(false);
		}
	}
}
