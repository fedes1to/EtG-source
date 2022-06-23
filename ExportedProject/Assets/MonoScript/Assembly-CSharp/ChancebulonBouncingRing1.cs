using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Chancebulon/BouncingRing1")]
public class ChancebulonBouncingRing1 : Script
{
	public class BouncingRingBullet : Bullet
	{
		private Vector2 m_desiredOffset;

		public BouncingRingBullet(string name, Vector2 desiredOffset)
			: base(name)
		{
			m_desiredOffset = desiredOffset;
		}

		protected override IEnumerator Top()
		{
			Vector2 centerPoint = base.Position;
			Vector2 lowestOffset = BraveMathCollege.DegreesToVector(-90f, 1.5f);
			Vector2 currentOffset = Vector2.zero;
			float squishFactor = 1f;
			float verticalOffset2 = 0f;
			int unsquishIndex = 100;
			base.ManualControl = true;
			for (int i = 0; i < 300; i++)
			{
				if (i < 30)
				{
					currentOffset = Vector2.Lerp(Vector2.zero, m_desiredOffset, (float)i / 30f);
				}
				verticalOffset2 = (Mathf.Abs(Mathf.Cos((float)i / 90f * (float)Math.PI)) - 1f) * 2.5f;
				if (unsquishIndex <= 10)
				{
					squishFactor = Mathf.Abs(Mathf.SmoothStep(0.6f, 1f, (float)unsquishIndex / 10f));
					unsquishIndex++;
				}
				Vector2 relativeOffset2 = currentOffset - lowestOffset;
				Vector2 squishedOffset2 = lowestOffset + relativeOffset2.Scale(1f, squishFactor);
				UpdateVelocity();
				centerPoint += Velocity / 60f;
				base.Position = centerPoint + squishedOffset2 + new Vector2(0f, verticalOffset2);
				if (i % 90 == 45)
				{
					for (int j = 1; j <= 10; j++)
					{
						squishFactor = Mathf.Abs(Mathf.SmoothStep(1f, 0.5f, (float)j / 10f));
						relativeOffset2 = currentOffset - lowestOffset;
						squishedOffset2 = lowestOffset + relativeOffset2.Scale(1f, squishFactor);
						centerPoint += 0.333f * Velocity / 60f;
						base.Position = centerPoint + squishedOffset2 + new Vector2(0f, verticalOffset2);
						yield return Wait(1);
					}
					unsquishIndex = 1;
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 18;

	protected override IEnumerator Top()
	{
		float direction = GetAimDirection((UnityEngine.Random.value < 0.4f) ? 1 : 0, 8f) + UnityEngine.Random.Range(-10f, 10f);
		for (int i = 0; i < 18; i++)
		{
			float angle = (float)i * 20f;
			Vector2 desiredOffset = BraveMathCollege.DegreesToVector(angle, 1.8f);
			Fire(new Direction(direction), new Speed(8f), new BouncingRingBullet("bouncingRing", desiredOffset));
		}
		Fire(new Direction(direction), new Speed(8f), new BouncingRingBullet("bouncingRing", new Vector2(-0.7f, 0.7f)));
		Fire(new Direction(direction), new Speed(8f), new BouncingRingBullet("bouncingMouth", new Vector2(0f, 0.4f)));
		Fire(new Direction(direction), new Speed(8f), new BouncingRingBullet("bouncingRing", new Vector2(0.7f, 0.7f)));
		return null;
	}
}
