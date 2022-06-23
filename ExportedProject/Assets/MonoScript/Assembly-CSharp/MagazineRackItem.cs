using UnityEngine;

public class MagazineRackItem : PlayerItem
{
	public GameObject MagazineRackPrefab;

	private GameObject m_instanceRack;

	public override bool CanBeUsed(PlayerController user)
	{
		if ((bool)m_instanceRack)
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	protected override void DoEffect(PlayerController user)
	{
		m_instanceRack = Object.Instantiate(MagazineRackPrefab, user.CenterPosition.ToVector3ZisY(), Quaternion.identity, null);
	}
}
