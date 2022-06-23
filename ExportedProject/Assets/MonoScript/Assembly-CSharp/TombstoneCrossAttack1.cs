using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class TombstoneCrossAttack1 : Script
{
	public class CrossBullet : Bullet
	{
		private Vector2 m_offset;

		private int m_setupDelay;

		private int m_setupTime;

		public CrossBullet(Vector2 offset, int setupDelay, int setupTime)
		{
			m_offset = offset;
			m_setupDelay = setupDelay;
			m_setupTime = setupTime;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			m_offset = m_offset.Rotate(Direction);
			for (int i = 0; i < 360; i++)
			{
				if (i > m_setupDelay && i < m_setupDelay + m_setupTime)
				{
					base.Position += m_offset / m_setupTime;
				}
				base.Position += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int BulletSpeed = 10;

	private const float GapDist = 0.7f;

	protected override IEnumerator Top()
	{
		float aimDirection = GetAimDirection((Random.value < 0.25f) ? 1 : 0, 10f);
		Fire(new Direction(aimDirection), new Speed(10f), new CrossBullet(new Vector2(0.7f, 0f), 0, 20));
		Fire(new Direction(aimDirection), new Speed(10f), new CrossBullet(new Vector2(0f, 0f), 0, 20));
		Fire(new Direction(aimDirection), new Speed(10f), new CrossBullet(new Vector2(-0.7f, 0f), 0, 20));
		Fire(new Direction(aimDirection), new Speed(10f), new CrossBullet(new Vector2(-1.4f, 0f), 0, 20));
		Fire(new Direction(aimDirection), new Speed(10f), new CrossBullet(new Vector2(0f, 0.7f), 18, 15));
		Fire(new Direction(aimDirection), new Speed(10f), new CrossBullet(new Vector2(0f, -0.7f), 18, 15));
		return null;
	}
}
