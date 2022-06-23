using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRobot/M16")]
public class BossFinalRobotM16 : Script
{
	private const float NumBullets = 23f;

	private const int ArcTime = 54;

	private const float ShotVariance = 6f;

	private const float EllipseA = 2.92f;

	private const float EllipseB = 2.03f;

	protected override IEnumerator Top()
	{
		yield return Wait(5);
		float deltaAngle = 15.652174f;
		float deltaT = 2.347826f;
		float t = 0f;
		for (int i = 0; (float)i < 23f; i++)
		{
			float angle = -90f - (float)i * deltaAngle;
			for (t += deltaT; t > 1f; t -= 1f)
			{
				yield return Wait(1);
			}
			Vector2 offset = BraveMathCollege.GetEllipsePoint(Vector2.zero, 2.92f, 2.03f, angle);
			for (int j = -2; j <= 2; j++)
			{
				Fire(new Offset(offset, 0f, string.Empty), new Direction(angle + (float)j * 6f), new Speed(12f), new DelayedBullet((j != -2) ? "default" : "default_vfx", (j + 2) * 10));
			}
		}
	}
}
