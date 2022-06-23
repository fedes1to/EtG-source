public class RegenerationPassiveItem : PassiveItem
{
	public float RequiredDamage = 1000f;

	protected PlayerController m_player;

	protected float m_damageDealtCounter;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			m_player.OnDealtDamage += PlayerDealtDamage;
			base.Pickup(player);
		}
	}

	private void PlayerDealtDamage(PlayerController p, float damage)
	{
		if (p.healthHaver.GetCurrentHealthPercentage() >= 1f)
		{
			m_damageDealtCounter = 0f;
			return;
		}
		m_damageDealtCounter += damage;
		if (m_damageDealtCounter >= RequiredDamage)
		{
			p.healthHaver.ApplyHealing(0.5f);
			m_damageDealtCounter = 0f;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<RegenerationPassiveItem>().m_pickedUpThisRun = true;
		m_player.OnDealtDamage -= PlayerDealtDamage;
		m_player = null;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.OnDealtDamage -= PlayerDealtDamage;
		}
	}
}
