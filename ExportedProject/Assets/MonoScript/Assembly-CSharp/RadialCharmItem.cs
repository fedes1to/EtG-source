public class RadialCharmItem : AffectEnemiesInRadiusItem
{
	public bool DoCharm = true;

	[ShowInInspectorIf("DoCharm", false)]
	public GameActorCharmEffect CharmEffect;

	public bool HasProjectileSynergy;

	[LongNumericEnum]
	public CustomSynergyType ProjectileSynergyRequired;

	public ProjectileVolleyData SynergyVolley;

	protected override void AffectEnemy(AIActor target)
	{
		if (DoCharm)
		{
			target.ApplyEffect(CharmEffect);
			if (HasProjectileSynergy && (bool)LastOwner && LastOwner.HasActiveBonusSynergy(ProjectileSynergyRequired))
			{
				VolleyUtility.FireVolley(SynergyVolley, LastOwner.CenterPosition, target.CenterPosition - LastOwner.CenterPosition, LastOwner);
			}
		}
	}

	protected override void AffectShop(BaseShopController target)
	{
		if (DoCharm)
		{
			FakeGameActorEffectHandler componentInChildren = target.GetComponentInChildren<FakeGameActorEffectHandler>();
			componentInChildren.ApplyEffect(CharmEffect);
			target.SetCapableOfBeingStolenFrom(true, "RadialCharmItem", CharmEffect.duration);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
