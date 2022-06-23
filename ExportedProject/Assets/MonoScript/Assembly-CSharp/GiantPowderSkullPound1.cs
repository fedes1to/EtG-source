using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/Pound1")]
public class GiantPowderSkullPound1 : Script
{
	private const float WaveSeparation = 12f;

	private const float OffsetDist = 1f;

	private static int s_lastPatternNum;

	protected override IEnumerator Top()
	{
		switch (s_lastPatternNum = BraveUtility.SequentialRandomRange(0, 4, s_lastPatternNum, null, true))
		{
		case 0:
		{
			float num = base.AimDirection - 48f;
			for (int l = 0; l < 9; l++)
			{
				float num5 = num + (float)l * 12f;
				Fire(new Offset(new Vector2(1f, 0f), num5, string.Empty), new Direction(num5), new Speed(8f), new Bullet("default_ground"));
			}
			break;
		}
		case 1:
		{
			float num = base.AimDirection - 48f + Random.Range(-35f, 35f);
			for (int j = 0; j < 9; j++)
			{
				float num3 = num + (float)j * 12f;
				Fire(new Offset(new Vector2(1f, 0f), num3, string.Empty), new Direction(num3), new Speed(8f), new Bullet("default_ground"));
			}
			break;
		}
		case 2:
		{
			float num = RandomAngle();
			for (int k = 0; k < 12; k++)
			{
				float num4 = num + (float)k * 30f;
				Fire(new Offset(new Vector2(1f, 0f), num4, string.Empty), new Direction(num4), new Speed(8f), new Bullet("default_ground"));
			}
			break;
		}
		case 3:
		{
			float num = RandomAngle();
			for (int i = 0; i < 36; i++)
			{
				float num2 = num + (float)i * 10f;
				Fire(new Offset(new Vector2(1f, 0f), num2, string.Empty), new Direction(num2), new Speed(8f), new Bullet("default_ground"));
			}
			break;
		}
		}
		return null;
	}
}
