using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Bunker/BigBulletPop1")]
public class BunkerBigBulletPop1 : Script
{
	public class BigBullet : Bullet
	{
		public BigBullet()
			: base("default_black")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(40);
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float num = RandomAngle();
				for (int i = 0; i < 8; i++)
				{
					Fire(new Direction(num + (float)(i * 45)), new Speed(9f), new Bullet("default3"));
				}
			}
		}
	}

	protected override IEnumerator Top()
	{
		Fire(new Offset("left shooter"), new Direction(0f, DirectionType.Relative), new Speed(9f), new BigBullet());
		return null;
	}
}
