using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossStatues/KaliSlam1")]
public class BossStatuesKaliSlam1 : Script
{
	public class SpiralBullet1 : Bullet
	{
		public SpiralBullet1()
			: base("spiralbullet1")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeDirection(new Direction(0.8f, DirectionType.Sequence));
			ChangeSpeed(new Speed(12f), 180);
			yield return Wait(600);
			Vanish();
		}
	}

	public class SpiralBullet2 : Bullet
	{
		public SpiralBullet2()
			: base("spiralbullet2")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeDirection(new Direction(-0.8f, DirectionType.Sequence));
			ChangeSpeed(new Speed(12f), 180);
			yield return Wait(600);
			Vanish();
		}
	}

	public class SpiralBullet3 : Bullet
	{
		public SpiralBullet3()
			: base("spiralbullet3")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeDirection(new Direction(0.8f, DirectionType.Sequence));
			ChangeSpeed(new Speed(12f), 180);
			yield return Wait(600);
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		Vector2 fixedPosition = base.Position;
		for (int i = 0; i < 36; i++)
		{
			Fire(Offset.OverridePosition(fixedPosition), new Direction(-60f + (float)(i * 10)), new Speed(7f), new SpiralBullet1());
		}
		yield return Wait(5);
		for (int j = 0; j < 36; j++)
		{
			Fire(Offset.OverridePosition(fixedPosition), new Direction(-66f + (float)(j * 10)), new Speed(7f), new SpiralBullet2());
		}
		for (int k = 0; k < 8; k++)
		{
			Fire(Offset.OverridePosition(fixedPosition), new Direction(k * 45, DirectionType.Aim), new Speed(11f), new Bullet("egg"));
		}
		yield return Wait(5);
		for (int l = 0; l < 36; l++)
		{
			Fire(Offset.OverridePosition(fixedPosition), new Direction(-72f + (float)(l * 10)), new Speed(7f), new SpiralBullet3());
		}
	}
}
