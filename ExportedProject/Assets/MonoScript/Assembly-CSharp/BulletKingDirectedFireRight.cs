using System.Collections;
using FullInspector;

[InspectorDropdownName("Bosses/BulletKing/DirectedFireRight")]
public class BulletKingDirectedFireRight : BulletKingDirectedFire
{
	protected override IEnumerator Top()
	{
		yield return Wait(10);
		DirectedShots(2.125f, 2.375f, 98f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(2.1875f, 1.3125f, 90f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(1.875f, 0.25f, 82f);
		yield return Wait((!base.IsHard) ? 12 : 8);
		DirectedShots(2.125f, 2.375f, 94f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(2.1875f, 1.3125f, 90f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(1.875f, 0.25f, 86f);
	}
}
