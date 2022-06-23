using System.Collections;
using Brave.BulletScript;

public class ForgeHammerCircle1 : Script
{
	public class DefaultBullet : Bullet
	{
		public int spawnTime;

		public DefaultBullet(int spawnTime)
		{
			this.spawnTime = spawnTime;
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(8f));
			yield return null;
		}
	}

	public int CircleBullets = 12;

	protected override IEnumerator Top()
	{
		int count = 0;
		float degDelta = 360f / (float)CircleBullets;
		for (int i = 0; i < CircleBullets; i++)
		{
			Fire(new Offset(0f, 1f, (float)i * degDelta, string.Empty), new Direction(90f + (float)i * degDelta), new DefaultBullet(count++));
		}
		yield return null;
	}
}
