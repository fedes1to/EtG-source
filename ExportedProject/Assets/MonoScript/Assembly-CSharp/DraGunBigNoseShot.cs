using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/DraGun/BigNoseShot")]
public class DraGunBigNoseShot : Script
{
	protected override IEnumerator Top()
	{
		Fire(new Direction(-90f), new Speed(6f), new Bullet("homing"));
		Fire(new Direction(-110f), new Speed(6f), new Bullet("homing"));
		Fire(new Direction(-130f), new Speed(6f), new Bullet("homing"));
		Fire(new Direction(-70f), new Speed(6f), new Bullet("homing"));
		Fire(new Direction(-50f), new Speed(6f), new Bullet("homing"));
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			Fire(new Direction(-60f), new Speed(6f), new Bullet("homing"));
			Fire(new Direction(-80f), new Speed(6f), new Bullet("homing"));
			Fire(new Direction(-100f), new Speed(6f), new Bullet("homing"));
			Fire(new Direction(-120f), new Speed(6f), new Bullet("homing"));
		}
		return null;
	}
}
