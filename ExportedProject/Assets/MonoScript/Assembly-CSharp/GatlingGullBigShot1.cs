using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/GatlingGull/BigShot1")]
public class GatlingGullBigShot1 : Script
{
	private class BigBullet : Bullet
	{
		public BigBullet()
			: base("bigBullet")
		{
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float num = RandomAngle();
				float num2 = 11.25f;
				for (int i = 0; i < 32; i++)
				{
					Fire(new Direction(num + num2 * (float)i), new Speed(10f));
				}
			}
		}
	}

	private const int NumDeathBullets = 32;

	protected override IEnumerator Top()
	{
		Fire(new Direction(0f, DirectionType.Aim), new Speed(10f), new BigBullet());
		return null;
	}
}
