using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/TankTreader/ScatterShot1")]
public class TankTreaderScatterShot1 : Script
{
	private class ScatterBullet : Bullet
	{
		public ScatterBullet()
			: base("scatterBullet")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(30);
			for (int i = 0; i < 16; i++)
			{
				Fire(new Direction(Random.Range(-35, 35), DirectionType.Relative), new Speed(Random.Range(3, 12)), new LittleScatterBullet());
			}
			Vanish();
		}
	}

	private class LittleScatterBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(12f), 40);
			yield return Wait(300);
			Vanish();
		}
	}

	private const int AirTime = 30;

	private const int NumDeathBullets = 16;

	protected override IEnumerator Top()
	{
		Fire(new Direction(0f, DirectionType.Aim), new Speed(12f), new ScatterBullet());
		return null;
	}
}
