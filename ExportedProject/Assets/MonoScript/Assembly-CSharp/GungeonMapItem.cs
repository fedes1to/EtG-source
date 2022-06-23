using Dungeonator;
using UnityEngine;

public class GungeonMapItem : PassiveItem
{
	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			if (RoomHandler.unassignedInteractableObjects.Contains(this))
			{
				RoomHandler.unassignedInteractableObjects.Remove(this);
			}
			m_pickedUp = true;
			if (!m_pickedUpThisRun)
			{
				HandleEncounterable(player);
			}
			GameObject original = (GameObject)BraveResources.Load("Global VFX/VFX_Item_Pickup", typeof(GameObject));
			GameObject gameObject = Object.Instantiate(original);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.PlaceAtPositionByAnchor(base.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
			component.UpdateZDepth();
			m_pickedUpThisRun = true;
			player.EverHadMap = true;
			player.stats.RecalculateStats(player);
			if (Minimap.Instance != null)
			{
				Minimap.Instance.RevealAllRooms(true);
			}
			Object.Destroy(base.gameObject);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		Debug.LogError("IT SHOULD BE IMPOSSIBLE TO DROP MAPS.");
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<GungeonMapItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
