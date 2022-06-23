using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Blobulord/Slam1")]
public class BlobulordSlam1 : Script
{
	public class SlamBullet : Bullet
	{
		private int m_spawnDelay;

		public SlamBullet(int spawnDelay)
			: base("slam")
		{
			m_spawnDelay = spawnDelay;
		}

		protected override IEnumerator Top()
		{
			int slowTime = m_spawnDelay * 40;
			int i = 0;
			while (true)
			{
				yield return Wait(1);
				if (i == slowTime)
				{
					ChangeSpeed(new Speed(15f), 60);
				}
				i++;
			}
		}
	}

	private const int NumBullets = 32;

	private const int NumWaves = 4;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 4; i++)
		{
			float num = RandomAngle();
			for (int j = 0; j < 32; j++)
			{
				float num2 = num + (float)j * 11.25f;
				Fire(new Offset(2f, 0f, num2, string.Empty), new Direction(num2), new Speed(0.7f), new SlamBullet(i));
			}
		}
		yield return Wait(80);
	}
}
