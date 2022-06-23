using System.Collections;

namespace Brave.BulletScript
{
	public class TimedBullet : Bullet
	{
		private int m_destroyTimer;

		public TimedBullet(int destroyTimer)
		{
			m_destroyTimer = destroyTimer;
		}

		public TimedBullet(string name, int destroyTimer)
			: base(name)
		{
			m_destroyTimer = destroyTimer;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(m_destroyTimer);
			Vanish();
		}
	}
}
