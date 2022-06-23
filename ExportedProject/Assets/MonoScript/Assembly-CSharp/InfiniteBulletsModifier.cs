public class InfiniteBulletsModifier : BraveBehaviour
{
	public void Start()
	{
		Projectile projectile = base.projectile;
		projectile.OnDestruction += HandleDestruction;
	}

	private void HandleDestruction(Projectile p)
	{
		if (!p.HasImpactedEnemy && (bool)p.PossibleSourceGun && p.PossibleSourceGun.gameObject.activeSelf)
		{
			p.PossibleSourceGun.GainAmmo(1);
			p.PossibleSourceGun.ForceFireProjectile(p);
		}
	}
}
