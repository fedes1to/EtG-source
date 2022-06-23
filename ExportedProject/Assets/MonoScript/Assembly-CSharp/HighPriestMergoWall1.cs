using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/HighPriest/MergoWall1")]
public class HighPriestMergoWall1 : Script
{
	public class BigBullet : Bullet
	{
		public BigBullet()
			: base("mergoWall")
		{
		}

		protected override IEnumerator Top()
		{
			Projectile.IgnoreTileCollisionsFor(1.5f);
			ChangeDirection(new Direction(30f, DirectionType.Relative), 30);
			yield return Wait(30);
			ChangeDirection(new Direction(-60f, DirectionType.Relative), 60);
			yield return Wait(30);
			ChangeSpeed(new Speed(8f), 4);
			yield return Wait(60);
			for (int i = 0; i < 10; i++)
			{
				ChangeDirection(new Direction(60f, DirectionType.Relative), 60);
				yield return Wait(60);
				ChangeDirection(new Direction(-60f, DirectionType.Relative), 60);
				yield return Wait(60);
			}
			ChangeDirection(new Direction(30f, DirectionType.Relative), 30);
			yield return Wait(30);
			Vanish();
		}
	}

	private const int NumBullets = 20;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 20; i++)
		{
			Fire(new Direction(0f, DirectionType.Relative), new Speed(4f), new BigBullet());
		}
		return null;
	}
}
