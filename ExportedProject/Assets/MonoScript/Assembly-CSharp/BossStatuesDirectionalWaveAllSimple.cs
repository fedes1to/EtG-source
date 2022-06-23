using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossStatues/DirectionalWaveAllSimple")]
public class BossStatuesDirectionalWaveAllSimple : Script
{
	public class EggBullet : Bullet
	{
		public EggBullet()
			: base("egg")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(10f), 120);
			yield return Wait(600);
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		Fire(new Offset("top 0"), new Direction(100f), new Speed(9f), new EggBullet());
		Fire(new Offset("top 1"), new Direction(90f), new Speed(9f), new EggBullet());
		Fire(new Offset("top 2"), new Direction(80f), new Speed(9f), new EggBullet());
		Fire(new Offset("right 0"), new Direction(10f), new Speed(9f), new EggBullet());
		Fire(new Offset("right 1"), new Direction(), new Speed(9f), new EggBullet());
		Fire(new Offset("right 2"), new Direction(-10f), new Speed(9f), new EggBullet());
		Fire(new Offset("bottom 0"), new Direction(-80f), new Speed(9f), new EggBullet());
		Fire(new Offset("bottom 1"), new Direction(-90f), new Speed(9f), new EggBullet());
		Fire(new Offset("bottom 2"), new Direction(-100f), new Speed(9f), new EggBullet());
		Fire(new Offset("left 0"), new Direction(190f), new Speed(9f), new EggBullet());
		Fire(new Offset("left 1"), new Direction(180f), new Speed(9f), new EggBullet());
		Fire(new Offset("left 2"), new Direction(170f), new Speed(9f), new EggBullet());
		BossStatuesChaos1.AntiCornerShot(this);
		return null;
	}
}
