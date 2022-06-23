using System.Collections;
using FullInspector;

[InspectorDropdownName("Bosses/BulletKing/DirectedFireLeft")]
public class BulletKingDirectedFireLeft : BulletKingDirectedFire
{
	protected override IEnumerator Top()
	{
		yield return Wait(10);
		DirectedShots(-2.0625f, 2.375f, -98f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-2.125f, 1.3125f, -90f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-1.75f, 0.25f, -82f);
		yield return Wait((!base.IsHard) ? 12 : 8);
		DirectedShots(-2.0625f, 2.375f, -94f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-2.125f, 1.3125f, -90f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-1.75f, 0.25f, -86f);
	}
}
