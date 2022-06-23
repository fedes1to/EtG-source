using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/WingFlap1")]
public class DraGunWingFlap1 : Script
{
	public class WindProjectile : Bullet
	{
		private float m_sign;

		public WindProjectile(float sign)
		{
			m_sign = sign;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(Random.Range(60, 126));
			ChangeDirection(new Direction(-90f + m_sign * 90f), 30);
		}
	}

	private const int NumBullets = 30;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		for (int i = 0; i < 30; i++)
		{
			Fire(new Offset(-17f + Random.Range(0f, 5f), 18f, 0f, string.Empty), new Direction(-90f), new Speed(12f), new WindProjectile(1f));
			Fire(new Offset(17f - Random.Range(0f, 5f), 18f, 0f, string.Empty), new Direction(-90f), new Speed(12f), new WindProjectile(-1f));
			yield return Wait(8);
		}
		yield return Wait(60);
	}
}
