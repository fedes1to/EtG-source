using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/FlameBreath2")]
public class DraGunFlameBreath2 : Script
{
	public class FlameBullet : Bullet
	{
		public FlameBullet()
			: base("Breath")
		{
		}

		protected override IEnumerator Top()
		{
			while (base.Position.y > StopYHeight)
			{
				yield return Wait(1);
			}
			ChangeSpeed(new Speed(0.33f), 12);
			yield return Wait(60);
			Vanish();
		}
	}

	private const int NumBullets = 120;

	private const int NumWaveBullets = 12;

	private const float Spread = 30f;

	private const int PocketResetTime = 30;

	private const float PocketWidth = 5f;

	protected static float StopYHeight;

	protected override IEnumerator Top()
	{
		StopYHeight = base.BulletBank.aiActor.ParentRoom.area.UnitBottomLeft.y + 21f;
		int pocketResetTimer = 0;
		float pocketAngle = 0f;
		float pocketSign = BraveUtility.RandomSign();
		for (int i = 0; i < 120; i++)
		{
			if (i % 40 == 27)
			{
				for (int j = 0; j < 12; j++)
				{
					Fire(new Direction(SubdivideArc(-30f, 60f, 12, j), DirectionType.Aim), new Speed(14f), new FlameBullet());
				}
			}
			float direction = Random.Range(-30f, 30f);
			if (pocketResetTimer == 0)
			{
				pocketAngle = pocketSign * Random.Range(0f, 15f);
				pocketSign *= -1f;
				pocketResetTimer = 30;
			}
			pocketResetTimer--;
			if (direction >= pocketAngle - 5f && direction <= pocketAngle)
			{
				direction -= 5f;
			}
			else if (direction <= pocketAngle + 5f && direction >= pocketAngle)
			{
				direction += 5f;
			}
			Fire(new Direction(direction, DirectionType.Aim), new Speed(14f), new FlameBullet());
			yield return Wait(2);
		}
	}
}
