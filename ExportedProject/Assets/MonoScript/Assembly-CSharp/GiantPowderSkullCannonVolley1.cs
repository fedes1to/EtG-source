using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GiantPowderSkull/CannonVolley1")]
public class GiantPowderSkullCannonVolley1 : Script
{
	private const int NumBullets = 5;

	private const float HalfWidth = 4.5f;

	protected override IEnumerator Top()
	{
		AIAnimator aiAnimator = base.BulletBank.aiAnimator;
		string name = "eyeflash";
		Vector2? position = base.Position;
		aiAnimator.PlayVfx(name, null, null, position);
		yield return Wait(30);
		float angle = base.AimDirection;
		for (int i = 0; i < 5; i++)
		{
			float y = Mathf.Lerp(-4.5f, 4.5f, (float)i / 4f);
			Fire(new Offset(new Vector2(1f, y), angle, string.Empty), new Direction(angle), new Speed(13f), new Bullet("cannon"));
			yield return Wait(5);
		}
	}
}
