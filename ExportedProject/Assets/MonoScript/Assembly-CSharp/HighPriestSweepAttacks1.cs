using System.Collections;
using Brave.BulletScript;

public abstract class HighPriestSweepAttacks1 : Script
{
	public class SweepBullet : Bullet
	{
		private int m_delay;

		public SweepBullet(int delay)
			: base("sweep")
		{
			m_delay = delay;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(30 - m_delay);
			Speed = 12f;
			yield return Wait(270);
			Vanish();
		}
	}

	private const int NumBullets = 15;

	private bool m_shootLeft;

	private bool m_shootRight;

	public HighPriestSweepAttacks1(bool shootLeft, bool shootRight)
	{
		m_shootLeft = shootLeft;
		m_shootRight = shootRight;
	}

	protected override IEnumerator Top()
	{
		float angleDelta = 9f;
		for (int i = 0; i < 15; i++)
		{
			if (m_shootLeft)
			{
				Fire(new Offset(0f, -2.5f, -30f - (float)i * angleDelta, string.Empty), new Direction((8 - i) * 5, DirectionType.Aim), new Speed(), new SweepBullet(i));
			}
			if (m_shootRight)
			{
				Fire(new Offset(0f, -2.5f, 30f + (float)i * angleDelta, string.Empty), new Direction((8 - i) * -5, DirectionType.Aim), new Speed(), new SweepBullet(i));
			}
			yield return Wait(1);
		}
	}
}
