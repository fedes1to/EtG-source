using System.Collections;

namespace Brave.BulletScript
{
	public class SpeedChangingBullet : Bullet
	{
		private float m_newSpeed;

		private int m_term;

		private int m_destroyTimer;

		public SpeedChangingBullet(float newSpeed, int term, int destroyTimer = -1)
		{
			m_newSpeed = newSpeed;
			m_term = term;
			m_destroyTimer = destroyTimer;
		}

		public SpeedChangingBullet(string name, float newSpeed, int term, int destroyTimer = -1, bool suppressVfx = false)
			: base(name, suppressVfx)
		{
			m_newSpeed = newSpeed;
			m_term = term;
			m_destroyTimer = destroyTimer;
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(m_newSpeed), m_term);
			if (m_destroyTimer >= 0)
			{
				yield return Wait(m_term + m_destroyTimer);
				Vanish();
			}
		}
	}
}
