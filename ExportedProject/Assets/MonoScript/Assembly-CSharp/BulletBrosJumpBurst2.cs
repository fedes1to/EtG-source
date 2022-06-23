using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BulletBros/JumpBurst2")]
public class BulletBrosJumpBurst2 : Script
{
	private const int NumFastBullets = 18;

	private const int NumSlowBullets = 9;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		for (int i = 0; i < 18; i++)
		{
			Fire(new Direction(SubdivideCircle(num, 18, i)), new Speed(9f), new Bullet("jump", true));
		}
		num += 10f;
		for (int j = 0; j < 9; j++)
		{
			Fire(new Direction(SubdivideCircle(num, 9, j)), new Speed(), new SpeedChangingBullet("jump", 9f, 75, -1, true));
		}
		return null;
	}
}
