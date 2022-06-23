using System.Collections;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("ManfredsRival/ShieldSlam2")]
public class ManfredsRivalShieldSlam2 : ManfredsRivalShieldSlam1
{
	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		FireExpandingLine(new Vector2(-0.6f, -1f), new Vector2(0.6f, -1f), 10);
		FireExpandingLine(new Vector2(-0.7f, -1f), new Vector2(-0.8f, -0.9f), 3);
		FireExpandingLine(new Vector2(0.7f, -1f), new Vector2(0.8f, -0.9f), 3);
		FireExpandingLine(new Vector2(-0.8f, -0.9f), new Vector2(-0.8f, 0.2f), 12);
		FireExpandingLine(new Vector2(0.8f, -0.9f), new Vector2(0.8f, 0.2f), 12);
		FireExpandingLine(new Vector2(-0.8f, 0.2f), new Vector2(-0.15f, 1f), 10);
		FireExpandingLine(new Vector2(0.8f, 0.2f), new Vector2(0.15f, 1f), 10);
		FireExpandingLine(new Vector2(-0.15f, 1f), new Vector2(0.15f, 1f), 5);
		FireSpinningLine(new Vector2(0f, -1.5f), new Vector2(0f, 1.5f), 4);
		FireSpinningLine(new Vector2(-0.6f, -0.4f), new Vector2(0.6f, -0.4f), 2);
		yield return Wait(40);
		FireSpinningLine(new Vector2(0f, -1.5f), new Vector2(0f, 1.5f), 4);
		FireSpinningLine(new Vector2(-0.6f, -0.4f), new Vector2(0.6f, -0.4f), 2);
		yield return Wait(20);
	}
}
