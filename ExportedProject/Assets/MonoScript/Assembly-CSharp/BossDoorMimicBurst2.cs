using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossDoorMimic/Burst2")]
public class BossDoorMimicBurst2 : Script
{
	public class BurstBullet : Bullet
	{
		private Vector2 m_addtionalVelocity;

		public BurstBullet(Vector2 additionalVelocity)
			: base("burst")
		{
			m_addtionalVelocity = additionalVelocity;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			for (int i = 0; i < 300; i++)
			{
				UpdateVelocity();
				Velocity += m_addtionalVelocity * Mathf.Min(9f, (float)i / 30f);
				UpdatePosition();
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBursts = 5;

	private const int NumBullets = 36;

	protected override IEnumerator Top()
	{
		float floatDirection = RandomAngle();
		for (int i = 0; i < 5; i++)
		{
			float startDirection = RandomAngle();
			Vector2 floatVelocity = BraveMathCollege.DegreesToVector(floatDirection, 4f);
			for (int j = 0; j < 36; j++)
			{
				Fire(new Direction(SubdivideCircle(startDirection, 36, j)), new Speed(8f), new BurstBullet(floatVelocity));
			}
			floatDirection = floatDirection + 180f + Random.Range(-60f, 60f);
			yield return Wait(90);
		}
		yield return Wait(75);
	}
}
