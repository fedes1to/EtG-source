using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("AngryBook/BasicAttack2")]
public class AngryBookBasicAttack2 : Script
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
			ChangeDirection(new Direction(0f, DirectionType.Aim));
			ChangeSpeed(new Speed(12f));
		}
	}

	public int LineBullets = 10;

	public const float Height = 2.5f;

	public const float Width = 1.9f;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		int count = 0;
		float xOffset = 1.9f / (float)(LineBullets - 1);
		float yOffset = 2.5f / (float)(LineBullets - 1);
		for (int i = 0; i < LineBullets; i++)
		{
			Fire(new Offset(-0.95f, -1.25f + yOffset * (float)i, 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int j = 0; j < LineBullets; j++)
		{
			Fire(new Offset(-0.95f + xOffset * (float)j, 1.25f - yOffset * (float)j, 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
		for (int k = 0; k < LineBullets; k++)
		{
			Fire(new Offset(0.95f, -1.25f + yOffset * (float)k, 0f, string.Empty), new DefaultBullet(count++));
			yield return Wait(1);
		}
	}
}
