using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/ResourcefulRat/SpinFire1")]
public class ResourcefulRatSpinFire1 : Script
{
	private const float NumBullets = 23f;

	private const int ArcTime = 70;

	private const float SpreadAngle = 6f;

	private const float BulletSpeed = 16f;

	private const float EllipseA = 1.39f;

	private const float EllipseB = 0.92f;

	protected override IEnumerator Top()
	{
		yield return Wait(5);
		float deltaAngle = 15.652174f;
		float deltaT = 3.04347825f;
		float t = 0f;
		for (int i = 0; (float)i < 23f; i++)
		{
			float angle = -90f - (float)i * deltaAngle;
			for (t += deltaT; t > 1f; t -= 1f)
			{
				yield return Wait(1);
			}
			Vector2 offset = BraveMathCollege.GetEllipsePoint(Vector2.zero, 1.39f, 0.92f, angle);
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle - 6f), new Speed(16f), new Bullet("cheese", true));
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle), new Speed(16f), new Bullet("cheese", true));
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle + 6f), new Speed(16f), new Bullet("cheese", true));
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle - 6f), new Speed(), new SpeedChangingBullet("cheese", 16f, 50, -1, true));
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle), new Speed(), new SpeedChangingBullet("cheese", 16f, 50, -1, true));
			Fire(new Offset(offset, 0f, string.Empty), new Direction(angle + 6f), new Speed(), new SpeedChangingBullet("cheese", 16f, 50, -1, true));
		}
	}
}
