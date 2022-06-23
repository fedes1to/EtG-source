using System;

public class RingOfPitFriendship : PassiveItem
{
	private string boolKey = string.Empty;

	private PlayerController m_currentOwner;

	private void Awake()
	{
		boolKey = "ringPitFriend" + Guid.NewGuid().ToString();
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			m_currentOwner = player;
			player.ImmuneToPits.SetOverride(boolKey, true);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<RingOfPitFriendship>().m_pickedUpThisRun = true;
		player.ImmuneToPits.SetOverride(boolKey, false);
		m_currentOwner = null;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_currentOwner != null)
		{
			m_currentOwner.ImmuneToPits.SetOverride(boolKey, false);
		}
		base.OnDestroy();
	}
}
