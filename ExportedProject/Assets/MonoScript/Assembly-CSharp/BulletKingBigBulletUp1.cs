using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BulletKing/BigBulletUp1")]
public class BulletKingBigBulletUp1 : Script
{
	public class BigBullet : Bullet
	{
		private bool m_isHard;

		public BigBullet(bool isHard)
			: base("bigBullet")
		{
			m_isHard = isHard;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(40);
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float startAngle = RandomAngle();
				for (int i = 0; i < 8; i++)
				{
					float direction = ((!m_isHard) ? SubdivideCircle(startAngle, 8, i) : SubdivideArc(base.AimDirection - 120f, 240f, 8, i));
					Fire(new Direction(direction), new Speed(6f), new MediumBullet());
				}
			}
		}
	}

	public class MediumBullet : Bullet
	{
		public MediumBullet()
			: base("quad")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(30);
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float num = RandomAngle();
				float num2 = 45f;
				for (int i = 0; i < 8; i++)
				{
					Fire(new Direction(num + (float)i * num2), new Speed(10f), new Bullet("default_novfx"));
				}
			}
		}
	}

	private const int NumMediumBullets = 8;

	private const int NumSmallBullets = 8;

	protected bool IsHard
	{
		get
		{
			return this is BulletKingBigBulletUpHard1;
		}
	}

	protected override IEnumerator Top()
	{
		Fire(new Offset(0.0625f, 3.5625f, 0f, string.Empty), new Direction(90f), new Speed(3f), new BigBullet(IsHard));
		return null;
	}
}
