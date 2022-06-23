using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossFinalRogue/TeethVolley1")]
public class BossFinalRogueTeethVolley1 : Script
{
	protected override IEnumerator Top()
	{
		yield return Wait(10);
		Fire(new Offset("teeth mid 1"), new Direction(-90f), new Speed(10f), new Bullet("teeth_default"));
		Fire(new Offset("teeth mid 9"), new Direction(-90f), new Speed(10f), new Bullet("teeth_default"));
		yield return Wait(10);
		float dir2 = GetAimDirection("teeth mid 3", 1f, 10f);
		Fire(new Offset("teeth mid 2"), new Direction(dir2), new Speed(10f), new Bullet("teeth_default"));
		Fire(new Offset("teeth mid 3"), new Direction(dir2), new Speed(10f), new Bullet("teeth_default"));
		Fire(new Offset("teeth mid 4"), new Direction(dir2), new Speed(10f), new Bullet("teeth_default"));
		dir2 = GetAimDirection("teeth mid 7", 1f, 10f);
		Fire(new Offset("teeth mid 6"), new Direction(dir2), new Speed(10f), new Bullet("teeth_default"));
		Fire(new Offset("teeth mid 7"), new Direction(dir2), new Speed(10f), new Bullet("teeth_default"));
		Fire(new Offset("teeth mid 8"), new Direction(dir2), new Speed(10f), new Bullet("teeth_default"));
		yield return Wait(10);
		Fire(new Offset("teeth top 1"), new Direction(-90f), new Speed(10f), new Bullet("teeth_large"));
		Fire(new Offset("teeth top 2"), new Direction(-90f), new Speed(10f), new Bullet("teeth_large"));
		Fire(new Offset("teeth top 3"), new Direction(-90f), new Speed(10f), new Bullet("teeth_large"));
		Fire(new Offset("teeth mid 5"), new Direction(-90f), new Speed(10f), new Bullet("teeth_default"));
		yield return Wait(10);
		Fire(direction: new Direction(GetAimDirection("teeth bottom 1", 1f, 10f)), offset: new Offset("teeth bottom 1"), speed: new Speed(10f), bullet: new Bullet("teeth_football"));
		Fire(direction: new Direction(GetAimDirection("teeth bottom 4", 1f, 10f)), offset: new Offset("teeth bottom 4"), speed: new Speed(10f), bullet: new Bullet("teeth_football"));
		yield return Wait(10);
	}
}
