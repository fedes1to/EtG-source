using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("AngryBook/BasicAttack1")]
public class AngryBookBasicAttack1 : Script
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
			yield return Wait(45 - spawnTime);
			ChangeSpeed(new Speed(8f));
		}
	}

	public int CircleBullets = 20;

	public int LineBullets = 12;

	public const float CircleRadius = 1.3f;

	public const float LineHalfDist = 1.6f;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		int count = 0;
		float degDelta = 360f / (float)CircleBullets;
		for (int i = 0; i < CircleBullets; i++)
		{
			Fire(new Offset(0f, 1.3f, (float)i * degDelta, string.Empty), new Direction(90f + (float)i * degDelta), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int j = 0; j < LineBullets / 2; j++)
		{
			Fire(new Offset(0f, 1.6f - 3.2f / (float)(LineBullets - 1) * (float)j, 0f, string.Empty), new Direction(90f), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int k = 0; k < LineBullets / 2; k++)
		{
			Fire(new Offset(0f, 1.6f - 3.2f / (float)(LineBullets - 1) * (float)(k + LineBullets / 2), 0f, string.Empty), new Direction(-90f), new DefaultBullet(count++));
			yield return Wait(1);
		}
	}
}
