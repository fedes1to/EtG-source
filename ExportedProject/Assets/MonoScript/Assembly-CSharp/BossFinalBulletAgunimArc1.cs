using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalBullet/AgunimArc1")]
public class BossFinalBulletAgunimArc1 : Script
{
	private const float NumBullets = 19f;

	private const int ArcTime = 15;

	private const float EllipseA = 2.25f;

	private const float EllipseB = 1.5f;

	protected override IEnumerator Top()
	{
		bool facingRight = BraveMathCollege.AbsAngleBetween(base.BulletBank.aiAnimator.FacingDirection, 0f) < 90f;
		float startAngle = ((!facingRight) ? (-170f) : (-10f));
		float deltaAngle = ((!facingRight) ? 160f : (-160f)) / 19f;
		float deltaT = 0.7894737f;
		float t = 0f;
		for (int i = 0; (float)i < 19f; i++)
		{
			float angle = startAngle + (float)i * deltaAngle;
			for (t += deltaT; t > 1f; t -= 1f)
			{
				yield return Wait(1);
			}
			Vector2 offset = BraveMathCollege.GetEllipsePoint(Vector2.zero, 2.25f, 1.5f, angle);
			for (int j = 0; j < 3; j++)
			{
				Fire(new Offset(offset, 0f, string.Empty), new Direction(angle + Random.Range(-25f, 25f)), new Speed(Random.Range(10f, 14f) - (float)j), new DelayedBullet("default", j * 9));
			}
		}
	}
}
