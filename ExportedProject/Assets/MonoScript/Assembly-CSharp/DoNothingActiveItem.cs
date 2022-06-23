public class DoNothingActiveItem : PlayerItem
{
	public override bool CanBeUsed(PlayerController user)
	{
		return base.CanBeUsed(user);
	}

	protected override void DoEffect(PlayerController user)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
