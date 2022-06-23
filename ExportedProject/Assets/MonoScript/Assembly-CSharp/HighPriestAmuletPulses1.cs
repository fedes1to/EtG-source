using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/HighPriest/AmuletPulses1")]
public class HighPriestAmuletPulses1 : Script
{
	public class VibratingBullet : Bullet
	{
		public VibratingBullet()
			: base("amuletRing")
		{
		}

		protected override IEnumerator Top()
		{
			Speed = 1f;
			yield return Wait(1);
			for (int i = 0; i < 20; i++)
			{
				float randWait = Random.Range(1, 11);
				yield return Wait(randWait);
				Direction += 180f;
				yield return Wait(10);
				Direction -= 180f;
				yield return Wait(10f - randWait);
				Direction += 180f;
			}
			Speed = 12f;
			yield return Wait(90);
			Vanish();
		}
	}

	private const int NumBullets = 25;

	protected override IEnumerator Top()
	{
		float angleDelta = 14.4f;
		for (int j = 0; j < 25; j++)
		{
			Fire(new Offset(2.5f, 0f, (float)j * angleDelta, string.Empty), new Direction((float)j * angleDelta), new Speed(), new VibratingBullet());
		}
		for (int k = 0; k < 25; k++)
		{
			Fire(new Offset(3.25f, 0f, ((float)k + 0.5f) * angleDelta, string.Empty), new Direction(((float)k + 0.5f) * angleDelta), new Speed(), new VibratingBullet());
		}
		yield return Wait(60);
		for (int i = 0; i < 12; i++)
		{
			Fire(new Direction(RandomAngle()), new Bullet("homing"));
			yield return Wait(10);
		}
		yield return Wait(220);
	}
}
