using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/RollSlam1")]
public class GiantPowderSkullRollSlam1 : Script
{
	private const float OffsetDist = 1.5f;

	protected override IEnumerator Top()
	{
		AkSoundEngine.PostEvent("Play_BOSS_doormimic_blast_01", base.BulletBank.gameObject);
		float startDirection3 = 0f;
		for (int i = 0; i < 30; i++)
		{
			float num = startDirection3 + (float)(i * 12);
			Fire(new Offset(new Vector2(1.5f, 0f), num, string.Empty), new Direction(num), new Speed(7f), new SpeedChangingBullet("slam", 12f, 180));
		}
		yield return Wait(5);
		startDirection3 = 3f;
		for (int j = 0; j < 60; j++)
		{
			float num2 = startDirection3 + (float)(j * 6);
			Fire(new Offset(new Vector2(1.5f, 0f), num2, string.Empty), new Direction(num2), new Speed(7f), new SpeedChangingBullet("slam", 12f, 180));
		}
		yield return Wait(5);
		startDirection3 = 6f;
		for (int k = 0; k < 30; k++)
		{
			float num3 = startDirection3 + (float)(k * 12);
			Fire(new Offset(new Vector2(1.5f, 0f), num3, string.Empty), new Direction(num3), new Speed(7f), new SpeedChangingBullet("slam", 12f, 180));
		}
	}
}
