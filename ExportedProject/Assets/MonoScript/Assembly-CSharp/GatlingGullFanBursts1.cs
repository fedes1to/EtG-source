using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/GatlingGull/FanBursts1")]
public class GatlingGullFanBursts1 : Script
{
	private const int NumWaves = 2;

	private const int NumBulletsPerWave = 20;

	private const float WaveArcLength = 130f;

	protected override IEnumerator Top()
	{
		float startAngle = base.AimDirection - 65f;
		float deltaAngle = 6.84210539f;
		base.BulletBank.aiAnimator.LockFacingDirection = true;
		base.BulletBank.aiAnimator.FacingDirection = base.AimDirection;
		for (int i = 0; i < 2; i++)
		{
			float angle = startAngle;
			for (int j = 0; j < 20; j++)
			{
				Fire(new Direction(angle), new Speed(10f), new Bullet("defaultWithVfx"));
				angle += deltaAngle;
			}
			if (i < 1)
			{
				yield return Wait(75);
			}
		}
		yield return Wait(20);
		base.BulletBank.aiAnimator.LockFacingDirection = false;
	}
}
