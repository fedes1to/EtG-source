using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Lich/Burst1")]
public class LichBurst1 : Script
{
	public class BurstBullet : Bullet
	{
		public BurstBullet()
			: base("burst")
		{
		}

		protected override IEnumerator Top()
		{
			Projectile.GetComponent<BounceProjModifier>().OnBounce += OnBounce;
			ChangeSpeed(new Speed(16f), 180);
			return null;
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)Projectile)
			{
				Projectile.GetComponent<BounceProjModifier>().OnBounce -= OnBounce;
			}
		}

		private void OnBounce()
		{
			Direction = Projectile.Direction.ToAngle();
		}
	}

	private const int NumBounceBullets = 24;

	private const int NumNormalBullets = 24;

	protected override IEnumerator Top()
	{
		float startAngle = RandomAngle();
		for (int i = 0; i < 24; i++)
		{
			Fire(new Direction(SubdivideCircle(startAngle, 24, i)), new Speed(9f), new BurstBullet());
		}
		for (int j = 0; j < 24; j++)
		{
			Fire(new Direction(SubdivideCircle(startAngle, 24, j, 1f, true)), new Speed(6f));
		}
		return null;
	}
}
