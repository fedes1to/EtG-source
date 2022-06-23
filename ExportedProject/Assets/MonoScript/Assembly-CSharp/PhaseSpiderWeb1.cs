using System.Collections;
using Brave.BulletScript;

public class PhaseSpiderWeb1 : Script
{
	private class WebBullet : Bullet
	{
		private int m_delayFrames;

		private bool m_spawnGoop;

		public WebBullet(int delayFrames, bool spawnGoop = false)
			: base((!spawnGoop) ? "default" : "web")
		{
			m_delayFrames = delayFrames;
			m_spawnGoop = spawnGoop;
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

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (m_spawnGoop && destroyType != 0 && (bool)base.BulletBank)
			{
				GoopDoer component = base.BulletBank.GetComponent<GoopDoer>();
				DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(component.goopDefinition).AddGoopCircle(base.Position, 1.5f);
			}
			base.OnBulletDestruction(destroyType, hitRigidbody, preventSpawningProjectiles);
		}
	}

	private const int NumWaves = 7;

	private const int BulletsPerWave = 13;

	private const float WebDegrees = 120f;

	private const float BulletSpeed = 9f;

	protected override IEnumerator Top()
	{
		float startDirection = base.AimDirection - 60f;
		for (int i = 0; i < 7; i++)
		{
			int baseDelay = i * 7;
			if (i % 3 == 1)
			{
				for (int j = 0; j < 13; j++)
				{
					float num = 9.230769f;
					int num2 = 0;
					if (j % 4 == 1 || j % 4 == 3)
					{
						num2 = 3;
					}
					if (j % 4 == 2)
					{
						num2 = 5;
					}
					Fire(new Direction(startDirection + (float)j * num), new Speed(9f), new WebBullet(baseDelay + num2));
				}
			}
			else
			{
				for (int k = 0; k < 13; k++)
				{
					float num3 = 9.230769f;
					if (k % 4 == 0)
					{
						Fire(new Direction(startDirection + (float)k * num3), new Speed(9f), new WebBullet(baseDelay, i == 0));
					}
				}
			}
			yield return Wait(3);
		}
	}
}
