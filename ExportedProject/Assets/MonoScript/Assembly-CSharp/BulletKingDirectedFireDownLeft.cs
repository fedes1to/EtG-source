using System.Collections;
using FullInspector;

[InspectorDropdownName("Bosses/BulletKing/DirectedFireDownLeft")]
public class BulletKingDirectedFireDownLeft : BulletKingDirectedFire
{
	protected override IEnumerator Top()
	{
		yield return Wait(10);
		DirectedShots(-1.3125f, -0.4375f, -24f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-1.5f, -0.1875f, -34f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-1.75f, 0.25f, -44f);
		yield return Wait((!base.IsHard) ? 12 : 8);
		DirectedShots(-1.3125f, -0.4375f, -28f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-1.5f, -0.1875f, -34f);
		yield return Wait((!base.IsHard) ? 6 : 4);
		DirectedShots(-1.75f, 0.25f, -42f);
	}
}
