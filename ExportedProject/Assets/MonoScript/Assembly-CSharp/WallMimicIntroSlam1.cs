using System.Collections;
using FullInspector;

[InspectorDropdownName("MimicWall/IntroSlam1")]
public class WallMimicIntroSlam1 : WallMimicSlam1
{
	protected override IEnumerator Top()
	{
		float facingDirection = base.BulletBank.aiAnimator.CurrentArtAngle;
		FireLine(facingDirection - 90f, 5f, 45f, -15f);
		FireLine(facingDirection, 11f, -45f, 45f);
		FireLine(facingDirection + 90f, 5f, -45f, 15f);
		yield return Wait(10);
		FireLine(facingDirection - 90f, 4f, 45f, -15f, true);
		FireLine(facingDirection, 10f, -45f, 45f, true);
		FireLine(facingDirection + 90f, 4f, -45f, 15f, true);
	}
}
