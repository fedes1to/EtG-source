public class BlankModificationItem : PassiveItem
{
	public float BlankForceMultiplier = 1f;

	public float BlankStunTime = 1f;

	public bool MakeBlankDealDamage;

	public float BlankDamageRadius = 10f;

	public float BlankDamage = 20f;

	public float BlankFireChance;

	public GameActorFireEffect BlankFireEffect;

	public float BlankPoisonChance;

	public GameActorHealthEffect BlankPoisonEffect;

	public float BlankFreezeChance;

	public GameActorFreezeEffect BlankFreezeEffect;

	public float RegainAmmoFraction;

	public bool BlankReflectsEnemyBullets;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			EngageEffect(player);
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		DisengageEffect(player);
		debrisObject.GetComponent<BlankModificationItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp)
		{
			DisengageEffect(m_owner);
		}
		base.OnDestroy();
	}

	protected void EngageEffect(PlayerController user)
	{
	}

	protected void DisengageEffect(PlayerController user)
	{
	}
}
