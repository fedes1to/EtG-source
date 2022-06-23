using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MineFlayer/Shoot1")]
public class MineFlayerShoot1 : Script
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
		float floatSign = BraveUtility.RandomSign();
		for (int i = 0; i < 5; i++)
		{
			float floatDirection = 90f + floatSign * 90f + Random.Range(25f, -25f);
			Vector2 floatVelocity = BraveMathCollege.DegreesToVector(floatDirection, 4f);
			float startDirection = RandomAngle();
			for (int j = 0; j < 36; j++)
			{
				Fire(new Direction(SubdivideCircle(startDirection, 36, j)), new Speed(8f), new BurstBullet(floatVelocity));
			}
			yield return Wait(30);
			floatSign *= -1f;
		}
	}
}
