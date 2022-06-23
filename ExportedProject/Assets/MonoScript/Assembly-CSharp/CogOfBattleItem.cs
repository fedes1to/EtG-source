public class CogOfBattleItem : PassiveItem
{
	public static float ACTIVE_RELOAD_DAMAGE_MULTIPLIER = 1.25f;

	public float DamageMultiplier = 1.25f;

	private PlayerController m_localOwner;

	private void Awake()
	{
		ACTIVE_RELOAD_DAMAGE_MULTIPLIER = DamageMultiplier;
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_localOwner = player;
			if (player.IsPrimaryPlayer)
			{
				Gun.ActiveReloadActivated = true;
			}
			else
			{
				Gun.ActiveReloadActivatedPlayerTwo = true;
			}
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_localOwner = null;
		if (player.IsPrimaryPlayer)
		{
			Gun.ActiveReloadActivated = false;
		}
		else
		{
			Gun.ActiveReloadActivatedPlayerTwo = false;
		}
		debrisObject.GetComponent<CogOfBattleItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_localOwner != null)
		{
			if (m_localOwner.IsPrimaryPlayer)
			{
				Gun.ActiveReloadActivated = false;
			}
			else
			{
				Gun.ActiveReloadActivatedPlayerTwo = false;
			}
		}
		base.OnDestroy();
	}
}
