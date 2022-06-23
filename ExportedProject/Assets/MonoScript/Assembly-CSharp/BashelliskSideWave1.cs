using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Bashellisk/SideWave1")]
public class BashelliskSideWave1 : Script
{
	public class WaveBullet : Bullet
	{
		public WaveBullet()
			: base("bigBullet")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(20);
			for (int i = 0; i < 2; i++)
			{
				ChangeSpeed(new Speed(-2f), 20);
				yield return Wait(56);
				ChangeSpeed(new Speed(9f), 20);
				yield return Wait(40);
			}
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		Fire(new Direction(-90f, DirectionType.Relative), new Speed(9f), new WaveBullet());
		Fire(new Direction(90f, DirectionType.Relative), new Speed(9f), new WaveBullet());
		return null;
	}
}
