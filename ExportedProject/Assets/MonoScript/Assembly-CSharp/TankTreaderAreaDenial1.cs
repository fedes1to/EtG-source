using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/TankTreader/AreaDenial1")]
public class TankTreaderAreaDenial1 : Script
{
	public class HugeBullet : Bullet
	{
		private const int SemiCircleNumBullets = 4;

		private const int SemiCirclePhases = 3;

		private bool m_fireSemicircles;

		public HugeBullet()
			: base("hugeBullet")
		{
		}

		protected override IEnumerator Top()
		{
			m_fireSemicircles = true;
			StartTask(FireSemicircles());
			ChangeSpeed(new Speed(), HugeBulletDecelerationTime);
			yield return Wait(HugeBulletDecelerationTime);
			Vector2 truePosition = base.Position;
			base.ManualControl = true;
			for (int i = 0; (float)i < HugeBulletHangTime; i++)
			{
				if (m_fireSemicircles && (float)i > HugeBulletHangTime - 45f)
				{
					m_fireSemicircles = false;
				}
				base.Position = truePosition + new Vector2(0.12f * ((float)i / HugeBulletHangTime), 0f) * Mathf.Sin((float)i / 5f * (float)Math.PI);
				yield return Wait(1);
			}
			for (int j = 0; j < 36; j++)
			{
				Fire(new Direction(j * 10), new Speed(12f));
			}
			for (int k = 0; k < 36; k++)
			{
				Fire(new Direction(5 + k * 10), new Speed(8f), new SpeedChangingBullet(12f, 30));
			}
			Vanish();
		}

		private IEnumerator FireSemicircles()
		{
			yield return Wait(60);
			int phase = 0;
			while (m_fireSemicircles)
			{
				for (int i = 0; i < 36; i++)
				{
					if (i / 4 % 3 == phase)
					{
						Fire(new Direction(i * 10), new Speed(9f));
					}
				}
				yield return Wait(45);
				phase = (phase + 1) % 3;
			}
		}
	}

	public static float HugeBulletStartSpeed = 6f;

	public static int HugeBulletDecelerationTime = 180;

	public static float HugeBulletHangTime = 300f;

	public static float SpinningBulletSpinSpeed = 180f;

	protected override IEnumerator Top()
	{
		Fire(new Direction(0f, DirectionType.Aim), new Speed(HugeBulletStartSpeed), new HugeBullet());
		return null;
	}
}
