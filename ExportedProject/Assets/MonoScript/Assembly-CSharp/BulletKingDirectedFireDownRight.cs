using System.Collections;
using FullInspector;

[InspectorDropdownName("Bosses/BulletKing/DirectedFireDownRight")]
public class BulletKingDirectedFireDownRight : BulletKingDirectedFire
{
	protected override IEnumerator Top()
	{
		yield return Wait(10);
		DirectedShots(1.875f, 0.25f, 44f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(1.625f, -0.1875f, 34f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(1.4275f, -0.4375f, 24f);
		yield return Wait((!base.IsHard) ? 12 : 8);
		DirectedShots(1.875f, 0.25f, 42f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(1.625f, -0.1875f, 34f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(1.4275f, -0.4375f, 28f);
	}
}
