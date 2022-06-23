using System.Collections;

namespace Brave.BulletScript
{
	public class DelayedBullet : Bullet
	{
		private int m_delayFrames;

		public DelayedBullet(int delayFrames)
		{
			m_delayFrames = delayFrames;
		}

		public DelayedBullet(string name, int delayFrames)
			: base(name)
		{
			m_delayFrames = delayFrames;
		}

		protected override IEnumerator Top()
		{
			if (m_delayFrames != 0)
			{
				float speed = Speed;
				Speed = 0f;
				yield return Wait(m_delayFrames);
				Speed = speed;
			}
		}
	}
}
