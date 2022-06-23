using UnityEngine;

public class CartographersRingItem : PassiveItem
{
	public float revealChanceOnLoad = 0.5f;

	public bool revealSecretRooms;

	public bool executeOnPickup;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			bool flag = false;
			if (executeOnPickup && !m_pickedUpThisRun)
			{
				flag = true;
			}
			base.Pickup(player);
			if (flag)
			{
				PossiblyRevealMap();
			}
			GameManager.Instance.OnNewLevelFullyLoaded += PossiblyRevealMap;
		}
	}

	public void PossiblyRevealMap()
	{
		if (Random.value < revealChanceOnLoad && Minimap.Instance != null)
		{
			Minimap.Instance.RevealAllRooms(revealSecretRooms);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<CartographersRingItem>().m_pickedUpThisRun = true;
		GameManager.Instance.OnNewLevelFullyLoaded -= PossiblyRevealMap;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp && GameManager.HasInstance)
		{
			GameManager.Instance.OnNewLevelFullyLoaded -= PossiblyRevealMap;
		}
		base.OnDestroy();
	}
}
