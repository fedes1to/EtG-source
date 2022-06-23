using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/EyesTwinBeams1")]
public class GiantPowderSkullEyesTwinBeams1 : Script
{
	private const float CoreSpread = 20f;

	private const float IncSpread = 10f;

	private const float TurnSpeed = 1f;

	private const float BulletSpeed = 18f;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float startDirection = base.AimDirection;
		float sign = BraveUtility.RandomSign();
		for (int i = 0; i < 210; i++)
		{
			float offset = 0f;
			if (i < 30)
			{
				offset = Mathf.Lerp(135f, 0f, (float)i / 29f);
			}
			float currentAngle = startDirection + sign * Mathf.Max(0f, (float)(i - 60) * 1f);
			for (int j = 0; j < 5; j++)
			{
				float num = offset + 20f + (float)j * 10f;
				Fire(new Offset("right eye"), new Direction(currentAngle + num), new Speed(18f), new Bullet("default_novfx"));
				Fire(new Offset("left eye"), new Direction(currentAngle - num), new Speed(18f), new Bullet("default_novfx"));
			}
			if (i > 30 && i % 30 == 29)
			{
				Fire(new Direction(currentAngle + Random.Range(-1f, 1f) * 20f), new Speed(12f));
			}
			if (i >= 60)
			{
				float num2 = Vector2.Distance(BulletManager.PlayerPosition(), base.Position);
				float num3 = num2 / 18f * 30f;
				if (num3 > (float)(i - 60))
				{
					num3 = Mathf.Max(0, i - 60);
				}
				float num4 = (0f - sign) * num3 * 1f;
				float num5 = currentAngle + 40f + num4;
				float num6 = currentAngle - 40f + num4;
				if (BraveMathCollege.ClampAngle180(num5 - GetAimDirection("right eye")) < 0f || BraveMathCollege.ClampAngle180(num6 - GetAimDirection("left eye")) > 0f)
				{
					break;
				}
			}
			yield return Wait(2);
		}
	}
}
