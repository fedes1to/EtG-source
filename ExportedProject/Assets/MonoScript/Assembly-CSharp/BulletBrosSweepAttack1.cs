using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BulletBros/SweepAttack1")]
public class BulletBrosSweepAttack1 : Script
{
	private const int NumBullets = 15;

	private const float ArcDegrees = 60f;

	protected override IEnumerator Top()
	{
		float sign = 1f;
		if (BulletManager.PlayerVelocity() != Vector2.zero)
		{
			float a = base.AimDirection + 90f;
			float b = BulletManager.PlayerVelocity().ToAngle();
			if (BraveMathCollege.AbsAngleBetween(a, b) > 90f)
			{
				sign = -1f;
			}
		}
		for (int i = 0; i < 15; i++)
		{
			Fire(new Direction(SubdivideArc((0f - sign) * 60f / 2f, sign * 60f, 15, i), DirectionType.Aim), new Speed(10f));
			yield return Wait(6);
		}
	}
}
