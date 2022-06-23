using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossFinalBullet/GunonQuickShot1")]
public class BossFinalBulletGunonQuickShot1 : Script
{
	public class BatBullet : Bullet
	{
		public BatBullet()
			: base("hipbat")
		{
		}

		protected override IEnumerator Top()
		{
			Direction = GetAimDirection(base.Position, (!BraveUtility.RandomBool()) ? 1 : 0, 12f);
			ChangeSpeed(new Speed(12f), 3);
			while (base.Tick < 180)
			{
				if (base.Tick > 7 && base.Tick % 7 == 0)
				{
					Fire(new FireBullet());
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class FireBullet : Bullet
	{
		public FireBullet()
			: base("fire")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(30);
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		Fire(new Offset("left double shoot point"), new BatBullet());
		yield return Wait(6);
		Fire(new Offset("right double shoot point"), new BatBullet());
	}
}
