public class DamageTypeModifierItem : PassiveItem
{
	public DamageTypeModifier[] modifiers;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			for (int i = 0; i < modifiers.Length; i++)
			{
				player.healthHaver.damageTypeModifiers.Add(modifiers[i]);
			}
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		for (int i = 0; i < modifiers.Length; i++)
		{
			player.healthHaver.damageTypeModifiers.Remove(modifiers[i]);
		}
		m_player = null;
		debrisObject.GetComponent<DamageTypeModifierItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_player != null)
		{
			for (int i = 0; i < modifiers.Length; i++)
			{
				m_player.healthHaver.damageTypeModifiers.Remove(modifiers[i]);
			}
		}
		base.OnDestroy();
	}
}
