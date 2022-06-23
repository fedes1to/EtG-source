using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Mac10Burst2")]
public class DraGunMac10Burst2 : Script
{
	public class UziBullet : Bullet
	{
		public UziBullet()
			: base("UziBullet")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(60);
			Fire(new Direction(Random.Range(-45f, 45f), DirectionType.Relative), new Speed(12f), new Bullet("UziBurst"));
			yield return Wait(60);
			Fire(new Direction(0f, DirectionType.Relative), new Speed(12f), new Bullet("UziBurst"));
			yield return Wait(60);
			Speed = 12f;
			Direction = RandomAngle();
		}
	}

	protected override IEnumerator Top()
	{
		yield return Wait(1);
		Vector2 lastPosition = base.Position;
		PostWwiseEvent("Play_BOSS_Dragun_Uzi_01");
		while (true)
		{
			if (Vector2.Distance(lastPosition, base.Position) > 1f)
			{
				Fire(new Offset((lastPosition - base.Position) * 0.33f, 0f, string.Empty), new Direction(0f, DirectionType.Relative), new UziBullet());
				Fire(new Offset((lastPosition - base.Position) * 0.66f, 0f, string.Empty), new Direction(0f, DirectionType.Relative), new UziBullet());
			}
			Fire(new Direction(0f, DirectionType.Relative), new UziBullet());
			lastPosition = base.Position;
			yield return Wait(Random.Range(2, 4));
		}
	}
}
