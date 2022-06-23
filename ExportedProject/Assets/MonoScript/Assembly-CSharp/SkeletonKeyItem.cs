public class SkeletonKeyItem : PassiveItem
{
	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				GameManager.Instance.AllPlayers[i].carriedConsumables.InfiniteKeys = true;
			}
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<SkeletonKeyItem>().m_pickedUpThisRun = true;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].carriedConsumables.InfiniteKeys = false;
		}
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].carriedConsumables.InfiniteKeys = false;
		}
		base.OnDestroy();
	}
}
