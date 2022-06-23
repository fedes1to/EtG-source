using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DemonWall/LeapLine1")]
public class DemonWallLeapLine1 : Script
{
	private class WaveBullet : Bullet
	{
		private const float SinPeriod = 0.75f;

		private const float SinMagnitude = 1.5f;

		public WaveBullet()
			: base("leap")
		{
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			for (int i = 0; i < 600; i++)
			{
				UpdateVelocity();
				truePosition += Velocity / 60f;
				base.Position = truePosition + new Vector2(0f, Mathf.Sin((float)base.Tick / 60f / 0.75f * (float)Math.PI) * 1.5f);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 24;

	protected override IEnumerator Top()
	{
		float num = 1f;
		for (int i = 0; i < 24; i++)
		{
			Fire(new Offset(-11.5f + (float)i * num, 0f, 0f, string.Empty), new Direction(-90f), new Speed(5f), new WaveBullet());
		}
		return null;
	}
}
