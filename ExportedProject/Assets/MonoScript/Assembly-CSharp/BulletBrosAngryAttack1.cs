using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BulletBros/AngryAttack1")]
public class BulletBrosAngryAttack1 : Script
{
	protected override IEnumerator Top()
	{
		for (float num = -2f; num <= 2f; num += 1f)
		{
			Fire(new Direction(num * 20f, DirectionType.Aim), new Speed(10f), new Bullet("angrybullet"));
		}
		yield return Wait(40);
		for (float num2 = -1.5f; (double)num2 <= 1.5; num2 += 1f)
		{
			Fire(new Direction(num2 * 20f, DirectionType.Aim), new Speed(10f), new Bullet("angrybullet"));
		}
		yield return Wait(40);
		for (float num3 = -2f; num3 <= 2f; num3 += 0.5f)
		{
			Fire(new Direction(num3 * 20f, DirectionType.Aim), new Speed(10f), new Bullet("angrybullet"));
		}
	}
}
