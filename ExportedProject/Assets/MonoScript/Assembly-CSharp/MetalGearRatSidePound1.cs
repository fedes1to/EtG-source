using System;
using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public abstract class MetalGearRatSidePound1 : Script
{
	public class WaftBullet : Bullet
	{
		public WaftBullet()
			: base("default_noramp")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(), 150);
			yield return Wait(150);
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			float xOffset = UnityEngine.Random.Range(0f, 3f);
			float yOffset = UnityEngine.Random.Range(0f, 1f);
			truePosition -= new Vector2(Mathf.Sin(xOffset * (float)Math.PI / 3f) * 0.65f, Mathf.Sin(yOffset * (float)Math.PI / 1f) * 0.25f);
			for (int i = 0; i < 300; i++)
			{
				truePosition += new Vector2(0f, 1f / 120f);
				float t = (float)i / 60f;
				float waftXOffset = Mathf.Sin((t + xOffset) * (float)Math.PI / 3f) * 0.65f;
				float waftYOffset = Mathf.Sin((t + yOffset) * (float)Math.PI / 1f) * 0.25f;
				base.Position = truePosition + new Vector2(waftXOffset, waftYOffset);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const float NumWaves = 7f;

	private const int NumBullets = 9;

	private const float EllipseA = 2.5f;

	private const float EllipseB = 1f;

	private const float VerticalDriftVelocity = 0.5f;

	private const float WaftXPeriod = 3f;

	private const float WaftXMagnitude = 0.65f;

	private const float WaftYPeriod = 1f;

	private const float WaftYMagnitude = 0.25f;

	private const int WaftLifeTime = 300;

	protected abstract float StartAngle { get; }

	protected abstract float SweepAngle { get; }

	protected override IEnumerator Top()
	{
		for (int i = 0; (float)i < 7f; i++)
		{
			bool isOffset = i % 2 == 1;
			int numBullets = 9 - i;
			for (int j = 0; j < numBullets + (isOffset ? (-1) : 0); j++)
			{
				float num = SubdivideArc(StartAngle, SweepAngle, numBullets, j, isOffset);
				Vector2 ellipsePointSmooth = BraveMathCollege.GetEllipsePointSmooth(Vector2.zero, 2.5f, 1f, num);
				Fire(new Offset(ellipsePointSmooth, 0f, string.Empty), new Direction(num), new Speed(14 - i * 2), new WaftBullet());
			}
			yield return Wait(6);
		}
	}
}
