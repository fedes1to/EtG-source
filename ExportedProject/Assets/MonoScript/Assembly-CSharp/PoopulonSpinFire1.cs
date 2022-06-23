using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class PoopulonSpinFire1 : Script
{
	public class RotatingBullet : Bullet
	{
		private Vector2 m_origin;

		public RotatingBullet(Vector2 origin)
		{
			m_origin = origin;
		}

		protected override IEnumerator Top()
		{
			Vector2 originToPos = base.Position - m_origin;
			float dist = originToPos.magnitude;
			float angle = originToPos.ToAngle();
			base.ManualControl = true;
			for (int i = 0; i < 300; i++)
			{
				angle -= 0.4f;
				dist += Speed / 60f;
				base.Position = m_origin + BraveMathCollege.DegreesToVector(angle, dist);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 100;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 100; i++)
		{
			float angle = RandomAngle();
			Fire(new Offset(0.75f, 0f, angle, string.Empty), new Direction(angle), new Speed(7f), new RotatingBullet(base.Position));
			yield return Wait(3);
		}
	}
}
