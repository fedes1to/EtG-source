using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossStatues/Chaos2")]
public class BossStatuesChaos2 : Script
{
	protected override IEnumerator Top()
	{
		Fire(new Offset("top 0"), new Direction(135f), new Speed(7.5f), new Bullet("egg"));
		Fire(new Offset("top 2"), new Direction(45f), new Speed(7.5f), new Bullet("egg"));
		Fire(new Offset("right 0"), new Direction(45f), new Speed(7.5f), new Bullet("egg"));
		Fire(new Offset("right 2"), new Direction(-45f), new Speed(7.5f), new Bullet("egg"));
		Fire(new Offset("bottom 0"), new Direction(-45f), new Speed(7.5f), new Bullet("egg"));
		Fire(new Offset("bottom 2"), new Direction(-135f), new Speed(7.5f), new Bullet("egg"));
		Fire(new Offset("left 0"), new Direction(-135f), new Speed(7.5f), new Bullet("egg"));
		Fire(new Offset("left 2"), new Direction(135f), new Speed(7.5f), new Bullet("egg"));
		BossStatuesChaos1.AntiCornerShot(this);
		return null;
	}
}
