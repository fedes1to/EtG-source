using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MetalGearRat/JumpPound1")]
public class MetalGearRatJumpPound1 : Script
{
	private const int NumWaves = 3;

	private const int NumBullets = 43;

	private const float EllipseA = 6f;

	private const float EllipseB = 2f;

	protected override IEnumerator Top()
	{
		float deltaAngle = 8.372093f;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 43; k++)
				{
					float num = -90f - ((float)k + (float)j * 0.5f) * deltaAngle;
					Vector2 ellipsePointSmooth = BraveMathCollege.GetEllipsePointSmooth(Vector2.zero, 6f, 2f, num);
					Fire(new Offset(ellipsePointSmooth, 0f, string.Empty), new Direction(num), new Speed(12f), new DelayedBullet("default_noramp", j * 4));
				}
			}
			yield return Wait(40);
		}
	}
}
