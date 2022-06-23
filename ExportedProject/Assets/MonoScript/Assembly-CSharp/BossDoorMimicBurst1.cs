using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossDoorMimic/Burst1")]
public class BossDoorMimicBurst1 : Script
{
	public class SpinBullet : Bullet
	{
		public SpinBullet()
			: base("teleport_burst")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeDirection(new Direction(179f, DirectionType.Relative), 180);
			ChangeSpeed(new Speed(10f), 180);
			yield return Wait(600);
			Vanish();
		}
	}

	private const int NumBullets = 36;

	protected override IEnumerator Top()
	{
		float startDirection = -60f;
		float delta = 10f;
		for (int i = 0; i < 36; i++)
		{
			Fire(new Direction(startDirection + (float)i * delta), new Speed(6f), new SpinBullet());
		}
		yield return Wait(30);
	}
}
