using System.Collections;
using Brave.BulletScript;

public class TutorialKnightBurstAttack1 : Script
{
	protected override IEnumerator Top()
	{
		yield return Wait(15);
		for (int i = 0; i < 36; i++)
		{
			Fire(new Direction(i * 10), new Speed(5f), new Bullet("burst", true));
		}
		yield return Wait(90);
		for (int j = 0; j < 36; j++)
		{
			Fire(new Direction(j * 10), new Speed(5f), new Bullet("burst", true));
		}
		yield return Wait(30);
	}
}
